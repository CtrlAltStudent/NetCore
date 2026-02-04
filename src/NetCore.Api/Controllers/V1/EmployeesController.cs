using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCore.Domain.Entities;
using NetCore.Infrastructure.Data;

namespace NetCore.Api.Controllers.V1;

[ApiController]
[Route("api/v1/employees")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _db;

    public EmployeesController(AppDbContext db) => _db = db;

    private Guid OrgId => Guid.Parse(User.FindFirstValue("OrganizationId") ?? Guid.Empty.ToString());

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> List(CancellationToken ct)
    {
        var list = await _db.Employees
            .Where(e => e.OrganizationId == OrgId)
            .OrderBy(e => e.Name)
            .Select(e => new { e.Id, e.Name, e.DepartmentId, e.CreatedAtUtc })
            .ToListAsync(ct);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<object>> Get(Guid id, CancellationToken ct)
    {
        var e = await _db.Employees
            .Where(x => x.OrganizationId == OrgId && x.Id == id)
            .Select(x => new { x.Id, x.Name, x.DepartmentId, x.CreatedAtUtc })
            .FirstOrDefaultAsync(ct);
        if (e == null) return NotFound();
        return Ok(e);
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] CreateEmployeeRequest request, CancellationToken ct)
    {
        var entity = new Employee
        {
            Id = Guid.NewGuid(),
            OrganizationId = OrgId,
            Name = request.Name,
            DepartmentId = request.DepartmentId,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Employees.Add(entity);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, new { entity.Id, entity.Name, entity.DepartmentId, entity.CreatedAtUtc });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateEmployeeRequest request, CancellationToken ct)
    {
        var entity = await _db.Employees.FirstOrDefaultAsync(x => x.OrganizationId == OrgId && x.Id == id, ct);
        if (entity == null) return NotFound();
        entity.Name = request.Name;
        entity.DepartmentId = request.DepartmentId;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _db.Employees.FirstOrDefaultAsync(x => x.OrganizationId == OrgId && x.Id == id, ct);
        if (entity == null) return NotFound();
        _db.Employees.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}

public record CreateEmployeeRequest(string Name, Guid? DepartmentId);
public record UpdateEmployeeRequest(string Name, Guid? DepartmentId);
