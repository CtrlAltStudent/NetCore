using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCore.Domain.Entities;
using NetCore.Infrastructure.Data;

namespace NetCore.Api.Controllers.V1;

[ApiController]
[Route("api/v1/periods")]
[Authorize]
public class PeriodsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PeriodsController(AppDbContext db) => _db = db;

    private Guid OrgId => Guid.Parse(User.FindFirstValue("OrganizationId") ?? Guid.Empty.ToString());

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> List(CancellationToken ct)
    {
        var list = await _db.Periods
            .Where(p => p.OrganizationId == OrgId)
            .OrderByDescending(p => p.StartDate)
            .Select(p => new { p.Id, p.Label, p.StartDate, p.EndDate, p.CreatedAtUtc })
            .ToListAsync(ct);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<object>> Get(Guid id, CancellationToken ct)
    {
        var p = await _db.Periods
            .Where(x => x.OrganizationId == OrgId && x.Id == id)
            .Select(x => new { x.Id, x.Label, x.StartDate, x.EndDate, x.CreatedAtUtc })
            .FirstOrDefaultAsync(ct);
        if (p == null) return NotFound();
        return Ok(p);
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] CreatePeriodRequest request, CancellationToken ct)
    {
        var entity = new Period
        {
            Id = Guid.NewGuid(),
            OrganizationId = OrgId,
            Label = request.Label,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Periods.Add(entity);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, new { entity.Id, entity.Label, entity.StartDate, entity.EndDate, entity.CreatedAtUtc });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdatePeriodRequest request, CancellationToken ct)
    {
        var entity = await _db.Periods.FirstOrDefaultAsync(x => x.OrganizationId == OrgId && x.Id == id, ct);
        if (entity == null) return NotFound();
        entity.Label = request.Label;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _db.Periods.FirstOrDefaultAsync(x => x.OrganizationId == OrgId && x.Id == id, ct);
        if (entity == null) return NotFound();
        _db.Periods.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}

public record CreatePeriodRequest(string Label, DateTime StartDate, DateTime EndDate);
public record UpdatePeriodRequest(string Label, DateTime StartDate, DateTime EndDate);
