$output = dotnet build src/BankApp.UI/BankApp.UI.csproj -consoleloggerparameters:ErrorsOnly 2>&1
$output | Select-String ": error" | ForEach-Object { Write-Host $_ }
