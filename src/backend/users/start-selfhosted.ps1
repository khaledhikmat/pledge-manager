dapr run `
    --app-id pledgemanager-users `
    --app-port 6001 `
    --dapr-http-port 3601 `
    --dapr-grpc-port 60001 `
    --config ../../deploy/dapr/config/config.yaml `
    --components-path ../../deploy/dapr/components `
    dotnet run