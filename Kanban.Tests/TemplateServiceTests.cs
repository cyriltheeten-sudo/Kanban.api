using Kanban.Api.Models;
using Kanban.Api.Services;
using Xunit;

namespace Kanban.Tests;

public class TemplateServiceTests
{
    [Fact]
    public async Task GetTemplatesForUser_ReturnsSystemAndOwnTemplates_ExcludesOthers()
    {
        using var context = TestDbContextFactory.Create();
        context.Templates.AddRange(
            new Template { Id = 1, Name = "Système", OwnerId = null },
            new Template { Id = 2, Name = "Perso utilisateur 42", OwnerId = 42 },
            new Template { Id = 3, Name = "Perso utilisateur 99", OwnerId = 99 }
        );
        await context.SaveChangesAsync();

        var service = new TemplateService(context);
        var result = await service.GetTemplatesForUser(42);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Name == "Système");
        Assert.Contains(result, t => t.Name == "Perso utilisateur 42");
        Assert.DoesNotContain(result, t => t.Name == "Perso utilisateur 99");
    }

    [Fact]
    public async Task GetTemplatesForUser_WithOnlySystemTemplates_ReturnsThem()
    {
        using var context = TestDbContextFactory.Create();
        context.Templates.AddRange(
            new Template { Id = 1, Name = "Kanban simple", OwnerId = null },
            new Template { Id = 2, Name = "Cycle de développement", OwnerId = null }
        );
        await context.SaveChangesAsync();

        var service = new TemplateService(context);
        var result = await service.GetTemplatesForUser(42);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetTemplateById_ReturnsTemplateWithColumns()
    {
        using var context = TestDbContextFactory.Create();
        context.Templates.Add(new Template
        {
            Id = 1,
            Name = "Kanban simple",
            OwnerId = null,
            TemplateColumns = new List<TemplateColumn>
            {
                new TemplateColumn { Title = "À faire", Order = 0 },
                new TemplateColumn { Title = "Terminé", Order = 1 }
            }
        });
        await context.SaveChangesAsync();

        var service = new TemplateService(context);
        var template = await service.GetTemplateById(1);

        Assert.NotNull(template);
        Assert.Equal(2, template!.TemplateColumns.Count);
    }

    [Fact]
    public async Task GetTemplateById_WhenNotFound_ReturnsNull()
    {
        using var context = TestDbContextFactory.Create();
        var service = new TemplateService(context);

        var template = await service.GetTemplateById(999);

        Assert.Null(template);
    }
}
