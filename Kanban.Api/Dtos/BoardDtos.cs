namespace Kanban.Api.Dtos;

public class BoardDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<ColumnDto> Columns { get; set; } = new();
}

public class ColumnDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int Order { get; set; }
    public List<CardDto> Cards { get; set; } = new();
}

public class CardDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public int Order { get; set; }
    public List<CardEntryDto> Entries { get; set; } = new();
}

public class CardEntryDto
{
    public int Id { get; set; }
    public int ColumnId { get; set; }
    public string Content { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
}