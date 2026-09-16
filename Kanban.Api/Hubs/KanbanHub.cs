using Kanban.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Kanban.Api.Hubs;

[Authorize]
public class KanbanHub : Hub
{
    private readonly BoardService _boardService;

    public KanbanHub(BoardService boardService)
    {
        _boardService = boardService;
    }

    // A client joins a board's "group" to only receive
    // events for THIS board (not all of them).
    public async Task JoinBoard(int boardId)
    {
        var userId = int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        if (!await _boardService.IsBoardOwnedBy(boardId, userId)) return;

        await Groups.AddToGroupAsync(Context.ConnectionId, $"board-{boardId}");
    }

    public async Task LeaveBoard(int boardId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"board-{boardId}");
    }
}
