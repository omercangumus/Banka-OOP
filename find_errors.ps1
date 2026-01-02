# Build error finder script
Write-Host "=== BUILD ERROR DETECTOR ===" -ForegroundColor Cyan
Write-Host ""

# Build and capture ALL output
Write-Host "Building solution..." -ForegroundColor Yellow
$output = dotnet build BankaBenim.sln 2>&1 | Out-String

# Save to file
$output | Out-File -FilePath "full_build_output.txt" -Encoding UTF8

# Extract errors
Write-Host "`n=== ERRORS FOUND ===" -ForegroundColor Red
$errors = $output -split "`n" | Where-Object { $_ -match "error CS" -or $_ -match "error MSB" }

if ($errors.Count -eq 0) {
    Write-Host "No errors found in build output!" -ForegroundColor Green
    Write-Host "`nChecking build result..." -ForegroundColor Yellow
    if ($output -match "Build FAILED") {
        Write-Host "Build FAILED but no CS errors detected" -ForegroundColor Red
        Write-Host "Full output saved to: full_build_output.txt" -ForegroundColor Yellow
    }
    else {
        Write-Host "BUILD SUCCESSFUL!" -ForegroundColor Green
    }
}
else {
    foreach ($error in $errors) {
        Write-Host $error -ForegroundColor Red
    }
    Write-Host "`nTotal errors: $($errors.Count)" -ForegroundColor Yellow
}

Write-Host "`nFull output saved to: full_build_output.txt" -ForegroundColor Cyan
