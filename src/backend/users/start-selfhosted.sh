export SIGNALR_CONN_STRING="Endpoint=https://my-service.service.signalr.net;"
export Azure__SignalR__ConnectionString="Endpoint=https://my-service.service.signalr.net;"
export TARGET_ENV="local"
export PRODUCT="pledgemanager"
export STATESTORE_NAME="pledgemanager-local-statestore"
export PUBSUB_NAME="pledgemanager-local-pubsub"
dapr run --app-id=pledgemanager-local-users --app-port=6001 --dapr-http-port=3601 --dapr-grpc-port=60001 --config=../../deploy/dapr/config/config.yaml --components-path=../../deploy/dapr/components dotnet run