namespace Kanban.Api.Models
{
    public record CreateCardRequest(string Title, int ColumnId);
    public record UpdateCardRequest(string Title, string? Description);
    public record MoveCardRequest(int ColumnId, int Order);
    
}
