# ==============================================================================
# Unity Project Line Ending Normalization Script (Safe Version)
#
# Description:
#   This script safely converts line endings of text files in the Unity project
#   to LF (Unix-style) and ensures the file encoding is UTF-8 without BOM.
#
# Features:
#   - Creates a backup (.bak) of each file before modification.
#   - Processes only specified text file extensions.
#   - Skips binary-like Unity asset files to prevent corruption.
#   - Skips files in ThirdParty and Plugins directories.
#   - Only writes to files if a change in line endings is detected.
#   - Provides detailed logging of actions.
# ==============================================================================

Write-Host "=== Starting Line Ending Normalization (Safe Version) ===" -ForegroundColor Green

# --- Configuration ---
$projectRoot = $PSScriptRoot
$targetDirectory = Join-Path $projectRoot "Assets/_Project"
$backupEnabled = $true # Set to $false to disable backup creation

# Extensions to process (add or remove as needed)
$targetExtensions = @(
    "*.cs",
    "*.md",
    "*.json",
    "*.txt",
    "*.xml",
    "*.yml",
    "*.yaml",
    "*.shader",
    "*.cginc",
    "*.asmdef"
)

# Directories to exclude from processing
$excludeDirs = @(
    "Assets/_Project/ThirdParty",
    "Assets/Plugins" # Also exclude the root Plugins folder
)

# --- Script Body ---
$filesToProcess = Get-ChildItem -Path $targetDirectory -Recurse -Include $targetExtensions -File | Where-Object {
    $shouldExclude = $false
    foreach ($dir in $excludeDirs) {
        if ($_.FullName.StartsWith((Join-Path $projectRoot $dir))) {
            $shouldExclude = $true
            break
        }
    }
    -not $shouldExclude
}

$totalFiles = $filesToProcess.Count
$processedCount = 0
$changedCount = 0
$errorCount = 0

Write-Host "Found $totalFiles files to check."

foreach ($file in $filesToProcess) {
    $processedCount++
    $relativePath = $file.FullName.Substring($projectRoot.Length)
    Write-Host "($processedCount/$totalFiles) Checking: $relativePath"

    try {
        # Read file content as a single string
        $content = Get-Content $file.FullName -Raw -Encoding Default
        
        # Check if the file is empty or read failed
        if ([string]::IsNullOrEmpty($content)) {
            Write-Host "  -> Skipped (File is empty or could not be read)." -ForegroundColor Yellow
            continue
        }

        # Create backup if enabled
        if ($backupEnabled) {
            $backupPath = "$($file.FullName).bak"
            Copy-Item -Path $file.FullName -Destination $backupPath -Force
        }

        # Normalize line endings (CRLF -> LF)
        $normalizedContent = $content -replace "`r`n", "`n"

        # Write back to the file ONLY if content has changed
        if ($normalizedContent -ne $content) {
            $changedCount++
            # Use UTF8 without BOM encoding
            $utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)
            [System.IO.File]::WriteAllText($file.FullName, $normalizedContent, $utf8WithoutBom)
            Write-Host "  -> CONVERTED: Line endings normalized." -ForegroundColor Cyan
        } else {
            Write-Host "  -> OK: No conversion needed."
        }
    }
    catch {
        $errorCount++
        Write-Host "  -> ERROR processing $($file.Name): $($_.Exception.Message)" -ForegroundColor Red
        # Restore from backup if an error occurred during processing
        if ($backupEnabled) {
            $backupPath = "$($file.FullName).bak"
            if (Test-Path $backupPath) {
                Copy-Item -Path $backupPath -Destination $file.FullName -Force
                Remove-Item -Path $backupPath
                Write-Host "  -> RESTORED from backup due to error." -ForegroundColor Magenta
            }
        }
    }
}

Write-Host ""
Write-Host "=== Normalization Complete ===" -ForegroundColor Green
Write-Host "Total files checked: $totalFiles"
Write-Host "Files modified: $changedCount"
Write-Host "Errors encountered: $errorCount"

if ($backupEnabled -and $changedCount -gt 0) {
    Write-Host ""
    Write-Host "Backup files (.bak) have been created for all modified files." -ForegroundColor Yellow
    Write-Host "Please verify the changes. Once confirmed, you can delete the .bak files." -ForegroundColor Yellow
}
