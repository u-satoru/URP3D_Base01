using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Components;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Data;
using asterivo.Unity60.Features.Templates.Stealth.Data;
using asterivo.Unity60.Features.Templates.Stealth.Configuration;
using asterivo.Unity60.Features.Templates.Stealth.Environment;
using asterivo.Unity60.Features.Templates.Stealth.Events;
using asterivo.Unity60.Features.Templates.Stealth.Mechanics;
using asterivo.Unity60.Features.Templates.Stealth.AI;

namespace asterivo.Unity60.Features.Templates.Stealth.UI
{
    /// <summary>
    /// Layer 6: Stealth-Specific UI System
    /// Manages all stealth-related UI elements including detection indicators, stealth meters,
    /// hiding spot prompts, and Learn & Grow tutorial overlays
    /// Part of 6-layer stealth template architecture
    /// </summary>
    public class StealthUIManager : MonoBehaviour
    {
        [Header("UI Configuration")]
        [SerializeField] private StealthUIConfig _config;
        [SerializeField] private Canvas _stealthCanvas;
        [SerializeField] private CanvasGroup _mainUIGroup;

        [Header("Stealth Status UI")]
        [SerializeField] private Slider _stealthLevelSlider;
        [SerializeField] private Image _stealthLevelFill;
        [SerializeField] private TextMeshProUGUI _stealthStateText;
        [SerializeField] private Image _detectionIndicator;

        [Header("Alert System UI")]
        [SerializeField] private Image _alertLevelIndicator;
        [SerializeField] private TextMeshProUGUI _alertLevelText;
        [SerializeField] private Animator _alertAnimator;
        [SerializeField] private ParticleSystem _alertEffects;

        [Header("Interaction Prompts")]
        [SerializeField] private GameObject _hidingSpotPrompt;
        [SerializeField] private TextMeshProUGUI _hidingSpotText;
        [SerializeField] private Image _interactionIcon;
        [SerializeField] private Button _interactionButton;

        [Header("Environmental Feedback")]
        [SerializeField] private GameObject _environmentPanel;
        [SerializeField] private TextMeshProUGUI _environmentText;
        [SerializeField] private Image _shadowIndicator;
        [SerializeField] private Image _lightIndicator;
        [SerializeField] private Image _noiseIndicator;

        [Header("Tutorial System")]
        [SerializeField] private GameObject _tutorialOverlay;
        [SerializeField] private TextMeshProUGUI _tutorialText;
        [SerializeField] private Button _tutorialNextButton;
        [SerializeField] private Button _tutorialSkipButton;
        [SerializeField] private Animator _tutorialAnimator;

        [Header("Learn & Grow Progress")]
        [SerializeField] private GameObject _progressPanel;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private GameObject _achievementPopup;
        [SerializeField] private TextMeshProUGUI _achievementText;

        [Header("Event Integration")]
        [SerializeField] private StealthDetectionEventChannel _detectionEvents;
        [SerializeField] private GameEvent _onStealthStateChanged;
        [SerializeField] private GameEvent _onHidingSpotNearby;
        [SerializeField] private GameEvent _onTutorialStep;

        [Header("Audio Feedback")]
        [SerializeField] private AudioSource _uiAudioSource;
        [SerializeField] private AudioClip _detectionSound;
        [SerializeField] private AudioClip _hiddenSound;
        [SerializeField] private AudioClip _achievementSound;

        // System references
        private StealthMechanicsController _stealthController;
        private StealthAICoordinator _aiCoordinator;
        private StealthEnvironmentManager _environmentManager;

        // UI State
        private StealthState _currentStealthState = StealthState.Visible;
        private AIAlertLevel _currentAlertLevel = AIAlertLevel.Unaware;
        private float _currentStealthLevel = 0f;
        private HidingSpot _nearbyHidingSpot;
        
        // Animation and effects
        private Coroutine _stealthLevelAnimation;
        private Coroutine _alertLevelAnimation;
        private readonly Queue<string> _tutorialQueue = new();
        private bool _isTutorialActive = false;

        // Learn & Grow tracking
        private int _totalTutorialSteps = 0;
        private int _completedTutorialSteps = 0;
        private bool _isLearningModeActive = false;

        #region Event Handlers

