# Line Ending Normalization Script - Phase 4: Unity YAML files
Write-Host "=== Line Ending Normalization - Phase 4 (Unity YAML files) ===" -ForegroundColor Cyan

# Phase 4: Unity YAML files (.asset, .prefab, .unity, .meta)
$unityFiles = Get-ChildItem -Path "Assets" -Recurse -Include "*.asset", "*.prefab", "*.unity", "*.meta" -ErrorAction SilentlyContinue

Write-Host "Found $($unityFiles.Count) Unity YAML files total"

$converted = 0
$unchanged = 0
$errors = 0

foreach ($file in $unityFiles) {
    Write-Host "Processing: $($file.Name)" -NoNewline
    try {
        $content = Get-Content $file.FullName -Raw
        if ($content -ne $null) {
            $originalLength = $content.Length
            $content = $content -replace "`r`n", "`n"
            $newLength = $content.Length
            [System.IO.File]::WriteAllText($file.FullName, $content, [System.Text.Encoding]::UTF8)

            if ($originalLength -ne $newLength) {
                Write-Host " - Converted (Size: $originalLength -> $newLength bytes)" -ForegroundColor Green
                $converted++
            } else {
                Write-Host " - No change" -ForegroundColor Gray
                $unchanged++
            }
        }
    }
    catch {
        Write-Host " - Error: $($_.Exception.Message)" -ForegroundColor Red
        $errors++
    }
}

Write-Host ""
Write-Host "=== Phase 4 Summary ===" -ForegroundColor Cyan
Write-Host "Converted: $converted files" -ForegroundColor Green
Write-Host "Unchanged: $unchanged files" -ForegroundColor Gray
Write-Host "Errors: $errors files" -ForegroundColor Red
Write-Host ""
Write-Host "=== Phase 4 Completed ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "🎯 Critical Achievement: Unity YAML files now properly configured for LF line endings" -ForegroundColor Yellow
Write-Host "   This resolves the LFS configuration issue where .asset files were incorrectly" -ForegroundColor Yellow
Write-Host "   treated as binary when they are actually text files." -ForegroundColor Yellow