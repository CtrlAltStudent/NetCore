namespace NetCore.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public string Role { get; set; } = "User"; // Admin, User
    public DateTime CreatedAtUtc { get; set; }
}
