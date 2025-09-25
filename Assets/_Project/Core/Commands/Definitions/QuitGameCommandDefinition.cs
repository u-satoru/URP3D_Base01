using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// ゲーム終了コマンドの定義。
    /// アプリケーションの終了アクションをカプセル化します。
    /// 
    /// 主な機能：
    /// - 安全なアプリケーション終了
    /// - 終了前の自動セーブ
    /// - 終了確認ダイアログの表示
    /// - リソースのクリーンアップ
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
        public bool cleanupTempFiles = true;
        public bool saveSettingsOnQuit = true;
        public bool saveStatisticsOnQuit = true;
        public bool disposeResources = true;

        [Header("Scene Transition")]
        public bool fadeOutBeforeQuit = false;
        public float fadeOutDuration = 1f;
        public string targetScene = ""; // ToMainMenuの場合に使用

        [Header("Platform Specific")]
        public bool confirmOnMobile = true;
        public bool minimizeInsteadOfQuitOnMobile = false;
        public bool handleBackButtonOnMobile = true;

        [Header("Debug")]
        public bool logQuitAttempt = true;
        public bool showQuitReasonInLog = true;

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public QuitGameCommandDefinition()
        {
        }

        /// <summary>
        /// パラメータ付きコンストラクタ
        /// </summary>
        public QuitGameCommandDefinition(QuitType type, bool confirm = true, bool autoSave = true)
        {
            quitType = type;
            showConfirmDialog = confirm;
            autoSaveBeforeQuit = autoSave;
        }

        /// <summary>
        /// 終了コマンドが実行可能かどうかを判定します
        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本的な実行可能性チェック
            if (autoSaveTimeout <= 0f) return false;
            if (fadeOutDuration < 0f) return false;

            // ToMainMenuの場合はターゲットシーンが必要
            if (quitType == QuitType.ToMainMenu && string.IsNullOrEmpty(targetScene))
                return false;

            // コンテキストがある場合の追加チェック
            if (context != null)
            {
                // 重要な処理中（セーブ中、ロード中等）は終了不可
                // マルチプレイゲームでの特別な制約
                // プラットフォーム固有の制約
            }

            return true;
        }

        /// <summary>
        /// 終了コマンドを作成します
        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

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
        private bool userConfirmed = false;
        private System.DateTime quitStartTime;

        public QuitGameCommand(QuitGameCommandDefinition quitDefinition, object executionContext)
        {
            definition = quitDefinition;
            context = executionContext;
        }

        /// <summary>
        /// 終了コマンドの実行
        /// </summary>
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

            // プラットフォーム固有の処理
            if (Application.isMobilePlatform)
            {
                HandleMobilePlatformQuit();
                return;
            }

            // 確認ダイアログの表示
            if (definition.showConfirmDialog && definition.quitType == QuitGameCommandDefinition.QuitType.Confirm)
            {
                ShowQuitConfirmDialog();
                return; // ユーザーの応答待ち
            }

            // 実際の終了処理を開始
            StartQuitProcess();
            executed = true;
        }

        /// <summary>
        /// モバイルプラットフォーム用の終了処理
        /// </summary>
        private void HandleMobilePlatformQuit()
        {
            // モバイルでは通常アプリ終了ではなく最小化
            if (definition.minimizeInsteadOfQuitOnMobile)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                // Androidでのホーム画面への移動
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    currentActivity.Call<bool>("moveTaskToBack", true);
                }
#elif UNITY_IOS && !UNITY_EDITOR
                // iOSでは通常アプリ終了は推奨されない
                UnityEngine.Debug.LogWarning("App quit is not recommended on iOS platform");
#endif
                return;
            }

            // モバイルでの確認ダイアログ
            if (definition.confirmOnMobile)
            {
                ShowMobileQuitConfirmDialog();
                return;
            }

            StartQuitProcess();
        }

        /// <summary>
        /// 終了確認ダイアログの表示
        /// </summary>
        private void ShowQuitConfirmDialog()
        {
            // 実際の実装では UISystem との連携
            // var dialog = UISystem.ShowConfirmDialog(definition.confirmMessage, OnQuitConfirmed, OnQuitCancelled);

            // 仮の実装（即座に確認されたとして処理）
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Showing quit confirmation: {definition.confirmMessage}");
#endif
            
            // 実際の実装ではユーザー入力待ち
            OnQuitConfirmed(); // テスト用に即座に確認
        }

        /// <summary>
        /// モバイル用終了確認ダイアログの表示
        /// </summary>
        private void ShowMobileQuitConfirmDialog()
        {
            // モバイル固有のダイアログ表示
            ShowQuitConfirmDialog(); // 基本的には同じ処理
        }

        /// <summary>
        /// 終了が確認された際のコールバック
        /// </summary>
        private void OnQuitConfirmed()
        {
            userConfirmed = true;
            StartQuitProcess();
        }

        /// <summary>
        /// 終了がキャンセルされた際のコールバック
        /// </summary>
        private void OnQuitCancelled()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Quit cancelled by user");
