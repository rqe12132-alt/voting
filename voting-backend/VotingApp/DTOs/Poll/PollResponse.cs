namespace VotingApp.DTOs.Poll;

public class PollResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ResultsVisibility { get; set; } = string.Empty;
    public bool IsRealtime { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PollOptionDto> Options { get; set; } = new();
}
