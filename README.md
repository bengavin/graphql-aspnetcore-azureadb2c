## Securing ASP.NET Core GraphQL API with Azure Active Directory B2C
### Blog Series Sample

This repository contains code samples for the blog series.  For details, see https://virtual-olympus.com/.

Branches are setup for each part in the series.  Switch to the appropriate branch to see the associated code.

The current state of the completed sample can be found in the 'release' branch.

## Infrastructure

The project uses Azure Bicep, which must be setup in your environment prior to utilizing the solution.  Setup instructions are here:

[Bicep Installation](https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/install)

## Linking with the target Azure subscription

The example uses 'cli' based authentication to Azure.  This method uses the Azure CLI to link with the target Azure subscription.  Instructions for installing the Azure CLI can be [found here](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)

Getting hooked into your subscription is as simple as executing the following commands:

```
az login
```

This launches a browser window to prompt you to login to your subscription.  Once logged in, you'll be presented with a list of available subscriptions.

```
az account set --subscription '<subscription id>'
```

... and that's it, now the Bicep templates should work properly.

## Deploying the Bicep templates

Run the following command to execute the playbook and install/configure your Azure resources in the currently selected subscription.  The Bicep file creates a resource group based on the environment and application name, and as such, requires that the deployment be created in the 'subscription' scope.  This means a location needs to be specified to store the metadata about the deployment(s), which can be different than the location that the Azure resources are housed.

Dev Environment Deployment
```
az deployment sub create --name playbook-centralus --location centralus --template-file playbook.bicep --parameters vars/playbook.parameters.json vars/playbook.parameters.test.json
```

Test Environment Deployment
```
az deployment sub create --name playbook-eastus2 --location eastus2 --template-file playbook.bicep --parameters vars/playbook.parameters.json vars/playbook.parameters.test.json
```

Stage Environment Deployment
```
az deployment sub create --name playbook-northcentralus --location northcentralus --template-file playbook.bicep --parameters vars/playbook.parameters.json vars/playbook.parameters.stage.json
```
