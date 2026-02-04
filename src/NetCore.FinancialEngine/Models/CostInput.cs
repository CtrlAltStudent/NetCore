using NetCore.Domain.Enums;

namespace NetCore.FinancialEngine.Models;

public class CostInput
{
    public Guid Id { get; set; }
    public CostType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Guid PeriodId { get; set; }
    public List<CostAssignmentInput> Assignments { get; set; } = new();
}
