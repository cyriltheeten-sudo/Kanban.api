using Kanban.Api.Data;
using Kanban.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Kanban.Api.Services
{
    public class CardService
    {
        private readonly AppDbContext _context;

        public CardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Card?> CreateCard(CreateCardRequest request)
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
            return card;          
        }

        public async Task<Card?> GetCardById(int id)
        {
            var card = await _context.Cards.FindAsync(id);
            if (card is null) return null;

            return card;
        }

        public async Task<bool> UpdateCard(Card card, UpdateCardRequest request)
        {
            if (card is null) return false;

            card.Title = request.Title;
            card.Description = request.Description;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCard(Card card)
        {
            if (card is null) return false;
            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Card?> MoveCard(Card card, MoveCardRequest request)
        {
            if (card is null) return null;

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
            return card;
        }

    }
}
