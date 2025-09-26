using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.Stealth.Services;
using asterivo.Unity60.Features.Templates.Stealth.Events;
using StealthInteractionType = asterivo.Unity60.Features.Templates.Stealth.Services.StealthInteractionType;

namespace asterivo.Unity60.Features.Templates.Stealth.Commands
{
    /// <summary>
    /// 繧ｹ繝・Ν繧ｹ迺ｰ蠅・嶌莠剃ｽ懃畑繧ｳ繝槭Φ繝・
    /// 繧ｹ繝・Ν繧ｹ繧ｲ繝ｼ繝縺ｫ迚ｹ蛹悶＠縺溽腸蠅・が繝悶ず繧ｧ繧ｯ繝医→縺ｮ逶ｸ莠剃ｽ懃畑繧堤ｮ｡逅・
    /// ServiceLocator邨ｱ蜷医↓繧医ｋStealthService縺ｨ縺ｮ騾｣謳ｺ
    /// ObjectPool譛驕ｩ蛹門ｯｾ蠢・
    /// </summary>
    public class StealthInteractionCommand : IResettableCommand
    {

        // Private fields for command state
        private StealthInteractionType _interactionType;
        private GameObject _targetObject;
        private Vector3 _targetPosition;
        private float _interactionDuration;
        private bool _requiresStealth;
        private string _interactionId;

        // Service references
        private IStealthService _stealthService;

        // Execution state
        private bool _isExecuted = false;
        private bool _wasPlayerConcealed;
        private float _originalVisibility;
        private EnvironmentalInteractionData _interactionData;

        /// <summary>
        /// Undo謫堺ｽ懊し繝昴・繝育憾豕・
        /// 繧ｹ繝・Ν繧ｹ逶ｸ莠剃ｽ懃畑縺ｯ蝓ｺ譛ｬ逧・↓Undo繧偵し繝昴・繝・
        /// </summary>
        public bool CanUndo => _isExecuted && _interactionType != StealthInteractionType.HideBody;

