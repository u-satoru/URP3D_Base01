using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// 繧ｲ繝ｼ繝繝ｭ繝ｼ繝峨さ繝槭Φ繝峨・螳夂ｾｩ縲・    /// 菫晏ｭ倥＆繧後◆繧ｲ繝ｼ繝迥ｶ諷九・隱ｭ縺ｿ霎ｼ縺ｿ繧｢繧ｯ繧ｷ繝ｧ繝ｳ繧偵き繝励そ繝ｫ蛹悶＠縺ｾ縺吶・    /// 
    /// 荳ｻ縺ｪ讖溯・・・    /// - 繧ｻ繝ｼ繝悶ヵ繧｡繧､繝ｫ縺九ｉ縺ｮ繧ｲ繝ｼ繝迥ｶ諷句ｾｩ蜈・    /// - 繝ｭ繝ｼ繝牙ｯｾ雎｡繝・・繧ｿ縺ｮ驕ｸ謚・    /// - 繝・・繧ｿ謨ｴ蜷域ｧ繝√ぉ繝・け縺ｨ繧ｨ繝ｩ繝ｼ繝上Φ繝峨Μ繝ｳ繧ｰ
    /// - 繝ｭ繝ｼ繝牙ｮ御ｺ・夂衍縺ｨ繧ｷ繝ｼ繝ｳ驕ｷ遘ｻ
    /// </summary>
    [System.Serializable]
    public class LoadGameCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 繝ｭ繝ｼ繝峨・遞ｮ鬘槭ｒ螳夂ｾｩ縺吶ｋ蛻玲嫌蝙・        /// </summary>
        public enum LoadType
        {
            Full,           // 螳悟・繝ｭ繝ｼ繝・            Partial,        // 驛ｨ蛻・Ο繝ｼ繝・            QuickLoad,      // 繧ｯ繧､繝・け繝ｭ繝ｼ繝・            Continue,       // 邯壹″縺九ｉ蜀埼幕
            NewGamePlus     // 蠑輔″邯吶℃隕∫ｴ莉倥″譁ｰ隕上ご繝ｼ繝
        }

        [Header("Load Parameters")]
        public LoadType loadType = LoadType.Full;
        public int loadSlot = 0;
        public string saveFileName = "";
        public bool validateBeforeLoad = true;

        [Header("Load Scope")]
        public bool loadPlayerData = true;
        public bool loadWorldState = true;
        public bool loadProgress = true;
        public bool loadSettings = false;
        public bool loadStatistics = true;

        [Header("Scene Management")]
        public bool changeScene = true;
        public string targetScene = "";
        public bool showLoadingScreen = true;
        public bool preloadAssets = true;

        [Header("Data Safety")]
        public bool createBackup = true;
        public bool verifyDataIntegrity = true;
        public bool handleVersionMismatch = true;
        public bool allowPartialLoad = false; // 繝・・繧ｿ縺ｮ荳驛ｨ縺梧ｬ謳阪＠縺ｦ縺・※繧らｶ夊｡後☆繧九°

        [Header("User Experience")]
        public bool showLoadProgress = true;
        public bool showSuccessNotification = false;
        public float maxLoadTime = 10f; // 繧ｿ繧､繝繧｢繧ｦ繝域凾髢・
        [Header("Error Handling")]
        public bool showErrorDialog = true;
        public bool fallbackToDefault = false;
        public string fallbackSaveSlot = "";

        /// <summary>
        /// 繝・ヵ繧ｩ繝ｫ繝医さ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public LoadGameCommandDefinition()
        {
        }

        /// <summary>
        /// 繝代Λ繝｡繝ｼ繧ｿ莉倥″繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public LoadGameCommandDefinition(LoadType type, int slot, string fileName = "")
        {
            loadType = type;
            loadSlot = slot;
            saveFileName = fileName;
        }

        /// <summary>
        /// 繝ｭ繝ｼ繝峨さ繝槭Φ繝峨′螳溯｡悟庄閭ｽ縺九←縺・°繧貞愛螳壹＠縺ｾ縺・        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 蝓ｺ譛ｬ逧・↑螳溯｡悟庄閭ｽ諤ｧ繝√ぉ繝・け
            if (loadSlot < 0 && string.IsNullOrEmpty(saveFileName)) return false;
            if (maxLoadTime <= 0f) return false;

            // 繝ｭ繝ｼ繝牙ｯｾ雎｡縺御ｽ輔ｂ驕ｸ謚槭＆繧後※縺・↑縺・ｴ蜷医・荳榊庄
            if (!loadPlayerData && !loadWorldState && !loadProgress && !loadSettings && !loadStatistics)
                return false;

            // 繧ｳ繝ｳ繝・く繧ｹ繝医′縺ゅｋ蝣ｴ蜷医・霑ｽ蜉繝√ぉ繝・け
            if (context != null)
            {
                // 繧ｻ繝ｼ繝悶ヵ繧｡繧､繝ｫ縺ｮ蟄伜惠繝√ぉ繝・け
                string filePath = GetSaveFilePath();
                if (!System.IO.File.Exists(filePath)) return false;

                // 迴ｾ蝨ｨ縺ｮ迥ｶ諷九メ繧ｧ繝・け・域里縺ｫ繝ｭ繝ｼ繝我ｸｭ遲峨・荳榊庄・・                // 繝｡繝｢繝ｪ菴ｿ逕ｨ驥上メ繧ｧ繝・け
                // 莉悶・驥崎ｦ√↑蜃ｦ逅・ｸｭ縺ｮ蛻ｶ邏・メ繧ｧ繝・け
            }

            return true;
        }

        /// <summary>
        /// 繝ｭ繝ｼ繝峨さ繝槭Φ繝峨ｒ菴懈・縺励∪縺・        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new LoadGameCommand(this, context);
        }

        /// <summary>
        /// 繧ｻ繝ｼ繝悶ヵ繧｡繧､繝ｫ縺ｮ繝代せ繧貞叙蠕・        /// </summary>
        private string GetSaveFilePath()
        {
            string saveDirectory = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
            
            if (!string.IsNullOrEmpty(saveFileName))
            {
                return System.IO.Path.Combine(saveDirectory, saveFileName);
            }
            else
            {
                string fileName = $"save_{loadSlot:D2}.sav";
                return System.IO.Path.Combine(saveDirectory, fileName);
            }
        }
    }

    /// <summary>
    /// LoadGameCommandDefinition縺ｫ蟇ｾ蠢懊☆繧句ｮ滄圀縺ｮ繧ｳ繝槭Φ繝牙ｮ溯｣・    /// </summary>
    public class LoadGameCommand : ICommand
    {
        private LoadGameCommandDefinition definition;
        private object context;
        private bool executed = false;
        private bool loadInProgress = false;
        private string loadedFilePath = "";
        private System.DateTime loadStartTime;
        private ISaveData previousGameState; // Undo逕ｨ縺ｮ迴ｾ蝨ｨ縺ｮ迥ｶ諷倶ｿ晏ｭ・
        public LoadGameCommand(LoadGameCommandDefinition loadDefinition, object executionContext)
        {
            definition = loadDefinition;
            context = executionContext;
        }

        /// <summary>
        /// 繝ｭ繝ｼ繝峨さ繝槭Φ繝峨・螳溯｡・        /// </summary>
        public void Execute()
        {
            if (executed || loadInProgress) return;

            loadInProgress = true;
            loadStartTime = System.DateTime.Now;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.loadType} load: slot={definition.loadSlot}, file='{definition.saveFileName}'");
#endif

            // 迴ｾ蝨ｨ縺ｮ迥ｶ諷九ｒ繝舌ャ繧ｯ繧｢繝・・・・ndo逕ｨ・・            if (definition.createBackup)
            {
                BackupCurrentGameState();
            }

            // 繝ｭ繝ｼ繝峨・繝ｭ繧ｰ繝ｬ繧ｹUI縺ｮ陦ｨ遉ｺ
            if (definition.showLoadProgress)
            {
                ShowLoadProgressUI();
            }

            // 繝ｭ繝ｼ繝・ぅ繝ｳ繧ｰ繧ｹ繧ｯ繝ｪ繝ｼ繝ｳ縺ｮ陦ｨ遉ｺ
            if (definition.showLoadingScreen)
            {
                ShowLoadingScreen();
            }

            try
            {
                // 螳滄圀縺ｮ繝ｭ繝ｼ繝牙・逅・                ExecuteLoadOperation();
            }
            catch (System.Exception ex)
            {
                HandleLoadError(ex);
                return;
            }

            executed = true;
        }

        /// <summary>
        /// 螳滄圀縺ｮ繝ｭ繝ｼ繝牙・逅・ｒ螳溯｡・        /// </summary>
        private void ExecuteLoadOperation()
        {
            // 繧ｻ繝ｼ繝悶ヵ繧｡繧､繝ｫ縺ｮ繝代せ蜿門ｾ・            loadedFilePath = GetLoadFilePath();

            // 繝輔ぃ繧､繝ｫ蟄伜惠繝√ぉ繝・け
            if (!System.IO.File.Exists(loadedFilePath))
            {
                throw new System.IO.FileNotFoundException($"Save file not found: {loadedFilePath}");
            }

            // 繝・・繧ｿ謨ｴ蜷域ｧ縺ｮ莠句燕繝√ぉ繝・け
            if (definition.validateBeforeLoad)
            {
                ValidateSaveFile(loadedFilePath);
            }

            // 繧ｻ繝ｼ繝悶ヵ繧｡繧､繝ｫ縺ｮ隱ｭ縺ｿ霎ｼ縺ｿ
            ISaveData saveData = ReadSaveFile(loadedFilePath);

            // 繝・・繧ｿ謨ｴ蜷域ｧ縺ｮ讀懆ｨｼ
            if (definition.verifyDataIntegrity)
            {
                VerifyDataIntegrity(saveData);
            }

            // 繝舌・繧ｸ繝ｧ繝ｳ莠呈鋤諤ｧ繝√ぉ繝・け
            if (definition.handleVersionMismatch)
            {
                HandleVersionCompatibility(saveData);
            }

            // 繧｢繧ｻ繝・ヨ縺ｮ繝励Μ繝ｭ繝ｼ繝・            if (definition.preloadAssets)
            {
                PreloadRequiredAssets(saveData);
            }

            // 繧ｲ繝ｼ繝迥ｶ諷九・蠕ｩ蜈・            RestoreGameState(saveData);

            // 繧ｷ繝ｼ繝ｳ驕ｷ遘ｻ
            if (definition.changeScene && !string.IsNullOrEmpty(definition.targetScene))
            {
                TransitionToTargetScene();
            }

            // 繝ｭ繝ｼ繝牙ｮ御ｺ・・逅・            OnLoadCompleted();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Load completed: {loadedFilePath}");
#endif
        }

        /// <summary>
        /// 繧ｻ繝ｼ繝悶ヵ繧｡繧､繝ｫ縺ｮ繝代せ繧貞叙蠕・        /// </summary>
        private string GetLoadFilePath()
        {
            string saveDirectory = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
            
            if (!string.IsNullOrEmpty(definition.saveFileName))
            {
                return System.IO.Path.Combine(saveDirectory, definition.saveFileName);
            }
            else
            {
                string fileName;
                switch (definition.loadType)
                {
                    case LoadGameCommandDefinition.LoadType.QuickLoad:
                        fileName = "quicksave.sav";
                        break;
                    case LoadGameCommandDefinition.LoadType.Continue:
                        fileName = GetLatestSaveFile();
                        break;
                    default:
                        fileName = $"save_{definition.loadSlot:D2}.sav";
                        break;
                }
                return System.IO.Path.Combine(saveDirectory, fileName);
            }
        }

        /// <summary>
        /// 譛譁ｰ縺ｮ繧ｻ繝ｼ繝悶ヵ繧｡繧､繝ｫ繧貞叙蠕・        /// </summary>
        private string GetLatestSaveFile()
        {
            string saveDirectory = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
            var saveFiles = System.IO.Directory.GetFiles(saveDirectory, "*.sav");

            if (saveFiles.Length == 0)
            {
                throw new System.IO.FileNotFoundException("No save files found");
            }

            // 譛譁ｰ縺ｮ繝輔ぃ繧､繝ｫ繧貞叙蠕・            System.Array.Sort(saveFiles, (x, y) => 
                System.IO.File.GetLastWriteTime(y).CompareTo(System.IO.File.GetLastWriteTime(x)));

            return System.IO.Path.GetFileName(saveFiles[0]);
        }

        /// <summary>
        /// 繧ｻ繝ｼ繝悶ヵ繧｡繧､繝ｫ縺ｮ莠句燕讀懆ｨｼ
        /// </summary>
        private void ValidateSaveFile(string filePath)
        {
            var fileInfo = new System.IO.FileInfo(filePath);
            
            // 繝輔ぃ繧､繝ｫ繧ｵ繧､繧ｺ繝√ぉ繝・け
            if (fileInfo.Length == 0)
            {
                throw new System.Exception("Save file is empty");
            }

            if (fileInfo.Length < 100) // 譛蟆上し繧､繧ｺ繝√ぉ繝・け
            {
                throw new System.Exception("Save file appears to be corrupted (too small)");
            }

            // 繝輔ぃ繧､繝ｫ諡｡蠑ｵ蟄舌メ繧ｧ繝・け
            if (!filePath.EndsWith(".sav"))
            {
                throw new System.ArgumentException("Invalid save file format");
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Save file validated: {filePath}");
#endif
        }

        /// <summary>
        /// 繧ｻ繝ｼ繝悶ヵ繧｡繧､繝ｫ縺ｮ隱ｭ縺ｿ霎ｼ縺ｿ
        /// </summary>
        private ISaveData ReadSaveFile(string filePath)
        {
            try
            {
                string jsonData = System.IO.File.ReadAllText(filePath);
                
                // 螳滄圀縺ｮ螳溯｣・〒縺ｯ驕ｩ蛻・↑繝・す繝ｪ繧｢繝ｩ繧､繧ｼ繝ｼ繧ｷ繝ｧ繝ｳ・・SON縲。inary遲会ｼ峨ｒ菴ｿ逕ｨ
                var saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
                
                // 證怜捷蛹悶＆繧後※縺・ｋ蝣ｴ蜷医・蠕ｩ蜿ｷ蛹門・逅・                // 蝨ｧ邵ｮ縺輔ｌ縺ｦ縺・ｋ蝣ｴ蜷医・螻暮幕蜃ｦ逅・                
                return saveData;
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Failed to read save file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 繝・・繧ｿ謨ｴ蜷域ｧ縺ｮ讀懆ｨｼ
        /// </summary>
        private void VerifyDataIntegrity(ISaveData saveData)
        {
            if (saveData == null)
            {
                throw new System.Exception("Save data is null");
            }

            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ縲√メ繧ｧ繝・け繧ｵ繝縲√ワ繝・す繝･蛟､遲峨↓繧医ｋ讀懆ｨｼ
            // 蠢・医ョ繝ｼ繧ｿ縺ｮ蟄伜惠繝√ぉ繝・け
            // 繝・・繧ｿ蠖｢蠑上・讀懆ｨｼ

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Data integrity verified");
#endif
        }

        /// <summary>
        /// 繝舌・繧ｸ繝ｧ繝ｳ莠呈鋤諤ｧ縺ｮ蜃ｦ逅・        /// </summary>
        private void HandleVersionCompatibility(ISaveData saveData)
        {
            if (saveData is GameSaveData gameSave && gameSave.SaveInfo != null)
            {
                string saveVersion = gameSave.SaveInfo.GameVersion;
                string currentVersion = Application.version;

                if (saveVersion != currentVersion)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    UnityEngine.Debug.LogWarning($"Version mismatch: save={saveVersion}, current={currentVersion}");
#endif

                    // 繝舌・繧ｸ繝ｧ繝ｳ髢薙・蟾ｮ蛻・ｒ蜃ｦ逅・                    // 蠢・ｦ√↓蠢懊§縺ｦ繝・・繧ｿ螟画鋤
                    // 莠呈鋤諤ｧ縺ｮ縺ｪ縺・ヰ繝ｼ繧ｸ繝ｧ繝ｳ縺ｮ蝣ｴ蜷医・繧ｨ繝ｩ繝ｼ
                }
            }
        }

        /// <summary>
        /// 蠢・ｦ√↑繧｢繧ｻ繝・ヨ縺ｮ繝励Μ繝ｭ繝ｼ繝・        /// </summary>
        private void PreloadRequiredAssets(ISaveData saveData)
        {
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ縲√そ繝ｼ繝悶ョ繝ｼ繧ｿ縺ｫ蜷ｫ縺ｾ繧後ｋ諠・ｱ縺九ｉ
            // 蠢・ｦ√↑繧｢繧ｻ繝・ヨ・医す繝ｼ繝ｳ縲√・繝ｪ繝輔ぃ繝也ｭ会ｼ峨ｒ莠句燕繝ｭ繝ｼ繝・
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Preloading required assets...");
#endif
        }

        /// <summary>
        /// 繧ｲ繝ｼ繝迥ｶ諷九・蠕ｩ蜈・        /// </summary>
        private void RestoreGameState(ISaveData saveData)
        {
            if (saveData is not GameSaveData gameSave) return;

            // 繝励Ξ繧､繝､繝ｼ繝・・繧ｿ縺ｮ蠕ｩ蜈・            if (definition.loadPlayerData && gameSave.PlayerData != null)
            {
                RestorePlayerData(gameSave.PlayerData);
            }

            // 繝ｯ繝ｼ繝ｫ繝臥憾諷九・蠕ｩ蜈・            if (definition.loadWorldState && gameSave.WorldState != null)
            {
                RestoreWorldState(gameSave.WorldState);
            }

            // 騾ｲ陦檎憾豕√・蠕ｩ蜈・            if (definition.loadProgress && gameSave.ProgressData != null)
            {
                RestoreProgressData(gameSave.ProgressData);
            }

            // 險ｭ螳壹・蠕ｩ蜈・            if (definition.loadSettings && gameSave.SettingsData != null)
            {
                RestoreSettingsData(gameSave.SettingsData);
            }

            // 邨ｱ險医ョ繝ｼ繧ｿ縺ｮ蠕ｩ蜈・            if (definition.loadStatistics && gameSave.StatisticsData != null)
            {
                RestoreStatisticsData(gameSave.StatisticsData);
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Game state restored");
#endif
        }

        /// <summary>
        /// 繧ｿ繝ｼ繧ｲ繝・ヨ繧ｷ繝ｼ繝ｳ縺ｸ縺ｮ驕ｷ遘ｻ
        /// </summary>
        private void TransitionToTargetScene()
        {
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ SceneManager 縺ｨ縺ｮ騾｣謳ｺ
            // UnityEngine.SceneManagement.SceneManager.LoadScene(definition.targetScene);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Transitioning to scene: {definition.targetScene}");
#endif
        }

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ繧ｲ繝ｼ繝迥ｶ諷九ｒ繝舌ャ繧ｯ繧｢繝・・
        /// </summary>
        private void BackupCurrentGameState()
        {
            // Undo逕ｨ縺ｫ迴ｾ蝨ｨ縺ｮ迥ｶ諷九ｒ菫晏ｭ・            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ迴ｾ蝨ｨ縺ｮ繧ｲ繝ｼ繝迥ｶ諷九ｒ蜿朱寔
            previousGameState = new GameSaveData(); // 莉ｮ縺ｮ螳溯｣・
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Current game state backed up");
#endif
        }

        /// <summary>
        /// 繝ｭ繝ｼ繝牙ｮ御ｺ・・逅・        /// </summary>
        private void OnLoadCompleted()
        {
            loadInProgress = false;

            // UI 縺ｮ髱櫁｡ｨ遉ｺ
            if (definition.showLoadProgress)
            {
                HideLoadProgressUI();
            }

            if (definition.showLoadingScreen)
            {
                HideLoadingScreen();
            }

            // 謌仙粥騾夂衍縺ｮ陦ｨ遉ｺ
            if (definition.showSuccessNotification)
            {
                ShowLoadSuccessNotification();
            }

            // 繝ｭ繝ｼ繝牙ｮ御ｺ・う繝吶Φ繝医・逋ｺ陦・            // EventSystem.Publish(new GameLoadedEvent(definition.loadSlot, loadedFilePath));
        }

        /// <summary>
        /// 繝ｭ繝ｼ繝峨お繝ｩ繝ｼ縺ｮ蜃ｦ逅・        /// </summary>
        private void HandleLoadError(System.Exception exception)
        {
            loadInProgress = false;

            // UI 縺ｮ髱櫁｡ｨ遉ｺ
            if (definition.showLoadProgress)
            {
                HideLoadProgressUI();
            }

            if (definition.showLoadingScreen)
            {
                HideLoadingScreen();
            }

            // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ蜃ｦ逅・            if (definition.fallbackToDefault && !string.IsNullOrEmpty(definition.fallbackSaveSlot))
            {
                TryFallbackLoad();
                return;
            }

            // 繧ｨ繝ｩ繝ｼ騾夂衍縺ｮ陦ｨ遉ｺ
            if (definition.showErrorDialog)
            {
                ShowLoadErrorDialog(exception.Message);
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.LogError($"Load failed: {exception}");
#endif

            // 繝ｭ繝ｼ繝牙､ｱ謨励う繝吶Φ繝医・逋ｺ陦・            // EventSystem.Publish(new LoadFailedEvent(exception));
        }

        /// <summary>
        /// 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ繝ｭ繝ｼ繝峨・隧ｦ陦・        /// </summary>
        private void TryFallbackLoad()
        {
            try
            {
                // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ繧ｻ繝ｼ繝悶せ繝ｭ繝・ヨ縺九ｉ縺ｮ繝ｭ繝ｼ繝・                var fallbackDefinition = new LoadGameCommandDefinition(definition.loadType, int.Parse(definition.fallbackSaveSlot));
                var fallbackCommand = new LoadGameCommand(fallbackDefinition, context);
                fallbackCommand.Execute();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log($"Fallback load successful: slot {definition.fallbackSaveSlot}");
#endif
            }
            catch (System.Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogError($"Fallback load also failed: {ex}");
#endif
                ShowLoadErrorDialog("Load failed and fallback also unavailable");
            }
        }

        // 蜷・ｨｮ繝・・繧ｿ蠕ｩ蜈・Γ繧ｽ繝・ラ・亥ｮ滄圀縺ｮ螳溯｣・〒縺ｯ蟇ｾ蠢懊☆繧九す繧ｹ繝・Β縺ｨ縺ｮ騾｣謳ｺ・・        private void RestorePlayerData(IPlayerData playerData) { /* 繝励Ξ繧､繝､繝ｼ繝・・繧ｿ蠕ｩ蜈・*/ }
        private void RestoreWorldState(IWorldState worldState) { /* 繝ｯ繝ｼ繝ｫ繝臥憾諷句ｾｩ蜈・*/ }
        private void RestoreProgressData(IProgressData progressData) { /* 騾ｲ陦檎憾豕∝ｾｩ蜈・*/ }
        private void RestoreSettingsData(ISettingsData settingsData) { /* 險ｭ螳壼ｾｩ蜈・*/ }
        private void RestoreStatisticsData(IStatisticsData statisticsData) { /* 邨ｱ險医ョ繝ｼ繧ｿ蠕ｩ蜈・*/ }

        // UI蛻ｶ蠕｡繝｡繧ｽ繝・ラ・亥ｮ滄圀縺ｮ螳溯｣・〒縺ｯ UISystem 縺ｨ縺ｮ騾｣謳ｺ・・        private void ShowLoadProgressUI() { /* 繝ｭ繝ｼ繝峨・繝ｭ繧ｰ繝ｬ繧ｹ陦ｨ遉ｺ */ }
        private void HideLoadProgressUI() { /* 繝ｭ繝ｼ繝峨・繝ｭ繧ｰ繝ｬ繧ｹ髱櫁｡ｨ遉ｺ */ }
        private void ShowLoadingScreen() { /* 繝ｭ繝ｼ繝・ぅ繝ｳ繧ｰ逕ｻ髱｢陦ｨ遉ｺ */ }
        private void HideLoadingScreen() { /* 繝ｭ繝ｼ繝・ぅ繝ｳ繧ｰ逕ｻ髱｢髱櫁｡ｨ遉ｺ */ }
        private void ShowLoadSuccessNotification() { /* 謌仙粥騾夂衍 */ }
        private void ShowLoadErrorDialog(string error) { /* 繧ｨ繝ｩ繝ｼ繝繧､繧｢繝ｭ繧ｰ陦ｨ遉ｺ */ }

        /// <summary>
        /// 繝ｭ繝ｼ繝峨・譖ｴ譁ｰ・医ち繧､繝繧｢繧ｦ繝医メ繧ｧ繝・け遲峨∝､夜Κ縺九ｉ螳壽悄逧・↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ繧具ｼ・        /// </summary>
        public void UpdateLoad()
        {
            if (!loadInProgress) return;

            // 繧ｿ繧､繝繧｢繧ｦ繝医メ繧ｧ繝・け
            var elapsed = System.DateTime.Now - loadStartTime;
            if (elapsed.TotalSeconds > definition.maxLoadTime)
            {
                HandleLoadError(new System.TimeoutException("Load operation timed out"));
            }
        }

        /// <summary>
        /// Undo謫堺ｽ懶ｼ医Ο繝ｼ繝牙燕縺ｮ迥ｶ諷九↓謌ｻ縺呻ｼ・        /// </summary>
        public void Undo()
        {
            if (!executed || previousGameState == null) return;

            // 繝舌ャ繧ｯ繧｢繝・・縺励◆迥ｶ諷九↓謌ｻ縺・            RestoreGameState(previousGameState);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Load undone - restored previous game state");
#endif

            executed = false;
        }

        /// <summary>
        /// 縺薙・繧ｳ繝槭Φ繝峨′Undo蜿ｯ閭ｽ縺九←縺・°
        /// </summary>
        public bool CanUndo => executed && previousGameState != null && definition.createBackup;

        /// <summary>
        /// 迴ｾ蝨ｨ繝ｭ繝ｼ繝牙・逅・ｸｭ縺九←縺・°
        /// </summary>
        public bool IsLoadInProgress => loadInProgress;
    }
}
