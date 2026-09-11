namespace Kanban.Api.Models
{
    public record CreateCardRequest(string Title, int ColumnId);
    public record UpdateCardRequest(string Title);
    public record MoveCardRequest(int ColumnId, int Order);
    
}
