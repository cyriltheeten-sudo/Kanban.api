using Kanban.Api.Models;
using Kanban.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kanban.Tests;

public class ColumnServiceTests
{
    [Fact]
    public async Task CreateColumn_AddsColumnAtEndOfBoard()
    {
        using var context = TestDbContextFactory.Create();
        context.Columns.AddRange(
            new Column { Id = 1, BoardId = 5, Order = 0, Title = "À faire" },
            new Column { Id = 2, BoardId = 5, Order = 1, Title = "En cours" }
        );
        await context.SaveChangesAsync();

        var service = new ColumnService(context);
        var created = await service.CreateColumn(new CreateColumnRequest("Terminé", 5));

        Assert.NotNull(created);
        Assert.Equal(2, created!.Order);
        Assert.Equal("Terminé", created.Title);
        Assert.Equal(5, created.BoardId);
    }

    [Fact]
    public async Task CreateColumn_OnEmptyBoard_StartsAtZero()
    {
        using var context = TestDbContextFactory.Create();
        var service = new ColumnService(context);

        var created = await service.CreateColumn(new CreateColumnRequest("Première colonne", 5));

        Assert.NotNull(created);
        Assert.Equal(0, created!.Order);
    }

    [Fact]
    public async Task DeleteColumn_RemovesColumnAndItsCards()
    {
        using var context = TestDbContextFactory.Create();
        var column = new Column
        {
            Id = 1,
            BoardId = 5,
            Order = 0,
            Title = "À faire",
            Cards = new List<Card>
            {
                new Card { Id = 1, Order = 0, Title = "Carte A" },
                new Card { Id = 2, Order = 1, Title = "Carte B" }
            }
        };
        context.Columns.Add(column);
        await context.SaveChangesAsync();

        var service = new ColumnService(context);
        var result = await service.DeleteColumn(1);

        Assert.True(result);
        var columnExists = await context.Columns.AnyAsync(c => c.Id == 1);
        var cardsExist = await context.Cards.AnyAsync(c => c.ColumnId == 1);
        Assert.False(columnExists);
        Assert.False(cardsExist);
    }

    [Fact]
    public async Task DeleteColumn_WhenNotFound_ReturnsFalse()
    {
        using var context = TestDbContextFactory.Create();
        var service = new ColumnService(context);

        var result = await service.DeleteColumn(999);

        Assert.False(result);
    }
}
