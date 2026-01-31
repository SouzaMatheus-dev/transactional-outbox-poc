FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/POC.Worker.Consumer/POC.Worker.Consumer.csproj", "POC.Worker.Consumer/"]
COPY ["src/POC.Application/POC.Application.csproj", "POC.Application/"]
COPY ["src/POC.Domain/POC.Domain.csproj", "POC.Domain/"]
COPY ["src/POC.Infra/POC.Infra.csproj", "POC.Infra/"]
RUN dotnet restore "POC.Worker.Consumer/POC.Worker.Consumer.csproj"
COPY src/ .
WORKDIR /src/POC.Worker.Consumer
RUN dotnet build "POC.Worker.Consumer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "POC.Worker.Consumer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "POC.Worker.Consumer.dll"]
