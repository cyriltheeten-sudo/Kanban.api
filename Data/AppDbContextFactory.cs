using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Kanban.Api.Data;

// Utilisée UNIQUEMENT par les outils EF (dotnet ef), jamais à l'exécution
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=kanban.db")
            .Options;

        return new AppDbContext(options);
    }
}