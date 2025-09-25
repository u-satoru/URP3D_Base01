# Comprehensive fix for all compilation errors

Write-Host "Fixing compilation errors in Unity project..." -ForegroundColor Green

# Fix 1: Update Services namespace references
Write-Host "`nStep 1: Fixing Services namespace references..." -ForegroundColor Yellow

$filesToFixServices = @(
    "Assets\_Project\Core\Bootstrap\GameBootstrapper.cs",
    "Assets\_Project\Core\Bootstrap\GameEventBridge.cs",
    "Assets\_Project\Core\Camera\CameraService.cs",
    "Assets\_Project\Core\Character\CharacterManager.cs",
    "Assets\_Project\Core\Patterns\StateHandlerRegistry.cs"
)

foreach ($file in $filesToFixServices) {
    $filePath = "D:\UnityProjects\URP3D_Base01\$file"
    if (Test-Path $filePath) {
        $content = Get-Content $filePath -Raw

        # Replace Core.Services with Core.Services.Interfaces
        $content = $content -replace 'using asterivo\.Unity60\.Core\.Services;', 'using asterivo.Unity60.Core.Services.Interfaces;'

        Set-Content -Path $filePath -Value $content -NoNewline
        Write-Host "  Fixed: $file" -ForegroundColor Cyan
    }
}

# Fix 2: Add missing Core.Data using statements for stealth types
Write-Host "`nStep 2: Adding missing Core.Data namespaces for stealth types..." -ForegroundColor Yellow

$filesToAddData = @(
    "Assets\_Project\Core\Audio\Interfaces\IStealthAudioService.cs",
    "Assets\_Project\Core\Audio\StealthAudioCoordinator.cs",
    "Assets\_Project\Core\Audio\Services\StealthAudioService.cs",
    "Assets\_Project\Core\Events\StealthEvents\AlertEvents.cs",
    "Assets\_Project\Core\Events\StealthEvents\DetectionEvents.cs",
    "Assets\_Project\Core\Events\StealthEvents\MovementEvents.cs"
)

foreach ($file in $filesToAddData) {
    $filePath = "D:\UnityProjects\URP3D_Base01\$file"
    if (Test-Path $filePath) {
        $content = Get-Content $filePath -Raw

        # Add using Core.Data if not present
        if ($content -notmatch 'using asterivo\.Unity60\.Core\.Data;') {
            # Find the namespace line and add before it
            $content = $content -replace '(namespace asterivo\.Unity60)', "using asterivo.Unity60.Core.Data;`r`n`r`n`$1"
        }

        Set-Content -Path $filePath -Value $content -NoNewline
        Write-Host "  Fixed: $file" -ForegroundColor Cyan
    }
}

# Fix 3: Add missing IInitializable properties to AudioManager
Write-Host "`nStep 3: Fixing AudioManager IInitializable implementation..." -ForegroundColor Yellow

$audioManagerPath = "D:\UnityProjects\URP3D_Base01\Assets\_Project\Core\Audio\AudioManager.cs"
if (Test-Path $audioManagerPath) {
    $content = Get-Content $audioManagerPath -Raw

    # Check if properties are missing
    if ($content -notmatch 'public int Priority') {
        # Add properties after the class declaration and fields
        $insertPoint = $content.IndexOf("// 初期化状態")
        if ($insertPoint -gt 0) {
            $insertion = "`r`n`r`n        #region IInitializable Implementation`r`n`r`n"
            $insertion += "        /// <summary>`r`n"
            $insertion += "        /// 初期化優先度（AudioManagerは早期に初期化）`r`n"
            $insertion += "        /// </summary>`r`n"
            $insertion += "        public int Priority => 10;`r`n`r`n"
            $insertion += "        /// <summary>`r`n"
            $insertion += "        /// 初期化完了状態`r`n"
            $insertion += "        /// </summary>`r`n"
            $insertion += "        public bool IsInitialized => isInitialized;`r`n`r`n"
            $insertion += "        #endregion`r`n`r`n"

            $content = $content.Insert($insertPoint, $insertion)
        }
    }

    Set-Content -Path $audioManagerPath -Value $content -NoNewline
    Write-Host "  Fixed: AudioManager.cs" -ForegroundColor Cyan
}

# Fix 4: Add missing IInitializable properties to AudioService
Write-Host "`nStep 4: Fixing AudioService IInitializable implementation..." -ForegroundColor Yellow

