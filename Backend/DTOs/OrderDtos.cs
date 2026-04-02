namespace Backend.DTOs;

public sealed record CheckoutRequest(
    string Name,
    string Email,
    string AddressLine,
    string City,
    string PostalCode,
    string PaymentMethod,
    string? CouponCode,
    string? Notes);

public sealed record CheckoutResponseDto(OrderDetailDto Order);

public sealed record OrdersResponseDto(IReadOnlyList<OrderSummaryDto> Items);

public sealed record OrderSummaryDto(
    int Id,
    string OrderNumber,
    string Status,
    string TrackingStage,
    string PaymentStatus,
    string PaymentMethod,
    string OrderDate,
    string? EstimatedDeliveryDate,
    string? TrackingNumber,
    string? CourierName,
    string ShippingAddress,
    int ItemCount,
    decimal Total);

public sealed record OrderDetailDto(
    int Id,
    string OrderNumber,
    string Status,
    string PaymentStatus,
    string PaymentMethod,
    string OrderDate,
    string? EstimatedDeliveryDate,
    string? TrackingNumber,
    string? CourierName,
    string TrackingStage,
    string ShippingAddress,
    string CustomerName,
    string CustomerEmail,
    decimal Subtotal,
    decimal Discount,
    decimal Shipping,
    decimal Total,
    string? CouponCode,
    string? Notes,
    IReadOnlyList<OrderItemDto> Items,
    IReadOnlyList<OrderTrackingEventDto> TrackingEvents);

public sealed record OrderItemDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    string? ImageUrl);

public sealed record OrderTrackingEventDto(
    string Code,
    string Title,
    string Description,
    string Location,
    string OccurredAt,
    bool Completed);
