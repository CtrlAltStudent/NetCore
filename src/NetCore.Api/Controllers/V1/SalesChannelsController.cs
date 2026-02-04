using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCore.Domain.Entities;
using NetCore.Infrastructure.Data;

namespace NetCore.Api.Controllers.V1;

[ApiController]
[Route("api/v1/sales-channels")]
[Authorize]
public class SalesChannelsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SalesChannelsController(AppDbContext db) => _db = db;

    private Guid OrgId => Guid.Parse(User.FindFirstValue("OrganizationId") ?? Guid.Empty.ToString());

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> List(CancellationToken ct)
    {
        var list = await _db.SalesChannels
            .Where(c => c.OrganizationId == OrgId)
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name, c.CreatedAtUtc })
            .ToListAsync(ct);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<object>> Get(Guid id, CancellationToken ct)
    {
        var c = await _db.SalesChannels
            .Where(x => x.OrganizationId == OrgId && x.Id == id)
            .Select(x => new { x.Id, x.Name, x.CreatedAtUtc })
            .FirstOrDefaultAsync(ct);
        if (c == null) return NotFound();
        return Ok(c);
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] CreateChannelRequest request, CancellationToken ct)
    {
        var entity = new SalesChannel
        {
            Id = Guid.NewGuid(),
            OrganizationId = OrgId,
            Name = request.Name,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.SalesChannels.Add(entity);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, new { entity.Id, entity.Name, entity.CreatedAtUtc });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateChannelRequest request, CancellationToken ct)
    {
        var entity = await _db.SalesChannels.FirstOrDefaultAsync(x => x.OrganizationId == OrgId && x.Id == id, ct);
        if (entity == null) return NotFound();
        entity.Name = request.Name;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _db.SalesChannels.FirstOrDefaultAsync(x => x.OrganizationId == OrgId && x.Id == id, ct);
        if (entity == null) return NotFound();
        _db.SalesChannels.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}

public record CreateChannelRequest(string Name);
public record UpdateChannelRequest(string Name);
