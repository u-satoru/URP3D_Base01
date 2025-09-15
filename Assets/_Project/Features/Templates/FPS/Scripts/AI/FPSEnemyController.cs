using UnityEngine;
using UnityEngine.AI;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Features.Templates.FPS.Combat;
using System.Collections;

namespace asterivo.Unity60.Features.Templates.FPS.AI
{
    /// <summary>
    /// FPS専用エネミーAIコントローラー
    /// NavMeshAgent、戦闘AI、プレイヤー追跡を統合管理
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class FPSEnemyController : MonoBehaviour
    {
        [Header("AI Behavior Settings")]
        [SerializeField] private AIState currentState = AIState.Patrol;
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float attackRange = 5f;
        [SerializeField] private float fieldOfView = 60f;
        [SerializeField] private LayerMask playerLayerMask = 1;
        [SerializeField] private LayerMask obstacleLayerMask = 1;
        
        [Header("Combat Settings")]
        [SerializeField] private float damage = 25f;
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private float attackAnimationTime = 1f;
        
        [Header("Movement Settings")]
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float chaseSpeed = 4f;
        [SerializeField] private float patrolWaitTime = 3f;
        [SerializeField] private Transform[] patrolPoints;
        
        [Header("Events")]
        [SerializeField] private GameEvent onEnemySpotted;
        [SerializeField] private GameEvent onEnemyLostTarget;
        [SerializeField] private GameEvent onEnemyAttack;
        [SerializeField] private GameEvent onEnemyDeath;
        
        public enum AIState
        {
            Patrol,
            Chase,
            Attack,
            SearchLastKnownPosition,
            Dead
        }
        
        private NavMeshAgent navAgent;
        private FPSHealthSystem healthSystem;
        private Transform playerTransform;
        private Vector3 lastKnownPlayerPosition;
        private float lastAttackTime;
        private int currentPatrolIndex = 0;
        private bool isWaiting = false;
        private bool hasLineOfSight = false;
        
        // Properties
        public AIState CurrentState => currentState;
        public bool IsAlive => healthSystem != null && !healthSystem.IsDead;
        public float DistanceToPlayer => playerTransform != null ? Vector3.Distance(transform.position, playerTransform.position) : float.MaxValue;
        public bool HasLineOfSight => hasLineOfSight;
        
        private void Awake()
        {
            navAgent = GetComponent<NavMeshAgent>();
            healthSystem = GetComponent<FPSHealthSystem>();
            
            // プレイヤーを検索
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }
        
        private void Start()
        {
            Initialize();
        }
        
        private void Update()
        {
            if (!IsAlive) return;
            
            UpdateAIBehavior();
        }
        
        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            if (navAgent != null)
            {
                navAgent.speed = patrolSpeed;
                navAgent.stoppingDistance = 0.5f;
            }
            
            if (healthSystem != null)
            {
                // 死亡イベントはUpdateでポーリングして検知
            }
            
            // パトロール開始
            if (patrolPoints.Length > 0)
            {
                SetState(AIState.Patrol);
                StartPatrol();
            }
        }
        
        /// <summary>
        /// AI行動更新
        /// </summary>
        private void UpdateAIBehavior()
        {
            // 死亡状態のチェック
            if (healthSystem != null && healthSystem.IsDead && currentState != AIState.Dead)
            {
                OnEnemyDied();
                return;
            }

            // プレイヤー検知
            DetectPlayer();
            
            // 状態別処理
            switch (currentState)
            {
                case AIState.Patrol:
                    HandlePatrolState();
                    break;
                case AIState.Chase:
                    HandleChaseState();
                    break;
                case AIState.Attack:
                    HandleAttackState();
                    break;
                case AIState.SearchLastKnownPosition:
                    HandleSearchState();
                    break;
                case AIState.Dead:
                    // 何もしない
                    break;
            }
        }
        
        /// <summary>
        /// プレイヤー検知処理
        /// </summary>
        private void DetectPlayer()
        {
            if (playerTransform == null) return;
            
            float distanceToPlayer = DistanceToPlayer;
            
            // 検知範囲外の場合
            if (distanceToPlayer > detectionRange)
            {
                hasLineOfSight = false;
                return;
            }
            
            // プレイヤーの方向
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            
            // 視野角内かチェック
            if (angleToPlayer < fieldOfView / 2)
            {
                // 障害物チェック
                if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out RaycastHit hit, detectionRange, obstacleLayerMask))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        hasLineOfSight = true;
                        lastKnownPlayerPosition = playerTransform.position;
                        
