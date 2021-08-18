# Securing ASP.NET Core GraphQL API with Azure Active Directory B2C
## Blog Series Sample

This repository contains code samples for the blog series.  For details, see https://virtual-olympus.com/.

Branches are setup for each part in the series.  Switch to the appropriate branch to see the associated code.

The current state of the completed sample can be found in the 'release' branch.

## Infrastructure

The project uses Azure Bicep, which must be setup in your environment prior to utilizing the solution.  Setup instructions are here:

[Bicep Installation](https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/install)

This example uses some [Ansible](https://www.ansible.com) (another Infrastructure as Code tool) terminology to setup the environment.  In this case, the main Bicep file is called playbook.bicep, that file references the other module level .bicep files to effect the desired state of the environment.  See the 'part-4' branch for the Ansible variant of this solution.  This Bicep solution was created to address some of the limitations in the Ansible approach that can be remedied via bringing the solution 'closer to the metal' (e.g. the requirement to setup some Azure resources via ARM template as full support isn't available in Ansible).

## Linking with the target Azure subscription

The example uses 'cli' based authentication to Azure.  This method uses the Azure CLI to link with the target Azure subscription.  Getting Ansible installed and configured should have installed the Azure CLI so it can be used independently.

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

Run the following command to execute the playbook and install/configure your Azure resources in the currently selected subscription.  The Bicep file creates a couple resource groups based on the environment and application name, and as such, requires that the deployment be created in the 'subscription' scope.  This means a location needs to be specified to store the metadata about the deployment(s), which can be different than the location that the Azure resources are housed.

Test Environment Deployment
```
az deployment sub create --location eastus2 --template-file playbook.bicep --parameters vars/playbook.parameters.json vars/playbook.parameters.test.json
```

Stage Environment Deployment
```
az deployment sub create --location eastus2 --template-file playbook.bicep --parameters vars/playbook.parameters.json vars/playbook.parameters.stage.json
```

## 'Features' of the solution thus far

There is currently a limitation in Azure around consumption based function apps, specifically those hosted in linux environments.  These apps cannot be hosted in a 'Free' tier service plan, and additionally, must be hosted in a special-use consumption based application service plan.  These Linux based consumption plans can only co-exist in a resource group that contains only other consumption based service plans.  The function app `.bicep` file in use here puts both the service plan and the function app together in a resource group, but technically, only the consumption based service plan need exist outside the main resource group.  For more details on the 'why' of this, see [this article](https://github.com/Azure/Azure-Functions/wiki/Creating-Function-Apps-in-an-existing-Resource-Group)
