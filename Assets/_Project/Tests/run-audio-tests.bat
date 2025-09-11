@echo off
REM Audio System Test Runner for CI/CD
REM Week 2 テストインフラ構築 - バッチ実行スクリプト

echo [Audio Test Runner] Starting Unity Test Runner for Audio System Tests...

REM Unity Editor Path (adjust as needed)
set UNITY_PATH="C:\Program Files\Unity\Hub\Editor\6000.0.42f1\Editor\Unity.exe"

REM Project Path
set PROJECT_PATH=%~dp0..\..\..

REM Results Directory
set RESULTS_DIR=%PROJECT_PATH%\Assets\_Project\Tests\Results

REM Create results directory if it doesn't exist
if not exist "%RESULTS_DIR%" (
    mkdir "%RESULTS_DIR%"
)

echo [Audio Test Runner] Running Unity Test Runner in batch mode...

REM Run tests and generate XML results
%UNITY_PATH% -projectPath "%PROJECT_PATH%" -batchmode -runTests -testResults "%RESULTS_DIR%\audio-system-test-results.xml" -logFile "%RESULTS_DIR%\audio-system-test-log.txt" -quit

REM Check if Unity test run was successful
if %ERRORLEVEL% EQU 0 (
    echo [Audio Test Runner] ✅ Unity Test Runner completed successfully
    echo [Audio Test Runner] Results saved to: %RESULTS_DIR%\audio-system-test-results.xml
    echo [Audio Test Runner] Log saved to: %RESULTS_DIR%\audio-system-test-log.txt
) else (
    echo [Audio Test Runner] ❌ Unity Test Runner failed with error code %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)

echo [Audio Test Runner] Audio System Test execution completed.
echo [Audio Test Runner] Check the Results directory for XML and Markdown reports.

pause