using NetCore.FinancialEngine.Models;

namespace NetCore.FinancialEngine;

public interface IBonusCalculator
{
    IReadOnlyList<BonusResultDto> Calculate(
        MarginResult marginResult,
        IReadOnlyList<BonusRuleInput> rules,
        IReadOnlyDictionary<Guid, string> employeeNames);
}

public class BonusRuleInput
{
    public Guid Id { get; set; }
    public Guid? DepartmentId { get; set; }
    public int FormulaType { get; set; } // BonusFormulaType enum value
    public string ParametersJson { get; set; } = "{}";
}
