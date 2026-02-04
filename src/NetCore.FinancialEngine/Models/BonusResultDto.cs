namespace NetCore.FinancialEngine.Models;

public class BonusResultDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid BonusRuleId { get; set; }
    public decimal Amount { get; set; }
    public string Details { get; set; } = string.Empty;
}
