namespace NetCore.FinancialEngine.Models;

public class RevenueInput
{
    public Guid? ChannelId { get; set; }
    public Guid PeriodId { get; set; }
    public decimal Amount { get; set; }
}
