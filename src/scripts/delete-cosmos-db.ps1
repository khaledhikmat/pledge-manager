param (
    [String]
    $resourceGroupName = 'pledge-manager',
    [String]
    $accountName = 'pledge-manager-db',
    [String]
    $dbName = 'PledgeManagerDb'
)

# Kill the Pledge Manager DB so it will be re-created by the application
Write-Output 'Deleting CosmosDB Pledge Manager DB so it can be re-created....'
Remove-AzCosmosDBSqlDatabase -ResourceGroupName $resourceGroupName -AccountName $accountName -Name $dbName
