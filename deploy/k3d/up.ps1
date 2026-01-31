# Sobe a POC inteira no Kubernetes (k3d) na sua maquina
# Requer: Docker Desktop, k3d, kubectl
# Executar na raiz do repo: .\deploy\k3d\up.ps1

$ErrorActionPreference = "Stop"
# Raiz do repo: sobe dois niveis a partir de deploy/k3d
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
Set-Location $root

# Portas e namespace (fonte unica: deploy/k3d/config.ps1)
. (Join-Path $PSScriptRoot 'config.ps1')

Write-Host "=== 1) Criando cluster k3d ===" -ForegroundColor Cyan
k3d cluster delete $clusterName 2>$null
k3d cluster create $clusterName `
  --agents 1 `
  --servers 1 `
  --port "${apiPort}:80@loadbalancer" `
  --port "30443:443@loadbalancer"
if ($LASTEXITCODE -ne 0) {
  Write-Host "ERRO: Falha ao criar o cluster. Se apareceu 'port is already allocated', outra aplicacao esta usando a porta $apiPort." -ForegroundColor Red
  Write-Host "Altere `$apiPort no inicio deste script (ex.: 28081) ou libere a porta e rode de novo." -ForegroundColor Yellow
  exit 1
}
Write-Host "Cluster criado.`n" -ForegroundColor Green

Write-Host "=== 2) Aplicando namespace, ConfigMap, SQL Server, Kafka ===" -ForegroundColor Cyan
kubectl apply -f deploy/k8s/namespace.yaml
kubectl apply -f deploy/k8s/configmap.yaml
kubectl apply -f deploy/k8s/sql-server.yaml
kubectl apply -f deploy/k8s/kafka.yaml
Write-Host ""

Write-Host "=== 3) Aguardando SQL Server e Kafka ficarem Ready ===" -ForegroundColor Cyan
Write-Host "SQL Server: ~2-3 min. Kafka (Apache KRaft): ate ~5 min por causa da startupProbe. Timeout 10 min." -ForegroundColor Gray
kubectl wait --for=condition=ready pod -l app=sql-server -n outbox-poc --timeout=600s
if ($LASTEXITCODE -ne 0) {
  Write-Host "AVISO: SQL Server nao ficou Ready em 10 min. Continuando." -ForegroundColor Yellow
  Write-Host "  Quando 1/1: .\scripts\run-sql-in-k8s.ps1" -ForegroundColor Gray
}
kubectl wait --for=condition=ready pod -l app=kafka -n outbox-poc --timeout=600s
if ($LASTEXITCODE -ne 0) {
  Write-Host "AVISO: Kafka nao ficou Ready em 10 min. Continuando." -ForegroundColor Yellow
}
Write-Host "Infra pronta.`n" -ForegroundColor Green

Write-Host "=== 4) CRIAR BANCO POC (obrigatorio) ===" -ForegroundColor Yellow
Write-Host "O banco e as tabelas nao sao criados automaticamente. Rode UMA das opcoes:"
Write-Host ""
Write-Host "  Opcao A (recomendado) - script dentro do pod (nao precisa de port-forward):" -ForegroundColor Cyan
Write-Host "    cd $root"
Write-Host "    .\scripts\run-sql-in-k8s.ps1"
Write-Host ""
Write-Host "  Opcao B - port-forward + sqlcmd: Terminal A: kubectl port-forward svc/sql-server 21433:1433 -n outbox-poc"
Write-Host "    Terminal B: sqlcmd -S 127.0.0.1,21433 -U sa -P YourStrong!Passw0rd -i scripts/create-database-and-tables.sql -C"
Write-Host ""
Write-Host "  Para visualizar no SSMS: use 127.0.0.1,21433 (com port-forward ativo)."
Write-Host ""
$null = Read-Host "Depois de rodar o script SQL, pressione Enter para continuar"

Write-Host "=== 5) Build das imagens e import no k3d ===" -ForegroundColor Cyan
docker build -t poc-api:latest -f docker/POC.Api/Dockerfile $root
docker build -t poc-worker-dispatcher:latest -f docker/POC.Worker.Dispatcher/Dockerfile $root
docker build -t poc-worker-consumer:latest -f docker/POC.Worker.Consumer/Dockerfile $root
k3d image import poc-api:latest poc-worker-dispatcher:latest poc-worker-consumer:latest -c $clusterName
Write-Host "Imagens importadas.`n" -ForegroundColor Green

Write-Host "=== 6) Deploy da API e dos Workers ===" -ForegroundColor Cyan
kubectl apply -f deploy/k8s/api.yaml
kubectl apply -f deploy/k8s/worker-dispatcher.yaml
kubectl apply -f deploy/k8s/worker-consumer.yaml
Write-Host ""

# Opcional: Kafka UI
$ui = Read-Host "Subir Kafka UI? (s/N)"
if ($ui -eq "s" -or $ui -eq "S") {
  kubectl apply -f deploy/k8s/kafka-ui.yaml
  Write-Host "Kafka UI aplicado. Acesse pelo port-forward: kubectl port-forward svc/kafka-ui 8081:8080 -n outbox-poc -> http://localhost:8081" -ForegroundColor Gray
}

# Opcional: Kubernetes Dashboard
$dash = Read-Host "Subir Kubernetes Dashboard? (s/N)"
if ($dash -eq "s" -or $dash -eq "S") {
  Write-Host "Aplicando Dashboard (pode demorar um pouco)..." -ForegroundColor Gray
  kubectl apply -f https://raw.githubusercontent.com/kubernetes/dashboard/v2.7.0/aio/deploy/recommended.yaml
  kubectl apply -f deploy/k8s/dashboard-admin.yaml
  Write-Host "Dashboard instalado. Para acessar:" -ForegroundColor Green
  Write-Host "  1. Em um terminal: kubectl port-forward -n kubernetes-dashboard svc/kubernetes-dashboard 8443:443"
  Write-Host "  2. Abra https://localhost:8443 no navegador (aceite o aviso de certificado)"
  Write-Host "  3. Login: escolha 'Token' e cole o token gerado com:"
  Write-Host "     kubectl -n kubernetes-dashboard create token admin-user"
  Write-Host ""
}

Write-Host ""
Write-Host "=== Pronto ===" -ForegroundColor Green
Write-Host "API: http://localhost:$apiPort"
Write-Host "Swagger: http://localhost:$apiPort/swagger"
Write-Host ""
Write-Host "Teste:"
Write-Host "  Invoke-RestMethod -Method Post -Uri 'http://localhost:$apiPort/initializations' -ContentType 'application/json' -Body '{\"externalId\":\"EXT-K8S-001\"}'"
Write-Host ""
Write-Host "Logs:"
Write-Host "  kubectl logs -f deployment/poc-api -n outbox-poc"
Write-Host "  kubectl logs -f deployment/poc-worker-dispatcher -n outbox-poc"
Write-Host "  kubectl logs -f deployment/poc-worker-consumer -n outbox-poc"
