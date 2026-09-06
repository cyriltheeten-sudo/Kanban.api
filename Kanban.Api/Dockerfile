# Étape 1 — construire l'appli avec le SDK .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Étape 2 — image légère qui fait juste tourner l'appli
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "Kanban.Api.dll"]