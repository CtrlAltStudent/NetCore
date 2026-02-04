namespace NetCore.FinancialEngine.Models;

public class MarginResult
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalCosts { get; set; }
    public decimal OperatingProfit { get; set; }
    public decimal MarginPercent { get; set; }
    public List<MarginByChannel> ByChannel { get; set; } = new();
    public List<MarginByDepartment> ByDepartment { get; set; } = new();
}

public class MarginByChannel
{
    public Guid? ChannelId { get; set; }
    public string? ChannelName { get; set; }
    public decimal Revenue { get; set; }
    public decimal Costs { get; set; }
    public decimal Profit { get; set; }
    public decimal MarginPercent { get; set; }
}

public class MarginByDepartment
{
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public decimal Revenue { get; set; }
    public decimal Costs { get; set; }
    public decimal Profit { get; set; }
    public decimal MarginPercent { get; set; }
}
