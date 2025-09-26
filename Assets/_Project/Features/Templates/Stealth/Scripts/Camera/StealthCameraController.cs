using UnityEngine;
using Unity.Cinemachine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Data;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Features.Player;
using asterivo.Unity60.Features.Player.States;
// using asterivo.Unity60.Features.Player.Stealth; // Fixed: namespace doesn't exist
using asterivo.Unity60.Features.AI.Visual;
using asterivo.Unity60.Features.Templates.Stealth.Mechanics;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Stealth
{
    /// <summary>
    /// 繧ｹ繝・Ν繧ｹ迚ｹ蛹悶き繝｡繝ｩ繧ｳ繝ｳ繝医Ο繝ｼ繝ｩ繝ｼ
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ髫蟇・憾諷九↓蠢懊§縺滓怙驕ｩ縺ｪ繧ｫ繝｡繝ｩ繧｢繝ｳ繧ｰ繝ｫ縺ｨ蛻ｶ蠕｡繧呈署萓・
    /// </summary>
    public class StealthCameraController : MonoBehaviour
    {
        #region Stealth Camera Configuration

        [TabGroup("Stealth Camera", "Basic Settings")]
        [Title("Stealth Camera Settings", "髫蟇・｡悟虚縺ｫ譛驕ｩ蛹悶＆繧後◆繧ｫ繝｡繝ｩ繧ｷ繧ｹ繝・Β", TitleAlignments.Centered)]
        [SerializeField] private bool enableStealthCamera = true;
        [SerializeField] private bool debugMode = false;

        [TabGroup("Stealth Camera", "Camera References")]
        [Header("Cinemachine Virtual Cameras")]
        [SerializeField] private CinemachineCamera normalCamera;
        [SerializeField] private CinemachineCamera crouchCamera;
        [SerializeField] private CinemachineCamera proneCamera;
        [SerializeField] private CinemachineCamera coverCamera;
        [SerializeField] private CinemachineCamera peekingCamera;

        [TabGroup("Stealth Camera", "Detection Integration")]
        [Header("Detection Indicator Settings")]
        [SerializeField] private bool enableDetectionIndicators = true;
        [SerializeField] private float detectionUIDistance = 50f;
        [SerializeField] private LayerMask detectionLayerMask = -1;
        [SerializeField] private Color undetectedColor = Color.green;
        [SerializeField] private Color suspiciousColor = Color.yellow;
        [SerializeField] private Color alertColor = Color.red;

        #endregion

        #region Camera Behavior Settings

        [TabGroup("Stealth Camera", "Behavior")]
        [Header("Low-Profile Camera Angles")]
        [SerializeField, Range(0f, 45f)] private float crouchCameraAngle = 15f;
        [SerializeField, Range(0f, 30f)] private float proneCameraAngle = 8f;
        [SerializeField, Range(2f, 10f)] private float normalCameraDistance = 5f;
        [SerializeField, Range(1.5f, 8f)] private float crouchCameraDistance = 3.5f;
        [SerializeField, Range(1f, 6f)] private float proneCameraDistance = 2.5f;

        [TabGroup("Stealth Camera", "Shadow")]
        [Header("Shadow-Enhanced Visibility")]
        [SerializeField] private bool enableShadowDetection = true;
        [SerializeField] private Light mainDirectionalLight;
        [SerializeField] private float shadowThreshold = 0.3f;
        [SerializeField] private float shadowCameraBoost = 1.2f;
        [SerializeField] private AnimationCurve shadowVisibilityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [TabGroup("Stealth Camera", "Transitions")]
        [Header("Camera Transition Settings")]
        [SerializeField, Range(0.1f, 2f)] private float transitionSpeed = 0.8f;
        [SerializeField, Range(0.05f, 0.5f)] private float stealthTransitionSpeed = 0.3f;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        #endregion

        #region Runtime State

        [TabGroup("Stealth Camera", "Runtime")]
        [Header("Current State")]
        [SerializeField, ReadOnly] private StealthCameraMode currentMode = StealthCameraMode.Normal;
        [SerializeField, ReadOnly] private float currentShadowLevel = 0f;
        [SerializeField, ReadOnly] private bool isInShadow = false;
        [SerializeField, ReadOnly] private bool isDetectionActive = false;
        [SerializeField, ReadOnly] private Vector3 lastKnownPosition;

        private StealthMechanics stealthMechanics;
        private IStealthAudioService stealthAudioService;
        private Transform playerTransform;
        private UnityEngine.Camera mainCamera;
        private CinemachineCamera activeVirtualCamera;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeStealthCamera();
        }

        private void Start()
        {
            if (enableStealthCamera)
            {
                SetupStealthCameraSystem();
            }
        }

        private void Update()
        {
            if (enableStealthCamera && playerTransform != null)
            {
                UpdateStealthCamera();
            }
        }

        private void LateUpdate()
        {
            if (enableStealthCamera && enableShadowDetection)
            {
                UpdateShadowDetection();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ繧ｫ繝｡繝ｩ繧ｷ繧ｹ繝・Β縺ｮ蛻晄悄蛹・
        /// </summary>
        private void InitializeStealthCamera()
        {
            LogDebug("[StealthCamera] Initializing Stealth Camera System...");

            try
            {
                // 繝｡繧､繝ｳ繧ｫ繝｡繝ｩ縺ｮ蜿門ｾ・
                mainCamera = UnityEngine.Camera.main;
                if (mainCamera == null)
                {
                    mainCamera = FindFirstObjectByType<UnityEngine.Camera>();
                }

                // 繝励Ξ繧､繝､繝ｼ繧ｳ繝ｳ繝昴・繝阪Φ繝医・蜿門ｾ・
                stealthMechanics = StealthMechanics.Instance;
                if (stealthMechanics != null)
                {
                    playerTransform = stealthMechanics.transform;
                }

                // 繧ｵ繝ｼ繝薙せ縺ｮ蜿門ｾ・
                if (FeatureFlags.UseServiceLocator)
                {
                    stealthAudioService = ServiceLocator.GetService<IStealthAudioService>();
                }

                // 繝｡繧､繝ｳ繝ｩ繧､繝医・閾ｪ蜍墓､懷・
                if (mainDirectionalLight == null)
                {
                    Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
                    foreach (Light light in lights)
                    {
                        if (light.type == LightType.Directional)
                        {
                            mainDirectionalLight = light;
                            break;
                        }
                    }
                }

                LogDebug("[StealthCamera] 笨・Stealth Camera initialization completed");
            }
            catch (System.Exception ex)
            {
                LogError($"[StealthCamera] 笶・Initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ繧ｫ繝｡繝ｩ繧ｷ繧ｹ繝・Β縺ｮ繧ｻ繝・ヨ繧｢繝・・
        /// </summary>
        private void SetupStealthCameraSystem()
        {
            LogDebug("[StealthCamera] Setting up Stealth Camera System...");

            // Virtual Camera縺ｮ險ｭ螳・
            SetupVirtualCameras();

            // 蛻晄悄繧ｫ繝｡繝ｩ繝｢繝ｼ繝峨・險ｭ螳・
            SetCameraMode(StealthCameraMode.Normal);

            // 繧､繝吶Φ繝医Μ繧ｹ繝翫・縺ｮ險ｭ螳・
            SetupEventListeners();

            LogDebug("[StealthCamera] 笨・Stealth Camera setup completed");
        }

        /// <summary>
        /// Virtual Camera縺ｮ險ｭ螳・
        /// </summary>
        private void SetupVirtualCameras()
        {
            if (normalCamera != null)
            {
                ConfigureVirtualCamera(normalCamera, normalCameraDistance, 0f);
                activeVirtualCamera = normalCamera;
            }

            if (crouchCamera != null)
            {
                ConfigureVirtualCamera(crouchCamera, crouchCameraDistance, crouchCameraAngle);
            }

            if (proneCamera != null)
            {
                ConfigureVirtualCamera(proneCamera, proneCameraDistance, proneCameraAngle);
            }

            // 繧ｫ繝舌・繧ｫ繝｡繝ｩ縺ｨ繝斐・繧ｭ繝ｳ繧ｰ繧ｫ繝｡繝ｩ縺ｮ迚ｹ谿願ｨｭ螳・
            if (coverCamera != null)
            {
                ConfigureCoverCamera(coverCamera);
            }

            if (peekingCamera != null)
            {
                ConfigurePeekingCamera(peekingCamera);
            }
        }

        /// <summary>
        /// Virtual Camera縺ｮ蝓ｺ譛ｬ險ｭ螳・
        /// </summary>
        private void ConfigureVirtualCamera(CinemachineCamera vcam, float distance, float angle)
        {
            if (vcam == null) return;

            // Follow險ｭ螳・
            if (playerTransform != null)
            {
                vcam.Follow = playerTransform;
                vcam.LookAt = playerTransform;
            }

            // Composer險ｭ螳・
            var composer = vcam.GetComponent<CinemachinePositionComposer>();
            if (composer == null)
            {
                composer = vcam.gameObject.AddComponent<CinemachinePositionComposer>();
            }

            // Transposer險ｭ螳・
            var transposer = vcam.GetComponent<CinemachineFollow>();
            if (transposer == null)
            {
                transposer = vcam.gameObject.AddComponent<CinemachineFollow>();
            }

            // 霍晞屬縺ｨ繧｢繝ｳ繧ｰ繝ｫ縺ｮ險ｭ螳・
            transposer.FollowOffset = new Vector3(0f, distance * 0.6f, -distance);
            // composer.TrackedObjectOffset = new Vector3(0f, angle * 0.1f, 0f);
        }

        /// <summary>
        /// 繧ｫ繝舌・繧ｫ繝｡繝ｩ縺ｮ迚ｹ谿願ｨｭ螳・
        /// </summary>
        private void ConfigureCoverCamera(CinemachineCamera vcam)
        {
            // 繧ｫ繝舌・譎ゅ・閧ｩ雜翫＠隕也せ險ｭ螳・
            var transposer = vcam.GetComponent<CinemachineFollow>();
            if (transposer != null)
            {
                transposer.FollowOffset = new Vector3(1.2f, 1.8f, -2f);
            }

            var composer = vcam.GetComponent<CinemachinePositionComposer>();
            if (composer != null)
            {
                // composer.TrackedObjectOffset = new Vector3(0.5f, 0.2f, 0f);
            }
        }

        /// <summary>
        /// 繝斐・繧ｭ繝ｳ繧ｰ繧ｫ繝｡繝ｩ縺ｮ迚ｹ谿願ｨｭ螳・
        /// </summary>
        private void ConfigurePeekingCamera(CinemachineCamera vcam)
        {
            // 繝斐・繧ｭ繝ｳ繧ｰ譎ゅ・隕也せ險ｭ螳・
            var transposer = vcam.GetComponent<CinemachineFollow>();
            if (transposer != null)
            {
                transposer.FollowOffset = new Vector3(0.8f, 1.5f, -1.5f);
            }
        }

        /// <summary>
        /// 繧､繝吶Φ繝医Μ繧ｹ繝翫・縺ｮ險ｭ螳・
        /// </summary>
        private void SetupEventListeners()
        {
            // 繝励Ξ繧､繝､繝ｼ迥ｶ諷句､画峩繧､繝吶Φ繝医・繝ｪ繧ｹ繝ｳ
            // 螳溯｣・・譌｢蟄倥・繧､繝吶Φ繝医す繧ｹ繝・Β縺ｫ萓晏ｭ・
        }

        #endregion

        #region Camera Control

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ繧ｫ繝｡繝ｩ縺ｮ譖ｴ譁ｰ
        /// </summary>
        private void UpdateStealthCamera()
        {
            // 繝励Ξ繧､繝､繝ｼ縺ｮ迥ｶ諷九↓蝓ｺ縺･縺上き繝｡繝ｩ繝｢繝ｼ繝画ｱｺ螳・
            DetermineOptimalCameraMode();

            // 繧ｫ繝｡繝ｩ繝医Λ繝ｳ繧ｸ繧ｷ繝ｧ繝ｳ縺ｮ蜃ｦ逅・
            ProcessCameraTransition();

            // 讀懃衍繧､繝ｳ繧ｸ繧ｱ繝ｼ繧ｿ繝ｼ縺ｮ譖ｴ譁ｰ
            if (enableDetectionIndicators)
            {
                UpdateDetectionIndicators();
            }

            // 菴咲ｽｮ縺ｮ險倬鹸
            lastKnownPosition = playerTransform.position;
        }

        /// <summary>
        /// 譛驕ｩ縺ｪ繧ｫ繝｡繝ｩ繝｢繝ｼ繝峨・豎ｺ螳・
        /// </summary>
        private void DetermineOptimalCameraMode()
        {
            if (stealthMechanics == null) return;

            StealthCameraMode newMode = StealthCameraMode.Normal;

            // 繝励Ξ繧､繝､繝ｼ縺ｮ迥ｶ諷九ｒ讀懈渊・・tealthMovementController縺九ｉ諠・ｱ蜿門ｾ暦ｼ・
            var stealthMovement = stealthMechanics.GetComponent<StealthMovementController>();
            if (stealthMovement != null)
            {
                // 蟋ｿ蜍｢縺ｫ蝓ｺ縺･縺上き繝｡繝ｩ繝｢繝ｼ繝画ｱｺ螳・
                switch (stealthMovement.GetCurrentStance())
                {
                    case MovementStance.Standing:
                        newMode = StealthCameraMode.Normal;
                        break;
                    case MovementStance.Crouching:
                        newMode = StealthCameraMode.Crouch;
                        break;
                    case MovementStance.Prone:
                        newMode = StealthCameraMode.Prone;
                        break;
                }

                // 蠖ｱ縺ｮ迥ｶ諷九↓繧医ｋ隱ｿ謨ｴ
                if (isInShadow && currentShadowLevel > shadowThreshold)
                {
                    ApplyShadowCameraBoost(newMode);
                }
            }

            if (newMode != currentMode)
            {
                SetCameraMode(newMode);
            }
        }

        /// <summary>
        /// 繧ｫ繝舌・繝｢繝ｼ繝峨・隧ｳ邏ｰ豎ｺ螳・
        /// </summary>
        private StealthCameraMode DetermineCoverMode(StealthMovementController stealthMovement)
        {
            // 繝斐・繧ｭ繝ｳ繧ｰ迥ｶ諷九・遒ｺ隱・
            // TODO: Add IsPeeking property to StealthMovementController
            if (false) // stealthMovement.IsPeeking
            {
                return StealthCameraMode.Peeking;
            }

            return StealthCameraMode.Cover;
        }

        /// <summary>
        /// 繧ｫ繝｡繝ｩ繝｢繝ｼ繝峨・險ｭ螳・
        /// </summary>
        public void SetCameraMode(StealthCameraMode newMode)
        {
            if (currentMode == newMode) return;

            var previousMode = currentMode;
            currentMode = newMode;

            // Virtual Camera縺ｮ蜆ｪ蜈亥ｺｦ險ｭ螳・
            ResetAllCameraPriorities();

            CinemachineCamera targetCamera = GetCameraForMode(newMode);
            if (targetCamera != null)
            {
                targetCamera.Priority = 10;
                activeVirtualCamera = targetCamera;
            }

            LogDebug($"[StealthCamera] Camera mode changed: {previousMode} 竊・{newMode}");

            // 繝医Λ繝ｳ繧ｸ繧ｷ繝ｧ繝ｳ騾溷ｺｦ縺ｮ隱ｿ謨ｴ
            AdjustTransitionSpeed(newMode);
        }

        /// <summary>
        /// 蜈ｨ繧ｫ繝｡繝ｩ縺ｮ蜆ｪ蜈亥ｺｦ繝ｪ繧ｻ繝・ヨ
        /// </summary>
        private void ResetAllCameraPriorities()
        {
            if (normalCamera != null) normalCamera.Priority = 0;
            if (crouchCamera != null) crouchCamera.Priority = 0;
            if (proneCamera != null) proneCamera.Priority = 0;
            if (coverCamera != null) coverCamera.Priority = 0;
            if (peekingCamera != null) peekingCamera.Priority = 0;
        }

        /// <summary>
        /// 繝｢繝ｼ繝峨↓蟇ｾ蠢懊☆繧九き繝｡繝ｩ縺ｮ蜿門ｾ・
        /// </summary>
        private CinemachineCamera GetCameraForMode(StealthCameraMode mode)
        {
            return mode switch
            {
                StealthCameraMode.Normal => normalCamera,
                StealthCameraMode.Crouch => crouchCamera,
                StealthCameraMode.Prone => proneCamera,
                StealthCameraMode.Cover => coverCamera,
                StealthCameraMode.Peeking => peekingCamera,
                _ => normalCamera
            };
        }

        /// <summary>
        /// 繝医Λ繝ｳ繧ｸ繧ｷ繝ｧ繝ｳ騾溷ｺｦ縺ｮ隱ｿ謨ｴ
        /// </summary>
        private void AdjustTransitionSpeed(StealthCameraMode mode)
        {
            float speed = (mode == StealthCameraMode.Cover || mode == StealthCameraMode.Peeking)
                ? stealthTransitionSpeed
                : transitionSpeed;

            if (activeVirtualCamera != null)
            {
                // TODO: Update CinemachineCore API for 3.1.4
                // var brain = CinemachineCore.Instance.GetActiveBrain(0);
                // if (brain != null)
                // {
                //     brain.m_DefaultBlend.m_Time = speed;
                // }
            }
        }

        /// <summary>
        /// 繧ｫ繝｡繝ｩ繝医Λ繝ｳ繧ｸ繧ｷ繝ｧ繝ｳ縺ｮ蜃ｦ逅・
        /// </summary>
        private void ProcessCameraTransition()
        {
            if (activeVirtualCamera == null) return;

            // 繧ｹ繝繝ｼ繧ｺ縺ｪ繝医Λ繝ｳ繧ｸ繧ｷ繝ｧ繝ｳ蜉ｹ譫懊・驕ｩ逕ｨ
            ApplyTransitionEffects();
        }

        /// <summary>
        /// 繝医Λ繝ｳ繧ｸ繧ｷ繝ｧ繝ｳ蜉ｹ譫懊・驕ｩ逕ｨ
        /// </summary>
        private void ApplyTransitionEffects()
        {
            // 繧ｫ繝ｼ繝悶ｒ菴ｿ逕ｨ縺励◆繧ｹ繝繝ｼ繧ｺ縺ｪ繝医Λ繝ｳ繧ｸ繧ｷ繝ｧ繝ｳ
            if (activeVirtualCamera != null)
            {
                var transposer = activeVirtualCamera.GetComponent<CinemachineFollow>();
                if (transposer != null)
                {
                    float t = Time.time % 1f;
                    float curveValue = transitionCurve.Evaluate(t);
                    // 繝医Λ繝ｳ繧ｸ繧ｷ繝ｧ繝ｳ蜉ｹ譫懊・驕ｩ逕ｨ
                }
            }
        }

        #endregion

        #region Shadow Detection & Enhancement

        /// <summary>
        /// 蠖ｱ讀懃衍縺ｮ譖ｴ譁ｰ
        /// </summary>
        private void UpdateShadowDetection()
        {
            if (mainDirectionalLight == null || playerTransform == null) return;

            // 蜈臥ｷ壹↓繧医ｋ蠖ｱ蛻､螳・
            Vector3 lightDirection = -mainDirectionalLight.transform.forward;
            Vector3 playerPosition = playerTransform.position + Vector3.up * 0.1f;

            RaycastHit hit;
            bool inShadow = Physics.Raycast(playerPosition, lightDirection, out hit, 100f);

            if (inShadow)
            {
                currentShadowLevel = CalculateShadowLevel(hit);
                isInShadow = currentShadowLevel > shadowThreshold;
            }
            else
            {
                currentShadowLevel = 0f;
                isInShadow = false;
            }

            // 蠖ｱ迥ｶ諷九・螟牙喧繧呈､懃衍
            if (isInShadow)
            {
                ApplyShadowCameraBoost(currentMode);
            }
        }

        /// <summary>
        /// 蠖ｱ繝ｬ繝吶Ν縺ｮ險育ｮ・
        /// </summary>
        private float CalculateShadowLevel(RaycastHit hit)
        {
            float distance = hit.distance;
            float lightIntensity = mainDirectionalLight.intensity;

            // 霍晞屬縺ｨ蜈峨・蠑ｷ蠎ｦ繧定・・縺励◆蠖ｱ繝ｬ繝吶Ν險育ｮ・
            float shadowLevel = Mathf.Clamp01(1f - (distance * 0.1f)) * lightIntensity;

            return shadowVisibilityCurve.Evaluate(shadowLevel);
        }

        /// <summary>
        /// 蠖ｱ繧ｫ繝｡繝ｩ繝悶・繧ｹ繝医・驕ｩ逕ｨ
        /// </summary>
        private void ApplyShadowCameraBoost(StealthCameraMode mode)
        {
            if (activeVirtualCamera == null) return;

            var transposer = activeVirtualCamera.GetComponent<CinemachineFollow>();
            if (transposer != null)
            {
                Vector3 originalOffset = transposer.FollowOffset;
                Vector3 boostedOffset = originalOffset * shadowCameraBoost;

                // 繧ｹ繝繝ｼ繧ｺ縺ｪ驕ｩ逕ｨ
                transposer.FollowOffset = Vector3.Lerp(originalOffset, boostedOffset, Time.deltaTime * 2f);
            }
        }

        #endregion

        #region Detection Indicators

        /// <summary>
        /// 讀懃衍繧､繝ｳ繧ｸ繧ｱ繝ｼ繧ｿ繝ｼ縺ｮ譖ｴ譁ｰ
        /// </summary>
        private void UpdateDetectionIndicators()
        {
            if (!enableDetectionIndicators || mainCamera == null) return;

            // 遽・峇蜀・・NPC繧呈､懃ｴ｢
            Collider[] nearbyNPCs = Physics.OverlapSphere(playerTransform.position, detectionUIDistance, detectionLayerMask);

            foreach (Collider npcCollider in nearbyNPCs)
            {
                var npcVisualSensor = npcCollider.GetComponent<NPCVisualSensor>();
                if (npcVisualSensor != null)
                {
                    UpdateNPCDetectionIndicator(npcVisualSensor);
                }
            }
        }

        /// <summary>
        /// NPC讀懃衍繧､繝ｳ繧ｸ繧ｱ繝ｼ繧ｿ繝ｼ縺ｮ譖ｴ譁ｰ
        /// </summary>
        private void UpdateNPCDetectionIndicator(NPCVisualSensor npcSensor)
        {
            // NPC縺ｮ隴ｦ謌偵Ξ繝吶Ν縺ｫ蝓ｺ縺･縺剰牡險ｭ螳・
            Color indicatorColor = GetIndicatorColor(npcSensor.GetCurrentAlertLevel());

            // 逕ｻ髱｢荳翫・縺ｮ菴咲ｽｮ險育ｮ・
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(npcSensor.transform.position);

            if (screenPosition.z > 0) // 繧ｫ繝｡繝ｩ縺ｮ蜑肴婿縺ｫ縺・ｋ蝣ｴ蜷・
            {
                // UI隕∫ｴ縺ｮ譖ｴ譁ｰ・亥ｮ溯｣・・UI繧ｷ繧ｹ繝・Β縺ｫ萓晏ｭ假ｼ・
                DisplayDetectionIndicator(screenPosition, indicatorColor, npcSensor.GetCurrentAlertLevel());
            }
        }

        /// <summary>
        /// 繧､繝ｳ繧ｸ繧ｱ繝ｼ繧ｿ繝ｼ濶ｲ縺ｮ蜿門ｾ・
        /// </summary>
        private Color GetIndicatorColor(asterivo.Unity60.Core.Data.AlertLevel alertLevel)
        {
            return alertLevel switch
            {
                asterivo.Unity60.Core.Data.AlertLevel.Relaxed => undetectedColor,
                asterivo.Unity60.Core.Data.AlertLevel.Suspicious => suspiciousColor,
                asterivo.Unity60.Core.Data.AlertLevel.Investigating => suspiciousColor,
                asterivo.Unity60.Core.Data.AlertLevel.Alert => alertColor,
                _ => undetectedColor
            };
        }

        /// <summary>
        /// 讀懃衍繧､繝ｳ繧ｸ繧ｱ繝ｼ繧ｿ繝ｼ縺ｮ陦ｨ遉ｺ
        /// </summary>
        private void DisplayDetectionIndicator(Vector3 screenPosition, Color color, asterivo.Unity60.Core.Data.AlertLevel alertLevel)
        {
            // UI繧ｷ繧ｹ繝・Β縺ｨ縺ｮ騾｣謳ｺ・亥ｮ溯｣・・StealthUIManager縺ｫ蟋碑ｭｲ・・
            // 縺薙％縺ｧ縺ｯ蝓ｺ譛ｬ逧・↑謠冗判蜃ｦ逅・・縺ｿ
        }

        #endregion

        #region Public API

        /// <summary>
        /// 謇句虚繧ｫ繝｡繝ｩ繝｢繝ｼ繝芽ｨｭ螳・
        /// </summary>
        public void ForceSetCameraMode(StealthCameraMode mode)
        {
            SetCameraMode(mode);
        }

        /// <summary>
        /// 繧ｫ繝｡繝ｩ繧ｷ繧ｧ繧､繧ｯ蜉ｹ譫・
        /// </summary>
        public void TriggerCameraShake(float intensity, float duration)
        {
            if (activeVirtualCamera != null)
            {
                var impulse = activeVirtualCamera.GetComponent<CinemachineImpulseSource>();
                if (impulse == null)
                {
                    impulse = activeVirtualCamera.gameObject.AddComponent<CinemachineImpulseSource>();
                }

                impulse.GenerateImpulse(Vector3.one * intensity);
            }
        }

        /// <summary>
        /// 讀懃衍迥ｶ諷九・蜿門ｾ・
        /// </summary>
        public bool IsPlayerDetected()
        {
            return isDetectionActive;
        }

        /// <summary>
        /// 蠖ｱ迥ｶ諷九・蜿門ｾ・
        /// </summary>
        public bool IsPlayerInShadow()
        {
            return isInShadow;
        }

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ繧ｫ繝｡繝ｩ繝｢繝ｼ繝牙叙蠕・
        /// </summary>
        public StealthCameraMode GetCurrentCameraMode()
        {
            return currentMode;
        }

        #endregion

        #region Editor Support

        [TabGroup("Stealth Camera", "Actions")]
        [Button("Test Normal Camera")]
        public void TestNormalCamera()
        {
            SetCameraMode(StealthCameraMode.Normal);
        }

        [Button("Test Crouch Camera")]
        public void TestCrouchCamera()
        {
            SetCameraMode(StealthCameraMode.Crouch);
        }

        [Button("Test Prone Camera")]
        public void TestProneCamera()
        {
            SetCameraMode(StealthCameraMode.Prone);
        }

        [Button("Test Cover Camera")]
        public void TestCoverCamera()
        {
            SetCameraMode(StealthCameraMode.Cover);
        }

        [Button("Test Camera Shake")]
        public void TestCameraShake()
        {
            TriggerCameraShake(1f, 0.5f);
        }

        [Button("Validate Camera Setup")]
        public void ValidateCameraSetup()
        {
            LogDebug("=== Stealth Camera Validation ===");
            LogDebug($"Normal Camera: {(normalCamera != null ? "笨・Found" : "笶・Missing")}");
            LogDebug($"Crouch Camera: {(crouchCamera != null ? "笨・Found" : "笶・Missing")}");
            LogDebug($"Prone Camera: {(proneCamera != null ? "笨・Found" : "笶・Missing")}");
            LogDebug($"Cover Camera: {(coverCamera != null ? "笨・Found" : "笶・Missing")}");
            LogDebug($"Peeking Camera: {(peekingCamera != null ? "笨・Found" : "笶・Missing")}");
            LogDebug($"Main Light: {(mainDirectionalLight != null ? "笨・Found" : "笶・Missing")}");
            LogDebug($"Player Controller: {(stealthMechanics != null ? "笨・Found" : "笶・Missing")}");
            LogDebug($"Current Mode: {currentMode}");
            LogDebug($"Shadow Level: {currentShadowLevel:F2}");
            LogDebug($"In Shadow: {isInShadow}");
            LogDebug("=== Validation Complete ===");
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (playerTransform == null) return;

            // 讀懃衍遽・峇縺ｮ蜿ｯ隕門喧
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(playerTransform.position, detectionUIDistance);

            // 蠖ｱ蛻､螳壹Ξ繧､縺ｮ蜿ｯ隕門喧
            if (mainDirectionalLight != null)
            {
                Gizmos.color = isInShadow ? Color.blue : Color.yellow;
                Vector3 lightDirection = -mainDirectionalLight.transform.forward;
                Gizmos.DrawRay(playerTransform.position + Vector3.up * 0.1f, lightDirection * 10f);
            }

            // 繧ｫ繝｡繝ｩ菴咲ｽｮ縺ｮ蜿ｯ隕門喧
            if (activeVirtualCamera != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(activeVirtualCamera.transform.position, Vector3.one * 0.5f);
            }
        }
#endif

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
    /// 繧ｹ繝・Ν繧ｹ繧ｫ繝｡繝ｩ繝｢繝ｼ繝・
    /// </summary>
    public enum StealthCameraMode
    {
        Normal,     // 騾壼ｸｸ遶九■迥ｶ諷・
        Crouch,     // 縺励ｃ縺後∩迥ｶ諷・
        Prone,      // 莨上○迥ｶ諷・
        Cover,      // 繧ｫ繝舌・迥ｶ諷・
        Peeking     // 繝斐・繧ｭ繝ｳ繧ｰ迥ｶ諷・
    }
}


