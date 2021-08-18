@description('The stage (or environment) that the resource is a part of')
param stage string

@description('The application that the resource(s) are a part of')
param application string

@description('The region the resource(s) should be deployed to')
param region string

@description('Should the API be hosted in a Linux environment?')
param isLinux bool

@description('Resource Group Name for Application Insights / Storage Account')
param siblingResourceGroupName string

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-04-01' existing = {
  scope: resourceGroup(siblingResourceGroupName)
  name: 'stg${stage}${application}'
}

resource appInsights 'Microsoft.Insights/components@2020-02-02-preview' existing = {
  scope: resourceGroup(siblingResourceGroupName)
  name: 'appi-${stage}-${application}'
}

var appKind = isLinux ? 'functionapp,linux' : 'functionapp'
var storageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}'
var funcAppName = 'func-${stage}-${application}'

// Consumption based functions require a custom app service plan
resource funcServicePlan 'Microsoft.Web/serverfarms@2020-12-01' = {
  name: '${funcAppName}-plan'
  location: region
  kind: isLinux ? 'linux' : 'web'
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {
    targetWorkerSizeId: 0
    targetWorkerCount: 1
    reserved: true
  }
}

resource funcApp 'Microsoft.Web/sites@2020-12-01' = {
  name: funcAppName
  location: region
  kind: appKind
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: funcServicePlan.id
    clientAffinityEnabled: false
    siteConfig: {      
      appSettings: [
        {
          name: 'AzureWebJobsDashboard'
          value: storageConnectionString
        }
        {
          name: 'AzureWebJobsStorage'
          value: storageConnectionString
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: storageConnectionString
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: toLower(funcAppName)
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'APPINSIGHTS_CONNECTIONSTRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
      ]
      use32BitWorkerProcess: false
      linuxFxVersion: 'DOTNET|3.1'
      windowsFxVersion: 'DOTNET|3.1'
    }
  }
}

output funcApp object = funcApp
