namespace VotingApp.DTOs.Admin;

public class VoteRigRequest
{
    public Guid PollId { get; set; }
    public int Count { get; set; } = 1;
    public Guid? OptionId { get; set; }
}
