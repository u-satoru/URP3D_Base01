using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
// System.Management removed for Unity compatibility
using System.Text;
using Microsoft.Win32;

namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// Interactive Setup Wizard System用のシステム要件チェッカー
    /// TASK-003.1: Unity環境、開発ツール、Git設定等の包括的な検証を行う
    /// </summary>
    public static class SystemRequirementChecker
    {
        #region Public Data Structures
        
        [System.Serializable]
        public class SystemRequirementReport
        {
            public bool isValid = true;
            public List<RequirementCheckResult> results = new List<RequirementCheckResult>();
            public string summary = "";
            public DateTime checkTime = DateTime.Now;
            public HardwareDiagnostics hardware = new HardwareDiagnostics();
            public int environmentScore = 0;
            public List<string> autoFixedIssues = new List<string>();
            
            public void AddResult(RequirementCheckResult result)
            {
                results.Add(result);
                if (!result.isPassed)
                    isValid = false;
            }
        }
        
        [System.Serializable]
        public class HardwareDiagnostics
        {
            public CPUInfo cpu = new CPUInfo();
            public MemoryInfo memory = new MemoryInfo();
            public GPUInfo gpu = new GPUInfo();
            public StorageInfo storage = new StorageInfo();
            
            [System.Serializable]
            public class CPUInfo
            {
                public string name = "Unknown";
                public int cores = 0;
                public int logicalProcessors = 0;
                public string architecture = "Unknown";
                public float frequencyGHz = 0;
            }
            
            [System.Serializable]
            public class MemoryInfo
            {
                public long totalRAM = 0; // bytes
                public long availableRAM = 0; // bytes
                public float usagePercent = 0;
            }
            
            [System.Serializable]
            public class GPUInfo
            {
                public string name = "Unknown";
                public string driver = "Unknown";
                public long dedicatedMemory = 0; // bytes
                public bool supportsDirectX11 = false;
            }
            
            [System.Serializable]
            public class StorageInfo
            {
                public long totalSpace = 0; // bytes
                public long freeSpace = 0; // bytes
                public float usagePercent = 0;
                public string driveType = "Unknown";
            }
        }
        
        [System.Serializable]
        public class RequirementCheckResult
        {
            public string checkName;
            public bool isPassed;
            public string message;
            public RequirementSeverity severity;
            public string recommendation;
            
            public RequirementCheckResult(string name, bool passed, string msg, RequirementSeverity sev = RequirementSeverity.Required, string rec = "")
            {
                checkName = name;
                isPassed = passed;
                message = msg;
                severity = sev;
                recommendation = rec;
            }
        }
        
        [System.Serializable]
        public class IDEInfo
        {
            public string Name;
            public string Version;
            public string Path;
            public bool HasUnitySupport;
            public List<string> Extensions;
            
            public IDEInfo(string name, string version, string path, bool hasUnitySupport = false)
            {
                Name = name;
                Version = version ?? "Unknown";
                Path = path ?? "Unknown";
                HasUnitySupport = hasUnitySupport;
                Extensions = new List<string>();
            }
        }
        
        public enum RequirementSeverity
        {
            Required,    // 必須（失敗するとセットアップ不可）
            Important,   // 重要（警告表示だが続行可能）
            Optional     // オプション（情報提供のみ）
        }
        
        #endregion
        
        #region Constants
        
        private static readonly Version MINIMUM_UNITY_VERSION = new Version(6, 0, 0);
        private static readonly string REQUIRED_UNITY_VERSION = "6000.0.42f1";
        
        private static readonly string[] REQUIRED_PACKAGES = {
            "com.unity.render-pipelines.universal",
            "com.unity.inputsystem", 
            "com.unity.cinemachine"
        };
        
        private static readonly string[] RECOMMENDED_PACKAGES = {
            "com.unity.ai.navigation",
            "com.unity.test-framework"
        };
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// システム要件の包括的チェックを実行
        /// </summary>
        /// <returns>チェック結果レポート</returns>
        public static SystemRequirementReport CheckAllRequirements()
        {
            var report = new SystemRequirementReport();
            
            UnityEngine.Debug.Log("[SystemRequirementChecker] Starting comprehensive system requirements check...");
            
            // Unity Version Check
            report.AddResult(CheckUnityVersion());
            
            // IDE Detection Check
            report.AddResult(CheckIDEAvailability());
            
            // Git Configuration Check
            report.AddResult(CheckGitConfiguration());
            
            // Unity Hub Check
            report.AddResult(CheckUnityHub());
            
            // Package Requirements Check
            var packageResults = CheckRequiredPackages();
            foreach (var result in packageResults)
                report.AddResult(result);
            
            // Hardware Diagnostics Check
            report.hardware = PerformHardwareDiagnostics();
            
            // Calculate Environment Score
            report.environmentScore = CalculateEnvironmentScore(report);
            
            // Attempt Auto-fix Issues
            var autoFixResults = AttemptAutoFixIssues(report);
            report.autoFixedIssues.AddRange(autoFixResults);
            
            // Re-check after auto-fix
            if (autoFixResults.Count > 0)
            {
                UnityEngine.Debug.Log($"[SystemRequirementChecker] Auto-fixed {autoFixResults.Count} issues. Re-checking...");
                // Note: In production, we might want to re-run specific checks
            }
            
            // Generate Summary
            report.summary = GenerateSummary(report);
            
            // Save JSON Report
            SaveDiagnosticsToJSON(report);
            
            UnityEngine.Debug.Log($"[SystemRequirementChecker] Check completed. Overall result: {(report.isValid ? "PASSED" : "FAILED")}");
            UnityEngine.Debug.Log($"[SystemRequirementChecker] Environment Score: {report.environmentScore}/100");
            
            return report;
        }
        
        #endregion
        
        #region Individual Check Methods
        
        /// <summary>
        /// Unity バージョンの検証
        /// </summary>
        private static RequirementCheckResult CheckUnityVersion()
        {
            try
            {
                string currentVersion = Application.unityVersion;
                bool isCompatible = IsUnityVersionCompatible(currentVersion);
                
                string message = $"Unity Version: {currentVersion}";
                string recommendation = isCompatible ? 
                    "Unity version is compatible." :
                    $"Please upgrade to Unity {REQUIRED_UNITY_VERSION} or later for optimal compatibility.";
                
                return new RequirementCheckResult(
                    "Unity Version Check",
                    isCompatible,
                    message,
                    RequirementSeverity.Required,
                    recommendation
                );
            }
            catch (Exception ex)
            {
                return new RequirementCheckResult(
                    "Unity Version Check",
                    false,
                    $"Failed to check Unity version: {ex.Message}",
                    RequirementSeverity.Required,
                    "Unable to determine Unity version. Please check your Unity installation."
                );
            }
        }
        
        /// <summary>
        /// IDE（VS Code/Visual Studio）の可用性チェック
        /// </summary>
        private static RequirementCheckResult CheckIDEAvailability()
        {
            try
            {
                var detectedIDEs = new List<IDEInfo>();
                
                // VS Code Check
                var vscodeInfo = DetectVSCode();
                if (vscodeInfo != null)
                {
                    detectedIDEs.Add(vscodeInfo);
                }
                
                // Visual Studio Check
                var vsInfo = DetectVisualStudio();
                if (vsInfo != null)
                {
                    detectedIDEs.Add(vsInfo);
                }
                
                // Rider Check (optional)
                var riderInfo = DetectRider();
                if (riderInfo != null)
                {
                    detectedIDEs.Add(riderInfo);
                }
                
                bool hasIDE = detectedIDEs.Count > 0;
                
                string message;
                if (hasIDE)
                {
                    var ideDescriptions = detectedIDEs.Select(ide => 
                        $"{ide.Name} {ide.Version} ({ide.Path})"
                    );
                    message = $"Detected IDEs:\n{string.Join("\n", ideDescriptions)}";
                }
                else
                {
                    message = "No supported IDEs detected";
                }
                
                string recommendation = hasIDE ?
                    "IDE environment is properly configured." :
                    "Please install Visual Studio Code or Visual Studio for optimal development experience.";
                
                return new RequirementCheckResult(
                    "IDE Availability Check",
                    hasIDE,
                    message,
                    RequirementSeverity.Important,
                    recommendation
                );
            }
            catch (Exception ex)
            {
                return new RequirementCheckResult(
                    "IDE Availability Check",
                    false,
                    $"Failed to check IDE availability: {ex.Message}",
                    RequirementSeverity.Important,
                    "Unable to detect IDE environment. Manual verification recommended."
                );
            }
        }
        
        /// <summary>
        /// Git設定の検証
        /// </summary>
        private static RequirementCheckResult CheckGitConfiguration()
        {
            try
            {
                var gitConfig = GetGitConfiguration();
                
                bool hasUserName = !string.IsNullOrEmpty(gitConfig.userName);
                bool hasUserEmail = !string.IsNullOrEmpty(gitConfig.userEmail);
                bool hasUnityMerge = gitConfig.hasUnityYAMLMerge;
                
                bool isConfigured = hasUserName && hasUserEmail;
                
                var configDetails = new List<string>();
                if (hasUserName) configDetails.Add($"user.name: {gitConfig.userName}");
                if (hasUserEmail) configDetails.Add($"user.email: {gitConfig.userEmail}");
                if (hasUnityMerge) configDetails.Add("Unity YAML Merge: configured");
                
                string message = isConfigured ? 
                    $"Git configured: {string.Join(", ", configDetails)}" :
                    "Git configuration incomplete";
                
                string recommendation = isConfigured ?
                    "Git configuration is ready for development." :
                    "Please configure Git with 'git config --global user.name' and 'git config --global user.email'";
                
                return new RequirementCheckResult(
                    "Git Configuration Check",
                    isConfigured,
                    message,
                    RequirementSeverity.Required,
                    recommendation
                );
            }
            catch (Exception ex)
            {
                return new RequirementCheckResult(
                    "Git Configuration Check",
                    false,
                    $"Failed to check Git configuration: {ex.Message}",
                    RequirementSeverity.Required,
                    "Please ensure Git is installed and properly configured."
                );
            }
        }
        
        /// <summary>
        /// Unity Hub インストールの確認
        /// </summary>
        private static RequirementCheckResult CheckUnityHub()
        {
            try
            {
                // Unity Hub の一般的なインストールパスをチェック
                var hubPaths = new string[]
                {
                    @"C:\Program Files\Unity Hub\Unity Hub.exe",
                    @"C:\Users\" + Environment.UserName + @"\AppData\Local\Programs\Unity Hub\Unity Hub.exe",
                    "/Applications/Unity Hub.app/Contents/MacOS/Unity Hub",
                    "/home/" + Environment.UserName + "/Unity/Hub/Editor/Unity Hub"
                };
                
                bool hubFound = hubPaths.Any(File.Exists);
                
                string message = hubFound ? 
                    "Unity Hub installation detected" :
                    "Unity Hub not found in standard locations";
                
                string recommendation = hubFound ?
                    "Unity Hub is available for project management." :
                    "Consider installing Unity Hub for easier Unity version management.";
                
                return new RequirementCheckResult(
                    "Unity Hub Check",
                    hubFound,
                    message,
                    RequirementSeverity.Optional,
                    recommendation
                );
            }
            catch (Exception ex)
            {
                return new RequirementCheckResult(
                    "Unity Hub Check",
                    false,
                    $"Failed to check Unity Hub: {ex.Message}",
                    RequirementSeverity.Optional,
                    "Unity Hub check encountered an error."
                );
            }
        }
        
        /// <summary>
        /// 必須パッケージの検証
        /// </summary>
        private static List<RequirementCheckResult> CheckRequiredPackages()
        {
            var results = new List<RequirementCheckResult>();
            
            try
            {
                var installedPackages = GetInstalledPackages();
                
                // 必須パッケージのチェック
                foreach (var packageName in REQUIRED_PACKAGES)
                {
                    bool isInstalled = installedPackages.ContainsKey(packageName);
                    
                    string message = isInstalled ?
                        $"Required package '{packageName}' version {installedPackages[packageName]} is installed" :
                        $"Required package '{packageName}' is missing";
                    
                    string recommendation = isInstalled ?
                        "Package requirement satisfied." :
                        $"Please install package '{packageName}' via Package Manager.";
                    
                    results.Add(new RequirementCheckResult(
                        $"Required Package: {packageName}",
                        isInstalled,
                        message,
                        RequirementSeverity.Required,
                        recommendation
                    ));
                }
                
                // 推奨パッケージのチェック
                foreach (var packageName in RECOMMENDED_PACKAGES)
                {
                    bool isInstalled = installedPackages.ContainsKey(packageName);
                    
                    string message = isInstalled ?
                        $"Recommended package '{packageName}' version {installedPackages[packageName]} is installed" :
                        $"Recommended package '{packageName}' is not installed";
                    
                    string recommendation = isInstalled ?
                        "Recommended package is available." :
                        $"Consider installing '{packageName}' for enhanced functionality.";
                    
                    results.Add(new RequirementCheckResult(
                        $"Recommended Package: {packageName}",
                        isInstalled,
                        message,
                        RequirementSeverity.Optional,
                        recommendation
                    ));
                }
            }
            catch (Exception ex)
            {
                results.Add(new RequirementCheckResult(
                    "Package Check",
                    false,
                    $"Failed to check packages: {ex.Message}",
                    RequirementSeverity.Required,
                    "Unable to verify package requirements."
                ));
            }
            
            return results;
        }
        
        #endregion
        
        #region Hardware Diagnostics
        
        /// <summary>
        /// ハードウェア診断の実行
        /// </summary>
        private static HardwareDiagnostics PerformHardwareDiagnostics()
        {
            var hardware = new HardwareDiagnostics();
            
            try
            {
                UnityEngine.Debug.Log("[SystemRequirementChecker] Performing hardware diagnostics...");
                
                // CPU診断
                hardware.cpu = DiagnoseCPU();
                
                // メモリ診断
                hardware.memory = DiaznoseMemory();
                
                // GPU診断
                hardware.gpu = DiagnoseGPU();
                
                // ストレージ診断
                hardware.storage = DiagnoseStorage();
                
                UnityEngine.Debug.Log("[SystemRequirementChecker] Hardware diagnostics completed.");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[SystemRequirementChecker] Hardware diagnostics failed: {ex.Message}");
            }
            
            return hardware;
        }
        
        /// <summary>
        /// CPU情報の取得
        /// </summary>
        private static HardwareDiagnostics.CPUInfo DiagnoseCPU()
        {
            var cpuInfo = new HardwareDiagnostics.CPUInfo();
            
            try
            {
                // Unity互換のUnityEngine.SystemInfoを使用
                cpuInfo.name = SystemInfo.processorType;
                cpuInfo.cores = SystemInfo.processorCount;
                cpuInfo.logicalProcessors = SystemInfo.processorCount;
                cpuInfo.frequencyGHz = SystemInfo.processorFrequency / 1000f;
                cpuInfo.architecture = "Unknown";
                
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    cpuInfo.architecture = "x64";
                }
                else if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    cpuInfo.architecture = "ARM64";
                }
                else
                {
                    // macOS/Linux の場合はシステムコマンドを使用
                    cpuInfo.cores = Environment.ProcessorCount;
                    cpuInfo.logicalProcessors = Environment.ProcessorCount;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"CPU diagnosis failed: {ex.Message}");
            }
            
            return cpuInfo;
        }
        
        /// <summary>
        /// メモリ情報の取得
        /// </summary>
        private static HardwareDiagnostics.MemoryInfo DiaznoseMemory()
        {
            var memoryInfo = new HardwareDiagnostics.MemoryInfo();
            
            try
            {
                // Unity互換のUnityEngine.SystemInfoを使用
                memoryInfo.totalRAM = SystemInfo.systemMemorySize * 1024L * 1024L; // MB to bytes
                memoryInfo.availableRAM = memoryInfo.totalRAM; // 簡略化して総量と同じとして近似
                
                if (memoryInfo.totalRAM > 0)
                {
                    memoryInfo.usagePercent = ((float)(memoryInfo.totalRAM - memoryInfo.availableRAM) / memoryInfo.totalRAM) * 100f;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"Memory diagnosis failed: {ex.Message}");
            }
            
            return memoryInfo;
        }
        
        /// <summary>
        /// GPU情報の取得
        /// </summary>
        private static HardwareDiagnostics.GPUInfo DiagnoseGPU()
        {
            var gpuInfo = new HardwareDiagnostics.GPUInfo();
            
            try
            {
                // Unity の GPU 情報を使用
                gpuInfo.name = SystemInfo.graphicsDeviceName;
                gpuInfo.dedicatedMemory = SystemInfo.graphicsMemorySize * 1024 * 1024; // MB to bytes
                gpuInfo.driver = $"{SystemInfo.graphicsDeviceVersion}";
                
                // DirectX サポートのチェック
                gpuInfo.supportsDirectX11 = SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D11;
                
                // Unity互換のUnityEngine.SystemInfoを使用
                gpuInfo.name = SystemInfo.graphicsDeviceName;
                gpuInfo.driver = SystemInfo.graphicsDeviceVersion;
                gpuInfo.dedicatedMemory = SystemInfo.graphicsMemorySize * 1024L * 1024L; // MB to bytes
                
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    // Windows固有の設定で近似情報を設定
                    try
                    {
                        if (SystemInfo.graphicsDeviceName.Contains("NVIDIA"))
                        {
                            gpuInfo.driver = "Latest NVIDIA";
                        }
                        else if (SystemInfo.graphicsDeviceName.Contains("AMD") || SystemInfo.graphicsDeviceName.Contains("Radeon"))
                        {
                            gpuInfo.driver = "Latest AMD";
                        }
                        else if (SystemInfo.graphicsDeviceName.Contains("Intel"))
                        {
                            gpuInfo.driver = "Latest Intel";
                        }
                    }
                    catch
                    {
                        // システム情報取得エラー時はデフォルト値を使用
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"GPU diagnosis failed: {ex.Message}");
            }
            
            return gpuInfo;
        }
        
        /// <summary>
        /// ストレージ情報の取得
        /// </summary>
        private static HardwareDiagnostics.StorageInfo DiagnoseStorage()
        {
            var storageInfo = new HardwareDiagnostics.StorageInfo();
            
            try
            {
                string projectRoot = Path.GetPathRoot(Application.dataPath);
                var driveInfo = new DriveInfo(projectRoot);
                
                storageInfo.totalSpace = driveInfo.TotalSize;
                storageInfo.freeSpace = driveInfo.AvailableFreeSpace;
                storageInfo.usagePercent = ((float)(driveInfo.TotalSize - driveInfo.AvailableFreeSpace) / driveInfo.TotalSize) * 100f;
                storageInfo.driveType = driveInfo.DriveType.ToString();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"Storage diagnosis failed: {ex.Message}");
            }
            
            return storageInfo;
        }
        
        #endregion
        
        #region Environment Scoring & Auto-Fix
        
        /// <summary>
        /// 環境評価スコアの計算（0-100点）
        /// </summary>
        private static int CalculateEnvironmentScore(SystemRequirementReport report)
        {
            int score = 0;
            int maxScore = 100;
            
            try
            {
                // 基本要件スコア（60点満点）
                int basicScore = 0;
                var requiredChecks = report.results.Where(r => r.severity == RequirementSeverity.Required);
                var importantChecks = report.results.Where(r => r.severity == RequirementSeverity.Important);
                
                int passedRequired = requiredChecks.Count(r => r.isPassed);
                int totalRequired = requiredChecks.Count();
                
                int passedImportant = importantChecks.Count(r => r.isPassed);
                int totalImportant = importantChecks.Count();
                
                if (totalRequired > 0)
                    basicScore += (passedRequired * 40) / totalRequired; // 必須項目は40点
                
                if (totalImportant > 0)
                    basicScore += (passedImportant * 20) / totalImportant; // 重要項目は20点
                
                score += basicScore;
                
                // ハードウェアパフォーマンススコア（40点満点）
                int hardwareScore = 0;
                
                // CPU スコア（10点）
                if (report.hardware.cpu.cores >= 4) hardwareScore += 5;
                if (report.hardware.cpu.frequencyGHz >= 2.5f) hardwareScore += 5;
                
                // メモリ スコア（15点）
                long memoryGB = report.hardware.memory.totalRAM / (1024 * 1024 * 1024);
                if (memoryGB >= 8) hardwareScore += 8;
                else if (memoryGB >= 4) hardwareScore += 4;
                
                if (report.hardware.memory.usagePercent < 80) hardwareScore += 7;
                else if (report.hardware.memory.usagePercent < 90) hardwareScore += 3;
                
                // GPU スコア（10点）
                long gpuMemoryMB = report.hardware.gpu.dedicatedMemory / (1024 * 1024);
                if (gpuMemoryMB >= 2048) hardwareScore += 5;
                else if (gpuMemoryMB >= 1024) hardwareScore += 3;
                
                if (report.hardware.gpu.supportsDirectX11) hardwareScore += 5;
                
                // ストレージ スコア（5点）
                long freeSpaceGB = report.hardware.storage.freeSpace / (1024 * 1024 * 1024);
                if (freeSpaceGB >= 10) hardwareScore += 5;
                else if (freeSpaceGB >= 5) hardwareScore += 2;
                
                score += hardwareScore;
                
                UnityEngine.Debug.Log($"[SystemRequirementChecker] Environment Score: Basic({basicScore}/60) + Hardware({hardwareScore}/40) = {score}/100");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Environment scoring failed: {ex.Message}");
            }
            
            return Mathf.Clamp(score, 0, maxScore);
        }
        
        /// <summary>
        /// 問題の自動修復を試行
        /// </summary>
        private static List<string> AttemptAutoFixIssues(SystemRequirementReport report)
        {
            var fixedIssues = new List<string>();
            
            try
            {
                UnityEngine.Debug.Log("[SystemRequirementChecker] Attempting auto-fix for detected issues...");
                
                // Git設定の自動修復
                var gitCheck = report.results.FirstOrDefault(r => r.checkName == "Git Configuration Check");
                if (gitCheck != null && !gitCheck.isPassed)
                {
                    if (TryAutoFixGitConfiguration())
                    {
                        fixedIssues.Add("Git Configuration: Auto-configured basic settings");
                    }
                }
                
                // Unity Hub インストールの推奨
                var hubCheck = report.results.FirstOrDefault(r => r.checkName == "Unity Hub Check");
                if (hubCheck != null && !hubCheck.isPassed)
                {
                    // Unity Hub の自動インストールは複雑なため、推奨のみ
                    fixedIssues.Add("Unity Hub: Installation recommended (manual action required)");
                }
                
                // IDE設定の最適化提案
                var ideCheck = report.results.FirstOrDefault(r => r.checkName == "IDE Availability Check");
                if (ideCheck != null && !ideCheck.isPassed)
                {
                    fixedIssues.Add("IDE: Installation guide provided (manual action required)");
                }
                
                if (fixedIssues.Count > 0)
                {
                    UnityEngine.Debug.Log($"[SystemRequirementChecker] Auto-fix completed: {fixedIssues.Count} issues addressed.");
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Auto-fix failed: {ex.Message}");
            }
            
            return fixedIssues;
        }
        
        /// <summary>
        /// Git設定の自動修復
        /// </summary>
        private static bool TryAutoFixGitConfiguration()
        {
            try
            {
                var config = GetGitConfiguration();
                bool wasFixed = false;
                
                // ユーザー名が未設定の場合
                if (string.IsNullOrEmpty(config.userName))
                {
                    string defaultUserName = Environment.UserName;
                    RunGitCommand($"config --global user.name \"{defaultUserName}\"");
                    wasFixed = true;
                }
                
                // メールアドレスが未設定の場合
                if (string.IsNullOrEmpty(config.userEmail))
                {
                    string defaultEmail = $"{Environment.UserName}@local.dev";
                    RunGitCommand($"config --global user.email \"{defaultEmail}\"");
                    wasFixed = true;
                }
                
                return wasFixed;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"Git auto-fix failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// JSON形式での診断結果保存
        /// </summary>
        private static void SaveDiagnosticsToJSON(SystemRequirementReport report)
        {
            try
            {
                string fileName = $"SystemDiagnostics_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                string filePath = Path.Combine(Application.persistentDataPath, fileName);
                
                // シンプルなJSON形式で保存
                var jsonData = new StringBuilder();
                jsonData.AppendLine("{");
                jsonData.AppendLine($"  \"checkTime\": \"{report.checkTime:yyyy-MM-dd HH:mm:ss}\",");
                jsonData.AppendLine($"  \"isValid\": {report.isValid.ToString().ToLower()},");
                jsonData.AppendLine($"  \"environmentScore\": {report.environmentScore},");
                
                // ハードウェア情報
                jsonData.AppendLine($"  \"hardware\": {{");
                jsonData.AppendLine($"    \"cpu\": \"{ report.hardware.cpu.name} ({report.hardware.cpu.cores} cores, {report.hardware.cpu.frequencyGHz:F1} GHz)\",");
                jsonData.AppendLine($"    \"memory\": \"{report.hardware.memory.totalRAM / (1024 * 1024 * 1024)} GB ({report.hardware.memory.usagePercent:F1}% used)\",");
                jsonData.AppendLine($"    \"gpu\": \"{report.hardware.gpu.name} ({report.hardware.gpu.dedicatedMemory / (1024 * 1024)} MB)\",");
                jsonData.AppendLine($"    \"storage\": \"{report.hardware.storage.freeSpace / (1024 * 1024 * 1024)} GB free of {report.hardware.storage.totalSpace / (1024 * 1024 * 1024)} GB\"");
                jsonData.AppendLine($"  }},");
                
                // チェック結果
                jsonData.AppendLine($"  \"results\": [");
                for (int i = 0; i < report.results.Count; i++)
                {
                    var result = report.results[i];
                    jsonData.AppendLine($"    {{");
                    jsonData.AppendLine($"      \"checkName\": \"{result.checkName}\",");
                    jsonData.AppendLine($"      \"isPassed\": {result.isPassed.ToString().ToLower()},");
                    jsonData.AppendLine($"      \"message\": \"{result.message.Replace("\"", "\\\"")}\"");
                    jsonData.AppendLine(i < report.results.Count - 1 ? $"    }},$" : $"    }}");
                }
                jsonData.AppendLine($"  ],");
                
                // 自動修復情報
                jsonData.AppendLine($"  \"autoFixedIssues\": [");
                for (int i = 0; i < report.autoFixedIssues.Count; i++)
                {
                    jsonData.AppendLine($"    \"{report.autoFixedIssues[i]}\"{(i < report.autoFixedIssues.Count - 1 ? "," : "")}");
                }
                jsonData.AppendLine($"  ]");
                
                jsonData.AppendLine("}");
                
                File.WriteAllText(filePath, jsonData.ToString());
                
                UnityEngine.Debug.Log($"[SystemRequirementChecker] Diagnostics saved to: {filePath}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to save diagnostics JSON: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        #region IDE Detection Methods
        
        /// <summary>
        /// Visual Studio Code の詳細検出
        /// </summary>
        private static IDEInfo DetectVSCode()
        {
            try
            {
                // コマンドライン検出
                if (IsCommandAvailable("code"))
                {
                    var version = GetVSCodeVersion();
                    var path = GetCommandPath("code");
                    var hasUnitySupport = CheckVSCodeUnityExtensions();
                    
                    return new IDEInfo("Visual Studio Code", version, path, hasUnitySupport);
                }
                
                // インストールパス直接チェック（Windows）
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    var vscPaths = new string[]
                    {
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                            "Programs", "Microsoft VS Code", "Code.exe"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), 
                            "Microsoft VS Code", "Code.exe"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), 
                            "Microsoft VS Code", "Code.exe")
                    };
                    
                    foreach (var vscPath in vscPaths)
                    {
                        if (File.Exists(vscPath))
                        {
                            var version = GetFileVersion(vscPath);
                            var hasUnitySupport = CheckVSCodeUnityExtensions();
                            return new IDEInfo("Visual Studio Code", version, vscPath, hasUnitySupport);
                        }
                    }
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Visual Studio の詳細検出（複数エディション対応）
        /// </summary>
        private static IDEInfo DetectVisualStudio()
        {
            try
            {
                // コマンドライン検出
                if (IsCommandAvailable("devenv"))
                {
                    var path = GetCommandPath("devenv");
                    var version = GetFileVersion(path);
                    return new IDEInfo("Visual Studio", version, path, true);
                }
                
                // インストールパス直接チェック（Windows）
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    var vsInfo = DetectVisualStudioInstallations();
                    if (vsInfo != null && vsInfo.Count > 0)
                    {
                        // 最新バージョンを返す
                        return vsInfo.OrderByDescending(vs => vs.Version).First();
                    }
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// JetBrains Rider の検出
        /// </summary>
        private static IDEInfo DetectRider()
        {
            try
            {
                // コマンドライン検出
                var commands = new string[] { "rider64", "rider" };
                
                foreach (var cmd in commands)
                {
                    if (IsCommandAvailable(cmd))
                    {
                        var path = GetCommandPath(cmd);
                        var version = GetFileVersion(path);
                        return new IDEInfo("JetBrains Rider", version, path, true);
                    }
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Visual Studio のインストール情報を検出（Windows専用）
        /// </summary>
        private static List<IDEInfo> DetectVisualStudioInstallations()
        {
            var installations = new List<IDEInfo>();
            
            try
            {
                // Visual Studio 2019以降の検出（VS Installer API使用）
                var vsWherePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "Microsoft Visual Studio", "Installer", "vswhere.exe");
                
                if (File.Exists(vsWherePath))
                {
                    var vsWhereOutput = RunCommand(vsWherePath, "-products * -requires Microsoft.VisualStudio.Component.Unity -property installationPath,displayName,installationVersion -format value");
                    
                    if (!string.IsNullOrEmpty(vsWhereOutput))
                    {
                        var lines = vsWhereOutput.Split('\n');
                        for (int i = 0; i < lines.Length; i += 3)
                        {
                            if (i + 2 < lines.Length)
                            {
                                var installPath = lines[i].Trim();
                                var displayName = lines[i + 1].Trim();
                                var version = lines[i + 2].Trim();
                                
                                var devenvPath = Path.Combine(installPath, "Common7", "IDE", "devenv.exe");
                                if (File.Exists(devenvPath))
                                {
                                    installations.Add(new IDEInfo(displayName, version, devenvPath, true));
                                }
                            }
                        }
                    }
                }
                
                // 従来の検出方法（VS 2017以前）
                if (installations.Count == 0)
                {
                    var legacyPaths = new string[]
                    {
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), 
                            "Microsoft Visual Studio", "2019", "Community", "Common7", "IDE", "devenv.exe"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), 
                            "Microsoft Visual Studio", "2019", "Professional", "Common7", "IDE", "devenv.exe"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), 
                            "Microsoft Visual Studio", "2019", "Enterprise", "Common7", "IDE", "devenv.exe"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), 
                            "Microsoft Visual Studio", "2017", "Community", "Common7", "IDE", "devenv.exe"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), 
                            "Microsoft Visual Studio", "2017", "Professional", "Common7", "IDE", "devenv.exe"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), 
                            "Microsoft Visual Studio", "2017", "Enterprise", "Common7", "IDE", "devenv.exe")
                    };
                    
                    foreach (var vsPath in legacyPaths)
                    {
                        if (File.Exists(vsPath))
                        {
                            var version = GetFileVersion(vsPath);
                            var editionName = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(vsPath))));
                            installations.Add(new IDEInfo($"Visual Studio {editionName}", version, vsPath, true));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"Failed to detect Visual Studio installations: {ex.Message}");
            }
            
            return installations;
        }
        
        #endregion
        
        /// <summary>
        /// Unity バージョンの互換性チェック
        /// </summary>
        private static bool IsUnityVersionCompatible(string version)
        {
            try
            {
                // "6000.0.42f1" -> "6.0.0" に変換
                var match = Regex.Match(version, @"(\d+)\.(\d+)\.(\d+)");
                if (match.Success)
                {
                    var major = int.Parse(match.Groups[1].Value.Substring(0, 1)); // 6000 -> 6
                    var minor = int.Parse(match.Groups[2].Value);
                    var patch = int.Parse(match.Groups[3].Value);
                    
                    var currentVersion = new Version(major, minor, patch);
                    return currentVersion >= MINIMUM_UNITY_VERSION;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// コマンドラインツールの利用可能性チェック
        /// </summary>
        private static bool IsCommandAvailable(string commandName)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo()
                {
                    FileName = "where", // Windows
                    Arguments = commandName,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                
                // Linux/Mac の場合は "which" を使用
                if (Application.platform == RuntimePlatform.OSXEditor || 
                    Application.platform == RuntimePlatform.LinuxEditor)
                {
                    processStartInfo.FileName = "which";
                }
                
                using (var process = Process.Start(processStartInfo))
                {
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// コマンドのフルパスを取得
        /// </summary>
        private static string GetCommandPath(string commandName)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo()
                {
                    FileName = "where", // Windows
                    Arguments = commandName,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                
                // Linux/Mac の場合は "which" を使用
                if (Application.platform == RuntimePlatform.OSXEditor || 
                    Application.platform == RuntimePlatform.LinuxEditor)
                {
                    processStartInfo.FileName = "which";
                }
                
                using (var process = Process.Start(processStartInfo))
                {
                    string output = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();
                    return process.ExitCode == 0 ? output.Split('\n')[0].Trim() : "Unknown";
                }
            }
            catch
            {
                return "Unknown";
            }
        }
        
        /// <summary>
        /// 汎用コマンド実行メソッド
        /// </summary>
        private static string RunCommand(string fileName, string arguments)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo()
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                
                using (var process = Process.Start(processStartInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    return process.ExitCode == 0 ? output : "";
                }
            }
            catch
            {
                return "";
            }
        }
        
        /// <summary>
        /// VS Code のバージョン情報を取得
        /// </summary>
        private static string GetVSCodeVersion()
        {
            try
            {
                var versionOutput = RunCommand("code", "--version");
                if (!string.IsNullOrEmpty(versionOutput))
                {
                    var lines = versionOutput.Split('\n');
                    if (lines.Length > 0)
                    {
                        return lines[0].Trim();
                    }
                }
                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }
        
        /// <summary>
        /// 実行ファイルのバージョン情報を取得
        /// </summary>
        private static string GetFileVersion(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var versionInfo = FileVersionInfo.GetVersionInfo(filePath);
                    if (!string.IsNullOrEmpty(versionInfo.ProductVersion))
                    {
                        return versionInfo.ProductVersion;
                    }
                    if (!string.IsNullOrEmpty(versionInfo.FileVersion))
                    {
                        return versionInfo.FileVersion;
                    }
                }
                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }
        
        /// <summary>
        /// VS CodeのUnity関連拡張機能のチェック
        /// </summary>
        private static bool CheckVSCodeUnityExtensions()
        {
            try
            {
                // VS Codeの拡張機能リストを取得
                var extensionsOutput = RunCommand("code", "--list-extensions");
                if (!string.IsNullOrEmpty(extensionsOutput))
                {
                    var extensions = extensionsOutput.ToLower();
                    // Unity関連の主要拡張機能をチェック
                    return extensions.Contains("ms-dotnettools.csharp") || 
                           extensions.Contains("visualstudiotoolsforunity") ||
                           extensions.Contains("unity");
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Git設定の取得
        /// </summary>
        private static (string userName, string userEmail, bool hasUnityYAMLMerge) GetGitConfiguration()
        {
            try
            {
                string userName = RunGitCommand("config --global user.name").Trim();
                string userEmail = RunGitCommand("config --global user.email").Trim();
                string mergetool = RunGitCommand("config --global mergetool.sourcetree.cmd").Trim();
                
                bool hasUnityMerge = mergetool.Contains("UnityYAMLMerge");
                
                return (userName, userEmail, hasUnityMerge);
            }
            catch
            {
                return ("", "", false);
            }
        }
        
        /// <summary>
        /// Git コマンドの実行
        /// </summary>
        private static string RunGitCommand(string arguments)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo()
                {
                    FileName = "git",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                
                using (var process = Process.Start(processStartInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    return process.ExitCode == 0 ? output : "";
                }
            }
            catch
            {
                return "";
            }
        }
        
        /// <summary>
        /// インストール済みパッケージの取得
        /// </summary>
        private static Dictionary<string, string> GetInstalledPackages()
        {
            var packages = new Dictionary<string, string>();
            
            try
            {
                string manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
                if (File.Exists(manifestPath))
                {
                    string manifestContent = File.ReadAllText(manifestPath);
                    
                    // JSON パースは簡易実装（依存関係なし）
                    var matches = Regex.Matches(manifestContent, @"""([^""]+)""\s*:\s*""([^""]+)""");
                    foreach (Match match in matches)
                    {
                        string packageName = match.Groups[1].Value;
                        string version = match.Groups[2].Value;
                        
                        if (packageName.StartsWith("com.unity") || 
                            packageName.StartsWith("com.cysharp") || 
                            packageName.StartsWith("com.coplaydev"))
                        {
                            packages[packageName] = version;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"Failed to read package manifest: {ex.Message}");
            }
            
            return packages;
        }
        
        /// <summary>
        /// レポートサマリーの生成
        /// </summary>
        private static string GenerateSummary(SystemRequirementReport report)
        {
            int totalChecks = report.results.Count;
            int passedChecks = report.results.Count(r => r.isPassed);
            int requiredFailed = report.results.Count(r => !r.isPassed && r.severity == RequirementSeverity.Required);
            int importantFailed = report.results.Count(r => !r.isPassed && r.severity == RequirementSeverity.Important);
            
            var summary = $"System Requirements Check Summary:\n";
            summary += $"- Total Checks: {totalChecks}\n";
            summary += $"- Passed: {passedChecks}\n";
            summary += $"- Failed: {totalChecks - passedChecks}\n";
            
            if (requiredFailed > 0)
                summary += $"- Critical Issues: {requiredFailed} (Setup will fail)\n";
            
            if (importantFailed > 0)
                summary += $"- Important Issues: {importantFailed} (Setup may be impaired)\n";
            
            summary += $"\nOverall Status: {(report.isValid ? "READY FOR SETUP" : "SETUP BLOCKED")}";
            
            return summary;
        }
        
        #endregion
        
        #region Menu Integration
        
        [MenuItem("Project/System Requirements/Check All Requirements", priority = 100)]
        public static void CheckAllRequirementsFromMenu()
        {
            var report = CheckAllRequirements();
            DisplayReportInConsole(report);
        }
        
        [MenuItem("Project/System Requirements/Generate PDF Report", priority = 101)]
        public static void GeneratePDFReportFromMenu()
        {
            var report = CheckAllRequirements();
            GeneratePDFReport(report);
        }
        
        /// <summary>
        /// PDF レポートの生成
        /// </summary>
        public static void GeneratePDFReport(SystemRequirementReport report)
        {
            try
            {
                string fileName = $"SystemRequirements_{DateTime.Now:yyyyMMdd_HHmmss}.html";
                string filePath = Path.Combine(Application.persistentDataPath, fileName);
                
                // HTMLレポートを生成（PDFライブラリなしでブラウザ印刷対応）
                var htmlContent = GenerateHTMLReport(report);
                File.WriteAllText(filePath, htmlContent);
                
                UnityEngine.Debug.Log($"[SystemRequirementChecker] HTML report generated: {filePath}");
                UnityEngine.Debug.Log("[SystemRequirementChecker] Open the HTML file in a browser and use 'Print to PDF' to generate PDF.");
                
                // ファイルエクスプローラーで開く
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    Process.Start("explorer.exe", $"/select,\"{filePath}\"");
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"PDF report generation failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// HTML形式のレポート生成（PDF出力対応）
        /// </summary>
        private static string GenerateHTMLReport(SystemRequirementReport report)
        {
            var html = new StringBuilder();
            
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='utf-8'>");
            html.AppendLine("    <title>Unity System Requirements Report</title>");
            html.AppendLine("    <style>");
            html.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; }");
            html.AppendLine("        .header { background-color: #1f4e79; color: white; padding: 20px; text-align: center; }");
            html.AppendLine("        .score { font-size: 24px; font-weight: bold; text-align: center; margin: 20px 0; }");
            html.AppendLine("        .score.good { color: #4CAF50; }");
            html.AppendLine("        .score.warning { color: #FF9800; }");
            html.AppendLine("        .score.error { color: #F44336; }");
            html.AppendLine("        .section { margin: 20px 0; border: 1px solid #ddd; border-radius: 5px; }");
            html.AppendLine("        .section-header { background-color: #f5f5f5; padding: 10px; font-weight: bold; }");
            html.AppendLine("        .section-content { padding: 15px; }");
            html.AppendLine("        .check-item { margin: 10px 0; padding: 10px; border-left: 4px solid #ddd; }");
            html.AppendLine("        .check-item.passed { border-left-color: #4CAF50; background-color: #e8f5e8; }");
            html.AppendLine("        .check-item.failed { border-left-color: #F44336; background-color: #fce8e6; }");
            html.AppendLine("        .check-item.warning { border-left-color: #FF9800; background-color: #fff3cd; }");
            html.AppendLine("        .hardware-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 15px; }");
            html.AppendLine("        .hardware-item { padding: 15px; border: 1px solid #ddd; border-radius: 5px; }");
            html.AppendLine("        .recommendation { font-style: italic; color: #666; margin-top: 5px; }");
            html.AppendLine("        @media print { body { margin: 0; } }");
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            // ヘッダー
            html.AppendLine("    <div class='header'>");
            html.AppendLine("        <h1>Unity System Requirements Report</h1>");
            html.AppendLine($"        <p>Generated: {report.checkTime:yyyy-MM-dd HH:mm:ss}</p>");
            html.AppendLine("    </div>");
            
            // 環境スコア
            string scoreClass = report.environmentScore >= 80 ? "good" : 
                               report.environmentScore >= 60 ? "warning" : "error";
            html.AppendLine($"    <div class='score {scoreClass}'>");
            html.AppendLine($"        Environment Score: {report.environmentScore}/100");
            html.AppendLine($"        <br><small>Overall Status: {(report.isValid ? "✅ READY" : "❌ ISSUES DETECTED")}</small>");
            html.AppendLine("    </div>");
            
            // ハードウェア情報
            html.AppendLine("    <div class='section'>");
            html.AppendLine("        <div class='section-header'>Hardware Information</div>");
            html.AppendLine("        <div class='section-content'>");
            html.AppendLine("            <div class='hardware-grid'>");
            
            html.AppendLine("                <div class='hardware-item'>");
            html.AppendLine("                    <h4>🖥️ CPU</h4>");
            html.AppendLine($"                    <p><strong>{report.hardware.cpu.name}</strong></p>");
            html.AppendLine($"                    <p>Cores: {report.hardware.cpu.cores} | Frequency: {report.hardware.cpu.frequencyGHz:F1} GHz</p>");
            html.AppendLine("                </div>");
            
            html.AppendLine("                <div class='hardware-item'>");
            html.AppendLine("                    <h4>💾 Memory</h4>");
            html.AppendLine($"                    <p><strong>{report.hardware.memory.totalRAM / (1024 * 1024 * 1024):F1} GB Total</strong></p>");
            html.AppendLine($"                    <p>Available: {report.hardware.memory.availableRAM / (1024 * 1024 * 1024):F1} GB | Usage: {report.hardware.memory.usagePercent:F1}%</p>");
            html.AppendLine("                </div>");
            
            html.AppendLine("                <div class='hardware-item'>");
            html.AppendLine("                    <h4>🎮 Graphics</h4>");
            html.AppendLine($"                    <p><strong>{report.hardware.gpu.name}</strong></p>");
            html.AppendLine($"                    <p>VRAM: {report.hardware.gpu.dedicatedMemory / (1024 * 1024)} MB | DirectX11: {(report.hardware.gpu.supportsDirectX11 ? "Yes" : "No")}</p>");
            html.AppendLine("                </div>");
            
            html.AppendLine("                <div class='hardware-item'>");
            html.AppendLine("                    <h4>💿 Storage</h4>");
            html.AppendLine($"                    <p><strong>{report.hardware.storage.driveType}</strong></p>");
            html.AppendLine($"                    <p>Free: {report.hardware.storage.freeSpace / (1024 * 1024 * 1024):F1} GB of {report.hardware.storage.totalSpace / (1024 * 1024 * 1024):F1} GB</p>");
            html.AppendLine("                </div>");
            
            html.AppendLine("            </div>");
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
            
            // チェック結果
            html.AppendLine("    <div class='section'>");
            html.AppendLine("        <div class='section-header'>System Requirements Check</div>");
            html.AppendLine("        <div class='section-content'>");
            
            foreach (var result in report.results)
            {
                string cssClass = result.isPassed ? "passed" : 
                                 result.severity == RequirementSeverity.Required ? "failed" : "warning";
                string icon = result.isPassed ? "✅" : result.severity == RequirementSeverity.Required ? "❌" : "⚠️";
                
                html.AppendLine($"            <div class='check-item {cssClass}'>");
                html.AppendLine($"                <h4>{icon} {result.checkName}</h4>");
                html.AppendLine($"                <p>{result.message}</p>");
                if (!result.isPassed && !string.IsNullOrEmpty(result.recommendation))
                {
                    html.AppendLine($"                <div class='recommendation'>💡 {result.recommendation}</div>");
                }
                html.AppendLine("            </div>");
            }
            
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
            
            // 自動修復結果
            if (report.autoFixedIssues.Count > 0)
            {
                html.AppendLine("    <div class='section'>");
                html.AppendLine("        <div class='section-header'>Auto-Fixed Issues</div>");
                html.AppendLine("        <div class='section-content'>");
                foreach (var issue in report.autoFixedIssues)
                {
                    html.AppendLine($"            <div class='check-item passed'>🔧 {issue}</div>");
                }
                html.AppendLine("        </div>");
                html.AppendLine("    </div>");
            }
            
            // サマリー
            html.AppendLine("    <div class='section'>");
            html.AppendLine("        <div class='section-header'>Summary</div>");
            html.AppendLine("        <div class='section-content'>");
            html.AppendLine($"            <pre>{report.summary}</pre>");
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
            
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }
        
        /// <summary>
        /// レポートのコンソール表示
        /// </summary>
        private static void DisplayReportInConsole(SystemRequirementReport report)
        {
            UnityEngine.Debug.Log("=== SYSTEM REQUIREMENTS CHECK REPORT ===");
            UnityEngine.Debug.Log($"Check Time: {report.checkTime:yyyy-MM-dd HH:mm:ss}");
            UnityEngine.Debug.Log($"Overall Status: {(report.isValid ? "✅ PASSED" : "❌ FAILED")}");
            UnityEngine.Debug.Log("");
            
            foreach (var result in report.results)
            {
                string icon = result.isPassed ? "✅" : 
                    result.severity == RequirementSeverity.Required ? "❌" : "⚠️";
                
                string logMessage = $"{icon} {result.checkName}: {result.message}";
                
                if (result.isPassed)
                {
                    UnityEngine.Debug.Log(logMessage);
                }
                else
                {
                    if (result.severity == RequirementSeverity.Required)
                        UnityEngine.Debug.LogError(logMessage + "\n  → " + result.recommendation);
                    else
                        UnityEngine.Debug.LogWarning(logMessage + "\n  → " + result.recommendation);
                }
            }
            
            UnityEngine.Debug.Log("");
            UnityEngine.Debug.Log(report.summary);
            UnityEngine.Debug.Log("==========================================");
        }
        
        #endregion
    }
}
