kubectl apply \
    -f namespace.yaml \
    -f secrets.yaml \
    -f dapr-config.yaml \
    -f zipkin.yaml \
    -f redis.yaml \
    -f campaigns-store.yaml \
    -f campaigns-pubsub.yaml \
    -f campaigns.yaml \
    -f users.yaml \
    -f functions.yaml
