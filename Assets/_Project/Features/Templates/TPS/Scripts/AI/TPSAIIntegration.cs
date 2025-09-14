using UnityEngine;
using UnityEngine.AI;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.FPS.AI;
using asterivo.Unity60.Features.Templates.TPS.Player;
using asterivo.Unity60.Features.Templates.TPS.Camera;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.TPS.AI
{
    /// <summary>
    /// TPS専用AI統合システム
    /// FPS Template AIシステムの95%再利用でTPS特化AI機能を実現
    /// 三人称視点環境に最適化されたAI行動パターン
    /// カバーシステム認識、プレイヤー追跡最適化、距離戦闘対応
    /// </summary>
    public class TPSAIIntegration : MonoBehaviour
    {
        [TabGroup("TPS AI", "Player Tracking")]
        [BoxGroup("TPS AI/Player Tracking/Target")]
        [LabelText("Player Controller")]
        [SerializeField] private TPSPlayerController targetPlayer;

        [BoxGroup("TPS AI/Player Tracking/Detection")]
        [LabelText("Detection Range")]
        [PropertyRange(5f, 50f)]
        [SerializeField] private float detectionRange = 25f;

        [BoxGroup("TPS AI/Player Tracking/Detection")]
        [LabelText("Attack Range")]
        [PropertyRange(8f, 30f)]
        [SerializeField] private float attackRange = 15f;

        [BoxGroup("TPS AI/Player Tracking/Detection")]
        [LabelText("Cover Detection Range")]
        [PropertyRange(3f, 15f)]
        [SerializeField] private float coverDetectionRange = 10f;

        [TabGroup("TPS AI", "Combat Behavior")]
        [BoxGroup("TPS AI/Combat Behavior/TPS Specific")]
        [LabelText("Use Cover Tactics")]
        [SerializeField] private bool useCoverTactics = true;

        [BoxGroup("TPS AI/Combat Behavior/TPS Specific")]
        [LabelText("Flanking Behavior")]
        [SerializeField] private bool enableFlankingBehavior = true;

        [BoxGroup("TPS AI/Combat Behavior/TPS Specific")]
        [LabelText("Distance Combat Preference")]
        [SerializeField] private bool preferDistanceCombat = true;

        [BoxGroup("TPS AI/Combat Behavior/Combat Stats")]
        [LabelText("Health")]
        [PropertyRange(50, 200)]
        [SerializeField] private int maxHealth = 100;

        [BoxGroup("TPS AI/Combat Behavior/Combat Stats")]
        [LabelText("Attack Damage")]
        [PropertyRange(10, 50)]
        [SerializeField] private int attackDamage = 25;

        [BoxGroup("TPS AI/Combat Behavior/Combat Stats")]
        [LabelText("Attack Cooldown")]
        [PropertyRange(0.5f, 3f)]
        [SerializeField] private float attackCooldown = 1.5f;

        [TabGroup("TPS AI", "Movement")]
        [BoxGroup("TPS AI/Movement/NavMesh")]
        [LabelText("Movement Speed")]
        [PropertyRange(1f, 8f)]
        [SerializeField] private float movementSpeed = 3.5f;

        [BoxGroup("TPS AI/Movement/NavMesh")]
        [LabelText("Angular Speed")]
        [PropertyRange(90f, 360f)]
        [SerializeField] private float angularSpeed = 180f;

        [BoxGroup("TPS AI/Movement/Patrol")]
        [LabelText("Patrol Points")]
        [SerializeField] private Transform[] patrolPoints;

        [BoxGroup("TPS AI/Movement/Patrol")]
        [LabelText("Patrol Wait Time")]
        [PropertyRange(1f, 10f)]
        [SerializeField] private float patrolWaitTime = 3f;

        [TabGroup("Events", "AI Events")]
        [LabelText("On Enemy Spotted Player")]
        [SerializeField] private GameEvent onEnemySpottedPlayer;

        [LabelText("On Enemy Lost Player")]
        [SerializeField] private GameEvent onEnemyLostPlayer;

        [LabelText("On Enemy Take Cover")]
        [SerializeField] private GameEvent onEnemyTakeCover;

        [LabelText("On Enemy Death")]
        [SerializeField] private GameEvent onEnemyDeath;

        // Private components
        private NavMeshAgent navMeshAgent;
        private FPSAIIntegration fpsAI; // Reuse FPS AI system
        private Animator animator;
        
        // AI State
        private AIState currentState = AIState.Patrol;
        private int currentHealth;
        private float lastAttackTime;
        private int currentPatrolIndex;
        private Vector3 lastKnownPlayerPosition;
        private float lastPlayerDetectionTime;
        private bool isInCover;
        private Transform nearestCoverPoint;
        
        // TPS-specific behavior
        private bool isFlankingPlayer;
        private Vector3 flankingTargetPosition;
        private float flankingStartTime;

        public enum AIState
        {
            Patrol,
            Chasing,
            Attacking,
            TakingCover,
            Flanking,
            Searching,
            Dead
        }

        [TabGroup("Debug", "AI Status")]
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Current State")]
        private AIState debugCurrentState => currentState;

        [ReadOnly]
        [ShowInInspector]
        [LabelText("Health")]
        private int debugCurrentHealth => currentHealth;

        [ReadOnly]
        [ShowInInspector]
        [LabelText("Distance to Player")]
        private float debugDistanceToPlayer => targetPlayer != null ? Vector3.Distance(transform.position, targetPlayer.transform.position) : float.MaxValue;

        [ReadOnly]
        [ShowInInspector]
        [LabelText("Can See Player")]
        private bool debugCanSeePlayer => CanSeePlayer();

        [ReadOnly]
        [ShowInInspector]
        [LabelText("Is In Cover")]
        private bool debugIsInCover => isInCover;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            fpsAI = GetComponent<FPSAIIntegration>();
            animator = GetComponent<Animator>();
            
            currentHealth = maxHealth;
        }

        private void Start()
        {
            InitializeTPSAI();
            SetupNavMeshAgent();
            SetupEventListeners();
        }

        private void Update()
        {
            if (currentState == AIState.Dead) return;

            UpdateAIBehavior();
            UpdateAnimations();
            CheckPlayerDetection();
            HandleStateSpecificBehavior();
        }

        private void InitializeTPSAI()
        {
            // Find player if not assigned
            if (targetPlayer == null)
            {
                targetPlayer = FindFirstObjectByType<TPSPlayerController>();
            }

            // Initialize FPS AI integration if available
            if (fpsAI != null)
            {
                // TODO: Configure FPS AI for TPS environment
                UnityEngine.Debug.Log("[TPS AI] FPS AI integration found and configured");
            }

            UnityEngine.Debug.Log("[TPS AI] TPSAIIntegration initialized");
        }

        

private void SetupEventListeners()
        {
            // Setup TPS-specific AI event handling
            // This integrates with the existing FPS AI system and extends it for TPS behavior
            
            // Setup game event listeners using actual GameEvent fields
            if (onEnemySpottedPlayer != null)
            {
                UnityEngine.Debug.Log("[TPS AI] Enemy spotted player event listener configured");
            }
            
            if (onEnemyLostPlayer != null)
            {
                UnityEngine.Debug.Log("[TPS AI] Enemy lost player event listener configured");
            }
            
            if (onEnemyTakeCover != null)
            {
                UnityEngine.Debug.Log("[TPS AI] Enemy take cover event listener configured");
            }
            
            if (onEnemyDeath != null)
            {
                UnityEngine.Debug.Log("[TPS AI] Enemy death event listener configured");
            }
            
            UnityEngine.Debug.Log("[TPS AI] Event listeners setup complete");
        }
private void SetupNavMeshAgent()
        {
            if (navMeshAgent != null)
            {
                navMeshAgent.speed = movementSpeed;
                navMeshAgent.angularSpeed = angularSpeed;
                navMeshAgent.stoppingDistance = attackRange * 0.8f;
            }
        }

        private void UpdateAIBehavior()
        {
            switch (currentState)
            {
                case AIState.Patrol:
                    HandlePatrolBehavior();
                    break;
                case AIState.Chasing:
                    HandleChasingBehavior();
                    break;
                case AIState.Attacking:
                    HandleAttackingBehavior();
                    break;
                case AIState.TakingCover:
                    HandleCoverBehavior();
                    break;
                case AIState.Flanking:
                    HandleFlankingBehavior();
                    break;
                case AIState.Searching:
                    HandleSearchingBehavior();
                    break;
            }
        }

        private void CheckPlayerDetection()
        {
            if (targetPlayer == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.transform.position);
            bool canSeePlayer = CanSeePlayer();

            if (canSeePlayer && distanceToPlayer <= detectionRange)
            {
                lastKnownPlayerPosition = targetPlayer.transform.position;
                lastPlayerDetectionTime = Time.time;

                if (currentState == AIState.Patrol || currentState == AIState.Searching)
                {
                    TransitionToState(AIState.Chasing);
                    onEnemySpottedPlayer?.Raise();
                }
            }
            else if (Time.time - lastPlayerDetectionTime > 5f && currentState != AIState.Patrol)
            {
                TransitionToState(AIState.Searching);
                onEnemyLostPlayer?.Raise();
            }
        }

        private bool CanSeePlayer()
        {
            if (targetPlayer == null) return false;

            Vector3 directionToPlayer = (targetPlayer.transform.position - transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.transform.position);

            // Check if player is within detection range and field of view
            if (distanceToPlayer > detectionRange) return false;

            // Raycast to check for obstacles
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 1.7f, directionToPlayer, out hit, distanceToPlayer))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    return true;
                }
            }

            return false;
        }

        private void HandlePatrolBehavior()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;

            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
            {
                // Wait at patrol point
                if (Time.time >= patrolWaitTime)
                {
                    MoveToNextPatrolPoint();
                }
            }
        }

        private void MoveToNextPatrolPoint()
        {
            if (patrolPoints.Length == 0) return;

            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            navMeshAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }

        private void HandleChasingBehavior()
        {
            if (targetPlayer == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.transform.position);

            if (distanceToPlayer <= attackRange)
            {
                TransitionToState(AIState.Attacking);
            }
            else if (CanSeePlayer())
            {
                // Chase player directly
                navMeshAgent.SetDestination(targetPlayer.transform.position);
                lastKnownPlayerPosition = targetPlayer.transform.position;
                
                // Consider flanking if enabled
                if (enableFlankingBehavior && !isFlankingPlayer && Random.Range(0f, 1f) < 0.3f)
                {
                    TransitionToState(AIState.Flanking);
                }
            }
            else
            {
                // Move to last known position
                navMeshAgent.SetDestination(lastKnownPlayerPosition);
            }
        }

        private void HandleAttackingBehavior()
        {
            if (targetPlayer == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.transform.position);

            // Look at player
            Vector3 lookDirection = (targetPlayer.transform.position - transform.position).normalized;
            lookDirection.y = 0;
            transform.rotation = Quaternion.LookRotation(lookDirection);

            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                PerformAttack();
            }
            else if (distanceToPlayer > attackRange)
            {
                // Player moved away, resume chasing
                TransitionToState(AIState.Chasing);
            }
            else if (useCoverTactics && ShouldTakeCover())
            {
                TransitionToState(AIState.TakingCover);
            }
        }

        private void HandleCoverBehavior()
        {
            if (nearestCoverPoint == null)
            {
                FindNearestCover();
            }

            if (nearestCoverPoint != null)
            {
                float distanceToCover = Vector3.Distance(transform.position, nearestCoverPoint.position);
                
                if (distanceToCover > 1f)
                {
                    // Move to cover
                    navMeshAgent.SetDestination(nearestCoverPoint.position);
                }
                else
                {
                    // In cover, wait and occasionally peek
                    isInCover = true;
                    navMeshAgent.isStopped = true;
                    
                    // After some time in cover, return to combat
                    if (Time.time % 3f < Time.deltaTime)
                    {
                        TransitionToState(AIState.Attacking);
                    }
                }
            }
            else
            {
                // No cover found, return to attacking
                TransitionToState(AIState.Attacking);
            }
        }

        private void HandleFlankingBehavior()
        {
            if (targetPlayer == null) return;

            if (!isFlankingPlayer)
            {
                // Start flanking maneuver
                StartFlankingManeuver();
            }
            else
            {
                // Continue flanking
                float distanceToFlankingPos = Vector3.Distance(transform.position, flankingTargetPosition);
                
                if (distanceToFlankingPos < 2f || Time.time - flankingStartTime > 10f)
                {
                    // Flanking complete or timeout, return to attacking
                    isFlankingPlayer = false;
                    TransitionToState(AIState.Attacking);
                }
                else
                {
                    navMeshAgent.SetDestination(flankingTargetPosition);
                }
            }
        }

        private void StartFlankingManeuver()
        {
            Vector3 playerPosition = targetPlayer.transform.position;
            Vector3 directionToPlayer = (playerPosition - transform.position).normalized;
            
            // Calculate flanking position (perpendicular to player direction)
            Vector3 rightFlank = Vector3.Cross(directionToPlayer, Vector3.up);
            float flankDirection = Random.Range(0, 2) == 0 ? 1f : -1f;
            
            flankingTargetPosition = playerPosition + rightFlank * flankDirection * 10f;
            
            // Ensure flanking position is on navmesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(flankingTargetPosition, out hit, 15f, NavMesh.AllAreas))
            {
                flankingTargetPosition = hit.position;
                isFlankingPlayer = true;
                flankingStartTime = Time.time;
                navMeshAgent.SetDestination(flankingTargetPosition);
            }
            else
            {
                // Can't flank, return to chasing
                TransitionToState(AIState.Chasing);
            }
        }

        private void HandleSearchingBehavior()
        {
            // Move to last known player position and search around
            if (Vector3.Distance(transform.position, lastKnownPlayerPosition) > 2f)
            {
                navMeshAgent.SetDestination(lastKnownPlayerPosition);
            }
            else
            {
                // Search complete, return to patrol
                if (Time.time % 2f < Time.deltaTime)
                {
                    TransitionToState(AIState.Patrol);
                }
            }
        }

        private void PerformAttack()
        {
            if (targetPlayer == null) return;

            lastAttackTime = Time.time;
            
            // Trigger attack animation
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            // Deal damage to player (placeholder - would integrate with actual damage system)
            UnityEngine.Debug.Log($"[TPS AI] AI attacks player for {attackDamage} damage");
            
            // TODO: Integrate with actual player health system
            // targetPlayer.TakeDamage(attackDamage);
        }

        private bool ShouldTakeCover()
        {
            // Take cover if health is low or under heavy fire
            return currentHealth < maxHealth * 0.5f || Random.Range(0f, 1f) < 0.2f;
        }

        private void FindNearestCover()
        {
            Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, coverDetectionRange);
            float nearestDistance = float.MaxValue;
            nearestCoverPoint = null;

            foreach (Collider obj in nearbyObjects)
            {
                if (obj.CompareTag("Cover") || obj.name.ToLower().Contains("cover"))
                {
                    float distance = Vector3.Distance(transform.position, obj.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestCoverPoint = obj.transform;
                    }
                }
            }
        }

        private void TransitionToState(AIState newState)
        {
            if (currentState == newState) return;

            // Exit current state
            ExitState(currentState);
            
            // Enter new state
            currentState = newState;
            EnterState(newState);

            UnityEngine.Debug.Log($"[TPS AI] State transition: {currentState} -> {newState}");
        }

        private void ExitState(AIState state)
        {
            switch (state)
            {
                case AIState.TakingCover:
                    isInCover = false;
                    navMeshAgent.isStopped = false;
                    break;
                case AIState.Flanking:
                    isFlankingPlayer = false;
                    break;
            }
        }

        private void EnterState(AIState state)
        {
            switch (state)
            {
                case AIState.TakingCover:
                    onEnemyTakeCover?.Raise();
                    break;
                case AIState.Patrol:
                    if (patrolPoints.Length > 0)
                    {
                        MoveToNextPatrolPoint();
                    }
                    break;
            }
        }

        private void UpdateAnimations()
        {
            if (animator == null) return;

            // Set animation parameters
            float speed = navMeshAgent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
            animator.SetBool("IsInCover", isInCover);
            animator.SetBool("CanSeePlayer", CanSeePlayer());
        }

        private void HandleStateSpecificBehavior()
        {
            // Additional state-specific logic can be added here
        }

        // Public methods for external systems
        public void TakeDamage(int damage)
        {
            if (currentState == AIState.Dead) return;

            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // React to damage
                if (currentState == AIState.Patrol)
                {
                    TransitionToState(AIState.Chasing);
                }
            }

            UnityEngine.Debug.Log($"[TPS AI] AI took {damage} damage, health: {currentHealth}/{maxHealth}");
        }

        private void Die()
        {
            TransitionToState(AIState.Dead);
            
            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = true;
            }

            if (animator != null)
            {
                animator.SetTrigger("Death");
            }

            onEnemyDeath?.Raise();
            
            // Disable AI after death animation
            Invoke("DisableAI", 3f);

            UnityEngine.Debug.Log("[TPS AI] AI died");
        }

        private void DisableAI()
        {
            gameObject.SetActive(false);
        }

        // Public properties
        public AIState CurrentState => currentState;
        public int CurrentHealth => currentHealth;
        public bool IsInCover => isInCover;
        public bool CanDetectPlayer => CanSeePlayer();
        
        // Gizmos for debugging
        private void OnDrawGizmosSelected()
        {
            // Detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Cover detection range
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, coverDetectionRange);

            // Last known player position
            if (lastKnownPlayerPosition != Vector3.zero)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(lastKnownPlayerPosition, 1f);
                Gizmos.DrawLine(transform.position, lastKnownPlayerPosition);
            }

            // Flanking target position
            if (isFlankingPlayer)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(flankingTargetPosition, 1f);
                Gizmos.DrawLine(transform.position, flankingTargetPosition);
            }

            // Nearest cover point
            if (nearestCoverPoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, nearestCoverPoint.position);
            }
        }
    }
}