                        // 状態遷移
                        if (currentState == AIState.Patrol)
                        {
                            SetState(AIState.Chase);
                            onEnemySpotted?.Raise();
                        }
                    }
                }
            }
            else
            {
                hasLineOfSight = false;
            }
        }
        
        /// <summary>
        /// パトロール状態処理
        /// </summary>
        private void HandlePatrolState()
        {
            if (patrolPoints.Length == 0) return;
            
            if (!isWaiting && (!navAgent.pathPending && navAgent.remainingDistance < 0.5f))
            {
                StartCoroutine(WaitAtPatrolPoint());
            }
        }
        
        /// <summary>
        /// 追跡状態処理
        /// </summary>
        private void HandleChaseState()
        {
            if (playerTransform == null) return;
            
            float distanceToPlayer = DistanceToPlayer;
            
            // 攻撃範囲内の場合
            if (distanceToPlayer <= attackRange && hasLineOfSight)
            {
                SetState(AIState.Attack);
                return;
            }
            
            // プレイヤーを見失った場合
            if (!hasLineOfSight)
            {
                SetState(AIState.SearchLastKnownPosition);
                return;
            }
            
            // プレイヤーを追跡
            navAgent.SetDestination(playerTransform.position);
        }
        
        /// <summary>
        /// 攻撃状態処理
        /// </summary>
        private void HandleAttackState()
        {
            if (playerTransform == null) return;
            
            float distanceToPlayer = DistanceToPlayer;
            
            // 攻撃範囲外に出た場合
            if (distanceToPlayer > attackRange || !hasLineOfSight)
            {
                SetState(AIState.Chase);
                return;
            }
            
            // プレイヤーの方向を向く
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(direction);
            
            // 攻撃実行
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                PerformAttack();
            }
        }
        
        /// <summary>
        /// 最後の位置検索状態処理
        /// </summary>
        private void HandleSearchState()
        {
            if (!navAgent.pathPending && navAgent.remainingDistance < 1f)
            {
                // 検索完了、パトロールに戻る
                SetState(AIState.Patrol);
                StartPatrol();
            }
        }
        
        /// <summary>
        /// 状態変更
        /// </summary>
        private void SetState(AIState newState)
        {
            if (currentState == newState) return;
            
            currentState = newState;
            
            // 状態に応じた初期化
            switch (newState)
            {
                case AIState.Patrol:
                    navAgent.speed = patrolSpeed;
                    break;
                case AIState.Chase:
                    navAgent.speed = chaseSpeed;
                    break;
                case AIState.Attack:
                    navAgent.isStopped = true;
                    break;
                case AIState.SearchLastKnownPosition:
                    navAgent.isStopped = false;
                    navAgent.speed = chaseSpeed;
                    navAgent.SetDestination(lastKnownPlayerPosition);
                    break;
            }
            
            Debug.Log($"[FPSEnemyController] {gameObject.name} state changed to: {newState}");
        }
        
        /// <summary>
        /// パトロール開始
        /// </summary>
        private void StartPatrol()
        {
            if (patrolPoints.Length == 0) return;
            
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            navAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
        
        /// <summary>
        /// パトロールポイントでの待機
        /// </summary>
        private IEnumerator WaitAtPatrolPoint()
        {
            isWaiting = true;
            yield return new WaitForSeconds(patrolWaitTime);
            
            if (currentState == AIState.Patrol)
            {
                StartPatrol();
            }
            
            isWaiting = false;
        }
        
        /// <summary>
        /// 攻撃実行
        /// </summary>
        private void PerformAttack()
        {
            lastAttackTime = Time.time;
            onEnemyAttack?.Raise();
            
            Debug.Log($"[FPSEnemyController] {gameObject.name} attacking player with {damage} damage");
            
            // プレイヤーにダメージを与える
            if (playerTransform != null)
            {
                var playerHealth = playerTransform.GetComponent<FPSHealthSystem>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
            }
        }
        
        /// <summary>
        /// 死亡時処理
        /// </summary>
        private void OnEnemyDied()
        {
            SetState(AIState.Dead);
            navAgent.isStopped = true;
            onEnemyDeath?.Raise();
            
            Debug.Log($"[FPSEnemyController] {gameObject.name} has died");
        }
        
        /// <summary>
        /// ダメージを受ける（外部から呼び出し可能）
        /// </summary>
        public void TakeDamage(float damageAmount)
        {
            if (healthSystem != null && IsAlive)
            {
                healthSystem.TakeDamage(damageAmount);
                
                // ダメージを受けたらプレイヤーを追跡開始
                if (currentState == AIState.Patrol && playerTransform != null)
                {
                    lastKnownPlayerPosition = playerTransform.position;
                    SetState(AIState.Chase);
                    onEnemySpotted?.Raise();
                }
            }
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // 検知範囲を表示
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // 攻撃範囲を表示
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            // 視野角を表示
            Vector3 forward = transform.forward;
            Vector3 left = Quaternion.Euler(0, -fieldOfView / 2, 0) * forward;
            Vector3 right = Quaternion.Euler(0, fieldOfView / 2, 0) * forward;
            
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, left * detectionRange);
            Gizmos.DrawRay(transform.position, right * detectionRange);
            
            // パトロールポイントを表示
            if (patrolPoints != null)
            {
                Gizmos.color = Color.green;
                foreach (var point in patrolPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawWireSphere(point.position, 0.5f);
                    }
                }
            }
        }
        #endif
    }
}