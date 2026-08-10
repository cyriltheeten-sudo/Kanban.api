using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kanban.Api.Data;
using Kanban.Api.Models;

namespace Kanban.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BoardsController : ControllerBase
{
    private readonly AppDbContext _context;
    public BoardsController(AppDbContext context) => _context = context;

    // GET /api/boards → tous les boards (sans le détail)
    [HttpGet]
    public async Task<IEnumerable<Board>> GetAll()
    {
        return await _context.Boards.ToListAsync();
    }

    // GET /api/boards/1 → un board AVEC ses colonnes et leurs cartes
    [HttpGet("{id}")]
    public async Task<ActionResult<Board>> GetById(int id)
    {
        var board = await _context.Boards
            .Include(b => b.Columns.OrderBy(c => c.Order))          // colonnes triées
                .ThenInclude(c => c.Cards.OrderBy(card => card.Order))  // cartes triées
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
        return NoContent();
    }
}