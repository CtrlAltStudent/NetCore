namespace NetCore.Domain.Entities;

/// <summary>
/// Assigns a portion of a cost to a channel, department, and/or employee.
/// Weight is 0..1 (percentage share). Exactly one of Weight or Amount can be used per assignment.
/// </summary>
public class CostAssignment
{
    public Guid Id { get; set; }
    public Guid CostId { get; set; }
    public Cost Cost { get; set; } = null!;
    public Guid? SalesChannelId { get; set; }
    public SalesChannel? SalesChannel { get; set; }
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    /// <summary>Share of the cost (0..1). Used when distributing by weight.</summary>
    public decimal? Weight { get; set; }
    /// <summary>Fixed amount assigned. Alternative to Weight.</summary>
    public decimal? Amount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
