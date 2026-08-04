namespace Kanban.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";   // le mot de passe HACHÉ, jamais en clair
    public string Name { get; set; } = "";
}