using Kanban.Api.Data;
using Kanban.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Kanban.Api.Services
{
    public class TemplateService
    {
        private readonly AppDbContext _context;

        public TemplateService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Template>> GetTemplatesForUser(int userId)
        {
            return await _context.Templates
                .Include(t => t.TemplateColumns.OrderBy(c => c.Order))
                .Where(t => t.OwnerId == null || t.OwnerId == userId)
                .ToListAsync();
        }
    }
}