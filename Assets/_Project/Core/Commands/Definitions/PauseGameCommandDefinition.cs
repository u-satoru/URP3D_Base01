using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// ゲーム一時停止コマンド�E定義、E    /// ゲームの一時停止/再開アクションをカプセル化します、E    /// 
    /// 主な機�E�E�E    /// - ゲーム時間の停止/再開
    /// - 一時停止中のUI表示制御
    /// - オーチE��オの一時停止/再開
    /// - 入力�E無効匁E有効化制御
    /// </summary>
    [System.Serializable]
    public class PauseGameCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 一時停止の種類を定義する列挙垁E        /// </summary>
        public enum PauseType
        {
            Full,           // 完�E一時停止�E�時間、E��、�E力�Eて�E�E            Partial,        // 部刁E��時停止�E�時間�Eみ、E��は継続等！E            Menu,           // メニュー表示用一時停止
            Dialog,         // ダイアログ表示用一時停止
            Cutscene        // カチE��シーン用一時停止
        }

        [Header("Pause Parameters")]
        public PauseType pauseType = PauseType.Full;
        public bool toggleMode = true; // true: トグル形弁E false: 一時停止のみ
        public bool allowUnpauseInCode = true; // コードから�E再開を許可するぁE
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
        public bool showTimeScale = false; // チE��チE��用

        [Header("Auto Pause")]
        public bool pauseOnFocusLost = true;
        public bool pauseOnMinimize = true;
        public bool resumeOnFocusGain = false; // 手動再開を要求する場合�Efalse

        /// <summary>
        /// チE��ォルトコンストラクタ
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
        /// 一時停止コマンドが実行可能かどぁE��を判定しまぁE        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本皁E��実行可能性チェチE��
            if (backgroundDimAmount < 0f || backgroundDimAmount > 1f) return false;

            // コンチE��ストがある場合�E追加チェチE��
            if (context != null)
            {
                // ゲームの状態チェチE���E�既に一時停止中、ローチE��ング中等！E                // 重要なシーンでの一時停止制限（カチE��シーン中等！E                // マルチ�Eレイゲームでの一時停止制紁E            }

            return true;
        }

        /// <summary>
        /// 一時停止コマンドを作�EしまぁE        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new PauseGameCommand(this, context);
        }
    }

    /// <summary>
    /// PauseGameCommandDefinitionに対応する実際のコマンド実裁E    /// </summary>
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
        /// 一時停止コマンド�E実衁E        /// </summary>
        public void Execute()
        {
            if (executed) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.pauseType} pause: toggle={definition.toggleMode}");
