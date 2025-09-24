using UnityEngine;
using System;
using System.Collections.Generic;
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Services;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Step 3.7: 谿ｵ髫守噪遘ｻ陦後・騾ｲ陦檎憾豕∫屮隕悶→讀懆ｨｼ讖溯・
    /// 蜷・ヵ繧ｧ繝ｼ繧ｺ縺ｮ謌仙粥/螟ｱ謨励・霑ｽ霍｡縲√ヱ繝輔か繝ｼ繝槭Φ繧ｹ貂ｬ螳壹∵､懆ｨｼ讖溯・繧呈署萓・    /// </summary>
    public class MigrationProgressTracker : MonoBehaviour
    {
        [Header("Monitoring Configuration")]
        [SerializeField] private bool enableProgressTracking = true;
        [SerializeField] private bool enablePerformanceMonitoring = true;
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private float validationInterval = 10f; // 10遘帝俣髫斐〒讀懆ｨｼ

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
        /// 繝輔ぉ繝ｼ繧ｺ驕ｷ遘ｻ險倬鹸縺ｮ讒矩菴・        /// </summary>
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
        /// 讀懆ｨｼ邨先棡縺ｮ讒矩菴・        /// </summary>
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
        /// 繝輔ぉ繝ｼ繧ｺ邨ｱ險医・讒矩菴・        /// </summary>
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
        /// 繝輔ぉ繝ｼ繧ｺ讀懆ｨｼ邨先棡縺ｮ讒矩菴・        /// </summary>
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
        /// 騾ｲ陦檎憾豕∬ｿｽ霍｡縺ｮ髢句ｧ・        /// </summary>
        public void StartProgressTracking()
        {
            LogProgress("Starting migration progress tracking");
            
            // 迴ｾ蝨ｨ縺ｮ繝輔ぉ繝ｼ繧ｺ繧定ｨ倬鹸
            currentPhase = MigrationScheduler.MigrationPhase.Day1_2_Staging;
            phaseStartTime = Time.time;
            currentPhaseValid = false;

            // 邨ｱ險医ョ繝ｼ繧ｿ縺ｮ蛻晄悄蛹・            InitializePhaseStatistics();
            
            // 蛻晄悄讀懆ｨｼ縺ｮ螳溯｡・            PerformInitialValidation();
        }

        /// <summary>
        /// 繝輔ぉ繝ｼ繧ｺ髢句ｧ九・險倬鹸
        /// </summary>
        /// <param name="phase">髢句ｧ九☆繧九ヵ繧ｧ繝ｼ繧ｺ</param>
        public void RecordPhaseStart(MigrationScheduler.MigrationPhase phase)
        {
            LogProgress($"Phase started: {phase}");
            
            currentPhase = phase;
            phaseStartTime = Time.time;
            
            // 邨ｱ險医ョ繝ｼ繧ｿ縺ｮ譖ｴ譁ｰ
            UpdatePhaseStatistics(phase, PhaseEvent.Enter);
            
            // 繝輔ぉ繝ｼ繧ｺ髢句ｧ区凾縺ｮ讀懆ｨｼ
            var validation = ValidateCurrentPhase();
            currentPhaseValid = validation.allServicesWorking;
            
            if (!currentPhaseValid)
            {
                LogProgress($"笞・・Phase {phase} started with validation issues: {validation.issues}");
            }
        }

        /// <summary>
        /// 繝輔ぉ繝ｼ繧ｺ驕ｷ遘ｻ縺ｮ險倬鹸
        /// </summary>
        /// <param name="fromPhase">驕ｷ遘ｻ蜈・ヵ繧ｧ繝ｼ繧ｺ</param>
        /// <param name="toPhase">驕ｷ遘ｻ蜈医ヵ繧ｧ繝ｼ繧ｺ</param>
        public void RecordPhaseTransition(MigrationScheduler.MigrationPhase fromPhase, MigrationScheduler.MigrationPhase toPhase)
        {
            float transitionTime = Time.time - phaseStartTime;
            
            LogProgress($"Phase transition: {fromPhase} -> {toPhase} (Duration: {transitionTime:F1}s)");
            
            // 驕ｷ遘ｻ蜑榊ｾ後・讀懆ｨｼ
            var preValidation = ValidatePhaseDetailed(fromPhase);
            var postValidation = ValidatePhaseDetailed(toPhase);
            
            bool successful = postValidation.isValid;
            
            // 驕ｷ遘ｻ險倬鹸縺ｮ菴懈・
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
            
            // 邨ｱ險医・譖ｴ譁ｰ
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
            
            // 蟷ｳ蝮・・遘ｻ譎る俣縺ｮ譖ｴ譁ｰ
            UpdateAverageTransitionTime(transitionTime);
            lastTransitionTime = transitionTime;
            
            // 迴ｾ蝨ｨ縺ｮ繝輔ぉ繝ｼ繧ｺ迥ｶ諷九・譖ｴ譁ｰ
            currentPhase = toPhase;
            phaseStartTime = Time.time;
            currentPhaseValid = successful;
            
            LogProgress($"Transition result: {(successful ? "笨・SUCCESS" : "笶・FAILED")}");
        }

        /// <summary>
        /// 繧ｹ繧ｱ繧ｸ繝･繝ｼ繝ｫ螳御ｺ・・險倬鹸
        /// </summary>
        public void RecordScheduleCompletion()
        {
            LogProgress("Migration schedule completed successfully!");
            
            // 螳御ｺ・ｵｱ險医・險倬鹸
            float totalTime = Time.time - phaseStartTime;
            LogProgress($"Total migration time: {totalTime:F1} seconds");
            LogProgress($"Successful transitions: {successfulTransitions}");
            LogProgress($"Failed transitions: {failedTransitions}");
            LogProgress($"Success rate: {GetOverallSuccessRate():P1}");
            
            // 譛邨よ､懆ｨｼ縺ｮ螳溯｡・            var finalValidation = ValidateCurrentPhase();
            if (finalValidation.allServicesWorking)
            {
                LogProgress("脂 Final validation passed - Migration completed successfully!");
            }
            else
            {
                LogProgress($"笞・・Final validation issues: {finalValidation.issues}");
            }
            
            SaveProgressData();
        }

        #endregion

        #region Validation System

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ繝輔ぉ繝ｼ繧ｺ縺ｮ讀懆ｨｼ
        /// </summary>
        /// <returns>讀懆ｨｼ邨先棡</returns>
        [ContextMenu("Validate Current Phase")]
        public ValidationResult ValidateCurrentPhase()
        {
            return ValidatePhase(currentPhase);
        }

        /// <summary>
        /// 謖・ｮ壹ヵ繧ｧ繝ｼ繧ｺ縺ｮ讀懆ｨｼ
        /// </summary>
        /// <param name="phase">讀懆ｨｼ縺吶ｋ繝輔ぉ繝ｼ繧ｺ</param>
        /// <returns>讀懆ｨｼ邨先棡</returns>
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
            
            // 蜈ｨ繧ｵ繝ｼ繝薙せ蜍穂ｽ懃｢ｺ隱・            result.allServicesWorking = result.serviceLocatorWorking;
            
            // 繝輔ぉ繝ｼ繧ｺ蛻･縺ｮ蠢・ｦ√し繝ｼ繝薙せ遒ｺ隱・            switch (phase)
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
            
            // 蝠城｡後・蜿朱寔
            List<string> issues = new List<string>();
            if (!result.serviceLocatorWorking) issues.Add("ServiceLocator not working");
            if (!result.audioServiceWorking && ShouldValidateAudioService(phase)) issues.Add("AudioService not working");
            if (!result.spatialServiceWorking && ShouldValidateSpatialService(phase)) issues.Add("SpatialService not working");
            if (!result.stealthServiceWorking && ShouldValidateStealthService(phase)) issues.Add("StealthService not working");
            
            result.issues = string.Join(", ", issues);
            
            return result;
        }

        /// <summary>
        /// 螳壽悄逧・↑讀懆ｨｼ繝√ぉ繝・け
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
                LogProgress($"笶・Validation failed for {currentPhase}: {validation.issues}");
                
                // 閾ｪ蜍穂ｿｮ蠕ｩ縺ｮ隧ｦ陦鯉ｼ亥ｿ・ｦ√↓蠢懊§縺ｦ螳溯｣・ｼ・                if (FeatureFlags.EnableAutoRollback)
                {
                    AttemptAutoRepair(validation);
                }
            }
            
            // 螻･豁ｴ繧ｵ繧､繧ｺ縺ｮ蛻ｶ髯・            if (validationHistory.Count > 100)
            {
                validationHistory.RemoveRange(0, validationHistory.Count - 100);
            }
        }

        /// <summary>
        /// 蛻晄悄讀懆ｨｼ縺ｮ螳溯｡・        /// </summary>
        private void PerformInitialValidation()
        {
            LogProgress("Performing initial validation...");
            var validation = ValidateCurrentPhase();
            validationHistory.Add(validation);
            
            LogProgress($"Initial validation result: {(validation.allServicesWorking ? "笨・PASSED" : "笶・FAILED")}");
            if (!validation.allServicesWorking)
            {
                LogProgress($"Issues found: {validation.issues}");
            }
        }

        #endregion

        #region Service Validation

        /// <summary>
        /// ServiceLocator縺ｮ蜍穂ｽ懃｢ｺ隱・        /// </summary>
        /// <returns>蜍穂ｽ懊＠縺ｦ縺・ｋ蝣ｴ蜷・rue</returns>
        private bool ValidateServiceLocator()
        {
            try
            {
                // ServiceLocator縺悟虚菴懊＠縺ｦ縺・ｋ縺狗｢ｺ隱搾ｼ医し繝ｼ繝薙せ謨ｰ縺ｧ蛻､螳夲ｼ・                int serviceCount = ServiceLocator.GetServiceCount();
                return serviceCount >= 0; // 繧ｵ繝ｼ繝薙せ謨ｰ縺悟叙蠕励〒縺阪ｌ縺ｰ蜍穂ｽ應ｸｭ
            }
            catch (Exception ex)
            {
                LogProgress($"ServiceLocator validation failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// AudioService縺ｮ蜍穂ｽ懃｢ｺ隱・        /// </summary>
        /// <returns>蜍穂ｽ懊＠縺ｦ縺・ｋ蝣ｴ蜷・rue</returns>
        private bool ValidateAudioService()
        {
            try
            {
                if (!FeatureFlags.UseNewAudioService) return true; // 辟｡蜉ｹ蛹悶＆繧後※縺・ｋ蝣ｴ蜷医・繧ｹ繧ｭ繝・・
                
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
        /// SpatialService縺ｮ蜍穂ｽ懃｢ｺ隱・        /// </summary>
        /// <returns>蜍穂ｽ懊＠縺ｦ縺・ｋ蝣ｴ蜷・rue</returns>
        private bool ValidateSpatialService()
        {
            try
            {
                if (!FeatureFlags.UseNewSpatialService) return true; // 辟｡蜉ｹ蛹悶＆繧後※縺・ｋ蝣ｴ蜷医・繧ｹ繧ｭ繝・・
                
                // SpatialAudioService縺ｮ蜍穂ｽ懃｢ｺ隱阪Ο繧ｸ繝・け・井ｻｮ螳溯｣・ｼ・                LogProgress("SpatialService validation - implementation needed");
                return true; // TODO: 螳滄圀縺ｮ繧ｵ繝ｼ繝薙せ讀懆ｨｼ繧貞ｮ溯｣・            }
            catch (Exception ex)
            {
                LogProgress($"SpatialService validation failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// StealthService縺ｮ蜍穂ｽ懃｢ｺ隱・        /// </summary>
        /// <returns>蜍穂ｽ懊＠縺ｦ縺・ｋ蝣ｴ蜷・rue</returns>
        private bool ValidateStealthService()
        {
            try
            {
                if (!FeatureFlags.UseNewStealthService) return true; // 辟｡蜉ｹ蛹悶＆繧後※縺・ｋ蝣ｴ蜷医・繧ｹ繧ｭ繝・・
                
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
        /// 繝輔ぉ繝ｼ繧ｺ縺ｫ蠢懊§縺溘し繝ｼ繝薙せ讀懆ｨｼ縺ｮ蠢・ｦ∵ｧ蛻､螳・        /// </summary>
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
        /// 繝輔ぉ繝ｼ繧ｺ邨ｱ險医・蛻晄悄蛹・        /// </summary>
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
        /// 繝輔ぉ繝ｼ繧ｺ邨ｱ險医・譖ｴ譁ｰ
        /// </summary>
        /// <param name="phase">繝輔ぉ繝ｼ繧ｺ</param>
        /// <param name="eventType">繧､繝吶Φ繝育ｨｮ蛻･</param>
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
            
            // 謌仙粥邇・・險育ｮ・            if (stats.enterCount > 0)
            {
                stats.successRate = (float)stats.successCount / stats.enterCount;
            }
            
            phaseStats[phase] = stats;
        }

        /// <summary>
        /// 蟷ｳ蝮・・遘ｻ譎る俣縺ｮ譖ｴ譁ｰ
        /// </summary>
        /// <param name="newTransitionTime">譁ｰ縺励＞驕ｷ遘ｻ譎る俣</param>
        private void UpdateAverageTransitionTime(float newTransitionTime)
        {
            int totalTransitions = successfulTransitions + failedTransitions;
            averageTransitionTime = ((averageTransitionTime * (totalTransitions - 1)) + newTransitionTime) / totalTransitions;
        }

        /// <summary>
        /// 蜈ｨ菴鍋噪縺ｪ謌仙粥邇・・蜿門ｾ・        /// </summary>
        /// <returns>謌仙粥邇・ｼ・.0-1.0・・/returns>
        public float GetOverallSuccessRate()
        {
            int totalTransitions = successfulTransitions + failedTransitions;
            return totalTransitions > 0 ? (float)successfulTransitions / totalTransitions : 0f;
        }

        /// <summary>
        /// 騾ｲ陦檎憾豕√Ξ繝昴・繝医・逕滓・
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
            
            // 譛霑代・蝠城｡後・繝ｬ繝昴・繝・            ReportRecentIssues();
        }

        /// <summary>
        /// 譛霑代・蝠城｡後・繝ｬ繝昴・繝・        /// </summary>
        private void ReportRecentIssues()
        {
            var recentValidations = validationHistory.FindAll(v => Time.time - v.timestamp < 60f); // 驕主悉1蛻・            var issueValidations = recentValidations.FindAll(v => !v.allServicesWorking);
            
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
        /// 閾ｪ蜍穂ｿｮ蠕ｩ縺ｮ隧ｦ陦・        /// </summary>
        /// <param name="validation">讀懆ｨｼ邨先棡</param>
        private void AttemptAutoRepair(ValidationResult validation)
        {
            LogProgress($"Attempting auto-repair for {validation.phase}...");
            
            // 蝓ｺ譛ｬ逧・↑菫ｮ蠕ｩ謇矩・            if (!validation.serviceLocatorWorking)
            {
                LogProgress("ServiceLocator issue detected - attempting restart");
                // TODO: ServiceLocator蜀榊・譛溷喧繝ｭ繧ｸ繝・け
            }
            
            // 繧医ｊ隧ｳ邏ｰ縺ｪ菫ｮ蠕ｩ繝ｭ繧ｸ繝・け縺ｯ蠢・ｦ√↓蠢懊§縺ｦ螳溯｣・            LogProgress("Auto-repair attempt completed");
        }

        #endregion

        #region Data Persistence

        /// <summary>
        /// 騾ｲ陦檎憾豕√ョ繝ｼ繧ｿ縺ｮ菫晏ｭ・        /// </summary>
        private void SaveProgressData()
        {
            try
            {
                // PlayerPrefs縺ｫ蝓ｺ譛ｬ邨ｱ險医ｒ菫晏ｭ・                PlayerPrefs.SetInt("MigrationProgress_SuccessfulTransitions", successfulTransitions);
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
        /// 騾ｲ陦檎憾豕√ョ繝ｼ繧ｿ縺ｮ隱ｭ縺ｿ霎ｼ縺ｿ
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
        /// 繝輔ぉ繝ｼ繧ｺ繧､繝吶Φ繝医・遞ｮ鬘・        /// </summary>
        private enum PhaseEvent
        {
            Enter,
            SuccessfulExit,
            FailedExit
        }

        /// <summary>
        /// 繝輔ぉ繝ｼ繧ｺ讀懆ｨｼ縺ｮ繝倥Ν繝代・髢｢謨ｰ
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
        /// 騾ｲ陦檎憾豕√Ο繧ｰ縺ｮ蜃ｺ蜉・        /// </summary>
        /// <param name="message">繝｡繝・そ繝ｼ繧ｸ</param>
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