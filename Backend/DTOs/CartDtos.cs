namespace Backend.DTOs;

public sealed record CartResponseDto(
    IReadOnlyList<CartLineDto> Items,
    int ItemCount,
    decimal Subtotal,
    decimal Discount,
    decimal Shipping,
    decimal Total,
    bool QualifiesForFreeShipping,
    decimal FreeShippingGap);

public sealed record CartLineDto(
    int ProductId,
    string Name,
    string Summary,
    string Accent,
    string Badge,
    int Quantity,
    int Stock,
    decimal UnitPrice,
    decimal LineTotal,
    string ImageUrl);

public sealed record UpsertCartItemRequest(int ProductId, int Quantity);

public sealed record CouponValidationRequest(string Code, decimal Subtotal);

public sealed record CouponValidationResponse(
    string Code,
    string Description,
    decimal Discount,
    decimal AdjustedSubtotal);

public sealed record WishlistResponseDto(
    IReadOnlyList<ProductCardDto> Items,
    int Count);
