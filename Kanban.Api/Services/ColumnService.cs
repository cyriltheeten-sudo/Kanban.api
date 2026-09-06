using Kanban.Api.Data;
using Kanban.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Kanban.Api.Services
{
    public class ColumnService
    {
        private readonly AppDbContext _context;

        public ColumnService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Column?> CreateColumn(CreateColumnRequest request)
        {
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
            return column;
        }

        public async Task<bool> DeleteColumn(int id)
        {
            var column = await _context.Columns
            .Include(c => c.Cards)
            .FirstOrDefaultAsync(c => c.Id == id);

            if (column is null) return false;

            _context.Columns.Remove(column);
            await _context.SaveChangesAsync();
            return true;
        }


    }
}
