# Line Ending Normalization Script for Unity Project
# Phase 1: Test with small scope (Test directory only)

Write-Host "=== Line Ending Normalization - Phase 1 (Test) ==="

# Phase 1: Test directory only
$testFiles = Get-ChildItem -Path "Assets/_Project/Tests" -Recurse -Include "*.md" -ErrorAction SilentlyContinue

Write-Host "Found $($testFiles.Count) Markdown files in Tests directory"

foreach ($file in $testFiles) {
    Write-Host "Processing: $($file.Name)"
    try {
        $content = Get-Content $file.FullName -Raw
        if ($content -ne $null) {
            $originalLength = $content.Length
            $content = $content -replace "`r`n", "`n"
            $newLength = $content.Length
            [System.IO.File]::WriteAllText($file.FullName, $content, [System.Text.Encoding]::UTF8)
            Write-Host "  Converted: $($file.Name) (Size: $originalLength -> $newLength bytes)"
        }
    }
    catch {
        Write-Host "  Error processing $($file.Name): $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "=== Phase 1 Completed ==="