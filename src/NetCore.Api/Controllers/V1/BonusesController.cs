using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCore.FinancialEngine;
using NetCore.FinancialEngine.Models;
using NetCore.Infrastructure.Data;

namespace NetCore.Api.Controllers.V1;

[ApiController]
[Route("api/v1/bonuses")]
[Authorize]
public class BonusesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMarginCalculator _marginCalculator;
    private readonly IBonusCalculator _bonusCalculator;

    public BonusesController(AppDbContext db, IMarginCalculator marginCalculator, IBonusCalculator bonusCalculator)
    {
        _db = db;
        _marginCalculator = marginCalculator;
        _bonusCalculator = bonusCalculator;
    }

    private Guid OrgId => Guid.Parse(User.FindFirstValue("OrganizationId") ?? Guid.Empty.ToString());

    [HttpGet("calculate")]
    public async Task<ActionResult<IEnumerable<BonusResultDto>>> Calculate([FromQuery] Guid periodId, CancellationToken ct)
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

        var channelNames = await _db.SalesChannels.Where(s => s.OrganizationId == OrgId).ToDictionaryAsync(s => s.Id, s => s.Name, ct);
        var departmentNames = await _db.Departments.Where(d => d.OrganizationId == OrgId).ToDictionaryAsync(d => d.Id, d => d.Name, ct);
        var marginResult = _marginCalculator.Calculate(periodId, revenues, costInputs, channelNames, departmentNames);

        var rules = await _db.BonusRules
            .Where(r => r.OrganizationId == OrgId && r.IsActive)
            .Select(r => new BonusRuleInput
            {
                Id = r.Id,
                DepartmentId = r.DepartmentId,
                FormulaType = (int)r.FormulaType,
                ParametersJson = r.ParametersJson
            })
            .ToListAsync(ct);

        var employeeNames = await _db.Employees.Where(e => e.OrganizationId == OrgId).ToDictionaryAsync(e => e.Id, e => e.Name, ct);

        var results = _bonusCalculator.Calculate(marginResult, rules, employeeNames);
        return Ok(results);
    }
}
