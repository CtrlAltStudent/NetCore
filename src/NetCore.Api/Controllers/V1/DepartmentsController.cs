using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCore.Domain.Entities;
using NetCore.Infrastructure.Data;

namespace NetCore.Api.Controllers.V1;

[ApiController]
[Route("api/v1/departments")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public DepartmentsController(AppDbContext db) => _db = db;

    private Guid OrgId => Guid.Parse(User.FindFirstValue("OrganizationId") ?? Guid.Empty.ToString());

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> List(CancellationToken ct)
    {
        var list = await _db.Departments
            .Where(d => d.OrganizationId == OrgId)
            .OrderBy(d => d.Name)
            .Select(d => new { d.Id, d.Name, d.CreatedAtUtc })
            .ToListAsync(ct);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<object>> Get(Guid id, CancellationToken ct)
    {
        var d = await _db.Departments
            .Where(x => x.OrganizationId == OrgId && x.Id == id)
            .Select(x => new { x.Id, x.Name, x.CreatedAtUtc })
            .FirstOrDefaultAsync(ct);
        if (d == null) return NotFound();
        return Ok(d);
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] CreateDepartmentRequest request, CancellationToken ct)
    {
        var entity = new Department
        {
            Id = Guid.NewGuid(),
            OrganizationId = OrgId,
            Name = request.Name,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Departments.Add(entity);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, new { entity.Id, entity.Name, entity.CreatedAtUtc });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateDepartmentRequest request, CancellationToken ct)
    {
        var entity = await _db.Departments.FirstOrDefaultAsync(x => x.OrganizationId == OrgId && x.Id == id, ct);
        if (entity == null) return NotFound();
        entity.Name = request.Name;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _db.Departments.FirstOrDefaultAsync(x => x.OrganizationId == OrgId && x.Id == id, ct);
        if (entity == null) return NotFound();
        _db.Departments.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}

public record CreateDepartmentRequest(string Name);
public record UpdateDepartmentRequest(string Name);
