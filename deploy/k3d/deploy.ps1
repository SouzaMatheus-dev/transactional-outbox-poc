# Build das imagens e carregamento no cluster k3d
# Executar a partir da raiz do repo apos o cluster e a infra (SQL + Kafka) estarem Ready.
# Requer: cluster outbox-poc criado, banco POC criado (.\scripts\run-sql-in-k8s.ps1)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
Set-Location $root

Write-Host "Build das imagens..." -ForegroundColor Cyan
docker build -t poc-api:latest -f docker/POC.Api/Dockerfile .
docker build -t poc-worker-dispatcher:latest -f docker/POC.Worker.Dispatcher/Dockerfile .
docker build -t poc-worker-consumer:latest -f docker/POC.Worker.Consumer/Dockerfile .

Write-Host "Importando imagens no cluster outbox-poc..." -ForegroundColor Cyan
k3d image import poc-api:latest poc-worker-dispatcher:latest poc-worker-consumer:latest -c outbox-poc

Write-Host "Imagens importadas. Aplique API e Workers:" -ForegroundColor Green
Write-Host "  kubectl apply -f deploy/k8s/api.yaml -f deploy/k8s/worker-dispatcher.yaml -f deploy/k8s/worker-consumer.yaml"
Write-Host "API (loadbalancer): http://localhost:28080  Swagger: http://localhost:28080/swagger"
