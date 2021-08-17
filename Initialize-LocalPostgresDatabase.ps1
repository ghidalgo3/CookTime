param(
    [switch]$DropDb,
    [string]$User = $env:USER,
    [string]$DatabaseName = "KookTime"
)

if ($DropDb) {
    Write-Host "Recreating database $DatabaseName" -ForegroundColor 'Yellow'
    IF ($Username) {
        Write-Host "Deleting"
        dropdb -U $Username $DatabaseName
        Write-Host "Creating"
        createdb -U $Username $DatabaseName
    } else {
        Write-Host "Deleting"
        dropdb $DatabaseName
        Write-Host "Creating"
        createdb $DatabaseName
    }
}

Write-Host "Setting environment variables" -ForegroundColor 'Yellow'
[System.Environment]::SetEnvironmentVariable("ConnectionStrings__Postgres", "Server=localhost;Database=$DatabaseName")

Write-Host "Creating migrations" -ForegroundColor 'Yellow'
dotnet ef migrations add InitialMigration

Write-Host "Resetting environment variables" -ForegroundColor 'Yellow'
[System.Environment]::SetEnvironmentVariable("ConnectionStrings__Postgres", "")