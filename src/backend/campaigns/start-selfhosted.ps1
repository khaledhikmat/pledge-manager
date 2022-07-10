dapr run `
    --app-id pledgemanager-campaigns `
    --app-port 6000 `
    --dapr-http-port 3600 `
    --dapr-grpc-port 60000 `
    --config ../../deploy/dapr/config/config.yaml `
    --components-path ../../deploy/dapr/components `
    dotnet run