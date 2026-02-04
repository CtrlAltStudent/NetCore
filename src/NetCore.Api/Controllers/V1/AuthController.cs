using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCore.Api.DTOs.Auth;
using NetCore.Api.Services;
using NetCore.Domain.Entities;
using NetCore.Infrastructure.Data;

namespace NetCore.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;

    public AuthController(AppDbContext db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email, ct))
            return BadRequest("Email already registered.");

        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = request.OrganizationName ?? "My Company",
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Organizations.Add(org);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            OrganizationId = org.Id,
            Role = "Admin",
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        var token = _jwt.GenerateToken(user.Id, user.Email, user.OrganizationId, user.Role);
        return Ok(new AuthResponse { Token = token, Email = user.Email, OrganizationId = user.OrganizationId, Role = user.Role });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var user = await _db.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Invalid email or password.");

        var token = _jwt.GenerateToken(user.Id, user.Email, user.OrganizationId, user.Role);
        return Ok(new AuthResponse { Token = token, Email = user.Email, OrganizationId = user.OrganizationId, Role = user.Role });
    }
}
