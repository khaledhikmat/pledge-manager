
## Dapr Init

For local self-hosted:
```
dapr init
```

For kubernetes support:

```
dapr init --kubernetes --wait
```

## Create Solution

```
mkdir pledge-manager
cd pledge-manager
mkdir src
cd src
```

## Create Projects

```
dotnet new classlib -o shared
dotnet new webapi -o processors
dotnet new webapi -o api
dotnet new console -o simulator
```

## Nuget Packages

```
dotnet add package Dapr.AspNetCore
dotnet add package Dapr.Actors
dotnet add package Dapr.Actors.AspNetCore
```

```
dotnet add processors/processors.csproj reference shared/shared.csproj
dotnet add api/api.csproj reference shared/shared.csproj
```

## Services

| Microservice | Application Port | Dapr sidecar HTTP port | Dapr sidecar gRPC port |
| --- | --- | --- | --- |
| processors | 6000 | 3600 | 60000 |
| api | 6001 | 3601 | 60001 |

## Dashboard

Runs on port 8080:

```
dapr dashboard 
```

## Redis cli

Access Docker CLI:

```
docker exec -it dapr_redis redis-cli
```

- Clean Redis: 
```
FLUSHALL
```

- Make sure:
```
KEYS *
```

- Get key value:
```
HGET statestore||CAMP-00001 data
```

If running in k8s, do `docker ps` to discover the container name of the REDIS running in K8s. and then do the above docker command:

```bash
docker exec -it k8s_redis_redis-75db659ddc-q6jfn_dapr-storemanager_b116ad62-7b4e-4a75-968f-39f84ce8a16c_0 redis-cli
```

## Run Locally

*Start Processors:*

```bash
cd processors
bash ./start-selfhosted.sh
```

*Start API:*

```bash
cd api
bash ./start-selfhosted.sh
```

*Query State:*

```
curl -X POST http://localhost:3601/v1.0-alpha1/state/statestore/query?metadata.contentType=application/json -H "Content-Type: application/json" -d '{}'
```

## Docker

Docker files have to be at the root because they need to include the shared library.

```bash
docker image build -t pledge-manager/processors:1.0 . -f Dockerfile-processors
docker container run -it  -p 6000:6000 pledge-manager/processors:1.0
docker inspect <container-id>
```

Make sure the ASP.Net core project run using `0.0.0.0` as opposed to `localhost`. Otherwise the error is `socket hang` while running in Docker.

You can also get into the Docker container:

```
docker run  -it pledge-manager/processors:1.0 /bin/bash
```

To push to Docker hub:

```
docker login
docker push khaledhikmat/image-name:tag
```
## Kubernetes

```bash
bash ./start.sh
kubectl get pods -n dapr-pledgemanager
bash ./stop.sh
```