        /// <summary>
        /// 繝・ヵ繧ｩ繝ｫ繝医さ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ・・bjectPool蟇ｾ蠢懶ｼ・
        /// </summary>
        public StealthInteractionCommand()
        {
            // 繝励・繝ｫ蛹門ｯｾ蠢懊・縺溘ａ遨ｺ縺ｮ繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        }

        /// <summary>
        /// 繝代Λ繝｡繝ｼ繧ｿ莉倥″繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        /// <param name="interactionType">逶ｸ莠剃ｽ懃畑縺ｮ遞ｮ鬘・/param>
        /// <param name="targetObject">蟇ｾ雎｡繧ｪ繝悶ず繧ｧ繧ｯ繝・/param>
        /// <param name="duration">螳溯｡梧凾髢・/param>
        /// <param name="requiresStealth">繧ｹ繝・Ν繧ｹ迥ｶ諷玖ｦ∵ｱ・/param>
        public StealthInteractionCommand(StealthInteractionType interactionType, GameObject targetObject,
            float duration = 1.0f, bool requiresStealth = true)
        {
            _interactionType = interactionType;
            _targetObject = targetObject;
            _targetPosition = targetObject != null ? targetObject.transform.position : Vector3.zero;
            _interactionDuration = duration;
            _requiresStealth = requiresStealth;
            _interactionId = System.Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 繧ｳ繝槭Φ繝牙ｮ溯｡・
        /// StealthService縺ｨ騾｣謳ｺ縺励※繧ｹ繝・Ν繧ｹ逶ｸ莠剃ｽ懃畑繧貞ｮ溯｡・
        /// </summary>
        public void Execute()
        {
            if (_isExecuted)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[StealthInteractionCommand] Already executed: {_interactionType}");
#endif
                return;
            }

            // ServiceLocator邨檎罰縺ｧStealthService繧貞叙蠕・
            _stealthService = ServiceLocator.GetService<IStealthService>();
            if (_stealthService == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("[StealthInteractionCommand] StealthService not found in ServiceLocator");
#endif
                return;
            }

            // 繧ｹ繝・Ν繧ｹ迥ｶ諷九・莠句燕繝√ぉ繝・け
            if (_requiresStealth && !ValidateStealthState())
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[StealthInteractionCommand] Stealth requirements not met for {_interactionType}");
#endif
                return;
            }

            // 螳溯｡悟燕縺ｮ迥ｶ諷九ｒ菫晏ｭ・
            _wasPlayerConcealed = _stealthService.IsPlayerConcealed;
            _originalVisibility = _stealthService.PlayerVisibilityFactor;

            // 逶ｸ莠剃ｽ懃畑繝・・繧ｿ縺ｮ貅門ｙ
            _interactionData = new EnvironmentalInteractionData
            {
                TargetObject = _targetObject,
                InteractionType = _interactionType,
                Success = false, // 螳溯｡悟ｾ後↓true縺ｫ險ｭ螳・
                Position = _targetPosition,
                GeneratedNoiseLevel = 0.0f, // 逶ｸ莠剃ｽ懃畑縺ｮ遞ｮ鬘槭↓蠢懊§縺ｦ險ｭ螳・
                Timestamp = Time.time
            };

            // 逶ｸ莠剃ｽ懃畑縺ｮ遞ｮ鬘槭↓蠢懊§縺溷ｮ溯｡・
            ExecuteSpecificInteraction();

            // 螳溯｡悟ｮ御ｺ・ｾ後・迥ｶ諷区峩譁ｰ
            _interactionData.Success = true;
            _interactionData.GeneratedNoiseLevel = GetNoiseLevel(_interactionType);

            _isExecuted = true;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[StealthInteractionCommand] Executed {_interactionType} on {_targetObject?.name ?? "position"}");
#endif
        }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ迥ｶ諷九・讀懆ｨｼ
        /// </summary>
        private bool ValidateStealthState()
        {
            if (_stealthService == null) return false;

            // 蝓ｺ譛ｬ逧・↑繧ｹ繝・Ν繧ｹ隕∽ｻｶ繝√ぉ繝・け
            if (_requiresStealth && !_stealthService.IsStealthModeActive)
                return false;

            // 逶ｸ莠剃ｽ懃畑繧ｿ繧､繝怜挨縺ｮ迚ｹ谿願ｦ∽ｻｶ
            switch (_interactionType)
            {
                case StealthInteractionType.HideBody:
                    // 豁ｻ菴馴國蛹ｿ縺ｯ螳悟・縺ｫ髫阡ｽ縺輔ｌ縺ｦ縺・ｋ蠢・ｦ√′縺ゅｋ
                    return _stealthService.IsPlayerConcealed && _stealthService.PlayerVisibilityFactor < 0.3f;

                case StealthInteractionType.OperateDoor:
                case StealthInteractionType.OperateSwitch:
                    // 謫堺ｽ懊・荳ｭ遞句ｺｦ縺ｮ髫阡ｽ縺ｧ蜊∝・
                    return _stealthService.PlayerVisibilityFactor < 0.7f;

                case StealthInteractionType.DisableCamera:
                case StealthInteractionType.SabotageLight:
                    // 險ｭ蛯咏ｴ螢翫・豈碑ｼ・噪蟇帛ｮｹ
                    return _stealthService.PlayerVisibilityFactor < 0.8f;

                case StealthInteractionType.ThrowObject:
                    // 髯ｽ蜍輔・迚ｹ蛻･縺ｪ隕∽ｻｶ縺ｪ縺・
                    return true;

                default:
                    return _stealthService.PlayerVisibilityFactor < 0.5f;
            }
        }

        /// <summary>
        /// 逶ｸ莠剃ｽ懃畑繧ｿ繧､繝怜挨縺ｮ蜈ｷ菴鍋噪螳溯｡悟・逅・
        /// </summary>
        private void ExecuteSpecificInteraction()
        {
            switch (_interactionType)
            {
                case StealthInteractionType.DisableCamera:
                    ExecuteDisableCamera();
                    break;

                case StealthInteractionType.SabotageLight:
                    ExecuteDisableLight();
                    break;

                case StealthInteractionType.ThrowObject:
                    ExecuteCreateDistraction();
                    break;

                case StealthInteractionType.OperateDoor:
                    ExecuteOperateDoor();
                    break;

                case StealthInteractionType.OperateSwitch:
                    ExecuteOperateSwitch();
                    break;

                case StealthInteractionType.HideBody:
                    ExecuteHideBody();
                    break;

                default:
                    Debug.LogWarning($"[StealthInteractionCommand] Unknown interaction type: {_interactionType}");
                    break;
            }

            // StealthService繧帝壹§縺ｦ繧､繝吶Φ繝育匱陦・
            _stealthService?.InteractWithEnvironment(_targetObject, _interactionType);
        }

