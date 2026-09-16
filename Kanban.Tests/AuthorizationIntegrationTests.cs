using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Kanban.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kanban.Tests;

// End-to-end authorization tests: real HTTP pipeline, real JWTs, two distinct
// users. Unlike the service-layer tests, these exercise the controllers, which
// is where the ownership checks actually live.
public class AuthorizationIntegrationTests
{
    private const int SeededTemplateId = 1; // "Apprentissage", seeded by DbSeeder on startup

    private static async Task<(int BoardId, int FirstColumnId)> CreateBoardAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/api/boards", new { name, templateId = SeededTemplateId });
        response.EnsureSuccessStatusCode();

        var board = await response.Content.ReadFromJsonAsync<JsonElement>();
        var boardId = board.GetProperty("id").GetInt32();
        var firstColumnId = board.GetProperty("columns")[0].GetProperty("id").GetInt32();
        return (boardId, firstColumnId);
    }

    private static async Task<int> CreateCardAsync(HttpClient client, int columnId, string title)
    {
        var response = await client.PostAsJsonAsync("/api/cards", new { title, columnId });
        response.EnsureSuccessStatusCode();

        var card = await response.Content.ReadFromJsonAsync<JsonElement>();
        return card.GetProperty("id").GetInt32();
    }

    [Fact]
    public async Task GetBoards_WithoutToken_ReturnsUnauthorized()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/boards");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Move_ToColumnInAnotherUsersBoard_ReturnsNotFoundAndDoesNotMoveTheCard()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var tokenA = await AuthTestHelper.RegisterAndLoginAsync(client, db, "alice@test.com", "Alice");
        var tokenB = await AuthTestHelper.RegisterAndLoginAsync(client, db, "bob@test.com", "Bob");

        AuthTestHelper.UseToken(client, tokenA);
        var (_, columnA) = await CreateBoardAsync(client, "Board A");
        var cardId = await CreateCardAsync(client, columnA, "Carte d'Alice");

        AuthTestHelper.UseToken(client, tokenB);
        var (_, columnB) = await CreateBoardAsync(client, "Board B");

        // Alice tries to move her own card into one of Bob's columns.
        AuthTestHelper.UseToken(client, tokenA);
        var moveResponse = await client.PutAsJsonAsync(
            $"/api/cards/{cardId}/move", new { columnId = columnB, order = 0 });

        Assert.Equal(HttpStatusCode.NotFound, moveResponse.StatusCode);

        var card = await db.Cards.FindAsync(cardId);
        Assert.Equal(columnA, card!.ColumnId);
    }

    [Fact]
    public async Task Move_ToColumnInOwnBoard_Succeeds()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var token = await AuthTestHelper.RegisterAndLoginAsync(client, db, "alice@test.com", "Alice");
        AuthTestHelper.UseToken(client, token);

        var (boardId, firstColumn) = await CreateBoardAsync(client, "Board A");
        var cardId = await CreateCardAsync(client, firstColumn, "Carte");

        var boardResponse = await client.GetFromJsonAsync<JsonElement>($"/api/boards/{boardId}");
        var secondColumn = boardResponse.GetProperty("columns")[1].GetProperty("id").GetInt32();

        var moveResponse = await client.PutAsJsonAsync(
            $"/api/cards/{cardId}/move", new { columnId = secondColumn, order = 0 });

        Assert.Equal(HttpStatusCode.NoContent, moveResponse.StatusCode);

        var card = await db.Cards.FindAsync(cardId);
        Assert.Equal(secondColumn, card!.ColumnId);
    }

    [Fact]
    public async Task UpsertEntry_OnAnotherUsersCard_ReturnsNotFoundAndDoesNotCreateAnEntry()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var tokenA = await AuthTestHelper.RegisterAndLoginAsync(client, db, "alice@test.com", "Alice");
        var tokenB = await AuthTestHelper.RegisterAndLoginAsync(client, db, "bob@test.com", "Bob");

        AuthTestHelper.UseToken(client, tokenA);
        var (_, columnA) = await CreateBoardAsync(client, "Board A");
        var cardId = await CreateCardAsync(client, columnA, "Carte d'Alice");

        AuthTestHelper.UseToken(client, tokenB);
        var (_, columnB) = await CreateBoardAsync(client, "Board B");

        // Bob owns columnB, but not Alice's card. He tries to plant an entry on it.
        var response = await client.PutAsJsonAsync(
            $"/api/cards/{cardId}/entries/{columnB}", new { content = "Injecté par Bob" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var entryExists = await db.CardEntries.AnyAsync(e => e.CardId == cardId);
        Assert.False(entryExists);
    }

    [Fact]
    public async Task UpsertEntry_OnOwnCard_Succeeds()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var token = await AuthTestHelper.RegisterAndLoginAsync(client, db, "alice@test.com", "Alice");
        AuthTestHelper.UseToken(client, token);

        var (_, columnA) = await CreateBoardAsync(client, "Board A");
        var cardId = await CreateCardAsync(client, columnA, "Carte d'Alice");

        var response = await client.PutAsJsonAsync(
            $"/api/cards/{cardId}/entries/{columnA}", new { content = "Mon objectif" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var entry = await db.CardEntries.FirstOrDefaultAsync(e => e.CardId == cardId);
        Assert.NotNull(entry);
        Assert.Equal("Mon objectif", entry!.Content);
    }
}
