using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Stealth.Services;
using asterivo.Unity60.Features.Templates.Stealth.Events;
using StealthInteractionType = asterivo.Unity60.Features.Templates.Stealth.Services.StealthInteractionType;

namespace asterivo.Unity60.Features.Templates.Stealth.Commands
{
    /// <summary>
    /// ステルス環境相互作用コマンド
    /// ステルスゲームに特化した環境オブジェクトとの相互作用を管理
    /// ServiceLocator統合によるStealthServiceとの連携
    /// ObjectPool最適化対応
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
        /// Undo操作サポート状況
        /// ステルス相互作用は基本的にUndoをサポート
        /// </summary>
        public bool CanUndo => _isExecuted && _interactionType != StealthInteractionType.HideBody;

        /// <summary>
        /// デフォルトコンストラクタ（ObjectPool対応）
        /// </summary>
        public StealthInteractionCommand()
        {
            // プール化対応のため空のコンストラクタ
        }

        /// <summary>
        /// パラメータ付きコンストラクタ
        /// </summary>
        /// <param name="interactionType">相互作用の種類</param>
        /// <param name="targetObject">対象オブジェクト</param>
        /// <param name="duration">実行時間</param>
        /// <param name="requiresStealth">ステルス状態要求</param>
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
        /// コマンド実行
        /// StealthServiceと連携してステルス相互作用を実行
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

            // ServiceLocator経由でStealthServiceを取得
            _stealthService = ServiceLocator.GetService<IStealthService>();
            if (_stealthService == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("[StealthInteractionCommand] StealthService not found in ServiceLocator");
#endif
                return;
            }

            // ステルス状態の事前チェック
            if (_requiresStealth && !ValidateStealthState())
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[StealthInteractionCommand] Stealth requirements not met for {_interactionType}");
#endif
                return;
            }

            // 実行前の状態を保存
            _wasPlayerConcealed = _stealthService.IsPlayerConcealed;
            _originalVisibility = _stealthService.PlayerVisibilityFactor;

            // 相互作用データの準備
            _interactionData = new EnvironmentalInteractionData
            {
                TargetObject = _targetObject,
                InteractionType = _interactionType,
                Success = false, // 実行後にtrueに設定
                Position = _targetPosition,
                GeneratedNoiseLevel = 0.0f, // 相互作用の種類に応じて設定
                Timestamp = Time.time
            };

            // 相互作用の種類に応じた実行
            ExecuteSpecificInteraction();

            // 実行完了後の状態更新
            _interactionData.Success = true;
            _interactionData.GeneratedNoiseLevel = GetNoiseLevel(_interactionType);

            _isExecuted = true;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[StealthInteractionCommand] Executed {_interactionType} on {_targetObject?.name ?? "position"}");
