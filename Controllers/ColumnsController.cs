using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kanban.Api.Data;
using Kanban.Api.Models;
using Microsoft.AspNetCore.SignalR;
using Kanban.Api.Hubs;

namespace Kanban.Api.Controllers;

// Ce que le front envoie pour créer une colonne (pas un Column complet)
public record CreateColumnRequest(string Title, int BoardId);

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ColumnsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHubContext<KanbanHub> _hub;
    public ColumnsController(AppDbContext context, IHubContext<KanbanHub> hub)
    {
        _context = context;
        _hub = hub;
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
        // calcule l'ordre : à la fin des colonnes existantes du board
        var maxOrder = await _context.Columns
            .Where(c => c.BoardId == request.BoardId)
            .Select(c => (int?)c.Order)
            .MaxAsync() ?? -1;

        var column = new Column
        {
            Title = request.Title,
            BoardId = request.BoardId,
            Order = maxOrder + 1,
        };

        _context.Columns.Add(column);
        await _context.SaveChangesAsync();

        await NotifyBoardChanged(column.BoardId);

        return CreatedAtAction(nameof(Create), new { id = column.Id }, column);
    }

    // DELETE /api/columns/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var column = await _context.Columns
            .Include(c => c.Cards)      // on charge les cartes pour qu'EF les supprime avec
            .FirstOrDefaultAsync(c => c.Id == id);

        if (column is null) return NotFound();

        var boardId = column.BoardId;

        _context.Columns.Remove(column);
        await _context.SaveChangesAsync();

        await NotifyBoardChanged(boardId);
        return NoContent();
    }
}