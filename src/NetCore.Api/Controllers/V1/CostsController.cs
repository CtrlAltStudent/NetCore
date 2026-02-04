using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCore.Domain.Entities;
using NetCore.Domain.Enums;
using NetCore.Infrastructure.Data;

namespace NetCore.Api.Controllers.V1;

[ApiController]
[Route("api/v1/costs")]
[Authorize]
public class CostsController : ControllerBase
{
    private readonly AppDbContext _db;

    public CostsController(AppDbContext db) => _db = db;

    private Guid OrgId => Guid.Parse(User.FindFirstValue("OrganizationId") ?? Guid.Empty.ToString());

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> List([FromQuery] Guid? periodId, CancellationToken ct)
    {
        var query = _db.Costs
            .Include(c => c.Assignments)
            .Where(c => c.OrganizationId == OrgId);
        if (periodId.HasValue)
            query = query.Where(c => c.PeriodId == periodId.Value);
        var list = await query
            .OrderBy(c => c.Name)
            .Select(c => new
            {
                c.Id,
                c.Type,
                c.Name,
                c.Amount,
                c.PeriodId,
                c.Currency,
                c.CreatedAtUtc,
                Assignments = c.Assignments.Select(a => new { a.Id, a.SalesChannelId, a.DepartmentId, a.EmployeeId, a.Weight, a.Amount })
            })
            .ToListAsync(ct);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<object>> Get(Guid id, CancellationToken ct)
    {
        var c = await _db.Costs
            .Include(x => x.Assignments)
            .Where(x => x.OrganizationId == OrgId && x.Id == id)
            .Select(c => new
            {
                c.Id,
                c.Type,
                c.Name,
                c.Amount,
                c.PeriodId,
                c.Currency,
                c.CreatedAtUtc,
                Assignments = c.Assignments.Select(a => new { a.Id, a.SalesChannelId, a.DepartmentId, a.EmployeeId, a.Weight, a.Amount })
            })
            .FirstOrDefaultAsync(ct);
        if (c == null) return NotFound();
        return Ok(c);
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] CreateCostRequest request, CancellationToken ct)
    {
        var entity = new Cost
        {
            Id = Guid.NewGuid(),
            OrganizationId = OrgId,
            Type = (CostType)request.Type,
            Name = request.Name,
            Amount = request.Amount,
            PeriodId = request.PeriodId,
            Currency = request.Currency ?? "PLN",
            CreatedAtUtc = DateTime.UtcNow
        };
        foreach (var a in request.Assignments ?? [])
        {
            entity.Assignments.Add(new CostAssignment
            {
                Id = Guid.NewGuid(),
                CostId = entity.Id,
                SalesChannelId = a.SalesChannelId,
                DepartmentId = a.DepartmentId,
                EmployeeId = a.EmployeeId,
                Weight = a.Weight,
                Amount = a.Amount,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
        _db.Costs.Add(entity);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, new { entity.Id, entity.Type, entity.Name, entity.Amount, entity.PeriodId, entity.Currency, entity.CreatedAtUtc });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateCostRequest request, CancellationToken ct)
    {
        var entity = await _db.Costs.Include(c => c.Assignments).FirstOrDefaultAsync(x => x.OrganizationId == OrgId && x.Id == id, ct);
        if (entity == null) return NotFound();
        entity.Type = (CostType)request.Type;
        entity.Name = request.Name;
        entity.Amount = request.Amount;
        entity.PeriodId = request.PeriodId;
        entity.Currency = request.Currency ?? "PLN";
        _db.CostAssignments.RemoveRange(entity.Assignments);
        foreach (var a in request.Assignments ?? [])
        {
            entity.Assignments.Add(new CostAssignment
            {
                Id = Guid.NewGuid(),
                CostId = entity.Id,
                SalesChannelId = a.SalesChannelId,
                DepartmentId = a.DepartmentId,
                EmployeeId = a.EmployeeId,
                Weight = a.Weight,
                Amount = a.Amount,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _db.Costs.FirstOrDefaultAsync(x => x.OrganizationId == OrgId && x.Id == id, ct);
        if (entity == null) return NotFound();
        _db.Costs.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}

public record CostAssignmentRequest(Guid? SalesChannelId, Guid? DepartmentId, Guid? EmployeeId, decimal? Weight, decimal? Amount);
public record CreateCostRequest(int Type, string Name, decimal Amount, Guid PeriodId, string? Currency, List<CostAssignmentRequest>? Assignments);
public record UpdateCostRequest(int Type, string Name, decimal Amount, Guid PeriodId, string? Currency, List<CostAssignmentRequest>? Assignments);
