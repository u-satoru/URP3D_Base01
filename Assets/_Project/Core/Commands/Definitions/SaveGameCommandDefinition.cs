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
    /// 繧ｲ繝ｼ繝繧ｻ繝ｼ繝悶さ繝槭Φ繝峨・螳夂ｾｩ縲・    /// 繧ｲ繝ｼ繝迥ｶ諷九・菫晏ｭ倥い繧ｯ繧ｷ繝ｧ繝ｳ繧偵き繝励そ繝ｫ蛹悶＠縺ｾ縺吶・    /// 
    /// 荳ｻ縺ｪ讖溯・・・    /// - 閾ｪ蜍・謇句虚繧ｻ繝ｼ繝悶・螳溯｡・    /// - 繧ｻ繝ｼ繝悶ヵ繧｡繧､繝ｫ縺ｮ邂｡逅・ｼ医せ繝ｭ繝・ヨ縲∝錐蜑堺ｻ倥￠遲会ｼ・    /// - 繧ｻ繝ｼ繝門ｯｾ雎｡繝・・繧ｿ縺ｮ驕ｸ謚・    /// - 繧ｻ繝ｼ繝門ｮ御ｺ・夂衍縺ｨ繧ｨ繝ｩ繝ｼ繝上Φ繝峨Μ繝ｳ繧ｰ
    /// </summary>
    [System.Serializable]
    public class SaveGameCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 繧ｻ繝ｼ繝悶・遞ｮ鬘槭ｒ螳夂ｾｩ縺吶ｋ蛻玲嫌蝙・        /// </summary>
        public enum SaveType
        {
            Manual,         // 謇句虚繧ｻ繝ｼ繝・            Auto,           // 閾ｪ蜍輔そ繝ｼ繝・            QuickSave,      // 繧ｯ繧､繝・け繧ｻ繝ｼ繝・            Checkpoint,     // 繝√ぉ繝・け繝昴う繝ｳ繝医そ繝ｼ繝・            NewGame         // 譁ｰ隕上ご繝ｼ繝髢句ｧ区凾繧ｻ繝ｼ繝・        }

        [Header("Save Parameters")]
        public SaveType saveType = SaveType.Manual;
        public int saveSlot = 0;
        public string saveName = "";
        public bool overwriteExisting = true;

        [Header("Save Scope")]
        public bool savePlayerData = true;
        public bool saveWorldState = true;
        public bool saveProgress = true;
        public bool saveSettings = false;
        public bool saveStatistics = true;

        [Header("Compression & Security")]
        public bool compressData = true;
        public bool encryptData = false;
        public bool validateIntegrity = true;

        [Header("User Experience")]
        public bool showSaveProgress = true;
        public bool showSuccessNotification = true;
        public bool pauseGameDuringSave = false;
        public float maxSaveTime = 5f; // 繧ｿ繧､繝繧｢繧ｦ繝域凾髢・
        [Header("Auto Save Settings")]
        [Tooltip("閾ｪ蜍輔そ繝ｼ繝匁凾縺ｮ髢馴囈・育ｧ抵ｼ・)]
        public float autoSaveInterval = 300f; // 5蛻・        [Tooltip("閾ｪ蜍輔そ繝ｼ繝悶ヵ繧｡繧､繝ｫ縺ｮ譛螟ｧ菫晄戟謨ｰ")]
        public int maxAutoSaveFiles = 5;

        /// <summary>
        /// 繝・ヵ繧ｩ繝ｫ繝医さ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public SaveGameCommandDefinition()
        {
        }

        /// <summary>
        /// 繝代Λ繝｡繝ｼ繧ｿ莉倥″繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public SaveGameCommandDefinition(SaveType type, int slot, string name = "")
        {
            saveType = type;
            saveSlot = slot;
            saveName = name;
        }

        /// <summary>
        /// 繧ｻ繝ｼ繝悶さ繝槭Φ繝峨′螳溯｡悟庄閭ｽ縺九←縺・°繧貞愛螳壹＠縺ｾ縺・        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 蝓ｺ譛ｬ逧・↑螳溯｡悟庄閭ｽ諤ｧ繝√ぉ繝・け
            if (saveSlot < 0) return false;
            if (maxSaveTime <= 0f) return false;

            // 繧ｻ繝ｼ繝門ｯｾ雎｡縺御ｽ輔ｂ驕ｸ謚槭＆繧後※縺・↑縺・ｴ蜷医・荳榊庄
            if (!savePlayerData && !saveWorldState && !saveProgress && !saveSettings && !saveStatistics)
                return false;

            // 繧ｳ繝ｳ繝・く繧ｹ繝医′縺ゅｋ蝣ｴ蜷医・霑ｽ蜉繝√ぉ繝・け
            if (context != null)
            {
                // 繧ｻ繝ｼ繝門庄閭ｽ縺ｪ迥ｶ諷九°繝√ぉ繝・け・医Ο繝ｼ繝・ぅ繝ｳ繧ｰ荳ｭ縲√そ繝ｼ繝紋ｸｭ遲峨・荳榊庄・・                // 繝・ぅ繧ｹ繧ｯ螳ｹ驥上メ繧ｧ繝・け
                // 繧ｻ繝ｼ繝悶ヵ繧｡繧､繝ｫ縺ｮ譖ｸ縺崎ｾｼ縺ｿ讓ｩ髯舌メ繧ｧ繝・け
                // 繧ｲ繝ｼ繝縺ｮ驥崎ｦ√↑蜃ｦ逅・ｸｭ・域姶髣倅ｸｭ遲会ｼ峨・蛻ｶ邏・メ繧ｧ繝・け
            }

            return true;
        }

        /// <summary>
        /// 繧ｻ繝ｼ繝悶さ繝槭Φ繝峨ｒ菴懈・縺励∪縺・        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new SaveGameCommand(this, context);
        }
    }

    /// <summary>
    /// SaveGameCommandDefinition縺ｫ蟇ｾ蠢懊☆繧句ｮ滄圀縺ｮ繧ｳ繝槭Φ繝牙ｮ溯｣・    /// </summary>
    public class SaveGameCommand : ICommand
    {
        private SaveGameCommandDefinition definition;
        private object context;
        private bool executed = false;
        private bool saveInProgress = false;
        private string savedFilePath = "";
        private System.DateTime saveStartTime;

        public SaveGameCommand(SaveGameCommandDefinition saveDefinition, object executionContext)
        {
            definition = saveDefinition;
            context = executionContext;
        }

        /// <summary>
        /// 繧ｻ繝ｼ繝悶さ繝槭Φ繝峨・螳溯｡・        /// </summary>
        public void Execute()
        {
            if (executed || saveInProgress) return;

            saveInProgress = true;
            saveStartTime = System.DateTime.Now;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.saveType} save: slot={definition.saveSlot}, name='{definition.saveName}'");
#endif

            // 繧ｲ繝ｼ繝縺ｮ荳譎ょ●豁｢・亥ｿ・ｦ√↑蝣ｴ蜷茨ｼ・            if (definition.pauseGameDuringSave)
            {
                PauseGame();
            }

            // 繧ｻ繝ｼ繝悶・繝ｭ繧ｰ繝ｬ繧ｹUI縺ｮ陦ｨ遉ｺ
            if (definition.showSaveProgress)
            {
                ShowSaveProgressUI();
            }

            try
            {
                // 螳滄圀縺ｮ繧ｻ繝ｼ繝門・逅・                ExecuteSaveOperation();
            }
            catch (System.Exception ex)
            {
                HandleSaveError(ex);
                return;
            }

            executed = true;
        }

        /// <summary>
        /// 螳滄圀縺ｮ繧ｻ繝ｼ繝門・逅・ｒ螳溯｡・        /// </summary>
        private void ExecuteSaveOperation()
        {
            // 繧ｻ繝ｼ繝悶ョ繝ｼ繧ｿ縺ｮ蜿朱寔
            var saveData = CollectSaveData();

            // 繧ｻ繝ｼ繝悶ヵ繧｡繧､繝ｫ蜷阪・逕滓・
            string fileName = GenerateSaveFileName();

            // 繝・・繧ｿ縺ｮ蝨ｧ邵ｮ・郁ｨｭ螳壹＆繧後※縺・ｋ蝣ｴ蜷茨ｼ・            if (definition.compressData)
            {
                saveData = CompressSaveData(saveData);
            }

            // 繝・・繧ｿ縺ｮ證怜捷蛹厄ｼ郁ｨｭ螳壹＆繧後※縺・ｋ蝣ｴ蜷茨ｼ・            if (definition.encryptData)
            {
                saveData = EncryptSaveData(saveData);
            }

            // 繝輔ぃ繧､繝ｫ縺ｸ縺ｮ譖ｸ縺崎ｾｼ縺ｿ
            savedFilePath = WriteSaveFile(fileName, saveData);

            // 謨ｴ蜷域ｧ讀懆ｨｼ・郁ｨｭ螳壹＆繧後※縺・ｋ蝣ｴ蜷茨ｼ・            if (definition.validateIntegrity)
            {
                ValidateSavedFile(savedFilePath);
            }

            // 閾ｪ蜍輔そ繝ｼ繝悶ヵ繧｡繧､繝ｫ縺ｮ邂｡逅・            if (definition.saveType == SaveGameCommandDefinition.SaveType.Auto)
            {
                ManageAutoSaveFiles();
            }

            // 繧ｻ繝ｼ繝門ｮ御ｺ・・逅・            OnSaveCompleted();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Save completed: {savedFilePath}");
#endif
        }

        /// <summary>
        /// 繧ｻ繝ｼ繝悶ョ繝ｼ繧ｿ縺ｮ蜿朱寔
        /// </summary>
        private ISaveData CollectSaveData()
        {
            var saveData = new GameSaveData();

            // 繝励Ξ繧､繝､繝ｼ繝・・繧ｿ縺ｮ菫晏ｭ・            if (definition.savePlayerData)
            {
                saveData.PlayerData = GetPlayerData();
            }

            // 繝ｯ繝ｼ繝ｫ繝臥憾諷九・菫晏ｭ・            if (definition.saveWorldState)
            {
                saveData.WorldState = GetWorldState();
            }

            // 騾ｲ陦檎憾豕√・菫晏ｭ・            if (definition.saveProgress)
            {
                saveData.ProgressData = GetProgressData();
            }

            // 險ｭ螳壹・菫晏ｭ・            if (definition.saveSettings)
            {
                saveData.SettingsData = GetSettingsData();
            }

            // 邨ｱ險医ョ繝ｼ繧ｿ縺ｮ菫晏ｭ・            if (definition.saveStatistics)
            {
                saveData.StatisticsData = GetStatisticsData();
            }

            // 繝｡繧ｿ繝・・繧ｿ縺ｮ霑ｽ蜉
            saveData.SaveInfo = new SaveMetaData
            {
                SaveType = definition.saveType.ToString(),
                SaveSlot = definition.saveSlot,
                SaveName = definition.saveName,
                SaveTime = System.DateTime.Now,
                GameVersion = Application.version,
                PlayTime = GetTotalPlayTime()
            };

            return saveData;
        }

        /// <summary>
        /// 繧ｻ繝ｼ繝悶ヵ繧｡繧､繝ｫ蜷阪・逕滓・
        /// </summary>
        private string GenerateSaveFileName()
        {
            string baseName;

            switch (definition.saveType)
            {
                case SaveGameCommandDefinition.SaveType.Auto:
                    baseName = $"autosave_{definition.saveSlot:D2}";
                    break;
                case SaveGameCommandDefinition.SaveType.QuickSave:
                    baseName = "quicksave";
                    break;
                case SaveGameCommandDefinition.SaveType.Checkpoint:
                    baseName = $"checkpoint_{System.DateTime.Now:yyyyMMdd_HHmmss}";
                    break;
                default:
                    baseName = string.IsNullOrEmpty(definition.saveName) 
                        ? $"save_{definition.saveSlot:D2}" 
                        : definition.saveName;
                    break;
            }

            return $"{baseName}.sav";
        }

        /// <summary>
        /// 繧ｻ繝ｼ繝悶ョ繝ｼ繧ｿ縺ｮ蝨ｧ邵ｮ
        /// </summary>
        private ISaveData CompressSaveData(ISaveData data)
        {
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ驕ｩ蛻・↑蝨ｧ邵ｮ繧｢繝ｫ繧ｴ繝ｪ繧ｺ繝・・Z4縲“zip遲会ｼ峨ｒ菴ｿ逕ｨ
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Compressing save data...");
#endif
            return data; // 莉ｮ縺ｮ螳溯｣・        }

        /// <summary>
        /// 繧ｻ繝ｼ繝悶ョ繝ｼ繧ｿ縺ｮ證怜捷蛹・        /// </summary>
        private ISaveData EncryptSaveData(ISaveData data)
        {
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ驕ｩ蛻・↑證怜捷蛹悶い繝ｫ繧ｴ繝ｪ繧ｺ繝・・ES遲会ｼ峨ｒ菴ｿ逕ｨ
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Encrypting save data...");
#endif
            return data; // 莉ｮ縺ｮ螳溯｣・        }

        /// <summary>
        /// 繧ｻ繝ｼ繝悶ヵ繧｡繧､繝ｫ縺ｮ譖ｸ縺崎ｾｼ縺ｿ
        /// </summary>
        private string WriteSaveFile(string fileName, ISaveData data)
        {
            string saveDirectory = GetSaveDirectory();
            string fullPath = System.IO.Path.Combine(saveDirectory, fileName);

            // 繝・ぅ繝ｬ繧ｯ繝医Μ縺ｮ菴懈・
            if (!System.IO.Directory.Exists(saveDirectory))
            {
                System.IO.Directory.CreateDirectory(saveDirectory);
            }

            // 譌｢蟄倥ヵ繧｡繧､繝ｫ縺ｮ荳頑嶌縺阪メ繧ｧ繝・け
            if (System.IO.File.Exists(fullPath) && !definition.overwriteExisting)
            {
                throw new System.InvalidOperationException($"Save file already exists: {fullPath}");
            }

            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ驕ｩ蛻・↑繧ｷ繝ｪ繧｢繝ｩ繧､繧ｼ繝ｼ繧ｷ繝ｧ繝ｳ・・SON縲。inary遲会ｼ峨ｒ菴ｿ逕ｨ
            string jsonData = JsonUtility.ToJson(data, true);
            System.IO.File.WriteAllText(fullPath, jsonData);

            return fullPath;
        }

        /// <summary>
        /// 菫晏ｭ倥＆繧後◆繝輔ぃ繧､繝ｫ縺ｮ謨ｴ蜷域ｧ讀懆ｨｼ
        /// </summary>
        private void ValidateSavedFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                throw new System.IO.FileNotFoundException($"Saved file not found: {filePath}");
            }

            // 繝輔ぃ繧､繝ｫ繧ｵ繧､繧ｺ繝√ぉ繝・け
            var fileInfo = new System.IO.FileInfo(filePath);
            if (fileInfo.Length == 0)
            {
                throw new System.Exception("Saved file is empty");
            }

            // 繝・・繧ｿ縺ｮ隱ｭ縺ｿ霎ｼ縺ｿ繝・せ繝・            try
            {
                string content = System.IO.File.ReadAllText(filePath);
                // 蝓ｺ譛ｬ逧・↑讒区枚繝√ぉ繝・け遲・            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Saved file validation failed: {ex.Message}");
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Save file validated: {filePath}");
#endif
        }

        /// <summary>
        /// 閾ｪ蜍輔そ繝ｼ繝悶ヵ繧｡繧､繝ｫ縺ｮ邂｡逅・        /// </summary>
        private void ManageAutoSaveFiles()
        {
            string saveDirectory = GetSaveDirectory();
            var autoSaveFiles = System.IO.Directory.GetFiles(saveDirectory, "autosave_*.sav");

            // 繝輔ぃ繧､繝ｫ繧剃ｽ懈・譌･譎る・↓繧ｽ繝ｼ繝・            System.Array.Sort(autoSaveFiles, (x, y) => 
                System.IO.File.GetCreationTime(y).CompareTo(System.IO.File.GetCreationTime(x)));

            // 譛螟ｧ菫晄戟謨ｰ繧定ｶ・∴繧句商縺・ヵ繧｡繧､繝ｫ繧貞炎髯､
            for (int i = definition.maxAutoSaveFiles; i < autoSaveFiles.Length; i++)
            {
                try
                {
                    System.IO.File.Delete(autoSaveFiles[i]);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    UnityEngine.Debug.Log($"Deleted old auto save: {autoSaveFiles[i]}");
#endif
                }
                catch (System.Exception ex)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    UnityEngine.Debug.LogWarning($"Failed to delete old auto save: {ex.Message}");
#endif
                }
            }
        }

        /// <summary>
        /// 繧ｻ繝ｼ繝門ｮ御ｺ・・逅・        /// </summary>
        private void OnSaveCompleted()
        {
            saveInProgress = false;

            // 繧ｲ繝ｼ繝縺ｮ蜀埼幕
            if (definition.pauseGameDuringSave)
            {
                ResumeGame();
            }

            // 繝励Ο繧ｰ繝ｬ繧ｹUI縺ｮ髱櫁｡ｨ遉ｺ
            if (definition.showSaveProgress)
            {
                HideSaveProgressUI();
            }

            // 謌仙粥騾夂衍縺ｮ陦ｨ遉ｺ
            if (definition.showSuccessNotification)
            {
                ShowSaveSuccessNotification();
            }

            // 繧ｻ繝ｼ繝門ｮ御ｺ・う繝吶Φ繝医・逋ｺ陦・            // EventSystem.Publish(new GameSavedEvent(definition.saveSlot, savedFilePath));
        }

        /// <summary>
        /// 繧ｻ繝ｼ繝悶お繝ｩ繝ｼ縺ｮ蜃ｦ逅・        /// </summary>
        private void HandleSaveError(System.Exception exception)
        {
            saveInProgress = false;

            // 繧ｲ繝ｼ繝縺ｮ蜀埼幕
            if (definition.pauseGameDuringSave)
            {
                ResumeGame();
            }

            // 繝励Ο繧ｰ繝ｬ繧ｹUI縺ｮ髱櫁｡ｨ遉ｺ
            if (definition.showSaveProgress)
            {
                HideSaveProgressUI();
            }

            // 繧ｨ繝ｩ繝ｼ騾夂衍縺ｮ陦ｨ遉ｺ
            ShowSaveErrorNotification(exception.Message);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.LogError($"Save failed: {exception}");
#endif

            // 繧ｻ繝ｼ繝門､ｱ謨励う繝吶Φ繝医・逋ｺ陦・            // EventSystem.Publish(new SaveFailedEvent(exception));
        }

        // 蜷・ｨｮ繝・・繧ｿ蜿門ｾ励Γ繧ｽ繝・ラ・亥ｮ滄圀縺ｮ螳溯｣・〒縺ｯ蟇ｾ蠢懊☆繧九す繧ｹ繝・Β縺ｨ縺ｮ騾｣謳ｺ・・        private IPlayerData GetPlayerData() => new PlayerData();
        private IWorldState GetWorldState() => new WorldState();
        private IProgressData GetProgressData() => new ProgressData();
        private ISettingsData GetSettingsData() => new SettingsData();
        private IStatisticsData GetStatisticsData() => new StatisticsData();
        private float GetTotalPlayTime() => Time.realtimeSinceStartup; // 莉ｮ縺ｮ螳溯｣・
        // UI蛻ｶ蠕｡繝｡繧ｽ繝・ラ・亥ｮ滄圀縺ｮ螳溯｣・〒縺ｯ UISystem 縺ｨ縺ｮ騾｣謳ｺ・・        private void ShowSaveProgressUI() { /* UI陦ｨ遉ｺ */ }
        private void HideSaveProgressUI() { /* UI髱櫁｡ｨ遉ｺ */ }
        private void ShowSaveSuccessNotification() { /* 謌仙粥騾夂衍 */ }
        private void ShowSaveErrorNotification(string error) { /* 繧ｨ繝ｩ繝ｼ騾夂衍 */ }

        // 繧ｲ繝ｼ繝蛻ｶ蠕｡繝｡繧ｽ繝・ラ・亥ｮ滄圀縺ｮ螳溯｣・〒縺ｯ GameManager 縺ｨ縺ｮ騾｣謳ｺ・・        private void PauseGame() { /* 繧ｲ繝ｼ繝荳譎ょ●豁｢ */ }
        private void ResumeGame() { /* 繧ｲ繝ｼ繝蜀埼幕 */ }

        /// <summary>
        /// 繧ｻ繝ｼ繝悶ョ繧｣繝ｬ繧ｯ繝医Μ縺ｮ繝代せ繧貞叙蠕・        /// </summary>
        private string GetSaveDirectory()
        {
            return System.IO.Path.Combine(Application.persistentDataPath, "Saves");
        }

        /// <summary>
        /// 繧ｻ繝ｼ繝悶・譖ｴ譁ｰ・医ち繧､繝繧｢繧ｦ繝医メ繧ｧ繝・け遲峨∝､夜Κ縺九ｉ螳壽悄逧・↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ繧具ｼ・        /// </summary>
        public void UpdateSave()
        {
            if (!saveInProgress) return;

            // 繧ｿ繧､繝繧｢繧ｦ繝医メ繧ｧ繝・け
            var elapsed = System.DateTime.Now - saveStartTime;
            if (elapsed.TotalSeconds > definition.maxSaveTime)
            {
                HandleSaveError(new System.TimeoutException("Save operation timed out"));
            }
        }

        /// <summary>
        /// Undo謫堺ｽ懶ｼ医そ繝ｼ繝悶・蜿悶ｊ豸医＠ - 騾壼ｸｸ縺ｯ荳榊庄閭ｽ・・        /// </summary>
        public void Undo()
        {
            // 繧ｻ繝ｼ繝匁桃菴懊・蜿悶ｊ豸医＠縺ｯ騾壼ｸｸ荳榊庄閭ｽ
            // 縺溘□縺励√そ繝ｼ繝紋ｸｭ縺ｮ蝣ｴ蜷医・繧ｭ繝｣繝ｳ繧ｻ繝ｫ蜿ｯ閭ｽ
            if (saveInProgress)
            {
                saveInProgress = false;
                ResumeGame();
                HideSaveProgressUI();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log("Save operation cancelled");
#endif
            }

            executed = false;
        }

        /// <summary>
        /// 縺薙・繧ｳ繝槭Φ繝峨′Undo蜿ｯ閭ｽ縺九←縺・°
        /// </summary>
        public bool CanUndo => saveInProgress; // 繧ｻ繝ｼ繝紋ｸｭ縺ｮ縺ｿ繧ｭ繝｣繝ｳ繧ｻ繝ｫ蜿ｯ閭ｽ

        /// <summary>
        /// 迴ｾ蝨ｨ繧ｻ繝ｼ繝門・逅・ｸｭ縺九←縺・°
        /// </summary>
        public bool IsSaveInProgress => saveInProgress;
    }

    // 繧ｻ繝ｼ繝悶ョ繝ｼ繧ｿ髢｢騾｣縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ縺ｨ繧ｯ繝ｩ繧ｹ・亥ｮ滄圀縺ｮ螳溯｣・〒縺ｯ驕ｩ蛻・↓螳夂ｾｩ・・    public interface ISaveData { }
    public interface IPlayerData { }
    public interface IWorldState { }
    public interface IProgressData { }
    public interface ISettingsData { }
    public interface IStatisticsData { }

    [System.Serializable]
    public class GameSaveData : ISaveData
    {
        public IPlayerData PlayerData;
        public IWorldState WorldState;
        public IProgressData ProgressData;
        public ISettingsData SettingsData;
        public IStatisticsData StatisticsData;
        public SaveMetaData SaveInfo;
    }

    [System.Serializable]
    public class SaveMetaData
    {
        public string SaveType;
        public int SaveSlot;
        public string SaveName;
        public System.DateTime SaveTime;
        public string GameVersion;
        public float PlayTime;
    }

    // 莉ｮ縺ｮ螳溯｣・け繝ｩ繧ｹ
    public class PlayerData : IPlayerData { }
    public class WorldState : IWorldState { }
    public class ProgressData : IProgressData { }
    public class SettingsData : ISettingsData { }
    public class StatisticsData : IStatisticsData { }
}