using Kanban.Api.Data;
using Kanban.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Kanban.Api.Services
{
    public class BoardService
    {
        private readonly AppDbContext _context;
        public BoardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Board> CreateBoard(CreateBoardRequest request)
        {
            var board = new Board
            {
                Name = request.Name,
                Columns = new List<Column>
                {
                    new Column { Title = "À faire", Order = 0 },
                    new Column { Title = "En cours", Order = 1 },
                    new Column { Title = "Terminé", Order = 2 },

                }
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
