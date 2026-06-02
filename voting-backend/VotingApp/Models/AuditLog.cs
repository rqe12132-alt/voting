namespace VotingApp.Models;

public class AuditLog
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;       // CREATE, PUBLISH, DELETE, VOTE, LOGIN
    public string EntityType { get; set; } = string.Empty;   // Poll, Vote
    public string? EntityId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
}
