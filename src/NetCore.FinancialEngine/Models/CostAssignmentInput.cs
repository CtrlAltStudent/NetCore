namespace NetCore.FinancialEngine.Models;

public class CostAssignmentInput
{
    public Guid? SalesChannelId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? EmployeeId { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Amount { get; set; }
}
