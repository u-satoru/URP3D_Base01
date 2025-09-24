using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// 繧ｲ繝ｼ繝荳譎ょ●豁｢繧ｳ繝槭Φ繝峨・螳夂ｾｩ縲・    /// 繧ｲ繝ｼ繝縺ｮ荳譎ょ●豁｢/蜀埼幕繧｢繧ｯ繧ｷ繝ｧ繝ｳ繧偵き繝励そ繝ｫ蛹悶＠縺ｾ縺吶・    /// 
    /// 荳ｻ縺ｪ讖溯・・・    /// - 繧ｲ繝ｼ繝譎る俣縺ｮ蛛懈ｭ｢/蜀埼幕
    /// - 荳譎ょ●豁｢荳ｭ縺ｮUI陦ｨ遉ｺ蛻ｶ蠕｡
    /// - 繧ｪ繝ｼ繝・ぅ繧ｪ縺ｮ荳譎ょ●豁｢/蜀埼幕
    /// - 蜈･蜉帙・辟｡蜉ｹ蛹・譛牙柑蛹門宛蠕｡
    /// </summary>
    [System.Serializable]
    public class PauseGameCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 荳譎ょ●豁｢縺ｮ遞ｮ鬘槭ｒ螳夂ｾｩ縺吶ｋ蛻玲嫌蝙・        /// </summary>
        public enum PauseType
        {
            Full,           // 螳悟・荳譎ょ●豁｢・域凾髢薙・浹縲∝・蜉帛・縺ｦ・・            Partial,        // 驛ｨ蛻・ｸ譎ょ●豁｢・域凾髢薙・縺ｿ縲・浹縺ｯ邯咏ｶ夂ｭ会ｼ・            Menu,           // 繝｡繝九Η繝ｼ陦ｨ遉ｺ逕ｨ荳譎ょ●豁｢
            Dialog,         // 繝繧､繧｢繝ｭ繧ｰ陦ｨ遉ｺ逕ｨ荳譎ょ●豁｢
            Cutscene        // 繧ｫ繝・ヨ繧ｷ繝ｼ繝ｳ逕ｨ荳譎ょ●豁｢
        }

        [Header("Pause Parameters")]
        public PauseType pauseType = PauseType.Full;
        public bool toggleMode = true; // true: 繝医げ繝ｫ蠖｢蠑・ false: 荳譎ょ●豁｢縺ｮ縺ｿ
        public bool allowUnpauseInCode = true; // 繧ｳ繝ｼ繝峨°繧峨・蜀埼幕繧定ｨｱ蜿ｯ縺吶ｋ縺・
        [Header("Time Control")]
        public bool pauseGameTime = true;
        public bool pausePhysics = true;
        public bool pauseAnimations = true;
        public bool pauseParticles = true;

        [Header("Audio Control")]
        public bool pauseMusic = true;
        public bool pauseSFX = true;
        public bool pauseVoice = false; // 繝懊う繧ｹ縺ｯ邯咏ｶ壹☆繧句ｴ蜷医′螟壹＞
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
        public bool showTimeScale = false; // 繝・ヰ繝・げ逕ｨ

        [Header("Auto Pause")]
        public bool pauseOnFocusLost = true;
        public bool pauseOnMinimize = true;
        public bool resumeOnFocusGain = false; // 謇句虚蜀埼幕繧定ｦ∵ｱゅ☆繧句ｴ蜷医・false

        /// <summary>
        /// 繝・ヵ繧ｩ繝ｫ繝医さ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public PauseGameCommandDefinition()
        {
        }

        /// <summary>
        /// 繝代Λ繝｡繝ｼ繧ｿ莉倥″繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public PauseGameCommandDefinition(PauseType type, bool isToggle = true)
        {
            pauseType = type;
            toggleMode = isToggle;
        }

        /// <summary>
        /// 荳譎ょ●豁｢繧ｳ繝槭Φ繝峨′螳溯｡悟庄閭ｽ縺九←縺・°繧貞愛螳壹＠縺ｾ縺・        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 蝓ｺ譛ｬ逧・↑螳溯｡悟庄閭ｽ諤ｧ繝√ぉ繝・け
            if (backgroundDimAmount < 0f || backgroundDimAmount > 1f) return false;

            // 繧ｳ繝ｳ繝・く繧ｹ繝医′縺ゅｋ蝣ｴ蜷医・霑ｽ蜉繝√ぉ繝・け
            if (context != null)
            {
                // 繧ｲ繝ｼ繝縺ｮ迥ｶ諷九メ繧ｧ繝・け・域里縺ｫ荳譎ょ●豁｢荳ｭ縲√Ο繝ｼ繝・ぅ繝ｳ繧ｰ荳ｭ遲会ｼ・                // 驥崎ｦ√↑繧ｷ繝ｼ繝ｳ縺ｧ縺ｮ荳譎ょ●豁｢蛻ｶ髯撰ｼ医き繝・ヨ繧ｷ繝ｼ繝ｳ荳ｭ遲会ｼ・                // 繝槭Ν繝√・繝ｬ繧､繧ｲ繝ｼ繝縺ｧ縺ｮ荳譎ょ●豁｢蛻ｶ邏・            }

            return true;
        }

        /// <summary>
        /// 荳譎ょ●豁｢繧ｳ繝槭Φ繝峨ｒ菴懈・縺励∪縺・        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new PauseGameCommand(this, context);
        }
    }

    /// <summary>
    /// PauseGameCommandDefinition縺ｫ蟇ｾ蠢懊☆繧句ｮ滄圀縺ｮ繧ｳ繝槭Φ繝牙ｮ溯｣・    /// </summary>
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
        /// 荳譎ょ●豁｢繧ｳ繝槭Φ繝峨・螳溯｡・        /// </summary>
        public void Execute()
        {
            if (executed) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.pauseType} pause: toggle={definition.toggleMode}");
