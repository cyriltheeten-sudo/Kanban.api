using Kanban.Api.Data;
using Kanban.Api.Models;
using Kanban.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Kanban.Api.Services
{
    public class BoardService
    {
        private readonly AppDbContext _context;
        private readonly TemplateService _templateService;
        public BoardService(AppDbContext context, TemplateService templateService)
        {
            _context = context;
            _templateService = templateService;
        }

        public async Task<Board?> CreateBoard(CreateBoardRequest request)
        {
            var template = await _templateService.GetTemplateById(request.TemplateId);

            if (template is null) return null;

            var board = new Board
            {
                Name = request.Name,
                Columns = template.TemplateColumns
                .Select(tc => new Column { Title = tc.Title, Order = tc.Order })
                .ToList()
            
            };
            _context.Boards.Add(board);
            await _context.SaveChangesAsync();
            return board;
        }

        public Task<int> GetBoardIdFromColumn(int columnId) =>
        _context.Columns.Where(c => c.Id == columnId).Select(c => c.BoardId).FirstAsync();

        public async Task<List<Board>> GetAllBoards()
        {
            return await _context.Boards.ToListAsync();
        }

        public async Task<Board?> GetBoardById(int id)
        {
            var board = await _context.Boards
            .Include(b => b.Columns.OrderBy(c => c.Order))
                .ThenInclude(c => c.Cards.OrderBy(card => card.Order))
            .FirstOrDefaultAsync(b => b.Id == id);
            return board;
        }

        public async Task<bool> UpdateBoard(int id, UpdateBoardRequest request)
        {
            var board = await _context.Boards.FindAsync(id);
            if (board is null) return false;

            board.Name = request.Name;
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
