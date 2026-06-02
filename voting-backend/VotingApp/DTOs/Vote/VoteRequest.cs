namespace VotingApp.DTOs.Vote;

public class VoteRequest
{
    public List<Guid> OptionIds { get; set; } = new();
}
