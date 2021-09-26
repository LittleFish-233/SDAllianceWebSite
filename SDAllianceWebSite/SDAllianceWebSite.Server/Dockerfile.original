#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["SDAllianceWebSite/SDAllianceWebSite.Server/SDAllianceWebSite.Server.csproj", "SDAllianceWebSite/SDAllianceWebSite.Server/"]
COPY ["SDAllianceWebSite/SDAllianceWebSite.Shared/SDAllianceWebSite.Shared.csproj", "SDAllianceWebSite/SDAllianceWebSite.Shared/"]
RUN dotnet restore "SDAllianceWebSite/SDAllianceWebSite.Server/SDAllianceWebSite.Server.csproj"
COPY . .
WORKDIR "/src/SDAllianceWebSite/SDAllianceWebSite.Server"
RUN dotnet build "SDAllianceWebSite.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SDAllianceWebSite.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SDAllianceWebSite.Server.dll"]
