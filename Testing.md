# Stratégie de tests — Gemboard API

## Objectif

Ce document décrit la stratégie de tests unitaires de la couche service de l'API Gemboard. Les tests vérifient la logique métier (création, modification, suppression, déplacement, filtrage) et le **cloisonnement des données par utilisateur**, indépendamment de l'infrastructure (base de données réelle, HTTP, temps réel).

## Approche

- **Type de tests** : tests unitaires sur la couche service (`CardService`, `ColumnService`, `BoardService`, `TemplateService`).
- **Framework** : xUnit.
- **Base de données** : Entity Framework Core InMemory. Chaque test s'exécute sur une base isolée (nom unique via `Guid`), ce qui garantit qu'aucun test n'influence un autre.
- **Structure** : chaque test suit le motif **AAA** (Arrange / Act / Assert) — préparer les données, exécuter l'action testée, vérifier le résultat observable.

## Périmètre

Les tests portent sur la **logique métier des services**, là où un défaut aurait un impact fonctionnel ou de sécurité :

| Service | Méthode | Ce qui est vérifié |
|---|---|---|
| CardService | CreateCard | La carte est placée en fin de colonne (Order = max + 1) |
| CardService | UpdateCard | Le titre et la description sont mis à jour |
| CardService | DeleteCard | La carte est retirée de la base |
| CardService | MoveCard (même colonne) | Les ordres sont recalculés correctement |
| CardService | MoveCard (autre colonne) | La carte change de colonne, les ordres sont cohérents |
| ColumnService | CreateColumn | La colonne est placée en fin de tableau (Order = max + 1) |
| ColumnService | DeleteColumn | La colonne (et ses cartes en cascade) est supprimée |
| BoardService | CreateBoard | Le tableau est créé avec les colonnes issues du modèle choisi, et rattaché à son propriétaire |
| BoardService | CreateBoard (modèle inexistant) | Retourne null (aucun tableau créé) |
| BoardService | GetAllBoards | **Cloisonnement : un utilisateur ne récupère que ses propres tableaux, jamais ceux des autres** |
| BoardService | UpdateBoard | Le nom du tableau est mis à jour |
| BoardService | DeleteBoard | Le tableau est supprimé |
| TemplateService | GetTemplatesForUser | Filtre correct : modèles système (OwnerId null) + modèles de l'utilisateur, en excluant ceux des autres utilisateurs |
| TemplateService | GetTemplateById | Le modèle est retourné avec ses colonnes (ou null si inexistant) |

## Un focus sur la sécurité

Au-delà du fonctionnel, les tests couvrent le **cloisonnement des données par utilisateur** — un point sensible :

- **Tableaux** : `GetAllBoards` ne renvoie que les tableaux dont l'utilisateur est propriétaire (`OwnerId`). Un test dédié vérifie qu'un utilisateur ne voit pas les tableaux d'un autre.
- **Modèles** : `GetTemplatesForUser` ne renvoie que les modèles système (partagés) et les modèles personnels de l'utilisateur, en excluant ceux des autres.

L'identité de l'utilisateur provient toujours du token JWT (côté serveur), jamais des données envoyées par le client.

## Ce qui n'est pas couvert (et pourquoi)

- **Contrôleurs** : ils ne portent pas de logique métier (validation HTTP + délégation au service). Leur couverture relèverait de tests d'intégration, hors périmètre de ces tests unitaires.
- **Temps réel (SignalR)** : préoccupation d'infrastructure, testée manuellement (avec deux clients).
- **Accès EF pur** (ex. GetById simple) : peu de logique propre, faible valeur ajoutée d'un test unitaire.

## Organisation des fichiers

Un fichier de tests par service testé, dans le projet `Kanban.Tests` :

```
Kanban.Tests/
├── TestDbContextFactory.cs      (helper commun : création d'une base InMemory isolée)
├── CardServiceTests.cs
├── ColumnServiceTests.cs
├── BoardServiceTests.cs
└── TemplateServiceTests.cs
```

## Exécution

```bash
dotnet test Kanban.Tests/Kanban.Tests.csproj
```

Tous les tests doivent passer au vert. En cas d'échec, le message xUnit indique la valeur attendue et la valeur obtenue, permettant d'identifier la régression.

> Note : lancer les tests nécessite que l'API ne soit pas déjà en cours d'exécution (le fichier exécutable serait verrouillé). Arrêter l'API (`Ctrl+C`) avant de lancer les tests.

## Principe directeur

Un bon test vérifie l'**effet réel** de l'opération (l'état de la base après action), pas seulement la valeur de retour de la méthode. Un test doit pouvoir **échouer** si la logique métier est cassée — c'est sa raison d'être.