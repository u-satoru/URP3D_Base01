# convert_files_to_utf8.ps1
$filesToConvert = @(
    "D:\UnityProjects\URP3D_Base01\Assets\_Project\Core\Audio\AmbientManager.cs",
    "D:\UnityProjects\URP3D_Base01\Assets\_Project\Core\Audio\AudioManager.cs",
    "D:\UnityProjects\URP3D_Base01\Assets\_Project\Core\Audio\BGMManager.cs",
    "D:\UnityProjects\URP3D_Base01\Assets\_Project\Core\Audio\Controllers\MaskingEffectController.cs",
    "D:\UnityProjects\URP3D_Base01\Assets\_Project\Core\Audio\Controllers\TimeAmbientController.cs",
    "D:\UnityProjects\URP3D_Base01\Assets\_Project\Core\Audio\Controllers\WeatherAmbientController.cs",
    "D:\UnityProjects\URP3D_Base01\Assets\_Project\Core\Audio\DynamicAudioEnvironment.cs",
    "D:\UnityProjects\URP3D_Base01\Assets\_Project\Core\Audio\Services\AudioService.cs",
    "D:\UnityProjects\URP3D_Base01\Assets\_Project\Features\Player\Scripts\States\CoverState.cs",
    "D:\UnityProjects\URP3D_Base01\Assets\_Project\Features\Player\Scripts\States\JumpingState.cs",
    "D:\UnityProjects\URP3D_Base01\Assets\_Project\Features\Player\Scripts\States\ProneState.cs",
    "D:\UnityProjects\URP3D_Base01\Assets\_Project\Features\Player\Scripts\States\RollingState.cs",
    "D:\UnityProjects\URP3D_Base01\Assets\_Project\Features\Player\Scripts\States\WalkingState.cs"
)

# Create a UTF-8 encoding object without a BOM
$utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)

foreach ($file in $filesToConvert) {
    try {
        Write-Host "Converting $file..."
        # Read the file assuming the system's default ANSI encoding (like Shift-JIS on Japanese Windows)
        $content = Get-Content -Path $file -Encoding Default -Raw
        # Write the content back as UTF-8 without BOM using the custom encoding object
        [System.IO.File]::WriteAllLines($file, $content, $utf8WithoutBom)
        Write-Host "Successfully converted $file to UTF-8 (No BOM)." -ForegroundColor Green
    } catch {
        Write-Host "Error converting file $file`: $_" -ForegroundColor Red
    }
}

Write-Host "Conversion process complete."