#endif

            // 迴ｾ蝨ｨ縺ｮ迥ｶ諷九ｒ菫晏ｭ・            SaveCurrentState();

            // 繝医げ繝ｫ繝｢繝ｼ繝峨・蝣ｴ蜷医・迥ｶ諷九ｒ蛻・ｊ譖ｿ縺・            if (definition.toggleMode)
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
                // 荳譎ょ●豁｢縺ｮ縺ｿ繝｢繝ｼ繝峨・蝣ｴ蜷医・蟶ｸ縺ｫ荳譎ょ●豁｢
                PauseGame();
            }

            executed = true;
        }

        /// <summary>
        /// 繧ｲ繝ｼ繝縺ｮ荳譎ょ●豁｢
        /// </summary>
        private void PauseGame()
        {
            if (isPaused) return;

            isPaused = true;

            // 譎る俣蛻ｶ蠕｡
            if (definition.pauseGameTime)
            {
                Time.timeScale = 0f;
            }

            // 迚ｩ逅・・荳譎ょ●豁｢
            if (definition.pausePhysics)
            {
                Physics.simulationMode = SimulationMode.Script;
                Physics2D.simulationMode = SimulationMode2D.Script;
            }

            // 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ縺ｮ荳譎ょ●豁｢
            if (definition.pauseAnimations)
            {
                PauseAnimations();
            }

            // 繝代・繝・ぅ繧ｯ繝ｫ縺ｮ荳譎ょ●豁｢
            if (definition.pauseParticles)
            {
                PauseParticles();
            }

            // 繧ｪ繝ｼ繝・ぅ繧ｪ蛻ｶ蠕｡
            PauseAudio();

            // 蜈･蜉帛宛蠕｡
            ConfigureInputForPause();

            // UI蛻ｶ蠕｡
            ConfigureUIForPause();

            // 荳譎ょ●豁｢繧､繝吶Φ繝医・逋ｺ陦・            // EventSystem.Publish(new GamePausedEvent(definition.pauseType));

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Game paused");
#endif
        }

        /// <summary>
        /// 繧ｲ繝ｼ繝縺ｮ蜀埼幕
        /// </summary>
        private void ResumeGame()
        {
            if (!isPaused) return;

            // 迥ｶ諷九・蠕ｩ蜈・            RestorePreviousState();

            isPaused = false;

            // 荳譎ょ●豁｢隗｣髯､繧､繝吶Φ繝医・逋ｺ陦・            // EventSystem.Publish(new GameResumedEvent());

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Game resumed");
#endif
        }

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ迥ｶ諷九ｒ菫晏ｭ・        /// </summary>
        private void SaveCurrentState()
        {
            previousState = new PauseState
            {
            TimeScale = Time.timeScale,
            PhysicsSimulationMode = Physics.simulationMode,
            Physics2DSimulationMode = Physics2D.simulationMode,
            // 縺昴・莉悶・迥ｶ諷九ｂ菫晏ｭ・            };
        }

        /// <summary>
        /// 蜑阪・迥ｶ諷九ｒ蠕ｩ蜈・        /// </summary>
        private void RestorePreviousState()
        {
            if (previousState == null) return;

            // 譎る俣蛻ｶ蠕｡縺ｮ蠕ｩ蜈・            if (definition.pauseGameTime)
            {
                Time.timeScale = previousState.TimeScale;
            }

            // 迚ｩ逅・・蠕ｩ蜈・            if (definition.pausePhysics)
            {
                Physics.simulationMode = previousState.PhysicsSimulationMode;
                Physics2D.simulationMode = previousState.Physics2DSimulationMode;
            }

            // 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ縺ｮ蠕ｩ蜈・            if (definition.pauseAnimations)
            {
                ResumeAnimations();
            }

            // 繝代・繝・ぅ繧ｯ繝ｫ縺ｮ蠕ｩ蜈・            if (definition.pauseParticles)
            {
                ResumeParticles();
            }

            // 繧ｪ繝ｼ繝・ぅ繧ｪ縺ｮ蠕ｩ蜈・            ResumeAudio();

            // 蜈･蜉帙・蠕ｩ蜈・            ConfigureInputForResume();

            // UI縺ｮ蠕ｩ蜈・            ConfigureUIForResume();
        }

        /// <summary>
        /// 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ縺ｮ荳譎ょ●豁｢
        /// </summary>
        private void PauseAnimations()
        {
            // 蜈ｨ縺ｦ縺ｮAnimator繧呈､懃ｴ｢縺励※荳譎ょ●豁｢
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
        /// 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ縺ｮ蜀埼幕
        /// </summary>
        private void ResumeAnimations()
        {
            // 蜈ｨ縺ｦ縺ｮAnimator繧呈､懃ｴ｢縺励※蜀埼幕
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
        /// 繝代・繝・ぅ繧ｯ繝ｫ縺ｮ荳譎ょ●豁｢
        /// </summary>
        private void PauseParticles()
        {
            // 蜈ｨ縺ｦ縺ｮParticleSystem繧呈､懃ｴ｢縺励※荳譎ょ●豁｢
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
        /// 繝代・繝・ぅ繧ｯ繝ｫ縺ｮ蜀埼幕
        /// </summary>
        private void ResumeParticles()
        {
            // 蜈ｨ縺ｦ縺ｮParticleSystem繧呈､懃ｴ｢縺励※蜀埼幕
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
        /// 繧ｪ繝ｼ繝・ぅ繧ｪ縺ｮ荳譎ょ●豁｢蛻ｶ蠕｡
        /// </summary>
        private void PauseAudio()
        {
            // AudioSource縺ｮ荳譎ょ●豁｢蛻ｶ蠕｡
            var audioSources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            foreach (var audioSource in audioSources)
            {
                if (!audioSource.isPlaying) continue;

                // 繧ｿ繧ｰ縺ｾ縺溘・繝ｬ繧､繝､繝ｼ縺ｫ繧医ｋ蛻ｶ蠕｡
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

            // AudioListener 縺ｮ荳譎ょ●豁｢
            AudioListener.pause = definition.pauseMusic || definition.pauseSFX;
        }

        /// <summary>
        /// 繧ｪ繝ｼ繝・ぅ繧ｪ縺ｮ蜀埼幕蛻ｶ蠕｡
        /// </summary>
        private void ResumeAudio()
        {
            // AudioSource縺ｮ蜀埼幕蛻ｶ蠕｡
            var audioSources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            foreach (var audioSource in audioSources)
            {
                if (audioSource.isPlaying) continue; // 譌｢縺ｫ蜀咲函荳ｭ縺ｯ繧ｹ繧ｭ繝・・

                // 繧ｿ繧ｰ縺ｫ繧医ｋ蜀埼幕蛻､螳夲ｼ亥ｮ滄圀縺ｮ螳溯｣・〒縺ｯ繧医ｊ隧ｳ邏ｰ縺ｪ邂｡逅・′蠢・ｦ・ｼ・                audioSource.UnPause();
            }

            // AudioListener 縺ｮ蜀埼幕
            AudioListener.pause = false;
        }

        /// <summary>
        /// 荳譎ょ●豁｢逕ｨ縺ｮ蜈･蜉帛宛蠕｡
        /// </summary>
        private void ConfigureInputForPause()
        {
            if (definition.disableGameplayInput)
            {
                // 繧ｲ繝ｼ繝繝励Ξ繧､蜈･蜉帙・辟｡蜉ｹ蛹厄ｼ亥ｮ滄圀縺ｮ螳溯｣・〒縺ｯ InputSystem 縺ｨ縺ｮ騾｣謳ｺ・・                // InputSystem.DisableActionMap("Gameplay");
            }

            if (definition.allowMenuInput)
            {
                // 繝｡繝九Η繝ｼ蜈･蜉帙・譛牙柑蛹・                // InputSystem.EnableActionMap("UI");
            }

            // 繝槭え繧ｹ繧ｫ繝ｼ繧ｽ繝ｫ縺ｮ蛻ｶ蠕｡
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
        /// 蜀埼幕逕ｨ縺ｮ蜈･蜉帛宛蠕｡
        /// </summary>
        private void ConfigureInputForResume()
        {
            // 繧ｲ繝ｼ繝繝励Ξ繧､蜈･蜉帙・蠕ｩ蜈・            // InputSystem.EnableActionMap("Gameplay");

            // 繝槭え繧ｹ繧ｫ繝ｼ繧ｽ繝ｫ縺ｮ蠕ｩ蜈・            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ previousState 縺九ｉ蠕ｩ蜈・        }

        /// <summary>
        /// 荳譎ょ●豁｢逕ｨ縺ｮUI蛻ｶ蠕｡
        /// </summary>
        private void ConfigureUIForPause()
        {
            // 荳譎ょ●豁｢繝｡繝九Η繝ｼ縺ｮ陦ｨ遉ｺ
            if (definition.showPauseMenu)
            {
                ShowPauseMenu();
            }

            // 閭梧勹縺ｮ證苓ｻ｢
            if (definition.dimBackground)
            {
                DimBackground(definition.backgroundDimAmount);
            }

            // 閭梧勹縺ｮ繝悶Λ繝ｼ
            if (definition.blurBackground)
            {
                ApplyBackgroundBlur();
            }

            // 荳譎ょ●豁｢繧､繝ｳ繧ｸ繧ｱ繝ｼ繧ｿ繝ｼ縺ｮ陦ｨ遉ｺ
            if (definition.showPauseIndicator)
            {
                ShowPauseIndicator(definition.pauseIndicatorText);
            }

            // 繧ｿ繧､繝繧ｹ繧ｱ繝ｼ繝ｫ陦ｨ遉ｺ・医ョ繝舌ャ繧ｰ逕ｨ・・            if (definition.showTimeScale)
            {
                ShowTimeScaleDebugInfo();
            }
        }

        /// <summary>
        /// 蜀埼幕逕ｨ縺ｮUI蛻ｶ蠕｡
        /// </summary>
        private void ConfigureUIForResume()
        {
            // 荳譎ょ●豁｢繝｡繝九Η繝ｼ縺ｮ髱櫁｡ｨ遉ｺ
            if (definition.showPauseMenu)
            {
                HidePauseMenu();
            }

            // 閭梧勹蜉ｹ譫懊・隗｣髯､
            if (definition.dimBackground)
            {
                RemoveBackgroundDim();
            }

            if (definition.blurBackground)
            {
                RemoveBackgroundBlur();
            }

            // 繧､繝ｳ繧ｸ繧ｱ繝ｼ繧ｿ繝ｼ縺ｮ髱櫁｡ｨ遉ｺ
            if (definition.showPauseIndicator)
            {
                HidePauseIndicator();
            }

            if (definition.showTimeScale)
            {
                HideTimeScaleDebugInfo();
            }
        }

        // UI蛻ｶ蠕｡繝｡繧ｽ繝・ラ・亥ｮ滄圀縺ｮ螳溯｣・〒縺ｯ UISystem 縺ｨ縺ｮ騾｣謳ｺ・・        private void ShowPauseMenu() { /* 荳譎ょ●豁｢繝｡繝九Η繝ｼ陦ｨ遉ｺ */ }
        private void HidePauseMenu() { /* 荳譎ょ●豁｢繝｡繝九Η繝ｼ髱櫁｡ｨ遉ｺ */ }
        private void DimBackground(float amount) { /* 閭梧勹證苓ｻ｢ */ }
        private void RemoveBackgroundDim() { /* 閭梧勹證苓ｻ｢隗｣髯､ */ }
        private void ApplyBackgroundBlur() { /* 閭梧勹繝悶Λ繝ｼ */ }
        private void RemoveBackgroundBlur() { /* 閭梧勹繝悶Λ繝ｼ隗｣髯､ */ }
        private void ShowPauseIndicator(string text) { /* 荳譎ょ●豁｢繧､繝ｳ繧ｸ繧ｱ繝ｼ繧ｿ繝ｼ陦ｨ遉ｺ */ }
        private void HidePauseIndicator() { /* 荳譎ょ●豁｢繧､繝ｳ繧ｸ繧ｱ繝ｼ繧ｿ繝ｼ髱櫁｡ｨ遉ｺ */ }
        private void ShowTimeScaleDebugInfo() { /* 繧ｿ繧､繝繧ｹ繧ｱ繝ｼ繝ｫ諠・ｱ陦ｨ遉ｺ */ }
        private void HideTimeScaleDebugInfo() { /* 繧ｿ繧､繝繧ｹ繧ｱ繝ｼ繝ｫ諠・ｱ髱櫁｡ｨ遉ｺ */ }

        /// <summary>
        /// 螟夜Κ縺九ｉ縺ｮ蜀埼幕隕∵ｱゑｼ・enu繧ФI邨檎罰・・        /// </summary>
        public void RequestResume()
        {
            if (isPaused && definition.allowUnpauseInCode)
            {
                ResumeGame();
            }
        }

        /// <summary>
        /// 繧｢繝励Μ繧ｱ繝ｼ繧ｷ繝ｧ繝ｳ繝輔か繝ｼ繧ｫ繧ｹ螟画峩譎ゅ・蜃ｦ逅・        /// </summary>
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
        /// 繧｢繝励Μ繧ｱ繝ｼ繧ｷ繝ｧ繝ｳ譛蟆丞喧譎ゅ・蜃ｦ逅・        /// </summary>
        public void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && definition.pauseOnMinimize && !isPaused)
            {
                PauseGame();
            }
        }

        /// <summary>
        /// Undo謫堺ｽ懶ｼ井ｸ譎ょ●豁｢迥ｶ諷九・蜿悶ｊ豸医＠・・        /// </summary>
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
        /// 縺薙・繧ｳ繝槭Φ繝峨′Undo蜿ｯ閭ｽ縺九←縺・°
        /// </summary>
        public bool CanUndo => executed;

        /// <summary>
        /// 迴ｾ蝨ｨ荳譎ょ●豁｢荳ｭ縺九←縺・°
        /// </summary>
        public bool IsPaused => isPaused;
    }

    /// <summary>
    /// 荳譎ょ●豁｢蜑阪・迥ｶ諷九ｒ菫晏ｭ倥☆繧九け繝ｩ繧ｹ
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
        // 蠢・ｦ√↓蠢懊§縺ｦ莉悶・迥ｶ諷九ｂ霑ｽ蜉
    }
}
