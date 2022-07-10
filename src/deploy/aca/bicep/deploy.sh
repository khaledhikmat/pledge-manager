az login
az upgrade
az extension add --name containerapp --upgrade
az provider register --namespace Microsoft.App

# Set environment variables
RESOURCE_GROUP="pledgemanager-dev-rg"

# Deploy against the SUBSCRIPTION

# validate the environment
az deployment sub validate --location eastus --template-file ./main.bicep --parameters ./dev-env-params.json 

# Buildup the environment
az deployment sub create --location eastus --template-file ./main.bicep --parameters ./dev-env-params.json 

# Whatif the environment
az deployment sub create --what-if --location eastus --template-file ./main.bicep --parameters ./dev-env-params.json 

# Teardown the environment
az group delete \
  --resource-group $RESOURCE_GROUP