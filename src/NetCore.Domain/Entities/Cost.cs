using NetCore.Domain.Enums;

namespace NetCore.Domain.Entities;

public class Cost
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public CostType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Guid PeriodId { get; set; }
    public Period Period { get; set; } = null!;
    public string Currency { get; set; } = "PLN";
    public DateTime CreatedAtUtc { get; set; }
    public ICollection<CostAssignment> Assignments { get; set; } = new List<CostAssignment>();
}
