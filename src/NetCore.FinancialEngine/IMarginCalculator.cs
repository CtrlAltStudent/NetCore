using NetCore.FinancialEngine.Models;

namespace NetCore.FinancialEngine;

public interface IMarginCalculator
{
    MarginResult Calculate(
        Guid periodId,
        IReadOnlyList<RevenueInput> revenues,
        IReadOnlyList<CostInput> costs,
        IReadOnlyDictionary<Guid, string>? channelNames = null,
        IReadOnlyDictionary<Guid, string>? departmentNames = null);
}
