# Guia dinamico: portas, port-forwards, URLs e script da base
# Executar na raiz do repo: .\scripts\access-guide.ps1
# Portas e namespace: deploy/k3d/config.ps1 (fonte unica)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$configPath = Join-Path $root 'deploy\k3d\config.ps1'
if (Test-Path $configPath) {
  . $configPath
} else {
  $namespace = "outbox-poc"; $apiPort = 28080; $sqlHostPort = 21433
  $kafkaPort = 9092; $kafkaUiPort = 8081; $dashboardPort = 8443
}

Write-Host ""
Write-Host "=== PORTAS E ACESSO (k8s outbox-poc) ===" -ForegroundColor Cyan
Write-Host "Raiz do repo: $root" -ForegroundColor Gray
Write-Host ""

Write-Host "--- 1) PORT-FORWARDS (deixe cada um em um terminal) ---" -ForegroundColor Yellow
Write-Host "SQL Server (SSMS / script no host):"
Write-Host "  kubectl port-forward svc/sql-server ${sqlHostPort}:1433 -n $namespace"
Write-Host ""
Write-Host "API (Swagger no navegador - loadbalancer ja expoe $apiPort; so precisa se quiser 8080):"
Write-Host "  kubectl port-forward svc/poc-api 8080:80 -n $namespace"
Write-Host ""
Write-Host "Kafka (apps no host):"
Write-Host "  kubectl port-forward svc/kafka ${kafkaPort}:9092 -n $namespace"
Write-Host ""
Write-Host "Kafka UI:"
Write-Host "  kubectl port-forward svc/kafka-ui ${kafkaUiPort}:8080 -n $namespace"
Write-Host ""
Write-Host "Kubernetes Dashboard:"
Write-Host "  kubectl port-forward -n kubernetes-dashboard svc/kubernetes-dashboard ${dashboardPort}:443"
Write-Host ""

Write-Host "--- 1.1) TOKEN DO KUBERNETES DASHBOARD (para login) ---" -ForegroundColor Yellow
Write-Host "  Gerar token (cole na tela de login do Dashboard, opcao Token):"
Write-Host "  kubectl -n kubernetes-dashboard create token admin-user"
Write-Host ""

Write-Host "--- 2) CRIAR BANCO POC (uma vez apos SQL Ready) ---" -ForegroundColor Yellow
Write-Host "  cd `"$root`""
Write-Host "  .\scripts\run-sql-in-k8s.ps1"
Write-Host ""

Write-Host "--- 3) URLS NO NAVEGADOR ---" -ForegroundColor Yellow
Write-Host "  API:        http://localhost:$apiPort"
Write-Host "  Swagger:    http://localhost:$apiPort/swagger"
Write-Host "  Health:     http://localhost:$apiPort/health"
Write-Host "  Kafka UI:   http://localhost:${kafkaUiPort}  (com port-forward ativo)"
Write-Host "  K8s Dashboard: https://localhost:${dashboardPort}  (port-forward ativo; aceite o certificado; login com Token gerado acima)"
Write-Host ""

Write-Host "--- 4) SSMS / AZURE DATA STUDIO ---" -ForegroundColor Yellow
Write-Host "  Servidor: 127.0.0.1,$sqlHostPort"
Write-Host "  Login: sa  |  Senha: YourStrong!Passw0rd"
Write-Host "  (Deixe o port-forward do SQL ativo.)"
Write-Host ""

Write-Host "--- 5) sqlcmd NO HOST (opcional, com port-forward SQL ativo) ---" -ForegroundColor Yellow
Write-Host "  cd `"$root`""
Write-Host "  sqlcmd -S 127.0.0.1,$sqlHostPort -U sa -P YourStrong!Passw0rd -i scripts/create-database-and-tables.sql -C"
Write-Host ""
