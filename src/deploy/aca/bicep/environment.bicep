param paramEnvironment object
param paramLoc string

param paramScopes array 
param paramTags object = {}

var signalRName = '${paramEnvironment.product}-${paramEnvironment.name}-signalr'
var redisName = '${paramEnvironment.product}-${paramEnvironment.name}-redis'
var logName = '${paramEnvironment.product}-${paramEnvironment.name}-log'
var k8sEnvName = '${paramEnvironment.product}-${paramEnvironment.name}-k8senv'
var redisStateStoreName = '${paramEnvironment.product}-${paramEnvironment.name}-statestore'
//WARNING: .NET SDK Issue in subscribing to topics forced me to hard-code the pubsun name
//var redisPubsubName = '${paramEnvironment.product}-${paramEnvironment.name}-pubsub'
var redisPubsubName = '${paramEnvironment.product}-local-pubsub'

resource signalr 'Microsoft.SignalRService/signalR@2022-02-01' = {
  name: signalRName
  tags: paramTags
  location: paramLoc
  kind: 'SignalR'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    tls: {
      clientCertEnabled: false
    }
    features: [
      {
        flag: 'ServiceMode'
        value: 'Default'
      }
      {
        flag: 'EnableConnectivityLogs'
        value: 'true'
      }
      {
        flag: 'EnableMessagingLogs'
        value: 'true'
      }
      {
        flag: 'EnableLiveTrace'
        value: 'true'
      }
    ]
  }  
  sku: {
    name: 'Standard_S1'
    tier: 'Standard'
    capacity: 10
  }
}

// Template format: https://docs.microsoft.com/en-us/azure/templates/microsoft.cache/redis?tabs=bicep
resource redis 'Microsoft.Cache/redis@2021-06-01' = if (paramEnvironment.isRedis) {
  name: redisName
  tags: paramTags
  location: paramLoc
  properties: {
    enableNonSslPort: true
    redisVersion: '6'
    sku:{
      name: 'Basic'
      family: 'C'
      capacity: 0
    }
  }  
}

resource law 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' = {
  name: logName
  tags: paramTags
  location: paramLoc
  properties: {
    retentionInDays: 30
    sku: {
      name: 'PerGB2018'
    }
  }
}

resource k8senv 'Microsoft.App/managedEnvironments@2022-03-01' = {
  name: k8sEnvName
  tags: paramTags
  location: paramLoc
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: law.properties.customerId
        sharedKey: law.listKeys().primarySharedKey
      }
    }
  }
}

resource redisStatestore 'Microsoft.App/managedEnvironments/daprComponents@2022-03-01' = if (paramEnvironment.isRedisStateStore) {
  name: redisStateStoreName
  parent: k8senv
  properties: {
    componentType: 'state.redis'
    version: 'v1'
    ignoreErrors: false
    initTimeout: '5s'
    secrets: [
      {
        name: 'rediskey'
        value: redis.listKeys().primaryKey
      }
    ]
    metadata: [
      {
        name: 'redisHost'
        value: '${redis.properties.hostName}:6379'
      }
      {
        name: 'redisPassword'
        secretRef: 'rediskey'
      }
      {
        name: 'actorStateStore'
        value: 'true'
      }
      {
        name: 'keyPrefix'
        value: 'name'
      }
    ]
    scopes: paramScopes
  }
}

resource redisPubsub 'Microsoft.App/managedEnvironments/daprComponents@2022-03-01' = if (paramEnvironment.isRedisPubsub) {
  name: redisPubsubName
  parent: k8senv
  properties: {
    componentType: 'pubsub.redis'
    version: 'v1'
    ignoreErrors: false
    initTimeout: '5s'
    secrets: [
      {
        name: 'rediskey'
        value: redis.listKeys().primaryKey
      }
    ]
    metadata: [
      {
        name: 'redisHost'
        value: '${redis.properties.hostName}:6379'
      }
      {
        name: 'redisPassword'
        secretRef: 'rediskey'
      }
      {
        name: 'consumerID'
        value: 'myGroup'
      }
      {
        name: 'enableTLS'
        value: 'false'
      }
    ]
    scopes: paramScopes
  }
}

output environmentName string = k8senv.name
output environmentId string = k8senv.id
output singalRConnectionString string = signalr.listKeys().primaryConnectionString
output stateStoreName string = redisStateStoreName
output pubsubName string = redisPubsubName

