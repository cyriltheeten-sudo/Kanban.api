using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Kanban.Api.Data;
using Kanban.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace Kanban.Tests;

public static class AuthTestHelper
{
    // Seeds a user directly in the database (bypassing the admin-gated /register
    // endpoint) then goes through the real /login endpoint to obtain a genuine JWT.
    public static async Task<string> RegisterAndLoginAsync(
        HttpClient client, AppDbContext db, string email, string name, string password = "Password123!")
    {
        var user = new User { Email = email, Name = name };
        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, password);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("token").GetString()!;
    }

    public static void UseToken(HttpClient client, string token) =>
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
}
