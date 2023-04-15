Pledge manager is a real-time funding platform. Please visit the project's [Wiki](https://github.com/khaledhikmat/pledge-manager/wiki) for technical documentation.

Changes:
- Use Vuw JS instead of Blazor. This allows us to be more versatile and flexible in front-end which is our major weakness so we can delegate this to anyone else.
- Use Pusher instead of SignalR for Pub/Sub messages. I think Pusher provides an easier integration and complete solution with fixed pricing.
- Continue to use Cosmos for domain storage
- Upgrade to .NET 7 or 8
- Upgrade DAPR to latest
- Upgrade Bicep to latest
