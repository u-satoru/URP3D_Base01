using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// ゲームセーブコマンドの定義。
    /// ゲーム状態の保存アクションをカプセル化します。
    /// 
    /// 主な機能：
    /// - 自動/手動セーブの実行
    /// - セーブファイルの管理（スロット、名前付け等）
    /// - セーブ対象データの選択
    /// - セーブ完了通知とエラーハンドリング
    /// </summary>
    [System.Serializable]
    public class SaveGameCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// セーブの種類を定義する列挙型
        /// </summary>
        public enum SaveType
        {
            Manual,         // 手動セーブ
            Auto,           // 自動セーブ
            QuickSave,      // クイックセーブ
            Checkpoint,     // チェックポイントセーブ
            NewGame         // 新規ゲーム開始時セーブ
        }

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
        public float maxSaveTime = 5f; // タイムアウト時間

        [Header("Auto Save Settings")]
        [Tooltip("自動セーブ時の間隔（秒）")]
        public float autoSaveInterval = 300f; // 5分
        [Tooltip("自動セーブファイルの最大保持数")]
        public int maxAutoSaveFiles = 5;

        /// <summary>
        /// デフォルトコンストラクタ
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
        /// セーブコマンドが実行可能かどうかを判定します
        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本的な実行可能性チェック
            if (saveSlot < 0) return false;
            if (maxSaveTime <= 0f) return false;

            // セーブ対象が何も選択されていない場合は不可
            if (!savePlayerData && !saveWorldState && !saveProgress && !saveSettings && !saveStatistics)
                return false;

            // コンテキストがある場合の追加チェック
            if (context != null)
            {
                // セーブ可能な状態かチェック（ローディング中、セーブ中等は不可）
                // ディスク容量チェック
                // セーブファイルの書き込み権限チェック
                // ゲームの重要な処理中（戦闘中等）の制約チェック
            }

            return true;
        }

        /// <summary>
        /// セーブコマンドを作成します
        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new SaveGameCommand(this, context);
        }
    }

    /// <summary>
    /// SaveGameCommandDefinitionに対応する実際のコマンド実装
    /// </summary>
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
        /// セーブコマンドの実行
        /// </summary>
        public void Execute()
        {
            if (executed || saveInProgress) return;

            saveInProgress = true;
            saveStartTime = System.DateTime.Now;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.saveType} save: slot={definition.saveSlot}, name='{definition.saveName}'");
