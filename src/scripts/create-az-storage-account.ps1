# Assumes u have:
# Install-Module -Name Az
# Install-Module Az.Accounts
# Connect-AzAccount

$resourceGroupName = 'pledge-manager'
$location = 'eastus'
$storageAccountName = 'pledgemanagerstorage'
$newStorageAccountSplat = @{
    ResourceGroupName = $resourceGroupName
    AccountName       = $storageAccountName
    Location          = $location
    SkuName           = 'Standard_LRS'
    Kind              = 'StorageV2'
}
New-AzStorageAccount @newStorageAccountSplat

$getStorageAccountSplat = @{
    ResourceGroupName = $resourceGroupName
    Name              = $storageAccountName
}
$storageContext = Get-AzStorageAccount @getStorageAccountSplat
$containerName = 'pledgemanagercontainer'
$newStorageContainerSplat = @{
    Context    = $storageContext.Context
    Name       = $containerName
    Permission = 'Off'
}
New-AzStorageContainer @newStorageContainerSplat