using Kanban.Api.Models;
using Kanban.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kanban.Tests;

public class BoardServiceTests
{
    [Fact]
    public async Task CreateBoard_FromTemplate_CreatesBoardWithTemplateColumns()
    {
        using var context = TestDbContextFactory.Create();
        var template = new Template
        {
            Id = 1,
            Name = "Kanban simple",
            OwnerId = null,
            TemplateColumns = new List<TemplateColumn>
            {
                new TemplateColumn { Title = "À faire", Order = 0 },
                new TemplateColumn { Title = "En cours", Order = 1 },
                new TemplateColumn { Title = "Terminé", Order = 2 }
            }
        };
        context.Templates.Add(template);
        await context.SaveChangesAsync();

        var service = new BoardService(context, new TemplateService(context));
        var board = await service.CreateBoard(new CreateBoardRequest("Mon projet", 1));

        Assert.NotNull(board);
        Assert.Equal("Mon projet", board!.Name);
        Assert.Equal(3, board.Columns.Count);
        Assert.Equal("À faire", board.Columns[0].Title);
        Assert.Equal("En cours", board.Columns[1].Title);
        Assert.Equal("Terminé", board.Columns[2].Title);
    }

    [Fact]
    public async Task CreateBoard_WithUnknownTemplate_ReturnsNull()
    {
        using var context = TestDbContextFactory.Create();
        var service = new BoardService(context, new TemplateService(context));

        var board = await service.CreateBoard(new CreateBoardRequest("Mon projet", 999));

        Assert.Null(board);
    }

    [Fact]
    public async Task UpdateBoard_ChangesName()
    {
        using var context = TestDbContextFactory.Create();
        context.Boards.Add(new Board { Id = 1, Name = "Ancien nom" });
        await context.SaveChangesAsync();

        var service = new BoardService(context, new TemplateService(context));
        var result = await service.UpdateBoard(1, new UpdateBoardRequest("Nouveau nom"));

        Assert.True(result);

        context.ChangeTracker.Clear();
        var updated = await context.Boards.FindAsync(1);
        Assert.Equal("Nouveau nom", updated!.Name);
    }

    [Fact]
    public async Task UpdateBoard_WhenNotFound_ReturnsFalse()
    {
        using var context = TestDbContextFactory.Create();
        var service = new BoardService(context, new TemplateService(context));

        var result = await service.UpdateBoard(999, new UpdateBoardRequest("Nouveau nom"));

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteBoard_RemovesBoard()
    {
        using var context = TestDbContextFactory.Create();
        context.Boards.Add(new Board { Id = 1, Name = "À supprimer" });
        await context.SaveChangesAsync();

        var service = new BoardService(context, new TemplateService(context));
        var result = await service.DeleteBoard(1);

        Assert.True(result);
        var exists = await context.Boards.AnyAsync(b => b.Id == 1);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteBoard_WhenNotFound_ReturnsFalse()
    {
        using var context = TestDbContextFactory.Create();
        var service = new BoardService(context, new TemplateService(context));

        var result = await service.DeleteBoard(999);

        Assert.False(result);
    }
}