#endif
        }

        /// <summary>
        /// ステルス状態の検証
        /// </summary>
        private bool ValidateStealthState()
        {
            if (_stealthService == null) return false;

            // 基本的なステルス要件チェック
            if (_requiresStealth && !_stealthService.IsStealthModeActive)
                return false;

            // 相互作用タイプ別の特殊要件
            switch (_interactionType)
            {
                case StealthInteractionType.HideBody:
                    // 死体隠匿は完全に隠蔽されている必要がある
                    return _stealthService.IsPlayerConcealed && _stealthService.PlayerVisibilityFactor < 0.3f;

                case StealthInteractionType.OperateDoor:
                case StealthInteractionType.OperateSwitch:
                    // 操作は中程度の隠蔽で十分
                    return _stealthService.PlayerVisibilityFactor < 0.7f;

                case StealthInteractionType.DisableCamera:
                case StealthInteractionType.SabotageLight:
                    // 設備破壊は比較的寛容
                    return _stealthService.PlayerVisibilityFactor < 0.8f;

                case StealthInteractionType.ThrowObject:
                    // 陽動は特別な要件なし
                    return true;

                default:
                    return _stealthService.PlayerVisibilityFactor < 0.5f;
            }
        }

        /// <summary>
        /// 相互作用タイプ別の具体的実行処理
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

            // StealthServiceを通じてイベント発行
            _stealthService?.InteractWithEnvironment(_targetObject, _interactionType);
        }

        /// <summary>
        /// 監視カメラ無効化実行
        /// </summary>
        private void ExecuteDisableCamera()
        {
            if (_targetObject == null) return;

            // カメラコンポーネントの無効化
            var camera = _targetObject.GetComponent<UnityEngine.Camera>();
            if (camera != null)
            {
                camera.enabled = false;
            }

            // ステルス検知システムの無効化
            var detectionSensor = _targetObject.GetComponent<MonoBehaviour>();
            if (detectionSensor != null)
            {
                detectionSensor.enabled = false;
            }

            // 一時的な視認性向上（作業中のリスク）
            _stealthService?.UpdatePlayerVisibility(_originalVisibility + 0.3f);

            // エフェクト・サウンド再生
            PlayInteractionEffects("camera_disable");
        }

        /// <summary>
        /// 照明無効化実行
        /// </summary>
        private void ExecuteDisableLight()
        {
            if (_targetObject == null) return;

            // ライトコンポーネントの無効化
            var light = _targetObject.GetComponent<Light>();
            if (light != null)
            {
                light.enabled = false;
            }

            // 周囲の暗闇化による隠蔽性向上
            _stealthService?.UpdatePlayerVisibility(_originalVisibility * 0.7f);

            PlayInteractionEffects("light_disable");
        }

        /// <summary>
        /// 陽動作成実行
        /// </summary>
        private void ExecuteCreateDistraction()
        {
            // 陽動音の作成
            _stealthService?.CreateDistraction(_targetPosition, 0.8f);

            // プレイヤーの一時的隠蔽性向上（注意がそれるため）
            _stealthService?.UpdatePlayerVisibility(_originalVisibility * 0.8f);

            PlayInteractionEffects("distraction_create");
        }

        /// <summary>
        /// 鍵開け実行
        /// </summary>
        private void ExecuteLockPicking()
        {
            if (_targetObject == null) return;

            // ドアの解錠状態変更
            var lockable = _targetObject.GetComponent<ILockable>();
            lockable?.Unlock();

            // 集中による一時的視認性増加
            _stealthService?.UpdatePlayerVisibility(_originalVisibility + 0.2f);

            PlayInteractionEffects("lock_picking");
        }

        /// <summary>
        /// 無音制圧実行
        /// </summary>
        private void ExecuteSilentTakedown()
        {
            if (_targetObject == null) return;

            // 対象NPCの無力化
            var npcHealth = _targetObject.GetComponent<MonoBehaviour>();
            if (npcHealth != null)
            {
                // NPCの状態を無力化に変更
                npcHealth.enabled = false;
            }

            // 制圧後の隠蔽性向上
            _stealthService?.UpdatePlayerVisibility(_originalVisibility * 0.5f);

            PlayInteractionEffects("silent_takedown");
        }

        /// <summary>
        /// 死体隠匿実行
        /// </summary>
        private void ExecuteHideBody()
        {
            if (_targetObject == null) return;

            // 死体オブジェクトの非表示化
            _targetObject.SetActive(false);

            // 証拠隠滅による隠蔽性向上
            _stealthService?.UpdatePlayerVisibility(_originalVisibility * 0.9f);

            PlayInteractionEffects("hide_body");
        }

        /// <summary>
        /// 警報装置破壊実行
        /// </summary>
        private void ExecuteSabotageAlarm()
        {
            if (_targetObject == null) return;

            // 警報システムの無効化
            var alarmSystem = _targetObject.GetComponent<MonoBehaviour>();
            if (alarmSystem != null)
            {
                alarmSystem.enabled = false;
            }

            PlayInteractionEffects("sabotage_alarm");
        }

        /// <summary>
        /// 端末アクセス実行
        /// </summary>
        private void ExecuteAccessTerminal()
        {
            if (_targetObject == null) return;

            // 端末インターフェースのアクティベート
            var terminal = _targetObject.GetComponent<IAccessible>();
            terminal?.Access();

            // 集中による視認性増加
            _stealthService?.UpdatePlayerVisibility(_originalVisibility + 0.4f);

            PlayInteractionEffects("terminal_access");
        }

        /// <summary>
        /// 環境隠蔽利用実行
        /// </summary>
        private void ExecuteEnvironmentalHide()
        {
            // 環境隠蔽ゾーンへの移動・利用
            _stealthService?.UpdatePlayerVisibility(_originalVisibility * 0.3f);

            PlayInteractionEffects("environmental_hide");
        }

        /// <summary>
        /// 情報収集実行
        /// </summary>
        private void ExecutePickupIntel()
        {
            if (_targetObject == null) return;

            // 情報アイテムの収集
            var collectible = _targetObject.GetComponent<ICollectible>();
            collectible?.Collect();

            // 情報収集完了後のオブジェクト除去
            _targetObject.SetActive(false);

            PlayInteractionEffects("pickup_intel");
        }

        /// <summary>
        /// 相互作用エフェクト・サウンド再生
        /// </summary>
        private void PlayInteractionEffects(string effectType)
        {
            // パーティクルエフェクト再生
            // サウンドエフェクト再生（StealthServiceの音響システム連動）
            // UIフィードバック表示

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[StealthInteractionCommand] Playing effects: {effectType}");
#endif
        }

        /// <summary>
        /// コマンドの取り消し（Undo）
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

            // 相互作用の逆処理実行
            UndoSpecificInteraction();

            // 状態復元
            _stealthService?.UpdatePlayerVisibility(_originalVisibility);

            _isExecuted = false;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[StealthInteractionCommand] Undoed {_interactionType}");
