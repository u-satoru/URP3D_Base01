# Line Ending Normalization Script - Phase 2: All Markdown files
Write-Host "=== Line Ending Normalization - Phase 2 (All Markdown) ==="

# Phase 2: All Markdown files
$mdFiles = Get-ChildItem -Path "Assets" -Recurse -Include "*.md" -ErrorAction SilentlyContinue

Write-Host "Found $($mdFiles.Count) Markdown files total"

$converted = 0
$unchanged = 0
$errors = 0

foreach ($file in $mdFiles) {
    Write-Host "Processing: $($file.Name)"
    try {
        $content = Get-Content $file.FullName -Raw
        if ($content -ne $null) {
            $originalLength = $content.Length
            $content = $content -replace "`r`n", "`n"
            $newLength = $content.Length
            [System.IO.File]::WriteAllText($file.FullName, $content, [System.Text.Encoding]::UTF8)

            if ($originalLength -ne $newLength) {
                Write-Host "  Converted: $($file.Name) (Size: $originalLength -> $newLength bytes)" -ForegroundColor Green
                $converted++
            } else {
                Write-Host "  No change: $($file.Name)" -ForegroundColor Gray
                $unchanged++
            }
        }
    }
    catch {
        Write-Host "  Error processing $($file.Name): $($_.Exception.Message)" -ForegroundColor Red
        $errors++
    }
}

Write-Host "=== Phase 2 Summary ===" -ForegroundColor Cyan
Write-Host "Converted: $converted files" -ForegroundColor Green
Write-Host "Unchanged: $unchanged files" -ForegroundColor Gray
Write-Host "Errors: $errors files" -ForegroundColor Red
Write-Host "=== Phase 2 Completed ===" -ForegroundColor Cyan