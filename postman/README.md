# Postman + Docker + Visual Studio (teste local)

## 1. Subir a infra no Docker

Na raiz do repositório:

```powershell
docker-compose -f docker-compose.infra.yml up -d
```

Aguarde ~30 segundos para o SQL Server ficar pronto. Sobem: SQL Server, Zookeeper, Kafka e **Kafka UI (Provectus)** em **http://localhost:8080** — use a UI para ver tópicos, consumer groups e mensagens. Não acesse http://localhost:2181 no navegador (porta do Zookeeper; causa "Len error" nos logs).

**Criar o banco POC e as tabelas** (antes de subir API/Dispatcher/Consumer): execute o script **`scripts/create-database-and-tables.sql`** (sqlcmd, SSMS ou Azure Data Studio). Exemplo: `sqlcmd -S localhost,1433 -U sa -P YourStrong!Passw0rd -i scripts/create-database-and-tables.sql -C`

## 2. Abrir a solution no Visual Studio

- Abra `OutboxKfk.sln` no **Visual Studio 2022** (ou superior).
- Restaure pacotes e compile (Ctrl+Shift+B).

## 3. Ordem de subida (importante)

O **banco POC** e as tabelas (Initializations, Outbox, ReceivedEvents) precisam existir antes do Dispatcher e do Consumer.

- **Subir a API primeiro** e aguardar ela ficar no ar (a API cria o banco POC e aplica as migrations na subida).
- Depois subir o **Dispatcher** e o **Consumer**.

Se o Dispatcher/Consumer subirem antes da API, você verá: "Cannot open database 'POC'" ou "Invalid object name 'Outbox'".

## 4. Rodar múltiplos projetos para debug

Para ver o fluxo completo (API → Outbox → Dispatcher → Kafka → Consumer) com breakpoints:

1. **Configurar múltiplos projetos de inicialização**
   - Clique com o botão direito na solution → **Configure Startup Projects**.
   - Selecione **Multiple startup projects**.
   - Para **POC.Api**, **POC.Worker.Dispatcher** e **POC.Worker.Consumer**, defina **Start**.
   - Confirme com OK.

2. **F5 (Start)**  
   Os três projetos sobem; a API fica em **http://localhost:5000** (veja `launchSettings.json`).

3. **Breakpoints**
   - API: `InitializationsController.Create` ou `CreateInitializationHandler.HandleAsync`.
   - Dispatcher: `OutboxDispatcherService.ProcessBatchAsync`.
   - Consumer: `KafkaConsumerService` no `Consume`.

## 5. Importar a collection no Postman

1. Abra o Postman.
2. **Import** → escolha o arquivo `Outbox-POC-API.postman_collection.json` (pasta `postman/`).
3. A collection **Outbox POC API** aparecerá na sidebar.

## 6. Testar

1. Na collection, use a variável **baseUrl**: `http://localhost:5000` (já é o padrão).
2. Envie **Create Initialization** (ou **Create Initialization (ID fixo)**).
3. Verifique:
   - Resposta **201 Created** com `id`, `externalId`, `createdAt`.
   - Logs no Visual Studio (Output) da API, do Dispatcher e do Consumer.
   - Se quiser, confira no SQL: tabelas `Initializations`, `Outbox` (ProcessedAt preenchido após o Dispatcher rodar), `ReceivedEvents`.

## Resumo

| Etapa              | Ferramenta        | Ação |
|--------------------|-------------------|------|
| Infra              | Docker            | `docker-compose -f docker-compose.infra.yml up -d` |
| API + Workers      | Visual Studio     | Multiple startup → F5 |
| Requisições HTTP   | Postman           | Importar collection → Create Initialization |

Sim: você consegue usar a **collection no Postman**, subir a **infra no Docker** e testar **localmente pelo Visual Studio** (com ou sem breakpoints).
