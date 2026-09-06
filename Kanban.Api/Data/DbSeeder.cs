using Kanban.Api.Models;

namespace Kanban.Api.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext db)
        {
            if (!db.Boards.Any())
            {
                var board = new Board
                {
                    Name = "Mon premier projet",
                    Columns = new List<Column>
                    {
                        new Column { Title = "À faire", Order = 0 },
                        new Column { Title = "En cours", Order = 1 },
                        new Column { Title = "Terminé", Order = 2 },
                    }
                };
                db.Boards.Add(board);
                db.SaveChanges();
            }

            if (!db.Templates.Any())
            {
                var templates = new List<Template>
                {
                    new Template
                    {
                        Name = "Kanban simple",
                        OwnerId = null,
                        TemplateColumns = new List<TemplateColumn>
                        {
                        new TemplateColumn { Title = "À faire", Order = 0 },
                        new TemplateColumn { Title = "En cours", Order = 1 },
                        new TemplateColumn { Title = "Terminé", Order = 2 },
                        }
                    },
                    new Template
                    {
                        Name = "Apprentissage",
                        OwnerId = null,
                        TemplateColumns = new List<TemplateColumn>
                        {
                        new TemplateColumn { Title = "Théorie à voir", Order = 0 },
                        new TemplateColumn { Title = "En cours de compréhension", Order = 1 },
                        new TemplateColumn { Title = "En pratique", Order = 2 },
                        new TemplateColumn { Title = "Acquis", Order = 3 },
                        }
                    },
                    new Template
                    {
                        Name = "Cycle de développement",
                        OwnerId = null,
                        TemplateColumns = new List<TemplateColumn>
                        {
                            new TemplateColumn { Title = "Analyse", Order = 0 },
                            new TemplateColumn { Title = "Conception", Order = 1 },
                            new TemplateColumn { Title = "Réalisation", Order = 2 },
                            new TemplateColumn { Title = "Test", Order = 3 },
                            new TemplateColumn { Title = "Déploiement", Order = 4 },
                            new TemplateColumn { Title = "Terminé", Order = 5 },
                        }
                    },
                };
                db.Templates.AddRange(templates);
                db.SaveChanges();
            }
        }
    }
}