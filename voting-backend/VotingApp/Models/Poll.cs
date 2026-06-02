namespace VotingApp.Models;

public enum PollType
{
    SingleChoice,
    MultipleChoice,
    YesNo
}

public enum PollStatus
{
    Draft,
    Active,
    Closed
}

public enum ResultsVisibility
{
    Hidden,
    VisibleAfterVote,
    AlwaysVisible
}

public class Poll
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PollType Type { get; set; }
    public PollStatus Status { get; set; } = PollStatus.Draft;
    public ResultsVisibility ResultsVisibility { get; set; } = ResultsVisibility.AlwaysVisible;
    public bool IsRealtime { get; set; } = true;
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public string? ImageUrl { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User CreatedBy { get; set; } = null!;
    public ICollection<PollOption> Options { get; set; } = new List<PollOption>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
