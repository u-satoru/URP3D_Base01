using UnityEngine;
using System;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// ゲーム終了コマンドの定義
    /// アプリケーションの終了アクションをカプセル化します
    /// </summary>
    [System.Serializable]
    public class QuitGameCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 終了の種類を定義する列挙型
        /// </summary>
        public enum QuitType
        {
            Immediate,      // 即座に終了
            Safe,           // 安全な終了（セーブ等を実行）
            Confirm,        // 確認ダイアログ付き終了
            ToMainMenu,     // メインメニューに戻る
            Restart         // アプリケーション再起動
        }

        [Header("Quit Parameters")]
        public QuitType quitType = QuitType.Safe;
        public bool showConfirmDialog = true;
        public string confirmMessage = "Are you sure you want to quit?";

        [Header("Auto Save")]
        public bool autoSaveBeforeQuit = true;
        public bool forceSaveEvenIfRecent = false;
        public float autoSaveTimeout = 5f;

        [Header("Cleanup")]
        public bool saveSettingsOnQuit = true;
        public bool saveStatisticsOnQuit = true;
        public bool cleanupTempFiles = true;
        public bool disposeResources = true;

        [Header("Fade Out")]
        public bool fadeOutBeforeQuit = false;
        public float fadeOutDuration = 1f;
        public Color fadeColor = Color.black;

        [Header("Debug")]
        public bool logQuitAttempt = true;
        public bool showQuitReasonInLog = false;

        public ICommand CreateCommand(object context = null)
        {
            return new QuitGameCommand(this, context);
        }
    }

    /// <summary>
    /// QuitGameCommandDefinitionに対応する実際のコマンド実装
    /// </summary>
    public class QuitGameCommand : ICommand
    {
        private QuitGameCommandDefinition definition;
        private object context;
        private bool executed = false;
        private bool quitInProgress = false;
        private DateTime quitStartTime;
        private bool userConfirmed = false;

        public QuitGameCommand(QuitGameCommandDefinition definition, object context = null)
        {
            this.definition = definition;
            this.context = context;
        }

        /// <summary>
        /// 終了コマンドの実行
        /// </summary>
        public void Execute()
        {
            if (executed || quitInProgress) return;

            quitStartTime = DateTime.Now;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (definition.logQuitAttempt)
            {
                string reason = definition.showQuitReasonInLog ? $" (Type: {definition.quitType})" : "";
                UnityEngine.Debug.Log($"Quit game command executed{reason}");
            }
#endif

            quitInProgress = true;

            switch (definition.quitType)
            {
                case QuitGameCommandDefinition.QuitType.Immediate:
                    ExecuteImmediateQuit();
                    break;

                case QuitGameCommandDefinition.QuitType.Safe:
                    ExecuteSafeQuit();
                    break;

                case QuitGameCommandDefinition.QuitType.Confirm:
                    ExecuteConfirmQuit();
                    break;

                case QuitGameCommandDefinition.QuitType.ToMainMenu:
                    ExecuteReturnToMainMenu();
                    break;

                case QuitGameCommandDefinition.QuitType.Restart:
                    ExecuteRestart();
                    break;

                default:
                    ExecuteSafeQuit();
                    break;
            }

            executed = true;
        }

        /// <summary>
        /// 即座に終了
        /// </summary>
        private void ExecuteImmediateQuit()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Executing immediate quit");
#endif

            // 最小限のクリーンアップ
            if (definition.disposeResources)
            {
                PerformMinimalCleanup();
            }

            // アプリケーション終了
            QuitApplication();
        }

        /// <summary>
        /// 安全な終了
        /// </summary>
        private void ExecuteSafeQuit()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Executing safe quit");
#endif

            // 自動セーブ
            if (definition.autoSaveBeforeQuit)
            {
                PerformAutoSave();
            }

            // 設定の保存
            if (definition.saveSettingsOnQuit)
            {
                SaveSettings();
            }

            // 統計データの保存
            if (definition.saveStatisticsOnQuit)
            {
                SaveStatistics();
            }

            // クリーンアップ
            if (definition.cleanupTempFiles || definition.disposeResources)
            {
                PerformFullCleanup();
            }

            // フェードアウト
            if (definition.fadeOutBeforeQuit)
            {
                StartFadeOut();
            }

            // アプリケーション終了
            QuitApplication();
        }

        /// <summary>
        /// 確認ダイアログ付き終了
        /// </summary>
        private void ExecuteConfirmQuit()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Showing quit confirmation dialog");
