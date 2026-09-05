namespace Kanban.Api.Models
{
    public class Template
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public User? Owner { get; set; }
        public int? OwnerId { get; set; }
        public List<TemplateColumn> TemplateColumns { get; set; } = new();
    }
}
