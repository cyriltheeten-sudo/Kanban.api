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

        public Task<int> GetBoardIdFromColumn(int columnId) =>
        _context.Columns.Where(c => c.Id == columnId).Select(c => c.BoardId).FirstAsync();    
    }
}
