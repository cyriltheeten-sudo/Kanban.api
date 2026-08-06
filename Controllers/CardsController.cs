using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kanban.Api.Data;
using Kanban.Api.Models;

namespace Kanban.Api.Controllers;

// Ce que le front envoie pour créer une carte
public record CreateCardRequest(string Title, int ColumnId);

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CardsController : ControllerBase
{
    private readonly AppDbContext _context;
    public CardsController(AppDbContext context) => _context = context;

    // POST /api/cards
    [HttpPost]
    public async Task<ActionResult<Card>> Create(CreateCardRequest request)
    {
        var maxOrder = await _context.Cards
            .Where(c => c.ColumnId == request.ColumnId)
            .Select(c => (int?)c.Order)
            .MaxAsync() ?? -1;

        var card = new Card
        {
            Title = request.Title,
            ColumnId = request.ColumnId,
            Order = maxOrder + 1,
        };

        _context.Cards.Add(card);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Create), new { id = card.Id }, card);
    }

    // DELETE /api/cards/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var card = await _context.Cards.FindAsync(id);
        if (card is null) return NotFound();

        _context.Cards.Remove(card);
        await _context.SaveChangesAsync();
        return NoContent();   // 204 : succès, rien à renvoyer
    }
}

