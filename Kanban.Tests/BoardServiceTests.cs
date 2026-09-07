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
        var board = await service.CreateBoard(new CreateBoardRequest("Mon projet", 1), 1);

        Assert.NotNull(board);
        Assert.Equal("Mon projet", board!.Name);
        Assert.Equal(1, board.OwnerId);
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

        var board = await service.CreateBoard(new CreateBoardRequest("Mon projet", 999), 1);

        Assert.Null(board);
    }

    [Fact]
    public async Task GetAllBoards_ReturnsOnlyOwnBoards()
    {
        using var context = TestDbContextFactory.Create();
        context.Boards.AddRange(
            new Board { Id = 1, Name = "Board User 1", OwnerId = 1 },
            new Board { Id = 2, Name = "Board User 1 bis", OwnerId = 1 },
            new Board { Id = 3, Name = "Board User 2", OwnerId = 2 }
        );
        await context.SaveChangesAsync();

        var service = new BoardService(context, new TemplateService(context));
        var result = await service.GetAllBoards(1);

        Assert.Equal(2, result.Count);
        Assert.All(result, b => Assert.Equal(1, b.OwnerId));
    }

    [Fact]
    public async Task GetBoardById_WhenOwner_ReturnsBoard()
    {
        using var context = TestDbContextFactory.Create();
        context.Boards.Add(new Board { Id = 1, Name = "Mon board", OwnerId = 1 });
        await context.SaveChangesAsync();

        var service = new BoardService(context, new TemplateService(context));
        var board = await service.GetBoardById(1, 1);

        Assert.NotNull(board);
        Assert.Equal(1, board!.Id);
    }

    [Fact]
    public async Task GetBoardById_WhenNotOwner_ReturnsNull()
    {
        using var context = TestDbContextFactory.Create();
        context.Boards.Add(new Board { Id = 1, Name = "Board de l'user 2", OwnerId = 2 });
        await context.SaveChangesAsync();

        var service = new BoardService(context, new TemplateService(context));
        var board = await service.GetBoardById(1, 1);

        Assert.Null(board);
    }

    [Fact]
    public async Task UpdateBoard_WhenOwner_ChangesName()
    {
        using var context = TestDbContextFactory.Create();
        context.Boards.Add(new Board { Id = 1, Name = "Ancien nom", OwnerId = 1 });
        await context.SaveChangesAsync();

        var service = new BoardService(context, new TemplateService(context));
        var result = await service.UpdateBoard(1, new UpdateBoardRequest("Nouveau nom"), 1);

        Assert.True(result);

        context.ChangeTracker.Clear();
        var updated = await context.Boards.FindAsync(1);
        Assert.Equal("Nouveau nom", updated!.Name);
    }

    [Fact]
    public async Task UpdateBoard_WhenNotOwner_ReturnsFalseAndKeepsName()
    {
        using var context = TestDbContextFactory.Create();
        context.Boards.Add(new Board { Id = 1, Name = "Nom original", OwnerId = 2 });
        await context.SaveChangesAsync();

        var service = new BoardService(context, new TemplateService(context));
        var result = await service.UpdateBoard(1, new UpdateBoardRequest("Tentative de vol"), 1);

        Assert.False(result);

        context.ChangeTracker.Clear();
        var board = await context.Boards.FindAsync(1);
        Assert.Equal("Nom original", board!.Name);
    }

    [Fact]
    public async Task UpdateBoard_WhenNotFound_ReturnsFalse()
    {
        using var context = TestDbContextFactory.Create();
        var service = new BoardService(context, new TemplateService(context));

        var result = await service.UpdateBoard(999, new UpdateBoardRequest("Nouveau nom"), 1);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteBoard_WhenOwner_RemovesBoard()
    {
        using var context = TestDbContextFactory.Create();
        context.Boards.Add(new Board { Id = 1, Name = "À supprimer", OwnerId = 1 });
        await context.SaveChangesAsync();

        var service = new BoardService(context, new TemplateService(context));
        var result = await service.DeleteBoard(1, 1);

        Assert.True(result);
        var exists = await context.Boards.AnyAsync(b => b.Id == 1);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteBoard_WhenNotOwner_ReturnsFalseAndKeepsBoard()
    {
        using var context = TestDbContextFactory.Create();
        context.Boards.Add(new Board { Id = 1, Name = "Board de l'user 2", OwnerId = 2 });
        await context.SaveChangesAsync();

        var service = new BoardService(context, new TemplateService(context));
        var result = await service.DeleteBoard(1, 1);

        Assert.False(result);
        var stillExists = await context.Boards.AnyAsync(b => b.Id == 1);
        Assert.True(stillExists);
    }

    [Fact]
    public async Task DeleteBoard_WhenNotFound_ReturnsFalse()
    {
        using var context = TestDbContextFactory.Create();
        var service = new BoardService(context, new TemplateService(context));

        var result = await service.DeleteBoard(999, 1);

        Assert.False(result);
    }
}