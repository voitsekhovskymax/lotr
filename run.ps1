param (
    [string]$WorldName = ""
)

Clear-Host
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "       LOTR Mod Build & Run Script       " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

Write-Host "Building project..." -ForegroundColor Yellow
dotnet build lotr.csproj -c Debug
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Build failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}
Write-Host "Build succeeded!" -ForegroundColor Green

# Check if Vintage Story is already running
$processes = Get-Process -Name "Vintagestory" -ErrorAction SilentlyContinue
if ($processes) {
    Write-Host "-----------------------------------------" -ForegroundColor Cyan
    Write-Host "Vintage Story is already running." -ForegroundColor Yellow
    Write-Host "To apply changes, just type '/reload' in the game chat." -ForegroundColor Green
    Write-Host "-----------------------------------------" -ForegroundColor Cyan
} else {
    Write-Host "Launching Vintage Story..." -ForegroundColor Yellow
    $arguments = @()
    if ($WorldName) {
        $arguments += "--openWorld=`"$WorldName`""
        Write-Host "Will auto-load world: $WorldName" -ForegroundColor Cyan
    }
    
    Start-Process -FilePath "D:\games\VintageStory\Vintagestory.exe" -ArgumentList $arguments
}
