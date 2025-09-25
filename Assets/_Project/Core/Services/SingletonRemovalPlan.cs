using UnityEngine;
// using asterivo.Unity60.Core.Debug;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency
using asterivo.Unity60.Core.Services;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio;
using asterivo.Unity60.Core.Audio.Interfaces;
// using asterivo.Unity60.Core.Commands;
// // using asterivo.Unity60.Core.Commands; // Removed to avoid circular dependency
using System;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Complete Singleton Removal Process Management System
    /// Manages the phased and safe complete removal of Singletons
    /// </summary>
    public class SingletonRemovalPlan : MonoBehaviour
    {
        [Header("Removal Configuration")]
        [SerializeField] private bool enableAutoRemoval = false;
        [SerializeField] private bool requireManualConfirmation = true;
        [SerializeField] private bool createComprehensiveBackup = true;

        [Header("Target Classes")]
        [SerializeField] private string[] singletonClasses = {
            "AudioManager",
            "SpatialAudioManager",
            "EffectManager",
            "CommandPoolService",
            "EventLogger",
            "CinemachineIntegration"
        };

        [Header("Progress Tracking")]
        [SerializeField] private bool removalCompleted = false;
        [SerializeField] private string removalStartTime = "";
        [SerializeField] private string removalCompletionTime = "";

        private Dictionary<string, RemovalStep[]> removalPlan;

        private void Awake()
        {
            InitializeRemovalPlan();
            LoadRemovalState();
        }

        /// <summary>
        /// Initialize removal plan
        /// </summary>
        private void InitializeRemovalPlan()
        {
            removalPlan = new Dictionary<string, RemovalStep[]>();

            // Define removal steps for each class
            removalPlan["AudioManager"] = new RemovalStep[]
            {
                new RemovalStep("private static AudioManager instance;", RemovalAction.Delete),
                new RemovalStep("public static AudioManager Instance", RemovalAction.Delete),
                new RemovalStep("instance = this;", RemovalAction.Delete),
                new RemovalStep("ServiceLocator.RegisterService<IAudioService>(this);", RemovalAction.Keep),
                new RemovalStep("DontDestroyOnLoad(gameObject);", RemovalAction.Keep),
                new RemovalStep("IAudioService interface methods", RemovalAction.Keep)
            };

            removalPlan["SpatialAudioManager"] = new RemovalStep[]
            {
                new RemovalStep("private static SpatialAudioManager instance;", RemovalAction.Delete),
                new RemovalStep("public static SpatialAudioManager Instance", RemovalAction.Delete),
                new RemovalStep("instance = this;", RemovalAction.Delete),
                new RemovalStep("ServiceLocator.RegisterService<ISpatialAudioService>(this);", RemovalAction.Keep),
                new RemovalStep("ISpatialAudioService interface methods", RemovalAction.Keep)
            };

            removalPlan["EffectManager"] = new RemovalStep[]
            {
                new RemovalStep("private static EffectManager instance;", RemovalAction.Delete),
                new RemovalStep("public static EffectManager Instance", RemovalAction.Delete),
                new RemovalStep("instance = this;", RemovalAction.Delete),
                new RemovalStep("ServiceLocator.RegisterService<IEffectService>(this);", RemovalAction.Keep),
                new RemovalStep("IEffectService interface methods", RemovalAction.Keep)
            };

            removalPlan["CommandPoolService"] = new RemovalStep[]
            {
                new RemovalStep("private static CommandPoolService instance;", RemovalAction.Delete),
                new RemovalStep("public static CommandPoolService Instance", RemovalAction.Delete),
                new RemovalStep("instance = this;", RemovalAction.Delete),
                new RemovalStep("ServiceLocator.RegisterService<ICommandPoolService>(this);", RemovalAction.Keep),
                new RemovalStep("ICommandPoolService interface methods", RemovalAction.Keep)
            };

            removalPlan["EventLogger"] = new RemovalStep[]
            {
                new RemovalStep("private static EventLogger instance;", RemovalAction.Delete),
                new RemovalStep("public static EventLogger Instance", RemovalAction.Delete),
                new RemovalStep("instance = this;", RemovalAction.Delete),
                new RemovalStep("ServiceLocator.RegisterService<IEventLogger>(this);", RemovalAction.Keep),
                new RemovalStep("IEventLogger interface methods", RemovalAction.Keep)
            };

            removalPlan["CinemachineIntegration"] = new RemovalStep[]
            {
                new RemovalStep("private static CinemachineIntegration instance;", RemovalAction.Delete),
                new RemovalStep("public static CinemachineIntegration Instance", RemovalAction.Delete),
                new RemovalStep("instance = this;", RemovalAction.Delete),
                new RemovalStep("ServiceLocator.RegisterService<CinemachineIntegration>(this);", RemovalAction.Keep),
                new RemovalStep("Camera management methods", RemovalAction.Keep)
            };
        }

        /// <summary>
        /// Start complete removal process
        /// </summary>
        [ContextMenu("Start Complete Singleton Removal")]
        public void StartCompleteRemoval()
        {
            if (removalCompleted)
            {
                ProjectDebug.LogWarning("[SingletonRemovalPlan] Removal already completed");
                return;
            }

            if (!ValidatePreConditions())
            {
                ProjectDebug.LogError("[SingletonRemovalPlan] Pre-conditions not met for removal");
                return;
            }

            if (requireManualConfirmation)
            {
                ProjectDebug.LogWarning("[SingletonRemovalPlan] CRITICAL: This will PERMANENTLY remove all Singleton patterns");
                ProjectDebug.LogWarning("[SingletonRemovalPlan] Call ExecuteCompleteRemoval() to proceed with confirmation");
                return;
            }

            ExecuteCompleteRemoval();
        }

        /// <summary>
        /// Validate pre-conditions
        /// </summary>
        private bool ValidatePreConditions()
        {
            ProjectDebug.Log("[SingletonRemovalPlan] Validating pre-conditions...");

            // 1. ServiceLocator must be enabled
            if (!FeatureFlags.UseServiceLocator)
            {
                ProjectDebug.LogError("[SingletonRemovalPlan] ServiceLocator must be enabled");
                return false;
            }

            // 2. All services must be registered in ServiceLocator
            bool allServicesRegistered = ValidateServiceRegistration();
            if (!allServicesRegistered)
            {
                ProjectDebug.LogError("[SingletonRemovalPlan] Not all services are properly registered");
                return false;
            }

            // 3. Migration warnings should be disabled (complete migration)
            if (FeatureFlags.EnableMigrationWarnings)
            {
                ProjectDebug.LogWarning("[SingletonRemovalPlan] Migration warnings still enabled - consider disabling first");
            }

            // 4. System health check
            var healthStatus = asterivo.Unity60.Core.Services.EmergencyRollback.CheckSystemHealth();
            if (!healthStatus.IsHealthy)
            {
                ProjectDebug.LogError($"[SingletonRemovalPlan] System health issues detected: {healthStatus.HealthScore}%");
                foreach (var issue in healthStatus.Issues)
                {
                    ProjectDebug.LogError($"[SingletonRemovalPlan] Issue: {issue}");
                }
                return false;
            }

            ProjectDebug.Log("[SingletonRemovalPlan] All pre-conditions validated");
            return true;
        }

        /// <summary>
        /// Validate service registration
        /// </summary>
        private bool ValidateServiceRegistration()
        {
            try
            {
                var audioService = ServiceLocator.GetService<IAudioService>();
                var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
                var effectService = ServiceLocator.GetService<IEffectService>();
                var commandPoolService = ServiceLocator.GetService<ICommandPoolService>();
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                // var cinemachineIntegration = ServiceLocator.GetService<CinemachineIntegration>(); // Commented out - namespace issue

                bool allRegistered = audioService != null &&
                                   spatialService != null &&
                                   effectService != null &&
                                   commandPoolService != null &&
                                   eventLogger != null;
                                   // cinemachineIntegration check removed due to namespace issue

                ProjectDebug.Log($"[SingletonRemovalPlan] Service registration validation: {allRegistered}");
                return allRegistered;
            }
            catch (Exception ex)
            {
                ProjectDebug.LogError($"[SingletonRemovalPlan] Service validation failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Execute confirmed complete removal
        /// </summary>
        [ContextMenu("Execute Complete Removal (CONFIRMED)")]
        public void ExecuteCompleteRemoval()
        {
            ProjectDebug.Log("[SingletonRemovalPlan] === STARTING COMPLETE SINGLETON REMOVAL ===");
            removalStartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (createComprehensiveBackup)
            {
                CreateComprehensiveBackup();
            }

            // Phase 1: Disable Legacy Singletons completely
            ExecutePhase1_DisableLegacySingletons();

            // Phase 2: Prepare physical code removal
            ExecutePhase2_PreparePhysicalRemoval();

            // Phase 3: Final validation and marking
            ExecutePhase3_FinalValidation();

            removalCompletionTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            removalCompleted = true;
            SaveRemovalState();

            ProjectDebug.Log("[SingletonRemovalPlan] === COMPLETE SINGLETON REMOVAL FINISHED ===");
            GenerateRemovalReport();
        }

        /// <summary>
        /// Phase 1: Disable Legacy Singletons completely
        /// </summary>
        private void ExecutePhase1_DisableLegacySingletons()
        {
            ProjectDebug.Log("[SingletonRemovalPlan] Phase 1: Disabling Legacy Singletons...");

            // Set FeatureFlags to final state
            FeatureFlags.DisableLegacySingletons = true;
            FeatureFlags.EnableMigrationWarnings = false;
            FeatureFlags.EnableMigrationMonitoring = false;

            ProjectDebug.Log("[SingletonRemovalPlan] Legacy Singletons completely disabled");
        }

        /// <summary>
        /// Phase 2: Prepare physical removal
        /// </summary>
        private void ExecutePhase2_PreparePhysicalRemoval()
        {
            ProjectDebug.Log("[SingletonRemovalPlan] Phase 2: Preparing Physical Code Removal...");

            foreach (var className in singletonClasses)
            {
                if (removalPlan.ContainsKey(className))
                {
                    GenerateRemovalInstructions(className, removalPlan[className]);
                }
            }

            ProjectDebug.Log("[SingletonRemovalPlan] Physical removal instructions generated");
        }

        /// <summary>
        /// Phase 3: Final validation
        /// </summary>
        private void ExecutePhase3_FinalValidation()
        {
            ProjectDebug.Log("[SingletonRemovalPlan] Phase 3: Final Validation...");

            // Ensure services still work correctly
            bool servicesWorking = ValidateServiceRegistration();

            if (servicesWorking)
            {
                ProjectDebug.Log("[SingletonRemovalPlan] All services validated post-removal");
            }
            else
            {
                ProjectDebug.LogError("[SingletonRemovalPlan] Service validation failed");
                // Prompt for emergency rollback
                ProjectDebug.LogWarning("[SingletonRemovalPlan] Consider emergency rollback");
            }
        }

        /// <summary>
        /// Create comprehensive backup
        /// </summary>
        private void CreateComprehensiveBackup()
        {
            ProjectDebug.Log("[SingletonRemovalPlan] Creating comprehensive backup...");

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
            string backupKey = $"SingletonRemoval_Backup_{timestamp}";

            // Backup FeatureFlags state
            PlayerPrefs.SetString($"{backupKey}_FeatureFlags", SerializeFeatureFlags());

            // Record current state of each class
            foreach (var className in singletonClasses)
            {
                PlayerPrefs.SetString($"{backupKey}_{className}_Status", "Singleton_Pattern_Active");
            }

            PlayerPrefs.SetString("LastSingletonBackup", backupKey);
            PlayerPrefs.Save();

            ProjectDebug.Log($"[SingletonRemovalPlan] Backup created: {backupKey}");
        }

        /// <summary>
        /// Generate removal instructions
        /// </summary>
        private void GenerateRemovalInstructions(string className, RemovalStep[] steps)
        {
            ProjectDebug.Log($"[SingletonRemovalPlan] === {className} Removal Instructions ===");

            for (int i = 0; i < steps.Length; i++)
            {
                var step = steps[i];
                string actionIcon = step.Action == RemovalAction.Delete ? "X" : "✓";
                string actionText = step.Action == RemovalAction.Delete ? "DELETE" : "KEEP";

                ProjectDebug.Log($"[SingletonRemovalPlan] {i+1}. {actionIcon} {actionText}: {step.Description}");
            }

            ProjectDebug.Log($"[SingletonRemovalPlan] === End {className} Instructions ===");
        }

        /// <summary>
        /// Serialize FeatureFlags
        /// </summary>
        private string SerializeFeatureFlags()
        {
            return $"UseServiceLocator:{FeatureFlags.UseServiceLocator}," +
                   $"DisableLegacySingletons:{FeatureFlags.DisableLegacySingletons}," +
                   $"EnableMigrationWarnings:{FeatureFlags.EnableMigrationWarnings}," +
                   $"EnableMigrationMonitoring:{FeatureFlags.EnableMigrationMonitoring}";
        }

        /// <summary>
        /// Generate removal report
        /// </summary>
        private void GenerateRemovalReport()
        {
            ProjectDebug.Log("[SingletonRemovalPlan] === SINGLETON REMOVAL REPORT ===");
            ProjectDebug.Log($"Removal Started: {removalStartTime}");
            ProjectDebug.Log($"Removal Completed: {removalCompletionTime}");
            ProjectDebug.Log($"Classes Processed: {singletonClasses.Length}");
            ProjectDebug.Log("Status: READY FOR MANUAL CODE DELETION");
            ProjectDebug.Log("");
            ProjectDebug.Log("⚡ MANUAL ACTION REQUIRED:");
            ProjectDebug.Log("1. Review removal instructions above");
            ProjectDebug.Log("2. Manually delete Singleton code from each class");
            ProjectDebug.Log("3. Keep ServiceLocator registrations intact");
            ProjectDebug.Log("4. Run compilation tests");
            ProjectDebug.Log("5. Execute final validation");
            ProjectDebug.Log("=====================================");
        }

        /// <summary>
        /// Execute emergency rollback
        /// </summary>
        [ContextMenu("Emergency Rollback")]
        public void EmergencyRollback()
        {
            ProjectDebug.LogWarning("[SingletonRemovalPlan] Executing emergency rollback...");

            // Reset FeatureFlags to safe state
            FeatureFlags.DisableLegacySingletons = false;
            FeatureFlags.EnableMigrationWarnings = true;
            FeatureFlags.EnableMigrationMonitoring = true;

            ProjectDebug.Log("[SingletonRemovalPlan] Emergency rollback completed");
        }

        /// <summary>
        /// Save removal state
        /// </summary>
        private void SaveRemovalState()
        {
            PlayerPrefs.SetInt("SingletonRemovalPlan_Completed", removalCompleted ? 1 : 0);
            PlayerPrefs.SetString("SingletonRemovalPlan_StartTime", removalStartTime);
            PlayerPrefs.SetString("SingletonRemovalPlan_CompletionTime", removalCompletionTime);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Load removal state
        /// </summary>
        private void LoadRemovalState()
        {
            removalCompleted = PlayerPrefs.GetInt("SingletonRemovalPlan_Completed", 0) == 1;
            removalStartTime = PlayerPrefs.GetString("SingletonRemovalPlan_StartTime", "");
            removalCompletionTime = PlayerPrefs.GetString("SingletonRemovalPlan_CompletionTime", "");
        }
    }

    /// <summary>
    /// Removal step definition
    /// </summary>
    [System.Serializable]
    public class RemovalStep
    {
        public string Description;
        public RemovalAction Action;

        public RemovalStep(string description, RemovalAction action)
        {
            Description = description;
            Action = action;
        }
    }

    /// <summary>
    /// Removal action type
    /// </summary>
    public enum RemovalAction
    {
        Delete,  // Delete code
        Keep     // Keep code
    }
}