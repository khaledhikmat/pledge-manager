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

The following are proposed microservices hosted in Azure Container Apps:
- pledgemanager-frontend:
    - Blazor Web Assesmbly (SPA) Client
    - API Endpoints as Server to handle/circumvent CORS
- pledgemanager-camapigns:
    - Entities Controller
    - Processors Controller
    - Campaign Actors 
    - 3 Pub/Sub: Campaigns, Commands, Pledges (Redis)
- pledgemanager-users:
    - Users Controller
    - User Actors 

**Please note** that we considered Azure Static Web Apps (SWA) to host the frontend but there were some friction with SignalR. Since our frontend relies heavily on SignalR, we opted to go with a non-SWA site. The net effect is that the frontend (client and API) will scale together in Azure Container Apps as opposed to scaling independently had they been running in SWA.

## Azure Services

### MVP:

- SignalR: Real-time updates to Blazor App clients
- Redis: State Store and Pub/Sub
- Azure Container Apps: hosts all containers

## Run Locally

### Pre-requisites

- [Docker](https://docs.docker.com/get-docker/)
- [DAPR](https://docs.dapr.io/getting-started/)

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

*Using a terminal session, start the backend campaigns*

```
cd src/backend/campaigns
bash ./start-selfhosted.sh
```

*Using a terminal session, start the backend users*

```
cd src/backend/users
bash ./start-selfhosted.sh
```

*Using a terminal session, start the simulator to seed the database*

```
cd src/backend/simulator
bash ./start-selfhosted-create-only.sh
bash ./start-selfhosted-simulate-donors.sh
bash ./start-selfhosted-simulate-pledges-one-campaign.sh
```

*Using Postman, issue different commands as needed*

Use the [Postman collection](./src/backend/postman/pledge-manager-collection.json):
- Create an institution
- Create an associated campaign (for now please use CAMP-00001 as the identifier :-))
- Submit several pledge for several donors 

### Frontend

*Using a terminal session, start the frontend API*

```
cd src/frontend/api
- dotnet run
```

*Using a browser, access the frontend in a browser*
[https://localhost:7022](https://localhost:7022)

## Test Scenarios

**This assumes that seeding the database is seeded as above**

- Test pledges against `CAMP-00001` which is configured for auto approval mode.
- Test pledges against `CAMP-00002` which is configured for manual approval.
- Test pledges against `CAMP-00003` which is configured for hybrid.
- Test pledges against `CAMP-00004` which is configured for hybrid.
- Test restricted amounts.
- Test min/max amounts.
- Test anonymous auto approval.
- Test match.
- Test pledge approval.
- Test pledge rejection.
- Test auto-deactivation.
- Test campaign update inlcuding behavior.


