using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// ゲーム一時停止コマンドの定義。
    /// ゲームの一時停止/再開アクションをカプセル化します。
    /// 
    /// 主な機能：
    /// - ゲーム時間の停止/再開
    /// - 一時停止中のUI表示制御
    /// - オーディオの一時停止/再開
    /// - 入力の無効化/有効化制御
    /// </summary>
    [System.Serializable]
    public class PauseGameCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 一時停止の種類を定義する列挙型
        /// </summary>
        public enum PauseType
        {
            Full,           // 完全一時停止（時間、音、入力全て）
            Partial,        // 部分一時停止（時間のみ、音は継続等）
            Menu,           // メニュー表示用一時停止
            Dialog,         // ダイアログ表示用一時停止
            Cutscene        // カットシーン用一時停止
        }

        [Header("Pause Parameters")]
        public PauseType pauseType = PauseType.Full;
        public bool toggleMode = true; // true: トグル形式, false: 一時停止のみ
        public bool allowUnpauseInCode = true; // コードからの再開を許可するか

        [Header("Time Control")]
        public bool pauseGameTime = true;
        public bool pausePhysics = true;
        public bool pauseAnimations = true;
        public bool pauseParticles = true;

        [Header("Audio Control")]
        public bool pauseMusic = true;
        public bool pauseSFX = true;
        public bool pauseVoice = false; // ボイスは継続する場合が多い
        public bool pauseAmbient = true;

        [Header("Input Control")]
        public bool disableGameplayInput = true;
        public bool allowMenuInput = true;
        public bool allowPauseToggle = true;
        public bool disableMouseCursor = false;

        [Header("UI Behavior")]
        public bool showPauseMenu = true;
        public bool dimBackground = true;
        public float backgroundDimAmount = 0.5f;
        public bool blurBackground = false;

        [Header("Visual Effects")]
        public bool showPauseIndicator = true;
        public string pauseIndicatorText = "PAUSED";
        public bool showTimeScale = false; // デバッグ用

        [Header("Auto Pause")]
        public bool pauseOnFocusLost = true;
        public bool pauseOnMinimize = true;
        public bool resumeOnFocusGain = false; // 手動再開を要求する場合はfalse

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public PauseGameCommandDefinition()
        {
        }

        /// <summary>
        /// パラメータ付きコンストラクタ
        /// </summary>
        public PauseGameCommandDefinition(PauseType type, bool isToggle = true)
        {
            pauseType = type;
            toggleMode = isToggle;
        }

        /// <summary>
        /// 一時停止コマンドが実行可能かどうかを判定します
        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本的な実行可能性チェック
            if (backgroundDimAmount < 0f || backgroundDimAmount > 1f) return false;

            // コンテキストがある場合の追加チェック
            if (context != null)
            {
                // ゲームの状態チェック（既に一時停止中、ローディング中等）
                // 重要なシーンでの一時停止制限（カットシーン中等）
                // マルチプレイゲームでの一時停止制約
            }

            return true;
        }

        /// <summary>
        /// 一時停止コマンドを作成します
        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new PauseGameCommand(this, context);
        }
    }

    /// <summary>
    /// PauseGameCommandDefinitionに対応する実際のコマンド実装
    /// </summary>
    public class PauseGameCommand : ICommand
    {
        private PauseGameCommandDefinition definition;
        private object context;
        private bool executed = false;
        private bool isPaused = false;
        private PauseState previousState;

        public PauseGameCommand(PauseGameCommandDefinition pauseDefinition, object executionContext)
        {
            definition = pauseDefinition;
            context = executionContext;
        }

        /// <summary>
        /// 一時停止コマンドの実行
        /// </summary>
        public void Execute()
        {
            if (executed) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.pauseType} pause: toggle={definition.toggleMode}");
#endif

            // 現在の状態を保存
            SaveCurrentState();

            // トグルモードの場合は状態を切り替え
            if (definition.toggleMode)
            {
                if (isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
            else
            {
                // 一時停止のみモードの場合は常に一時停止
                PauseGame();
            }

            executed = true;
        }

        /// <summary>
        /// ゲームの一時停止
        /// </summary>
        private void PauseGame()
        {
            if (isPaused) return;

            isPaused = true;

            // 時間制御
            if (definition.pauseGameTime)
            {
                Time.timeScale = 0f;
            }

            // 物理の一時停止
            if (definition.pausePhysics)
            {
                Physics.simulationMode = SimulationMode.Script;
                Physics2D.simulationMode = SimulationMode2D.Script;
            }

            // アニメーションの一時停止
            if (definition.pauseAnimations)
            {
                PauseAnimations();
            }

            // パーティクルの一時停止
            if (definition.pauseParticles)
            {
                PauseParticles();
            }

            // オーディオ制御
            PauseAudio();

            // 入力制御
            ConfigureInputForPause();

            // UI制御
            ConfigureUIForPause();

            // 一時停止イベントの発行
            // EventSystem.Publish(new GamePausedEvent(definition.pauseType));

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Game paused");
#endif
        }

        /// <summary>
        /// ゲームの再開
        /// </summary>
        private void ResumeGame()
        {
            if (!isPaused) return;

            // 状態の復元
            RestorePreviousState();

            isPaused = false;

            // 一時停止解除イベントの発行
            // EventSystem.Publish(new GameResumedEvent());

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Game resumed");
#endif
        }

        /// <summary>
        /// 現在の状態を保存
        /// </summary>
        private void SaveCurrentState()
        {
            previousState = new PauseState
            {
            TimeScale = Time.timeScale,
            PhysicsSimulationMode = Physics.simulationMode,
            Physics2DSimulationMode = Physics2D.simulationMode,
            // その他の状態も保存
            };
        }

        /// <summary>
        /// 前の状態を復元
        /// </summary>
        private void RestorePreviousState()
        {
            if (previousState == null) return;

            // 時間制御の復元
            if (definition.pauseGameTime)
            {
                Time.timeScale = previousState.TimeScale;
            }

            // 物理の復元
            if (definition.pausePhysics)
            {
                Physics.simulationMode = previousState.PhysicsSimulationMode;
                Physics2D.simulationMode = previousState.Physics2DSimulationMode;
            }

            // アニメーションの復元
            if (definition.pauseAnimations)
            {
                ResumeAnimations();
            }

            // パーティクルの復元
            if (definition.pauseParticles)
            {
                ResumeParticles();
            }

            // オーディオの復元
            ResumeAudio();

            // 入力の復元
            ConfigureInputForResume();

            // UIの復元
            ConfigureUIForResume();
        }

        /// <summary>
        /// アニメーションの一時停止
        /// </summary>
        private void PauseAnimations()
        {
            // 全てのAnimatorを検索して一時停止
            var animators = Object.FindObjectsByType<Animator>(FindObjectsSortMode.None);
            foreach (var animator in animators)
            {
                if (animator.gameObject.activeInHierarchy)
                {
                    animator.speed = 0f;
                }
            }
        }

        /// <summary>
        /// アニメーションの再開
        /// </summary>
        private void ResumeAnimations()
        {
            // 全てのAnimatorを検索して再開
            var animators = Object.FindObjectsByType<Animator>(FindObjectsSortMode.None);
            foreach (var animator in animators)
            {
                if (animator.gameObject.activeInHierarchy)
                {
                    animator.speed = 1f;
                }
            }
        }

        /// <summary>
        /// パーティクルの一時停止
        /// </summary>
        private void PauseParticles()
        {
            // 全てのParticleSystemを検索して一時停止
            var particles = Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
            foreach (var particle in particles)
            {
                if (particle.gameObject.activeInHierarchy)
                {
                    particle.Pause();
                }
            }
        }

        /// <summary>
        /// パーティクルの再開
        /// </summary>
        private void ResumeParticles()
        {
            // 全てのParticleSystemを検索して再開
            var particles = Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
            foreach (var particle in particles)
            {
                if (particle.gameObject.activeInHierarchy && particle.isPaused)
                {
                    particle.Play();
                }
            }
        }

        /// <summary>
        /// オーディオの一時停止制御
        /// </summary>
        private void PauseAudio()
        {
            // AudioSourceの一時停止制御
            var audioSources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            foreach (var audioSource in audioSources)
            {
                if (!audioSource.isPlaying) continue;

                // タグまたはレイヤーによる制御
                bool shouldPause = false;

                if (audioSource.CompareTag("Music") && definition.pauseMusic)
                    shouldPause = true;
                else if (audioSource.CompareTag("SFX") && definition.pauseSFX)
                    shouldPause = true;
                else if (audioSource.CompareTag("Voice") && definition.pauseVoice)
                    shouldPause = true;
                else if (audioSource.CompareTag("Ambient") && definition.pauseAmbient)
                    shouldPause = true;

                if (shouldPause)
                {
                    audioSource.Pause();
                }
            }

            // AudioListener の一時停止
            AudioListener.pause = definition.pauseMusic || definition.pauseSFX;
        }

        /// <summary>
        /// オーディオの再開制御
        /// </summary>
        private void ResumeAudio()
        {
            // AudioSourceの再開制御
            var audioSources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            foreach (var audioSource in audioSources)
            {
                if (audioSource.isPlaying) continue; // 既に再生中はスキップ

                // タグによる再開判定（実際の実装ではより詳細な管理が必要）
                audioSource.UnPause();
            }

            // AudioListener の再開
            AudioListener.pause = false;
        }

        /// <summary>
        /// 一時停止用の入力制御
        /// </summary>
        private void ConfigureInputForPause()
        {
            if (definition.disableGameplayInput)
            {
                // ゲームプレイ入力の無効化（実際の実装では InputSystem との連携）
                // InputSystem.DisableActionMap("Gameplay");
            }

            if (definition.allowMenuInput)
            {
                // メニュー入力の有効化
                // InputSystem.EnableActionMap("UI");
            }

            // マウスカーソルの制御
            if (definition.disableMouseCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        /// <summary>
        /// 再開用の入力制御
        /// </summary>
        private void ConfigureInputForResume()
        {
            // ゲームプレイ入力の復元
            // InputSystem.EnableActionMap("Gameplay");

            // マウスカーソルの復元
            // 実際の実装では previousState から復元
        }

        /// <summary>
        /// 一時停止用のUI制御
        /// </summary>
        private void ConfigureUIForPause()
        {
            // 一時停止メニューの表示
            if (definition.showPauseMenu)
            {
                ShowPauseMenu();
            }

            // 背景の暗転
            if (definition.dimBackground)
            {
                DimBackground(definition.backgroundDimAmount);
            }

            // 背景のブラー
            if (definition.blurBackground)
            {
                ApplyBackgroundBlur();
            }

            // 一時停止インジケーターの表示
            if (definition.showPauseIndicator)
            {
                ShowPauseIndicator(definition.pauseIndicatorText);
            }

            // タイムスケール表示（デバッグ用）
            if (definition.showTimeScale)
            {
                ShowTimeScaleDebugInfo();
            }
        }

        /// <summary>
        /// 再開用のUI制御
        /// </summary>
        private void ConfigureUIForResume()
        {
            // 一時停止メニューの非表示
            if (definition.showPauseMenu)
            {
                HidePauseMenu();
            }

            // 背景効果の解除
            if (definition.dimBackground)
            {
                RemoveBackgroundDim();
            }

            if (definition.blurBackground)
            {
                RemoveBackgroundBlur();
            }

            // インジケーターの非表示
            if (definition.showPauseIndicator)
            {
                HidePauseIndicator();
            }

            if (definition.showTimeScale)
            {
                HideTimeScaleDebugInfo();
            }
        }

        // UI制御メソッド（実際の実装では UISystem との連携）
        private void ShowPauseMenu() { /* 一時停止メニュー表示 */ }
        private void HidePauseMenu() { /* 一時停止メニュー非表示 */ }
        private void DimBackground(float amount) { /* 背景暗転 */ }
        private void RemoveBackgroundDim() { /* 背景暗転解除 */ }
        private void ApplyBackgroundBlur() { /* 背景ブラー */ }
        private void RemoveBackgroundBlur() { /* 背景ブラー解除 */ }
        private void ShowPauseIndicator(string text) { /* 一時停止インジケーター表示 */ }
        private void HidePauseIndicator() { /* 一時停止インジケーター非表示 */ }
        private void ShowTimeScaleDebugInfo() { /* タイムスケール情報表示 */ }
        private void HideTimeScaleDebugInfo() { /* タイムスケール情報非表示 */ }

        /// <summary>
        /// 外部からの再開要求（MenuやUI経由）
        /// </summary>
        public void RequestResume()
        {
            if (isPaused && definition.allowUnpauseInCode)
            {
                ResumeGame();
            }
        }

        /// <summary>
        /// アプリケーションフォーカス変更時の処理
        /// </summary>
        public void OnApplicationFocusChanged(bool hasFocus)
        {
            if (!hasFocus && definition.pauseOnFocusLost && !isPaused)
            {
                PauseGame();
            }
            else if (hasFocus && definition.resumeOnFocusGain && isPaused)
            {
                ResumeGame();
            }
        }

        /// <summary>
        /// アプリケーション最小化時の処理
        /// </summary>
        public void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && definition.pauseOnMinimize && !isPaused)
            {
                PauseGame();
            }
        }

        /// <summary>
        /// Undo操作（一時停止状態の取り消し）
        /// </summary>
        public void Undo()
        {
            if (!executed) return;

            if (isPaused)
            {
                ResumeGame();
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Pause command undone");
#endif

            executed = false;
        }

        /// <summary>
        /// このコマンドがUndo可能かどうか
        /// </summary>
        public bool CanUndo => executed;

        /// <summary>
        /// 現在一時停止中かどうか
        /// </summary>
        public bool IsPaused => isPaused;
    }

    /// <summary>
    /// 一時停止前の状態を保存するクラス
    /// </summary>
    [System.Serializable]
    public class PauseState
    {
        public float TimeScale;
        public SimulationMode PhysicsSimulationMode;
        public SimulationMode2D Physics2DSimulationMode;
        public bool AudioListenerPause;
        public CursorLockMode CursorLockState;
        public bool CursorVisible;
        // 必要に応じて他の状態も追加
    }
}