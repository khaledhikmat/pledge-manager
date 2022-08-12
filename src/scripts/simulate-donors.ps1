param (
    [String]
    $baseUrl = 'http://localhost:6000'
)

Write-Output 'Simulating donors....'

$donors = Invoke-RestMethod -Method Get -Uri "$baseUrl/users/donors"

$code = '123456'

$donors | ForEach-Object {
    $username = $_.UserName
    Write-Output "Simulating donor $username"
    Invoke-RestMethod -Method Post -Uri "$baseUrl/users/verifications/$username"
    Invoke-RestMethod -Method Post -Uri "$baseUrl/users/verifications/$username/$code"
}