#endif

            if (definition.showConfirmDialog)
            {
                ShowConfirmDialog();
            }
            else
            {
                ExecuteSafeQuit();
            }
        }

        /// <summary>
        /// メインメニューに戻る
        /// </summary>
        private void ExecuteReturnToMainMenu()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Returning to main menu");
#endif

            // 自動セーブ
            if (definition.autoSaveBeforeQuit)
            {
                PerformAutoSave();
            }

            // シーンのロード
            LoadMainMenuScene();
            quitInProgress = false;
        }

        /// <summary>
        /// アプリケーション再起動
        /// </summary>
        private void ExecuteRestart()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Restarting application");
#endif

            // 設定の保存
            if (definition.saveSettingsOnQuit)
            {
                SaveSettings();
            }

            // 再起動処理
            RestartApplication();
        }

        /// <summary>
        /// 自動セーブの実行
        /// </summary>
        private void PerformAutoSave()
        {
            // TODO: セーブシステムとの統合
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Auto-saving game data...");
#endif
        }

        /// <summary>
        /// 設定の保存
        /// </summary>
        private void SaveSettings()
        {
            // TODO: 設定システムとの統合
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Saving game settings...");
#endif
        }

        /// <summary>
        /// 統計データの保存
        /// </summary>
        private void SaveStatistics()
        {
            // TODO: 統計システムとの統合
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Saving statistics...");
#endif
        }

        /// <summary>
        /// 最小限のクリーンアップ
        /// </summary>
        private void PerformMinimalCleanup()
        {
            // リソースの解放
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        /// <summary>
        /// 完全なクリーンアップ
        /// </summary>
        private void PerformFullCleanup()
        {
            // リソースの解放
            Resources.UnloadUnusedAssets();

            // ガベージコレクション
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // TODO: 追加のクリーンアップ処理
        }

        /// <summary>
        /// フェードアウト開始
        /// </summary>
        private void StartFadeOut()
        {
            // TODO: フェードシステムとの統合
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Starting fade out (duration: {definition.fadeOutDuration}s)");
#endif
        }

        /// <summary>
        /// 確認ダイアログの表示
        /// </summary>
        private void ShowConfirmDialog()
        {
            // TODO: UIシステムとの統合
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Showing confirm dialog: {definition.confirmMessage}");
#endif

            // 仮の実装
            OnConfirmDialogResult(true);
        }

        /// <summary>
        /// 確認ダイアログの結果処理
        /// </summary>
        private void OnConfirmDialogResult(bool confirmed)
        {
            userConfirmed = confirmed;

            if (confirmed)
            {
                ExecuteSafeQuit();
            }
            else
            {
                quitInProgress = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log("Quit cancelled by user");
#endif
            }
        }

        /// <summary>
        /// メインメニューシーンのロード
        /// </summary>
        private void LoadMainMenuScene()
        {
            // TODO: シーン管理システムとの統合
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Loading main menu scene");
#endif
        }

        /// <summary>
        /// アプリケーションの再起動
        /// </summary>
        private void RestartApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            System.Diagnostics.Process.Start(Application.dataPath.Replace("Assets", ""));
#else
            System.Diagnostics.Process.Start(Application.dataPath);
            QuitApplication();
#endif
        }

        /// <summary>
        /// アプリケーション終了処理
        /// </summary>
        private void QuitApplication()
        {
            var elapsedTime = (DateTime.Now - quitStartTime).TotalSeconds;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Quitting application (elapsed time: {elapsedTime:F2}s)");
#endif

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public bool CanExecute()
        {
            return !quitInProgress && !executed;
        }

        /// <summary>
        /// 現在終了処理中かどうか
        /// </summary>
        public bool IsQuitInProgress => quitInProgress;

        /// <summary>
        /// ユーザーが終了を確認したかどうか
        /// </summary>
        public bool IsUserConfirmed => userConfirmed;
    }
}