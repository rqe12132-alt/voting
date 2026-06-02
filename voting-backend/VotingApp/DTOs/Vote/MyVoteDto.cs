namespace VotingApp.DTOs.Vote;

public class MyVoteDto
{
    public Guid PollId { get; set; }
    public List<Guid> OptionIds { get; set; } = new();
    public DateTime VotedAt { get; set; }
}
