namespace Backend.DTOs;

public sealed record HomeResponseDto(
    string Brand,
    string ShopperName,
    string Headline,
    string Subheadline,
    decimal FreeShippingThreshold,
    decimal StandardShippingRate,
    IReadOnlyList<string> FeatureCallouts,
    IReadOnlyList<StatDto> Stats,
    IReadOnlyList<CategoryDto> Categories,
    IReadOnlyList<ProductCardDto> FeaturedProducts,
    IReadOnlyList<ProductCardDto> SpotlightProducts);

public sealed record CatalogResponseDto(
    IReadOnlyList<ProductCardDto> Items,
    int TotalItems,
    string Search,
    string? Category,
    string Sort);

public sealed record CategoryDto(
    int Id,
    string Name,
    string Slug,
    string Description,
    int ProductCount,
    string ImageUrl);

public sealed record StatDto(string Label, string Value);

public sealed record ProductCardDto(
    int Id,
    string Name,
    string Slug,
    string Summary,
    string Category,
    decimal Price,
    decimal CompareAtPrice,
    int DiscountPercent,
    int Stock,
    double Rating,
    int ReviewCount,
    int WarrantyMonths,
    string Accent,
    string Badge,
    int ShippingDays,
    bool IsFeatured,
    bool IsNewArrival,
    bool IsWishlisted,
    IReadOnlyList<string> Tags,
    string ImageUrl);

public sealed record ProductDetailDto(
    int Id,
    string Name,
    string Slug,
    string Summary,
    string Description,
    string Category,
    decimal Price,
    decimal CompareAtPrice,
    int DiscountPercent,
    int Stock,
    double Rating,
    int ReviewCount,
    int WarrantyMonths,
    string Accent,
    string Badge,
    int ShippingDays,
    bool IsFeatured,
    bool IsNewArrival,
    bool IsWishlisted,
    IReadOnlyList<string> Tags,
    IReadOnlyList<ProductSpecDto> Specs,
    IReadOnlyList<ReviewDto> Reviews,
    string ImageUrl,
    IReadOnlyList<string> Gallery);

public sealed record ProductSpecDto(string Label, string Value);

public sealed record ReviewDto(
    string Author,
    int Rating,
    string Headline,
    string Comment,
    string PurchasedOn);
