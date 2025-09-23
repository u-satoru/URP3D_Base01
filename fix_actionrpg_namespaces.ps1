# ActionRPGの名前空間を修正するスクリプト

$dataFiles = @(
    'D:\UnityProjects\URP3D_Base01\Assets\_Project\Features\Templates\ActionRPG\Scripts\Data\ActionRPGTemplateConfig.cs',
    'D:\UnityProjects\URP3D_Base01\Assets\_Project\Features\Templates\ActionRPG\Scripts\Data\CharacterClassData.cs',
    'D:\UnityProjects\URP3D_Base01\Assets\_Project\Features\Templates\ActionRPG\Scripts\Data\EnemyData.cs',
    'D:\UnityProjects\URP3D_Base01\Assets\_Project\Features\Templates\ActionRPG\Scripts\Data\ItemData.cs',
    'D:\UnityProjects\URP3D_Base01\Assets\_Project\Features\Templates\ActionRPG\Scripts\Data\LevelUpCurveData.cs'
)

$componentFiles = @(
    'D:\UnityProjects\URP3D_Base01\Assets\_Project\Features\Templates\ActionRPG\Scripts\Components\InventoryComponent.cs',
    'D:\UnityProjects\URP3D_Base01\Assets\_Project\Features\Templates\ActionRPG\Scripts\Components\LevelUpShrine.cs',
    'D:\UnityProjects\URP3D_Base01\Assets\_Project\Features\Templates\ActionRPG\Scripts\Components\LootDropComponent.cs',
    'D:\UnityProjects\URP3D_Base01\Assets\_Project\Features\Templates\ActionRPG\Scripts\Components\PlayerCombatController.cs'
)

# Dataファイルの名前空間を修正
foreach ($file in $dataFiles) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
        $content = $content -replace 'namespace asterivo\.Unity60\.Features\.ActionRPG\.Data', 'namespace asterivo.Unity60.Features.Templates.ActionRPG.Data'
        $content = $content -replace 'using asterivo\.Unity60\.Features\.ActionRPG\.Data', 'using asterivo.Unity60.Features.Templates.ActionRPG.Data'
        Set-Content $file $content -NoNewline
        Write-Host "修正完了: $([System.IO.Path]::GetFileName($file))"
    }
}

# Componentファイルの名前空間を修正
foreach ($file in $componentFiles) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
        $content = $content -replace 'namespace asterivo\.Unity60\.Features\.ActionRPG\.Components', 'namespace asterivo.Unity60.Features.Templates.ActionRPG.Components'
        $content = $content -replace 'using asterivo\.Unity60\.Features\.ActionRPG\.Components', 'using asterivo.Unity60.Features.Templates.ActionRPG.Components'
        $content = $content -replace 'using asterivo\.Unity60\.Features\.ActionRPG\.Data', 'using asterivo.Unity60.Features.Templates.ActionRPG.Data'
        $content = $content -replace 'using asterivo\.Unity60\.Features\.ActionRPG\.Managers', 'using asterivo.Unity60.Features.Templates.ActionRPG.Managers'
        Set-Content $file $content -NoNewline
        Write-Host "修正完了: $([System.IO.Path]::GetFileName($file))"
    }
}

Write-Host "すべての名前空間の修正が完了しました。"