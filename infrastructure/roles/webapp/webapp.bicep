@description('The stage (or environment) that the resource is a part of')
param stage string

@description('The application that the resource(s) are a part of')
param application string

@description('The region the resource(s) should be deployed to')
param region string

@description('The storage account used for web hosting')
param storageAccountName string

@description('The storage account web endpoint')
param storageAccountStaticWebEndpoint string

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-04-01' existing = {
  name: storageAccountName
}

resource storageBlobs 'Microsoft.Storage/storageAccounts/blobServices@2021-04-01' existing = {
  name: '${storageAccountName}/default'
}

var defaultTags = {
  environment: stage
  application: application
}

// The web container
resource webContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-04-01' = {
  name: '$web'
  parent: storageBlobs
  properties: {
    defaultEncryptionScope: '$account-encryption-key'
    denyEncryptionScopeOverride: false
    publicAccess: 'None'
  }
}

// Enable static website via Azure CLI
var storageAccountContributorRoleDefinitionId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '17d1049b-9a84-46fb-8f53-869881c3d3ab') // as per https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#:~:text=17d1049b-9a84-46fb-8f53-869881c3d3ab
var storageAccountStorageBlobDataContributorRoleDefinitionId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe') // as per https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#:~:text=ba92f5b4-2d11-453d-a403-e96b0029c9fe

var managedIdentityName = 'StorageStaticWebsiteEnabler'
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: managedIdentityName
  location: region
}

resource roleAssignmentContributor 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  scope: storageAccount
  name: guid(resourceGroup().id, managedIdentity.id, storageAccountContributorRoleDefinitionId)
  properties: {
    roleDefinitionId: storageAccountContributorRoleDefinitionId
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource roleAssignmentStorageBlobDataContributor 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  scope: storageAccount
  name: guid(resourceGroup().id, managedIdentity.id, storageAccountStorageBlobDataContributorRoleDefinitionId)
  properties: {
    roleDefinitionId: storageAccountStorageBlobDataContributorRoleDefinitionId
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource webScript 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  kind: 'AzureCLI'
  name: 'enableBlobStaticWeb'
  location: region
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  dependsOn: [
    roleAssignmentContributor
    roleAssignmentStorageBlobDataContributor
  ]
  properties: {
    azCliVersion: '2.26.1'
    cleanupPreference: 'OnSuccess'
    scriptContent: 'az storage blob service-properties update --account-name ${storageAccount.name} --static-website --404-document index.html --index-document index.html'
    retentionInterval: 'PT4H'
  }
}

var staticWebsiteHostName = replace(replace(storageAccountStaticWebEndpoint, 'https://', ''), '/', '')

resource cdnProfile 'Microsoft.Cdn/profiles@2020-09-01' = {
  name: 'cdn-${stage}-${application}'
  location: 'global'
  sku: {
    name: 'Standard_Microsoft'
  }
  tags: defaultTags
}


var endpointOriginName = replace(staticWebsiteHostName, '.', '-')
var originProperties = {
  hostName: staticWebsiteHostName
  httpPort: 80
  httpsPort: 443
  originHostHeader: staticWebsiteHostName
  priority: 1
  weight: 1000
  enabled: true
}

resource cdnEndpoint 'Microsoft.Cdn/profiles/endpoints@2020-09-01' = {
  name: 'web-${stage}-${application}'
  parent: cdnProfile
  location: 'global'
  tags: defaultTags
  dependsOn: [
    storageAccount
  ]
  properties: {
    originHostHeader: staticWebsiteHostName
    isCompressionEnabled: true
    contentTypesToCompress: [
      'application/eot'
      'application/font'
      'application/font-sfnt'
      'application/javascript'
      'application/json'
      'application/opentype'
      'application/otf'
      'application/pkcs7-mime'
      'application/truetype'
      'application/ttf'
      'application/vnd.ms-fontobject'
      'application/xhtml+xml'
      'application/xml'
      'application/xml+rss'
      'application/x-font-opentype'
      'application/x-font-truetype'
      'application/x-font-ttf'
      'application/x-httpd-cgi'
      'application/x-javascript'
      'application/x-mpegurl'
      'application/x-opentype'
      'application/x-otf'
      'application/x-perl'
      'application/x-ttf'
      'font/eot'
      'font/ttf'
      'font/otf'
      'font/opentype'
      'image/svg+xml'
      'text/css'
      'text/csv'
      'text/html'
      'text/javascript'
      'text/js'
      'text/plain'
      'text/richtext'
      'text/tab-separated-values'
      'text/xml'
      'text/x-script'
      'text/x-component'
      'text/x-java-source'
    ]
    isHttpAllowed: true
    isHttpsAllowed: true
    queryStringCachingBehavior: 'IgnoreQueryString'
    origins: [
      {
        name: endpointOriginName
        properties: originProperties
      }
    ]
  }
}

resource cdnEndpointOrigin 'Microsoft.Cdn/profiles/endpoints/origins@2020-09-01' = {
  name: endpointOriginName
  parent: cdnEndpoint
  properties: originProperties
}

resource httpsRuleSet 'Microsoft.Cdn/profiles/ruleSets@2020-09-01' = {
  name: 'Global'
  parent: cdnProfile

  resource httpsRule 'rules@2020-09-01' = {
    name: 'EnforceHTTPS'
    properties: {
      conditions: [
        {
          name: 'RequestScheme'
          parameters: {
            '@odata.type': '#Microsoft.Azure.Cdn.Models.DeliveryRuleRequestSchemeConditionParameters'
            operator: 'Equal'
            matchValues: [
              'HTTP'
            ]
          }
        }
      ]
      actions: [
        {
          name: 'UrlRedirect'
          parameters: {
            '@odata.type': '#Microsoft.Azure.Cdn.Models.DeliveryRuleUrlRedirectActionParameters'
            redirectType: 'Found'
            destinationProtocol: 'Https'
          }
        }
      ]
      matchProcessingBehavior: 'Stop'
    }
  }
}

output websiteHostName string = cdnEndpoint.properties.hostName
output websiteStorageAccountName string = storageAccount.name
