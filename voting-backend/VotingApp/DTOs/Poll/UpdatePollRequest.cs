namespace VotingApp.DTOs.Poll;

public class UpdatePollRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<OptionCreateDto> Options { get; set; } = new();
    public string? ImageUrl { get; set; }
    public string ResultsVisibility { get; set; } = string.Empty;
    public bool IsRealtime { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
}
