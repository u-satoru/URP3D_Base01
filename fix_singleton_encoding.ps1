# Fix encoding for SingletonDisableScheduler.cs
$file = "Assets\_Project\Core\Services\SingletonDisableScheduler.cs"

# Read file with UTF-8
$content = Get-Content $file -Raw -Encoding UTF8

# Convert CRLF to LF
$content = $content -replace "`r`n", "`n"

# Save with UTF-8 without BOM
[System.IO.File]::WriteAllText((Resolve-Path $file).Path, $content, [System.Text.UTF8Encoding]::new($false))

Write-Host "File converted to UTF-8 with LF line endings: $file" -ForegroundColor Green