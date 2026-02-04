using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCore.FinancialEngine;
using NetCore.FinancialEngine.Models;
using NetCore.Infrastructure.Data;

namespace NetCore.Api.Controllers.V1;

[ApiController]
[Route("api/v1/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMarginCalculator _marginCalculator;

    public ReportsController(AppDbContext db, IMarginCalculator marginCalculator)
    {
        _db = db;
        _marginCalculator = marginCalculator;
    }

    private Guid OrgId => Guid.Parse(User.FindFirstValue("OrganizationId") ?? Guid.Empty.ToString());

    private async Task<MarginResult> GetMarginResultAsync(Guid periodId, CancellationToken ct)
    {
        var revenues = await _db.Revenues
            .Where(r => r.OrganizationId == OrgId && r.PeriodId == periodId)
            .Select(r => new RevenueInput { ChannelId = r.ChannelId, PeriodId = r.PeriodId, Amount = r.Amount })
            .ToListAsync(ct);

        var costs = await _db.Costs
            .Include(c => c.Assignments)
            .Where(c => c.OrganizationId == OrgId && c.PeriodId == periodId)
            .ToListAsync(ct);

        var costInputs = costs.Select(c => new CostInput
        {
            Id = c.Id,
            Type = c.Type,
            Name = c.Name,
            Amount = c.Amount,
            PeriodId = c.PeriodId,
            Assignments = c.Assignments.Select(a => new CostAssignmentInput
            {
                SalesChannelId = a.SalesChannelId,
                DepartmentId = a.DepartmentId,
                EmployeeId = a.EmployeeId,
                Weight = a.Weight,
                Amount = a.Amount
            }).ToList()
        }).ToList();

        var channelIds = revenues.Where(r => r.ChannelId.HasValue).Select(r => r.ChannelId!.Value).Distinct().ToList();
        var channelNames = channelIds.Count > 0
            ? await _db.SalesChannels.Where(s => channelIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, s => s.Name, ct)
            : new Dictionary<Guid, string>();

        var deptIds = costInputs.SelectMany(c => c.Assignments).Where(a => a.DepartmentId.HasValue).Select(a => a.DepartmentId!.Value).Distinct().ToList();
        var departmentNames = deptIds.Count > 0
            ? await _db.Departments.Where(d => deptIds.Contains(d.Id)).ToDictionaryAsync(d => d.Id, d => d.Name, ct)
            : new Dictionary<Guid, string>();

        return _marginCalculator.Calculate(periodId, revenues, costInputs, channelNames, departmentNames);
    }

    [HttpGet("margin")]
    public async Task<ActionResult<MarginResult>> GetMargin([FromQuery] Guid periodId, CancellationToken ct)
    {
        var result = await GetMarginResultAsync(periodId, ct);
        return Ok(result);
    }

    [HttpGet("operating-result")]
    public async Task<ActionResult<object>> GetOperatingResult([FromQuery] Guid periodId, CancellationToken ct)
    {
        var result = await GetMarginResultAsync(periodId, ct);
        return Ok(new { result.TotalRevenue, result.TotalCosts, result.OperatingProfit, result.MarginPercent });
    }

    [HttpGet("by-channel")]
    public async Task<ActionResult<object>> GetByChannel([FromQuery] Guid periodId, CancellationToken ct)
    {
        var result = await GetMarginResultAsync(periodId, ct);
        return Ok(result.ByChannel);
    }

    [HttpGet("by-department")]
    public async Task<ActionResult<object>> GetByDepartment([FromQuery] Guid periodId, CancellationToken ct)
    {
        var result = await GetMarginResultAsync(periodId, ct);
        return Ok(result.ByDepartment);
    }
}
