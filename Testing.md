# Stratégie de tests — Gemboard API

## Objectif

Ce document décrit la stratégie de tests unitaires de la couche service de l'API Gemboard. Les tests vérifient la logique métier (création, modification, suppression, déplacement, filtrage) indépendamment de l'infrastructure (base de données réelle, HTTP, temps réel).

## Approche

- **Type de tests** : tests unitaires sur la couche service (`CardService`, `ColumnService`, `BoardService`, `TemplateService`).
- **Framework** : xUnit.
- **Base de données** : Entity Framework Core InMemory. Chaque test s'exécute sur une base isolée (nom unique via `Guid`), ce qui garantit qu'aucun test n'influence un autre.
- **Structure** : chaque test suit le motif **AAA** (Arrange / Act / Assert) — préparer les données, exécuter l'action testée, vérifier le résultat observable.

## Périmètre

Les tests portent sur la **logique métier des services**, là où un défaut aurait un impact fonctionnel :

| Service | Méthode | Ce qui est vérifié |
|---|---|---|
| CardService | CreateCard | La carte est placée en fin de colonne (Order = max + 1) |
| CardService | UpdateCard | Le titre et la description sont mis à jour |
| CardService | DeleteCard | La carte est retirée de la base |
| CardService | MoveCard (même colonne) | Les ordres sont recalculés correctement |
| CardService | MoveCard (autre colonne) | La carte change de colonne, les ordres sont cohérents |
| ColumnService | CreateColumn | La colonne est placée en fin de tableau (Order = max + 1) |
| ColumnService | DeleteColumn | La colonne (et ses cartes en cascade) est supprimée |
| BoardService | CreateBoard | Le tableau est créé avec les colonnes issues du modèle choisi |
| BoardService | CreateBoard (modèle inexistant) | Retourne null (aucun tableau créé) |
| BoardService | UpdateBoard | Le nom du tableau est mis à jour |
| BoardService | DeleteBoard | Le tableau est supprimé |
| TemplateService | GetTemplatesForUser | Filtre correct : modèles système (OwnerId null) + modèles de l'utilisateur, en excluant ceux des autres utilisateurs |

## Ce qui n'est pas couvert (et pourquoi)

- **Contrôleurs** : ils ne portent pas de logique métier (validation HTTP + délégation au service). Leur couverture relèverait de tests d'intégration, hors périmètre de ces tests unitaires.
- **Temps réel (SignalR)** : préoccupation d'infrastructure, testée manuellement.
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

Les anciens fichiers `MoveCardTests.cs` et `DeleteCardTest.cs` sont regroupés dans `CardServiceTests.cs` pour centraliser tous les tests du `CardService` au même endroit (ils peuvent être supprimés une fois leur contenu repris).

## Exécution

```bash
dotnet test
```

Tous les tests doivent passer au vert. En cas d'échec, le message xUnit indique la valeur attendue et la valeur obtenue, permettant d'identifier la régression.

## Principe directeur

Un bon test vérifie l'**effet réel** de l'opération (l'état de la base après action), pas seulement la valeur de retour de la méthode. Un test doit pouvoir **échouer** si la logique métier est cassée — c'est sa raison d'être.
