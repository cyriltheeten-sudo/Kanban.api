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

    // DELETE /api/cards/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var card = await _context.Cards.FindAsync(id);
        if (card is null) return NotFound();

        _context.Cards.Remove(card);
        await _context.SaveChangesAsync();
        return NoContent();   // 204 : succès, rien à renvoyer
    }

    public record UpdateCardRequest(string Title, string? Description);

    // PUT /api/cards/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCardRequest request)
    {
        var card = await _context.Cards.FindAsync(id);
        if (card is null) return NotFound();

        card.Title = request.Title;
        card.Description = request.Description;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    public record MoveCardRequest(int ColumnId, int Order);

    // PUT /api/cards/{id}/move
    [HttpPut("{id}/move")]
    public async Task<IActionResult> Move(int id, MoveCardRequest request)
    {
        var card = await _context.Cards.FindAsync(id);
        if (card is null) return NotFound();

        // 1. Close the gap in the source column: cards after it shift up by one
        var sourceCards = await _context.Cards
            .Where(c => c.ColumnId == card.ColumnId && c.Order > card.Order)
            .ToListAsync();
        foreach (var c in sourceCards) c.Order--;

        // 2. Make room in the target column: cards at or after the target position shift down by one
        var targetCards = await _context.Cards
            .Where(c => c.ColumnId == request.ColumnId && c.Order >= request.Order)
            .ToListAsync();
        foreach (var c in targetCards) c.Order++;

        // 3. Place the card at its new column and position
        card.ColumnId = request.ColumnId;
        card.Order = request.Order;

        await _context.SaveChangesAsync();
        return NoContent();
    }
}