        /// <summary>
        /// 逶｣隕悶き繝｡繝ｩ辟｡蜉ｹ蛹門ｮ溯｡・
        /// </summary>
        private void ExecuteDisableCamera()
        {
            if (_targetObject == null) return;

            // 繧ｫ繝｡繝ｩ繧ｳ繝ｳ繝昴・繝阪Φ繝医・辟｡蜉ｹ蛹・
            var camera = _targetObject.GetComponent<UnityEngine.Camera>();
            if (camera != null)
            {
                camera.enabled = false;
            }

            // 繧ｹ繝・Ν繧ｹ讀懃衍繧ｷ繧ｹ繝・Β縺ｮ辟｡蜉ｹ蛹・
            var detectionSensor = _targetObject.GetComponent<MonoBehaviour>();
            if (detectionSensor != null)
            {
                detectionSensor.enabled = false;
            }

            // 荳譎ら噪縺ｪ隕冶ｪ肴ｧ蜷台ｸ奇ｼ井ｽ懈･ｭ荳ｭ縺ｮ繝ｪ繧ｹ繧ｯ・・
            _stealthService?.UpdatePlayerVisibility(_originalVisibility + 0.3f);

            // 繧ｨ繝輔ぉ繧ｯ繝医・繧ｵ繧ｦ繝ｳ繝牙・逕・
            PlayInteractionEffects("camera_disable");
        }

        /// <summary>
        /// 辣ｧ譏守┌蜉ｹ蛹門ｮ溯｡・
        /// </summary>
        private void ExecuteDisableLight()
        {
            if (_targetObject == null) return;

            // 繝ｩ繧､繝医さ繝ｳ繝昴・繝阪Φ繝医・辟｡蜉ｹ蛹・
            var light = _targetObject.GetComponent<Light>();
            if (light != null)
            {
                light.enabled = false;
            }

            // 蜻ｨ蝗ｲ縺ｮ證鈴裸蛹悶↓繧医ｋ髫阡ｽ諤ｧ蜷台ｸ・
            _stealthService?.UpdatePlayerVisibility(_originalVisibility * 0.7f);

            PlayInteractionEffects("light_disable");
        }

        /// <summary>
        /// 髯ｽ蜍穂ｽ懈・螳溯｡・
        /// </summary>
        private void ExecuteCreateDistraction()
        {
            // 髯ｽ蜍暮浹縺ｮ菴懈・
            _stealthService?.CreateDistraction(_targetPosition, 0.8f);

            // 繝励Ξ繧､繝､繝ｼ縺ｮ荳譎ら噪髫阡ｽ諤ｧ蜷台ｸ奇ｼ域ｳｨ諢上′縺昴ｌ繧九◆繧・ｼ・
            _stealthService?.UpdatePlayerVisibility(_originalVisibility * 0.8f);

            PlayInteractionEffects("distraction_create");
        }

        /// <summary>
        /// 骰ｵ髢九￠螳溯｡・
        /// </summary>
        private void ExecuteLockPicking()
        {
            if (_targetObject == null) return;

            // 繝峨い縺ｮ隗｣骭迥ｶ諷句､画峩
            var lockable = _targetObject.GetComponent<ILockable>();
            lockable?.Unlock();

            // 髮・ｸｭ縺ｫ繧医ｋ荳譎ら噪隕冶ｪ肴ｧ蠅怜刈
            _stealthService?.UpdatePlayerVisibility(_originalVisibility + 0.2f);

            PlayInteractionEffects("lock_picking");
        }

        /// <summary>
        /// 辟｡髻ｳ蛻ｶ蝨ｧ螳溯｡・
        /// </summary>
        private void ExecuteSilentTakedown()
        {
            if (_targetObject == null) return;

            // 蟇ｾ雎｡NPC縺ｮ辟｡蜉帛喧
            var npcHealth = _targetObject.GetComponent<MonoBehaviour>();
            if (npcHealth != null)
            {
                // NPC縺ｮ迥ｶ諷九ｒ辟｡蜉帛喧縺ｫ螟画峩
                npcHealth.enabled = false;
            }

            // 蛻ｶ蝨ｧ蠕後・髫阡ｽ諤ｧ蜷台ｸ・
            _stealthService?.UpdatePlayerVisibility(_originalVisibility * 0.5f);

            PlayInteractionEffects("silent_takedown");
        }

