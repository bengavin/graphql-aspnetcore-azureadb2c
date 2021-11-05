@description('The stage (or environment) that the resource is a part of')
param stage string

@description('The application that the resource(s) are a part of')
param application string

@description('The region the resource(s) should be deployed to')
param region string

@description('Should the API be hosted in a Linux environment?')
param isLinux bool

// We'll host the Blazor app in the same farm as the API (different site, same plan)
resource servicePlan 'Microsoft.Web/serverfarms@2020-12-01' existing = {
  name: 'plan-${stage}-${application}'
}

var appKind = isLinux ? 'app,linux' : 'app'

resource blazorApp 'Microsoft.Web/sites@2018-11-01' = {
  name: 'app-${stage}-${application}-web'
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
      webSocketsEnabled: true
      appSettings: [
        {
          name: 'AzureB2C_Blazor_UI__ClientId'
          value: '@Microsoft.KeyVault(SecretUri=https://kv-${stage}-${application}.vault.azure.net/secrets/BlazorApp-AzureB2C-Blazor-UI-ClientId/)'
        }
        {
          name: 'AzureB2C_Blazor_UI__ClientSecret'
          value: '@Microsoft.KeyVault(SecretUri=https://kv-${stage}-${application}.vault.azure.net/secrets/BlazorApp-AzureB2C-Blazor-UI-ClientSecret/)'
        }
        {
          name: 'AzureB2C_Blazor_UI__TokenCache'
          value: '@Microsoft.KeyVault(SecretUri=https://kv-${stage}-${application}.vault.azure.net/secrets/BlazorApp-AzureB2C-Blazor-UI-TokenCacheConfiguration/)'
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

output blazorApp object = blazorApp
