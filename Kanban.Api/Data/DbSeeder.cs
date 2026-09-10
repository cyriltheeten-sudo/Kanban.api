using Kanban.Api.Models;

namespace Kanban.Api.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext db)
        {
            if (!db.Templates.Any())
            {
                var templates = new List<Template>
                {
                    new Template
                    {
                        Name = "Apprentissage",
                        OwnerId = null,
                        TemplateColumns = new List<TemplateColumn>
                        {
                            new TemplateColumn { Title = "Théorie à voir", Order = 0,
                                Description = "Ce que tu veux apprendre, et l'objectif à atteindre pour cette carte." },
                            new TemplateColumn { Title = "En cours de compréhension", Order = 1,
                                Description = "Les infos notables, cours, liens, vidéos et notes qui t'aident à comprendre." },
                            new TemplateColumn { Title = "En pratique", Order = 2,
                                Description = "Ton journal de bord : ce que tu fais concrètement, jour après jour, pour progresser." },
                            new TemplateColumn { Title = "Acquis", Order = 3,
                                Description = "Le résultat obtenu et ce qui a changé une fois l'objectif atteint." },
                        }
                    },
                    new Template
                    {
                        Name = "Cycle de développement",
                        OwnerId = null,
                        TemplateColumns = new List<TemplateColumn>
                        {
                            new TemplateColumn { Title = "Analyse", Order = 0,
                                Description = "Le besoin (feature, bug…) et l'objectif visé." },
                            new TemplateColumn { Title = "Conception", Order = 1,
                                Description = "Où l'on intervient dans l'architecture (fichiers créés/modifiés/supprimés) et la ou les branches Git front/back." },
                            new TemplateColumn { Title = "Réalisation", Order = 2,
                                Description = "Les opérations effectuées et pourquoi (commits liés si utile)." },
                            new TemplateColumn { Title = "Test", Order = 3,
                                Description = "Les tests réalisés, reliés à ce qui a été fait." },
                            new TemplateColumn { Title = "Déploiement", Order = 4,
                                Description = "Le résultat des déploiements." },
                            new TemplateColumn { Title = "Terminé", Order = 5,
                                Description = "Constat et résultats après déploiement." },
                        }
                    },
                };
                db.Templates.AddRange(templates);
                db.SaveChanges();
            }
        }
    }
}