$audioServicePath = "D:\UnityProjects\URP3D_Base01\Assets\_Project\Core\Audio\Services\AudioService.cs"
if (Test-Path $audioServicePath) {
    $content = Get-Content $audioServicePath -Raw

    # Check if properties are missing
    if ($content -notmatch 'public int Priority') {
        # Find a good insertion point
        $insertPoint = $content.IndexOf("private bool isInitialized;")
        if ($insertPoint -gt 0) {
            $insertPoint = $content.IndexOf("`n", $insertPoint) + 1

            $insertion = "`r`n        #region IInitializable Implementation`r`n`r`n"
            $insertion += "        /// <summary>`r`n"
            $insertion += "        /// 初期化優先度`r`n"
            $insertion += "        /// </summary>`r`n"
            $insertion += "        public int Priority => 15;`r`n`r`n"
            $insertion += "        /// <summary>`r`n"
            $insertion += "        /// 初期化完了状態`r`n"
            $insertion += "        /// </summary>`r`n"
            $insertion += "        public bool IsInitialized => isInitialized;`r`n`r`n"
            $insertion += "        #endregion`r`n"

            $content = $content.Insert($insertPoint, $insertion)
        }
    }

    Set-Content -Path $audioServicePath -Value $content -NoNewline
    Write-Host "  Fixed: AudioService.cs" -ForegroundColor Cyan
}

# Fix 5: Fix missing interface implementation in StealthAudioService
Write-Host "`nStep 5: Fixing StealthAudioService interface implementation..." -ForegroundColor Yellow

$stealthAudioPath = "D:\UnityProjects\URP3D_Base01\Assets\_Project\Core\Audio\Services\StealthAudioService.cs"
if (Test-Path $stealthAudioPath) {
    $content = Get-Content $stealthAudioPath -Raw

    # Check if the method is missing
    if ($content -notmatch 'PlayObjectiveCompleteSound') {
        # Find the end of the class to add the method
        $lastBrace = $content.LastIndexOf("}")
        if ($lastBrace -gt 0) {
            $insertion = "`r`n        /// <summary>`r`n"
            $insertion += "        /// 目標達成音の再生`r`n"
            $insertion += "        /// </summary>`r`n"
            $insertion += "        public void PlayObjectiveCompleteSound(bool isMainObjective = false)`r`n"
            $insertion += "        {`r`n"
            $insertion += "            var eventType = isMainObjective ? StealthAudioEventType.MainObjectiveComplete : StealthAudioEventType.ObjectiveComplete;`r`n"
            $insertion += "            PlayAudio(eventType, Vector3.zero, 1.0f);`r`n"
            $insertion += "        }`r`n"

            $content = $content.Insert($lastBrace, $insertion)
        }
    }

    Set-Content -Path $stealthAudioPath -Value $content -NoNewline
    Write-Host "  Fixed: StealthAudioService.cs" -ForegroundColor Cyan
}

# Fix 6: Add missing IStateService and ICommandPoolService interfaces
Write-Host "`nStep 6: Creating missing service interfaces..." -ForegroundColor Yellow

# Create ICommandPoolService if it doesn't exist
$commandPoolServicePath = "D:\UnityProjects\URP3D_Base01\Assets\_Project\Core\Services\Interfaces\ICommandPoolService.cs"
if (-not (Test-Path $commandPoolServicePath)) {
    $commandPoolContent = "namespace asterivo.Unity60.Core.Services.Interfaces`r`n"
    $commandPoolContent += "{`r`n"
    $commandPoolContent += "    /// <summary>`r`n"
    $commandPoolContent += "    /// コマンドプール管理サービスのインターフェース`r`n"
    $commandPoolContent += "    /// </summary>`r`n"
    $commandPoolContent += "    public interface ICommandPoolService : IService`r`n"
    $commandPoolContent += "    {`r`n"
    $commandPoolContent += "        T GetCommand<T>() where T : class, new();`r`n"
    $commandPoolContent += "        void ReturnCommand<T>(T command) where T : class;`r`n"
    $commandPoolContent += "    }`r`n"
    $commandPoolContent += "}`r`n"

    Set-Content -Path $commandPoolServicePath -Value $commandPoolContent -NoNewline
    Write-Host "  Created: ICommandPoolService.cs" -ForegroundColor Cyan
}

Write-Host "`nAll fixes applied!" -ForegroundColor Green
Write-Host "Please run batch compilation to verify all errors are resolved." -ForegroundColor Yellow