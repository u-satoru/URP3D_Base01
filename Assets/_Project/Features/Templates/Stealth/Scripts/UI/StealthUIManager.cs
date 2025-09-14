using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Features.Player.Scripts;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Stealth
{
    /// <summary>
    /// ステルス特化UIマネージャー
    /// 検知インジケーター、警戒レベル表示、ノイズ・影レベル可視化を統合管理
    /// </summary>
    public class StealthUIManager : MonoBehaviour
    {
        #region UI Configuration

        [TabGroup("Stealth UI", "Basic Settings")]
        [Title("Stealth UI Settings", "ステルスゲーム特化UI統合管理システム", TitleAlignments.Centered)]
        [SerializeField] private bool enableStealthUI = true;
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool autoUpdateUI = true;

        [TabGroup("Stealth UI", "Detection Indicators")]
        [Header("Detection Indicator Settings")]
        [SerializeField] private Canvas detectionCanvas;
        [SerializeField] private GameObject detectionIndicatorPrefab;
        [SerializeField] private Transform indicatorParent;
        [SerializeField] private float indicatorLifetime = 3f;
        [SerializeField] private float indicatorUpdateRate = 0.1f;
        [SerializeField] private LayerMask npcLayerMask = -1;
        [SerializeField] private float maxDetectionRange = 50f;

        [TabGroup("Stealth UI", "Alert Level Display")]
        [Header("Alert Level UI")]
        [SerializeField] private Image alertLevelBar;
        [SerializeField] private TextMeshProUGUI alertLevelText;
        [SerializeField] private Image alertLevelIcon;
        [SerializeField] private Color[] alertLevelColors = new Color[]
        {
            Color.green,    // None
            Color.yellow,   // Low
            new Color(1f, 0.5f, 0f), // Medium
            Color.red,      // High
            new Color(0.5f, 0f, 0f)  // Combat
        };

        [TabGroup("Stealth UI", "Stealth Status")]
        [Header("Stealth Status Display")]
        [SerializeField] private Image stealthLevelBar;
        [SerializeField] private TextMeshProUGUI stealthStatusText;
        [SerializeField] private Image shadowCoverageBar;
        [SerializeField] private TextMeshProUGUI shadowStatusText;
        [SerializeField] private Image noiseLevelBar;
        [SerializeField] private TextMeshProUGUI noiseStatusText;

        [TabGroup("Stealth UI", "Visual Effects")]
        [Header("Visual Effect Settings")]
        [SerializeField] private AnimationCurve pulseAnimation = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private bool enableScreenEffects = true;
        [SerializeField] private Image screenBorderEffect;
        [SerializeField] private Color detectedBorderColor = Color.red;
        [SerializeField] private Color hiddenBorderColor = Color.blue;

        #endregion

        #region Runtime State

        [TabGroup("Stealth UI", "Runtime")]
        [Header("Current State")]
        [SerializeField, ReadOnly] private AlertLevel currentAlertLevel = AlertLevel.None;
        [SerializeField, ReadOnly] private float currentStealthLevel = 1f;
        [SerializeField, ReadOnly] private float currentShadowCoverage = 0f;
        [SerializeField, ReadOnly] private float currentNoiseLevel = 0f;
        [SerializeField, ReadOnly] private int activeDetectionIndicators = 0;
        [SerializeField, ReadOnly] private bool isPlayerDetected = false;

        private Dictionary<NPCVisualSensor, DetectionIndicatorUI> activeIndicators = new();
        private List<NPCVisualSensor> nearbyNPCs = new List<NPCVisualSensor>();
        private Camera mainCamera;
        private PlayerStealthController playerStealthController;
        private StealthCameraController stealthCameraController;
        private IStealthAudioService stealthAudioService;

        private float lastUIUpdateTime;
        private float pulseTimer;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeStealthUI();
        }

        private void Start()
        {
            if (enableStealthUI)
            {
                SetupStealthUISystem();
            }
        }

        private void Update()
        {
            if (enableStealthUI && autoUpdateUI)
            {
                UpdateStealthUI();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// ステルスUIシステムの初期化
        /// </summary>
        private void InitializeStealthUI()
        {
            LogDebug("[StealthUI] Initializing Stealth UI System...");

            try
            {
                // カメラとプレイヤー参照の取得
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    mainCamera = FindFirstObjectByType<Camera>();
                }

                playerStealthController = FindFirstObjectByType<PlayerStealthController>();
                stealthCameraController = FindFirstObjectByType<StealthCameraController>();

                // サービスの取得
                if (FeatureFlags.UseServiceLocator)
                {
                    stealthAudioService = ServiceLocator.GetService<IStealthAudioService>();
                }

                // UIキャンバス設定
                InitializeUICanvases();

                // 初期UI状態設定
                InitializeUIState();

                LogDebug("[StealthUI] ✅ Stealth UI initialization completed");
            }
            catch (System.Exception ex)
            {
                LogError($"[StealthUI] ❌ Initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// UIキャンバスの初期化
        /// </summary>
        private void InitializeUICanvases()
        {
            // 検知インジケーター用キャンバスの自動作成
            if (detectionCanvas == null)
            {
                GameObject canvasGO = new GameObject("StealthDetectionCanvas");
                detectionCanvas = canvasGO.AddComponent<Canvas>();
                detectionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                detectionCanvas.sortingOrder = 100; // 最前面表示

                var canvasScaler = canvasGO.AddComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920, 1080);

                canvasGO.AddComponent<GraphicRaycaster>();
            }

            // インジケーター親オブジェクトの設定
            if (indicatorParent == null && detectionCanvas != null)
            {
                GameObject indicatorParentGO = new GameObject("DetectionIndicators");
                indicatorParentGO.transform.SetParent(detectionCanvas.transform, false);
                indicatorParent = indicatorParentGO.transform;
            }
        }

        /// <summary>
        /// UI初期状態の設定
        /// </summary>
        private void InitializeUIState()
        {
            // 警戒レベル表示の初期化
            UpdateAlertLevelUI(AlertLevel.None);

            // ステルス状態表示の初期化
            UpdateStealthStatusUI(1f);

            // 影カバー表示の初期化
            UpdateShadowCoverageUI(0f);

            // ノイズレベル表示の初期化
            UpdateNoiseLevelUI(0f);

            // 画面効果の初期化
            if (screenBorderEffect != null)
            {
                screenBorderEffect.color = Color.clear;
            }
        }

        /// <summary>
        /// ステルスUIシステムのセットアップ
        /// </summary>
        private void SetupStealthUISystem()
        {
            LogDebug("[StealthUI] Setting up Stealth UI System...");

            // イベントリスナーの設定
            SetupEventListeners();

            // 検知インジケータープレハブの検証
            ValidateIndicatorPrefab();

            LogDebug("[StealthUI] ✅ Stealth UI setup completed");
        }

        /// <summary>
        /// イベントリスナーの設定
        /// </summary>
        private void SetupEventListeners()
        {
            // プレイヤー状態変更イベントのリスン
            // ステルス状態、検知状態の変更を監視
            // 実装は既存のイベントシステムに依存
        }

        /// <summary>
        /// インジケータープレハブの検証
        /// </summary>
        private void ValidateIndicatorPrefab()
        {
            if (detectionIndicatorPrefab == null)
            {
                LogDebug("[StealthUI] Creating default detection indicator prefab...");
                CreateDefaultIndicatorPrefab();
            }
        }

        /// <summary>
        /// デフォルトインジケータープレハブの作成
        /// </summary>
        private void CreateDefaultIndicatorPrefab()
        {
            // デフォルトのインジケーターUI要素を動的に作成
            GameObject prefab = new GameObject("DefaultDetectionIndicator");

            // Canvas Group for fade effects
            CanvasGroup canvasGroup = prefab.AddComponent<CanvasGroup>();

            // Background circle
            GameObject background = new GameObject("Background");
            background.transform.SetParent(prefab.transform, false);
            Image bgImage = background.AddComponent<Image>();
            bgImage.sprite = CreateCircleSprite();
            bgImage.color = new Color(1f, 0f, 0f, 0.3f);

            RectTransform bgRect = background.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(60, 60);

            // Alert icon
            GameObject icon = new GameObject("AlertIcon");
            icon.transform.SetParent(prefab.transform, false);
            Image iconImage = icon.AddComponent<Image>();
            iconImage.sprite = CreateAlertIconSprite();
            iconImage.color = Color.white;

            RectTransform iconRect = icon.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(40, 40);

            // DetectionIndicatorUI コンポーネント追加
            DetectionIndicatorUI indicatorUI = prefab.AddComponent<DetectionIndicatorUI>();
            indicatorUI.Initialize(bgImage, iconImage, canvasGroup);

            detectionIndicatorPrefab = prefab;
        }

        /// <summary>
        /// 円形スプライトの作成
        /// </summary>
        private Sprite CreateCircleSprite()
        {
            Texture2D texture = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];

            Vector2 center = new Vector2(32, 32);
            float radius = 30f;

            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = distance <= radius ? 1f : 0f;
                    pixels[y * 64 + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// 警告アイコンスプライトの作成
        /// </summary>
        private Sprite CreateAlertIconSprite()
        {
            // シンプルな三角形の警告アイコンを作成
            return CreateCircleSprite(); // 簡単のため円を使用
        }

        #endregion

        #region UI Update Management

        /// <summary>
        /// ステルスUIの更新
        /// </summary>
        private void UpdateStealthUI()
        {
            if (Time.time - lastUIUpdateTime < indicatorUpdateRate)
                return;

            lastUIUpdateTime = Time.time;

            // 検知インジケーターの更新
            UpdateDetectionIndicators();

            // プレイヤーステルス状態の更新
            UpdatePlayerStealthStatus();

            // 視覚効果の更新
            UpdateVisualEffects();

            // パルスアニメーションの更新
            pulseTimer += Time.deltaTime * pulseSpeed;
            if (pulseTimer > 1f) pulseTimer = 0f;
        }

        /// <summary>
        /// 検知インジケーターの更新
        /// </summary>
        private void UpdateDetectionIndicators()
        {
            if (mainCamera == null || playerStealthController == null) return;

            // 範囲内NPCの検索
            FindNearbyNPCs();

            // 既存インジケーターの更新
            UpdateExistingIndicators();

            // 新規インジケーターの作成
            CreateNewIndicators();

            // 期限切れインジケーターの削除
            RemoveExpiredIndicators();
        }

        /// <summary>
        /// 近くのNPCを検索
        /// </summary>
        private void FindNearbyNPCs()
        {
            nearbyNPCs.Clear();

            Collider[] colliders = Physics.OverlapSphere(
                playerStealthController.transform.position,
                maxDetectionRange,
                npcLayerMask
            );

            foreach (Collider col in colliders)
            {
                NPCVisualSensor sensor = col.GetComponent<NPCVisualSensor>();
                if (sensor != null && sensor.gameObject.activeInHierarchy)
                {
                    nearbyNPCs.Add(sensor);
                }
            }
        }

        /// <summary>
        /// 既存インジケーターの更新
        /// </summary>
        private void UpdateExistingIndicators()
        {
            var indicatorsToRemove = new List<NPCVisualSensor>();

            foreach (var kvp in activeIndicators)
            {
                NPCVisualSensor sensor = kvp.Key;
                DetectionIndicatorUI indicator = kvp.Value;

                if (sensor == null || !sensor.gameObject.activeInHierarchy || !nearbyNPCs.Contains(sensor))
                {
                    // NPCが無効または範囲外
                    indicatorsToRemove.Add(sensor);
                    continue;
                }

                // インジケーターの位置とスタイル更新
                UpdateIndicatorDisplay(sensor, indicator);
            }

            // 削除対象のインジケーターを処理
            foreach (NPCVisualSensor sensor in indicatorsToRemove)
            {
                RemoveIndicator(sensor);
            }
        }

        /// <summary>
        /// インジケーター表示の更新
        /// </summary>
        private void UpdateIndicatorDisplay(NPCVisualSensor sensor, DetectionIndicatorUI indicator)
        {
            // 画面位置の計算
            Vector3 worldPos = sensor.transform.position + Vector3.up * 2f; // NPCの頭上
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

            if (screenPos.z > 0) // カメラの前方
            {
                indicator.SetScreenPosition(screenPos);
                indicator.SetAlertLevel(sensor.AlertLevel);
                indicator.SetVisibility(true);
            }
            else
            {
                indicator.SetVisibility(false);
            }
        }

        /// <summary>
        /// 新規インジケーターの作成
        /// </summary>
        private void CreateNewIndicators()
        {
            foreach (NPCVisualSensor sensor in nearbyNPCs)
            {
                if (!activeIndicators.ContainsKey(sensor))
                {
                    CreateIndicator(sensor);
                }
            }
        }

        /// <summary>
        /// 個別インジケーターの作成
        /// </summary>
        private void CreateIndicator(NPCVisualSensor sensor)
        {
            if (detectionIndicatorPrefab == null || indicatorParent == null) return;

            GameObject indicatorGO = Instantiate(detectionIndicatorPrefab, indicatorParent);
            DetectionIndicatorUI indicator = indicatorGO.GetComponent<DetectionIndicatorUI>();

            if (indicator == null)
            {
                indicator = indicatorGO.AddComponent<DetectionIndicatorUI>();
                indicator.InitializeFromGameObject(indicatorGO);
            }

            indicator.Initialize(sensor, indicatorLifetime);
            activeIndicators[sensor] = indicator;

            LogDebug($"[StealthUI] Created detection indicator for {sensor.name}");
        }

        /// <summary>
        /// インジケーターの削除
        /// </summary>
        private void RemoveIndicator(NPCVisualSensor sensor)
        {
            if (activeIndicators.TryGetValue(sensor, out DetectionIndicatorUI indicator))
            {
                if (indicator != null && indicator.gameObject != null)
                {
                    Destroy(indicator.gameObject);
                }

                activeIndicators.Remove(sensor);
                LogDebug($"[StealthUI] Removed detection indicator for {sensor?.name ?? "null"}");
            }
        }

        /// <summary>
        /// 期限切れインジケーターの削除
        /// </summary>
        private void RemoveExpiredIndicators()
        {
            var expiredSensors = new List<NPCVisualSensor>();

            foreach (var kvp in activeIndicators)
            {
                DetectionIndicatorUI indicator = kvp.Value;
                if (indicator != null && indicator.IsExpired)
                {
                    expiredSensors.Add(kvp.Key);
                }
            }

            foreach (NPCVisualSensor sensor in expiredSensors)
            {
                RemoveIndicator(sensor);
            }
        }

        #endregion

        #region Player Status UI

        /// <summary>
        /// プレイヤーステルス状態の更新
        /// </summary>
        private void UpdatePlayerStealthStatus()
        {
            if (playerStealthController == null) return;

            // ステルス状態の取得と更新
            UpdateStealthLevel();
            UpdateShadowCoverage();
            UpdateNoiseLevel();
            UpdateOverallAlertLevel();
        }

        /// <summary>
        /// ステルスレベルの更新
        /// </summary>
        private void UpdateStealthLevel()
        {
            // StealthMovementControllerからステルスレベル取得
            var stealthMovement = playerStealthController.GetComponent<StealthMovementController>();
            if (stealthMovement != null)
            {
                currentStealthLevel = stealthMovement.CurrentStealthLevel;
                UpdateStealthStatusUI(currentStealthLevel);
            }
        }

        /// <summary>
        /// 影カバレッジの更新
        /// </summary>
        private void UpdateShadowCoverage()
        {
            // StealthCameraControllerから影状態取得
            if (stealthCameraController != null)
            {
                bool inShadow = stealthCameraController.IsPlayerInShadow();
                currentShadowCoverage = inShadow ? 1f : 0f;
                UpdateShadowCoverageUI(currentShadowCoverage);
            }
        }

        /// <summary>
        /// ノイズレベルの更新
        /// </summary>
        private void UpdateNoiseLevel()
        {
            var stealthMovement = playerStealthController.GetComponent<StealthMovementController>();
            if (stealthMovement != null)
            {
                currentNoiseLevel = stealthMovement.CurrentNoise;
                UpdateNoiseLevelUI(currentNoiseLevel);
            }
        }

        /// <summary>
        /// 全体警戒レベルの更新
        /// </summary>
        private void UpdateOverallAlertLevel()
        {
            AlertLevel maxAlertLevel = AlertLevel.None;

            // 近くのNPCの最高警戒レベルを取得
            foreach (NPCVisualSensor sensor in nearbyNPCs)
            {
                if (sensor.AlertLevel > maxAlertLevel)
                {
                    maxAlertLevel = sensor.AlertLevel;
                }
            }

            if (maxAlertLevel != currentAlertLevel)
            {
                currentAlertLevel = maxAlertLevel;
                UpdateAlertLevelUI(currentAlertLevel);
                isPlayerDetected = currentAlertLevel >= AlertLevel.Medium;
            }
        }

        #endregion

        #region UI Display Updates

        /// <summary>
        /// 警戒レベルUIの更新
        /// </summary>
        private void UpdateAlertLevelUI(AlertLevel alertLevel)
        {
            if (alertLevelBar != null)
            {
                float fillValue = (float)alertLevel / (float)AlertLevel.Combat;
                alertLevelBar.fillAmount = fillValue;
                alertLevelBar.color = alertLevelColors[(int)alertLevel];
            }

            if (alertLevelText != null)
            {
                alertLevelText.text = GetAlertLevelText(alertLevel);
                alertLevelText.color = alertLevelColors[(int)alertLevel];
            }

            if (alertLevelIcon != null)
            {
                alertLevelIcon.color = alertLevelColors[(int)alertLevel];
            }
        }

        /// <summary>
        /// ステルス状態UIの更新
        /// </summary>
        private void UpdateStealthStatusUI(float stealthLevel)
        {
            if (stealthLevelBar != null)
            {
                stealthLevelBar.fillAmount = stealthLevel;
                stealthLevelBar.color = Color.Lerp(Color.red, Color.green, stealthLevel);
            }

            if (stealthStatusText != null)
            {
                stealthStatusText.text = $"Stealth: {(stealthLevel * 100):F0}%";
                stealthStatusText.color = Color.Lerp(Color.red, Color.green, stealthLevel);
            }
        }

        /// <summary>
        /// 影カバーUIの更新
        /// </summary>
        private void UpdateShadowCoverageUI(float coverage)
        {
            if (shadowCoverageBar != null)
            {
                shadowCoverageBar.fillAmount = coverage;
                shadowCoverageBar.color = coverage > 0.5f ? Color.blue : Color.gray;
            }

            if (shadowStatusText != null)
            {
                shadowStatusText.text = coverage > 0.5f ? "Hidden" : "Exposed";
                shadowStatusText.color = coverage > 0.5f ? Color.blue : Color.gray;
            }
        }

        /// <summary>
        /// ノイズレベルUIの更新
        /// </summary>
        private void UpdateNoiseLevelUI(float noiseLevel)
        {
            if (noiseLevelBar != null)
            {
                noiseLevelBar.fillAmount = noiseLevel;
                noiseLevelBar.color = Color.Lerp(Color.green, Color.red, noiseLevel);
            }

            if (noiseStatusText != null)
            {
                noiseStatusText.text = $"Noise: {(noiseLevel * 100):F0}%";
                noiseStatusText.color = Color.Lerp(Color.green, Color.red, noiseLevel);
            }
        }

        /// <summary>
        /// 視覚効果の更新
        /// </summary>
        private void UpdateVisualEffects()
        {
            if (!enableScreenEffects) return;

            // 画面境界効果
            UpdateScreenBorderEffect();

            // パルス効果
            UpdatePulseEffect();
        }

        /// <summary>
        /// 画面境界効果の更新
        /// </summary>
        private void UpdateScreenBorderEffect()
        {
            if (screenBorderEffect == null) return;

            Color targetColor = Color.clear;

            if (isPlayerDetected)
            {
                targetColor = detectedBorderColor;
                targetColor.a = Mathf.Lerp(0.1f, 0.3f, (float)currentAlertLevel / (float)AlertLevel.Combat);
            }
            else if (currentShadowCoverage > 0.5f)
            {
                targetColor = hiddenBorderColor;
                targetColor.a = 0.1f;
            }

            screenBorderEffect.color = Color.Lerp(screenBorderEffect.color, targetColor, Time.deltaTime * 2f);
        }

        /// <summary>
        /// パルス効果の更新
        /// </summary>
        private void UpdatePulseEffect()
        {
            if (currentAlertLevel >= AlertLevel.Medium)
            {
                float pulseValue = pulseAnimation.Evaluate(pulseTimer);

                // 警戒レベル表示にパルス効果適用
                if (alertLevelBar != null)
                {
                    Color currentColor = alertLevelBar.color;
                    currentColor.a = Mathf.Lerp(0.5f, 1f, pulseValue);
                    alertLevelBar.color = currentColor;
                }
            }
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// 警戒レベルテキストの取得
        /// </summary>
        private string GetAlertLevelText(AlertLevel alertLevel)
        {
            return alertLevel switch
            {
                AlertLevel.None => "Safe",
                AlertLevel.Low => "Suspicious",
                AlertLevel.Medium => "Searching",
                AlertLevel.High => "Alert",
                AlertLevel.Combat => "Combat",
                _ => "Unknown"
            };
        }

        #endregion

        #region Public API

        /// <summary>
        /// 検知インジケーターの手動表示
        /// </summary>
        public void ShowDetectionIndicator(Transform target, AlertLevel alertLevel, float duration = -1f)
        {
            NPCVisualSensor sensor = target.GetComponent<NPCVisualSensor>();
            if (sensor != null)
            {
                if (!activeIndicators.ContainsKey(sensor))
                {
                    CreateIndicator(sensor);
                }

                if (duration > 0f)
                {
                    activeIndicators[sensor].SetLifetime(duration);
                }
            }
        }

        /// <summary>
        /// 全インジケーターの非表示
        /// </summary>
        public void HideAllIndicators()
        {
            var sensorsToRemove = new List<NPCVisualSensor>(activeIndicators.Keys);
            foreach (NPCVisualSensor sensor in sensorsToRemove)
            {
                RemoveIndicator(sensor);
            }
        }

        /// <summary>
        /// UI表示の切り替え
        /// </summary>
        public void ToggleUIVisibility(bool visible)
        {
            enableStealthUI = visible;

            if (detectionCanvas != null)
            {
                detectionCanvas.gameObject.SetActive(visible);
            }
        }

        /// <summary>
        /// 現在の検知状態取得
        /// </summary>
        public bool IsPlayerCurrentlyDetected()
        {
            return isPlayerDetected;
        }

        /// <summary>
        /// アクティブインジケーター数取得
        /// </summary>
        public int GetActiveIndicatorCount()
        {
            return activeIndicators.Count;
        }

        #endregion

        #region Editor Support

        [TabGroup("Stealth UI", "Actions")]
        [Button("Test Detection Alert")]
        public void TestDetectionAlert()
        {
            UpdateAlertLevelUI(AlertLevel.High);
            LogDebug("[StealthUI] Testing high alert level display");
        }

        [Button("Test Shadow Cover")]
        public void TestShadowCover()
        {
            UpdateShadowCoverageUI(1f);
            LogDebug("[StealthUI] Testing shadow coverage display");
        }

        [Button("Test Noise Level")]
        public void TestNoiseLevel()
        {
            UpdateNoiseLevelUI(0.8f);
            LogDebug("[StealthUI] Testing noise level display");
        }

        [Button("Hide All Indicators")]
        public void EditorHideAllIndicators()
        {
            HideAllIndicators();
        }

        [Button("Validate UI Setup")]
        public void ValidateUISetup()
        {
            LogDebug("=== Stealth UI Validation ===");
            LogDebug($"Detection Canvas: {(detectionCanvas != null ? "✅ Found" : "❌ Missing")}");
            LogDebug($"Indicator Prefab: {(detectionIndicatorPrefab != null ? "✅ Found" : "❌ Missing")}");
            LogDebug($"Alert Level Bar: {(alertLevelBar != null ? "✅ Found" : "❌ Missing")}");
            LogDebug($"Stealth Level Bar: {(stealthLevelBar != null ? "✅ Found" : "❌ Missing")}");
            LogDebug($"Shadow Coverage Bar: {(shadowCoverageBar != null ? "✅ Found" : "❌ Missing")}");
            LogDebug($"Noise Level Bar: {(noiseLevelBar != null ? "✅ Found" : "❌ Missing")}");
            LogDebug($"Main Camera: {(mainCamera != null ? "✅ Found" : "❌ Missing")}");
            LogDebug($"Player Controller: {(playerStealthController != null ? "✅ Found" : "❌ Missing")}");
            LogDebug($"Active Indicators: {activeIndicators.Count}");
            LogDebug($"Current Alert Level: {currentAlertLevel}");
            LogDebug($"Player Detected: {isPlayerDetected}");
            LogDebug("=== Validation Complete ===");
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (playerStealthController == null) return;

            // 検知範囲の可視化
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(playerStealthController.transform.position, maxDetectionRange);

            // アクティブなインジケーターの位置表示
            Gizmos.color = Color.red;
            foreach (var kvp in activeIndicators)
            {
                if (kvp.Key != null)
                {
                    Vector3 npcPos = kvp.Key.transform.position;
                    Gizmos.DrawLine(playerStealthController.transform.position, npcPos);
                    Gizmos.DrawWireCube(npcPos + Vector3.up * 3f, Vector3.one);
                }
            }
        }
#endif

        #endregion

        #region Gameplay UI Integration

        /// <summary>
        /// ミッション目標リストの表示
        /// </summary>
        public void ShowMissionObjectives(System.Collections.Generic.List<StealthMissionObjective> objectives)
        {
            if (objectives == null) return;

            LogDebug("[StealthUIManager] Displaying mission objectives...");

            // Create or update mission objectives UI
            if (objectivesPanel == null)
            {
                CreateObjectivesPanel();
            }

            // Clear existing objective displays
            ClearObjectiveDisplays();

            // Create displays for each objective
            foreach (var objective in objectives)
            {
                CreateObjectiveDisplay(objective);
            }

            LogDebug($"[StealthUIManager] ✅ {objectives.Count} mission objectives displayed");
        }

        /// <summary>
        /// 残り時間の更新表示
        /// </summary>
        public void UpdateTimeRemaining(float timeRemaining)
        {
            if (timeRemainingText == null) return;

            int minutes = (int)(timeRemaining / 60);
            int seconds = (int)(timeRemaining % 60);
            string timeString = $"{minutes:D2}:{seconds:D2}";

            timeRemainingText.text = timeString;

            // Color coding based on remaining time
            if (timeRemaining < 60f) // Less than 1 minute
            {
                timeRemainingText.color = Color.red;
            }
            else if (timeRemaining < 300f) // Less than 5 minutes
            {
                timeRemainingText.color = Color.yellow;
            }
            else
            {
                timeRemainingText.color = Color.white;
            }
        }

        /// <summary>
        /// 目標進捗の更新表示
        /// </summary>
        public void UpdateObjectiveProgress(int completed, int total)
        {
            if (objectiveProgressText == null) return;

            objectiveProgressText.text = $"Objectives: {completed}/{total}";

            // Progress bar update if available
            if (objectiveProgressBar != null)
            {
                float progress = total > 0 ? (float)completed / total : 0f;
                objectiveProgressBar.value = progress;
            }

            LogDebug($"[StealthUIManager] Objective progress updated: {completed}/{total}");
        }

        /// <summary>
        /// 目標完了の通知表示
        /// </summary>
        public void ShowObjectiveCompleted(StealthMissionObjective objective)
        {
            if (objective == null) return;

            LogDebug($"[StealthUIManager] Showing objective completed: {objective.Title}");

            // Create completion notification
            StartCoroutine(ShowObjectiveCompletionCoroutine(objective));

            // Update the objective display to show completion
            UpdateObjectiveDisplay(objective);
        }

        /// <summary>
        /// 時間警告の表示
        /// </summary>
        public void ShowTimeWarning(string warningMessage)
        {
            LogDebug($"[StealthUIManager] Showing time warning: {warningMessage}");

            // Create warning notification
            StartCoroutine(ShowWarningNotificationCoroutine(warningMessage));
        }

        /// <summary>
        /// グローバル警戒レベルの更新表示
        /// </summary>
        public void UpdateGlobalAlertLevel(GlobalAlertLevel alertLevel)
        {
            if (globalAlertLevelText == null) return;

            string alertText = alertLevel switch
            {
                GlobalAlertLevel.Normal => "Normal",
                GlobalAlertLevel.Suspicious => "Suspicious",
                GlobalAlertLevel.Heightened => "Heightened",
                GlobalAlertLevel.FullAlert => "Full Alert",
                _ => "Unknown"
            };

            globalAlertLevelText.text = $"Alert: {alertText}";

            // Color coding based on alert level
            globalAlertLevelText.color = alertLevel switch
            {
                GlobalAlertLevel.Normal => Color.green,
                GlobalAlertLevel.Suspicious => Color.yellow,
                GlobalAlertLevel.Heightened => new Color(1f, 0.5f, 0f), // Orange
                GlobalAlertLevel.FullAlert => Color.red,
                _ => Color.white
            };

            LogDebug($"[StealthUIManager] Global alert level updated: {alertLevel}");
        }

        /// <summary>
        /// ゲーム終了画面の表示
        /// </summary>
        public void ShowGameEndScreen(bool success, string reason, int finalScore, int completedObjectives, int totalObjectives)
        {
            LogDebug($"[StealthUIManager] Showing game end screen - Success: {success}");

            // Create game end screen if it doesn't exist
            if (gameEndPanel == null)
            {
                CreateGameEndPanel();
            }

            // Update game end panel content
            if (gameEndPanel != null)
            {
                gameEndPanel.SetActive(true);

                // Update end screen texts
                if (gameEndTitleText != null)
                {
                    gameEndTitleText.text = success ? "MISSION ACCOMPLISHED" : "MISSION FAILED";
                    gameEndTitleText.color = success ? Color.green : Color.red;
                }

                if (gameEndReasonText != null)
                {
                    gameEndReasonText.text = reason;
                }

                if (gameEndScoreText != null)
                {
                    gameEndScoreText.text = $"Final Score: {finalScore}";
                }

                if (gameEndObjectivesText != null)
                {
                    gameEndObjectivesText.text = $"Objectives Completed: {completedObjectives}/{totalObjectives}";
                }

                LogDebug($"[StealthUIManager] ✅ Game end screen displayed - Score: {finalScore}");
            }
        }

        #endregion

        #region Gameplay UI Creation Methods

        /// <summary>
        /// 目標パネルの作成
        /// </summary>
        private void CreateObjectivesPanel()
        {
            if (mainCanvas == null) return;

            // Create objectives panel
            GameObject objectivesPanelGO = new GameObject("ObjectivesPanel");
            objectivesPanelGO.transform.SetParent(mainCanvas.transform, false);

            objectivesPanel = objectivesPanelGO;

            // Add RectTransform and position
            var rectTransform = objectivesPanelGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0.4f, 1);
            rectTransform.anchoredPosition = new Vector2(20, -20);
            rectTransform.sizeDelta = new Vector2(0, 300);

            // Add background image
            var image = objectivesPanelGO.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.7f);

            LogDebug("[StealthUIManager] Objectives panel created");
        }

        /// <summary>
        /// 目標表示のクリア
        /// </summary>
        private void ClearObjectiveDisplays()
        {
            if (objectivesPanel == null) return;

            // Remove existing objective displays
            for (int i = objectivesPanel.transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = objectivesPanel.transform.GetChild(i).gameObject;
                if (child.name.StartsWith("Objective_"))
                {
                    DestroyImmediate(child);
                }
            }
        }

        /// <summary>
        /// 個別目標表示の作成
        /// </summary>
        private void CreateObjectiveDisplay(StealthMissionObjective objective)
        {
            if (objectivesPanel == null || objective == null) return;

            // Create objective display
            GameObject objectiveDisplay = new GameObject($"Objective_{objective.ObjectiveID}");
            objectiveDisplay.transform.SetParent(objectivesPanel.transform, false);

            // Add RectTransform
            var rectTransform = objectiveDisplay.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.sizeDelta = new Vector2(-20, 30);

            // Position based on existing objectives
            int objectiveIndex = objectivesPanel.transform.childCount - 1;
            rectTransform.anchoredPosition = new Vector2(10, -10 - (objectiveIndex * 35));

            // Add text component
            var textComponent = objectiveDisplay.AddComponent<TextMeshProUGUI>();
            textComponent.text = $"☐ {objective.Title}";
            textComponent.fontSize = 14;
            textComponent.color = objective.IsCompleted ? Color.green : Color.white;

            // Store reference for updates
            objectiveDisplays[objective.ObjectiveID] = textComponent;
        }

        /// <summary>
        /// 目標表示の更新
        /// </summary>
        private void UpdateObjectiveDisplay(StealthMissionObjective objective)
        {
            if (objective == null) return;

            if (objectiveDisplays.TryGetValue(objective.ObjectiveID, out var textComponent))
            {
                textComponent.text = $"☑ {objective.Title}";
                textComponent.color = Color.green;
            }
        }

        /// <summary>
        /// ゲーム終了パネルの作成
        /// </summary>
        private void CreateGameEndPanel()
        {
            if (mainCanvas == null) return;

            // Create game end panel
            GameObject gameEndPanelGO = new GameObject("GameEndPanel");
            gameEndPanelGO.transform.SetParent(mainCanvas.transform, false);

            gameEndPanel = gameEndPanelGO;

            // Add RectTransform (full screen)
            var rectTransform = gameEndPanelGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            // Add semi-transparent background
            var backgroundImage = gameEndPanelGO.AddComponent<Image>();
            backgroundImage.color = new Color(0, 0, 0, 0.8f);

            // Create title text
            CreateGameEndText("GameEndTitle", "MISSION RESULT", new Vector2(0, 100), 48, ref gameEndTitleText);
            CreateGameEndText("GameEndReason", "Reason", new Vector2(0, 50), 24, ref gameEndReasonText);
            CreateGameEndText("GameEndScore", "Score: 0", new Vector2(0, 0), 32, ref gameEndScoreText);
            CreateGameEndText("GameEndObjectives", "Objectives: 0/0", new Vector2(0, -50), 24, ref gameEndObjectivesText);

            // Initially hide the panel
            gameEndPanel.SetActive(false);

            LogDebug("[StealthUIManager] Game end panel created");
        }

        /// <summary>
        /// ゲーム終了画面のテキスト作成
        /// </summary>
        private void CreateGameEndText(string name, string text, Vector2 position, float fontSize, ref TextMeshProUGUI textRef)
        {
            if (gameEndPanel == null) return;

            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(gameEndPanel.transform, false);

            var rectTransform = textGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = new Vector2(600, 60);

            var textComponent = textGO.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;

            textRef = textComponent;
        }

        /// <summary>
        /// 目標完了通知のコルーチン
        /// </summary>
        private System.Collections.IEnumerator ShowObjectiveCompletionCoroutine(StealthMissionObjective objective)
        {
            // Create temporary notification
            GameObject notification = CreateNotification($"Objective Complete: {objective.Title}", Color.green);

            yield return new WaitForSeconds(3f);

            // Fade out and destroy
            if (notification != null)
            {
                Destroy(notification);
            }
        }

        /// <summary>
        /// 警告通知のコルーチン
        /// </summary>
        private System.Collections.IEnumerator ShowWarningNotificationCoroutine(string message)
        {
            // Create temporary warning notification
            GameObject warning = CreateNotification(message, Color.yellow);

            yield return new WaitForSeconds(2f);

            // Fade out and destroy
            if (warning != null)
            {
                Destroy(warning);
            }
        }

        /// <summary>
        /// 一時通知の作成
        /// </summary>
        private GameObject CreateNotification(string message, Color color)
        {
            if (mainCanvas == null) return null;

            GameObject notification = new GameObject("Notification");
            notification.transform.SetParent(mainCanvas.transform, false);

            // Position at top center
            var rectTransform = notification.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 1);
            rectTransform.anchorMax = new Vector2(0.5f, 1);
            rectTransform.anchoredPosition = new Vector2(0, -50);
            rectTransform.sizeDelta = new Vector2(400, 60);

            // Add background
            var background = notification.AddComponent<Image>();
            background.color = new Color(0, 0, 0, 0.8f);

            // Add text
            var textComponent = notification.AddComponent<TextMeshProUGUI>();
            textComponent.text = message;
            textComponent.fontSize = 18;
            textComponent.color = color;
            textComponent.alignment = TextAlignmentOptions.Center;

            return notification;
        }

        #endregion

        #region Additional UI References for Gameplay

        // Additional UI references for gameplay integration
        private GameObject objectivesPanel;
        private Dictionary<string, TextMeshProUGUI> objectiveDisplays = new Dictionary<string, TextMeshProUGUI>();

        [Header("Gameplay UI References")]
        [SerializeField] private TextMeshProUGUI timeRemainingText;
        [SerializeField] private TextMeshProUGUI objectiveProgressText;
        [SerializeField] private Slider objectiveProgressBar;
        [SerializeField] private TextMeshProUGUI globalAlertLevelText;

        // Game end screen references
        private GameObject gameEndPanel;
        private TextMeshProUGUI gameEndTitleText;
        private TextMeshProUGUI gameEndReasonText;
        private TextMeshProUGUI gameEndScoreText;
        private TextMeshProUGUI gameEndObjectivesText;

        #endregion

        #region Debug Support

        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log(message);
            }
        }

        private void LogError(string message)
        {
            Debug.LogError(message);
        }

        #endregion
    }

    /// <summary>
    /// 個別検知インジケーターUI制御クラス
    /// </summary>
    public class DetectionIndicatorUI : MonoBehaviour
    {
        private Image backgroundImage;
        private Image iconImage;
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private float lifetime;
        private float creationTime;
        private NPCVisualSensor targetSensor;

        public bool IsExpired => Time.time - creationTime > lifetime;

        public void Initialize(NPCVisualSensor sensor, float indicatorLifetime)
        {
            targetSensor = sensor;
            lifetime = indicatorLifetime;
            creationTime = Time.time;
        }

        public void Initialize(Image bgImage, Image iImage, CanvasGroup cGroup)
        {
            backgroundImage = bgImage;
            iconImage = iImage;
            canvasGroup = cGroup;
            rectTransform = GetComponent<RectTransform>();
        }

        public void InitializeFromGameObject(GameObject go)
        {
            backgroundImage = go.GetComponentInChildren<Image>();
            canvasGroup = go.GetComponent<CanvasGroup>();
            rectTransform = go.GetComponent<RectTransform>();
        }

        public void SetScreenPosition(Vector3 screenPos)
        {
            if (rectTransform != null)
            {
                rectTransform.position = screenPos;
            }
        }

        public void SetAlertLevel(AlertLevel alertLevel)
        {
            if (backgroundImage != null)
            {
                Color alertColor = alertLevel switch
                {
                    AlertLevel.None => Color.green,
                    AlertLevel.Low => Color.yellow,
                    AlertLevel.Medium => new Color(1f, 0.5f, 0f),
                    AlertLevel.High => Color.red,
                    AlertLevel.Combat => new Color(0.5f, 0f, 0f),
                    _ => Color.white
                };

                alertColor.a = 0.6f;
                backgroundImage.color = alertColor;
            }
        }

        public void SetVisibility(bool visible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
            }
        }

        public void SetLifetime(float newLifetime)
        {
            lifetime = newLifetime;
        }
    }
}