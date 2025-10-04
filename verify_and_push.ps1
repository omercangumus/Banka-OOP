# verify_and_push.ps1
Write-Host "Running Tests before pushing..." -ForegroundColor Cyan

# Run Tests
dotnet test src\BankApp.Tests\BankApp.Tests.csproj

if ($LastExitCode -ne 0) {
    Write-Error "Tests FAILED! Push aborted."
    exit 1
}

Write-Host "Tests Passed! Pushing to remote..." -ForegroundColor Green

# Push logic
# Checks if remote 'origin' exists
$remotes = git remote
if ($remotes -contains "origin") {
    git push origin main
}
else {
    Write-Warning "Remote 'origin' not found. Please add it using: git remote add origin <gitlab_url>"
}
