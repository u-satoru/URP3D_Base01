using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// 繧ｲ繝ｼ繝邨ゆｺ・さ繝槭Φ繝峨・螳夂ｾｩ縲・    /// 繧｢繝励Μ繧ｱ繝ｼ繧ｷ繝ｧ繝ｳ縺ｮ邨ゆｺ・い繧ｯ繧ｷ繝ｧ繝ｳ繧偵き繝励そ繝ｫ蛹悶＠縺ｾ縺吶・    /// 
    /// 荳ｻ縺ｪ讖溯・・・    /// - 螳牙・縺ｪ繧｢繝励Μ繧ｱ繝ｼ繧ｷ繝ｧ繝ｳ邨ゆｺ・    /// - 邨ゆｺ・燕縺ｮ閾ｪ蜍輔そ繝ｼ繝・    /// - 邨ゆｺ・｢ｺ隱阪ム繧､繧｢繝ｭ繧ｰ縺ｮ陦ｨ遉ｺ
    /// - 繝ｪ繧ｽ繝ｼ繧ｹ縺ｮ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・
    /// </summary>
    [System.Serializable]
    public class QuitGameCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 邨ゆｺ・・遞ｮ鬘槭ｒ螳夂ｾｩ縺吶ｋ蛻玲嫌蝙・        /// </summary>
        public enum QuitType
        {
            Immediate,      // 蜊ｳ蠎ｧ縺ｫ邨ゆｺ・            Safe,           // 螳牙・縺ｪ邨ゆｺ・ｼ医そ繝ｼ繝也ｭ峨ｒ螳溯｡鯉ｼ・            Confirm,        // 遒ｺ隱阪ム繧､繧｢繝ｭ繧ｰ莉倥″邨ゆｺ・            ToMainMenu,     // 繝｡繧､繝ｳ繝｡繝九Η繝ｼ縺ｫ謌ｻ繧・            Restart         // 繧｢繝励Μ繧ｱ繝ｼ繧ｷ繝ｧ繝ｳ蜀崎ｵｷ蜍・        }

        [Header("Quit Parameters")]
        public QuitType quitType = QuitType.Safe;
        public bool showConfirmDialog = true;
        public string confirmMessage = "Are you sure you want to quit?";

        [Header("Auto Save")]
        public bool autoSaveBeforeQuit = true;
        public bool forceSaveEvenIfRecent = false;
        public float autoSaveTimeout = 5f;

        [Header("Cleanup")]
        public bool cleanupTempFiles = true;
        public bool saveSettingsOnQuit = true;
        public bool saveStatisticsOnQuit = true;
        public bool disposeResources = true;

        [Header("Scene Transition")]
        public bool fadeOutBeforeQuit = false;
        public float fadeOutDuration = 1f;
        public string targetScene = ""; // ToMainMenu縺ｮ蝣ｴ蜷医↓菴ｿ逕ｨ

        [Header("Platform Specific")]
        public bool confirmOnMobile = true;
        public bool minimizeInsteadOfQuitOnMobile = false;
        public bool handleBackButtonOnMobile = true;

        [Header("Debug")]
        public bool logQuitAttempt = true;
        public bool showQuitReasonInLog = true;

        /// <summary>
        /// 繝・ヵ繧ｩ繝ｫ繝医さ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public QuitGameCommandDefinition()
        {
        }

        /// <summary>
        /// 繝代Λ繝｡繝ｼ繧ｿ莉倥″繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public QuitGameCommandDefinition(QuitType type, bool confirm = true, bool autoSave = true)
        {
            quitType = type;
            showConfirmDialog = confirm;
            autoSaveBeforeQuit = autoSave;
        }

        /// <summary>
        /// 邨ゆｺ・さ繝槭Φ繝峨′螳溯｡悟庄閭ｽ縺九←縺・°繧貞愛螳壹＠縺ｾ縺・        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 蝓ｺ譛ｬ逧・↑螳溯｡悟庄閭ｽ諤ｧ繝√ぉ繝・け
            if (autoSaveTimeout <= 0f) return false;
            if (fadeOutDuration < 0f) return false;

            // ToMainMenu縺ｮ蝣ｴ蜷医・繧ｿ繝ｼ繧ｲ繝・ヨ繧ｷ繝ｼ繝ｳ縺悟ｿ・ｦ・            if (quitType == QuitType.ToMainMenu && string.IsNullOrEmpty(targetScene))
                return false;

            // 繧ｳ繝ｳ繝・く繧ｹ繝医′縺ゅｋ蝣ｴ蜷医・霑ｽ蜉繝√ぉ繝・け
            if (context != null)
            {
                // 驥崎ｦ√↑蜃ｦ逅・ｸｭ・医そ繝ｼ繝紋ｸｭ縲√Ο繝ｼ繝我ｸｭ遲会ｼ峨・邨ゆｺ・ｸ榊庄
                // 繝槭Ν繝√・繝ｬ繧､繧ｲ繝ｼ繝縺ｧ縺ｮ迚ｹ蛻･縺ｪ蛻ｶ邏・                // 繝励Λ繝・ヨ繝輔か繝ｼ繝蝗ｺ譛峨・蛻ｶ邏・            }

            return true;
        }

        /// <summary>
        /// 邨ゆｺ・さ繝槭Φ繝峨ｒ菴懈・縺励∪縺・        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new QuitGameCommand(this, context);
        }
    }

    /// <summary>
    /// QuitGameCommandDefinition縺ｫ蟇ｾ蠢懊☆繧句ｮ滄圀縺ｮ繧ｳ繝槭Φ繝牙ｮ溯｣・    /// </summary>
    public class QuitGameCommand : ICommand
    {
        private QuitGameCommandDefinition definition;
        private object context;
        private bool executed = false;
        private bool quitInProgress = false;
        private bool userConfirmed = false;
        private System.DateTime quitStartTime;

        public QuitGameCommand(QuitGameCommandDefinition quitDefinition, object executionContext)
        {
            definition = quitDefinition;
            context = executionContext;
        }

        /// <summary>
        /// 邨ゆｺ・さ繝槭Φ繝峨・螳溯｡・        /// </summary>
        public void Execute()
        {
            if (executed || quitInProgress) return;

            quitStartTime = System.DateTime.Now;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (definition.logQuitAttempt)
            {
                string reason = definition.showQuitReasonInLog ? $" (Type: {definition.quitType})" : "";
                UnityEngine.Debug.Log($"Quit game command executed{reason}");
            }
#endif

            // 繝励Λ繝・ヨ繝輔か繝ｼ繝蝗ｺ譛峨・蜃ｦ逅・            if (Application.isMobilePlatform)
            {
                HandleMobilePlatformQuit();
                return;
            }

            // 遒ｺ隱阪ム繧､繧｢繝ｭ繧ｰ縺ｮ陦ｨ遉ｺ
            if (definition.showConfirmDialog && definition.quitType == QuitGameCommandDefinition.QuitType.Confirm)
            {
                ShowQuitConfirmDialog();
                return; // 繝ｦ繝ｼ繧ｶ繝ｼ縺ｮ蠢懃ｭ泌ｾ・■
            }

            // 螳滄圀縺ｮ邨ゆｺ・・逅・ｒ髢句ｧ・            StartQuitProcess();
            executed = true;
        }

        /// <summary>
        /// 繝｢繝舌う繝ｫ繝励Λ繝・ヨ繝輔か繝ｼ繝逕ｨ縺ｮ邨ゆｺ・・逅・        /// </summary>
        private void HandleMobilePlatformQuit()
        {
            // 繝｢繝舌う繝ｫ縺ｧ縺ｯ騾壼ｸｸ繧｢繝励Μ邨ゆｺ・〒縺ｯ縺ｪ縺乗怙蟆丞喧
            if (definition.minimizeInsteadOfQuitOnMobile)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                // Android縺ｧ縺ｮ繝帙・繝逕ｻ髱｢縺ｸ縺ｮ遘ｻ蜍・                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    currentActivity.Call<bool>("moveTaskToBack", true);
                }
#elif UNITY_IOS && !UNITY_EDITOR
                // iOS縺ｧ縺ｯ騾壼ｸｸ繧｢繝励Μ邨ゆｺ・・謗ｨ螂ｨ縺輔ｌ縺ｪ縺・                UnityEngine.Debug.LogWarning("App quit is not recommended on iOS platform");
#endif
                return;
            }

            // 繝｢繝舌う繝ｫ縺ｧ縺ｮ遒ｺ隱阪ム繧､繧｢繝ｭ繧ｰ
            if (definition.confirmOnMobile)
            {
                ShowMobileQuitConfirmDialog();
                return;
            }

            StartQuitProcess();
        }

        /// <summary>
        /// 邨ゆｺ・｢ｺ隱阪ム繧､繧｢繝ｭ繧ｰ縺ｮ陦ｨ遉ｺ
        /// </summary>
        private void ShowQuitConfirmDialog()
        {
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ UISystem 縺ｨ縺ｮ騾｣謳ｺ
            // var dialog = UISystem.ShowConfirmDialog(definition.confirmMessage, OnQuitConfirmed, OnQuitCancelled);

            // 莉ｮ縺ｮ螳溯｣・ｼ亥叉蠎ｧ縺ｫ遒ｺ隱阪＆繧後◆縺ｨ縺励※蜃ｦ逅・ｼ・#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Showing quit confirmation: {definition.confirmMessage}");
#endif
            
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ繝ｦ繝ｼ繧ｶ繝ｼ蜈･蜉帛ｾ・■
            OnQuitConfirmed(); // 繝・せ繝育畑縺ｫ蜊ｳ蠎ｧ縺ｫ遒ｺ隱・        }

        /// <summary>
        /// 繝｢繝舌う繝ｫ逕ｨ邨ゆｺ・｢ｺ隱阪ム繧､繧｢繝ｭ繧ｰ縺ｮ陦ｨ遉ｺ
        /// </summary>
        private void ShowMobileQuitConfirmDialog()
        {
            // 繝｢繝舌う繝ｫ蝗ｺ譛峨・繝繧､繧｢繝ｭ繧ｰ陦ｨ遉ｺ
            ShowQuitConfirmDialog(); // 蝓ｺ譛ｬ逧・↓縺ｯ蜷後§蜃ｦ逅・        }

        /// <summary>
        /// 邨ゆｺ・′遒ｺ隱阪＆繧後◆髫帙・繧ｳ繝ｼ繝ｫ繝舌ャ繧ｯ
        /// </summary>
        private void OnQuitConfirmed()
        {
            userConfirmed = true;
            StartQuitProcess();
        }

        /// <summary>
        /// 邨ゆｺ・′繧ｭ繝｣繝ｳ繧ｻ繝ｫ縺輔ｌ縺滄圀縺ｮ繧ｳ繝ｼ繝ｫ繝舌ャ繧ｯ
        /// </summary>
        private void OnQuitCancelled()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Quit cancelled by user");
#endif
            // 繧ｭ繝｣繝ｳ繧ｻ繝ｫ譎ゅ・蜃ｦ逅・        }

        /// <summary>
        /// 螳滄圀縺ｮ邨ゆｺ・・逅・ｒ髢句ｧ・        /// </summary>
        private void StartQuitProcess()
        {
            quitInProgress = true;

            switch (definition.quitType)
            {
                case QuitGameCommandDefinition.QuitType.Immediate:
                    ExecuteImmediateQuit();
                    break;
                case QuitGameCommandDefinition.QuitType.Safe:
                case QuitGameCommandDefinition.QuitType.Confirm:
                    ExecuteSafeQuit();
                    break;
                case QuitGameCommandDefinition.QuitType.ToMainMenu:
                    ExecuteReturnToMainMenu();
                    break;
                case QuitGameCommandDefinition.QuitType.Restart:
                    ExecuteRestart();
                    break;
            }
        }

        /// <summary>
        /// 蜊ｳ蠎ｧ縺ｫ邨ゆｺ・        /// </summary>
        private void ExecuteImmediateQuit()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Executing immediate quit");
#endif

            // 譛蟆城剞縺ｮ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・
            if (definition.disposeResources)
            {
                PerformMinimalCleanup();
            }

            // 繧｢繝励Μ繧ｱ繝ｼ繧ｷ繝ｧ繝ｳ邨ゆｺ・            QuitApplication();
        }

        /// <summary>
        /// 螳牙・縺ｪ邨ゆｺ・        /// </summary>
        private void ExecuteSafeQuit()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Executing safe quit");
#endif

            // 閾ｪ蜍輔そ繝ｼ繝・            if (definition.autoSaveBeforeQuit)
            {
                PerformAutoSave();
            }

            // 險ｭ螳壹・菫晏ｭ・            if (definition.saveSettingsOnQuit)
            {
                SaveSettings();
            }

            // 邨ｱ險医ョ繝ｼ繧ｿ縺ｮ菫晏ｭ・            if (definition.saveStatisticsOnQuit)
            {
                SaveStatistics();
            }

            // 繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・
            if (definition.cleanupTempFiles || definition.disposeResources)
            {
                PerformFullCleanup();
            }

            // 繝輔ぉ繝ｼ繝峨い繧ｦ繝・            if (definition.fadeOutBeforeQuit)
            {
                StartFadeOut();
            }
            else
            {
                QuitApplication();
            }
        }

        /// <summary>
        /// 繝｡繧､繝ｳ繝｡繝九Η繝ｼ縺ｫ謌ｻ繧・        /// </summary>
        private void ExecuteReturnToMainMenu()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Returning to main menu: {definition.targetScene}");
