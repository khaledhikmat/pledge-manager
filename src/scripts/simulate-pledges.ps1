param (
    [String]
    $baseUrl = 'http://localhost:6000',
    [String]
    $campaignId = 'CAMP-00001',
    [int]
    $loop = 100
)

Write-Output 'Simulating pledges....'

$donors = Invoke-RestMethod -Method Get -Uri "$baseUrl/users/donors"

Do {
    $donorIndex = Get-Random -Maximum ($donors.Count - 1) 
    $donor = $donors[$donorIndex]

    try {        
        $pledge = [PSCustomObject]@{
            CampaignIdentifier = $campaignId
            Amount = (Get-Random -Minimum 100 -Maximum 3000)
            Currency = 'USD'
            UserName = $donor.userName 
        }

        Write-Output "$loop - Submitting a pledge $pledge"
        Invoke-RestMethod -Method Post -Uri "$baseUrl/entities/campaigns/$campaignId/pledges" -Body ($pledge | ConvertTo-Json) -ContentType "application/json"
    }
    catch {

    }
    finally {
        $loop--
        Start-Sleep -s (Get-Random -Minimum 2 -Maximum 6)
    }
}
while ($loop -gt 0)
