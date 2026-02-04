using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCore.Domain.Entities;
using NetCore.Infrastructure.Data;

namespace NetCore.Api.Controllers.V1;

[ApiController]
[Route("api/v1/bonus-rules")]
[Authorize]
public class BonusRulesController : ControllerBase
{
    private readonly AppDbContext _db;

    public BonusRulesController(AppDbContext db) => _db = db;

    private Guid OrgId => Guid.Parse(User.FindFirstValue("OrganizationId") ?? Guid.Empty.ToString());

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> List(CancellationToken ct)
    {
        var list = await _db.BonusRules
            .Where(r => r.OrganizationId == OrgId)
            .OrderBy(r => r.CreatedAtUtc)
            .Select(r => new { r.Id, r.DepartmentId, r.FormulaType, r.ParametersJson, r.IsActive, r.CreatedAtUtc })
            .ToListAsync(ct);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<object>> Get(Guid id, CancellationToken ct)
    {
        var r = await _db.BonusRules
            .Where(x => x.OrganizationId == OrgId && x.Id == id)
            .Select(x => new { x.Id, x.DepartmentId, x.FormulaType, x.ParametersJson, x.IsActive, x.CreatedAtUtc })
            .FirstOrDefaultAsync(ct);
        if (r == null) return NotFound();
        return Ok(r);
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] CreateBonusRuleRequest request, CancellationToken ct)
    {
        var entity = new BonusRule
        {
            Id = Guid.NewGuid(),
            OrganizationId = OrgId,
            DepartmentId = request.DepartmentId,
            FormulaType = (Domain.Enums.BonusFormulaType)request.FormulaType,
            ParametersJson = request.ParametersJson ?? "{}",
            IsActive = request.IsActive,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.BonusRules.Add(entity);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, new { entity.Id, entity.DepartmentId, entity.FormulaType, entity.ParametersJson, entity.IsActive, entity.CreatedAtUtc });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateBonusRuleRequest request, CancellationToken ct)
    {
        var entity = await _db.BonusRules.FirstOrDefaultAsync(x => x.OrganizationId == OrgId && x.Id == id, ct);
        if (entity == null) return NotFound();
        entity.DepartmentId = request.DepartmentId;
        entity.FormulaType = (Domain.Enums.BonusFormulaType)request.FormulaType;
        entity.ParametersJson = request.ParametersJson ?? "{}";
        entity.IsActive = request.IsActive;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _db.BonusRules.FirstOrDefaultAsync(x => x.OrganizationId == OrgId && x.Id == id, ct);
        if (entity == null) return NotFound();
        _db.BonusRules.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}

public record CreateBonusRuleRequest(Guid? DepartmentId, int FormulaType, string? ParametersJson, bool IsActive);
public record UpdateBonusRuleRequest(Guid? DepartmentId, int FormulaType, string? ParametersJson, bool IsActive);
