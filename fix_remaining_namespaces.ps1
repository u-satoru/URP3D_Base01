# Script to fix remaining namespace references in Core/Services
# Fixes the namespace from Core.Services to Core.Services.Interfaces

$projectPath = "D:\UnityProjects\URP3D_Base01\Assets\_Project"

Write-Host "Scanning for files with incorrect namespace Core.Services..."

# Find all C# files in Core/Services that have wrong namespace
$coreServicesFiles = Get-ChildItem -Path "$projectPath\Core\Services" -Filter "*.cs" -Recurse

foreach ($file in $coreServicesFiles) {
    $content = Get-Content $file.FullName -Raw

    # Check if file has the old namespace
    if ($content -match "namespace asterivo\.Unity60\.Core\.Services(?![.])" -or
        $content -match "namespace asterivo\.Unity60\.Core\.Services$") {

        Write-Host "Found file with incorrect namespace: $($file.Name)"

        # Replace the namespace
        $newContent = $content -replace "namespace asterivo\.Unity60\.Core\.Services(?![.])", "namespace asterivo.Unity60.Core.Services.Interfaces"
        $newContent = $newContent -replace "namespace asterivo\.Unity60\.Core\.Services$", "namespace asterivo.Unity60.Core.Services.Interfaces"

        Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8
        Write-Host "  Fixed: $($file.Name)" -ForegroundColor Green
    }
}

Write-Host "`nScanning for files with incorrect using statements..."

# Find all C# files that have wrong using statements
$allCsFiles = Get-ChildItem -Path $projectPath -Filter "*.cs" -Recurse

$fixedCount = 0
foreach ($file in $allCsFiles) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content

    # Check if file has the old using statement
    if ($content -match "using asterivo\.Unity60\.Core\.Services(?![.])" -or
        $content -match "using asterivo\.Unity60\.Core\.Services;") {

        # Replace the using statement
        $content = $content -replace "using asterivo\.Unity60\.Core\.Services(?![.])", "using asterivo.Unity60.Core.Services.Interfaces"
        $content = $content -replace "using asterivo\.Unity60\.Core\.Services;", "using asterivo.Unity60.Core.Services.Interfaces;"

        if ($content -ne $originalContent) {
            Set-Content -Path $file.FullName -Value $content -Encoding UTF8
            Write-Host "Fixed using statement in: $($file.Name)" -ForegroundColor Green
            $fixedCount++
        }
    }
}

Write-Host "`nFixed $fixedCount files with incorrect using statements"
Write-Host "Namespace correction complete!" -ForegroundColor Cyan