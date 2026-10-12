FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /Conexao_Solidaria_REPO

COPY ["ConexaoSolidaria.slnx", "./"]
COPY ["ApplicationServices/ApplicationServices.csproj", "ApplicationServices/"]
COPY ["ConexaoSolidaria.API/ConexaoSolidaria.API.csproj", "ConexaoSolidaria.API/"]
COPY ["ConexaoSolidaria.Tests/ConexaoSolidaria.Tests.csproj", "ConexaoSolidaria.Tests/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Models/Models.csproj", "Models/"]

RUN dotnet restore "ConexaoSolidaria.slnx"

COPY .. .

WORKDIR "/Conexao_Solidaria_REPO/ConexaoSolidaria.API"
RUN dotnet build "ConexaoSolidaria.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/Conexao_Solidaria_REPO/ConexaoSolidaria.API"
RUN dotnet publish "ConexaoSolidaria.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConexaoSolidaria.API.dll"]