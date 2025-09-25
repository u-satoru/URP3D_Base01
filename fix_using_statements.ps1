# PowerShell script to fix missing using statements in Unity project

# Function to add using statement if not exists
function Add-UsingStatement {
    param(
        [string]$FilePath,
        [string]$UsingStatement
    )

    if (Test-Path $FilePath) {
        $content = Get-Content $FilePath -Raw

        # Check if using statement already exists (commented or not)
        if ($content -notmatch [regex]::Escape($UsingStatement)) {
            # Add after the first using statement or at the beginning
            if ($content -match "using\s+[^;]+;") {
                $firstUsing = $Matches[0]
                $newContent = $content -replace [regex]::Escape($firstUsing), "$firstUsing`n$UsingStatement"
            } else {
                $newContent = "$UsingStatement`n$content"
            }

            Set-Content -Path $FilePath -Value $newContent
            Write-Host "Added '$UsingStatement' to $FilePath" -ForegroundColor Green
        } elseif ($content -match "//\s*$([regex]::Escape($UsingStatement))") {
            # Uncomment if commented
            $newContent = $content -replace "//\s*$([regex]::Escape($UsingStatement))", $UsingStatement
            Set-Content -Path $FilePath -Value $newContent
            Write-Host "Uncommented '$UsingStatement' in $FilePath" -ForegroundColor Yellow
        }
    }
}

# Core folder files that need Commands namespace
$coreFiles = @(
    "Assets\_Project\Core\Audio\Commands\PlaySoundCommand.cs",
    "Assets\_Project\Core\Audio\Commands\PlaySoundCommandDefinition.cs",
    "Assets\_Project\Core\Character\CharacterManager.cs",
    "Assets\_Project\Core\Camera\CameraController.cs",
    "Assets\_Project\Core\Character\ICharacterManager.cs",
    "Assets\_Project\Core\Camera\ICameraController.cs",
    "Assets\_Project\Core\Interaction\InteractionCommands.cs"
)

foreach ($file in $coreFiles) {
    $fullPath = "D:\UnityProjects\URP3D_Base01\$file"
    Add-UsingStatement -FilePath $fullPath -UsingStatement "using asterivo.Unity60.Core.Commands;"
}

# Files that need Services.Interfaces namespace
$servicesFiles = @(
    "Assets\_Project\Core\Bootstrap\GameEventBridge.cs",
    "Assets\_Project\Core\Bootstrap\GameBootstrapper.cs",
    "Assets\_Project\Core\Camera\CameraService.cs",
    "Assets\_Project\Core\Character\CharacterManager.cs",
    "Assets\_Project\Core\Patterns\StateHandlerRegistry.cs"
)

foreach ($file in $servicesFiles) {
    $fullPath = "D:\UnityProjects\URP3D_Base01\$file"
    Add-UsingStatement -FilePath $fullPath -UsingStatement "using asterivo.Unity60.Core.Services.Interfaces;"
}

Write-Host "Using statement fixes completed!" -ForegroundColor Cyan