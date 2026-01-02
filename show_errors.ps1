$content = Get-Content "full_build_output.txt" -Raw -Encoding UTF8
$lines = $content -split "`n"

Write-Host "=== ALL BUILD ERRORS ===" -ForegroundColor Red
$errorLines = $lines | Where-Object { $_ -match ":\s*error\s+(CS|MSB)\d+" }

$errorCount = 0
foreach ($line in $errorLines) {
    $errorCount++
    Write-Host "$errorCount. $line" -ForegroundColor Yellow
}

if ($errorCount -eq 0) {
    Write-Host "NO ERRORS FOUND!" -ForegroundColor Green
    
    # Check if build succeeded
    if ($content -match "Build succeeded") {
        Write-Host "`nBUILD SUCCESSFUL!" -ForegroundColor Green
    }
    elseif ($content -match "Build FAILED") {
        Write-Host "`nBuild failed but no error lines detected!" -ForegroundColor Red
    }
}
else {
    Write-Host "`nTotal unique errors: $errorCount" -ForegroundColor Cyan
}
