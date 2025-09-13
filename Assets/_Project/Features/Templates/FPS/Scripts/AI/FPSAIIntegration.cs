using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.AI.Visual;
using asterivo.Unity60.Features.AI.Audio;
using asterivo.Unity60.Features.AI.States;
using asterivo.Unity60.Features.Templates.FPS.Player;
using asterivo.Unity60.Features.Templates.FPS.Weapons;
using asterivo.Unity60.Core.Components;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.FPS.AI
{
    /// <summary>
    /// FPS AIシステム統合コンポーネント
    /// 既存のNPCVisualSensor、NPCAuditorySensor、AIStateMachineと
    /// FPSテンプレートの統合を管理
    /// プレイヤー検知、戦闘AI、警戒システムを統合
    /// </summary>
    public class FPSAIIntegration : MonoBehaviour, IHealthTarget
    {
        [TabGroup("AI Integration", "Target Setup")]
        [LabelText("FPS Player")]
        [SerializeField] private FPSPlayerController fpsPlayer;
        
        [LabelText("Player Transform")]
        [SerializeField] private Transform playerTransform;
        
        [TabGroup("AI Integration", "AI Components")]
        [LabelText("NPC Visual Sensor")]
        [SerializeField] private NPCVisualSensor visualSensor;
        
        [LabelText("NPC Auditory Sensor")]
        [SerializeField] private MonoBehaviour auditorySensor; // NPCAuditorySensorSensor;
        
        [LabelText("AI State Machine")]
        [SerializeField] private AIStateMachine aiStateMachine;
        
        [TabGroup("AI Integration", "Combat Settings")]
        [BoxGroup("AI Integration/Combat Settings/Health")]
        [LabelText("Max Health")]
        [PropertyRange(50f, 500f)]
        [SerializeField] private float maxHealth = 100f;
        
        [BoxGroup("AI Integration/Combat Settings/Health")]
        [LabelText("Current Health")]
        [ReadOnly]
        [ShowInInspector]
        [SerializeField] private float currentHealth;
        
        [BoxGroup("AI Integration/Combat Settings/Combat")]
        [LabelText("Combat Range")]
        [PropertyRange(5f, 30f)]
        [SerializeField] private float combatRange = 15f;
        
        [BoxGroup("AI Integration/Combat Settings/Combat")]
        [LabelText("Damage Per Shot")]
        [PropertyRange(5f, 50f)]
        [SerializeField] private float damagePerShot = 20f;
        
        [BoxGroup("AI Integration/Combat Settings/Combat")]
        [LabelText("Fire Rate")]
        [PropertyRange(0.5f, 3f)]
        [SerializeField] private float fireRate = 1.5f;
        
        [TabGroup("AI Integration", "Detection Settings")]
        [BoxGroup("AI Integration/Detection Settings/Sound")]
        [LabelText("Footstep Detection Range")]
        [PropertyRange(5f, 20f)]
        [SerializeField] private float footstepDetectionRange = 10f;
        
        [BoxGroup("AI Integration/Detection Settings/Sound")]
        [LabelText("Gunshot Detection Range")]
        [PropertyRange(20f, 100f)]
        [SerializeField] private float gunshotDetectionRange = 50f;
        
        [BoxGroup("AI Integration/Detection Settings/Visual")]
        [LabelText("Alert Level Threshold")]
        [PropertyRange(0.3f, 1f)]
        [SerializeField] private float alertLevelThreshold = 0.7f;
        
        [TabGroup("Events", "AI Events")]
        [LabelText("On Player Detected")]
        [SerializeField] private GameEvent onPlayerDetected;
        
        [LabelText("On Player Lost")]
        [SerializeField] private GameEvent onPlayerLost;
        
        [LabelText("On AI Death")]
        [SerializeField] private GameEvent onAIDeath;
        
        [LabelText("On Combat Started")]
        [SerializeField] private GameEvent onCombatStarted;
        
        // Private variables
        private bool isPlayerDetected;
        private bool isInCombat;
        private float lastShotTime;
        private Vector3 lastKnownPlayerPosition;
        private float detectionLevel;
        
        [TabGroup("Debug", "AI Status")]
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Current AI State")]
        private string currentAIState = "Idle";
        
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Detection Level")]
        private float debugDetectionLevel;
        
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Distance to Player")]
        private float debugDistanceToPlayer;
        
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Health Percentage")]
        private float debugHealthPercentage;
        
        private void Start()
        {
            InitializeAI();
            SetupEventListeners();
        }
        
        private void Update()
        {
            UpdateAIBehavior();
            UpdateCombat();
            UpdateDebugInfo();
        }
        
        private void InitializeAI()
        {
            // ヘルス初期化
            currentHealth = maxHealth;
            
            // プレイヤーの自動検出
            if (fpsPlayer == null)
            {
                fpsPlayer = FindFirstObjectByType<FPSPlayerController>();
            }
            
            if (playerTransform == null && fpsPlayer != null)
            {
                playerTransform = fpsPlayer.transform;
            }
            
            // AIコンポーネントの自動取得
            if (visualSensor == null)
                visualSensor = GetComponent<NPCVisualSensor>();
            
            if (auditorySensor == null)
                auditorySensor = GetComponent<MonoBehaviour>();
            
            if (aiStateMachine == null)
                aiStateMachine = GetComponent<AIStateMachine>();
            
            // Visual Sensorにプレイヤーを設定
            if (visualSensor != null && playerTransform != null)
            {
                // TODO: NPCVisualSensorにターゲット設定メソッドがあれば呼び出し
            }
        }
        
        private void SetupEventListeners()
        {
            // 武器発射音の検知
            // TODO: FPSPlayerControllerの武器発射イベントをリッスン
            
            // プレイヤー移動音の検知
            // TODO: FPSPlayerControllerの移動イベントをリッスン
        }
        
        private void UpdateAIBehavior()
        {
            if (playerTransform == null) return;
            
            // プレイヤーとの距離を計算
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            // Visual Sensorからの検知レベル取得
            UpdateDetectionLevel();
            
            // 検知状態の更新
            UpdateDetectionState(distanceToPlayer);
            
            // AI状態の更新
            UpdateAIState(distanceToPlayer);
        }
        
        private void UpdateDetectionLevel()
        {
            // NPCVisualSensorから検知レベルを取得
            if (visualSensor != null)
            {
                // TODO: NPCVisualSensorから実際の検知レベルを取得
                // 現在は距離ベースの仮実装
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                detectionLevel = Mathf.Clamp01(1f - (distance / 15f));
            }
        }
        
        private void UpdateDetectionState(float distanceToPlayer)
        {
            bool previouslyDetected = isPlayerDetected;
            
            // 検知判定
            isPlayerDetected = detectionLevel >= alertLevelThreshold;
            
            // 検知状態の変化をイベントで通知
            if (isPlayerDetected && !previouslyDetected)
            {
                lastKnownPlayerPosition = playerTransform.position;
                onPlayerDetected?.Raise();
                
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"FPSAIIntegration: Player detected by {gameObject.name}");
#endif
            }
            else if (!isPlayerDetected && previouslyDetected)
            {
                onPlayerLost?.Raise();
                
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"FPSAIIntegration: Player lost by {gameObject.name}");
#endif
            }
        }
        
        private void UpdateAIState(float distanceToPlayer)
        {
            if (aiStateMachine == null) return;
            
            string newState = "Idle";
            
            if (currentHealth <= 0)
            {
                newState = "Dead";
            }
            else if (isPlayerDetected)
            {
                if (distanceToPlayer <= combatRange)
                {
                    newState = "Combat";
                    if (!isInCombat)
                    {
                        StartCombat();
                    }
                }
                else
                {
                    newState = "Alert";
                }
            }
            else if (detectionLevel > 0.3f)
            {
                newState = "Suspicious";
            }
            
            // AI状態の変更
            if (currentAIState != newState)
            {
                currentAIState = newState;
                TransitionToAIState(newState);
            }
        }
        
        private void TransitionToAIState(string stateName)
        {
            // AIStateMachineの状態遷移
            switch (stateName)
            {
                case "Combat":
                    // TODO: aiStateMachine.TransitionTo(CombatState)
                    break;
                case "Alert":
                    // TODO: aiStateMachine.TransitionTo(AlertState)
                    break;
                case "Suspicious":
                    // TODO: aiStateMachine.TransitionTo(SuspiciousState)
                    break;
                case "Dead":
                    HandleDeath();
                    break;
                default:
                    // TODO: aiStateMachine.TransitionTo(IdleState)
                    break;
            }
        }
        
        private void StartCombat()
        {
            isInCombat = true;
            onCombatStarted?.Raise();
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"FPSAIIntegration: Combat started with {gameObject.name}");
#endif
        }
        
        private void UpdateCombat()
        {
            if (!isInCombat || playerTransform == null) return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            // 戦闘範囲外なら戦闘終了
            if (distanceToPlayer > combatRange * 1.5f)
            {
                isInCombat = false;
                return;
            }
            
            // 射撃処理
            if (Time.time - lastShotTime >= fireRate)
            {
                PerformShoot();
                lastShotTime = Time.time;
            }
        }
        
        private void PerformShoot()
        {
            if (playerTransform == null) return;
            
            // プレイヤー方向への射撃
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            
            // レイキャストで命中判定
            if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, combatRange))
            {
                if (hit.collider.GetComponent<FPSPlayerController>() != null)
                {
                    // プレイヤーにダメージを与える
                    var healthTarget = hit.collider.GetComponent<IHealthTarget>();
                    if (healthTarget != null)
                    {
                        healthTarget.TakeDamage(Mathf.RoundToInt(damagePerShot), "ballistic");
                    }
                }
            }
            
            // 射撃エフェクトや音声の再生
            // TODO: 射撃エフェクトの実装
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.DrawLine(transform.position, transform.position + directionToPlayer * combatRange, Color.red, 0.1f);
#endif
        }
        
        public void OnWeaponFired(Vector3 position, float volume)
        {
            // 武器発射音の検知処理
            float distance = Vector3.Distance(transform.position, position);
            if (distance <= gunshotDetectionRange)
            {
                // 警戒レベル上昇
                detectionLevel = Mathf.Min(1f, detectionLevel + 0.4f);
                lastKnownPlayerPosition = position;
                
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"FPSAIIntegration: Gunshot heard by {gameObject.name} at distance {distance:F1}m");
#endif
            }
        }
        
        public void OnPlayerMovement(Vector3 position, float noiseLevel)
        {
            // プレイヤー移動音の検知処理
            float distance = Vector3.Distance(transform.position, position);
            if (distance <= footstepDetectionRange)
            {
                // 移動音に基づく警戒レベル上昇（銃声より小さい）
                detectionLevel = Mathf.Min(1f, detectionLevel + noiseLevel * 0.1f);
                
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"FPSAIIntegration: Movement heard by {gameObject.name}");
#endif
            }
        }
        
        // IHealthTarget implementation
        public void TakeDamage(int damage)
        {
            TakeDamage(damage, "physical");
        }
        
        public void TakeDamage(int damage, string damageType = "physical")
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            
            // ダメージを受けたら即座に警戒状態に
            detectionLevel = 1f;
            isPlayerDetected = true;
            lastKnownPlayerPosition = playerTransform != null ? playerTransform.position : transform.position;
            
            if (currentHealth <= 0)
            {
                HandleDeath();
            }
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"FPSAIIntegration: {gameObject.name} took {damage} {damageType} damage. Health: {currentHealth}/{maxHealth}");
#endif
        }
        
        public void Heal(int healAmount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"FPSAIIntegration: {gameObject.name} healed {healAmount}. Health: {currentHealth}/{maxHealth}");
