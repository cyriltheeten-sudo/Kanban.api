using Microsoft.EntityFrameworkCore;
using Kanban.Api.Data;
using Kanban.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite("Data Source=kanban.db"));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

// --- Seed : un board de démo si la base est vide ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.Boards.Any())
    {
        var board = new Board
        {
            Name = "Mon premier projet",
            Columns = new List<Column>
            {
                new Column { Title = "À faire", Order = 0, Cards = new List<Card>
                {
                    new Card { Title = "Configurer le projet", Order = 0 },
                    new Card { Title = "Écrire le modèle", Order = 1 },
                }},
                new Column { Title = "En cours", Order = 1 },
                new Column { Title = "Terminé", Order = 2 },
            }
        };
        db.Boards.Add(board);
        db.SaveChanges();
    }
}

app.Run();