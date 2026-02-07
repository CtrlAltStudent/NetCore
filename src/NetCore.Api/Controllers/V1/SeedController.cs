using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCore.Domain.Entities;
using NetCore.Domain.Enums;
using NetCore.Infrastructure.Data;

namespace NetCore.Api.Controllers.V1;

/// <summary>
/// Endpoint do wypełnienia organizacji danymi testowymi (okres, kanały, działy, pracownicy, przychody, koszty).
/// Umożliwia przetestowanie dashboardu i raportów bez ręcznego dodawania danych.
/// </summary>
[ApiController]
[Route("api/v1/seed")]
[Authorize]
public class SeedController : ControllerBase
{
    private readonly AppDbContext _db;

    public SeedController(AppDbContext db) => _db = db;

    private Guid OrgId => Guid.Parse(User.FindFirstValue("OrganizationId") ?? Guid.Empty.ToString());

    [HttpPost("test-data")]
    public async Task<ActionResult<object>> SeedTestData([FromQuery] bool force = false, CancellationToken ct = default)
    {
        var hasData = await _db.Periods.AnyAsync(p => p.OrganizationId == OrgId, ct);
        if (hasData && !force)
            return BadRequest(new { message = "Organizacja ma już dane. Użyj parametru ?force=true, aby usunąć je i załadować dane testowe ponownie." });

        if (hasData && force)
        {
            await RemoveOrganizationDataAsync(ct);
        }

        var now = DateTime.UtcNow;
        var periodStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = periodStart.AddMonths(1).AddDays(-1);

        var period = new Period
        {
            Id = Guid.NewGuid(),
            OrganizationId = OrgId,
            Label = $"Okres testowy {periodStart:MM/yyyy}",
            StartDate = periodStart,
            EndDate = periodEnd,
            CreatedAtUtc = now
        };
        _db.Periods.Add(period);

        var ch1 = new SalesChannel { Id = Guid.NewGuid(), OrganizationId = OrgId, Name = "Sklep własny", CreatedAtUtc = now };
        var ch2 = new SalesChannel { Id = Guid.NewGuid(), OrganizationId = OrgId, Name = "Allegro", CreatedAtUtc = now };
        var ch3 = new SalesChannel { Id = Guid.NewGuid(), OrganizationId = OrgId, Name = "Amazon", CreatedAtUtc = now };
        _db.SalesChannels.AddRange(ch1, ch2, ch3);

        var dept1 = new Department { Id = Guid.NewGuid(), OrganizationId = OrgId, Name = "Sprzedaż", CreatedAtUtc = now };
        var dept2 = new Department { Id = Guid.NewGuid(), OrganizationId = OrgId, Name = "Logistyka", CreatedAtUtc = now };
        _db.Departments.AddRange(dept1, dept2);

        var emp1 = new Employee { Id = Guid.NewGuid(), OrganizationId = OrgId, Name = "Anna Kowalska", DepartmentId = dept1.Id, CreatedAtUtc = now };
        var emp2 = new Employee { Id = Guid.NewGuid(), OrganizationId = OrgId, Name = "Jan Nowak", DepartmentId = dept1.Id, CreatedAtUtc = now };
        var emp3 = new Employee { Id = Guid.NewGuid(), OrganizationId = OrgId, Name = "Maria Wiśniewska", DepartmentId = dept2.Id, CreatedAtUtc = now };
        _db.Employees.AddRange(emp1, emp2, emp3);

        _db.Revenues.AddRange(
            new Revenue { Id = Guid.NewGuid(), OrganizationId = OrgId, ChannelId = ch1.Id, PeriodId = period.Id, Amount = 50_000m, Currency = "PLN", Date = periodStart, CreatedAtUtc = now },
            new Revenue { Id = Guid.NewGuid(), OrganizationId = OrgId, ChannelId = ch2.Id, PeriodId = period.Id, Amount = 30_000m, Currency = "PLN", Date = periodStart, CreatedAtUtc = now },
            new Revenue { Id = Guid.NewGuid(), OrganizationId = OrgId, ChannelId = ch3.Id, PeriodId = period.Id, Amount = 20_000m, Currency = "PLN", Date = periodStart, CreatedAtUtc = now }
        );

        var costWynajem = new Cost
        {
            Id = Guid.NewGuid(),
            OrganizationId = OrgId,
            Type = CostType.Fixed,
            Name = "Wynajem biura",
            Amount = 5_000m,
            PeriodId = period.Id,
            Currency = "PLN",
            CreatedAtUtc = now
        };
        costWynajem.Assignments.Add(new CostAssignment { Id = Guid.NewGuid(), CostId = costWynajem.Id, Weight = 1m, CreatedAtUtc = now });
        _db.Costs.Add(costWynajem);

        var costProwizje = new Cost
        {
            Id = Guid.NewGuid(),
            OrganizationId = OrgId,
            Type = CostType.Variable,
            Name = "Prowizje Allegro",
            Amount = 3_000m,
            PeriodId = period.Id,
            Currency = "PLN",
            CreatedAtUtc = now
        };
        costProwizje.Assignments.Add(new CostAssignment { Id = Guid.NewGuid(), CostId = costProwizje.Id, SalesChannelId = ch2.Id, Weight = 1m, CreatedAtUtc = now });
        _db.Costs.Add(costProwizje);

        var costLogistyka = new Cost
        {
            Id = Guid.NewGuid(),
            OrganizationId = OrgId,
            Type = CostType.Variable,
            Name = "Koszty magazynowe",
            Amount = 4_000m,
            PeriodId = period.Id,
            Currency = "PLN",
            CreatedAtUtc = now
        };
        costLogistyka.Assignments.Add(new CostAssignment { Id = Guid.NewGuid(), CostId = costLogistyka.Id, DepartmentId = dept2.Id, Weight = 1m, CreatedAtUtc = now });
        _db.Costs.Add(costLogistyka);

        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            message = "Dane testowe zostały utworzone.",
            periodId = period.Id,
            periodLabel = period.Label,
            channels = 3,
            departments = 2,
            employees = 3,
            revenuesCount = 3,
            costsCount = 3
        });
    }

    private async Task RemoveOrganizationDataAsync(CancellationToken ct)
    {
        var periodIds = await _db.Periods.Where(p => p.OrganizationId == OrgId).Select(p => p.Id).ToListAsync(ct);
        var channelIds = await _db.SalesChannels.Where(c => c.OrganizationId == OrgId).Select(c => c.Id).ToListAsync(ct);
        var deptIds = await _db.Departments.Where(d => d.OrganizationId == OrgId).Select(d => d.Id).ToListAsync(ct);
        var empIds = await _db.Employees.Where(e => e.OrganizationId == OrgId).Select(e => e.Id).ToListAsync(ct);
        var costIds = await _db.Costs.Where(c => c.OrganizationId == OrgId).Select(c => c.Id).ToListAsync(ct);

        if (periodIds.Count > 0)
        {
            await _db.BonusResults.Where(b => periodIds.Contains(b.PeriodId)).ExecuteDeleteAsync(ct);
            await _db.CostAssignments.Where(a => costIds.Contains(a.CostId)).ExecuteDeleteAsync(ct);
            await _db.Costs.Where(c => c.OrganizationId == OrgId).ExecuteDeleteAsync(ct);
            await _db.Revenues.Where(r => r.OrganizationId == OrgId).ExecuteDeleteAsync(ct);
            await _db.BonusRules.Where(b => b.OrganizationId == OrgId).ExecuteDeleteAsync(ct);
            await _db.Employees.Where(e => e.OrganizationId == OrgId).ExecuteDeleteAsync(ct);
            await _db.Departments.Where(d => d.OrganizationId == OrgId).ExecuteDeleteAsync(ct);
            await _db.SalesChannels.Where(c => c.OrganizationId == OrgId).ExecuteDeleteAsync(ct);
            await _db.Periods.Where(p => p.OrganizationId == OrgId).ExecuteDeleteAsync(ct);
        }
    }
}
