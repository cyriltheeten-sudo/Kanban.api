using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Kanban.Api.Models;
using Microsoft.AspNetCore.SignalR;
using Kanban.Api.Hubs;
using Kanban.Api.Services;

namespace Kanban.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ColumnsController : ControllerBase
{
    private readonly IHubContext<KanbanHub> _hub;
    private readonly BoardService _boardService;
    private readonly ColumnService _columnService;
    public ColumnsController(IHubContext<KanbanHub> hub, BoardService boardService, ColumnService columnService)
    {
        _hub = hub;
        _boardService = boardService;
        _columnService = columnService;
    }

    private async Task NotifyBoardChanged(int boardId)
    {
        var senderConnectionId = Request.Headers["X-Connection-Id"].FirstOrDefault();
        if (senderConnectionId is not null)
            await _hub.Clients.GroupExcept($"board-{boardId}", senderConnectionId).SendAsync("BoardChanged");
        else
            await _hub.Clients.Group($"board-{boardId}").SendAsync("BoardChanged");
    }

    // POST /api/columns
    [HttpPost]
    public async Task<ActionResult<Column>> Create(CreateColumnRequest request)
    {
        var column = await _columnService.CreateColumn(request);
        if(column is null) return BadRequest();

        await NotifyBoardChanged(column.BoardId);

        return CreatedAtAction(nameof(Create), new { id = column.Id }, column);
    }

    // DELETE /api/columns/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var boardId = await _boardService.GetBoardIdFromColumn(id);

        bool deleteResponse = await _columnService.DeleteColumn(id);
        if (!deleteResponse) return NotFound();

        await NotifyBoardChanged(boardId);
        return NoContent();
    }
}