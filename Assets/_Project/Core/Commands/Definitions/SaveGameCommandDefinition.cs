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
    /// ゲームセーブコマンド�E定義、E    /// ゲーム状態�E保存アクションをカプセル化します、E    /// 
    /// 主な機�E�E�E    /// - 自勁E手動セーブ�E実衁E    /// - セーブファイルの管琁E��スロチE��、名前付け等！E    /// - セーブ対象チE�Eタの選抁E    /// - セーブ完亁E��知とエラーハンドリング
    /// </summary>
    [System.Serializable]
    public class SaveGameCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// セーブ�E種類を定義する列挙垁E        /// </summary>
        public enum SaveType
        {
            Manual,         // 手動セーチE            Auto,           // 自動セーチE            QuickSave,      // クイチE��セーチE            Checkpoint,     // チェチE��ポイントセーチE            NewGame         // 新規ゲーム開始時セーチE        }

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
        public float maxSaveTime = 5f; // タイムアウト時閁E
        [Header("Auto Save Settings")]
        [Tooltip("自動セーブ時の間隔�E�秒！E)]
        public float autoSaveInterval = 300f; // 5刁E        [Tooltip("自動セーブファイルの最大保持数")]
        public int maxAutoSaveFiles = 5;

        /// <summary>
        /// チE��ォルトコンストラクタ
        /// </summary>
        public SaveGameCommandDefinition()
        {
        }

        /// <summary>
        /// パラメータ付きコンストラクタ
        /// </summary>
        public SaveGameCommandDefinition(SaveType type, int slot, string name = "")
        {
            saveType = type;
            saveSlot = slot;
            saveName = name;
        }

        /// <summary>
        /// セーブコマンドが実行可能かどぁE��を判定しまぁE        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本皁E��実行可能性チェチE��
            if (saveSlot < 0) return false;
            if (maxSaveTime <= 0f) return false;

            // セーブ対象が何も選択されてぁE��ぁE��合�E不可
            if (!savePlayerData && !saveWorldState && !saveProgress && !saveSettings && !saveStatistics)
                return false;

            // コンチE��ストがある場合�E追加チェチE��
            if (context != null)
            {
                // セーブ可能な状態かチェチE���E�ローチE��ング中、セーブ中等�E不可�E�E                // チE��スク容量チェチE��
                // セーブファイルの書き込み権限チェチE��
                // ゲームの重要な処琁E���E�戦闘中等）�E制紁E��ェチE��
            }

            return true;
        }

        /// <summary>
        /// セーブコマンドを作�EしまぁE        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new SaveGameCommand(this, context);
        }
    }

    /// <summary>
    /// SaveGameCommandDefinitionに対応する実際のコマンド実裁E    /// </summary>
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
        /// セーブコマンド�E実衁E        /// </summary>
        public void Execute()
        {
            if (executed || saveInProgress) return;

            saveInProgress = true;
            saveStartTime = System.DateTime.Now;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.saveType} save: slot={definition.saveSlot}, name='{definition.saveName}'");
