@description('The stage (or environment) that the resource is a part of')
param stage string

@description('The application that the resource(s) are a part of')
param application string

@description('The region the resource(s) should be deployed to')
param region string

@description('The API App Service Name of the API Application for assignment of KeyVault permissions')
param apiAppName string = ''

@description('The ObjectId of the Function Application for assignment of KeyVault permissions')
param funcAppName string = ''

@description('The Resource Group for the function app')
param funcAppResourceGroup string = ''

@description('The ObjectId of the Web Application for assignment of KeyVault permissions')
param webAppName string = ''

// App Service References
resource apiApp 'Microsoft.Web/sites@2018-11-01' existing = if (apiAppName != '') {
  name: apiAppName
}

resource funcApp 'Microsoft.Web/sites@2018-11-01' existing = if (funcAppName != '' && funcAppResourceGroup != '') {
  scope: resourceGroup(funcAppResourceGroup)
  name: funcAppName
}

resource webApp 'Microsoft.Web/sites@2018-11-01' existing = if (webAppName != '') {
  name: webAppName
}

var appPolicy = apiAppName != '' ? [
{
  objectId: apiApp.identity.principalId
  tenantId: subscription().tenantId
  permissions: {
    keys: [
      'get'
    ]
    secrets: [
      'get'
    ]
    certificates: [
      'get'
    ]
  } 
}
] : []

var funcPolicy = funcAppName != '' ? [
{
  objectId: funcApp.identity.principalId
  tenantId: subscription().tenantId
  permissions: {
    keys: [
      'get'
    ]
    secrets: [
      'get'
    ]
    certificates: [
      'get'
    ]
  } 
}
] : []

var webPolicy = webAppName != '' ? [
{
  objectId: webApp.identity.principalId
  tenantId: subscription().tenantId
  permissions: {
    keys: [
      'get'
    ]
    secrets: [
      'get'
    ]
    certificates: [
      'get'
    ]
  }
} 
] : []

var vaultPolicies = concat(appPolicy, funcPolicy, webPolicy)

// Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2019-09-01' = {
  name: 'kv-${stage}-${application}'
  location: region
  properties: {
    enabledForDeployment: true
    enabledForTemplateDeployment: true
    enabledForDiskEncryption: true
    enableRbacAuthorization: false
    accessPolicies: vaultPolicies
    tenantId: subscription().tenantId
    sku: {
      name: 'standard'
      family: 'A'
    }
  }
  tags: {
    application: application
    environment: stage
  }
}

// This will create the shells for the needed secrets, the actual values need to be filled in via
// the Key Vault portal or other external scripts
resource apiAppApiClientId 'Microsoft.KeyVault/vaults/secrets@2019-09-01' existing = {
  name: '${keyVault.name}/ApiApp-AzureB2C-Demo-API-ClientId'
}

resource apiAppApiClientIdNew 'Microsoft.KeyVault/vaults/secrets@2019-09-01' = if (apiAppApiClientId.id == null) {
  name: '${keyVault.name}/ApiApp-AzureB2C-Demo-API-ClientId'
  properties: {
    value: '<fill in portal>'
  }
}

resource apiAppUIClientId 'Microsoft.KeyVault/vaults/secrets@2019-09-01' existing = {
  name: '${keyVault.name}/ApiApp-AzureB2C-Demo-UI-ClientId'
}

resource apiAppUIClientIdNew 'Microsoft.KeyVault/vaults/secrets@2019-09-01' = if (apiAppUIClientId.id == null) {
  name: '${keyVault.name}/ApiApp-AzureB2C-Demo-UI-ClientId'
  properties: {
    value: '<fill in portal>'
  }
}

resource apiAppUIClientSecret 'Microsoft.KeyVault/vaults/secrets@2019-09-01' existing = {
  name: '${keyVault.name}/ApiApp-AzureB2C-Demo-UI-ClientSecret'
}

resource apiAppUIClientSecretNew 'Microsoft.KeyVault/vaults/secrets@2019-09-01' = if (apiAppUIClientSecret.id == null) {
  name: '${keyVault.name}/ApiApp-AzureB2C-Demo-UI-ClientSecret'
  properties: {
    value: '<fill in portal>'
  }
}
