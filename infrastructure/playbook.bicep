@description('The stage (or environment) that the resource is a part of')
@allowed([
  'dev'
  'test'
  'uat'
  'stage'
  'prod'
  'dr'
])
param stage string = 'test'

@description('The application that the resource(s) are a part of')
@minLength(3)
@maxLength(15)
param application string = 'securegqldemo'

@description('The region the resource(s) should be deployed to')
param region string = 'useast2'

@description('Host any web resources on Linux instead of Windows')
param hostOnLinux bool = true

@description('Existing Key Vault Secret Name(s)')
param existingKeyVaultSecrets array = [
  
]

// Target the overall file at the subscription level
// Modules are then targetted at the appropriate resource group
targetScope = 'subscription'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: 'rg-${stage}-${application}'
  location: region
}

module baseModule 'roles/base/base.bicep' = {
  scope: resourceGroup
  name: 'baseModule'
  params: {
    application: application
    region: region
    stage: stage
  }  
}

module apiAppModule 'roles/apiapp/apiapp.bicep' = {
  scope: resourceGroup
  name: 'apiAppModule'
  params: {
    application: application
    isLinux: hostOnLinux
    region: region
    stage: stage
  }
}

module webAppModule 'roles/webapp/webapp.bicep' = {
  scope: resourceGroup
  name: 'webAppModule'
  params: {
    application: application
    region: region
    stage: stage
    storageAccountName: baseModule.outputs.storageAccountName
    storageAccountStaticWebEndpoint: baseModule.outputs.storageAccountStaticWebsiteHost
  }
}

resource funcResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: 'rg-${stage}-${application}-func'
  location: region
}

module funcAppModule 'roles/funcapp/funcapp.bicep' = {
  scope: funcResourceGroup
  name: 'funcAppModule'
  params: {
    application: application
    isLinux: hostOnLinux
    region: region
    stage: stage
    siblingResourceGroupName: resourceGroup.name
  }
}

module keyVaultModule 'roles/keyvault/keyvault.bicep' = {
  scope: resourceGroup
  name: 'keyVaultModule'
  params: {
    apiAppName: apiAppModule.outputs.apiApp.properties.name
    application: application
    funcAppName: funcAppModule.outputs.funcApp.properties.name
    funcAppResourceGroup: funcResourceGroup.name
    region: region
    stage: stage
    existingSecrets: existingKeyVaultSecrets
  }
}

// Output created App Service information
output apiAppName string = apiAppModule.outputs.apiApp.properties.name
output webAppStorageAccountName string = webAppModule.outputs.websiteStorageAccountName
output webAppHostname string = webAppModule.outputs.websiteHostName
output funcAppName string = funcAppModule.outputs.funcApp.properties.name
