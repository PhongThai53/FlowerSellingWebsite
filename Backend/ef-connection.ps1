# Read the connection string from appsettings.json
$appSettingsPath = "appsettings.json"
$appSettings = Get-Content -Path $appSettingsPath -Raw | ConvertFrom-Json
$connectionString = $appSettings.ConnectionStrings.DefaultConnection

# Run the database update command with the connection string
dotnet ef database update --connection "$connectionString"