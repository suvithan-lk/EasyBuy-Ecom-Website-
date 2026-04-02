namespace Backend.DTOs;

public sealed record AdminSummaryDto(
    decimal GrossRevenue,
    int OrderCount,
    int UnitsSold,
    decimal AverageOrderValue,
    IReadOnlyList<InventoryAlertDto> InventoryAlerts,
    IReadOnlyList<CategoryPerformanceDto> CategoryPerformance,
    IReadOnlyList<RecentOrderDto> RecentOrders);

public sealed record InventoryAlertDto(
    int ProductId,
    string ProductName,
    int Stock,
    string Tone);

public sealed record CategoryPerformanceDto(
    string Category,
    int UnitsSold,
    decimal Revenue);

public sealed record RecentOrderDto(
    string OrderNumber,
    string CustomerName,
    string Status,
    decimal Total,
    string OrderDate);
