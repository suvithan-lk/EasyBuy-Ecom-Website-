using Backend.DTOs;

namespace Backend.Services;

public interface IEasyBuyService
{
    Task<HomeResponseDto> GetHome();
    Task<CatalogResponseDto> GetProducts(string? search, string? category, string? sort);
    Task<ServiceResult<ProductDetailDto>> GetProduct(int id);
    Task<CartResponseDto> GetCart();
    Task<ServiceResult<CartResponseDto>> UpsertCartItem(UpsertCartItemRequest request);
    Task<ServiceResult<CartResponseDto>> RemoveCartItem(int productId);
    Task<WishlistResponseDto> GetWishlist();
    Task<ServiceResult<WishlistResponseDto>> ToggleWishlist(int productId);
    Task<ServiceResult<CouponValidationResponse>> ValidateCoupon(CouponValidationRequest request);
    Task<OrdersResponseDto> GetOrders();
    Task<ServiceResult<OrderDetailDto>> GetOrder(int id);
    Task<ServiceResult<CheckoutResponseDto>> Checkout(CheckoutRequest request);
    Task<AdminSummaryDto> GetAdminSummary();
}
