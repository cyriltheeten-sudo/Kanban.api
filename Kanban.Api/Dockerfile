# Étape 1 — construire l'appli avec le SDK .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier le csproj de l'API et restaurer ses dépendances
COPY Kanban.Api/*.csproj Kanban.Api/
RUN dotnet restore Kanban.Api/Kanban.Api.csproj

# Copier tout le reste du code de l'API et publier
COPY Kanban.Api/ Kanban.Api/
RUN dotnet publish Kanban.Api/Kanban.Api.csproj -c Release -o /app/publish

# Étape 2 — image légère qui fait juste tourner l'appli
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "Kanban.Api.dll"]