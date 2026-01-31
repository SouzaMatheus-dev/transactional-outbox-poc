# Executa o script create-database-and-tables.sql DENTRO do pod do SQL Server no Kubernetes.
# Nao precisa de port-forward: conecta em 127.0.0.1,1433 dentro do proprio pod.
# Requer: kubectl, namespace outbox-poc, deployment sql-server com pelo menos 1 pod Ready.
# Uso: .\scripts\run-sql-in-k8s.ps1

$ErrorActionPreference = "Stop"
# Raiz do repo: pasta acima de scripts
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$namespace = "outbox-poc"
$scriptName = "create-database-and-tables.sql"
$scriptPath = Join-Path $root "scripts\$scriptName"

if (-not (Test-Path $scriptPath)) {
  Write-Host "Arquivo nao encontrado: $scriptPath" -ForegroundColor Red
  Write-Host "Execute o script a partir da raiz do repo: cd C:\Users\HOME\Desktop\OutboxKfk" -ForegroundColor Gray
  exit 1
}

# Pega o primeiro pod sql-server Running (evita jsonpath com filtro que quebra no PowerShell)
$podLine = kubectl get pods -n $namespace -l app=sql-server --field-selector=status.phase=Running -o name 2>$null | Select-Object -First 1
if ([string]::IsNullOrEmpty($podLine)) {
  Write-Host "Nenhum pod sql-server Running no namespace $namespace. Aguarde o pod ficar 1/1." -ForegroundColor Red
  exit 1
}
$podName = $podLine -replace '^pod/',''

Write-Host "Pod Ready: $podName" -ForegroundColor Cyan
Write-Host "Copiando script para o pod..." -ForegroundColor Cyan
# No Windows, caminho com C: confunde o kubectl cp; usar caminho relativo a partir da raiz do repo
Push-Location $root
try {
  kubectl cp "scripts/$scriptName" "${namespace}/${podName}:/tmp/$scriptName"
  if ($LASTEXITCODE -ne 0) { throw "kubectl cp falhou" }
} finally {
  Pop-Location
}
if ($LASTEXITCODE -ne 0) {
  Write-Host "Falha ao copiar. Verifique se o pod esta Running." -ForegroundColor Red
  exit 1
}

# SQL Server 2022 container pode ter sqlcmd em /opt/mssql-tools18/bin ou /opt/mssql-tools/bin
$sqlcmdPaths = @("/opt/mssql-tools18/bin/sqlcmd", "/opt/mssql-tools/bin/sqlcmd")
$sqlcmdCmd = $null
foreach ($p in $sqlcmdPaths) {
  $exists = kubectl exec -n $namespace $podName -- test -f $p 2>$null
  if ($LASTEXITCODE -eq 0) {
    $sqlcmdCmd = $p
    break
  }
}

if (-not $sqlcmdCmd) {
  Write-Host "sqlcmd nao encontrado no container. Execute o script manualmente via SSMS/Azure Data Studio com port-forward 21433:1433." -ForegroundColor Yellow
  Write-Host "Ou use: kubectl exec -n $namespace $podName -- cat /tmp/$scriptName para ver o script no pod." -ForegroundColor Gray
  exit 1
}

Write-Host "Executando script no SQL (127.0.0.1,1433 dentro do pod)..." -ForegroundColor Cyan
kubectl exec -n $namespace $podName -- $sqlcmdCmd -S 127.0.0.1,1433 -U sa -P 'YourStrong!Passw0rd' -i "/tmp/$scriptName" -C
if ($LASTEXITCODE -ne 0) {
  Write-Host "Falha ao executar o script." -ForegroundColor Red
  exit 1
}

Write-Host "Script executado com sucesso." -ForegroundColor Green
Write-Host "Para visualizar no SSMS: kubectl port-forward svc/sql-server 21433:1433 -n outbox-poc e conecte em 127.0.0.1,21433" -ForegroundColor Gray
