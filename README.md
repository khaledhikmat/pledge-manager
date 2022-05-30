Pledge manager is ....

## Source Code

TBA

## Macro Architecture

### Backend

![Backend Architecture](./media/pledge-manager-arch-slides.002.png)

### Frontend

![Frontend Architecture](./media/pledge-manager-arch-slides.003.png)

### Deployment Options

![Deployment Options](./media/pledge-manager-arch-slides.004.png)

## Microservices

The following are proposed microservices:
- Frontend (hosted in Azure Static Web Apps):
    - Blazor Web Assesmbly (SPA)
    - AZ Functions as APIs to handle/circumvent CORS
- Backend (hosted in Azure Container Apps):
    - Backend API (ACA App1) 
    - 3 Pub/Sub: Campaigns, Commands, Pledges (Redis)
    - Processors (ACA App2)
    - Campaign Actors (ACA App2)
    - 1 Pub/Sub: Externalizations (Redis)
    - Externalization State Update Processor (ACA App3)
    - Externalization gRPC Processor (ACA App3)
    - Externalization Redis Streams Processor (ACA App3)

## Azure Services

- SignalR: Real-time updates to Blazor App clients
- Redis: State Store and Pub/Sub
- Azure Container Apps: hosts all backend containers
- Azure Static Web Apps: hosts the front-end application

## Run Locally

### Pre-requisites

- [Docker](https://docs.docker.com/get-docker/)
- [DAPR](https://docs.dapr.io/getting-started/)
- [SWA CLI](https://github.com/Azure/static-web-apps-cli) (Static Web Apps)

### Redis

*Using a terminal session, access Redis Docker CLI*

```
docker exec -it dapr_redis redis-cli
```

*Clean Redis* 

```
FLUSHALL
```

*Make sure they are all flushed*

```
KEYS *
```

*To get a key value*

```
HGET statestore||CAMP-00001 data
```

### Backend

*Using a terminal session, start the backend processors*

```
cd src/backend/processors
bash ./start-selfhosted.sh
```

*Using a terminal session, start the backend api*

```
cd src/backend/api
bash ./start-selfhosted.sh
```

*Using Postman, Seed the database*

Use the [Postman collection](./src/backend/postman/pledge-manager-collection.json):
- Create an institution
- Create an associated campaign (for now please use CAMP-00001 as the identifier :-))
- Submit several pledge for several donors 

### Frontend

*Using a terminal session, start the frontend client and api using SWA*

```
cd src/frontend
- swa start http://localhost:5000 --run "dotnet run --project client/client.csproj" --api-location api
```

*Using a browser, access the frontend in SWA Emulator*
[http://localhost:4280](http://localhost:4280)