        /// <summary>
        /// 豁ｻ菴馴國蛹ｿ螳溯｡・
        /// </summary>
        private void ExecuteHideBody()
        {
            if (_targetObject == null) return;

            // 豁ｻ菴薙が繝悶ず繧ｧ繧ｯ繝医・髱櫁｡ｨ遉ｺ蛹・
            _targetObject.SetActive(false);

            // 險ｼ諡髫貊・↓繧医ｋ髫阡ｽ諤ｧ蜷台ｸ・
            _stealthService?.UpdatePlayerVisibility(_originalVisibility * 0.9f);

            PlayInteractionEffects("hide_body");
        }

        /// <summary>
        /// 隴ｦ蝣ｱ陬・ｽｮ遐ｴ螢雁ｮ溯｡・
        /// </summary>
        private void ExecuteSabotageAlarm()
        {
            if (_targetObject == null) return;

            // 隴ｦ蝣ｱ繧ｷ繧ｹ繝・Β縺ｮ辟｡蜉ｹ蛹・
            var alarmSystem = _targetObject.GetComponent<MonoBehaviour>();
            if (alarmSystem != null)
            {
                alarmSystem.enabled = false;
            }

            PlayInteractionEffects("sabotage_alarm");
        }

        /// <summary>
        /// 遶ｯ譛ｫ繧｢繧ｯ繧ｻ繧ｹ螳溯｡・
        /// </summary>
        private void ExecuteAccessTerminal()
        {
            if (_targetObject == null) return;

            // 遶ｯ譛ｫ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ縺ｮ繧｢繧ｯ繝・ぅ繝吶・繝・
            var terminal = _targetObject.GetComponent<IAccessible>();
            terminal?.Access();

            // 髮・ｸｭ縺ｫ繧医ｋ隕冶ｪ肴ｧ蠅怜刈
            _stealthService?.UpdatePlayerVisibility(_originalVisibility + 0.4f);

            PlayInteractionEffects("terminal_access");
        }

        /// <summary>
        /// 迺ｰ蠅・國阡ｽ蛻ｩ逕ｨ螳溯｡・
        /// </summary>
        private void ExecuteEnvironmentalHide()
        {
            // 迺ｰ蠅・國阡ｽ繧ｾ繝ｼ繝ｳ縺ｸ縺ｮ遘ｻ蜍輔・蛻ｩ逕ｨ
            _stealthService?.UpdatePlayerVisibility(_originalVisibility * 0.3f);

            PlayInteractionEffects("environmental_hide");
        }

        /// <summary>
        /// 諠・ｱ蜿朱寔螳溯｡・
        /// </summary>
        private void ExecutePickupIntel()
        {
            if (_targetObject == null) return;

            // 諠・ｱ繧｢繧､繝・Β縺ｮ蜿朱寔
            var collectible = _targetObject.GetComponent<ICollectible>();
            collectible?.Collect();

            // 諠・ｱ蜿朱寔螳御ｺ・ｾ後・繧ｪ繝悶ず繧ｧ繧ｯ繝磯勁蜴ｻ
            _targetObject.SetActive(false);

            PlayInteractionEffects("pickup_intel");
        }

        /// <summary>
        /// 逶ｸ莠剃ｽ懃畑繧ｨ繝輔ぉ繧ｯ繝医・繧ｵ繧ｦ繝ｳ繝牙・逕・
        /// </summary>
        private void PlayInteractionEffects(string effectType)
        {
            // 繝代・繝・ぅ繧ｯ繝ｫ繧ｨ繝輔ぉ繧ｯ繝亥・逕・
            // 繧ｵ繧ｦ繝ｳ繝峨お繝輔ぉ繧ｯ繝亥・逕滂ｼ・tealthService縺ｮ髻ｳ髻ｿ繧ｷ繧ｹ繝・Β騾｣蜍包ｼ・
            // UI繝輔ぅ繝ｼ繝峨ヰ繝・け陦ｨ遉ｺ

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[StealthInteractionCommand] Playing effects: {effectType}");
#endif
        }

        /// <summary>
        /// 繧ｳ繝槭Φ繝峨・蜿悶ｊ豸医＠・・ndo・・
        /// </summary>
        public void Undo()
        {
            if (!_isExecuted || !CanUndo)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[StealthInteractionCommand] Cannot undo {_interactionType}");
#endif
                return;
            }

            // 逶ｸ莠剃ｽ懃畑縺ｮ騾・・逅・ｮ溯｡・
            UndoSpecificInteraction();

