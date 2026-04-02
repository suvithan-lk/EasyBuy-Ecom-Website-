namespace Backend.Data;

public sealed class SiteSettingsEntity
{
    public int Id { get; set; }
    public required string Brand { get; set; }
    public required string Headline { get; set; }
    public required string Subheadline { get; set; }
    public decimal FreeShippingThreshold { get; set; }
    public decimal StandardShippingRate { get; set; }
    public List<SiteFeatureCalloutEntity> FeatureCallouts { get; set; } = [];
}

public sealed class SiteFeatureCalloutEntity
{
    public int Id { get; set; }
    public int SiteSettingsId { get; set; }
    public required string Text { get; set; }
    public int SortOrder { get; set; }
    public SiteSettingsEntity SiteSettings { get; set; } = null!;
}

public sealed class UserEntity
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public string? Phone { get; set; }
    public required string Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CartItemEntity> CartItems { get; set; } = [];
    public List<WishlistItemEntity> WishlistItems { get; set; } = [];
    public List<OrderEntity> Orders { get; set; } = [];
    public List<ReviewEntity> Reviews { get; set; } = [];
}

public sealed class CategoryEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public required string Description { get; set; }
    public required string ImageUrl { get; set; }
    public List<ProductEntity> Products { get; set; } = [];
}

public sealed class ProductEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public required string Summary { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public decimal CompareAtPrice { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public CategoryEntity Category { get; set; } = null!;
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public int WarrantyMonths { get; set; }
    public string? Badge { get; set; }
    public string? Accent { get; set; }
    public int ShippingDays { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsNewArrival { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProductImageEntity> Images { get; set; } = [];
    public List<ProductTagEntity> Tags { get; set; } = [];
    public List<ProductSpecEntity> Specs { get; set; } = [];
    public List<ReviewEntity> Reviews { get; set; } = [];
    public List<CartItemEntity> CartItems { get; set; } = [];
    public List<WishlistItemEntity> WishlistItems { get; set; } = [];
    public List<OrderItemEntity> OrderItems { get; set; } = [];
}

public sealed class ProductImageEntity
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public required string ImageUrl { get; set; }
    public bool IsMain { get; set; }
    public int SortOrder { get; set; }
    public ProductEntity Product { get; set; } = null!;
}

public sealed class ProductTagEntity
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public required string Value { get; set; }
    public int SortOrder { get; set; }
    public ProductEntity Product { get; set; } = null!;
}

public sealed class ProductSpecEntity
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public required string Label { get; set; }
    public required string Value { get; set; }
    public int SortOrder { get; set; }
    public ProductEntity Product { get; set; } = null!;
}

public sealed class CouponEntity
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Description { get; set; }
    public required string DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal MinimumSpend { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public List<OrderEntity> Orders { get; set; } = [];
}

public sealed class CartItemEntity
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime AddedAt { get; set; }
    public UserEntity User { get; set; } = null!;
    public ProductEntity Product { get; set; } = null!;
}

public sealed class WishlistItemEntity
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public int ProductId { get; set; }
    public DateTime AddedAt { get; set; }
    public UserEntity User { get; set; } = null!;
    public ProductEntity Product { get; set; } = null!;
}

public sealed class OrderEntity
{
    public int Id { get; set; }
    public required string OrderNumber { get; set; }
    public required string UserId { get; set; }
    public int? CouponId { get; set; }
    public required string CustomerName { get; set; }
    public required string CustomerEmail { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public required string Status { get; set; }
    public required string PaymentMethod { get; set; }
    public string? TrackingNumber { get; set; }
    public string? CourierName { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? ShippingDate { get; set; }
    public required string ShippingAddress { get; set; }
    public decimal ShippingCost { get; set; }
    public string? Notes { get; set; }
    public UserEntity User { get; set; } = null!;
    public CouponEntity? Coupon { get; set; }
    public PaymentEntity? Payment { get; set; }
    public List<OrderItemEntity> Items { get; set; } = [];
    public List<OrderTrackingEventEntity> TrackingEvents { get; set; } = [];
}

public sealed class OrderItemEntity
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public required string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public OrderEntity Order { get; set; } = null!;
    public ProductEntity Product { get; set; } = null!;
}

public sealed class OrderTrackingEventEntity
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public required string EventCode { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Location { get; set; }
    public DateTime OccurredAt { get; set; }
    public bool IsCompleted { get; set; }
    public OrderEntity Order { get; set; } = null!;
}

public sealed class PaymentEntity
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string? GatewayPaymentId { get; set; }
    public decimal Amount { get; set; }
    public required string Status { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? WebhookData { get; set; }
    public OrderEntity Order { get; set; } = null!;
}

public sealed class ReviewEntity
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public int ProductId { get; set; }
    public int Rating { get; set; }
    public required string Comment { get; set; }
    public string? Headline { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool VerifiedPurchase { get; set; }
    public UserEntity User { get; set; } = null!;
    public ProductEntity Product { get; set; } = null!;
}
