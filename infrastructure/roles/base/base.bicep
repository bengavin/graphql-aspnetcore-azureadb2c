@description('The stage (or environment) that the resource is a part of')
param stage string

@description('The application that the resource(s) are a part of')
param application string

@description('The region the resource(s) should be deployed to')
param region string

// Log Analytics
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2020-10-01' = {
  name: 'log-${stage}-${application}'
  location: region
  properties: {
    sku: {
      name: 'Free'
    }
  }
  tags: {
    environment: stage
    application: application
  }
}

// Application Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02-preview' = {
  name: 'appi-${stage}-${application}'
  location: region
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
  tags: {
    environment: stage
    application: application
  }
}

// Storage Account
resource storageAccount 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: 'stg${stage}${application}'
  location: region
  kind: 'StorageV2'
  sku: {
    name: 'Standard_RAGRS'
  }
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: true
    allowSharedKeyAccess: true
    supportsHttpsTrafficOnly: true
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
      virtualNetworkRules: []
      ipRules: []
    }
  }
  tags: {
    environment: stage
    application: application
  }
}

// Cosmos Cache
resource cosmosCache 'Microsoft.DocumentDB/databaseAccounts@2021-07-01-preview' = {
  name: 'cosmos-${stage}-${application}'
  location: region
  kind: 'GlobalDocumentDB'
  properties: {
    createMode: 'Default'
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: region
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
    enableFreeTier: false
  }
  tags: {
    environment: stage
    application: application
  }
}

resource cosmosSqlDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2021-07-01-preview' = {
  parent: cosmosCache
  name: 'TokenCache'
  properties: {
    resource: {
      id: 'TokenCache'
    }
  }
}

output logAnalytics object = logAnalytics
output appInsights object = appInsights
output storageAccountName string = storageAccount.name
output storageAccountStaticWebsiteHost string = storageAccount.properties.primaryEndpoints.web
