namespace Kanban.Api.Models
{
    public class TemplateColumn
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public int Order { get; set; }
        public int TemplateId { get; set; }
        public Template? Template { get; set; }

    }
}