#endif

            // ゲームの一時停止（必要な場合）
            if (definition.pauseGameDuringSave)
            {
                PauseGame();
            }

            // セーブプログレスUIの表示
            if (definition.showSaveProgress)
            {
                ShowSaveProgressUI();
            }

            try
            {
                // 実際のセーブ処理
                ExecuteSaveOperation();
            }
            catch (System.Exception ex)
            {
                HandleSaveError(ex);
                return;
            }

            executed = true;
        }

        /// <summary>
        /// 実際のセーブ処理を実行
        /// </summary>
        private void ExecuteSaveOperation()
        {
            // セーブデータの収集
            var saveData = CollectSaveData();

            // セーブファイル名の生成
            string fileName = GenerateSaveFileName();

            // データの圧縮（設定されている場合）
            if (definition.compressData)
            {
                saveData = CompressSaveData(saveData);
            }

            // データの暗号化（設定されている場合）
            if (definition.encryptData)
            {
                saveData = EncryptSaveData(saveData);
            }

            // ファイルへの書き込み
            savedFilePath = WriteSaveFile(fileName, saveData);

            // 整合性検証（設定されている場合）
            if (definition.validateIntegrity)
            {
                ValidateSavedFile(savedFilePath);
            }

            // 自動セーブファイルの管理
            if (definition.saveType == SaveGameCommandDefinition.SaveType.Auto)
            {
                ManageAutoSaveFiles();
            }

            // セーブ完了処理
            OnSaveCompleted();

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

            // プレイヤーデータの保存
            if (definition.savePlayerData)
            {
                saveData.PlayerData = GetPlayerData();
            }

            // ワールド状態の保存
            if (definition.saveWorldState)
            {
                saveData.WorldState = GetWorldState();
            }

            // 進行状況の保存
            if (definition.saveProgress)
            {
                saveData.ProgressData = GetProgressData();
            }

            // 設定の保存
            if (definition.saveSettings)
            {
                saveData.SettingsData = GetSettingsData();
            }

            // 統計データの保存
            if (definition.saveStatistics)
            {
                saveData.StatisticsData = GetStatisticsData();
            }

            // メタデータの追加
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
        /// セーブファイル名の生成
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
            // 実際の実装では適切な圧縮アルゴリズム（LZ4、gzip等）を使用
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Compressing save data...");
#endif
            return data; // 仮の実装
        }

        /// <summary>
        /// セーブデータの暗号化
        /// </summary>
        private ISaveData EncryptSaveData(ISaveData data)
        {
            // 実際の実装では適切な暗号化アルゴリズム（AES等）を使用
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Encrypting save data...");
#endif
            return data; // 仮の実装
        }

        /// <summary>
        /// セーブファイルの書き込み
        /// </summary>
        private string WriteSaveFile(string fileName, ISaveData data)
        {
            string saveDirectory = GetSaveDirectory();
            string fullPath = System.IO.Path.Combine(saveDirectory, fileName);

            // ディレクトリの作成
            if (!System.IO.Directory.Exists(saveDirectory))
            {
                System.IO.Directory.CreateDirectory(saveDirectory);
            }

            // 既存ファイルの上書きチェック
            if (System.IO.File.Exists(fullPath) && !definition.overwriteExisting)
            {
                throw new System.InvalidOperationException($"Save file already exists: {fullPath}");
            }

            // 実際の実装では適切なシリアライゼーション（JSON、Binary等）を使用
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

            // ファイルサイズチェック
            var fileInfo = new System.IO.FileInfo(filePath);
            if (fileInfo.Length == 0)
            {
                throw new System.Exception("Saved file is empty");
            }

            // データの読み込みテスト
            try
            {
                string content = System.IO.File.ReadAllText(filePath);
                // 基本的な構文チェック等
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Saved file validation failed: {ex.Message}");
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Save file validated: {filePath}");
#endif
        }

        /// <summary>
        /// 自動セーブファイルの管理
        /// </summary>
        private void ManageAutoSaveFiles()
        {
            string saveDirectory = GetSaveDirectory();
            var autoSaveFiles = System.IO.Directory.GetFiles(saveDirectory, "autosave_*.sav");

            // ファイルを作成日時順にソート
            System.Array.Sort(autoSaveFiles, (x, y) => 
                System.IO.File.GetCreationTime(y).CompareTo(System.IO.File.GetCreationTime(x)));

            // 最大保持数を超える古いファイルを削除
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
        /// セーブ完了処理
        /// </summary>
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

            // セーブ完了イベントの発行
            // EventSystem.Publish(new GameSavedEvent(definition.saveSlot, savedFilePath));
        }

        /// <summary>
        /// セーブエラーの処理
        /// </summary>
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

            // セーブ失敗イベントの発行
            // EventSystem.Publish(new SaveFailedEvent(exception));
        }

        // 各種データ取得メソッド（実際の実装では対応するシステムとの連携）
        private IPlayerData GetPlayerData() => new PlayerData();
        private IWorldState GetWorldState() => new WorldState();
        private IProgressData GetProgressData() => new ProgressData();
        private ISettingsData GetSettingsData() => new SettingsData();
        private IStatisticsData GetStatisticsData() => new StatisticsData();
        private float GetTotalPlayTime() => Time.realtimeSinceStartup; // 仮の実装

        // UI制御メソッド（実際の実装では UISystem との連携）
        private void ShowSaveProgressUI() { /* UI表示 */ }
        private void HideSaveProgressUI() { /* UI非表示 */ }
        private void ShowSaveSuccessNotification() { /* 成功通知 */ }
        private void ShowSaveErrorNotification(string error) { /* エラー通知 */ }

        // ゲーム制御メソッド（実際の実装では GameManager との連携）
        private void PauseGame() { /* ゲーム一時停止 */ }
        private void ResumeGame() { /* ゲーム再開 */ }

        /// <summary>
        /// セーブディレクトリのパスを取得
        /// </summary>
        private string GetSaveDirectory()
        {
            return System.IO.Path.Combine(Application.persistentDataPath, "Saves");
        }

        /// <summary>
        /// セーブの更新（タイムアウトチェック等、外部から定期的に呼び出される）
        /// </summary>
        public void UpdateSave()
        {
            if (!saveInProgress) return;

            // タイムアウトチェック
            var elapsed = System.DateTime.Now - saveStartTime;
            if (elapsed.TotalSeconds > definition.maxSaveTime)
            {
                HandleSaveError(new System.TimeoutException("Save operation timed out"));
            }
        }

        /// <summary>
        /// Undo操作（セーブの取り消し - 通常は不可能）
        /// </summary>
        public void Undo()
        {
            // セーブ操作の取り消しは通常不可能
            // ただし、セーブ中の場合はキャンセル可能
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
        /// このコマンドがUndo可能かどうか
        /// </summary>
        public bool CanUndo => saveInProgress; // セーブ中のみキャンセル可能

        /// <summary>
        /// 現在セーブ処理中かどうか
        /// </summary>
        public bool IsSaveInProgress => saveInProgress;
    }

    // セーブデータ関連のインターフェースとクラス（実際の実装では適切に定義）
    public interface ISaveData { }
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

    // 仮の実装クラス
    public class PlayerData : IPlayerData { }
    public class WorldState : IWorldState { }
    public class ProgressData : IProgressData { }
    public class SettingsData : ISettingsData { }
    public class StatisticsData : IStatisticsData { }
}