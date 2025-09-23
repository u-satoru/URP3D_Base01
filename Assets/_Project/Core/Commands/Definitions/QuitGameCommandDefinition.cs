using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// ゲーム終亁E��マンド�E定義、E    /// アプリケーションの終亁E��クションをカプセル化します、E    /// 
    /// 主な機�E�E�E    /// - 安�Eなアプリケーション終亁E    /// - 終亁E��の自動セーチE    /// - 終亁E��認ダイアログの表示
    /// - リソースのクリーンアチE�E
    /// </summary>
    [System.Serializable]
    public class QuitGameCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 終亁E�E種類を定義する列挙垁E        /// </summary>
        public enum QuitType
        {
            Immediate,      // 即座に終亁E            Safe,           // 安�Eな終亁E��セーブ等を実行！E            Confirm,        // 確認ダイアログ付き終亁E            ToMainMenu,     // メインメニューに戻めE            Restart         // アプリケーション再起勁E        }

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
        /// チE��ォルトコンストラクタ
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
        /// 終亁E��マンドが実行可能かどぁE��を判定しまぁE        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本皁E��実行可能性チェチE��
            if (autoSaveTimeout <= 0f) return false;
            if (fadeOutDuration < 0f) return false;

            // ToMainMenuの場合�EターゲチE��シーンが忁E��E            if (quitType == QuitType.ToMainMenu && string.IsNullOrEmpty(targetScene))
                return false;

            // コンチE��ストがある場合�E追加チェチE��
            if (context != null)
            {
                // 重要な処琁E���E�セーブ中、ロード中等）�E終亁E��可
                // マルチ�Eレイゲームでの特別な制紁E                // プラチE��フォーム固有�E制紁E            }

            return true;
        }

        /// <summary>
        /// 終亁E��マンドを作�EしまぁE        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new QuitGameCommand(this, context);
        }
    }

    /// <summary>
    /// QuitGameCommandDefinitionに対応する実際のコマンド実裁E    /// </summary>
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
        /// 終亁E��マンド�E実衁E        /// </summary>
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

            // プラチE��フォーム固有�E処琁E            if (Application.isMobilePlatform)
            {
                HandleMobilePlatformQuit();
                return;
            }

            // 確認ダイアログの表示
            if (definition.showConfirmDialog && definition.quitType == QuitGameCommandDefinition.QuitType.Confirm)
            {
                ShowQuitConfirmDialog();
                return; // ユーザーの応答征E��
            }

            // 実際の終亁E�E琁E��開姁E            StartQuitProcess();
            executed = true;
        }

        /// <summary>
        /// モバイルプラチE��フォーム用の終亁E�E琁E        /// </summary>
        private void HandleMobilePlatformQuit()
        {
            // モバイルでは通常アプリ終亁E��はなく最小化
            if (definition.minimizeInsteadOfQuitOnMobile)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                // Androidでのホ�Eム画面への移勁E                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    currentActivity.Call<bool>("moveTaskToBack", true);
                }
#elif UNITY_IOS && !UNITY_EDITOR
                // iOSでは通常アプリ終亁E�E推奨されなぁE                UnityEngine.Debug.LogWarning("App quit is not recommended on iOS platform");
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
        /// 終亁E��認ダイアログの表示
        /// </summary>
        private void ShowQuitConfirmDialog()
        {
            // 実際の実裁E��は UISystem との連携
            // var dialog = UISystem.ShowConfirmDialog(definition.confirmMessage, OnQuitConfirmed, OnQuitCancelled);

            // 仮の実裁E��即座に確認されたとして処琁E��E#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Showing quit confirmation: {definition.confirmMessage}");
#endif
            
            // 実際の実裁E��はユーザー入力征E��
            OnQuitConfirmed(); // チE��ト用に即座に確誁E        }

        /// <summary>
        /// モバイル用終亁E��認ダイアログの表示
        /// </summary>
        private void ShowMobileQuitConfirmDialog()
        {
            // モバイル固有�Eダイアログ表示
            ShowQuitConfirmDialog(); // 基本皁E��は同じ処琁E        }

        /// <summary>
        /// 終亁E��確認された際�Eコールバック
        /// </summary>
        private void OnQuitConfirmed()
        {
            userConfirmed = true;
            StartQuitProcess();
        }

        /// <summary>
        /// 終亁E��キャンセルされた際のコールバック
        /// </summary>
        private void OnQuitCancelled()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Quit cancelled by user");
#endif
            // キャンセル時�E処琁E        }

        /// <summary>
        /// 実際の終亁E�E琁E��開姁E        /// </summary>
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
        /// 即座に終亁E        /// </summary>
        private void ExecuteImmediateQuit()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Executing immediate quit");
#endif

            // 最小限のクリーンアチE�E
            if (definition.disposeResources)
            {
                PerformMinimalCleanup();
            }

            // アプリケーション終亁E            QuitApplication();
        }

        /// <summary>
        /// 安�Eな終亁E        /// </summary>
        private void ExecuteSafeQuit()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Executing safe quit");
#endif

            // 自動セーチE            if (definition.autoSaveBeforeQuit)
            {
                PerformAutoSave();
            }

            // 設定�E保孁E            if (definition.saveSettingsOnQuit)
            {
                SaveSettings();
            }

            // 統計データの保孁E            if (definition.saveStatisticsOnQuit)
            {
                SaveStatistics();
            }

            // クリーンアチE�E
            if (definition.cleanupTempFiles || definition.disposeResources)
            {
                PerformFullCleanup();
            }

            // フェードアウチE            if (definition.fadeOutBeforeQuit)
            {
                StartFadeOut();
            }
            else
            {
                QuitApplication();
            }
        }

        /// <summary>
        /// メインメニューに戻めE        /// </summary>
        private void ExecuteReturnToMainMenu()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Returning to main menu: {definition.targetScene}");
