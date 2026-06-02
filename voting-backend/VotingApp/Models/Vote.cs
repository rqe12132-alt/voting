namespace VotingApp.Models;

public class Vote
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PollId { get; set; }
    public Guid OptionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Poll Poll { get; set; } = null!;
    public PollOption Option { get; set; } = null!;
}
