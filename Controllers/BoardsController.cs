using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kanban.Api.Data;
using Kanban.Api.Models;
using Kanban.Api.Services;
using Microsoft.AspNetCore.SignalR;
using Kanban.Api.Hubs;


namespace Kanban.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BoardsController : ControllerBase
{
    private readonly IHubContext<KanbanHub> _hub;
    private readonly BoardService _boardService;
    public BoardsController(IHubContext<KanbanHub> hub, BoardService boardService)
    {
        _hub = hub;
        _boardService = boardService;
    }

    private async Task NotifyBoardChanged(int boardId)
    {
        var senderConnectionId = Request.Headers["X-Connection-Id"].FirstOrDefault();

        if (senderConnectionId is not null)
            await _hub.Clients.GroupExcept($"board-{boardId}", senderConnectionId)
                .SendAsync("BoardChanged");
        else
            await _hub.Clients.Group($"board-{boardId}").SendAsync("BoardChanged");
    }

    // POST /api/boards
    [HttpPost]
    public async Task<ActionResult<Board>> Create(CreateBoardRequest request)
    {
        var board = await _boardService.CreateBoard(request);
        if (board is null) return BadRequest();

        return CreatedAtAction(nameof(Create), new { id = board.Id }, board);
    }

    // GET /api/boards
    [HttpGet]
    public async Task<List<Board>> GetAll()
    {
        return await _boardService.GetAllBoards();
    }

    // GET /api/boards/id
    [HttpGet("{id}")]
    public async Task<ActionResult<Board>> GetById(int id)
    {
        var board = await _boardService.GetBoardById(id);
        if (board is null) return NotFound();
        return board;
    }

    // PUT /api/boards/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateBoardRequest request)
    {
        bool updateResponse = await _boardService.UpdateBoard(id, request);
        if (!updateResponse) return NotFound();

        await NotifyBoardChanged(id);
        return NoContent();
    }
}