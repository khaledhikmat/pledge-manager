param (
    [String]
    $redisServer = 'localhost',
    # [SecureString]
    # $residPassword = '',
    [int]
    $port = 6379
)

# Empty Redis Cache so it will be re-created by the application
Write-Output 'Emptying Redis Cache so it can be re-created....'
$redisCommands = "SELECT 0`r`nFLUSHDB`r`nQUIT`r`n"
$redisCommands | nc $redisServer $port