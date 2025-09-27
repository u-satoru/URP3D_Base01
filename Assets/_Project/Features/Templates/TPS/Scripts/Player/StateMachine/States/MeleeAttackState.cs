using UnityEngine;

namespace asterivo.Unity60.Features.Templates.TPS.Player.StateMachine.States
{
    /// <summary>
    /// Melee Attack state - Player is performing melee attack
    /// </summary>
    public class MeleeAttackState : BasePlayerState
    {
        private float _attackTimer;
        private float _attackDuration;
        private bool _attackComplete;
        private bool _damageDealt;

        public override PlayerState StateType => PlayerState.MeleeAttack;

        public override void Enter()
        {
            Debug.Log("[MeleeAttackState] Melee attack state entered");
            _attackTimer = 0f;
            _attackComplete = false;
            _damageDealt = false;
            
            // Get attack duration from player data
            _attackDuration = _playerData?.MeleeAttackDuration ?? 0.8f;

            // Stop movement during attack
            _controller.SetMovementMultiplier(0.1f);

            // Start attack animation
            StartMeleeAnimation();

            // Play melee attack sound through ServiceLocator
            var audioManager = _serviceManager?.GetAudioManager();
            if (audioManager != null)
            {
                audioManager.PlaySFX("melee_attack");
            }
        }

        public override void Update()
        {
            _attackTimer += Time.deltaTime;

            // Handle damage dealing at specific point in animation
            float damageWindow = _attackDuration * 0.5f; // Deal damage halfway through attack
            if (!_damageDealt && _attackTimer >= damageWindow)
            {
                DealMeleeDamage();
                _damageDealt = true;
            }

            // Limited look input during attack
            Vector2 lookInput = GetLookInput();
            if (lookInput.magnitude > 0.1f)
            {
                // Reduced sensitivity during attack
                float yRotation = lookInput.x * _playerData.LookSensitivity * 0.3f * Time.deltaTime;
                _controller.transform.Rotate(0, yRotation, 0);
            }

            // Check if attack is complete
            if (_attackTimer >= _attackDuration && !_attackComplete)
            {
                CompleteAttack();
            }
        }

        public override void Exit()
        {
            Debug.Log("[MeleeAttackState] Melee attack state exited");
            
            // Restore normal movement speed
            _controller.SetMovementMultiplier(1.0f);

            // Stop attack animation if still playing
            StopMeleeAnimation();
        }

        private void StartMeleeAnimation()
        {
            // TODO: Implement melee attack animation
            // - Play attack animation on character
            // - Handle animation events for damage timing
            // - Sync with attack duration
            Debug.Log("[MeleeAttackState] Melee attack animation started");
        }

        private void StopMeleeAnimation()
        {
            // TODO: Stop melee attack animation
            Debug.Log("[MeleeAttackState] Melee attack animation stopped");
        }

        private void DealMeleeDamage()
        {
            Debug.Log("[MeleeAttackState] Dealing melee damage");

            // TODO: Implement melee damage dealing
            // - Perform sphere cast or collision detection in front of player
            // - Check for enemies within melee range
            // - Apply damage to hit targets
            // - Play impact effects and sounds

            Vector3 attackOrigin = _controller.transform.position + _controller.transform.forward * 0.5f;
            float attackRadius = _playerData?.MeleeAttackRange ?? 1.0f;
            float attackDamage = _playerData?.MeleeAttackDamage ?? 25f;

            // Perform sphere cast for melee detection
            Collider[] hitColliders = Physics.OverlapSphere(attackOrigin, attackRadius);
            
            foreach (var hitCollider in hitColliders)
            {
                // Check if hit object is an enemy
                if (hitCollider.CompareTag("Enemy"))
                {
                    Debug.Log($"[MeleeAttackState] Hit enemy: {hitCollider.name}");
                    
                    // TODO: Apply damage to enemy
                    // var enemyHealth = hitCollider.GetComponent<EnemyHealth>();
                    // if (enemyHealth != null)
                    // {
                    //     enemyHealth.TakeDamage(attackDamage);
                    // }

                    // Play hit effect
                    var audioManager = _serviceManager?.GetAudioManager();
                    if (audioManager != null)
                    {
                        audioManager.PlaySFX("melee_hit");
                    }

                    // TODO: Spawn hit effect particles
                    // TODO: Apply knockback to enemy
                }
            }
        }

        private void CompleteAttack()
        {
            _attackComplete = true;
            Debug.Log("[MeleeAttackState] Melee attack completed");

            // TODO: Implement attack completion
            // - Check for combo possibilities
            // - Handle cooldown
            // - Update UI if needed

            // State machine will handle transition back to appropriate state
        }

        private void OnDrawGizmosSelected()
        {
            // Draw melee attack range for debugging
            if (_controller != null && _playerData != null)
            {
                Vector3 attackOrigin = _controller.transform.position + _controller.transform.forward * 0.5f;
                float attackRadius = _playerData.MeleeAttackRange;
                
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(attackOrigin, attackRadius);
            }
        }
    }
}
