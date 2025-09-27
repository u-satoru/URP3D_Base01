#!/usr/bin/env pwsh

# All Tests Execution Script
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "All Tests Validation" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$projectPath = "D:\UnityProjects\URP3D_Base01"
$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.0.42f1\Editor\Unity.exe"

# Check Unity installation
if (!(Test-Path $unityPath)) {
    Write-Host "Unity not found at: $unityPath" -ForegroundColor Red
    Write-Host "Attempting to find Unity in default installation path..." -ForegroundColor Yellow
    $unityPath = Get-ChildItem "C:\Program Files\Unity\Hub\Editor\" -Filter "Unity.exe" -Recurse | Select-Object -First 1 -ExpandProperty FullName
    if ($unityPath) {
        Write-Host "Found Unity at: $unityPath" -ForegroundColor Green
    }
    else {
        Write-Host "Unity installation not found. Please install Unity 6000.0.42f1" -ForegroundColor Red
        exit 1
    }
}

# Test output directory
$testResultsPath = "$projectPath\Tests\Results"
if (!(Test-Path $testResultsPath)) {
    New-Item -ItemType Directory -Path $testResultsPath -Force
}

# Test result files
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$testLogFile = "$testResultsPath\all-tests-$timestamp.log"
$xmlResultFile = "$testResultsPath\all-tests-results-$timestamp.xml"

Write-Host "`n[1] Running All Unit Tests..." -ForegroundColor Yellow

# Run Unity tests
$arguments = @(
    "-batchmode",
    "-projectPath",
    "`"$projectPath`"",
    "-runTests",
    "-testPlatform",
    "EditMode",
    "-testResults",
    "`"$xmlResultFile`"",
    "-logFile",
    "`"$testLogFile`""
)

Write-Host "Executing: $unityPath $arguments" -ForegroundColor Gray
Start-Process -FilePath $unityPath -ArgumentList $arguments -Wait -NoNewWindow

# Wait for test completion
Start-Sleep -Seconds 5

# Check test results
Write-Host "`n[2] Analyzing Test Results..." -ForegroundColor Yellow

if (Test-Path $xmlResultFile) {
    Write-Host "Test results saved to: $xmlResultFile" -ForegroundColor Green

    # Parse XML results
    [xml]$testResults = Get-Content $xmlResultFile
    $totalTests = $testResults.'test-run'.total
    $passed = $testResults.'test-run'.passed
    $failed = $testResults.'test-run'.failed
    $skipped = $testResults.'test-run'.skipped

    Write-Host "`nTest Summary:" -ForegroundColor Cyan
    Write-Host "  Total Tests: $totalTests" -ForegroundColor White
    Write-Host "  Passed: $passed" -ForegroundColor Green
    Write-Host "  Failed: $failed" -ForegroundColor $(if ($failed -eq "0") { "Gray" } else { "Red" })
    Write-Host "  Skipped: $skipped" -ForegroundColor Yellow

    if ($failed -ne "0") {
        Write-Host "`nFailed Tests:" -ForegroundColor Red
        $testResults.'test-run'..'test-case' | Where-Object { $_.result -eq "Failed" } | ForEach-Object {
            Write-Host "  - $($_.name)" -ForegroundColor Red
            if ($_.failure) {
                Write-Host "    $($_.failure.message)" -ForegroundColor DarkRed
            }
        }
    }
}
else {
    Write-Host "XML test results not found. Checking log file..." -ForegroundColor Yellow

    if (Test-Path $testLogFile) {
        Write-Host "Log file: $testLogFile" -ForegroundColor Gray

        # Extract test results from log
        $logContent = Get-Content $testLogFile -Raw

        # Check for compilation errors
        $compilationErrors = $logContent | Select-String -Pattern "CS\d{4}:|error CS|Compilation failed"
        if ($compilationErrors) {
            Write-Host "`nCompilation Errors Found:" -ForegroundColor Red
            $compilationErrors | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        }

        # Check for test execution
        $testExecution = $logContent | Select-String -Pattern "Test run completed|Tests run:"
        if ($testExecution) {
            Write-Host "`nTest Execution Info:" -ForegroundColor Cyan
            $testExecution | ForEach-Object { Write-Host $_ -ForegroundColor White }
        }
    }
}

Write-Host "`n[3] Creating Test Report..." -ForegroundColor Yellow

# Create markdown report
$reportFile = "$testResultsPath\all-tests-report-$timestamp.md"
@"
# All Tests Validation Report

## Date: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

## Test Execution Summary

- **Project Path**: $projectPath
- **Unity Version**: 6000.0.42f1
- **Test Framework**: Unity Test Runner (EditMode)

## Test Results

$(if (Test-Path $xmlResultFile) {
@"
### Statistics
- Total Tests: $totalTests
- Passed: $passed
- Failed: $failed
- Skipped: $skipped
"@
} else {
@"
### Test Execution Status
- XML results not generated
- Please check log file for details: $testLogFile
"@
})

## Log Files
- Test Log: $testLogFile
- XML Results: $xmlResultFile
"@ | Out-File -FilePath $reportFile -Encoding UTF8

Write-Host "Test report created: $reportFile" -ForegroundColor Green

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Test Validation Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
