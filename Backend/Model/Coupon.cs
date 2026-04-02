namespace Backend.Model;

public sealed class Coupon
{
    public required string Code { get; init; }
    public required string Description { get; init; }
    public required string DiscountType { get; init; }
    public decimal Amount { get; init; }
    public decimal MinimumSpend { get; init; }
    public DateTime ExpiresOn { get; init; }
    public int UsageLimit { get; init; }
    public int UsedCount { get; set; }
}
