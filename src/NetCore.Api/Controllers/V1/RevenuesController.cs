using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCore.Domain.Entities;
using NetCore.Infrastructure.Data;

namespace NetCore.Api.Controllers.V1;

[ApiController]
[Route("api/v1/revenues")]
[Authorize]
public class RevenuesController : ControllerBase
{
    private readonly AppDbContext _db;

    public RevenuesController(AppDbContext db) => _db = db;

    private Guid OrgId => Guid.Parse(User.FindFirstValue("OrganizationId") ?? Guid.Empty.ToString());

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> List([FromQuery] Guid? periodId, CancellationToken ct)
    {
        var query = _db.Revenues.Where(r => r.OrganizationId == OrgId);
        if (periodId.HasValue)
            query = query.Where(r => r.PeriodId == periodId.Value);
        var list = await query
            .OrderBy(r => r.Date)
            .Select(r => new { r.Id, r.ChannelId, r.PeriodId, r.Amount, r.Currency, r.Date, r.CreatedAtUtc })
            .ToListAsync(ct);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<object>> Get(Guid id, CancellationToken ct)
    {
        var r = await _db.Revenues
            .Where(x => x.OrganizationId == OrgId && x.Id == id)
            .Select(x => new { x.Id, x.ChannelId, x.PeriodId, x.Amount, x.Currency, x.Date, x.CreatedAtUtc })
            .FirstOrDefaultAsync(ct);
        if (r == null) return NotFound();
        return Ok(r);
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] CreateRevenueRequest request, CancellationToken ct)
    {
        var entity = new Revenue
        {
            Id = Guid.NewGuid(),
            OrganizationId = OrgId,
            ChannelId = request.ChannelId,
            PeriodId = request.PeriodId,
            Amount = request.Amount,
            Currency = request.Currency ?? "PLN",
            Date = request.Date,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Revenues.Add(entity);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, new { entity.Id, entity.ChannelId, entity.PeriodId, entity.Amount, entity.Currency, entity.Date, entity.CreatedAtUtc });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateRevenueRequest request, CancellationToken ct)
    {
        var entity = await _db.Revenues.FirstOrDefaultAsync(x => x.OrganizationId == OrgId && x.Id == id, ct);
        if (entity == null) return NotFound();
        entity.ChannelId = request.ChannelId;
        entity.PeriodId = request.PeriodId;
        entity.Amount = request.Amount;
        entity.Currency = request.Currency ?? "PLN";
        entity.Date = request.Date;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _db.Revenues.FirstOrDefaultAsync(x => x.OrganizationId == OrgId && x.Id == id, ct);
        if (entity == null) return NotFound();
        _db.Revenues.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}

public record CreateRevenueRequest(Guid? ChannelId, Guid PeriodId, decimal Amount, string? Currency, DateTime Date);
public record UpdateRevenueRequest(Guid? ChannelId, Guid PeriodId, decimal Amount, string? Currency, DateTime Date);
