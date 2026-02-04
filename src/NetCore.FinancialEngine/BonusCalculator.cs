using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NetCore.Domain.Enums;
using NetCore.FinancialEngine.Models;

namespace NetCore.FinancialEngine;

public class BonusCalculator : IBonusCalculator
{
    public IReadOnlyList<BonusResultDto> Calculate(
        MarginResult marginResult,
        IReadOnlyList<BonusRuleInput> rules,
        IReadOnlyDictionary<Guid, string> employeeNames)
    {
        var results = new List<BonusResultDto>();
        employeeNames ??= new Dictionary<Guid, string>();

        foreach (var rule in rules.Where(r => r.FormulaType >= 0))
        {
            var formulaType = (BonusFormulaType)rule.FormulaType;
            var parameters = ParseParameters(rule.ParametersJson);

            if (formulaType == BonusFormulaType.PercentOfCompanyProfit)
            {
                var percent = parameters.GetValueOrDefault("Percent", 0m);
                var amount = marginResult.OperatingProfit * percent;
                if (amount <= 0) continue;
                // This rule type typically applies to a pool; we'd need employee list per rule. For simplicity, skip per-employee here.
                // Caller can assign to employees. We output one "pool" result or require department/employee in rule.
                continue;
            }

            if (formulaType == BonusFormulaType.PercentOfDepartmentProfit && rule.DepartmentId.HasValue)
            {
                var dept = marginResult.ByDepartment.FirstOrDefault(d => d.DepartmentId == rule.DepartmentId);
                if (dept == null) continue;
                var percent = parameters.GetValueOrDefault("Percent", 0m);
                var profit = dept.Profit;
                if (profit <= 0) continue;
                var amount = profit * percent;
                results.Add(new BonusResultDto
                {
                    EmployeeId = Guid.Empty,
                    EmployeeName = "",
                    BonusRuleId = rule.Id,
                    Amount = amount,
                    Details = $"{percent:P0} of department profit {profit:N2} = {amount:N2}"
                });
            }
        }

        return results;
    }

    private static Dictionary<string, decimal> ParseParameters(string json)
    {
        var dict = new Dictionary<string, decimal>();
        if (string.IsNullOrWhiteSpace(json)) return dict;
        try
        {
            var doc = JsonDocument.Parse(json);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.Number && prop.Value.TryGetDecimal(out var d))
                    dict[prop.Name] = d;
            }
        }
        catch { /* ignore */ }
        return dict;
    }
}
