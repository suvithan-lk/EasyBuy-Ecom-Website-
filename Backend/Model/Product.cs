namespace Backend.Model;

public sealed class Product
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public required string Summary { get; init; }
    public required string Description { get; init; }
    public required string CategorySlug { get; init; }
    public decimal Price { get; init; }
    public decimal CompareAtPrice { get; init; }
    public int Stock { get; set; }
    public double Rating { get; init; }
    public int ReviewCount { get; init; }
    public int WarrantyMonths { get; init; }
    public bool Featured { get; init; }
    public bool NewArrival { get; init; }
    public required string Accent { get; init; }
    public required string Badge { get; init; }
    public int ShippingDays { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public IReadOnlyList<ProductSpec> Specs { get; init; } = Array.Empty<ProductSpec>();
    public IReadOnlyList<Review> Reviews { get; init; } = Array.Empty<Review>();
}

public sealed record ProductSpec(string Label, string Value);

public sealed record Review(
    string Author,
    int Rating,
    string Headline,
    string Comment,
    string PurchasedOn);
