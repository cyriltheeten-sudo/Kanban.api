# Stratégie de tests — Gemboard API

## Objectif

Ce document décrit la stratégie de tests unitaires de la couche service de l'API Gemboard. Les tests vérifient la logique métier (création, modification, suppression, déplacement, filtrage) et le **cloisonnement des données par utilisateur** (autorisation au niveau des objets), indépendamment de l'infrastructure (base de données réelle, HTTP, temps réel).

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
| CardService | UpdateCard | Le titre est mis à jour |
| CardService | DeleteCard | La carte est retirée de la base |
| CardService | MoveCard (même colonne) | Les ordres sont recalculés correctement |
| CardService | MoveCard (autre colonne) | La carte change de colonne, les ordres sont cohérents |
| CardService | UpsertEntry (nouvelle entrée) | Une entrée est créée pour le couple carte+colonne |
| CardService | UpsertEntry (entrée existante) | Le contenu est mis à jour, sans créer de doublon |
| CardService | UpsertEntry (colonnes différentes) | Deux étapes distinctes d'une même carte donnent deux entrées |
| ColumnService | CreateColumn | La colonne est placée en fin de tableau (Order = max + 1) |
| ColumnService | DeleteColumn | La colonne (et ses cartes en cascade) est supprimée |
| BoardService | CreateBoard | Le tableau est créé avec les colonnes du modèle choisi, et rattaché à son propriétaire |
| BoardService | CreateBoard (modèle inexistant) | Retourne null (aucun tableau créé) |
| BoardService | GetAllBoards | Cloisonnement : un utilisateur ne récupère que ses propres tableaux |
| BoardService | GetBoardById | Le tableau est retourné uniquement si l'utilisateur en est propriétaire (sinon null) |
| BoardService | UpdateBoard | Renommage autorisé au seul propriétaire ; refusé sinon (tableau inchangé) |
| BoardService | DeleteBoard | Suppression autorisée au seul propriétaire ; refusée sinon (tableau conservé) |
| TemplateService | GetTemplatesForUser | Filtre correct : modèles système + modèles de l'utilisateur, en excluant ceux des autres |
| TemplateService | GetTemplateById | Le modèle est retourné avec ses colonnes (ou null si inexistant) |

## Un focus sur la sécurité

Au-delà du fonctionnel, les tests couvrent l'**autorisation au niveau des objets** — un point sensible :

- **Tableaux** : non seulement la liste est filtrée (`GetAllBoards`), mais **chaque action individuelle** (ouvrir, modifier, supprimer un tableau par son id) vérifie que l'utilisateur en est propriétaire. Des tests dédiés confirment qu'un utilisateur ne peut ni voir, ni modifier, ni supprimer le tableau d'un autre.
- **Modèles** : `GetTemplatesForUser` ne renvoie que les modèles système (partagés) et les modèles personnels de l'utilisateur, en excluant ceux des autres.

L'identité de l'utilisateur provient toujours du token JWT (côté serveur), jamais des données envoyées par le client.

## Ce qui n'est pas couvert (et pourquoi)

- **Contrôleurs et authentification** : la validation HTTP, la génération du token et les attributs d'autorisation (`[Authorize]`) relèvent du pipeline ASP.NET Core. Ils sont validés manuellement ; leur couverture automatisée relèverait de **tests d'intégration** (instance de l'API en mémoire + vraies requêtes HTTP), une évolution possible.
- **Garde-fou sur cartes et colonnes** : le contrôle de propriété au niveau des cartes et colonnes est prévu (approche centralisée), non encore couvert.
- **Suppression d'entrée sur contenu vide** : le comportement « vider une étape supprime son entrée » n'est pas encore implémenté (l'upsert enregistre actuellement un contenu vide) ; il sera couvert quand cette règle sera ajoutée.
- **Cascade des entrées** : la base InMemory utilisée en test ne fait pas respecter les clés étrangères ni les règles de cascade (`Cascade` côté carte, `Restrict` côté colonne). Ces comportements relèvent du vrai moteur PostgreSQL et sont validés au niveau de la migration, pas des tests unitaires.
- **Temps réel (SignalR)** : préoccupation d'infrastructure, testée manuellement (avec deux clients).

## Organisation des fichiers

```
Kanban.Tests/
├── TestDbContextFactory.cs      (helper commun : base InMemory isolée)
├── CardServiceTests.cs
├── ColumnServiceTests.cs
├── BoardServiceTests.cs
└── TemplateServiceTests.cs
```

## Exécution

```bash
dotnet test Kanban.Tests/Kanban.Tests.csproj
```

Tous les tests doivent passer au vert. En cas d'échec, le message xUnit indique la valeur attendue et la valeur obtenue.

> Note : lancer les tests nécessite que l'API ne soit pas déjà en cours d'exécution (le fichier exécutable serait verrouillé). Arrêter l'API (`Ctrl+C`) avant de lancer les tests.

## Principe directeur

Un bon test vérifie l'**effet réel** de l'opération (l'état de la base après action), pas seulement la valeur de retour. Un test doit pouvoir **échouer** si la logique métier ou une règle de sécurité est cassée — c'est sa raison d'être.