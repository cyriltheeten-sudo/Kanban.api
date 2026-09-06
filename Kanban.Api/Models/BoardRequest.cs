namespace Kanban.Api.Models
{
    public record CreateBoardRequest(string Name, int TemplateId);
    public record UpdateBoardRequest(string Name);
}
