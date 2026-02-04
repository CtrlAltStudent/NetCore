namespace NetCore.Domain.Enums;

public enum BonusFormulaType
{
    /// <summary>Percentage of department operating profit.</summary>
    PercentOfDepartmentProfit = 0,
    /// <summary>Percentage of company operating profit (pool).</summary>
    PercentOfCompanyProfit = 1,
    /// <summary>Fixed amount per employee when target is met.</summary>
    FixedWhenTargetMet = 2
}
