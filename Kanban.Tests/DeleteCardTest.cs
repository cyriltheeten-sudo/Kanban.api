using Microsoft.EntityFrameworkCore;
using Kanban.Api.Models;
using Kanban.Api.Data;
using Kanban.Api.Services;
using Xunit;

public class DeleteCardTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task DeleteCard()
    {
        using var context = CreateInMemoryContext();

        context.Cards.AddRange(
            new Card { Id = 1, ColumnId = 10, Order = 0, Title = "Carte A" }
        );
        await context.SaveChangesAsync();

        var service = new CardService(context);
        var deletedCard = await context.Cards.FindAsync(1);

        await service.DeleteCard(deletedCard);

        var exists = await context.Cards.AnyAsync(c => c.Id == 1);
        Assert.False(exists);


    }
}