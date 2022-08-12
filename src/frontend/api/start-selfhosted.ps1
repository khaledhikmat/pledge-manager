# Assumes u have:
# Install-Module -Name Az
# Install-Module Az.Accounts
# Connect-AzAccount

$signalRPrimaryKey = (Get-AzSignalRKey -ResourceGroupName pledge-manager -Name pledges).PrimaryConnectionString 
$Env:SIGNALR_CONN_STRING=$signalRPrimaryKey
$Env:Azure__SignalR__ConnectionString=$signalRPrimaryKey

$Env:ALLOWED_ORIGINS="http://localhost:8080,http://localhost:8090"
$Env:TARGET_ENV="local"
$Env:PRODUCT="pledgemanager"
$Env:STATESTORE_NAME="pledgemanager-local-statestore"
$Env:PUBSUB_NAME="pledgemanager-local-pubsub"

dapr run `
    --app-id pledgemanager-local-functions `
    --app-port 6002 `
    --dapr-http-port 3602 `
    --dapr-grpc-port 60002 `
    --config ../../deploy/dapr/config/config.yaml `
    --components-path ../../deploy/dapr/components `
    dotnet run