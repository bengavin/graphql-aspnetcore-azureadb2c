@description('The stage (or environment) that the resource is a part of')
param stage string

@description('The application that the resource(s) are a part of')
param application string

@description('The region the resource(s) should be deployed to')
param region string

@description('Should the API be hosted in a Linux environment?')
param isLinux bool

resource servicePlan 'Microsoft.Web/serverfarms@2020-12-01' existing = {
  name: 'plan-${stage}-${application}'
}

var appKind = isLinux ? 'app,linux' : 'app'

resource webApp 'Microsoft.Web/sites@2018-11-01' = {
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

output webApp object = webApp
