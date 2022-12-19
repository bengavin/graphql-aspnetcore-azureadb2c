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

// Get a list of valid locations with: az account list-locations | jq -cr 'to_entries[] | .value.name'
@description('The region the resource(s) should be deployed to')
@allowed([
  'eastus'
  'eastus2'
  'westus'
  'westus2'
  'westus3'
  'centralus'
  'northcentralus'
  'southcentralus'
  'westcentralus'  
])
param region string = 'eastus2'

@description('Host any web resources on Linux instead of Windows')
param hostOnLinux bool = true

@description('Existing Key Vault Secret Name(s)')
param existingKeyVaultSecrets array = [
  
]

// Target the overall file at the subscription level
// Modules are then targetted at the appropriate resource group
targetScope = 'subscription'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: 'rg-${stage}-${application}'
  location: region
}

module baseModule 'roles/base.bicep' = {
  scope: resourceGroup
  name: 'baseModule'
  params: {
    application: application
    region: region
    stage: stage
  }
}

module apiAppModule 'roles/apiapp.bicep' = {
  scope: resourceGroup
  name: 'apiAppModule'
  params: {
    application: application
    region: region
    stage: stage
    isLinux: hostOnLinux
    appInsightsName: baseModule.outputs.appInsightsName
  }
}

module keyVaultModule 'roles/keyvault.bicep' = {
  scope: resourceGroup
  name: 'keyVaultModule'
  params: {
    apiAppName: apiAppModule.outputs.apiAppName
    application: application
    //funcAppName: funcAppModule.outputs.funcAppName
    region: region
    stage: stage
    //webAppName: webAppModule.outputs.webAppName
    existingSecrets: existingKeyVaultSecrets
  }
}

// Output created App Service information
output apiAppName string = apiAppModule.outputs.apiAppName
//output webAppName string = webAppModule.outputs.webApp.properties.name
//output funcAppName string = funcAppModule.outputs.funcApp.properties.name
