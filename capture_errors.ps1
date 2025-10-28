$ErrorActionPreference = "Continue"

Write-Host "Building and capturing errors..." -ForegroundColor Cyan

$proc = Start-Process -FilePath "dotnet" -ArgumentList "build", "BankaBenim.sln" -NoNewWindow -PassThru -Wait -RedirectStandardError "stderr.txt" -RedirectStandardOutput "stdout.txt"

Write-Host "`n=== ERRORS FROM STDOUT ===" -ForegroundColor Red
Get-Content "stdout.txt" | Where-Object { $_ -match "error CS" } | ForEach-Object {
    Write-Host $_ -ForegroundColor Yellow
}

Write-Host "`n=== ERRORS FROM STDERR ===" -ForegroundColor Red  
Get-Content "stderr.txt" | Where-Object { $_ -match "error CS" } | ForEach-Object {
    Write-Host $_ -ForegroundColor Yellow
}

Write-Host "`nExit code: $($proc.ExitCode)" -ForegroundColor Cyan