            // 迥ｶ諷句ｾｩ蜈・
            _stealthService?.UpdatePlayerVisibility(_originalVisibility);

            _isExecuted = false;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[StealthInteractionCommand] Undoed {_interactionType}");
#endif
        }

        /// <summary>
        /// 逶ｸ莠剃ｽ懃畑繧ｿ繧､繝怜挨縺ｮUndo蜃ｦ逅・
        /// </summary>
        private void UndoSpecificInteraction()
        {
            switch (_interactionType)
            {
                case StealthInteractionType.DisableCamera:
                    // 繧ｫ繝｡繝ｩ縺ｮ蜀肴怏蜉ｹ蛹・
                    if (_targetObject != null)
                    {
                        var camera = _targetObject.GetComponent<UnityEngine.Camera>();
                        if (camera != null) camera.enabled = true;

                        var sensor = _targetObject.GetComponent<MonoBehaviour>();
                        if (sensor != null) sensor.enabled = true;
                    }
                    break;

                case StealthInteractionType.SabotageLight:
                    // 繝ｩ繧､繝医・蜀肴怏蜉ｹ蛹・
                    if (_targetObject != null)
                    {
                        var light = _targetObject.GetComponent<Light>();
                        if (light != null) light.enabled = true;
                    }
                    break;

                case StealthInteractionType.OperateDoor:
                    // 繝峨い縺ｮ蜀肴命骭
                    if (_targetObject != null)
                    {
                        var lockable = _targetObject.GetComponent<ILockable>();
                        lockable?.Lock();
                    }
                    break;

                case StealthInteractionType.HideBody:
                    // 豁ｻ菴薙・蜀崎｡ｨ遉ｺ
                    if (_targetObject != null)
                    {
                        _targetObject.SetActive(true);
                    }
                    break;

                case StealthInteractionType.ThrowObject:
                    // 謚墓憧繧ｪ繝悶ず繧ｧ繧ｯ繝医・蠕ｩ蜈・
                    if (_targetObject != null)
                    {
                        _targetObject.SetActive(true);
                    }
                    break;

                case StealthInteractionType.OperateSwitch:
                    // 繧ｹ繧､繝・メ縺ｮ蠕ｩ蜈・
                    if (_targetObject != null)
                    {
                        var switchComponent = _targetObject.GetComponent<Animator>();
                        if (switchComponent != null)
                        {
                            switchComponent.SetTrigger("Reset");
                        }
                    }
                    break;

                default:
                    // 縺昴・莉悶・逶ｸ莠剃ｽ懃畑縺ｮ蝓ｺ譛ｬ逧・↑蠕ｩ蜈・・逅・
                    break;
            }
        }

        /// <summary>
        /// ObjectPool逕ｨ迥ｶ諷九Μ繧ｻ繝・ヨ
        /// </summary>
        public void Reset()
        {
            _interactionType = StealthInteractionType.ThrowObject;
            _targetObject = null;
            _targetPosition = Vector3.zero;
            _interactionDuration = 1.0f;
            _requiresStealth = true;
            _interactionId = null;

            _stealthService = null;
            _isExecuted = false;
            _wasPlayerConcealed = false;
            _originalVisibility = 1.0f;
            _interactionData = default;
        }

        /// <summary>
        /// ObjectPool逕ｨ蛻晄悄蛹・
        /// </summary>
        public void Initialize(params object[] parameters)
        {
            if (parameters.Length < 2)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("[StealthInteractionCommand] Initialize requires at least 2 parameters: interactionType, targetObject");
#endif
                return;
            }

            if (parameters[0] is StealthInteractionType interactionType)
            {
                _interactionType = interactionType;
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("[StealthInteractionCommand] First parameter must be StealthInteractionType");
#endif
                return;
            }

            if (parameters[1] is GameObject targetObject)
            {
                _targetObject = targetObject;
                _targetPosition = targetObject?.transform.position ?? Vector3.zero;
            }
            else if (parameters[1] is Vector3 position)
            {
                _targetObject = null;
                _targetPosition = position;
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("[StealthInteractionCommand] Second parameter must be GameObject or Vector3");
#endif
                return;
            }

            // Optional parameters
            if (parameters.Length > 2 && parameters[2] is float duration)
            {
                _interactionDuration = duration;
            }

            if (parameters.Length > 3 && parameters[3] is bool requiresStealth)
            {
                _requiresStealth = requiresStealth;
            }

