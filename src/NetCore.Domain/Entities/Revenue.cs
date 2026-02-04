namespace NetCore.Domain.Entities;

public class Revenue
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public Guid? ChannelId { get; set; }
    public SalesChannel? Channel { get; set; }
    public Guid PeriodId { get; set; }
    public Period Period { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "PLN";
    public DateTime Date { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
