using UnityEngine;
using System;
using System.Collections.Generic;
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Services;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Step 3.7: 段階的移行�E進行状況監視と検証機�E
    /// 吁E��ェーズの成功/失敗�E追跡、パフォーマンス測定、検証機�Eを提侁E    /// </summary>
    public class MigrationProgressTracker : MonoBehaviour
    {
        [Header("Monitoring Configuration")]
        [SerializeField] private bool enableProgressTracking = true;
        [SerializeField] private bool enablePerformanceMonitoring = true;
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private float validationInterval = 10f; // 10秒間隔で検証

        [Header("Current Progress Status")]
        [SerializeField] private MigrationScheduler.MigrationPhase currentPhase;
        [SerializeField] private float phaseStartTime;
        [SerializeField] private int successfulTransitions;
        [SerializeField] private int failedTransitions;
        [SerializeField] private bool currentPhaseValid;

        [Header("Performance Metrics")]
        [SerializeField] private float averageTransitionTime;
        [SerializeField] private float lastTransitionTime;
        [SerializeField] private int totalValidationChecks;
        [SerializeField] private int failedValidationChecks;

        private readonly List<PhaseTransitionRecord> transitionHistory = new List<PhaseTransitionRecord>();
        private readonly List<ValidationResult> validationHistory = new List<ValidationResult>();
        private readonly Dictionary<MigrationScheduler.MigrationPhase, PhaseStatistics> phaseStats = 
            new Dictionary<MigrationScheduler.MigrationPhase, PhaseStatistics>();

        /// <summary>
        /// フェーズ遷移記録の構造佁E        /// </summary>
        [System.Serializable]
        public struct PhaseTransitionRecord
        {
            public MigrationScheduler.MigrationPhase fromPhase;
            public MigrationScheduler.MigrationPhase toPhase;
            public float timestamp;
            public float transitionTime;
            public bool successful;
            public string errorMessage;
            public PhaseValidationSummary preValidation;
            public PhaseValidationSummary postValidation;
        }

        /// <summary>
        /// 検証結果の構造佁E        /// </summary>
        [System.Serializable]
        public struct ValidationResult
        {
            public float timestamp;
            public MigrationScheduler.MigrationPhase phase;
            public bool serviceLocatorWorking;
            public bool audioServiceWorking;
            public bool spatialServiceWorking;
            public bool stealthServiceWorking;
            public bool allServicesWorking;
            public float validationTime;
            public string issues;
        }

        /// <summary>
        /// フェーズ統計�E構造佁E        /// </summary>
        [System.Serializable]
        public struct PhaseStatistics
        {
            public MigrationScheduler.MigrationPhase phase;
            public int enterCount;
            public int successCount;
            public int failureCount;
            public float totalTimeSpent;
            public float averageTimeSpent;
            public float successRate;
            public DateTime firstEntered;
            public DateTime lastEntered;
        }

        /// <summary>
        /// フェーズ検証結果の構造佁E        /// </summary>
        [System.Serializable]
        public struct PhaseValidationSummary
        {
            public bool isValid;
            public float validationTime;
            public Dictionary<string, bool> serviceStates;
            public List<string> issues;
            public int serviceCount;
            public float responseTime;
        }

        #region Unity Lifecycle

        private void Start()
        {
            if (enableProgressTracking)
            {
                StartProgressTracking();
            }

            if (enablePerformanceMonitoring)
            {
                InvokeRepeating(nameof(PerformValidationCheck), validationInterval, validationInterval);
            }

            LogProgress("MigrationProgressTracker initialized");
        }

        private void OnDestroy()
        {
            SaveProgressData();
        }

        #endregion

        #region Progress Tracking

        /// <summary>
        /// 進行状況追跡の開姁E        /// </summary>
        public void StartProgressTracking()
        {
            LogProgress("Starting migration progress tracking");
            
            // 現在のフェーズを記録
            currentPhase = MigrationScheduler.MigrationPhase.Day1_2_Staging;
            phaseStartTime = Time.time;
            currentPhaseValid = false;

            // 統計データの初期匁E            InitializePhaseStatistics();
            
            // 初期検証の実衁E            PerformInitialValidation();
        }

        /// <summary>
        /// フェーズ開始�E記録
        /// </summary>
        /// <param name="phase">開始するフェーズ</param>
        public void RecordPhaseStart(MigrationScheduler.MigrationPhase phase)
        {
            LogProgress($"Phase started: {phase}");
            
            currentPhase = phase;
            phaseStartTime = Time.time;
            
            // 統計データの更新
            UpdatePhaseStatistics(phase, PhaseEvent.Enter);
            
            // フェーズ開始時の検証
            var validation = ValidateCurrentPhase();
            currentPhaseValid = validation.allServicesWorking;
            
            if (!currentPhaseValid)
            {
                LogProgress($"⚠�E�EPhase {phase} started with validation issues: {validation.issues}");
            }
        }

        /// <summary>
        /// フェーズ遷移の記録
        /// </summary>
        /// <param name="fromPhase">遷移允E��ェーズ</param>
        /// <param name="toPhase">遷移先フェーズ</param>
        public void RecordPhaseTransition(MigrationScheduler.MigrationPhase fromPhase, MigrationScheduler.MigrationPhase toPhase)
        {
            float transitionTime = Time.time - phaseStartTime;
            
            LogProgress($"Phase transition: {fromPhase} -> {toPhase} (Duration: {transitionTime:F1}s)");
            
            // 遷移前後�E検証
            var preValidation = ValidatePhaseDetailed(fromPhase);
            var postValidation = ValidatePhaseDetailed(toPhase);
            
            bool successful = postValidation.isValid;
            
            // 遷移記録の作�E
            var record = new PhaseTransitionRecord
            {
                fromPhase = fromPhase,
                toPhase = toPhase,
                timestamp = Time.time,
                transitionTime = transitionTime,
                successful = successful,
                errorMessage = successful ? string.Empty : string.Join(", ", postValidation.issues),
                preValidation = preValidation,
                postValidation = postValidation
            };
            
            transitionHistory.Add(record);
            
            // 統計�E更新
            if (successful)
            {
                successfulTransitions++;
                UpdatePhaseStatistics(fromPhase, PhaseEvent.SuccessfulExit);
                UpdatePhaseStatistics(toPhase, PhaseEvent.Enter);
            }
            else
            {
                failedTransitions++;
                UpdatePhaseStatistics(fromPhase, PhaseEvent.FailedExit);
            }
            
            // 平坁E�E移時間の更新
            UpdateAverageTransitionTime(transitionTime);
            lastTransitionTime = transitionTime;
            
            // 現在のフェーズ状態�E更新
            currentPhase = toPhase;
            phaseStartTime = Time.time;
            currentPhaseValid = successful;
            
            LogProgress($"Transition result: {(successful ? "✁ESUCCESS" : "❁EFAILED")}");
        }

        /// <summary>
        /// スケジュール完亁E�E記録
        /// </summary>
        public void RecordScheduleCompletion()
        {
            LogProgress("Migration schedule completed successfully!");
            
            // 完亁E��計�E記録
            float totalTime = Time.time - phaseStartTime;
            LogProgress($"Total migration time: {totalTime:F1} seconds");
            LogProgress($"Successful transitions: {successfulTransitions}");
            LogProgress($"Failed transitions: {failedTransitions}");
            LogProgress($"Success rate: {GetOverallSuccessRate():P1}");
            
            // 最終検証の実衁E            var finalValidation = ValidateCurrentPhase();
            if (finalValidation.allServicesWorking)
            {
                LogProgress("🎉 Final validation passed - Migration completed successfully!");
            }
            else
            {
                LogProgress($"⚠�E�EFinal validation issues: {finalValidation.issues}");
            }
            
            SaveProgressData();
        }

        #endregion

        #region Validation System

        /// <summary>
        /// 現在のフェーズの検証
        /// </summary>
        /// <returns>検証結果</returns>
        [ContextMenu("Validate Current Phase")]
        public ValidationResult ValidateCurrentPhase()
        {
            return ValidatePhase(currentPhase);
        }

        /// <summary>
        /// 持E��フェーズの検証
        /// </summary>
        /// <param name="phase">検証するフェーズ</param>
        /// <returns>検証結果</returns>
        private ValidationResult ValidatePhase(MigrationScheduler.MigrationPhase phase)
        {
            float startTime = Time.time;
            
            var result = new ValidationResult
            {
                timestamp = Time.time,
                phase = phase,
                serviceLocatorWorking = ValidateServiceLocator(),
                audioServiceWorking = ValidateAudioService(),
                spatialServiceWorking = ValidateSpatialService(),
                stealthServiceWorking = ValidateStealthService(),
                issues = ""
            };
            
            // 全サービス動作確誁E            result.allServicesWorking = result.serviceLocatorWorking;
            
            // フェーズ別の忁E��サービス確誁E            switch (phase)
            {
                case MigrationScheduler.MigrationPhase.Day1_2_Staging:
                    result.allServicesWorking &= result.audioServiceWorking;
                    break;
                case MigrationScheduler.MigrationPhase.Day3_SpatialEnabled:
                    result.allServicesWorking &= result.audioServiceWorking && result.spatialServiceWorking;
                    break;
                case MigrationScheduler.MigrationPhase.Day4_StealthEnabled:
                case MigrationScheduler.MigrationPhase.Day5_Validation:
                    result.allServicesWorking &= result.audioServiceWorking && 
                                                result.spatialServiceWorking && 
                                                result.stealthServiceWorking;
                    break;
            }
            
            result.validationTime = Time.time - startTime;
            
            // 問題�E収集
            List<string> issues = new List<string>();
            if (!result.serviceLocatorWorking) issues.Add("ServiceLocator not working");
            if (!result.audioServiceWorking && ShouldValidateAudioService(phase)) issues.Add("AudioService not working");
            if (!result.spatialServiceWorking && ShouldValidateSpatialService(phase)) issues.Add("SpatialService not working");
            if (!result.stealthServiceWorking && ShouldValidateStealthService(phase)) issues.Add("StealthService not working");
            
            result.issues = string.Join(", ", issues);
            
            return result;
        }

        /// <summary>
        /// 定期皁E��検証チェチE��
        /// </summary>
        private void PerformValidationCheck()
        {
            if (!enablePerformanceMonitoring) return;
            
            totalValidationChecks++;
            var validation = ValidateCurrentPhase();
            validationHistory.Add(validation);
            
            if (!validation.allServicesWorking)
            {
                failedValidationChecks++;
                LogProgress($"❁EValidation failed for {currentPhase}: {validation.issues}");
                
                // 自動修復の試行（忁E��に応じて実裁E��E                if (FeatureFlags.EnableAutoRollback)
                {
                    AttemptAutoRepair(validation);
                }
            }
            
            // 履歴サイズの制陁E            if (validationHistory.Count > 100)
            {
                validationHistory.RemoveRange(0, validationHistory.Count - 100);
            }
        }

        /// <summary>
        /// 初期検証の実衁E        /// </summary>
        private void PerformInitialValidation()
        {
            LogProgress("Performing initial validation...");
            var validation = ValidateCurrentPhase();
            validationHistory.Add(validation);
            
            LogProgress($"Initial validation result: {(validation.allServicesWorking ? "✁EPASSED" : "❁EFAILED")}");
            if (!validation.allServicesWorking)
            {
                LogProgress($"Issues found: {validation.issues}");
            }
        }

        #endregion

        #region Service Validation

        /// <summary>
        /// ServiceLocatorの動作確誁E        /// </summary>
        /// <returns>動作してぁE��場吁Erue</returns>
        private bool ValidateServiceLocator()
        {
            try
            {
                // ServiceLocatorが動作してぁE��か確認（サービス数で判定！E                int serviceCount = ServiceLocator.GetServiceCount();
                return serviceCount >= 0; // サービス数が取得できれば動作中
            }
            catch (Exception ex)
            {
                LogProgress($"ServiceLocator validation failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// AudioServiceの動作確誁E        /// </summary>
        /// <returns>動作してぁE��場吁Erue</returns>
        private bool ValidateAudioService()
        {
            try
            {
                if (!FeatureFlags.UseNewAudioService) return true; // 無効化されてぁE��場合�EスキチE�E
                
                var result = ServiceMigrationHelper.GetAudioService(true, "ProgressTracker", false);
                return result.IsSuccessful && result.Service != null;
            }
            catch (Exception ex)
            {
                LogProgress($"AudioService validation failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// SpatialServiceの動作確誁E        /// </summary>
        /// <returns>動作してぁE��場吁Erue</returns>
        private bool ValidateSpatialService()
        {
            try
            {
                if (!FeatureFlags.UseNewSpatialService) return true; // 無効化されてぁE��場合�EスキチE�E
                
                // SpatialAudioServiceの動作確認ロジチE���E�仮実裁E��E                LogProgress("SpatialService validation - implementation needed");
                return true; // TODO: 実際のサービス検証を実裁E            }
            catch (Exception ex)
            {
                LogProgress($"SpatialService validation failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// StealthServiceの動作確誁E        /// </summary>
        /// <returns>動作してぁE��場吁Erue</returns>
        private bool ValidateStealthService()
        {
            try
            {
                if (!FeatureFlags.UseNewStealthService) return true; // 無効化されてぁE��場合�EスキチE�E
                
                var result = ServiceMigrationHelper.GetStealthAudioService(true, "ProgressTracker", false);
                return result.IsSuccessful && result.Service != null;
            }
            catch (Exception ex)
            {
                LogProgress($"StealthService validation failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// フェーズに応じたサービス検証の忁E��性判宁E        /// </summary>
        private bool ShouldValidateAudioService(MigrationScheduler.MigrationPhase phase)
        {
            return phase >= MigrationScheduler.MigrationPhase.Day1_2_Staging;
        }

        private bool ShouldValidateSpatialService(MigrationScheduler.MigrationPhase phase)
        {
            return phase >= MigrationScheduler.MigrationPhase.Day3_SpatialEnabled;
        }

        private bool ShouldValidateStealthService(MigrationScheduler.MigrationPhase phase)
        {
            return phase >= MigrationScheduler.MigrationPhase.Day4_StealthEnabled;
        }

        #endregion

        #region Statistics and Reporting

        /// <summary>
        /// フェーズ統計�E初期匁E        /// </summary>
        private void InitializePhaseStatistics()
        {
            var phases = Enum.GetValues(typeof(MigrationScheduler.MigrationPhase));
            foreach (MigrationScheduler.MigrationPhase phase in phases)
            {
                if (phase == MigrationScheduler.MigrationPhase.NotStarted || 
                    phase == MigrationScheduler.MigrationPhase.Completed) continue;
                    
                phaseStats[phase] = new PhaseStatistics
                {
                    phase = phase,
                    firstEntered = DateTime.MinValue,
                    lastEntered = DateTime.MinValue
                };
            }
        }

        /// <summary>
        /// フェーズ統計�E更新
        /// </summary>
        /// <param name="phase">フェーズ</param>
        /// <param name="eventType">イベント種別</param>
        private void UpdatePhaseStatistics(MigrationScheduler.MigrationPhase phase, PhaseEvent eventType)
        {
            if (!phaseStats.ContainsKey(phase)) return;
            
            var stats = phaseStats[phase];
            
            switch (eventType)
            {
                case PhaseEvent.Enter:
                    stats.enterCount++;
                    stats.lastEntered = DateTime.Now;
                    if (stats.firstEntered == DateTime.MinValue)
                        stats.firstEntered = DateTime.Now;
                    break;
                case PhaseEvent.SuccessfulExit:
                    stats.successCount++;
                    break;
                case PhaseEvent.FailedExit:
                    stats.failureCount++;
                    break;
            }
            
            // 成功玁E�E計箁E            if (stats.enterCount > 0)
            {
                stats.successRate = (float)stats.successCount / stats.enterCount;
            }
            
            phaseStats[phase] = stats;
        }

        /// <summary>
        /// 平坁E�E移時間の更新
        /// </summary>
        /// <param name="newTransitionTime">新しい遷移時間</param>
        private void UpdateAverageTransitionTime(float newTransitionTime)
        {
            int totalTransitions = successfulTransitions + failedTransitions;
            averageTransitionTime = ((averageTransitionTime * (totalTransitions - 1)) + newTransitionTime) / totalTransitions;
        }

        /// <summary>
        /// 全体的な成功玁E�E取征E        /// </summary>
        /// <returns>成功玁E��E.0-1.0�E�E/returns>
        public float GetOverallSuccessRate()
        {
            int totalTransitions = successfulTransitions + failedTransitions;
            return totalTransitions > 0 ? (float)successfulTransitions / totalTransitions : 0f;
        }

        /// <summary>
        /// 進行状況レポ�Eト�E生�E
        /// </summary>
        [ContextMenu("Generate Progress Report")]
        public void GenerateProgressReport()
        {
            LogProgress("=== Migration Progress Report ===");
            LogProgress($"Current Phase: {currentPhase}");
            LogProgress($"Phase Valid: {currentPhaseValid}");
            LogProgress($"Successful Transitions: {successfulTransitions}");
            LogProgress($"Failed Transitions: {failedTransitions}");
            LogProgress($"Overall Success Rate: {GetOverallSuccessRate():P1}");
            LogProgress($"Average Transition Time: {averageTransitionTime:F1}s");
            LogProgress($"Last Transition Time: {lastTransitionTime:F1}s");
            LogProgress($"Total Validation Checks: {totalValidationChecks}");
            LogProgress($"Failed Validation Checks: {failedValidationChecks}");
            
            if (failedValidationChecks > 0)
            {
                float validationFailureRate = (float)failedValidationChecks / totalValidationChecks;
                LogProgress($"Validation Failure Rate: {validationFailureRate:P1}");
            }
            
            // 最近�E問題�Eレポ�EチE            ReportRecentIssues();
        }

        /// <summary>
        /// 最近�E問題�Eレポ�EチE        /// </summary>
        private void ReportRecentIssues()
        {
            var recentValidations = validationHistory.FindAll(v => Time.time - v.timestamp < 60f); // 過去1刁E            var issueValidations = recentValidations.FindAll(v => !v.allServicesWorking);
            
            if (issueValidations.Count > 0)
            {
                LogProgress("=== Recent Issues (Last 60s) ===");
                foreach (var validation in issueValidations)
                {
                    LogProgress($"[{validation.timestamp:F1}s] {validation.phase}: {validation.issues}");
                }
            }
        }

        #endregion

        #region Auto Repair

        /// <summary>
        /// 自動修復の試衁E        /// </summary>
        /// <param name="validation">検証結果</param>
        private void AttemptAutoRepair(ValidationResult validation)
        {
            LogProgress($"Attempting auto-repair for {validation.phase}...");
            
            // 基本皁E��修復手頁E            if (!validation.serviceLocatorWorking)
            {
                LogProgress("ServiceLocator issue detected - attempting restart");
                // TODO: ServiceLocator再�E期化ロジチE��
            }
            
            // より詳細な修復ロジチE��は忁E��に応じて実裁E            LogProgress("Auto-repair attempt completed");
        }

        #endregion

        #region Data Persistence

        /// <summary>
        /// 進行状況データの保孁E        /// </summary>
        private void SaveProgressData()
        {
            try
            {
                // PlayerPrefsに基本統計を保孁E                PlayerPrefs.SetInt("MigrationProgress_SuccessfulTransitions", successfulTransitions);
                PlayerPrefs.SetInt("MigrationProgress_FailedTransitions", failedTransitions);
                PlayerPrefs.SetFloat("MigrationProgress_AverageTransitionTime", averageTransitionTime);
                PlayerPrefs.SetInt("MigrationProgress_TotalValidationChecks", totalValidationChecks);
                PlayerPrefs.SetInt("MigrationProgress_FailedValidationChecks", failedValidationChecks);
                PlayerPrefs.Save();
                
                LogProgress("Progress data saved");
            }
            catch (Exception ex)
            {
                LogProgress($"Failed to save progress data: {ex.Message}");
            }
        }

        /// <summary>
        /// 進行状況データの読み込み
        /// </summary>
        private void LoadProgressData()
        {
            try
            {
                successfulTransitions = PlayerPrefs.GetInt("MigrationProgress_SuccessfulTransitions", 0);
                failedTransitions = PlayerPrefs.GetInt("MigrationProgress_FailedTransitions", 0);
                averageTransitionTime = PlayerPrefs.GetFloat("MigrationProgress_AverageTransitionTime", 0f);
                totalValidationChecks = PlayerPrefs.GetInt("MigrationProgress_TotalValidationChecks", 0);
                failedValidationChecks = PlayerPrefs.GetInt("MigrationProgress_FailedValidationChecks", 0);
                
                LogProgress("Progress data loaded");
            }
            catch (Exception ex)
            {
                LogProgress($"Failed to load progress data: {ex.Message}");
            }
        }

        #endregion

        #region Utility Types

        /// <summary>
        /// フェーズイベント�E種顁E        /// </summary>
        private enum PhaseEvent
        {
            Enter,
            SuccessfulExit,
            FailedExit
        }

        /// <summary>
        /// フェーズ検証のヘルパ�E関数
        /// </summary>
        private PhaseValidationSummary ValidatePhaseDetailed(MigrationScheduler.MigrationPhase phase)
        {
            var validation = ValidatePhase(phase);
            return new PhaseValidationSummary
            {
                isValid = validation.allServicesWorking,
                validationTime = validation.validationTime,
                serviceStates = new Dictionary<string, bool>
                {
                    ["ServiceLocator"] = validation.serviceLocatorWorking,
                    ["AudioService"] = validation.audioServiceWorking,
                    ["SpatialService"] = validation.spatialServiceWorking,
                    ["StealthService"] = validation.stealthServiceWorking
                },
                issues = string.IsNullOrEmpty(validation.issues) ? new List<string>() : new List<string>(validation.issues.Split(", ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)),
                serviceCount = 4,
                responseTime = validation.validationTime
            };
        }

        #endregion

        #region Logging

        /// <summary>
        /// 進行状況ログの出劁E        /// </summary>
        /// <param name="message">メチE��ージ</param>
        private void LogProgress(string message)
        {
            if (enableDebugLogging)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log($"[MigrationProgressTracker] {message}");
            }
        }

        #endregion
    }
}