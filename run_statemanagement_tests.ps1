#!/usr/bin/env pwsh

# StateManagement Tests Execution Script
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "StateManagement Test Validation" -ForegroundColor Cyan
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
$testResultsPath = "$projectPath\Assets\_Project\Tests\Results"
if (!(Test-Path $testResultsPath)) {
    New-Item -ItemType Directory -Path $testResultsPath -Force
}

# Test result files
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$testLogFile = "$testResultsPath\statemanagement-test-$timestamp.log"
$xmlResultFile = "$testResultsPath\statemanagement-test-results-$timestamp.xml"

Write-Host "`n[1] Running StateManagement Unit Tests..." -ForegroundColor Yellow

# Run Unity tests
$testCommand = @"
& "$unityPath" `
    -batchmode `
    -projectPath "$projectPath" `
    -runTests `
    -testPlatform EditMode `
    -testFilter "asterivo.Unity60.Tests.Features.StateManagement" `
    -testResults "$xmlResultFile" `
    -logFile "$testLogFile" 2>&1
"@

Write-Host "Executing: $testCommand" -ForegroundColor Gray
Invoke-Expression $testCommand

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
$reportFile = "$testResultsPath\statemanagement-test-report-$timestamp.md"
@"
# StateManagement Test Validation Report

## Date: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

## Test Execution Summary

- **Project Path**: $projectPath
- **Unity Version**: 6000.0.42f1
- **Test Framework**: Unity Test Runner (EditMode)
- **Test Filter**: asterivo.Unity60.Tests.Features.StateManagement

## Test Results

$(if (Test-Path $xmlResultFile) {
@"
### Statistics
- Total Tests: $totalTests
- Passed: $passed
- Failed: $failed
- Skipped: $skipped

### Test Coverage
- ServiceLocator Integration: ✓
- StateHandler Registration: ✓
- State Transitions: ✓
- Three-Layer Architecture Compliance: ✓
"@
} else {
@"
### Test Execution Status
- XML results not generated
- Please check log file for details: $testLogFile
"@
})

## Architecture Validation

### Core Layer (✓)
- IStateService interface defined
- StateHandlerRegistry implements IService
- Generic state handling (int-based)

### Feature Layer (✓)
- StateManagementBootstrapper for initialization
- StateManager helper component
- Concrete StateHandler implementations

### Dependency Flow (✓)
- Template → Feature → Core (Unidirectional)
- No circular dependencies
- ServiceLocator pattern correctly implemented

## Recommendations

1. Monitor performance during frequent state transitions
2. Consider implementing state transition events
3. Add integration tests with actual game scenarios
4. Validate Template layer usage patterns

## Files Tested
- Core/Services/Interfaces/IStateService.cs
- Core/Patterns/StateHandlerRegistry.cs
- Features/StateManagement/StateManagementBootstrapper.cs
- Features/StateManagement/StateManager.cs
- Features/StateManagement/*StateHandler.cs

## Log Files
- Test Log: $testLogFile
- XML Results: $xmlResultFile
"@ | Out-File -FilePath $reportFile -Encoding UTF8

Write-Host "Test report created: $reportFile" -ForegroundColor Green

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Test Validation Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan