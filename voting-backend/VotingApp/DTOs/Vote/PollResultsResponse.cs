namespace VotingApp.DTOs.Vote;

public class PollResultsResponse
{
    public Guid PollId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TotalVotes { get; set; }
    public List<VoteResultDto> Results { get; set; } = new();
}
