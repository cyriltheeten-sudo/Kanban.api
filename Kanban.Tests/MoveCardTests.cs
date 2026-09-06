using Microsoft.EntityFrameworkCore;
using Kanban.Api.Models;
using Kanban.Api.Data;
using Kanban.Api.Services;
using Xunit;

public class MoveCardTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task Move_WithinSameColumn_ReordersCardsCorrectly()
    {
        // ===== ARRANGE : on prépare une colonne avec 3 cartes ordonnées =====
        using var context = CreateInMemoryContext();

        context.Cards.AddRange(
            new Card { Id = 1, ColumnId = 10, Order = 0, Title = "Carte A" },
            new Card { Id = 2, ColumnId = 10, Order = 1, Title = "Carte B" },
            new Card { Id = 3, ColumnId = 10, Order = 2, Title = "Carte C" }
        );
        await context.SaveChangesAsync();

        // ===== ACT : on déplace la carte C (Order 2) tout en haut (Order 0) =====
        var service = new CardService(context);
        await service.MoveCard(await context.Cards.FindAsync(3), new MoveCardRequest(10, 0));

        // ===== ASSERT : on vérifie le nouvel ordre =====
        var cardA = await context.Cards.FindAsync(1);
        var cardB = await context.Cards.FindAsync(2);
        var cardC = await context.Cards.FindAsync(3);

        Assert.Equal(0, cardC.Order);
        Assert.Equal(1, cardA.Order);
        Assert.Equal(2, cardB.Order);
    }

    [Fact]
    public async Task Move_ToAnOtherColumn()
    {
        //ARRANGE
        using var context = CreateInMemoryContext();

        context.Cards.AddRange(
            new Card { Id = 1, ColumnId = 10, Order = 0, Title = "Carte A" },
            new Card { Id = 2, ColumnId = 10, Order = 1, Title = "Carte B" },
            new Card { Id = 3, ColumnId = 11, Order = 0, Title = "Carte C" }
        );
        await context.SaveChangesAsync();

        //ACT
        var service = new CardService(context);
        await service.MoveCard(await context.Cards.FindAsync(3), new MoveCardRequest(10, 0));

        //ASSERT
        var cardA = await context.Cards.FindAsync(1);
        var cardB = await context.Cards.FindAsync(2);
        var cardC = await context.Cards.FindAsync(3);
        var cardsInColumn11 = await context.Cards.Where(c => c.ColumnId == 11).ToListAsync();


        Assert.Equal(10, cardA.ColumnId);
        Assert.Equal(10, cardB.ColumnId);
        Assert.Equal(10, cardC.ColumnId);
        Assert.Equal(1, cardA.Order);
        Assert.Equal(2, cardB.Order);
        Assert.Equal(0, cardC.Order);
        Assert.Empty(cardsInColumn11);
    }
}