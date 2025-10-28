$env:MSBuildSDKsPath = $null
Write-Host "Temizleniyor..." -ForegroundColor Yellow
dotnet clean
if ($?) {
    Write-Host "Derleniyor..." -ForegroundColor Yellow
    dotnet build
    if ($?) {
        Write-Host "BAŞARILI! Uygulama Hazır." -ForegroundColor Green
    }
    else {
        Write-Host "DERLEME HATASI!" -ForegroundColor Red
    }
}
else {
    Write-Host "TEMİZLEME HATASI!" -ForegroundColor Red
}
Read-Host "Kapatmak için Enter'a bas..."
