namespace VotingApp.Models;

public class PersonalId
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public bool IsUsed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Optional navigation
    public User? User { get; set; }
}
