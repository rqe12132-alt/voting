using Microsoft.AspNetCore.SignalR;

namespace VotingApp.Hubs;

public class PollHub : Hub
{
    public async Task JoinPollGroup(string pollId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, pollId);
    }

    public async Task LeavePollGroup(string pollId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, pollId);
    }
}
