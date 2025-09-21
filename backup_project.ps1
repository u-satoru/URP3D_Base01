# Unity Project Backup Script
# Date: 2025-09-22

$timestamp = Get-Date -Format "yyyyMMdd_HHmm"
$backupPath = "../URP3D_Base01_Backup_$timestamp"

Write-Host "Creating backup at: $backupPath" -ForegroundColor Green

# Create backup directory
New-Item -Path $backupPath -ItemType Directory -Force | Out-Null

# Define folders to backup
$folders = @('Assets', 'Packages', 'ProjectSettings')

foreach ($folder in $folders) {
    if (Test-Path $folder) {
        Write-Host "Copying $folder..." -ForegroundColor Yellow
        Copy-Item -Path $folder -Destination $backupPath -Recurse -Force
    }
}

# Copy root files
$rootFiles = Get-ChildItem -Path . -File | Where-Object {
    $_.Extension -in @('.md', '.json', '.txt', '.yml', '.yaml', '.meta', '.asmdef')
}

foreach ($file in $rootFiles) {
    Write-Host "Copying $($file.Name)..." -ForegroundColor Yellow
    Copy-Item -Path $file.FullName -Destination $backupPath -Force
}

Write-Host "Backup completed successfully!" -ForegroundColor Green
Write-Host "Backup location: $backupPath" -ForegroundColor Cyan