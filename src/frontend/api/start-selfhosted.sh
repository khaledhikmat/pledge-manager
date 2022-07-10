export SIGNALR_CONN_STRING="Endpoint=https://my-service.service.signalr.net;"
export Azure__SignalR__ConnectionString="Endpoint=https://my-service.service.signalr.net;"
export ALLOWED_ORIGINS="http://localhost:8080,http://localhost:8090"
export TARGET_ENV="local"
export PRODUCT="pledgemanager"
export STATESTORE_NAME="pledgemanager-local-statestore"
export PUBSUB_NAME="pledgemanager-local-pubsub"
dapr run --app-id=pledgemanager-local-functions --app-port=6002 --dapr-http-port=3602 --dapr-grpc-port=60002 --config=../../deploy/dapr/config/config.yaml --components-path=../../deploy/dapr/components dotnet run