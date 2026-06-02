namespace VotingApp.DTOs.Poll;

public class PollOptionDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }
}
