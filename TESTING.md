# Testing strategy — Gemboard API

## Goal

This document describes the testing strategy of the Gemboard API: **unit tests** on the service layer (business logic — create, update, delete, move, filter) and **integration tests** on the controller layer (real HTTP pipeline, cross-user authorization), which together verify **per-user data isolation** (object-level authorization).

## Approach

- **Unit tests**: on the service layer (`CardService`, `ColumnService`, `BoardService`, `TemplateService`), with Entity Framework Core InMemory. Each test runs against an isolated database (unique name via `Guid`), so no test can influence another.
- **Integration tests**: via `WebApplicationFactory<Program>` — a real HTTP pipeline (routing, JWT authentication, `[Authorize]`, controllers) runs against an isolated InMemory database, with real tokens obtained from the `/api/auth/login` endpoint. See "Integration tests" below.
- **Framework**: xUnit for both kinds of tests.
- **Structure**: each test follows the **AAA** pattern (Arrange / Act / Assert) — set up the data, run the action under test, verify the observable result.

## Scope

The tests target the **services' business logic**, where a defect would have a functional or security impact:

| Service         | Method                            | What is verified                                                                                |
| ---------------- | ----------------------------------- | --------------------------------------------------------------------------------------------------- |
| CardService     | CreateCard                         | The card is placed at the end of the column (Order = max + 1)                                       |
| CardService     | UpdateCard                         | The title is updated                                                                                |
| CardService     | DeleteCard                         | The card is removed from the database                                                               |
| CardService     | MoveCard (same column)             | Orders are recalculated correctly                                                                   |
| CardService     | MoveCard (another column)          | The card changes column, orders stay consistent                                                     |
| CardService     | UpsertEntry (new entry)            | An entry is created for the card+column pair                                                        |
| CardService     | UpsertEntry (existing entry)       | The content is updated, without creating a duplicate                                                |
| CardService     | UpsertEntry (different columns)    | Two distinct steps of the same card produce two entries                                             |
| ColumnService   | CreateColumn                       | The column is placed at the end of the board (Order = max + 1)                                      |
| ColumnService   | DeleteColumn                       | The column (and its cards, in cascade) is deleted                                                   |
| BoardService    | CreateBoard                        | The board is created with the chosen template's columns, and attached to its owner                  |
| BoardService    | CreateBoard (unknown template)     | Returns null (no board created)                                                                     |
| BoardService    | GetAllBoards                       | Isolation: a user only retrieves their own boards                                                   |
| BoardService    | GetBoardById                       | The board is returned only if the user owns it (null otherwise)                                     |
| BoardService    | UpdateBoard                        | Renaming allowed to the owner only; refused otherwise (board unchanged)                              |
| BoardService    | DeleteBoard                        | Deletion allowed to the owner only; refused otherwise (board kept)                                   |
| BoardService    | IsBoardOwnedBy                     | Returns true if the board belongs to the user, false otherwise (building block of the card/column guard) |
| TemplateService | GetTemplatesForUser                | Correct filtering: system templates + the user's own, excluding other users'                        |
| TemplateService | GetTemplateById                    | The template is returned with its columns (or null if it doesn't exist)                              |

## A focus on security

Beyond functional correctness, the tests cover **object-level authorization** — a sensitive area:

- **Boards**: not only is the list filtered (`GetAllBoards`), but **every individual action** (open, update, delete a board by id) checks that the user owns it. Dedicated tests confirm a user can neither view, update, nor delete another user's board.
- **Cards and columns**: ownership is enforced at the controller level (centralized `BoardService.IsBoardOwnedBy` method). Every card action (create, update, delete, move, write an entry) and column action (create, delete) resolves back to its board and checks that the user owns it **before** acting; otherwise it returns `404 Not Found` (without revealing whether the resource exists). The two most sensitive cases — moving a card into another user's column, and writing an entry onto another user's card — are covered by **automated integration tests** (see below); the rest has been validated manually.
- **Templates**: `GetTemplatesForUser` only returns system templates (shared) and the user's own personal templates, excluding other users'.

The user's identity always comes from the JWT (server-side), never from client-supplied data.

## Integration tests (controllers)

`AuthorizationIntegrationTests.cs` runs the real HTTP pipeline (via `CustomWebApplicationFactory`, a `WebApplicationFactory<Program>` pointing at an isolated InMemory database) to verify authorization **between two distinct users**, with real JWTs obtained through `/api/auth/login`:

| Scenario                                                   | What is verified                                                    |
| ------------------------------------------------------------ | ----------------------------------------------------------------------- |
| Request without a token                                      | `401 Unauthorized`                                                      |
| Move your card into another user's column                    | `404 Not Found`, the card doesn't move in the database                 |
| Move your card into a column of your own board               | `204 No Content`, the card is actually moved                           |
| Write an entry on another user's card                        | `404 Not Found`, no entry is created in the database                   |
| Write an entry on your own card                              | `204 No Content`, the entry is created with the expected content       |

The first two cross-user scenarios are **regression tests** for two fixed IDOR vulnerabilities: they fail (unmet `404` assertion) if the authorization checks in `CardsController.Move`/`UpsertEntry` are removed — verified manually by temporarily disabling the fix.

## What isn't covered (and why)

- **Ownership guard on other card/column actions**: `Create`/`Update`/`Delete` for cards, and `Create`/`Delete` for columns, are protected by the same mechanism (`IsBoardOwnedBy`) but are only covered manually, not by dedicated integration tests. To be extended following the same pattern as `AuthorizationIntegrationTests`.
- **Deleting an entry on empty content**: the "clearing a step deletes its entry" behavior isn't implemented yet (upsert currently saves empty content as-is); it will be covered once that rule is added.
- **Entry cascading**: the InMemory database used in tests doesn't enforce foreign keys or cascade rules (`Cascade` on the card side, `Restrict` on the column side). These behaviors belong to the real PostgreSQL engine and are validated at the migration level, not by unit tests.
- **Real-time (SignalR)**: an infrastructure concern, tested manually (with two clients).

## File organization

```
Kanban.Tests/
├── TestDbContextFactory.cs            (unit helper: isolated InMemory database)
├── CardServiceTests.cs
├── ColumnServiceTests.cs
├── BoardServiceTests.cs
├── TemplateServiceTests.cs
├── CustomWebApplicationFactory.cs     (integration helper: HTTP pipeline + isolated InMemory database)
├── AuthTestHelper.cs                  (integration helper: seed a user + real login)
└── AuthorizationIntegrationTests.cs
```

## Running the tests

```bash
dotnet test Kanban.Tests/Kanban.Tests.csproj
```

All tests should pass. On failure, the xUnit message shows the expected and actual values.

> Note: running the tests requires that the API isn't already running (the executable file would be locked). Stop the API (`Ctrl+C`) before running the tests.

## Guiding principle

A good test verifies the **actual effect** of the operation (the state of the database after the action), not just the return value. A test should be able to **fail** if the business logic or a security rule is broken — that's its reason for existing.
