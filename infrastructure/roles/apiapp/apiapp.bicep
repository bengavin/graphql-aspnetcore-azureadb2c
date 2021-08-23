@description('The stage (or environment) that the resource is a part of')
param stage string

@description('The application that the resource(s) are a part of')
param application string

@description('The region the resource(s) should be deployed to')
param region string

@description('Should the API be hosted in a Linux environment?')
param isLinux bool

var planKind = isLinux ? 'linux' : 'app'

resource servicePlan 'Microsoft.Web/serverfarms@2020-12-01' = {
  name: 'plan-${stage}-${application}'
  location: region
  kind: planKind
  properties: {
    reserved: true
  }
  sku: {
    name: 'F1'
    capacity: 1
  }
  tags: {
    environment: stage
    application: application
  }
}

var appKind = isLinux ? 'app,linux' : 'app'

resource apiApp 'Microsoft.Web/sites@2018-11-01' = {
  name: 'app-${stage}-${application}-api'
  location: region
  kind: appKind
  properties: {
    serverFarmId: servicePlan.id
    enabled: true
    httpsOnly: false
    siteConfig: {
      numberOfWorkers: 1
      linuxFxVersion: 'DOTNETCORE|5.0'
      windowsFxVersion: 'DOTNETCORE|5.0'
      appSettings: [
        {
          name: 'AzureB2C-Demo-API__ClientId'
          value: '@Microsoft.KeyVault(https://kv-${stage}-${application}.vault.azure.net/secrets/ApiApp-AzureB2C-Demo-API-ClientId)'
        }
        {
          name: 'AzureB2C-Demo-UI__ClientId'
          value: '@Microsoft.KeyVault(https://kv-${stage}-${application}.vault.azure.net/secrets/ApiApp-AzureB2C-Demo-UI-ClientId)'
        }
        {
          name: 'AzureB2C-Demo-UI__ClientSecret'
          value: '@Microsoft.KeyVault(https://kv-${stage}-${application}.vault.azure.net/secrets/ApiApp-AzureB2C-Demo-UI-ClientSecret)'
        }
      ]
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
  tags: {
    environment: stage
    application: application
  }
}

output servicePlan object = servicePlan
output apiApp object = apiApp