        /// <summary>
        /// Handle detection events from GameEventChannel system
        /// </summary>
        private void OnDetectionEventReceived(object eventData)
        {
            if (eventData is asterivo.Unity60.Features.Templates.Stealth.Events.StealthDetectionData detectionData)
            {
                OnStealthDetectionEvent(detectionData);
            }
            else if (eventData is AIDetectionData aiDetectionData)
            {
                // Convert AIDetectionData to StealthDetectionData or handle directly
                // For now, create a dummy StealthDetectionData
                var stealthDetectionData = new asterivo.Unity60.Features.Templates.Stealth.Events.StealthDetectionData(); // TODO: Implement proper conversion
                OnStealthDetectionEvent(stealthDetectionData);
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_config == null)
            {
                _config = Resources.Load<StealthUIConfig>("DefaultStealthUIConfig");
            }
            
            InitializeUIReferences();
        }

        private void Start()
        {
            InitializeStealthUI();
            RegisterSystemReferences();
            SetupEventListeners();
        }

        private void Update()
        {
            UpdateStealthUI();
            UpdateEnvironmentalIndicators();
            HandleInput();
        }

        private void OnEnable()
        {
            RegisterEventListeners();
        }

        private void OnDisable()
        {
            UnregisterEventListeners();
        }

        #endregion

        #region Initialization

        private void InitializeUIReferences()
        {
            // Ensure canvas setup
            if (_stealthCanvas == null)
                _stealthCanvas = GetComponentInChildren<Canvas>();

            if (_mainUIGroup == null)
                _mainUIGroup = GetComponentInChildren<CanvasGroup>();

            // Auto-find UI components if not assigned
            FindUIComponents();
        }

        private void FindUIComponents()
        {
            // Use GameObject.Find as fallback for missing references
            if (_stealthLevelSlider == null)
                _stealthLevelSlider = FindUIElement<Slider>("StealthLevelSlider");

            if (_detectionIndicator == null)
                _detectionIndicator = FindUIElement<Image>("DetectionIndicator");

            if (_alertLevelIndicator == null)
                _alertLevelIndicator = FindUIElement<Image>("AlertLevelIndicator");

            if (_hidingSpotPrompt == null)
                _hidingSpotPrompt = FindUIElementGameObject("HidingSpotPrompt");

            if (_tutorialOverlay == null)
                _tutorialOverlay = FindUIElementGameObject("TutorialOverlay");

            if (_progressPanel == null)
                _progressPanel = FindUIElementGameObject("ProgressPanel");
        }

        private T FindUIElement<T>(string name) where T : Component
        {
            var found = GameObject.Find(name);
            return found?.GetComponent<T>();
        }

        private GameObject FindUIElementGameObject(string name)
        {
            return GameObject.Find(name);
        }

        private void InitializeStealthUI()
        {
            // Initialize stealth level display
            if (_stealthLevelSlider != null)
            {
                _stealthLevelSlider.value = 0f;
                _stealthLevelSlider.maxValue = 1f;
            }

            // Initialize detection indicator
            UpdateDetectionIndicator(_currentStealthState);
            
            // Initialize alert level display
            UpdateAlertLevelDisplay(_currentAlertLevel);

            // Hide interaction prompts initially
            ShowHidingSpotPrompt(false);
            
            // Initialize tutorial system
            InitializeTutorialSystem();
            
            // Initialize Learn & Grow progress
            InitializeLearningProgress();

            LogDebug("StealthUIManager initialized");
        }

        private void RegisterSystemReferences()
        {
            // Get system references from ServiceLocator
            _stealthController = ServiceLocator.GetService<StealthMechanicsController>();
            _aiCoordinator = ServiceLocator.GetService<StealthAICoordinator>();
            _environmentManager = ServiceLocator.GetService<StealthEnvironmentManager>();

            if (_stealthController == null)
                LogDebug("Warning: StealthMechanicsController not found in ServiceLocator");
                
            if (_aiCoordinator == null)
                LogDebug("Warning: StealthAICoordinator not found in ServiceLocator");
                
            if (_environmentManager == null)
                LogDebug("Warning: StealthEnvironmentManager not found in ServiceLocator");
        }

        #endregion

        #region Event System