#endif

            // 現在の状態を保孁E            SaveCurrentState();

            // トグルモード�E場合�E状態を刁E��替ぁE            if (definition.toggleMode)
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
                // 一時停止のみモード�E場合�E常に一時停止
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

            // 物琁E�E一時停止
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

            // パ�EチE��クルの一時停止
            if (definition.pauseParticles)
            {
                PauseParticles();
            }

            // オーチE��オ制御
            PauseAudio();

            // 入力制御
            ConfigureInputForPause();

            // UI制御
            ConfigureUIForPause();

            // 一時停止イベント�E発衁E            // EventSystem.Publish(new GamePausedEvent(definition.pauseType));

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

            // 状態�E復允E            RestorePreviousState();

            isPaused = false;

            // 一時停止解除イベント�E発衁E            // EventSystem.Publish(new GameResumedEvent());

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Game resumed");
#endif
        }

        /// <summary>
        /// 現在の状態を保孁E        /// </summary>
        private void SaveCurrentState()
        {
            previousState = new PauseState
            {
            TimeScale = Time.timeScale,
            PhysicsSimulationMode = Physics.simulationMode,
            Physics2DSimulationMode = Physics2D.simulationMode,
            // そ�E他�E状態も保孁E            };
        }

        /// <summary>
        /// 前�E状態を復允E        /// </summary>
        private void RestorePreviousState()
        {
            if (previousState == null) return;

            // 時間制御の復允E            if (definition.pauseGameTime)
            {
                Time.timeScale = previousState.TimeScale;
            }

            // 物琁E�E復允E            if (definition.pausePhysics)
            {
                Physics.simulationMode = previousState.PhysicsSimulationMode;
                Physics2D.simulationMode = previousState.Physics2DSimulationMode;
            }

            // アニメーションの復允E            if (definition.pauseAnimations)
            {
                ResumeAnimations();
            }

            // パ�EチE��クルの復允E            if (definition.pauseParticles)
            {
                ResumeParticles();
            }

            // オーチE��オの復允E            ResumeAudio();

            // 入力�E復允E            ConfigureInputForResume();

            // UIの復允E            ConfigureUIForResume();
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
        /// パ�EチE��クルの一時停止
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
        /// パ�EチE��クルの再開
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
        /// オーチE��オの一時停止制御
        /// </summary>
        private void PauseAudio()
        {
            // AudioSourceの一時停止制御
            var audioSources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            foreach (var audioSource in audioSources)
            {
                if (!audioSource.isPlaying) continue;

                // タグまた�Eレイヤーによる制御
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
        /// オーチE��オの再開制御
        /// </summary>
        private void ResumeAudio()
        {
            // AudioSourceの再開制御
            var audioSources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            foreach (var audioSource in audioSources)
            {
                if (audioSource.isPlaying) continue; // 既に再生中はスキチE�E

                // タグによる再開判定（実際の実裁E��はより詳細な管琁E��忁E��E��E                audioSource.UnPause();
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
                // ゲームプレイ入力�E無効化（実際の実裁E��は InputSystem との連携�E�E                // InputSystem.DisableActionMap("Gameplay");
            }

            if (definition.allowMenuInput)
            {
                // メニュー入力�E有効匁E                // InputSystem.EnableActionMap("UI");
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
            // ゲームプレイ入力�E復允E            // InputSystem.EnableActionMap("Gameplay");

            // マウスカーソルの復允E            // 実際の実裁E��は previousState から復允E        }

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

            // タイムスケール表示�E�デバッグ用�E�E            if (definition.showTimeScale)
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

            // 背景効果�E解除
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

        // UI制御メソチE���E�実際の実裁E��は UISystem との連携�E�E        private void ShowPauseMenu() { /* 一時停止メニュー表示 */ }
        private void HidePauseMenu() { /* 一時停止メニュー非表示 */ }
        private void DimBackground(float amount) { /* 背景暗転 */ }
        private void RemoveBackgroundDim() { /* 背景暗転解除 */ }
        private void ApplyBackgroundBlur() { /* 背景ブラー */ }
        private void RemoveBackgroundBlur() { /* 背景ブラー解除 */ }
        private void ShowPauseIndicator(string text) { /* 一時停止インジケーター表示 */ }
        private void HidePauseIndicator() { /* 一時停止インジケーター非表示 */ }
        private void ShowTimeScaleDebugInfo() { /* タイムスケール惁E��表示 */ }
        private void HideTimeScaleDebugInfo() { /* タイムスケール惁E��非表示 */ }

        /// <summary>
        /// 外部からの再開要求！EenuやUI経由�E�E        /// </summary>
        public void RequestResume()
        {
            if (isPaused && definition.allowUnpauseInCode)
            {
                ResumeGame();
            }
        }

        /// <summary>
        /// アプリケーションフォーカス変更時�E処琁E        /// </summary>
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
        /// アプリケーション最小化時�E処琁E        /// </summary>
        public void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && definition.pauseOnMinimize && !isPaused)
            {
                PauseGame();
            }
        }

        /// <summary>
        /// Undo操作（一時停止状態�E取り消し�E�E        /// </summary>
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
        /// こ�EコマンドがUndo可能かどぁE��
        /// </summary>
        public bool CanUndo => executed;

        /// <summary>
        /// 現在一時停止中かどぁE��
        /// </summary>
        public bool IsPaused => isPaused;
    }

    /// <summary>
    /// 一時停止前�E状態を保存するクラス
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
        // 忁E��に応じて他�E状態も追加
    }
}