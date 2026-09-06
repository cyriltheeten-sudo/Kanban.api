using Kanban.Api.Models;
using Kanban.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kanban.Tests;

public class CardServiceTests
{
    [Fact]
    public async Task CreateCard_AddsCardAtEndOfColumn()
    {
        using var context = TestDbContextFactory.Create();
        context.Cards.AddRange(
            new Card { Id = 1, ColumnId = 10, Order = 0, Title = "Carte A" },
            new Card { Id = 2, ColumnId = 10, Order = 1, Title = "Carte B" }
        );
        await context.SaveChangesAsync();

        var service = new CardService(context);
        var created = await service.CreateCard(new CreateCardRequest("Carte C", 10));

        Assert.NotNull(created);
        Assert.Equal(2, created!.Order);
        Assert.Equal("Carte C", created.Title);
        Assert.Equal(10, created.ColumnId);
    }

    [Fact]
    public async Task CreateCard_OnEmptyColumn_StartsAtZero()
    {
        using var context = TestDbContextFactory.Create();
        var service = new CardService(context);

        var created = await service.CreateCard(new CreateCardRequest("Première carte", 10));

        Assert.NotNull(created);
        Assert.Equal(0, created!.Order);
    }

    [Fact]
    public async Task UpdateCard_ChangesTitleAndDescription()
    {
        using var context = TestDbContextFactory.Create();
        var card = new Card { Id = 1, ColumnId = 10, Order = 0, Title = "Ancien titre" };
        context.Cards.Add(card);
        await context.SaveChangesAsync();

        var service = new CardService(context);
        var result = await service.UpdateCard(card, new UpdateCardRequest("Nouveau titre", "Une description"));

        Assert.True(result);

        context.ChangeTracker.Clear();
        var updated = await context.Cards.FindAsync(1);
        Assert.Equal("Nouveau titre", updated!.Title);
        Assert.Equal("Une description", updated.Description);
    }

    [Fact]
    public async Task DeleteCard_RemovesCardFromDatabase()
    {
        using var context = TestDbContextFactory.Create();
        var card = new Card { Id = 1, ColumnId = 10, Order = 0, Title = "Carte A" };
        context.Cards.Add(card);
        await context.SaveChangesAsync();

        var service = new CardService(context);
        var result = await service.DeleteCard(card);

        Assert.True(result);
        var exists = await context.Cards.AnyAsync(c => c.Id == 1);
        Assert.False(exists);
    }

    [Fact]
    public async Task MoveCard_WithinSameColumn_ReordersCards()
    {
        using var context = TestDbContextFactory.Create();
        context.Cards.AddRange(
            new Card { Id = 1, ColumnId = 10, Order = 0, Title = "Carte A" },
            new Card { Id = 2, ColumnId = 10, Order = 1, Title = "Carte B" },
            new Card { Id = 3, ColumnId = 10, Order = 2, Title = "Carte C" }
        );
        await context.SaveChangesAsync();

        var service = new CardService(context);
        var cardC = await context.Cards.FindAsync(3);
        await service.MoveCard(cardC!, new MoveCardRequest(10, 0));

        var cardA = await context.Cards.FindAsync(1);
        var cardB = await context.Cards.FindAsync(2);

        Assert.Equal(0, cardC!.Order);
        Assert.Equal(1, cardA!.Order);
        Assert.Equal(2, cardB!.Order);
    }

    [Fact]
    public async Task MoveCard_ToAnotherColumn_MovesAndReorders()
    {
        using var context = TestDbContextFactory.Create();
        context.Cards.AddRange(
            new Card { Id = 1, ColumnId = 10, Order = 0, Title = "Carte A" },
            new Card { Id = 2, ColumnId = 10, Order = 1, Title = "Carte B" },
            new Card { Id = 3, ColumnId = 11, Order = 0, Title = "Carte C" }
        );
        await context.SaveChangesAsync();

        var service = new CardService(context);
        var cardC = await context.Cards.FindAsync(3);
        await service.MoveCard(cardC!, new MoveCardRequest(10, 0));

        var cardA = await context.Cards.FindAsync(1);
        var cardB = await context.Cards.FindAsync(2);
        var cardsInColumn11 = await context.Cards.Where(c => c.ColumnId == 11).ToListAsync();

        Assert.Equal(10, cardC!.ColumnId);
        Assert.Equal(0, cardC.Order);
        Assert.Equal(1, cardA!.Order);
        Assert.Equal(2, cardB!.Order);
        Assert.Empty(cardsInColumn11);
    }
}
