namespace VotingApp.DTOs.Poll;

public class OptionCreateDto
{
    public string Text { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
