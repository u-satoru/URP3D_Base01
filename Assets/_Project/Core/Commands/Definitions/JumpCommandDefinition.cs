using UnityEngine;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// ジャンプコマンドの定義
    /// プレイヤーまたはAIのジャンプアクションをカプセル化します
    /// </summary>
    [System.Serializable]
    public class JumpCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// ジャンプの種類を定義する列挙型
        /// </summary>
        public enum JumpType
        {
            Normal,     // 通常ジャンプ
            Double,     // 二段ジャンプ
            Wall,       // 壁ジャンプ
            Long,       // 長距離ジャンプ
            High        // 高ジャンプ
        }

        [Header("Jump Parameters")]
        public JumpType jumpType = JumpType.Normal;
        public float jumpForce = 10f;
        public Vector3 direction = Vector3.up;
        public float horizontalBoost = 0f;

        [Header("Physics")]
        public float gravityMultiplier = 1f;
        public float airControl = 0.5f;
        public float maxAirSpeed = 5f;
        public bool preserveMomentum = true;

        [Header("Double Jump")]
        public int maxJumpCount = 1;
        public float doubleJumpForceMultiplier = 0.8f;
        public bool resetVelocityOnDoubleJump = false;

        [Header("Wall Jump")]
        public float wallJumpAngle = 45f;
        public float wallJumpForce = 8f;
        public float wallSlideSpeed = 2f;
        public LayerMask wallLayer = -1;

        [Header("Stamina")]
        public bool consumeStamina = false;
        public float staminaCost = 10f;
        public bool allowJumpWithoutStamina = false;

        [Header("Animation")]
        public string jumpAnimationTrigger = "Jump";
        public float animationCrossFade = 0.1f;

        [Header("Effects")]
        public bool playSound = true;
        public string jumpSoundName = "Jump";
        public bool spawnDustEffect = true;
        public GameObject dustEffectPrefab;

        [Header("Landing")]
        public float landingImpactThreshold = 10f;
        public bool playLandingSound = true;
        public bool screenShakeOnLanding = false;

        public ICommand CreateCommand(object context = null)
        {
            return new JumpCommand(this, context);
        }
    }

    /// <summary>
    /// JumpCommandDefinitionに対応する実際のコマンド実装
    /// </summary>
    public class JumpCommand : ICommand
    {
        private JumpCommandDefinition definition;
        private object context;
        private bool executed = false;

        public JumpCommand(JumpCommandDefinition definition, object context = null)
        {
            this.definition = definition;
            this.context = context;
        }

        public void Execute()
        {
            if (executed) return;

            // コンテキストから必要な情報を取得
            GameObject target = GetTargetFromContext();
            if (target == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("Jump command failed: No target found");
#endif
                return;
            }

            // Rigidbodyコンポーネントの取得
            Rigidbody rb = target.GetComponent<Rigidbody>();
            if (rb == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning($"Jump command failed: No Rigidbody on {target.name}");
#endif
                return;
            }

            // スタミナチェック
            if (definition.consumeStamina && !CheckStamina(target))
            {
                if (!definition.allowJumpWithoutStamina)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    UnityEngine.Debug.Log("Jump cancelled: Not enough stamina");
#endif
                    return;
                }
            }

            // ジャンプ実行
            PerformJump(target, rb);

            // エフェクトの再生
            PlayEffects(target);

            // スタミナ消費
            if (definition.consumeStamina)
            {
                ConsumeStamina(target);
            }

            executed = true;
        }

        private GameObject GetTargetFromContext()
        {
            if (context is GameObject gameObject)
                return gameObject;

            if (context is Component component)
                return component.gameObject;

            if (context is MonoBehaviour monoBehaviour)
                return monoBehaviour.gameObject;

            return null;
        }

        private void PerformJump(GameObject target, Rigidbody rb)
        {
            Vector3 jumpVelocity = CalculateJumpVelocity();

            if (definition.preserveMomentum)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z) + jumpVelocity;
            }
            else
            {
                rb.velocity = jumpVelocity;
            }

            // アニメーション再生
            if (!string.IsNullOrEmpty(definition.jumpAnimationTrigger))
            {
                Animator animator = target.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.SetTrigger(definition.jumpAnimationTrigger);
                }
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"{target.name} jumped with force {jumpVelocity.magnitude}");
#endif
        }

        private Vector3 CalculateJumpVelocity()
        {
            Vector3 jumpDir = definition.direction.normalized;
            float force = definition.jumpForce;

            switch (definition.jumpType)
            {
                case JumpCommandDefinition.JumpType.Double:
                    force *= definition.doubleJumpForceMultiplier;
                    break;

                case JumpCommandDefinition.JumpType.Wall:
                    force = definition.wallJumpForce;
                    jumpDir = Quaternion.Euler(0, definition.wallJumpAngle, 0) * jumpDir;
                    break;

                case JumpCommandDefinition.JumpType.Long:
                    force *= 0.8f;
                    jumpDir.x += definition.horizontalBoost;
                    jumpDir.z += definition.horizontalBoost;
                    break;

                case JumpCommandDefinition.JumpType.High:
                    force *= 1.5f;
                    break;
            }

            return jumpDir * force;
        }

        private bool CheckStamina(GameObject target)
        {
            // TODO: スタミナシステムとの統合
            return true;
        }

        private void ConsumeStamina(GameObject target)
        {
            // TODO: スタミナシステムとの統合
        }

        private void PlayEffects(GameObject target)
        {
            // サウンド再生
            if (definition.playSound && !string.IsNullOrEmpty(definition.jumpSoundName))
            {
                // TODO: オーディオシステムとの統合
            }

            // ダストエフェクト生成
            if (definition.spawnDustEffect && definition.dustEffectPrefab != null)
            {
                GameObject effect = Object.Instantiate(definition.dustEffectPrefab,
                    target.transform.position,
                    Quaternion.identity);

                Object.Destroy(effect, 2f);
            }
        }

        public bool CanExecute()
        {
            if (executed) return false;

            GameObject target = GetTargetFromContext();
            if (target == null) return false;

            Rigidbody rb = target.GetComponent<Rigidbody>();
            if (rb == null) return false;

            // スタミナチェック
            if (definition.consumeStamina && !definition.allowJumpWithoutStamina)
            {
                return CheckStamina(target);
            }

            return true;
        }
    }
}