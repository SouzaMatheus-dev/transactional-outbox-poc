FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/POC.Worker.Dispatcher/POC.Worker.Dispatcher.csproj", "POC.Worker.Dispatcher/"]
COPY ["src/POC.Application/POC.Application.csproj", "POC.Application/"]
COPY ["src/POC.Domain/POC.Domain.csproj", "POC.Domain/"]
COPY ["src/POC.Infra/POC.Infra.csproj", "POC.Infra/"]
RUN dotnet restore "POC.Worker.Dispatcher/POC.Worker.Dispatcher.csproj"
COPY src/ .
WORKDIR /src/POC.Worker.Dispatcher
RUN dotnet build "POC.Worker.Dispatcher.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "POC.Worker.Dispatcher.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "POC.Worker.Dispatcher.dll"]
