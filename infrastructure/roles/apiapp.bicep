@description('The stage (or environment) that the resource is a part of')
param stage string

@description('The application that the resource(s) are a part of')
param application string

@description('The region the resource(s) should be deployed to')
param region string

@description('Should the API be hosted in a Linux environment?')
param isLinux bool

@description('The application insights instance to connect to')
param appInsightsName string

var planKind = isLinux ? 'linux' : 'app'

resource servicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
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

resource appInsights 'Microsoft.Insights/components@2020-02-02-preview' existing = {
  name: appInsightsName
}

resource apiApp 'Microsoft.Web/sites@2022-03-01' = {
  name: 'app-${stage}-${application}-api'
  location: region
  kind: appKind
  dependsOn: [
    appInsights
  ]
  properties: {
    serverFarmId: servicePlan.id
    enabled: true
    httpsOnly: false
    siteConfig: {
      numberOfWorkers: 1
      linuxFxVersion: 'DOTNETCORE|7.0'
      windowsFxVersion: 'DOTNETCORE|7.0'
      appSettings: [
        {
          name: 'AzureB2C_Demo_API__ClientId'
          value: '@Microsoft.KeyVault(SecretUri=https://kv-${stage}-${application}.vault.azure.net/secrets/ApiApp-AzureB2C-Demo-API-ClientId/)'
        }
        {
          name: 'AzureB2C_Demo_UI__ClientId'
          value: '@Microsoft.KeyVault(SecretUri=https://kv-${stage}-${application}.vault.azure.net/secrets/ApiApp-AzureB2C-Demo-UI-ClientId/)'
        }
        {
          name: 'AzureB2C_Demo_UI__ClientSecret'
          value: '@Microsoft.KeyVault(SecretUri=https://kv-${stage}-${application}.vault.azure.net/secrets/ApiApp-AzureB2C-Demo-UI-ClientSecret/)'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
      ]
      keyVaultReferenceIdentity: 'SystemAssigned'
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

resource apiLogExtension 'Microsoft.Web/sites/siteextensions@2022-03-01' = if (!isLinux) {
  parent: apiApp
  name: 'Microsoft.ApplicationInsights.AzureWebSites'
  dependsOn: [
    appInsights
  ]
}

resource apiLogSettings 'Microsoft.Web/sites/config@2022-03-01' = {
  parent: apiApp
  name: 'logs'
  properties: {
    applicationLogs: {
      fileSystem: {
        level: 'Warning'
      }
    }
    httpLogs: {
      fileSystem: {
        retentionInMb: 40
        enabled: true
      }
    }
    failedRequestsTracing: {
      enabled: true
    }
    detailedErrorMessages: {
      enabled: true
    }
  }
}

output servicePlanName string = servicePlan.name
output apiAppName string = apiApp.name