#endif
        }
        
        private void HandleDeath()
        {
            currentAIState = "Dead";
            isInCombat = false;
            isPlayerDetected = false;
            
            onAIDeath?.Raise();
            
            // AIの無効化
            if (aiStateMachine != null)
                aiStateMachine.enabled = false;
            
            if (visualSensor != null)
                visualSensor.enabled = false;
            
            if (auditorySensor != null)
                auditorySensor.enabled = false;
            
            // 死亡アニメーション・エフェクト
            // TODO: 死亡時の処理実装
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"FPSAIIntegration: {gameObject.name} died");
#endif
        }
        
        private void UpdateDebugInfo()
        {
            debugDetectionLevel = detectionLevel;
            debugHealthPercentage = (currentHealth / maxHealth) * 100f;
            
            if (playerTransform != null)
            {
                debugDistanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            }
        }
        
        // 外部呼び出し用メソッド
        public bool IsPlayerDetected => isPlayerDetected;
        public bool IsInCombat => isInCombat;
        public int CurrentHealth => Mathf.RoundToInt(currentHealth);
        public int MaxHealth => Mathf.RoundToInt(maxHealth);
        public float HealthPercentage => currentHealth / maxHealth;
        public Vector3 LastKnownPlayerPosition => lastKnownPlayerPosition;
        
        private void OnDrawGizmosSelected()
        {
            // 戦闘範囲の可視化
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, combatRange);
            
            // 足音検知範囲
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, footstepDetectionRange);
            
            // 銃声検知範囲
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, gunshotDetectionRange);
            
            // 最後の既知プレイヤー位置
            if (isPlayerDetected)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(lastKnownPlayerPosition, 0.5f);
                Gizmos.DrawLine(transform.position, lastKnownPlayerPosition);
            }
        }
    }
}