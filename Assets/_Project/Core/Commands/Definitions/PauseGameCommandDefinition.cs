using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// Game pause command definition.
    /// Encapsulates game pause/resume actions.
    ///
    /// Main features:
    /// - Game time pause/resume
    /// - UI display control during pause
    /// - Audio pause/resume
    /// - Input disabling and management
    /// </summary>
    [System.Serializable]
    public class PauseGameCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// Types of pause behavior
        /// </summary>
        public enum PauseType
        {
            Full,           // Complete pause (time, sound, input all stopped)
            Partial,        // Partial pause (only time, sound continues)
            Menu,           // Menu display pause
            Dialog,         // Dialog display pause
            Cutscene        // Cutscene pause
        }

        [Header("Pause Parameters")]
        public PauseType pauseType = PauseType.Full;
        public bool toggleMode = true; // true: toggle mode, false: pause only
        public bool allowUnpauseInCode = true; // Allow resume from code

        [Header("Time Control")]
        public bool pauseGameTime = true;
        public bool pausePhysics = true;
        public bool pauseAnimations = true;
        public bool pauseParticles = true;

        [Header("Audio Control")]
        public bool pauseMusic = true;
        public bool pauseSFX = true;
        public bool pauseVoice = false; // Voice often continues during pause
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
        public bool showTimeScale = false; // For debug

        [Header("Auto Pause")]
        public bool pauseOnFocusLost = true;
        public bool pauseOnMinimize = true;
        public bool resumeOnFocusGain = false; // false to require manual resume

        /// <summary>
        /// Default constructor
        /// </summary>
        public PauseGameCommandDefinition()
        {
        }

        /// <summary>
        /// Parameterized constructor
        /// </summary>
        public PauseGameCommandDefinition(PauseType type, bool isToggle = true)
        {
            pauseType = type;
            toggleMode = isToggle;
        }

        /// <summary>
        /// Check if pause command can be executed
        /// </summary>
        public bool CanExecute(object context = null)
        {
            // Basic executability check
            if (backgroundDimAmount < 0f || backgroundDimAmount > 1f) return false;

            // Additional checks if context exists
            if (context != null)
            {
                // Game state checks (already paused, loading, etc.)
                // Pause restrictions in important scenes (cutscenes, etc.)
                // Pause control in multiplayer games
            }

            return true;
        }

        /// <summary>
        /// Create pause command instance
        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new PauseGameCommand(this, context);
        }
    }

    /// <summary>
    /// Actual implementation of pause command
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
        /// Execute pause command
        /// </summary>
        public void Execute()
        {
            if (executed) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.pauseType} pause: toggle={definition.toggleMode}");
