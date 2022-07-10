param paramLoc string
param paramAppName string

//Kube environment
param paramK8senvId string

// Container Image ref
param paramContainerImage string
param paramEnvVars array

// Networking
param paramUseExternalIngress bool = false
param paramContainerPort int
param paramContainerMaxReplicas int

//Registry
param paramIsPrivateRegistry bool = false
param paramRegistry string
param paramRegistryUsername string
@secure()
param paramRegistryPassword string

param paramTags object = {}

resource app 'Microsoft.App/containerApps@2022-03-01' = {
  name: paramAppName
  tags: paramTags
  location: paramLoc
  properties: {
    managedEnvironmentId: paramK8senvId
    configuration: {
      secrets: paramIsPrivateRegistry ? [
        {
          name: 'container-registry-password'
          value: paramRegistryPassword
        }
      ] : null     
      registries: paramIsPrivateRegistry ? [
        {
          server: paramRegistry
          username: paramRegistryUsername
          passwordSecretRef: 'container-registry-password'
        }
      ] : null
      dapr: {
        enabled: true
        appId: paramAppName
        appProtocol: 'http'
        appPort: paramContainerPort
      }
      ingress: {
        external: paramUseExternalIngress
        targetPort: paramContainerPort
      }
    }
    template: {
      containers: [
        {
          image: paramContainerImage
          name: paramAppName
          env: paramEnvVars
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: paramContainerMaxReplicas
      }
    }
  }  
}
