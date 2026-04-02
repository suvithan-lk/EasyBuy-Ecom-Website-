using Backend.Data;
using Backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public sealed class EasyBuyService(EasyBuyDbContext dbContext) : IEasyBuyService
{
    private const string DemoUserId = "customer-001";
    private readonly EasyBuyDbContext _dbContext = dbContext;

    public async Task<HomeResponseDto> GetHome()
    {
        var settings = await GetSiteSettings();
        var wishlist = await GetWishlistSet();
        var shopperName = await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == DemoUserId)
            .Select(user => user.Name)
            .FirstOrDefaultAsync() ?? "EasyBuy shopper";

        var categories = await _dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.Id)
            .Select(category => new CategoryDto(
                category.Id,
                category.Name,
                category.Slug,
                category.Description,
                category.Products.Count,
                category.ImageUrl))
            .ToListAsync();

        var featured = await BuildProductCardQuery()
            .Where(product => product.IsFeatured)
            .OrderByDescending(product => product.Rating)
            .ThenBy(product => product.Name)
            .Take(4)
            .ToListAsync();

        var spotlight = await BuildProductCardQuery()
            .Where(product => product.IsNewArrival || product.Stock <= 8)
            .OrderByDescending(product => product.IsNewArrival)
            .ThenBy(product => product.Stock)
            .ThenBy(product => product.Name)
            .Take(4)
            .ToListAsync();

        var productCount = await _dbContext.Products.CountAsync();
        var averageRating = await _dbContext.Products.AverageAsync(product => product.Rating);
        var fastestShipping = await _dbContext.Products.MinAsync(product => product.ShippingDays);
        var slowestShipping = await _dbContext.Products.MaxAsync(product => product.ShippingDays);

        return new HomeResponseDto(
            Brand: settings.Brand,
            ShopperName: shopperName,
            Headline: settings.Headline,
            Subheadline: settings.Subheadline,
            FreeShippingThreshold: settings.FreeShippingThreshold,
            StandardShippingRate: settings.StandardShippingRate,
            FeatureCallouts: settings.FeatureCallouts
                .OrderBy(item => item.SortOrder)
                .Select(item => item.Text)
                .ToList(),
            Stats:
            [
                new StatDto("Products", productCount.ToString()),
                new StatDto("Avg rating", averageRating.ToString("0.0")),
                new StatDto("Fast shipping", $"{fastestShipping}-{slowestShipping} days")
            ],
            Categories: categories,
            FeaturedProducts: featured.Select(product => MapProductCard(product, wishlist)).ToList(),
            SpotlightProducts: spotlight.Select(product => MapProductCard(product, wishlist)).ToList());
    }

    public async Task<CatalogResponseDto> GetProducts(string? search, string? category, string? sort)
    {
        var normalizedSearch = (search ?? string.Empty).Trim();
        var normalizedCategory = string.IsNullOrWhiteSpace(category) ? null : category.Trim().ToLowerInvariant();
        var normalizedSort = string.IsNullOrWhiteSpace(sort) ? "featured" : sort.Trim().ToLowerInvariant();

        var query = BuildProductCardQuery();

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            query = query.Where(product =>
                product.Name.Contains(normalizedSearch) ||
                product.Summary.Contains(normalizedSearch) ||
                product.Tags.Any(tag => tag.Value.Contains(normalizedSearch)));
        }

        if (!string.IsNullOrWhiteSpace(normalizedCategory) && normalizedCategory != "all")
        {
            query = query.Where(product => product.Category.Slug == normalizedCategory);
        }

        query = normalizedSort switch
        {
            "price-asc" => query.OrderBy(product => product.Price),
            "price-desc" => query.OrderByDescending(product => product.Price),
            "rating" => query.OrderByDescending(product => product.Rating).ThenByDescending(product => product.IsFeatured),
            "newest" => query.OrderByDescending(product => product.IsNewArrival).ThenByDescending(product => product.IsFeatured),
            _ => query.OrderByDescending(product => product.IsFeatured).ThenByDescending(product => product.Rating)
        };

        var wishlist = await GetWishlistSet();
        var products = await query.ToListAsync();
        var items = products.Select(product => MapProductCard(product, wishlist)).ToList();

        return new CatalogResponseDto(
            Items: items,
            TotalItems: items.Count,
            Search: normalizedSearch,
            Category: normalizedCategory,
            Sort: normalizedSort);
    }

    public async Task<ServiceResult<ProductDetailDto>> GetProduct(int id)
    {
        var product = await BuildProductDetailQuery()
            .FirstOrDefaultAsync(item => item.Id == id);

        if (product is null)
        {
            return ServiceResult<ProductDetailDto>.Fail("Product not found.");
        }

        return ServiceResult<ProductDetailDto>.Ok(MapProductDetail(product, await GetWishlistSet()));
    }

    public async Task<CartResponseDto> GetCart()
    {
        var settings = await GetSiteSettings();
        var cartItems = await LoadCartItems(asNoTracking: true);
        return BuildCartResponse(cartItems, settings);
    }

    public async Task<ServiceResult<CartResponseDto>> UpsertCartItem(UpsertCartItemRequest request)
    {
        if (request.Quantity < 1)
        {
            return ServiceResult<CartResponseDto>.Fail("Quantity must be at least 1.");
        }

        var product = await _dbContext.Products.FirstOrDefaultAsync(item => item.Id == request.ProductId);
        if (product is null)
        {
            return ServiceResult<CartResponseDto>.Fail("Product not found.");
        }

        if (request.Quantity > product.Stock)
        {
            return ServiceResult<CartResponseDto>.Fail("Requested quantity exceeds available stock.");
        }

        var existing = await _dbContext.CartItems
            .FirstOrDefaultAsync(item => item.UserId == DemoUserId && item.ProductId == request.ProductId);

        if (existing is null)
        {
            _dbContext.CartItems.Add(new CartItemEntity
            {
                UserId = DemoUserId,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                AddedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.Quantity = request.Quantity;
            existing.AddedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        var settings = await GetSiteSettings();
        var cartItems = await LoadCartItems(asNoTracking: true);
        return ServiceResult<CartResponseDto>.Ok(BuildCartResponse(cartItems, settings));
    }

    public async Task<ServiceResult<CartResponseDto>> RemoveCartItem(int productId)
    {
        var existing = await _dbContext.CartItems
            .FirstOrDefaultAsync(item => item.UserId == DemoUserId && item.ProductId == productId);

        if (existing is not null)
        {
            _dbContext.CartItems.Remove(existing);
            await _dbContext.SaveChangesAsync();
        }

        var settings = await GetSiteSettings();
        var cartItems = await LoadCartItems(asNoTracking: true);
        return ServiceResult<CartResponseDto>.Ok(BuildCartResponse(cartItems, settings));
    }

    public async Task<WishlistResponseDto> GetWishlist()
    {
        return await BuildWishlistResponse();
    }

    public async Task<ServiceResult<WishlistResponseDto>> ToggleWishlist(int productId)
    {
        var productExists = await _dbContext.Products.AnyAsync(item => item.Id == productId);
        if (!productExists)
        {
            return ServiceResult<WishlistResponseDto>.Fail("Product not found.");
        }

        var existing = await _dbContext.WishlistItems
            .FirstOrDefaultAsync(item => item.UserId == DemoUserId && item.ProductId == productId);

        if (existing is null)
        {
            _dbContext.WishlistItems.Add(new WishlistItemEntity
            {
                UserId = DemoUserId,
                ProductId = productId,
                AddedAt = DateTime.UtcNow
            });
        }
        else
        {
            _dbContext.WishlistItems.Remove(existing);
        }

        await _dbContext.SaveChangesAsync();
        return ServiceResult<WishlistResponseDto>.Ok(await BuildWishlistResponse());
    }

    public async Task<ServiceResult<CouponValidationResponse>> ValidateCoupon(CouponValidationRequest request)
    {
        var subtotal = request.Subtotal;

        if (subtotal <= 0m)
        {
            var settings = await GetSiteSettings();
            subtotal = BuildCartResponse(await LoadCartItems(asNoTracking: true), settings).Subtotal;
        }

        var couponResult = await ResolveCoupon(request.Code, subtotal);
        if (!couponResult.Success || couponResult.Value is null)
        {
            return ServiceResult<CouponValidationResponse>.Fail(couponResult.Error ?? "Coupon validation failed.");
        }

        var coupon = couponResult.Value;
        var discount = CalculateDiscount(coupon, subtotal);

        return ServiceResult<CouponValidationResponse>.Ok(
            new CouponValidationResponse(
                coupon.Code,
                coupon.Description,
                discount,
                Math.Max(subtotal - discount, 0m)));
    }

    public async Task<OrdersResponseDto> GetOrders()
    {
        var orders = await LoadOrdersForUser(DemoUserId);
        return new OrdersResponseDto(orders.Select(MapOrderSummary).ToList());
    }

    public async Task<ServiceResult<OrderDetailDto>> GetOrder(int id)
    {
        var order = await LoadOrder(id, DemoUserId);
        if (order is null)
        {
            return ServiceResult<OrderDetailDto>.Fail("Order not found.");
        }

        return ServiceResult<OrderDetailDto>.Ok(MapOrderDetail(order));
    }

    public async Task<ServiceResult<CheckoutResponseDto>> Checkout(CheckoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.AddressLine) ||
            string.IsNullOrWhiteSpace(request.City) ||
            string.IsNullOrWhiteSpace(request.PostalCode) ||
            string.IsNullOrWhiteSpace(request.PaymentMethod))
        {
            return ServiceResult<CheckoutResponseDto>.Fail("Complete customer, delivery, and payment fields before checkout.");
        }

        var settings = await GetSiteSettings();
        var cartItems = await LoadCartItems(asNoTracking: false);

        if (cartItems.Count == 0)
        {
            return ServiceResult<CheckoutResponseDto>.Fail("Your cart is empty.");
        }

        foreach (var line in cartItems)
        {
            if (line.Quantity > line.Product.Stock)
            {
                return ServiceResult<CheckoutResponseDto>.Fail($"Not enough stock for {line.Product.Name}.");
            }
        }

        CouponEntity? coupon = null;
        var cartSnapshot = BuildCartResponse(cartItems, settings);

        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var couponResult = await ResolveCoupon(request.CouponCode, cartSnapshot.Subtotal, asTracking: true);
            if (!couponResult.Success || couponResult.Value is null)
            {
                return ServiceResult<CheckoutResponseDto>.Fail(couponResult.Error ?? "Coupon is not valid.");
            }

            coupon = couponResult.Value;
        }

        var discount = coupon is null ? 0m : CalculateDiscount(coupon, cartSnapshot.Subtotal);
        var finalCart = BuildCartResponse(cartItems, settings, discount);
        var nextOrderId = (await _dbContext.Orders.MaxAsync(order => (int?)order.Id) ?? 1000) + 1;
        var normalizedMethod = request.PaymentMethod.Trim();
        var orderDate = DateTime.UtcNow;
        var estimatedDelivery = orderDate.AddDays(normalizedMethod.Equals("Bank transfer", StringComparison.OrdinalIgnoreCase) ? 5 : 3);
        var isCashOnDelivery = normalizedMethod.Equals("Cash on delivery", StringComparison.OrdinalIgnoreCase);

        var order = new OrderEntity
        {
            Id = nextOrderId,
            OrderNumber = $"EB-{nextOrderId}",
            UserId = DemoUserId,
            CouponId = coupon?.Id,
            CustomerName = request.Name.Trim(),
            CustomerEmail = request.Email.Trim(),
            SubtotalAmount = finalCart.Subtotal,
            TotalAmount = finalCart.Total,
            DiscountAmount = finalCart.Discount,
            Status = isCashOnDelivery ? "Pending" : "Paid",
            PaymentMethod = normalizedMethod,
            TrackingNumber = $"TRK-{nextOrderId}",
            CourierName = "EasyBuy Express",
            OrderDate = orderDate,
            EstimatedDeliveryDate = estimatedDelivery,
            ShippingDate = null,
            ShippingAddress = $"{request.AddressLine.Trim()}, {request.City.Trim()}, {request.PostalCode.Trim()}",
            ShippingCost = finalCart.Shipping,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            Payment = new PaymentEntity
            {
                GatewayPaymentId = isCashOnDelivery ? null : $"PAY-{nextOrderId}",
                Amount = finalCart.Total,
                Status = isCashOnDelivery ? "Pending" : "Success",
                PaymentDate = orderDate
            },
            Items = cartItems.Select(item => new OrderItemEntity
            {
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                Quantity = item.Quantity,
                UnitPrice = item.Product.Price
            }).ToList(),
            TrackingEvents = BuildInitialTrackingEvents(orderDate, isCashOnDelivery, request.City.Trim())
        };

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        _dbContext.Orders.Add(order);

        foreach (var line in cartItems)
        {
            line.Product.Stock -= line.Quantity;
        }

        if (coupon is not null)
        {
            coupon.UsedCount++;
        }

        _dbContext.CartItems.RemoveRange(cartItems);

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        var savedOrder = await LoadOrder(nextOrderId, DemoUserId);
        return savedOrder is null
            ? ServiceResult<CheckoutResponseDto>.Fail("Order was created but could not be reloaded.")
            : ServiceResult<CheckoutResponseDto>.Ok(new CheckoutResponseDto(MapOrderDetail(savedOrder)));
    }

    public async Task<AdminSummaryDto> GetAdminSummary()
    {
        var orders = await _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
                .ThenInclude(item => item.Product)
                    .ThenInclude(product => product.Category)
            .OrderByDescending(order => order.OrderDate)
            .ToListAsync();

        var grossRevenue = orders.Sum(order => order.TotalAmount);
        var unitsSold = orders.SelectMany(order => order.Items).Sum(item => item.Quantity);
        var averageOrderValue = orders.Count == 0 ? 0m : grossRevenue / orders.Count;

        var inventoryAlerts = await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.Stock <= 8)
            .OrderBy(product => product.Stock)
            .Select(product => new InventoryAlertDto(
                product.Id,
                product.Name,
                product.Stock,
                product.Stock <= 5 ? "urgent" : "watch"))
            .ToListAsync();

        var categoryPerformance = orders
            .SelectMany(order => order.Items)
            .GroupBy(item => item.Product.Category.Name)
            .Select(group => new CategoryPerformanceDto(
                group.Key,
                group.Sum(item => item.Quantity),
                group.Sum(item => item.Quantity * item.UnitPrice)))
            .OrderByDescending(item => item.Revenue)
            .ToList();

        var recentOrders = orders
            .Take(5)
            .Select(order => new RecentOrderDto(
                order.OrderNumber,
                order.CustomerName,
                order.Status,
                order.TotalAmount,
                order.OrderDate.ToString("yyyy-MM-dd")))
            .ToList();

        return new AdminSummaryDto(
            GrossRevenue: decimal.Round(grossRevenue, 2),
            OrderCount: orders.Count,
            UnitsSold: unitsSold,
            AverageOrderValue: decimal.Round(averageOrderValue, 2),
            InventoryAlerts: inventoryAlerts,
            CategoryPerformance: categoryPerformance,
            RecentOrders: recentOrders);
    }

    private IQueryable<ProductEntity> BuildProductCardQuery()
    {
        return _dbContext.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Include(product => product.Images)
            .Include(product => product.Tags);
    }

    private IQueryable<ProductEntity> BuildProductDetailQuery()
    {
        return _dbContext.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Include(product => product.Images)
            .Include(product => product.Tags)
            .Include(product => product.Specs)
            .Include(product => product.Reviews)
                .ThenInclude(review => review.User);
    }

    private async Task<SiteSettingsEntity> GetSiteSettings()
    {
        return await _dbContext.SiteSettings
            .AsNoTracking()
            .Include(item => item.FeatureCallouts)
            .SingleAsync();
    }

    private async Task<HashSet<int>> GetWishlistSet()
    {
        return await _dbContext.WishlistItems
            .AsNoTracking()
            .Where(item => item.UserId == DemoUserId)
            .Select(item => item.ProductId)
            .ToHashSetAsync();
    }

    private async Task<List<CartItemEntity>> LoadCartItems(bool asNoTracking)
    {
        IQueryable<CartItemEntity> query = _dbContext.CartItems
            .Where(item => item.UserId == DemoUserId)
            .Include(item => item.Product)
                .ThenInclude(product => product.Images);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query
            .OrderBy(item => item.Product.Name)
            .ToListAsync();
    }

    private async Task<WishlistResponseDto> BuildWishlistResponse()
    {
        var wishlist = await GetWishlistSet();
        var products = await BuildProductCardQuery()
            .Where(product => wishlist.Contains(product.Id))
            .OrderByDescending(product => product.IsFeatured)
            .ThenByDescending(product => product.Rating)
            .ToListAsync();

        var items = products.Select(product => MapProductCard(product, wishlist)).ToList();
        return new WishlistResponseDto(items, items.Count);
    }

    private async Task<ServiceResult<CouponEntity>> ResolveCoupon(string? code, decimal subtotal, bool asTracking = false)
    {
        var normalizedCode = (code ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalizedCode))
        {
            return ServiceResult<CouponEntity>.Fail("Enter a coupon code.");
        }

        IQueryable<CouponEntity> query = _dbContext.Coupons.Where(item => item.Code == normalizedCode);
        if (!asTracking)
        {
            query = query.AsNoTracking();
        }

        var coupon = await query.FirstOrDefaultAsync();
        if (coupon is null)
        {
            return ServiceResult<CouponEntity>.Fail("Coupon not found.");
        }

        if (coupon.ExpiryDate < DateTime.UtcNow)
        {
            return ServiceResult<CouponEntity>.Fail("Coupon has expired.");
        }

        if (coupon.UsedCount >= coupon.UsageLimit)
        {
            return ServiceResult<CouponEntity>.Fail("Coupon usage limit reached.");
        }

        if (subtotal < coupon.MinimumSpend)
        {
            return ServiceResult<CouponEntity>.Fail($"Coupon requires a minimum spend of {coupon.MinimumSpend:C0}.");
        }

        return ServiceResult<CouponEntity>.Ok(coupon);
    }

    private async Task<List<OrderEntity>> LoadOrdersForUser(string userId)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Where(order => order.UserId == userId)
            .Include(order => order.Coupon)
            .Include(order => order.Payment)
            .Include(order => order.Items)
            .Include(order => order.TrackingEvents)
            .OrderByDescending(order => order.OrderDate)
            .ToListAsync();
    }

    private async Task<OrderEntity?> LoadOrder(int id, string userId)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Where(order => order.Id == id && order.UserId == userId)
            .Include(order => order.Coupon)
            .Include(order => order.Payment)
            .Include(order => order.Items)
                .ThenInclude(item => item.Product)
                    .ThenInclude(product => product.Images)
            .Include(order => order.TrackingEvents)
            .FirstOrDefaultAsync();
    }

    private CartResponseDto BuildCartResponse(IReadOnlyList<CartItemEntity> cartItems, SiteSettingsEntity settings, decimal discount = 0m)
    {
        var lines = cartItems
            .Select(item => new CartLineDto(
                item.ProductId,
                item.Product.Name,
                item.Product.Summary,
                item.Product.Accent ?? string.Empty,
                item.Product.Badge ?? string.Empty,
                item.Quantity,
                item.Product.Stock,
                item.Product.Price,
                item.Product.Price * item.Quantity,
                GetPrimaryImageUrl(item.Product)))
            .OrderBy(line => line.Name)
            .ToList();

        var subtotal = lines.Sum(line => line.LineTotal);
        var adjustedSubtotal = Math.Max(subtotal - discount, 0m);
        var qualifiesForFreeShipping = adjustedSubtotal >= settings.FreeShippingThreshold;
        var shipping = subtotal == 0m ? 0m : qualifiesForFreeShipping ? 0m : settings.StandardShippingRate;
        var total = adjustedSubtotal + shipping;
        var itemCount = lines.Sum(line => line.Quantity);

        return new CartResponseDto(
            Items: lines,
            ItemCount: itemCount,
            Subtotal: decimal.Round(subtotal, 2),
            Discount: decimal.Round(discount, 2),
            Shipping: decimal.Round(shipping, 2),
            Total: decimal.Round(total, 2),
            QualifiesForFreeShipping: qualifiesForFreeShipping,
            FreeShippingGap: subtotal == 0m || qualifiesForFreeShipping
                ? 0m
                : decimal.Round(settings.FreeShippingThreshold - adjustedSubtotal, 2));
    }

    private ProductCardDto MapProductCard(ProductEntity product, ISet<int> wishlist)
    {
        return new ProductCardDto(
            Id: product.Id,
            Name: product.Name,
            Slug: product.Slug,
            Summary: product.Summary,
            Category: product.Category.Name,
            Price: product.Price,
            CompareAtPrice: product.CompareAtPrice,
            DiscountPercent: GetDiscountPercent(product),
            Stock: product.Stock,
            Rating: product.Rating,
            ReviewCount: product.ReviewCount,
            WarrantyMonths: product.WarrantyMonths,
            Accent: product.Accent ?? string.Empty,
            Badge: product.Badge ?? string.Empty,
            ShippingDays: product.ShippingDays,
            IsFeatured: product.IsFeatured,
            IsNewArrival: product.IsNewArrival,
            IsWishlisted: wishlist.Contains(product.Id),
            Tags: product.Tags
                .OrderBy(tag => tag.SortOrder)
                .Select(tag => tag.Value)
                .ToList(),
            ImageUrl: GetPrimaryImageUrl(product));
    }

    private ProductDetailDto MapProductDetail(ProductEntity product, ISet<int> wishlist)
    {
        return new ProductDetailDto(
            Id: product.Id,
            Name: product.Name,
            Slug: product.Slug,
            Summary: product.Summary,
            Description: product.Description,
            Category: product.Category.Name,
            Price: product.Price,
            CompareAtPrice: product.CompareAtPrice,
            DiscountPercent: GetDiscountPercent(product),
            Stock: product.Stock,
            Rating: product.Rating,
            ReviewCount: product.ReviewCount,
            WarrantyMonths: product.WarrantyMonths,
            Accent: product.Accent ?? string.Empty,
            Badge: product.Badge ?? string.Empty,
            ShippingDays: product.ShippingDays,
            IsFeatured: product.IsFeatured,
            IsNewArrival: product.IsNewArrival,
            IsWishlisted: wishlist.Contains(product.Id),
            Tags: product.Tags
                .OrderBy(tag => tag.SortOrder)
                .Select(tag => tag.Value)
                .ToList(),
            Specs: product.Specs
                .OrderBy(spec => spec.SortOrder)
                .Select(spec => new ProductSpecDto(spec.Label, spec.Value))
                .ToList(),
            Reviews: product.Reviews
                .OrderByDescending(review => review.CreatedAt)
                .Select(review => new ReviewDto(
                    review.User.Name,
                    review.Rating,
                    review.Headline ?? "Customer review",
                    review.Comment,
                    review.CreatedAt.ToString("O")))
                .ToList(),
            ImageUrl: GetPrimaryImageUrl(product),
            Gallery: GetGallery(product));
    }

    private OrderSummaryDto MapOrderSummary(OrderEntity order)
    {
        return new OrderSummaryDto(
            Id: order.Id,
            OrderNumber: order.OrderNumber,
            Status: order.Status,
            TrackingStage: GetTrackingStage(order),
            PaymentStatus: order.Payment?.Status ?? "Pending",
            PaymentMethod: order.PaymentMethod,
            OrderDate: order.OrderDate.ToString("O"),
            EstimatedDeliveryDate: order.EstimatedDeliveryDate?.ToString("O"),
            TrackingNumber: order.TrackingNumber,
            CourierName: order.CourierName,
            ShippingAddress: order.ShippingAddress,
            ItemCount: order.Items.Sum(item => item.Quantity),
            Total: order.TotalAmount);
    }

    private OrderDetailDto MapOrderDetail(OrderEntity order)
    {
        return new OrderDetailDto(
            Id: order.Id,
            OrderNumber: order.OrderNumber,
            Status: order.Status,
            PaymentStatus: order.Payment?.Status ?? "Pending",
            PaymentMethod: order.PaymentMethod,
            OrderDate: order.OrderDate.ToString("O"),
            EstimatedDeliveryDate: order.EstimatedDeliveryDate?.ToString("O"),
            TrackingNumber: order.TrackingNumber,
            CourierName: order.CourierName,
            TrackingStage: GetTrackingStage(order),
            ShippingAddress: order.ShippingAddress,
            CustomerName: order.CustomerName,
            CustomerEmail: order.CustomerEmail,
            Subtotal: order.SubtotalAmount,
            Discount: order.DiscountAmount,
            Shipping: order.ShippingCost,
            Total: order.TotalAmount,
            CouponCode: order.Coupon?.Code,
            Notes: order.Notes,
            Items: order.Items
                .Select(item => new OrderItemDto(
                    item.ProductId,
                    item.ProductName,
                    item.Quantity,
                    item.UnitPrice,
                    item.Quantity * item.UnitPrice,
                    item.Product is null ? null : GetPrimaryImageUrl(item.Product)))
                .ToList(),
            TrackingEvents: order.TrackingEvents
                .OrderByDescending(item => item.OccurredAt)
                .Select(item => new OrderTrackingEventDto(
                    item.EventCode,
                    item.Title,
                    item.Description,
                    item.Location,
                    item.OccurredAt.ToString("O"),
                    item.IsCompleted))
                .ToList());
    }

    private static decimal CalculateDiscount(CouponEntity coupon, decimal subtotal)
    {
        var rawDiscount = coupon.DiscountType.Equals("Percent", StringComparison.OrdinalIgnoreCase)
            ? subtotal * (coupon.DiscountValue / 100m)
            : coupon.DiscountValue;

        return decimal.Round(Math.Min(rawDiscount, subtotal), 2);
    }

    private static int GetDiscountPercent(ProductEntity product)
    {
        if (product.CompareAtPrice <= product.Price || product.CompareAtPrice == 0m)
        {
            return 0;
        }

        var ratio = (product.CompareAtPrice - product.Price) / product.CompareAtPrice * 100m;
        return (int)decimal.Round(ratio, MidpointRounding.AwayFromZero);
    }

    private static List<OrderTrackingEventEntity> BuildInitialTrackingEvents(DateTime orderDate, bool isCashOnDelivery, string city)
    {
        var events = new List<OrderTrackingEventEntity>
        {
            new()
            {
                EventCode = "order_placed",
                Title = "Order placed",
                Description = "We received the order and started confirmation checks.",
                Location = "EasyBuy online store",
                OccurredAt = orderDate,
                IsCompleted = true
            }
        };

        if (isCashOnDelivery)
        {
            events.Add(new OrderTrackingEventEntity
            {
                EventCode = "awaiting_payment",
                Title = "Awaiting payment on delivery",
                Description = "This order will be collected as cash on delivery.",
                Location = "Payment queue",
                OccurredAt = orderDate.AddMinutes(10),
                IsCompleted = true
            });
        }
        else
        {
            events.Add(new OrderTrackingEventEntity
            {
                EventCode = "payment_confirmed",
                Title = "Payment confirmed",
                Description = "Your payment has been authorized successfully.",
                Location = "Payment gateway",
                OccurredAt = orderDate.AddMinutes(3),
                IsCompleted = true
            });
        }

        events.Add(new OrderTrackingEventEntity
        {
            EventCode = "warehouse_queue",
            Title = "Packed for dispatch",
            Description = "The warehouse team is packing your order for courier handoff.",
            Location = $"Fulfilment hub - {city}",
            OccurredAt = orderDate.AddHours(5),
            IsCompleted = false
        });

        return events;
    }

    private static string GetTrackingStage(OrderEntity order)
    {
        return order.TrackingEvents
            .OrderByDescending(item => item.OccurredAt)
            .FirstOrDefault()
            ?.Title ?? order.Status;
    }

    private static string GetPrimaryImageUrl(ProductEntity product)
    {
        return product.Images
            .OrderByDescending(image => image.IsMain)
            .ThenBy(image => image.SortOrder)
            .Select(image => image.ImageUrl)
            .FirstOrDefault() ?? string.Empty;
    }

    private static IReadOnlyList<string> GetGallery(ProductEntity product)
    {
        var gallery = product.Images
            .OrderByDescending(image => image.IsMain)
            .ThenBy(image => image.SortOrder)
            .Select(image => image.ImageUrl)
            .ToList();

        if (gallery.Count == 0)
        {
            gallery.Add(string.Empty);
        }

        return gallery;
    }
}
