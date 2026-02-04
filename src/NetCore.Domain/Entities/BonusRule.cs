using NetCore.Domain.Enums;

namespace NetCore.Domain.Entities;

public class BonusRule
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public BonusFormulaType FormulaType { get; set; }
    /// <summary>JSON or key-value parameters, e.g. {"Percent": 0.1, "MinProfit": 0}</summary>
    public string ParametersJson { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
}