#endif

            // ゲームの一時停止�E�忁E��な場合！E            if (definition.pauseGameDuringSave)
            {
                PauseGame();
            }

            // セーブ�EログレスUIの表示
            if (definition.showSaveProgress)
            {
                ShowSaveProgressUI();
            }

            try
            {
                // 実際のセーブ�E琁E                ExecuteSaveOperation();
            }
            catch (System.Exception ex)
            {
                HandleSaveError(ex);
                return;
            }

            executed = true;
        }

        /// <summary>
        /// 実際のセーブ�E琁E��実衁E        /// </summary>
        private void ExecuteSaveOperation()
        {
            // セーブデータの収集
            var saveData = CollectSaveData();

            // セーブファイル名�E生�E
            string fileName = GenerateSaveFileName();

            // チE�Eタの圧縮�E�設定されてぁE��場合！E            if (definition.compressData)
            {
                saveData = CompressSaveData(saveData);
            }

            // チE�Eタの暗号化（設定されてぁE��場合！E            if (definition.encryptData)
            {
                saveData = EncryptSaveData(saveData);
            }

            // ファイルへの書き込み
            savedFilePath = WriteSaveFile(fileName, saveData);

            // 整合性検証�E�設定されてぁE��場合！E            if (definition.validateIntegrity)
            {
                ValidateSavedFile(savedFilePath);
            }

            // 自動セーブファイルの管琁E            if (definition.saveType == SaveGameCommandDefinition.SaveType.Auto)
            {
                ManageAutoSaveFiles();
            }

            // セーブ完亁E�E琁E            OnSaveCompleted();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Save completed: {savedFilePath}");
#endif
        }

        /// <summary>
        /// セーブデータの収集
        /// </summary>
        private ISaveData CollectSaveData()
        {
            var saveData = new GameSaveData();

            // プレイヤーチE�Eタの保孁E            if (definition.savePlayerData)
            {
                saveData.PlayerData = GetPlayerData();
            }

            // ワールド状態�E保孁E            if (definition.saveWorldState)
            {
                saveData.WorldState = GetWorldState();
            }

            // 進行状況�E保孁E            if (definition.saveProgress)
            {
                saveData.ProgressData = GetProgressData();
            }

            // 設定�E保孁E            if (definition.saveSettings)
            {
                saveData.SettingsData = GetSettingsData();
            }

            // 統計データの保孁E            if (definition.saveStatistics)
            {
                saveData.StatisticsData = GetStatisticsData();
            }

            // メタチE�Eタの追加
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
        /// セーブファイル名�E生�E
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
        /// セーブデータの圧縮
        /// </summary>
        private ISaveData CompressSaveData(ISaveData data)
        {
            // 実際の実裁E��は適刁E��圧縮アルゴリズム�E�EZ4、gzip等）を使用
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Compressing save data...");
#endif
            return data; // 仮の実裁E        }

        /// <summary>
        /// セーブデータの暗号匁E        /// </summary>
        private ISaveData EncryptSaveData(ISaveData data)
        {
            // 実際の実裁E��は適刁E��暗号化アルゴリズム�E�EES等）を使用
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Encrypting save data...");
#endif
            return data; // 仮の実裁E        }

        /// <summary>
        /// セーブファイルの書き込み
        /// </summary>
        private string WriteSaveFile(string fileName, ISaveData data)
        {
            string saveDirectory = GetSaveDirectory();
            string fullPath = System.IO.Path.Combine(saveDirectory, fileName);

            // チE��レクトリの作�E
            if (!System.IO.Directory.Exists(saveDirectory))
            {
                System.IO.Directory.CreateDirectory(saveDirectory);
            }

            // 既存ファイルの上書きチェチE��
            if (System.IO.File.Exists(fullPath) && !definition.overwriteExisting)
            {
                throw new System.InvalidOperationException($"Save file already exists: {fullPath}");
            }

            // 実際の実裁E��は適刁E��シリアライゼーション�E�ESON、Binary等）を使用
            string jsonData = JsonUtility.ToJson(data, true);
            System.IO.File.WriteAllText(fullPath, jsonData);

            return fullPath;
        }

        /// <summary>
        /// 保存されたファイルの整合性検証
        /// </summary>
        private void ValidateSavedFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                throw new System.IO.FileNotFoundException($"Saved file not found: {filePath}");
            }

            // ファイルサイズチェチE��
            var fileInfo = new System.IO.FileInfo(filePath);
            if (fileInfo.Length == 0)
            {
                throw new System.Exception("Saved file is empty");
            }

            // チE�Eタの読み込みチE��チE            try
            {
                string content = System.IO.File.ReadAllText(filePath);
                // 基本皁E��構文チェチE��筁E            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Saved file validation failed: {ex.Message}");
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Save file validated: {filePath}");
#endif
        }

        /// <summary>
        /// 自動セーブファイルの管琁E        /// </summary>
        private void ManageAutoSaveFiles()
        {
            string saveDirectory = GetSaveDirectory();
            var autoSaveFiles = System.IO.Directory.GetFiles(saveDirectory, "autosave_*.sav");

            // ファイルを作�E日時頁E��ソーチE            System.Array.Sort(autoSaveFiles, (x, y) => 
                System.IO.File.GetCreationTime(y).CompareTo(System.IO.File.GetCreationTime(x)));

            // 最大保持数を趁E��る古ぁE��ァイルを削除
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
        /// セーブ完亁E�E琁E        /// </summary>
        private void OnSaveCompleted()
        {
            saveInProgress = false;

            // ゲームの再開
            if (definition.pauseGameDuringSave)
            {
                ResumeGame();
            }

            // プログレスUIの非表示
            if (definition.showSaveProgress)
            {
                HideSaveProgressUI();
            }

            // 成功通知の表示
            if (definition.showSuccessNotification)
            {
                ShowSaveSuccessNotification();
            }

            // セーブ完亁E��ベント�E発衁E            // EventSystem.Publish(new GameSavedEvent(definition.saveSlot, savedFilePath));
        }

        /// <summary>
        /// セーブエラーの処琁E        /// </summary>
        private void HandleSaveError(System.Exception exception)
        {
            saveInProgress = false;

            // ゲームの再開
            if (definition.pauseGameDuringSave)
            {
                ResumeGame();
            }

            // プログレスUIの非表示
            if (definition.showSaveProgress)
            {
                HideSaveProgressUI();
            }

            // エラー通知の表示
            ShowSaveErrorNotification(exception.Message);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.LogError($"Save failed: {exception}");
#endif

            // セーブ失敗イベント�E発衁E            // EventSystem.Publish(new SaveFailedEvent(exception));
        }

        // 吁E��チE�Eタ取得メソチE���E�実際の実裁E��は対応するシスチE��との連携�E�E        private IPlayerData GetPlayerData() => new PlayerData();
        private IWorldState GetWorldState() => new WorldState();
        private IProgressData GetProgressData() => new ProgressData();
        private ISettingsData GetSettingsData() => new SettingsData();
        private IStatisticsData GetStatisticsData() => new StatisticsData();
        private float GetTotalPlayTime() => Time.realtimeSinceStartup; // 仮の実裁E
        // UI制御メソチE���E�実際の実裁E��は UISystem との連携�E�E        private void ShowSaveProgressUI() { /* UI表示 */ }
        private void HideSaveProgressUI() { /* UI非表示 */ }
        private void ShowSaveSuccessNotification() { /* 成功通知 */ }
        private void ShowSaveErrorNotification(string error) { /* エラー通知 */ }

        // ゲーム制御メソチE���E�実際の実裁E��は GameManager との連携�E�E        private void PauseGame() { /* ゲーム一時停止 */ }
        private void ResumeGame() { /* ゲーム再開 */ }

        /// <summary>
        /// セーブディレクトリのパスを取征E        /// </summary>
        private string GetSaveDirectory()
        {
            return System.IO.Path.Combine(Application.persistentDataPath, "Saves");
        }

        /// <summary>
        /// セーブ�E更新�E�タイムアウトチェチE��等、外部から定期皁E��呼び出される！E        /// </summary>
        public void UpdateSave()
        {
            if (!saveInProgress) return;

            // タイムアウトチェチE��
            var elapsed = System.DateTime.Now - saveStartTime;
            if (elapsed.TotalSeconds > definition.maxSaveTime)
            {
                HandleSaveError(new System.TimeoutException("Save operation timed out"));
            }
        }

        /// <summary>
        /// Undo操作（セーブ�E取り消し - 通常は不可能�E�E        /// </summary>
        public void Undo()
        {
            // セーブ操作�E取り消しは通常不可能
            // ただし、セーブ中の場合�Eキャンセル可能
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
        /// こ�EコマンドがUndo可能かどぁE��
        /// </summary>
        public bool CanUndo => saveInProgress; // セーブ中のみキャンセル可能

        /// <summary>
        /// 現在セーブ�E琁E��かどぁE��
        /// </summary>
        public bool IsSaveInProgress => saveInProgress;
    }

    // セーブデータ関連のインターフェースとクラス�E�実際の実裁E��は適刁E��定義�E�E    public interface ISaveData { }
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

    // 仮の実裁E��ラス
    public class PlayerData : IPlayerData { }
    public class WorldState : IWorldState { }
    public class ProgressData : IProgressData { }
    public class SettingsData : ISettingsData { }
    public class StatisticsData : IStatisticsData { }
}