            _interactionId = System.Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 蝙句ｮ牙・縺ｪ蛻晄悄蛹悶Γ繧ｽ繝・ラ
        /// </summary>
        public void Initialize(StealthInteractionType interactionType, GameObject targetObject,
            float duration = 1.0f, bool requiresStealth = true)
        {
            _interactionType = interactionType;
            _targetObject = targetObject;
            _targetPosition = targetObject?.transform.position ?? Vector3.zero;
            _interactionDuration = duration;
            _requiresStealth = requiresStealth;
            _interactionId = System.Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 菴咲ｽｮ繝吶・繧ｹ蛻晄悄蛹悶Γ繧ｽ繝・ラ
        /// </summary>
        public void Initialize(StealthInteractionType interactionType, Vector3 targetPosition,
            float duration = 1.0f, bool requiresStealth = true)
        {
            _interactionType = interactionType;
            _targetObject = null;
            _targetPosition = targetPosition;
            _interactionDuration = duration;
            _requiresStealth = requiresStealth;
            _interactionId = System.Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 逶ｸ莠剃ｽ懃畑縺ｮ遞ｮ鬘槭↓蠢懊§縺滄ｨ帝浹繝ｬ繝吶Ν繧貞叙蠕・
        /// </summary>
        private float GetNoiseLevel(StealthInteractionType interactionType)
        {
            return interactionType switch
            {
                StealthInteractionType.DisableCamera => 0.1f,
                StealthInteractionType.SabotageLight => 0.3f,
                StealthInteractionType.ThrowObject => 0.8f,
                StealthInteractionType.HideBody => 0.2f,
                StealthInteractionType.OperateDoor => 0.4f,
                StealthInteractionType.OperateSwitch => 0.1f,
                _ => 0.2f
            };
        }

        /// <summary>
        /// 繝峨い縺ｮ髢矩哩謫堺ｽ懷ｮ溯｡・
        /// </summary>
        private void ExecuteOperateDoor()
        {
            if (_targetObject == null)
            {
                Debug.LogWarning("[StealthInteractionCommand] Cannot operate door - target object is null");
                return;
            }

            // 繝峨い謫堺ｽ懊・繝ｭ繧ｰ
            Debug.Log($"[StealthInteractionCommand] Operating door: {_targetObject.name}");

            // 繝峨い謫堺ｽ懊・髻ｳ繧堤函謌・
            _interactionData.GeneratedNoiseLevel = GetNoiseLevel(StealthInteractionType.OperateDoor);

            // 繝峨い繧ｳ繝ｳ繝昴・繝阪Φ繝医′縺ゅｋ蝣ｴ蜷医・謫堺ｽ・
            var doorComponent = _targetObject.GetComponent<Animator>();
            if (doorComponent != null)
            {
                doorComponent.SetTrigger("Operate");
            }

            _interactionData.Success = true;
        }

        /// <summary>
        /// 繧ｹ繧､繝・メ謫堺ｽ懷ｮ溯｡・
        /// </summary>
        private void ExecuteOperateSwitch()
        {
            if (_targetObject == null)
            {
                Debug.LogWarning("[StealthInteractionCommand] Cannot operate switch - target object is null");
                return;
            }

            // 繧ｹ繧､繝・メ謫堺ｽ懊・繝ｭ繧ｰ
            Debug.Log($"[StealthInteractionCommand] Operating switch: {_targetObject.name}");

            // 繧ｹ繧､繝・メ謫堺ｽ懊・髻ｳ繧堤函謌・
            _interactionData.GeneratedNoiseLevel = GetNoiseLevel(StealthInteractionType.OperateSwitch);

            // 繧ｹ繧､繝・メ繧ｳ繝ｳ繝昴・繝阪Φ繝医′縺ゅｋ蝣ｴ蜷医・謫堺ｽ・
            var switchComponent = _targetObject.GetComponent<Animator>();
            if (switchComponent != null)
            {
                switchComponent.SetTrigger("Switch");
            }

            _interactionData.Success = true;
        }
    }

    // 逶ｸ莠剃ｽ懃畑蜿ｯ閭ｽ繧ｪ繝悶ず繧ｧ繧ｯ繝医・繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ螳夂ｾｩ
    public interface ILockable
    {
        void Lock();
        void Unlock();
        bool IsLocked { get; }
    }

    public interface IAccessible
    {
        void Access();
        bool IsAccessible { get; }
    }

    public interface ICollectible
    {
        void Collect();
        bool IsCollected { get; }
    }
}


