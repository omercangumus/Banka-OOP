$output = dotnet build src/BankApp.UI/BankApp.UI.csproj
$output | Where-Object { $_ -match "error" -or $_ -match "Hata" } | ForEach-Object { Write-Host $_ }
