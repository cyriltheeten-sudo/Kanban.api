using Microsoft.EntityFrameworkCore;
using Kanban.Api.Data;
using Kanban.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Déclare le schéma de sécurité "Bearer"
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Colle ton jeton JWT ici (sans écrire 'Bearer').",
    });

    // Applique ce schéma à toutes les routes protégées
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite("Data Source=kanban.db"));

var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();   // « qui es-tu ? » — vérifie le jeton
app.UseAuthorization();    // « as-tu le droit ? » — vérifie les permissions

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