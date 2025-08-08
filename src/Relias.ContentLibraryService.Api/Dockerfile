#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG VSS_NUGET_ACCESSTOKEN

ENV NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED true
ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS="{\"endpointCredentials\": [{\"endpoint\":\"https://pkgs.dev.azure.com/ReliasEngineering/PlatformDevelopment/_packaging/ReliasNugetPackages/nuget/v3/index.json\", \"password\":\"${VSS_NUGET_ACCESSTOKEN}\"}]}"
RUN sh -c "$(curl -fsSL https://aka.ms/install-artifacts-credprovider.sh)"

WORKDIR /src
COPY ["nuget.config", "src/Relias.ContentLibraryService.Api/"]
COPY ["src/Relias.ContentLibraryService.Api/Relias.ContentLibraryService.Api.csproj", "src/Relias.ContentLibraryService.Api/"]
#COPY ["src/Relias.ContentLibraryService.Infra/Relias.ContentLibraryService.Infra.csproj", "src/Relias.ContentLibraryService.Infra/"]
#COPY ["src/Relias.ContentLibraryService.Common/Relias.ContentLibraryService.Common.csproj", "src/Relias.ContentLibraryService.Common/"]
RUN dotnet restore "src/Relias.ContentLibraryService.Api/Relias.ContentLibraryService.Api.csproj"
COPY . .
WORKDIR "/src/src/Relias.ContentLibraryService.Api"
RUN dotnet build "Relias.ContentLibraryService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Relias.ContentLibraryService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER $APP_UID
ENTRYPOINT ["dotnet", "Relias.ContentLibraryService.Api.dll"]
