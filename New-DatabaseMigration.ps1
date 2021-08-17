param(
    [Parameter(Mandatory=$true)]
    [string]
    $MigrationName
)

$dotnetefVersion = "5.0.9"
$tools = dotnet tool list --global
$dotnetEfInstalled = $tools | Where-Object { $_.Contains('dotnet-ef') }
if ($null -ne $dotnetEfInstalled) {
    $line = $dotnetEfInstalled.Split(" ", [System.StringSplitOptions]::RemoveEmptyEntries)
    if ($line[1] -ne $dotnetefVersion) {
        Write-Warning "You must install dotnet-ef $dotnetefVersion. dotnet-ef global tool will be uninstalled and reinstalled at the correct version."
        dotnet tool uninstall --global dotnet-ef
        dotnet tool install --global dotnet-ef --version $dotnetefVersion
    } else {
        Write-Host "dotnet-ef global tool $dotnetefVersion found!" -ForegroundColor Green
    }
} else {
    dotnet tool install --global dotnet-ef --version $dotnetefVersion
}


Write-Host "Setting environment variables" -ForegroundColor 'Yellow'
[System.Environment]::SetEnvironmentVariable("POSTGRESQLCONNSTR_Postgres", "Server=localhost;Database=$DatabaseName")

Write-Host "Creating migrations" -ForegroundColor 'Yellow'
dotnet ef migrations add $MigrationName 

Write-Host "Resetting environment variables" -ForegroundColor 'Yellow'
[System.Environment]::SetEnvironmentVariable("POSTGRESQLCONNSTR_Postgres", "")

Write-Host "Done!" -ForegroundColor 'Green'