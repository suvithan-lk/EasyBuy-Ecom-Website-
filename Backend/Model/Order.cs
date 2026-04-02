namespace Backend.Model;

public sealed class Order
{
    public int Id { get; init; }
    public required string OrderNumber { get; init; }
    public required string CustomerId { get; init; }
    public required string CustomerName { get; init; }
    public required string CustomerEmail { get; init; }
    public required string Status { get; set; }
    public required string PaymentStatus { get; set; }
    public required string PaymentMethod { get; init; }
    public required string ShippingAddress { get; init; }
    public string? TrackingNumber { get; init; }
    public string? CourierName { get; init; }
    public string? CouponCode { get; init; }
    public string? Notes { get; init; }
    public DateTime OrderDate { get; init; }
    public DateTime? EstimatedDeliveryDate { get; init; }
    public decimal Subtotal { get; init; }
    public decimal Discount { get; init; }
    public decimal ShippingCost { get; init; }
    public decimal Total { get; init; }
    public IReadOnlyList<OrderItem> Items { get; init; } = Array.Empty<OrderItem>();
    public IReadOnlyList<TrackingEvent> TrackingEvents { get; init; } = Array.Empty<TrackingEvent>();
}

public sealed record OrderItem(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);

public sealed record TrackingEvent(
    string Code,
    string Title,
    string Description,
    string Location,
    DateTime OccurredAt,
    bool Completed);
