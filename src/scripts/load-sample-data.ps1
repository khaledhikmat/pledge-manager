param (
    [String]
    $baseUrl = 'http://localhost:6000'
)

Write-Output 'Loading Sample Data in CosmosDB....'
Invoke-RestMethod -Method Post -Uri "$baseUrl/entities/sample"
