# Gemboard — API

> Un espace simple pour se cadrer des étapes et suivre sa progression — un Kanban collaboratif temps réel, volontairement épuré, pensé pour celui qui s'auto-organise.

API back-end de Gemboard, développée en **ASP.NET Core / C#**. Elle gère l'authentification, les tableaux, les colonnes, les cartes, les modèles de projet, et la synchronisation temps réel.

🔗 **Démo en ligne** : https://kanban-cyril14.vercel.app
🔗 **Portfolio** : https://portfolio-cyril14.vercel.app
🔗 **Dépôt front (React)** : https://github.com/cyriltheeten-sudo/kanban-front

---

## Fonctionnalités

- **Authentification** par JWT (inscription, connexion, mots de passe hachés).
- **Tableaux / colonnes / cartes** : CRUD complet, avec réorganisation par glisser-déposer persistée côté serveur.
- **Modèles de projet** : création d'un tableau à partir d'un modèle de colonnes prédéfini (modèles système partagés + base prête pour des modèles personnels par utilisateur).
- **Temps réel** : synchronisation entre clients via SignalR (WebSockets) — les changements d'un utilisateur apparaissent chez les autres sans rechargement.

## Stack technique

| Couche | Technologies |
|---|---|
| Back-end | C#, ASP.NET Core, API REST |
| Accès aux données | Entity Framework Core |
| Base de données | PostgreSQL (hébergée sur Neon) |
| Temps réel | SignalR |
| Authentification | JWT |
| Conteneurisation | Docker |
| Déploiement | Render |
| Tests | xUnit (base InMemory) |

## Architecture

L'API suit une **séparation en couches** :

- **Controllers** — porte d'entrée HTTP : valident la requête, délèguent au service, renvoient le bon code de statut.
- **Services** (`CardService`, `ColumnService`, `BoardService`, `TemplateService`) — la logique métier, isolée et testable (principe de responsabilité unique).
- **Models** — les entités et les contrats de requête.
- **Data** — le `DbContext` Entity Framework et le seed des données de référence.

L'identité de l'utilisateur est extraite du token JWT à chaque requête (stateless).

## Tests

Le projet dispose d'une suite de **tests unitaires (xUnit)** couvrant la logique métier de la couche service : création, mise à jour, suppression, déplacement de cartes, création de tableaux depuis un modèle, et filtrage des modèles système/personnels.

Voir **[TESTING.md](./TESTING.md)** pour la stratégie de tests détaillée.

```bash
dotnet test Kanban.Tests/Kanban.Tests.csproj
```

## Lancer le projet en local

Prérequis : le SDK .NET 8 et une base PostgreSQL (ou un compte Neon).

1. Configurer la chaîne de connexion et la clé JWT via les **User Secrets** (jamais en clair dans le code) :
   ```bash
   cd Kanban.Api
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Database=...;Username=...;Password=...;SSL Mode=Require"
   dotnet user-secrets set "Jwt:Key" "votre-cle-secrete"
   ```
2. Appliquer les migrations et lancer :
   ```bash
   dotnet ef database update
   dotnet run
   ```
3. L'API démarre et Swagger est disponible pour explorer les endpoints.

## Structure du dépôt

```
.
├── Kanban.Api/        # le projet API (contrôleurs, services, modèles, données)
├── Kanban.Tests/      # les tests unitaires (xUnit)
├── Kanban.Api.sln     # la solution
└── TESTING.md         # stratégie de tests
```

---

*Projet personnel développé dans le cadre d'une montée en compétences full stack .NET / React.*
