namespace VotingApp.Models;

public class PollOption
{
    public Guid Id { get; set; }
    public Guid PollId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; } = 0;

    public Poll Poll { get; set; } = null!;
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
