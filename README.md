## Securing ASP.NET Core GraphQL API with Azure Active Directory B2C
### Blog Series Sample

This repository contains code samples for the blog series.  For details, see https://virtual-olympus.com/.

Branches are setup for each part in the series.  Switch to the appropriate branch to see the associated code.

The current state of the completed sample can be found in the 'release' branch.

### Infrastructure

The project uses Ansible, which must be setup in your environment prior to utilizing the playbook.  Setup instructions are here:

[Ansible Installation](https://docs.ansible.com/ansible/latest/installation_guide/index.html)

*NOTE: Plan to use the Python 3+ variant when possible.*

Once Ansible itself is configured, run the following command to verify the Azure related dependencies are installed.

```
pip install -r requirements-azure.txt
```

More details can be found [here](https://github.com/ansible-collections/azure)

#### Linking with the target Azure subscription

The example uses 'cli' based authentication to Azure.  This method uses the Azure CLI to link with the target Azure subscription.  Getting Ansible installed and configured should have installed the Azure CLI so it can be used independently.

Getting hooked into your subscription is as simple as executing the following commands:

```
az login
```

This launches a browser window to prompt you to login to your subscription.  Once logged in, you'll be presented with a list of available subscriptions.

```
az account set --subscription '<subscription id>'
```

... and that's it, now the Ansible playbooks should work properly.

#### Executing the Ansible playbooks

Run the following command to execute the playbook and install/configure your Azure resources in the currently selected subscription.

```
ansible-playbook -i inventories/azure/sample.azure_rm.yml playbook.yml --extra-vars="owner_id=\"<object id of an 'owner' user in tenant AD>\""
```
