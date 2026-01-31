# Script para criar cluster k3d (Kubernetes dentro do Docker)
# Requer: Docker Desktop, k3d instalado (choco install k3d ou https://k3d.io/)

$clusterName = "outbox-poc"
$apiPort = 28080

# Remove cluster existente se houver
k3d cluster delete $clusterName 2>$null

# Cria cluster com porta 28080 no host mapeada para a API (80 no loadbalancer)
k3d cluster create $clusterName `
  --agents 1 `
  --servers 1 `
  --port "${apiPort}:80@loadbalancer" `
  --port "30443:443@loadbalancer"

Write-Host "Cluster $clusterName criado. API sera exposta em http://localhost:$apiPort apos o deploy."