#endif
            // キャンセル時の処理
        }

        /// <summary>
        /// 実際の終了処理を開始
        /// </summary>
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
            else
            {
                QuitApplication();
            }
        }

        /// <summary>
        /// メインメニューに戻る
        /// </summary>
        private void ExecuteReturnToMainMenu()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Returning to main menu: {definition.targetScene}");
#endif

            // 自動セーブ（設定されている場合）
            if (definition.autoSaveBeforeQuit)
            {
                PerformAutoSave();
            }

            // ゲーム状態のリセット
            ResetGameState();

            // シーン遷移
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
        /// アプリケーション再起動
        /// </summary>
        private void ExecuteRestart()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Executing application restart");
#endif

            // 再起動用の設定保存
            SaveRestartFlag();

            // 安全な終了処理
            ExecuteSafeQuit();

            // 実際の実装では、プラットフォーム固有の再起動処理
            // Windows: Process.Start(Application.dataPath + "/../" + Application.productName + ".exe");
            // その他のプラットフォームでは適切な方法で再起動
        }

        /// <summary>
        /// 自動セーブの実行
        /// </summary>
        private void PerformAutoSave()
        {
            try
            {
                // 実際の実装では SaveSystem との連携
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
        /// 設定の保存
        /// </summary>
        private void SaveSettings()
        {
            // 実際の実装では SettingsManager との連携
            PlayerPrefs.Save(); // Unity標準の設定保存

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Settings saved before quit");
#endif
        }

        /// <summary>
        /// 統計データの保存
        /// </summary>
        private void SaveStatistics()
        {
            // 実際の実装では StatisticsSystem との連携
            // StatisticsSystem.SaveAllStatistics();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Statistics saved before quit");
#endif
        }

        /// <summary>
        /// 最小限のクリーンアップ
        /// </summary>
        private void PerformMinimalCleanup()
        {
            // メモリの強制開放
            System.GC.Collect();

            // リソースのアンロード
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// 完全なクリーンアップ
        /// </summary>
        private void PerformFullCleanup()
        {
            // 最小限のクリーンアップ
            PerformMinimalCleanup();

            // 一時ファイルの削除
            if (definition.cleanupTempFiles)
            {
                CleanupTempFiles();
            }

            // ネットワーク接続の切断
            // NetworkManager.Disconnect();

            // オーディオの停止
            AudioListener.pause = true;
        }

        /// <summary>
        /// 一時ファイルのクリーンアップ
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
        /// ゲーム状態のリセット
        /// </summary>
        private void ResetGameState()
        {
            // ゲーム状態のリセット
            Time.timeScale = 1f;
            AudioListener.pause = false;
            
            // 実際の実装では GameManager との連携
            // GameManager.ResetToMainMenuState();
        }

        /// <summary>
        /// フェードアウトの開始
        /// </summary>
        private void StartFadeOut()
        {
            // 実際の実装では UISystem または SceneTransition との連携
            // SceneTransition.FadeOut(definition.fadeOutDuration, OnFadeOutComplete);

            // 仮の実装（一定時間後に終了）
            System.Threading.Tasks.Task.Delay((int)(definition.fadeOutDuration * 1000))
                .ContinueWith(_ => QuitApplication());
        }

        /// <summary>
        /// シーンへのフェードアウト開始
        /// </summary>
        private void StartFadeOutToScene()
        {
            // 実際の実装では SceneTransition との連携
            // SceneTransition.FadeOutToScene(definition.targetScene, definition.fadeOutDuration);

            // 仮の実装
            System.Threading.Tasks.Task.Delay((int)(definition.fadeOutDuration * 1000))
                .ContinueWith(_ => TransitionToMainMenu());
        }

        /// <summary>
        /// メインメニューへの遷移
        /// </summary>
        private void TransitionToMainMenu()
        {
            // 実際の実装では SceneManager との連携
            UnityEngine.SceneManagement.SceneManager.LoadScene(definition.targetScene);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Transitioned to main menu: {definition.targetScene}");
#endif
        }

        /// <summary>
        /// 再起動フラグの保存
        /// </summary>
        private void SaveRestartFlag()
        {
            PlayerPrefs.SetInt("ShouldRestart", 1);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// アプリケーション終了
        /// </summary>
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
        /// 終了処理の更新（タイムアウトチェック等、外部から定期的に呼び出される）
        /// </summary>
        public void UpdateQuit()
        {
            if (!quitInProgress) return;

            // 自動セーブのタイムアウトチェック
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
        /// Undo操作（終了のキャンセル）
        /// </summary>
        public void Undo()
        {
            if (!executed) return;

            // 終了進行中の場合はキャンセル
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
        /// このコマンドがUndo可能かどうか（終了進行中のみキャンセル可能）
        /// </summary>
        public bool CanUndo => quitInProgress && definition.quitType != QuitGameCommandDefinition.QuitType.Immediate;

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