#endif

            // Save current state
            SaveCurrentState();

            // Toggle mode switches state
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
                // Pause only mode always pauses
                PauseGame();
            }

            executed = true;
        }

        /// <summary>
        /// Check if command can be executed
        /// </summary>
        public bool CanExecute()
        {
            return !executed && definition.CanExecute(context);
        }

        /// <summary>
        /// Pause the game
        /// </summary>
        private void PauseGame()
        {
            if (isPaused) return;

            isPaused = true;

            // Time control
            if (definition.pauseGameTime)
            {
                Time.timeScale = 0f;
            }

            // Physics pause
            if (definition.pausePhysics)
            {
                Physics.simulationMode = SimulationMode.Script;
                Physics2D.simulationMode = SimulationMode2D.Script;
            }

            // Animation pause
            if (definition.pauseAnimations)
            {
                PauseAnimations();
            }

            // Particle pause
            if (definition.pauseParticles)
            {
                PauseParticles();
            }

            // Audio control
            PauseAudio();

            // Input control
            ConfigureInputForPause();

            // UI control
            ConfigureUIForPause();

            // Raise pause event
            // EventSystem.Publish(new GamePausedEvent(definition.pauseType));

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Game paused");
#endif
        }

        /// <summary>
        /// Resume the game
        /// </summary>
        private void ResumeGame()
        {
            if (!isPaused) return;

            // Restore state
            RestorePreviousState();

            isPaused = false;

            // Raise resume event
            // EventSystem.Publish(new GameResumedEvent());

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Game resumed");
#endif
        }

        /// <summary>
        /// Save current state
        /// </summary>
        private void SaveCurrentState()
        {
            previousState = new PauseState
            {
                TimeScale = Time.timeScale,
                PhysicsSimulationMode = Physics.simulationMode,
                Physics2DSimulationMode = Physics2D.simulationMode,
                // Save other states as needed
            };
        }

        /// <summary>
        /// Restore previous state
        /// </summary>
        private void RestorePreviousState()
        {
            if (previousState == null) return;

            // Restore time control
            if (definition.pauseGameTime)
            {
                Time.timeScale = previousState.TimeScale;
            }

            // Restore physics
            if (definition.pausePhysics)
            {
                Physics.simulationMode = previousState.PhysicsSimulationMode;
                Physics2D.simulationMode = previousState.Physics2DSimulationMode;
            }

            // Restore animations
            if (definition.pauseAnimations)
            {
                ResumeAnimations();
            }

            // Restore particles
            if (definition.pauseParticles)
            {
                ResumeParticles();
            }

            // Restore audio
            ResumeAudio();

            // Restore input
            ConfigureInputForResume();

            // Restore UI
            ConfigureUIForResume();
        }

        /// <summary>
        /// Pause animations
        /// </summary>
        private void PauseAnimations()
        {
            // Find all Animators and pause them
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
        /// Resume animations
        /// </summary>
        private void ResumeAnimations()
        {
            // Find all Animators and resume them
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
        /// Pause particles
        /// </summary>
        private void PauseParticles()
        {
            // Find all ParticleSystems and pause them
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
        /// Resume particles
        /// </summary>
        private void ResumeParticles()
        {
            // Find all ParticleSystems and resume them
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
        /// Audio pause control
        /// </summary>
        private void PauseAudio()
        {
            // AudioSource pause control
            var audioSources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            foreach (var audioSource in audioSources)
            {
                if (!audioSource.isPlaying) continue;

                // Control by tag or layer
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

            // AudioListener pause
            AudioListener.pause = definition.pauseMusic || definition.pauseSFX;
        }

        /// <summary>
        /// Audio resume control
        /// </summary>
        private void ResumeAudio()
        {
            // AudioSource resume control
            var audioSources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            foreach (var audioSource in audioSources)
            {
                if (audioSource.isPlaying) continue; // Skip if already playing

                // Resume based on tag (actual implementation needs more detailed management)
                audioSource.UnPause();
            }

            // AudioListener resume
            AudioListener.pause = false;
        }

        /// <summary>
        /// Configure input for pause
        /// </summary>
        private void ConfigureInputForPause()
        {
            if (definition.disableGameplayInput)
            {
                // Disable gameplay input (actual implementation needs InputSystem integration)
                // InputSystem.DisableActionMap("Gameplay");
            }

            if (definition.allowMenuInput)
            {
                // Enable menu input
                // InputSystem.EnableActionMap("UI");
            }

            // Mouse cursor control
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
        /// Configure input for resume
        /// </summary>
        private void ConfigureInputForResume()
        {
            // Restore gameplay input
            // InputSystem.EnableActionMap("Gameplay");

            // Restore mouse cursor
            // Actual implementation should restore from previousState
        }

        /// <summary>
        /// Configure UI for pause
        /// </summary>
        private void ConfigureUIForPause()
        {
            // Show pause menu
            if (definition.showPauseMenu)
            {
                ShowPauseMenu();
            }

            // Dim background
            if (definition.dimBackground)
            {
                DimBackground(definition.backgroundDimAmount);
            }

            // Blur background
            if (definition.blurBackground)
            {
                ApplyBackgroundBlur();
            }

            // Show pause indicator
            if (definition.showPauseIndicator)
            {
                ShowPauseIndicator(definition.pauseIndicatorText);
            }

            // Show time scale (for debug)
            if (definition.showTimeScale)
            {
                ShowTimeScaleDebugInfo();
            }
        }

        /// <summary>
        /// Configure UI for resume
        /// </summary>
        private void ConfigureUIForResume()
        {
            // Hide pause menu
            if (definition.showPauseMenu)
            {
                HidePauseMenu();
            }

            // Remove background effects
            if (definition.dimBackground)
            {
                RemoveBackgroundDim();
            }

            if (definition.blurBackground)
            {
                RemoveBackgroundBlur();
            }

            // Hide indicators
            if (definition.showPauseIndicator)
            {
                HidePauseIndicator();
            }

            if (definition.showTimeScale)
            {
                HideTimeScaleDebugInfo();
            }
        }

        // UI control methods (actual implementation needs UISystem integration)
        private void ShowPauseMenu() { /* Show pause menu */ }
        private void HidePauseMenu() { /* Hide pause menu */ }
        private void DimBackground(float amount) { /* Dim background */ }
        private void RemoveBackgroundDim() { /* Remove background dim */ }
        private void ApplyBackgroundBlur() { /* Apply background blur */ }
        private void RemoveBackgroundBlur() { /* Remove background blur */ }
        private void ShowPauseIndicator(string text) { /* Show pause indicator */ }
        private void HidePauseIndicator() { /* Hide pause indicator */ }
        private void ShowTimeScaleDebugInfo() { /* Show time scale info */ }
        private void HideTimeScaleDebugInfo() { /* Hide time scale info */ }

        /// <summary>
        /// Resume request from external source (menu or UI system)
        /// </summary>
        public void RequestResume()
        {
            if (isPaused && definition.allowUnpauseInCode)
            {
                ResumeGame();
            }
        }

        /// <summary>
        /// Application focus change handling
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
        /// Application minimize handling
        /// </summary>
        public void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && definition.pauseOnMinimize && !isPaused)
            {
                PauseGame();
            }
        }

        /// <summary>
        /// Undo operation (cancel pause state)
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
        /// Whether this command can be undone
        /// </summary>
        public bool CanUndo => executed;

        /// <summary>
        /// Whether currently paused
        /// </summary>
        public bool IsPaused => isPaused;
    }

    /// <summary>
    /// Class to save state before pause
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
        // Add other states as needed
    }
}