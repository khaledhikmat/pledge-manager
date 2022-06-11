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

