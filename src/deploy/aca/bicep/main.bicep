targetScope = 'subscription'

param paramLoc string = deployment().location
param paramEnvironment object

param paramApps array 
param currentDate string = utcNow('dd MMM yyyy')

var tagValues = {
  createdBy: 'Github Action'
  deploymentDate: currentDate
  environment: paramEnvironment.name
  product: paramEnvironment.product
}

resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: '${paramEnvironment.product}-${paramEnvironment.name}-rg'
  location: paramLoc
  tags: tagValues
}

module appEnvironment 'environment.bicep' = {
  scope: rg
  name: 'environment'
  params: {
    paramEnvironment: paramEnvironment
    paramLoc: rg.location
    paramScopes: [for app in paramApps: '${paramEnvironment.product}-${paramEnvironment.name}-${app.shortName}']
    paramTags: tagValues
  }
}

module apps 'app.bicep' = [for app in paramApps: {
  scope: rg
  name: '${app.shortName}-app'
  params: {
    paramLoc: rg.location
    paramTags: tagValues
    paramK8senvId: appEnvironment.outputs.environmentId
    paramAppName: '${paramEnvironment.product}-${paramEnvironment.name}-${app.shortName}'
    paramContainerImage: app.image
    paramEnvVars: [
      {
        name: 'TARGET_ENV'
        value: paramEnvironment.name
      }
      {
        name: 'PRODUCT'
        value: paramEnvironment.product
      }
      {
        name: 'SIGNALR_CONN_STRING'
        value: appEnvironment.outputs.singalRConnectionString
      }
      {
        name: 'Azure__SignalR__ConnectionString'
        value: appEnvironment.outputs.singalRConnectionString
      }
      {
        name: 'ALLOWED_ORIGINS'
        value: 'https://blazor-app-domain.aca.com,http://localhost:8080'
      }
      {
        name: 'STATESTORE_NAME'
        value: appEnvironment.outputs.stateStoreName
      }
      {
        name: 'PUBSUB_NAME'
        value: appEnvironment.outputs.pubsubName
      }
    ]
    paramContainerPort: app.port
    paramContainerMaxReplicas: app.maxReplicas
    paramUseExternalIngress: app.isExternalIngress
    paramIsPrivateRegistry: app.isPrivateRegistry
    paramRegistry: app.registryName
    paramRegistryUsername: app.registryUserName
    paramRegistryPassword: app.registryPassword
  }
}]
