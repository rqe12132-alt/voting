namespace VotingApp.DTOs.Vote;

public class VoteResultDto
{
    public Guid OptionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}
