using System.Formats.Tar;

namespace Kanban.Api.Models;

public class Card
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public int Order { get; set; }

    public int ColumnId { get; set; }
    public Column? Column { get; set; }

    public List<CardEntry> Entries { get; set; } = new();
}