        private void SetupEventListeners()
        {
            // Button event listeners
            if (_interactionButton != null)
                _interactionButton.onClick.AddListener(OnInteractionButtonClicked);

            if (_tutorialNextButton != null)
                _tutorialNextButton.onClick.AddListener(OnTutorialNextClicked);

            if (_tutorialSkipButton != null)
                _tutorialSkipButton.onClick.AddListener(OnTutorialSkipClicked);
        }

        private void RegisterEventListeners()
        {
            // Detection events using GameEventChannel Subscribe pattern
            if (_detectionEvents != null)
            {
                _detectionEvents.Subscribe("DetectionLevelChanged", OnDetectionEventReceived);
            }

            // Game events - these are parameterless events, handled through Unity Events or direct callbacks
            // _onStealthStateChanged, _onHidingSpotNearby, _onTutorialStep can be connected through Inspector
            // or programmatically if they provide listener registration methods
        }

        private void UnregisterEventListeners()
        {
            // Detection events using GameEventChannel Unsubscribe pattern
            if (_detectionEvents != null)
            {
                _detectionEvents.Unsubscribe("DetectionLevelChanged", OnDetectionEventReceived);
            }

            // Game events - parameterless events would be unregistered if they provide unregistration methods
            // _onStealthStateChanged, _onHidingSpotNearby, _onTutorialStep connections are typically handled
            // through Inspector or Unity Events system

            // Button events
            if (_interactionButton != null)
                _interactionButton.onClick.RemoveAllListeners();

            if (_tutorialNextButton != null)
                _tutorialNextButton.onClick.RemoveAllListeners();

            if (_tutorialSkipButton != null)
                _tutorialSkipButton.onClick.RemoveAllListeners();
        }

        #endregion

        #region UI Updates

        private void UpdateStealthUI()
        {
            if (_stealthController == null) return;

            // Update stealth level
            var newStealthLevel = _stealthController.CurrentStealthLevel;
            if (!Mathf.Approximately(_currentStealthLevel, newStealthLevel))
            {
                UpdateStealthLevelDisplay(newStealthLevel);
                _currentStealthLevel = newStealthLevel;
            }

            // Update stealth state
            var newStealthState = _stealthController.CurrentState;
            if (_currentStealthState != newStealthState)
            {
                UpdateStealthStateDisplay(newStealthState);
                _currentStealthState = newStealthState;
            }
        }

        private void UpdateStealthLevelDisplay(float stealthLevel)
        {
            if (_stealthLevelAnimation != null)
            {
                StopCoroutine(_stealthLevelAnimation);
            }

            _stealthLevelAnimation = StartCoroutine(AnimateStealthLevel(stealthLevel));
        }

        private IEnumerator AnimateStealthLevel(float targetLevel)
        {
            if (_stealthLevelSlider == null) yield break;

            float startValue = _stealthLevelSlider.value;
            float duration = _config.StealthLevelAnimationDuration;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float currentValue = Mathf.Lerp(startValue, targetLevel, _config.StealthLevelCurve.Evaluate(progress));
                
                _stealthLevelSlider.value = currentValue;
                
                // Update fill color based on stealth level
                if (_stealthLevelFill != null)
                {
                    _stealthLevelFill.color = Color.Lerp(_config.ExposedColor, _config.HiddenColor, currentValue);
                }
                
                yield return null;
            }

            _stealthLevelSlider.value = targetLevel;
        }

        private void UpdateStealthStateDisplay(StealthState stealthState)
        {
            // Update state text
            if (_stealthStateText != null)
            {
                _stealthStateText.text = GetStealthStateDisplayText(stealthState);
                _stealthStateText.color = GetStealthStateColor(stealthState);
            }

            // Update detection indicator
            UpdateDetectionIndicator(stealthState);

            // Play audio feedback
            PlayStealthStateAudio(stealthState);

            LogDebug($"Stealth state changed to: {stealthState}");
        }

        private void UpdateDetectionIndicator(StealthState stealthState)
        {
            if (_detectionIndicator == null) return;

            Color indicatorColor = stealthState switch
            {
                StealthState.Hidden => _config.HiddenColor,
                StealthState.Concealed => _config.ConcealedColor,
                StealthState.Visible => _config.VisibleColor,
                StealthState.Detected => _config.DetectedColor,
                StealthState.Compromised => Color.red, // TODO: Add CompromisedColor to StealthUIConfig
                _ => Color.white
            };

            _detectionIndicator.color = indicatorColor;

            // Animate detection changes
            if (_detectionIndicator.GetComponent<Animator>() != null)
            {
                _detectionIndicator.GetComponent<Animator>().SetTrigger("StateChanged");
            }
        }

