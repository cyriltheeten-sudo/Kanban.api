namespace Kanban.Api.Models;

public class CardEntry
{
    public int Id { get; set; }
    public string Content { get; set; } = "";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int CardId { get; set; }
    public Card? Card { get; set; }

    public int ColumnId { get; set; }
    public Column? Column { get; set; }
}