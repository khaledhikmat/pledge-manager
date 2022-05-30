## Pledge Manager Frontend

This is built using [https://docs.microsoft.com/en-us/azure/static-web-apps/deploy-blazor](https://docs.microsoft.com/en-us/azure/static-web-apps/deploy-blazor)

## Deploy to Azure Static Web Apps

This application can be deployed to [Azure Static Web Apps](https://docs.microsoft.com/azure/static-web-apps), to learn how, check out [our quickstart guide](https://aka.ms/blazor-swa/quickstart).

Several Errors:
- Missing local.setting.json causes language not found!
- Multiple dotnet runtime and sdks cause problems: _GenerateFunctionsExtensionsMetadataPostBuild
    - https://github.com/dotnet/cli-lab/releases
    - https://github.com/Azure/azure-functions-core-tools/issues/2876
- Ambigius Match Found: https://adamstorr.azurewebsites.net/blog/ambiguous-match-found-in-azure-functions-project
- func start to start locally
- swa start http://localhost:5000 --run "dotnet run --project client/client.csproj" --api-location api
- Routing: https://www.c-sharpcorner.com/article/routing-in-azure-function/
- VS Code got into a problem where it could not resolve any application mode.....close VS Code and restart....make sure you close VS Code completely (all instances).
- I opted to only use the isolated mode.
- Configuration goes in local.setting.json ....but when in Azure, they must be added to application configuration.