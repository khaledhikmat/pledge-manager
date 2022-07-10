## Pledge Manager Frontend core

### Registration

- Organizers pre-register using some form with a primary phone number and organizer type.
- Donors need to register using their phone number. The system auto-creates users with a donor type.
- Anytime a user (organizer or donor) need to use the system must verify their phone number. Once verfied, the system allows 1 hour of usage
- The system provides two endpoints: 
    - Register identity: creates a donor user if one does not exist and sends a verification code to SMS or Email
    - Verify identity: validates identity and allows access for 1 hour. The front-end stores a cookie. 

### Organizers

- Lookup campaigns that belong to their institutions
- Observe one
- Update Profile
- Launch public view
- Send Pledge to public view
- Approve/Reject pledges
- Update behavior

### Donors

- Search campaigns across instituions
- Observe one
- Donate/Pledge
- Complete Profile
- Register/Validate
- Match
- Anonymous

## Functions SignalR

[https://docs.microsoft.com/en-us/azure/azure-signalr/concept-connection-string](https://docs.microsoft.com/en-us/azure/azure-signalr/concept-connection-string)

## Notes:

### SignalR

- Locally, using a different SignalR service than ACA.

### Blazor:
- Blazor docker image needs to be aware of the remote function API url. So I used configuration for that. 
- The Docker image is built with remote settings.
- The ACA Functions APP always allows `localhost:8080` to allow a local docker to connect to it.
- Also....the remote function API needs to be aware of the blazor base address so it can allow CORS. This brings a challenge to deploying evenrything at once due to circular dependency.
- Also....need to investigate Static Web Apps (SWA) with ACA backend instead of Azuer Functions....it is promising but requires that we update our free SKU to a standard SKU which will cost about $100/year as shown below.

### Wix:
- I was able to install SignalR NPM package.
- Using their backend code, Wix does not have CORS problems.
- Need to test SignalR to make sure. 
- Widgets are HTML based

### PowerPages:

- Unable to login.....need a work email address.

### SWA with ACA

- Need to investigate. [https://docs.microsoft.com/en-us/azure/static-web-apps/apis-container-apps](https://docs.microsoft.com/en-us/azure/static-web-apps/apis-container-apps)




