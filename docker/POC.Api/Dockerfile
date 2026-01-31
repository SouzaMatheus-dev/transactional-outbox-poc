# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/POC.Api/POC.Api.csproj", "POC.Api/"]
COPY ["src/POC.Application/POC.Application.csproj", "POC.Application/"]
COPY ["src/POC.Domain/POC.Domain.csproj", "POC.Domain/"]
COPY ["src/POC.Infra/POC.Infra.csproj", "POC.Infra/"]
RUN dotnet restore "POC.Api/POC.Api.csproj"
COPY src/ .
WORKDIR /src/POC.Api
RUN dotnet build "POC.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "POC.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "POC.Api.dll"]
