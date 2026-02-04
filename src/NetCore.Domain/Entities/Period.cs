namespace NetCore.Domain.Entities;

public class Period
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Label { get; set; } = string.Empty; // e.g. "2024-01", "Q1 2024"
    public DateTime CreatedAtUtc { get; set; }
}
