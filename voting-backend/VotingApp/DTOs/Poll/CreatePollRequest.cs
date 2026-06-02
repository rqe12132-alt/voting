namespace VotingApp.DTOs.Poll;

public class CreatePollRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty; // SingleChoice, MultipleChoice, YesNo
    public List<OptionCreateDto> Options { get; set; } = new();
    public string? ImageUrl { get; set; }
    public string ResultsVisibility { get; set; } = "AlwaysVisible"; // Hidden, VisibleAfterVote, AlwaysVisible
    public bool IsRealtime { get; set; } = true;
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
}
