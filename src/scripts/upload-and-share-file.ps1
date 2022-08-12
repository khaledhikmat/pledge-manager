# Assumes u have:
# Install-Module -Name Az
# Install-Module Az.Accounts
# Connect-AzAccount

param (
    [String]
    $resourceGroupName = 'pledge-manager',
    [String]
    $storageAccountName = 'pledgemanagerstorage',
    [String]
    $containerName = 'pledgemanagercontainer',
    [Parameter(Mandatory)]
    [String]
    $fileFullPath,
    [Parameter(Mandatory)]
    [String]
    $blobName
)

Write-Output $resourceGroupName

$getStorageAccountSplat = @{
    ResourceGroupName = $resourceGroupName
    Name              = $storageAccountName
}
$storageContext = Get-AzStorageAccount @getStorageAccountSplat

$setAzStorageBlobSplat = @{
    Context   = $storageContext.Context
    Container = $containerName
    File      = $fileFullPath
    Blob      = $blobName
}
Set-AzStorageBlobContent @setAzStorageBlobSplat

$newStorageSASSplat = @{
    Context    = $storageContext.Context
    Container  = $containerName
    Blob       = $blobName
    ExpiryTime = (Get-Date).AddDays(1)
    Permission = 'r'
    FullUri    = $true
}
$url = New-AzStorageBlobSASToken @newStorageSASSplat
$url