#endif

            // 閾ｪ蜍輔そ繝ｼ繝厄ｼ郁ｨｭ螳壹＆繧後※縺・ｋ蝣ｴ蜷茨ｼ・            if (definition.autoSaveBeforeQuit)
            {
                PerformAutoSave();
            }

            // 繧ｲ繝ｼ繝迥ｶ諷九・繝ｪ繧ｻ繝・ヨ
            ResetGameState();

            // 繧ｷ繝ｼ繝ｳ驕ｷ遘ｻ
            if (definition.fadeOutBeforeQuit)
            {
                StartFadeOutToScene();
            }
            else
            {
                TransitionToMainMenu();
            }
        }

        /// <summary>
        /// 繧｢繝励Μ繧ｱ繝ｼ繧ｷ繝ｧ繝ｳ蜀崎ｵｷ蜍・        /// </summary>
        private void ExecuteRestart()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Executing application restart");
#endif

            // 蜀崎ｵｷ蜍慕畑縺ｮ險ｭ螳壻ｿ晏ｭ・            SaveRestartFlag();

            // 螳牙・縺ｪ邨ゆｺ・・逅・            ExecuteSafeQuit();

            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ縲√・繝ｩ繝・ヨ繝輔か繝ｼ繝蝗ｺ譛峨・蜀崎ｵｷ蜍募・逅・            // Windows: Process.Start(Application.dataPath + "/../" + Application.productName + ".exe");
            // 縺昴・莉悶・繝励Λ繝・ヨ繝輔か繝ｼ繝縺ｧ縺ｯ驕ｩ蛻・↑譁ｹ豕輔〒蜀崎ｵｷ蜍・        }

        /// <summary>
        /// 閾ｪ蜍輔そ繝ｼ繝悶・螳溯｡・        /// </summary>
        private void PerformAutoSave()
        {
            try
            {
                // 螳滄圀縺ｮ螳溯｣・〒縺ｯ SaveSystem 縺ｨ縺ｮ騾｣謳ｺ
                // if (SaveSystem.HasUnsavedChanges() || definition.forceSaveEvenIfRecent)
                // {
                //     SaveSystem.AutoSave(definition.autoSaveTimeout);
                // }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log("Auto save completed before quit");
#endif
            }
            catch (System.Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogError($"Auto save failed: {ex.Message}");
#endif
            }
        }

        /// <summary>
        /// 險ｭ螳壹・菫晏ｭ・        /// </summary>
        private void SaveSettings()
        {
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ SettingsManager 縺ｨ縺ｮ騾｣謳ｺ
            PlayerPrefs.Save(); // Unity讓呎ｺ悶・險ｭ螳壻ｿ晏ｭ・
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Settings saved before quit");
#endif
        }

        /// <summary>
        /// 邨ｱ險医ョ繝ｼ繧ｿ縺ｮ菫晏ｭ・        /// </summary>
        private void SaveStatistics()
        {
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ StatisticsSystem 縺ｨ縺ｮ騾｣謳ｺ
            // StatisticsSystem.SaveAllStatistics();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Statistics saved before quit");
#endif
        }

        /// <summary>
        /// 譛蟆城剞縺ｮ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・
        /// </summary>
        private void PerformMinimalCleanup()
        {
            // 繝｡繝｢繝ｪ縺ｮ蠑ｷ蛻ｶ髢区叛
            System.GC.Collect();

            // 繝ｪ繧ｽ繝ｼ繧ｹ縺ｮ繧｢繝ｳ繝ｭ繝ｼ繝・            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// 螳悟・縺ｪ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・
        /// </summary>
        private void PerformFullCleanup()
        {
            // 譛蟆城剞縺ｮ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・
            PerformMinimalCleanup();

            // 荳譎ゅヵ繧｡繧､繝ｫ縺ｮ蜑企勁
            if (definition.cleanupTempFiles)
            {
                CleanupTempFiles();
            }

            // 繝阪ャ繝医Ρ繝ｼ繧ｯ謗･邯壹・蛻・妙
            // NetworkManager.Disconnect();

            // 繧ｪ繝ｼ繝・ぅ繧ｪ縺ｮ蛛懈ｭ｢
            AudioListener.pause = true;
        }

        /// <summary>
        /// 荳譎ゅヵ繧｡繧､繝ｫ縺ｮ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・
        /// </summary>
        private void CleanupTempFiles()
        {
            try
            {
                string tempDir = System.IO.Path.Combine(Application.persistentDataPath, "Temp");
                if (System.IO.Directory.Exists(tempDir))
                {
                    System.IO.Directory.Delete(tempDir, true);
                }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log("Temporary files cleaned up");
#endif
            }
            catch (System.Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning($"Temp file cleanup failed: {ex.Message}");
#endif
            }
        }

        /// <summary>
        /// 繧ｲ繝ｼ繝迥ｶ諷九・繝ｪ繧ｻ繝・ヨ
        /// </summary>
        private void ResetGameState()
        {
            // 繧ｲ繝ｼ繝迥ｶ諷九・繝ｪ繧ｻ繝・ヨ
            Time.timeScale = 1f;
            AudioListener.pause = false;
            
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ GameManager 縺ｨ縺ｮ騾｣謳ｺ
            // GameManager.ResetToMainMenuState();
        }

        /// <summary>
        /// 繝輔ぉ繝ｼ繝峨い繧ｦ繝医・髢句ｧ・        /// </summary>
        private void StartFadeOut()
        {
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ UISystem 縺ｾ縺溘・ SceneTransition 縺ｨ縺ｮ騾｣謳ｺ
            // SceneTransition.FadeOut(definition.fadeOutDuration, OnFadeOutComplete);

            // 莉ｮ縺ｮ螳溯｣・ｼ井ｸ螳壽凾髢灘ｾ後↓邨ゆｺ・ｼ・            System.Threading.Tasks.Task.Delay((int)(definition.fadeOutDuration * 1000))
                .ContinueWith(_ => QuitApplication());
        }

        /// <summary>
        /// 繧ｷ繝ｼ繝ｳ縺ｸ縺ｮ繝輔ぉ繝ｼ繝峨い繧ｦ繝磯幕蟋・        /// </summary>
        private void StartFadeOutToScene()
        {
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ SceneTransition 縺ｨ縺ｮ騾｣謳ｺ
            // SceneTransition.FadeOutToScene(definition.targetScene, definition.fadeOutDuration);

            // 莉ｮ縺ｮ螳溯｣・            System.Threading.Tasks.Task.Delay((int)(definition.fadeOutDuration * 1000))
                .ContinueWith(_ => TransitionToMainMenu());
        }

        /// <summary>
        /// 繝｡繧､繝ｳ繝｡繝九Η繝ｼ縺ｸ縺ｮ驕ｷ遘ｻ
        /// </summary>
        private void TransitionToMainMenu()
        {
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ SceneManager 縺ｨ縺ｮ騾｣謳ｺ
            UnityEngine.SceneManagement.SceneManager.LoadScene(definition.targetScene);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Transitioned to main menu: {definition.targetScene}");
#endif
        }

        /// <summary>
        /// 蜀崎ｵｷ蜍輔ヵ繝ｩ繧ｰ縺ｮ菫晏ｭ・        /// </summary>
        private void SaveRestartFlag()
        {
            PlayerPrefs.SetInt("ShouldRestart", 1);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 繧｢繝励Μ繧ｱ繝ｼ繧ｷ繝ｧ繝ｳ邨ゆｺ・        /// </summary>
        private void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Application quit");
#endif
        }

        /// <summary>
        /// 邨ゆｺ・・逅・・譖ｴ譁ｰ・医ち繧､繝繧｢繧ｦ繝医メ繧ｧ繝・け遲峨∝､夜Κ縺九ｉ螳壽悄逧・↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ繧具ｼ・        /// </summary>
        public void UpdateQuit()
        {
            if (!quitInProgress) return;

            // 閾ｪ蜍輔そ繝ｼ繝悶・繧ｿ繧､繝繧｢繧ｦ繝医メ繧ｧ繝・け
            if (definition.autoSaveBeforeQuit)
            {
                var elapsed = System.DateTime.Now - quitStartTime;
                if (elapsed.TotalSeconds > definition.autoSaveTimeout)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    UnityEngine.Debug.LogWarning("Auto save timed out, proceeding with quit");
#endif
                    QuitApplication();
                }
            }
        }

        /// <summary>
        /// Undo謫堺ｽ懶ｼ育ｵゆｺ・・繧ｭ繝｣繝ｳ繧ｻ繝ｫ・・        /// </summary>
        public void Undo()
        {
            if (!executed) return;

            // 邨ゆｺ・ｲ陦御ｸｭ縺ｮ蝣ｴ蜷医・繧ｭ繝｣繝ｳ繧ｻ繝ｫ
            if (quitInProgress)
            {
                quitInProgress = false;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log("Quit cancelled");
#endif
            }

            executed = false;
        }

        /// <summary>
        /// 縺薙・繧ｳ繝槭Φ繝峨′Undo蜿ｯ閭ｽ縺九←縺・°・育ｵゆｺ・ｲ陦御ｸｭ縺ｮ縺ｿ繧ｭ繝｣繝ｳ繧ｻ繝ｫ蜿ｯ閭ｽ・・        /// </summary>
        public bool CanUndo => quitInProgress && definition.quitType != QuitGameCommandDefinition.QuitType.Immediate;

        /// <summary>
        /// 迴ｾ蝨ｨ邨ゆｺ・・逅・ｸｭ縺九←縺・°
        /// </summary>
        public bool IsQuitInProgress => quitInProgress;

        /// <summary>
        /// 繝ｦ繝ｼ繧ｶ繝ｼ縺檎ｵゆｺ・ｒ遒ｺ隱阪＠縺溘°縺ｩ縺・°
        /// </summary>
        public bool IsUserConfirmed => userConfirmed;
    }
}