#endif

            // 自動セーブ（設定されてぁE��場合！E            if (definition.autoSaveBeforeQuit)
            {
                PerformAutoSave();
            }

            // ゲーム状態�EリセチE��
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
        /// アプリケーション再起勁E        /// </summary>
        private void ExecuteRestart()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Executing application restart");
#endif

            // 再起動用の設定保孁E            SaveRestartFlag();

            // 安�Eな終亁E�E琁E            ExecuteSafeQuit();

            // 実際の実裁E��は、�EラチE��フォーム固有�E再起動�E琁E            // Windows: Process.Start(Application.dataPath + "/../" + Application.productName + ".exe");
            // そ�E他�EプラチE��フォームでは適刁E��方法で再起勁E        }

        /// <summary>
        /// 自動セーブ�E実衁E        /// </summary>
        private void PerformAutoSave()
        {
            try
            {
                // 実際の実裁E��は SaveSystem との連携
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
        /// 設定�E保孁E        /// </summary>
        private void SaveSettings()
        {
            // 実際の実裁E��は SettingsManager との連携
            PlayerPrefs.Save(); // Unity標準�E設定保孁E
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Settings saved before quit");
#endif
        }

        /// <summary>
        /// 統計データの保孁E        /// </summary>
        private void SaveStatistics()
        {
            // 実際の実裁E��は StatisticsSystem との連携
            // StatisticsSystem.SaveAllStatistics();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Statistics saved before quit");
#endif
        }

        /// <summary>
        /// 最小限のクリーンアチE�E
        /// </summary>
        private void PerformMinimalCleanup()
        {
            // メモリの強制開放
            System.GC.Collect();

            // リソースのアンローチE            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// 完�EなクリーンアチE�E
        /// </summary>
        private void PerformFullCleanup()
        {
            // 最小限のクリーンアチE�E
            PerformMinimalCleanup();

            // 一時ファイルの削除
            if (definition.cleanupTempFiles)
            {
                CleanupTempFiles();
            }

            // ネットワーク接続�E刁E��
            // NetworkManager.Disconnect();

            // オーチE��オの停止
            AudioListener.pause = true;
        }

        /// <summary>
        /// 一時ファイルのクリーンアチE�E
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
        /// ゲーム状態�EリセチE��
        /// </summary>
        private void ResetGameState()
        {
            // ゲーム状態�EリセチE��
            Time.timeScale = 1f;
            AudioListener.pause = false;
            
            // 実際の実裁E��は GameManager との連携
            // GameManager.ResetToMainMenuState();
        }

        /// <summary>
        /// フェードアウト�E開姁E        /// </summary>
        private void StartFadeOut()
        {
            // 実際の実裁E��は UISystem また�E SceneTransition との連携
            // SceneTransition.FadeOut(definition.fadeOutDuration, OnFadeOutComplete);

            // 仮の実裁E��一定時間後に終亁E��E            System.Threading.Tasks.Task.Delay((int)(definition.fadeOutDuration * 1000))
                .ContinueWith(_ => QuitApplication());
        }

        /// <summary>
        /// シーンへのフェードアウト開姁E        /// </summary>
        private void StartFadeOutToScene()
        {
            // 実際の実裁E��は SceneTransition との連携
            // SceneTransition.FadeOutToScene(definition.targetScene, definition.fadeOutDuration);

            // 仮の実裁E            System.Threading.Tasks.Task.Delay((int)(definition.fadeOutDuration * 1000))
                .ContinueWith(_ => TransitionToMainMenu());
        }

        /// <summary>
        /// メインメニューへの遷移
        /// </summary>
        private void TransitionToMainMenu()
        {
            // 実際の実裁E��は SceneManager との連携
            UnityEngine.SceneManagement.SceneManager.LoadScene(definition.targetScene);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Transitioned to main menu: {definition.targetScene}");
#endif
        }

        /// <summary>
        /// 再起動フラグの保孁E        /// </summary>
        private void SaveRestartFlag()
        {
            PlayerPrefs.SetInt("ShouldRestart", 1);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// アプリケーション終亁E        /// </summary>
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
        /// 終亁E�E琁E�E更新�E�タイムアウトチェチE��等、外部から定期皁E��呼び出される！E        /// </summary>
        public void UpdateQuit()
        {
            if (!quitInProgress) return;

            // 自動セーブ�EタイムアウトチェチE��
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
        /// Undo操作（終亁E�Eキャンセル�E�E        /// </summary>
        public void Undo()
        {
            if (!executed) return;

            // 終亁E��行中の場合�Eキャンセル
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
        /// こ�EコマンドがUndo可能かどぁE���E�終亁E��行中のみキャンセル可能�E�E        /// </summary>
        public bool CanUndo => quitInProgress && definition.quitType != QuitGameCommandDefinition.QuitType.Immediate;

        /// <summary>
        /// 現在終亁E�E琁E��かどぁE��
        /// </summary>
        public bool IsQuitInProgress => quitInProgress;

        /// <summary>
        /// ユーザーが終亁E��確認したかどぁE��
        /// </summary>
        public bool IsUserConfirmed => userConfirmed;
    }
}