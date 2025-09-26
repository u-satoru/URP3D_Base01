# Script to fix interface namespaces - move them to Core namespace
# This avoids circular dependency between Core and Core.Services

$interfacePath = "D:\UnityProjects\URP3D_Base01\Assets\_Project\Core\Services\Interfaces"
$projectPath = "D:\UnityProjects\URP3D_Base01\Assets\_Project"

Write-Host "Fixing interface namespaces to avoid circular dependency..." -ForegroundColor Cyan

# Step 1: Change namespace in interface files from Core.Services.Interfaces to Core
Write-Host "`nStep 1: Changing namespaces in interface files..."
$interfaceFiles = Get-ChildItem -Path $interfacePath -Filter "*.cs" -Recurse

foreach ($file in $interfaceFiles) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content

    # Change namespace declaration
    $content = $content -replace "namespace asterivo\.Unity60\.Core\.Services\.Interfaces", "namespace asterivo.Unity60.Core"

    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
        Write-Host "  Fixed namespace in: $($file.Name)" -ForegroundColor Green
    }
}

# Step 2: Update all using statements throughout the project
Write-Host "`nStep 2: Updating using statements throughout the project..."
$allCsFiles = Get-ChildItem -Path $projectPath -Filter "*.cs" -Recurse

$fixedCount = 0
foreach ($file in $allCsFiles) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content

    # Change using statements
    $content = $content -replace "using asterivo\.Unity60\.Core\.Services\.Interfaces;", "using asterivo.Unity60.Core;"

    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
        Write-Host "  Fixed using statement in: $($file.Name)" -ForegroundColor Green
        $fixedCount++
    }
}

Write-Host "`nFixed $fixedCount files with using statements"
Write-Host "Interface namespace fix complete!" -ForegroundColor Cyan