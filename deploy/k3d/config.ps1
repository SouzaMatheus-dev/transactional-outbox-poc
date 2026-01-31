# Configuracao compartilhada: portas e namespace para a POC no k3d
# Alterar aqui reflete em up.ps1 e em scripts/access-guide.ps1

$clusterName   = "outbox-poc"
$namespace     = "outbox-poc"
$apiPort       = 28080
$sqlHostPort   = 21433
$kafkaPort     = 9092
$kafkaUiPort   = 8081
$dashboardPort = 8443
