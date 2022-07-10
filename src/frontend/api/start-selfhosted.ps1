dapr run `
    --app-id pledgemanager-functions `
    --app-port 6002 `
    --dapr-http-port 3602 `
    --dapr-grpc-port 60002 `
    --config ../../deploy/dapr/config/config.yaml `
    --components-path ../../deploy/dapr/components `
    dotnet run