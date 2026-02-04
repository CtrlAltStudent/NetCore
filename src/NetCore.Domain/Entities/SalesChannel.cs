namespace NetCore.Domain.Entities;

public class SalesChannel
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
