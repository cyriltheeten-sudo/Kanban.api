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
            .Include(b => b.Columns)          // charge les colonnes
                .ThenInclude(c => c.Cards)    // et pour chaque colonne, ses cartes
            .FirstOrDefaultAsync(b => b.Id == id);

        if (board is null) return NotFound();
        return board;
    }
}