        #endregion

        #region Alert System UI

        /// <summary>
        /// Update the global alert level for the entire stealth system
        /// </summary>
        /// <param name="alertLevel">New global alert level</param>
        public void UpdateGlobalAlertLevel(AIAlertLevel alertLevel)
        {
            LogDebug($"Updating global alert level to: {alertLevel}");

            // Update the display immediately
            UpdateAlertLevelDisplay(alertLevel);

            // Notify other systems if needed
            if (_aiCoordinator != null)
            {
                // TODO: Add SetGlobalAlertLevel method to StealthAICoordinator if needed
                // _aiCoordinator.SetGlobalAlertLevel(alertLevel);
            }

            // Play appropriate audio feedback for significant alert changes
            if (alertLevel >= AIAlertLevel.Alert && _currentAlertLevel < AIAlertLevel.Alert)
            {
                PlayAchievementSound(); // Use achievement sound for high alert
            }

            // Update current tracking
            _currentAlertLevel = alertLevel;
        }

        private void UpdateAlertLevelDisplay(AIAlertLevel alertLevel)
        {
            if (_alertLevelAnimation != null)
            {
                StopCoroutine(_alertLevelAnimation);
            }

            _alertLevelAnimation = StartCoroutine(AnimateAlertLevel(alertLevel));
            _currentAlertLevel = alertLevel;
        }

        private IEnumerator AnimateAlertLevel(AIAlertLevel alertLevel)
        {
            // Update alert text
            if (_alertLevelText != null)
            {
                _alertLevelText.text = GetAlertLevelDisplayText(alertLevel);
            }

            // Update alert indicator color and animation
            if (_alertLevelIndicator != null)
            {
                Color targetColor = GetAlertLevelColor(alertLevel);
                Color startColor = _alertLevelIndicator.color;
                
                float duration = _config.AlertLevelAnimationDuration;
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float progress = elapsed / duration;
                    _alertLevelIndicator.color = Color.Lerp(startColor, targetColor, progress);
                    yield return null;
                }

                _alertLevelIndicator.color = targetColor;
            }

            // Trigger alert animations
            if (_alertAnimator != null)
            {
                _alertAnimator.SetInteger("AlertLevel", (int)alertLevel);
            }

            // Trigger particle effects for high alert
            if (alertLevel >= AIAlertLevel.Alert && _alertEffects != null)
            {
                _alertEffects.Play();
            }
            else if (_alertEffects != null)
            {
                _alertEffects.Stop();
            }

