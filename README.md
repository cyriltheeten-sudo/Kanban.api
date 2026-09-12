# GemBoard — API

> Un outil minimaliste pour se cadrer des étapes et suivre sa progression. Chaque colonne est une **étape guidée**, et une carte **accumule son contenu** au fil de son parcours — elle devient l'historique lisible de sa propre progression.

API back-end de GemBoard, développée en **ASP.NET Core / C#**. Elle gère l'authentification, les tableaux, colonnes, cartes, les entrées de progression par étape, les modèles de projet, et la synchronisation temps réel.

🔗 **Démo en ligne** : https://kanban-cyril14.vercel.app
🔗 **Portfolio** : https://portfolio-cyril14.vercel.app
🔗 **Dépôt front (React)** : https://github.com/cyriltheeten-sudo/kanban-front

---

## Fonctionnalités

- **Authentification** par JWT (mots de passe hachés). La connexion est protégée par un **rate limiting** (5 tentatives/min par IP) contre le brute-force.
- **Tableaux / colonnes / cartes** : CRUD complet, avec réorganisation par glisser-déposer persistée côté serveur.
- **Entrées de carte par étape** : chaque carte porte un contenu distinct par colonne (objectif, ressources, journal, bilan…), créé à la demande et mis à jour via un *upsert* — le cœur de l'expérience « progression guidée ».
- **Modèles de projet** : création d'un tableau à partir d'un modèle de colonnes prédéfini, chaque étape portant sa propre description-guide (modèles système partagés + base prête pour des modèles personnels).
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

- **Controllers** — porte d'entrée HTTP : valident la requête, vérifient les autorisations, délèguent au service, renvoient le bon code de statut.
- **Services** (`CardService`, `ColumnService`, `BoardService`, `TemplateService`) — la logique métier, isolée et testable (principe de responsabilité unique).
- **DTOs** — des objets de lecture dédiés : l'API ne sérialise jamais les entités brutes, elle façonne exactement ce que le client reçoit (et évite les cycles de références).
- **Models** — les entités et les contrats de requête.
- **Data** — le `DbContext` Entity Framework et le seed des données de référence.

Quelques points soignés :

- **Autorisation au niveau des objets** : chaque action mutante vérifie la propriété de la ressource **avant** d'agir (via un contrôle centralisé `IsBoardOwnedBy`, en remontant carte → colonne → tableau → propriétaire). Un accès non autorisé renvoie `404` sans révéler l'existence de la ressource.
- **Identité stateless** : l'utilisateur est toujours extrait du token JWT côté serveur, jamais des données envoyées par le client.
- **Secrets hors du dépôt** : chaîne de connexion et clé JWT via User Secrets en local, variables d'environnement en production.

## Tests

Le projet dispose de **30 tests unitaires (xUnit)** couvrant la logique métier de la couche service : création / mise à jour / suppression / déplacement de cartes, upsert des entrées de progression, création de tableaux depuis un modèle, filtrage des modèles système/personnels, et **cloisonnement des données par utilisateur** (un utilisateur ne peut pas accéder aux tableaux d'un autre).

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
   dotnet user-secrets set "Jwt:Issuer" "Kanban.Api"
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
├── Kanban.Api/        # le projet API (contrôleurs, services, DTOs, modèles, données)
├── Kanban.Tests/      # les tests unitaires (xUnit)
├── Kanban.Api.sln     # la solution
└── TESTING.md         # stratégie de tests
```

---

*Projet personnel développé dans le cadre d'une montée en compétences full stack .NET / React.*
