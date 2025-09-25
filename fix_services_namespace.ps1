# Fix Services namespace references
$files = @(
    "Assets\_Project\Core\Audio\StealthAudioCoordinator.cs",
    "Assets\_Project\Core\Audio\AudioManager.cs",
    "Assets\_Project\Core\Patterns\StateHandlerRegistry.cs",
    "Assets\_Project\Core\Character\CharacterManager.cs",
    "Assets\_Project\Core\Camera\CameraService.cs",
    "Assets\_Project\Core\Bootstrap\GameBootstrapper.cs",
    "Assets\_Project\Core\Bootstrap\GameEventBridge.cs"
)

foreach ($file in $files) {
    $path = "D:\UnityProjects\URP3D_Base01\$file"
    if (Test-Path $path) {
        $content = Get-Content $path -Raw
        $content = $content -replace 'using asterivo\.Unity60\.Core\.Services;', 'using asterivo.Unity60.Core.Services.Interfaces;'
        Set-Content -Path $path -Value $content -NoNewline
        Write-Host "Fixed: $file"
    } else {
        Write-Host "File not found: $file" -ForegroundColor Yellow
    }
}

Write-Host "Done!" -ForegroundColor Green