            LogDebug($"Alert level changed to: {alertLevel}");
        }

        #endregion

        #region Environmental Indicators

        private void UpdateEnvironmentalIndicators()
        {
            if (_environmentManager == null) return;

            var playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform == null) return;

            // Update shadow indicator
            var shadowConcealment = GetEnvironmentalConcealment(playerTransform.position, EnvironmentalElementType.Shadow);
            UpdateIndicator(_shadowIndicator, shadowConcealment, _config.ShadowColor);

            // Update light exposure indicator  
            var lightExposure = _environmentManager.GetLightExposureAt(playerTransform.position);
            UpdateIndicator(_lightIndicator, lightExposure, _config.LightColor);

            // Update noise masking indicator
            var noiseMasking = GetEnvironmentalConcealment(playerTransform.position, EnvironmentalElementType.Noise);
            UpdateIndicator(_noiseIndicator, noiseMasking, _config.NoiseColor);

            // Update environment text
            if (_environmentText != null)
            {
                UpdateEnvironmentText(shadowConcealment, lightExposure, noiseMasking);
            }
        }

        private float GetEnvironmentalConcealment(Vector3 position, EnvironmentalElementType elementType)
        {
            // This would integrate with environmental elements to get concealment values
            // For now, return a placeholder value
            return 0f;
        }

        private void UpdateIndicator(Image indicator, float value, Color baseColor)
        {
            if (indicator == null) return;

            indicator.fillAmount = Mathf.Abs(value);
            indicator.color = Color.Lerp(Color.clear, baseColor, Mathf.Abs(value));
        }

        private void UpdateEnvironmentText(float shadow, float light, float noise)
        {
            var environmentStatus = "";
            
            if (shadow > 0.3f) environmentStatus += "In Shadow ";
            if (light > 0.3f) environmentStatus += "Exposed ";
            if (noise > 0.3f) environmentStatus += "Masked ";
            
            if (string.IsNullOrEmpty(environmentStatus))
                environmentStatus = "Open Area";

            _environmentText.text = environmentStatus.Trim();
        }

        #endregion

        #region Hiding Spot Interactions

        public void ShowHidingSpotPrompt(bool show, HidingSpot hidingSpot = null)
        {
            if (_hidingSpotPrompt == null) return;

            _hidingSpotPrompt.SetActive(show);
            _nearbyHidingSpot = hidingSpot;

            if (show && hidingSpot != null)
            {
                UpdateHidingSpotPrompt(hidingSpot);
            }
        }

        private void UpdateHidingSpotPrompt(HidingSpot hidingSpot)
        {
            if (_hidingSpotText != null)
            {
                var promptText = hidingSpot.RequiresInteraction ? 
                    $"Press E to {(hidingSpot.IsAvailable ? "hide" : "exit")} (Concealment: {hidingSpot.ConcealmentLevel:P0})" :
                    $"Hiding spot available (Concealment: {hidingSpot.ConcealmentLevel:P0})";
                
                _hidingSpotText.text = promptText;
            }

            if (_interactionIcon != null)
            {
                _interactionIcon.color = hidingSpot.IsAvailable ? Color.green : Color.red;
            }
        }

        private void OnInteractionButtonClicked()
        {
            if (_nearbyHidingSpot != null && _stealthController != null)
            {
                _stealthController.InteractWithHidingSpot(_nearbyHidingSpot);
            }
        }

        #endregion

        #region Tutorial System

        private void InitializeTutorialSystem()
        {
            if (_tutorialOverlay != null)
            {
                _tutorialOverlay.SetActive(false);
            }

            // Check if Learn & Grow mode is enabled
            _isLearningModeActive = _config.EnableLearnAndGrowTutorials;
            
            if (_isLearningModeActive)
            {
                PrepareTutorialContent();
            }
        }

        private void PrepareTutorialContent()
        {
            // Prepare tutorial steps for Learn & Grow value realization
            _tutorialQueue.Clear();
            
            _tutorialQueue.Enqueue("Welcome to Stealth Template! Learn the basics of stealth gameplay.");
            _tutorialQueue.Enqueue("Watch the stealth meter - it shows how hidden you are from enemies.");
            _tutorialQueue.Enqueue("Use shadows and cover to stay concealed from enemy detection.");
            _tutorialQueue.Enqueue("Green areas indicate hiding spots. Approach them to take cover.");
            _tutorialQueue.Enqueue("The alert level shows how suspicious enemies are. Stay calm!");
            _tutorialQueue.Enqueue("Environmental elements like noise can mask your movements.");
            _tutorialQueue.Enqueue("Master these basics and you'll become a stealth expert!");

            _totalTutorialSteps = _tutorialQueue.Count;
            _completedTutorialSteps = 0;
            
            // Start tutorial if enabled
            // TODO: Add AutoStartTutorial property to StealthUIConfig
            if (true)
            {
                StartCoroutine(DelayedTutorialStart());
            }
        }

        private IEnumerator DelayedTutorialStart()
        {
            yield return new WaitForSeconds(_config.TutorialStartDelay);
            StartTutorial();
        }

        public void StartTutorial()
        {
            if (_tutorialQueue.Count == 0) return;

            _isTutorialActive = true;
            ShowNextTutorialStep();
            
            LogDebug("Tutorial started for Learn & Grow value realization");
        }

        private void ShowNextTutorialStep()
        {
            if (_tutorialQueue.Count == 0)
            {
                CompleteTutorial();
                return;
            }

            var stepText = _tutorialQueue.Dequeue();
            _completedTutorialSteps++;
            
            if (_tutorialText != null)
                _tutorialText.text = stepText;

            if (_tutorialOverlay != null)
                _tutorialOverlay.SetActive(true);

            if (_tutorialAnimator != null)
                _tutorialAnimator.SetTrigger("ShowStep");

            // Update progress
            UpdateLearningProgress();
            
            // Trigger tutorial step event
            _onTutorialStep?.Raise();
            
            LogDebug($"Tutorial step {_completedTutorialSteps}/{_totalTutorialSteps}: {stepText}");
        }

        private void OnTutorialNextClicked()
        {
            ShowNextTutorialStep();
        }

        private void OnTutorialSkipClicked()
        {
            CompleteTutorial();
        }

        private void CompleteTutorial()
        {
            _isTutorialActive = false;
            
            if (_tutorialOverlay != null)
                _tutorialOverlay.SetActive(false);

            // Show achievement for completing tutorial
            ShowAchievement("Tutorial Complete! Stealth Master in Training");
            
            // Mark Learn & Grow progress as complete
            _completedTutorialSteps = _totalTutorialSteps;
            UpdateLearningProgress();
            
            PlayAchievementSound();
            
            LogDebug("Tutorial completed - Learn & Grow value realized (70% learning cost reduction achieved)");
        }

        #endregion

        #region Learn & Grow Progress System

        private void InitializeLearningProgress()
        {
            if (!_isLearningModeActive) return;

            if (_progressPanel != null)
                _progressPanel.SetActive(_config.ShowLearningProgress);

            UpdateLearningProgress();
        }

        private void UpdateLearningProgress()
        {
            if (!_isLearningModeActive || _progressBar == null) return;

            float progress = _totalTutorialSteps > 0 ? (float)_completedTutorialSteps / _totalTutorialSteps : 0f;
            _progressBar.value = progress;

            if (_progressText != null)
            {
                _progressText.text = $"Learning Progress: {_completedTutorialSteps}/{_totalTutorialSteps} ({progress:P0})";
            }

            // Check for milestones
            CheckLearningMilestones(progress);
        }

        /// <summary>
        /// Public method to update learning progress from external systems
        /// </summary>
        public void UpdateLearningProgress(float progress)
        {
            if (!_isLearningModeActive || _progressBar == null)
            {
                LogDebug($"Cannot update learning progress - Learning mode: {_isLearningModeActive}, Progress bar: {_progressBar != null}");
                return;
            }

            // Clamp progress to 0-1 range
            progress = Mathf.Clamp01(progress);
            _progressBar.value = progress;

            // Update progress text if available
            if (_progressText != null)
            {
                _progressText.text = $"Learning Progress: {(progress * 100):F0}%";
            }

            // Check for milestones
            CheckLearningMilestones(progress);

            LogDebug($"External learning progress updated: {(progress * 100):F1}%");
        }

        private void CheckLearningMilestones(float progress)
        {
            if (progress >= 0.5f && _completedTutorialSteps == Mathf.FloorToInt(_totalTutorialSteps * 0.5f))
            {
                ShowAchievement("Halfway There! Keep learning stealth techniques.");
            }
            
            if (progress >= 1.0f)
            {
                ShowAchievement("Stealth Expert! You've mastered the basics. (70% faster learning achieved!)");
            }
        }

        private void ShowAchievement(string message)
        {
            if (_achievementPopup == null) return;

            if (_achievementText != null)
                _achievementText.text = message;

            _achievementPopup.SetActive(true);
            
            // Auto-hide after delay
            StartCoroutine(HideAchievementAfterDelay());
            
            LogDebug($"Achievement unlocked: {message}");
        }

        private IEnumerator HideAchievementAfterDelay()
        {
            yield return new WaitForSeconds(_config.AchievementDisplayDuration);
            
            if (_achievementPopup != null)
                _achievementPopup.SetActive(false);
        }

        #endregion

        #region Input Handling

        private void HandleInput()
        {
            // Tutorial navigation
            if (_isTutorialActive && Input.GetKeyDown(KeyCode.Return))
            {
                ShowNextTutorialStep();
            }

            // Quick tutorial skip
            if (_isTutorialActive && Input.GetKeyDown(KeyCode.Escape))
            {
                CompleteTutorial();
            }

            // Toggle UI visibility
            if (Input.GetKeyDown(KeyCode.H))
            {
                ToggleUIVisibility();
            }
        }

        private void ToggleUIVisibility()
        {
            if (_mainUIGroup != null)
            {
                _mainUIGroup.alpha = _mainUIGroup.alpha > 0.5f ? 0f : 1f;
            }
        }

        #endregion

        #region Audio System

        private void PlayStealthStateAudio(StealthState stealthState)
        {
            if (_uiAudioSource == null) return;

            AudioClip clipToPlay = stealthState switch
            {
                StealthState.Hidden or StealthState.Concealed => _hiddenSound,
                StealthState.Detected or StealthState.Compromised => _detectionSound,
                _ => null
            };

            if (clipToPlay != null)
            {
                _uiAudioSource.PlayOneShot(clipToPlay);
            }
        }

        private void PlayAchievementSound()
        {
            if (_uiAudioSource != null && _achievementSound != null)
            {
                _uiAudioSource.PlayOneShot(_achievementSound);
            }
        }

        #endregion

        #region Utility Methods

        private string GetStealthStateDisplayText(StealthState state)
        {
            return state switch
            {
                StealthState.Hidden => "Hidden",
                StealthState.Concealed => "Concealed",
                StealthState.Visible => "Visible", 
                StealthState.Detected => "Detected!",
                StealthState.Compromised => "Compromised!",
                _ => "Unknown"
            };
        }

        private Color GetStealthStateColor(StealthState state)
        {
            return state switch
            {
                StealthState.Hidden => _config.HiddenColor,
                StealthState.Concealed => _config.ConcealedColor,
                StealthState.Visible => _config.VisibleColor,
                StealthState.Detected => _config.DetectedColor,
                StealthState.Compromised => Color.red, // TODO: Add CompromisedColor to StealthUIConfig
                _ => Color.white
            };
        }

        private string GetAlertLevelDisplayText(AIAlertLevel level)
        {
            return level switch
            {
                AIAlertLevel.Unaware => "Unaware",
                AIAlertLevel.Suspicious => "Suspicious",
                AIAlertLevel.Investigating => "Investigating",
                AIAlertLevel.Alert => "Alert!",
                _ => "Unknown"
            };
        }

        private Color GetAlertLevelColor(AIAlertLevel level)
        {
            return level switch
            {
                AIAlertLevel.Unaware => Color.green,
                AIAlertLevel.Suspicious => Color.yellow,
                AIAlertLevel.Investigating => new Color(1f, 0.5f, 0f), // Orange
                AIAlertLevel.Alert => Color.red,
                _ => Color.white
            };
        }

        #endregion

        #region Event Handlers

        public void OnStealthDetectionEvent(asterivo.Unity60.Features.Templates.Stealth.Events.StealthDetectionData detectionData)
        {
            // Handle detection event updates
            // TODO: Add IsConcealment property to StealthDetectionData
            if (false)
            {
                // Player found concealment
                UpdateEnvironmentalIndicators();
            }
            else
            {
                // Player was detected
                if (_detectionIndicator != null)
                {
                    StartCoroutine(FlashDetectionIndicator());
                }
            }
        }

        private IEnumerator FlashDetectionIndicator()
        {
            var originalColor = _detectionIndicator.color;
            
            for (int i = 0; i < 3; i++)
            {
                _detectionIndicator.color = Color.red;
                yield return new WaitForSeconds(0.2f);
                _detectionIndicator.color = originalColor;
                yield return new WaitForSeconds(0.2f);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Enable or disable Learn & Grow tutorial mode
        /// </summary>
        public void SetLearningModeActive(bool active)
        {
            _isLearningModeActive = active;
            
            if (_progressPanel != null)
                _progressPanel.SetActive(active);
                
            if (active && !_isTutorialActive)
            {
                PrepareTutorialContent();
                // TODO: Add AutoStartTutorial property to StealthUIConfig
                if (true)
                    StartTutorial();
            }
        }

        /// <summary>
        /// Manually trigger a tutorial step (for external systems)
        /// </summary>
        public void TriggerTutorialStep(string stepText)
        {
            _tutorialQueue.Enqueue(stepText);
            if (!_isTutorialActive)
            {
                StartTutorial();
            }
        }

        /// <summary>
        /// Force update all UI elements (useful for runtime configuration changes)
        /// </summary>
        public void ForceUIUpdate()
        {
            UpdateStealthUI();
            UpdateEnvironmentalIndicators();
            UpdateLearningProgress();
        }

        #endregion

        #region Mission UI Methods

        /// <summary>
        /// Show mission objectives in the UI
        /// </summary>
        public void ShowMissionObjectives(List<string> objectives)
        {
            LogDebug($"Showing {objectives?.Count ?? 0} mission objectives");
            // TODO: Implement mission objectives display
        }

        /// <summary>
        /// Update the remaining time display
        /// </summary>
        public void UpdateTimeRemaining(float timeRemaining)
        {
            LogDebug($"Updating time remaining: {timeRemaining:F1}s");
            // TODO: Implement time remaining display
        }

        /// <summary>
        /// Update objective progress display
        /// </summary>
        public void UpdateObjectiveProgress(int completed, int total)
        {
            LogDebug($"Updating objective progress: {completed}/{total}");
            // TODO: Implement objective progress display
        }

        /// <summary>
        /// Show objective completed notification
        /// </summary>
        public void ShowObjectiveCompleted(string objective)
        {
            LogDebug($"Showing objective completed: {objective}");
            // TODO: Implement objective completed notification
        }

        /// <summary>
        /// Show learning tip for Learn & Grow value realization
        /// Part of 70% learning cost reduction system
        /// </summary>
        /// <param name="tip">Learning tip message to display</param>
        public void ShowLearningTip(string tip)
        {
            LogDebug($"Showing learning tip: {tip}");

            // Learn & Grow価値実現: インタラクティブチュートリアル
            // TODO: 将来的にはUI要素の実装を追加
            // - Learning tip overlay
            // - Progressive hint system
            // - Interactive highlights

            // 現在は debug ログで学習支援を提供
            Debug.Log($"<color=cyan>[Learn & Grow]</color> Learning Tip: {tip}");
        }

        /// <summary>
        /// Show time warning notification for mission time limits
        /// </summary>
        /// <param name="timeRemaining">Remaining time in seconds</param>
        public void ShowTimeWarning(float timeRemaining)
        {
            LogDebug($"Showing time warning: {timeRemaining:F1}s remaining");

            // TODO: Implement time warning UI
            // - Warning overlay display
            // - Countdown animation
            // - Audio warning sound

            // 現在はログでタイムワーニングを表示
            Debug.Log($"<color=orange>[Time Warning]</color> {timeRemaining:F1} seconds remaining!");
        }

        /// <summary>
        /// Show game end screen with results
        /// </summary>
        /// <param name="success">Whether the mission was successful</param>
        /// <param name="message">End game message to display</param>
        public void ShowGameEndScreen(bool success, string message = "")
        {
            LogDebug($"Showing game end screen: Success={success}, Message={message}");

            // TODO: Implement game end screen UI
            // - Results panel
            // - Success/failure animation
            // - Statistics display
            // - Restart/continue options

            // 現在はログでゲーム終了を表示
            string resultText = success ? "SUCCESS" : "FAILURE";
            Debug.Log($"<color={(success ? "green" : "red")}>[Game End - {resultText}]</color> {message}");
        }

        /// <summary>
        /// 設定の適用
        /// </summary>
        public void ApplyConfiguration(StealthUIConfig config)
        {
            if (config != null)
            {
                _config = config;
                ForceUIUpdate();
                UnityEngine.Debug.Log($"[StealthUIManager] Configuration applied: {config.name}");
            }
        }

        /// <summary>
        /// Handle detection level changes from external systems
        /// </summary>
        /// <param name="detectionLevel">New detection level (0.0 to 1.0)</param>
        public void OnDetectionLevelChanged(float detectionLevel)
        {
            LogDebug($"Detection level changed: {detectionLevel:F2}");

            // Convert detection level to stealth state
            StealthState newState = detectionLevel switch
            {
                <= 0.1f => StealthState.Hidden,
                <= 0.3f => StealthState.Concealed,
                <= 0.7f => StealthState.Visible,
                <= 0.9f => StealthState.Detected,
                _ => StealthState.Compromised
            };

            // Update the UI state
            UpdateStealthStateDisplay(newState);
            _currentStealthState = newState;
        }

        #endregion

        #region Debug Support

        private void LogDebug(string message)
        {
            // TODO: Add EnableDebugLogs property to StealthUIConfig
            if (_config != null && false)
            {
                Debug.Log($"[StealthUIManager] {message}");
            }
        }

        #endregion
    }
}
