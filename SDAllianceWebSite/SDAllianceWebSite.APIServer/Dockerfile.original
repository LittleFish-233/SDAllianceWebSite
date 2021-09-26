#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM dotnetcore-with-libgdiplus:v5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["SDAllianceWebSite/SDAllianceWebSite.APIServer/SDAllianceWebSite.APIServer.csproj", "SDAllianceWebSite/SDAllianceWebSite.APIServer/"]
COPY ["SDAllianceWebSite/SDAllianceWebSite.Shared/SDAllianceWebSite.Shared.csproj", "SDAllianceWebSite/SDAllianceWebSite.Shared/"]
RUN dotnet restore "SDAllianceWebSite/SDAllianceWebSite.APIServer/SDAllianceWebSite.APIServer.csproj"
COPY . .
WORKDIR "/src/SDAllianceWebSite/SDAllianceWebSite.APIServer"
RUN dotnet build "SDAllianceWebSite.APIServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SDAllianceWebSite.APIServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SDAllianceWebSite.APIServer.dll"]
