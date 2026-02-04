namespace NetCore.Domain.Entities;

public class BonusResult
{
    public Guid Id { get; set; }
    public Guid PeriodId { get; set; }
    public Period Period { get; set; } = null!;
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public Guid BonusRuleId { get; set; }
    public BonusRule BonusRule { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Details { get; set; } = string.Empty; // e.g. "10% of dept profit 5000"
    public DateTime CreatedAtUtc { get; set; }
}
