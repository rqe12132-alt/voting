namespace VotingApp.DTOs.Poll;

public class PollListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsRealtime { get; set; }
    public bool HasVoted { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? EndsAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
