param (
    [String]
    $baseUrl = 'http://localhost:6000',
    [String]
    $scriptPath = 'C:/Users/khaled/source/repos'
)

# Assumes u have:
# Install-Module -Name Az
# Install-Module Az.Accounts
# Connect-AzAccount

$signalRPrimaryKey = (Get-AzSignalRKey -ResourceGroupName pledge-manager -Name pledges).PrimaryConnectionString 
$Env:SIGNALR_CONN_STRING=$signalRPrimaryKey
$Env:Azure__SignalR__ConnectionString=$signalRPrimaryKey

$cosmosPrimaryKey = (Get-AzCosmosDBAccountKey -ResourceGroupName pledge-manager -Name pledge-manager-db -Type "ConnectionStrings")['Primary SQL Connection String'] 
$Env:COSMOS_CONN_STRING=$cosmosPrimaryKey

$Env:TARGET_ENV="local"
$Env:PRODUCT="pledgemanager"
$Env:STATESTORE_NAME="pledgemanager-local-statestore"
$Env:PUBSUB_NAME="pledgemanager-local-pubsub"

../../scripts/delete-cosmos-db.ps1
../../scripts/empty-redis-cache.ps1
../../scripts/ping.ps1 -baseUrl $baseUrl -scriptPath $scriptPath

dapr run `
    --app-id pledgemanager-local-campaigns `
    --app-port 6000 `
    --dapr-http-port 3600 `
    --dapr-grpc-port 60000 `
    --config ../../deploy/dapr/config/config.yaml `
    --components-path ../../deploy/dapr/components `
    dotnet run
