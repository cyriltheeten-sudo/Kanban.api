using Microsoft.AspNetCore.SignalR;

namespace Kanban.Api.Hubs;

public class KanbanHub : Hub
{
    // Un client rejoint le "groupe" d'un tableau pour ne recevoir
    // que les évènements de CE tableau (et pas de tous les autres).
    public async Task JoinBoard(int boardId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"board-{boardId}");
    }

    public async Task LeaveBoard(int boardId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"board-{boardId}");
    }
}