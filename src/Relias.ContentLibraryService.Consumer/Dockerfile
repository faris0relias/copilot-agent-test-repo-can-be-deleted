#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

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
COPY ["nuget.config", "src/Relias.ContentLibraryService.Consumer/"]
COPY ["src/Relias.ContentLibraryService.Consumer/Relias.ContentLibraryService.Consumer.csproj", "src/Relias.ContentLibraryService.Consumer/"]
COPY ["src/Relias.ContentLibraryService.Common/Relias.ContentLibraryService.Common.csproj", "src/Relias.ContentLibraryService.Common/"]
RUN dotnet restore "src/Relias.ContentLibraryService.Consumer/Relias.ContentLibraryService.Consumer.csproj"
COPY . .
WORKDIR "/src/src/Relias.ContentLibraryService.Consumer"
RUN dotnet build "Relias.ContentLibraryService.Consumer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Relias.ContentLibraryService.Consumer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Relias.ContentLibraryService.Consumer.dll"]
