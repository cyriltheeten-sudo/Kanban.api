using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kanban.Api.Data;
using Kanban.Api.Models;
using Microsoft.AspNetCore.SignalR;
using Kanban.Api.Hubs;


namespace Kanban.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BoardsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHubContext<KanbanHub> _hub;
    public BoardsController(AppDbContext context, IHubContext<KanbanHub> hub)
    {
        _context = context;
        _hub = hub;
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

    // GET /api/boards
    [HttpGet]
    public async Task<IEnumerable<Board>> GetAll()
    {
        return await _context.Boards.ToListAsync();
    }

    // GET /api/boards/id
    [HttpGet("{id}")]
    public async Task<ActionResult<Board>> GetById(int id)
    {
        var board = await _context.Boards
            .Include(b => b.Columns.OrderBy(c => c.Order))          
                .ThenInclude(c => c.Cards.OrderBy(card => card.Order))  
            .FirstOrDefaultAsync(b => b.Id == id);

        if (board is null) return NotFound();
        return board;
    }

    public record UpdateBoardRequest(string Name);

    // PUT /api/boards/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateBoardRequest request)
    {
        var board = await _context.Boards.FindAsync(id);
        if (board is null) return NotFound();

        board.Name = request.Name;
        await _context.SaveChangesAsync();
        await NotifyBoardChanged(id);
        return NoContent();
    }
}