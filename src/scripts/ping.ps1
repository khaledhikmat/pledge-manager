
Start-ThreadJob -Name PingPledgeManager -Scriptblock { 
    param (
        [String]
        $baseUrl = 'http://localhost:6000'
    )

    [int]$loop = 0
    Do {
        Start-Sleep -s 5
        try {
            $result = Invoke-RestMethod -Method Get -Uri "$baseUrl/entities/ping"
        }
        catch {

        }
        finally {
            $loop++
        }
    }
    while ($result -ne 'I am up!' -and $loop -lt 5)

    if ($result -eq 'I am up!') {
        # //https://stackoverflow.com/questions/71624284/issue-with-start-threadjob-scriptblock-unable-to-find-powershell-script
        # The full path of the script has to be given because of above link
        /Users/khaled/github/pledge-manager/src/scripts/load-sample-data.ps1 -baseUrl $baseUrl
        /Users/khaled/github/pledge-manager/src/scripts/simulate-donors.ps1 -baseUrl $baseUrl
        #/Users/khaled/github/pledge-manager/src/scripts/simulate-pledges.ps1 -baseUrl $baseUrl
    }
    else {
        Write-Warning 'Could not reach pledge-manager'        
    }
}