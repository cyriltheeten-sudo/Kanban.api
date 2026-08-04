namespace Kanban.Api.Models;

public class Board
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // navigation : un board possède plusieurs colonnes
    public List<Column> Columns { get; set; } = new();
}