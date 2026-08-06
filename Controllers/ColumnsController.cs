using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kanban.Api.Data;
using Kanban.Api.Models;

namespace Kanban.Api.Controllers;

// Ce que le front envoie pour créer une colonne (pas un Column complet)
public record CreateColumnRequest(string Title, int BoardId);

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ColumnsController : ControllerBase
{
    private readonly AppDbContext _context;
    public ColumnsController(AppDbContext context) => _context = context;

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

        return CreatedAtAction(nameof(Create), new { id = column.Id }, column);
    }
}