#endif
        }

        /// <summary>
        /// 相互作用タイプ別のUndo処理
        /// </summary>
        private void UndoSpecificInteraction()
        {
            switch (_interactionType)
            {
                case StealthInteractionType.DisableCamera:
                    // カメラの再有効化
                    if (_targetObject != null)
                    {
                        var camera = _targetObject.GetComponent<UnityEngine.Camera>();
                        if (camera != null) camera.enabled = true;

                        var sensor = _targetObject.GetComponent<MonoBehaviour>();
                        if (sensor != null) sensor.enabled = true;
                    }
                    break;

                case StealthInteractionType.SabotageLight:
                    // ライトの再有効化
                    if (_targetObject != null)
                    {
                        var light = _targetObject.GetComponent<Light>();
                        if (light != null) light.enabled = true;
                    }
                    break;

                case StealthInteractionType.OperateDoor:
                    // ドアの再施錠
                    if (_targetObject != null)
                    {
                        var lockable = _targetObject.GetComponent<ILockable>();
                        lockable?.Lock();
                    }
                    break;

                case StealthInteractionType.HideBody:
                    // 死体の再表示
                    if (_targetObject != null)
                    {
                        _targetObject.SetActive(true);
                    }
                    break;

                case StealthInteractionType.ThrowObject:
                    // 投擲オブジェクトの復元
                    if (_targetObject != null)
                    {
                        _targetObject.SetActive(true);
                    }
                    break;

                case StealthInteractionType.OperateSwitch:
                    // スイッチの復元
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
                    // その他の相互作用の基本的な復元処理
                    break;
            }
        }

        /// <summary>
        /// ObjectPool用状態リセット
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
        /// ObjectPool用初期化
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
        /// 型安全な初期化メソッド
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
        /// 位置ベース初期化メソッド
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
        /// 相互作用の種類に応じた騒音レベルを取得
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
        /// ドアの開閉操作実行
        /// </summary>
        private void ExecuteOperateDoor()
        {
            if (_targetObject == null)
            {
                Debug.LogWarning("[StealthInteractionCommand] Cannot operate door - target object is null");
                return;
            }

            // ドア操作のログ
            Debug.Log($"[StealthInteractionCommand] Operating door: {_targetObject.name}");

            // ドア操作の音を生成
            _interactionData.GeneratedNoiseLevel = GetNoiseLevel(StealthInteractionType.OperateDoor);

            // ドアコンポーネントがある場合は操作
            var doorComponent = _targetObject.GetComponent<Animator>();
            if (doorComponent != null)
            {
                doorComponent.SetTrigger("Operate");
            }

            _interactionData.Success = true;
        }

        /// <summary>
        /// スイッチ操作実行
        /// </summary>
        private void ExecuteOperateSwitch()
        {
            if (_targetObject == null)
            {
                Debug.LogWarning("[StealthInteractionCommand] Cannot operate switch - target object is null");
                return;
            }

            // スイッチ操作のログ
            Debug.Log($"[StealthInteractionCommand] Operating switch: {_targetObject.name}");

            // スイッチ操作の音を生成
            _interactionData.GeneratedNoiseLevel = GetNoiseLevel(StealthInteractionType.OperateSwitch);

            // スイッチコンポーネントがある場合は操作
            var switchComponent = _targetObject.GetComponent<Animator>();
            if (switchComponent != null)
            {
                switchComponent.SetTrigger("Switch");
            }

            _interactionData.Success = true;
        }
    }

    // 相互作用可能オブジェクトのインターフェース定義
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