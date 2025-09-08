using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

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
            
            public void AddResult(RequirementCheckResult result)
            {
                results.Add(result);
                if (!result.isPassed)
                    isValid = false;
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
            
            // Generate Summary
            report.summary = GenerateSummary(report);
            
            UnityEngine.Debug.Log($"[SystemRequirementChecker] Check completed. Overall result: {(report.isValid ? "PASSED" : "FAILED")}");
            
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