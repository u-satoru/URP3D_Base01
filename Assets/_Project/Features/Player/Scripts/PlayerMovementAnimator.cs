using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Player
{
    /// <summary>
    /// DOTweenを使用したプレイヤーの移動アニメーション管理クラス
    /// スムーズな移動、ジャンプ、回転アニメーションを提供します。
    /// </summary>
    public class PlayerMovementAnimator : MonoBehaviour
    {
        [TabGroup("Animation Settings", "Movement")]
        [PropertyRange(0.1f, 2f)]
        [LabelText("Move Animation Duration")]
        [SuffixLabel("s", overlay: true)]
        [SerializeField] private float moveAnimationDuration = 0.3f;
        
        [TabGroup("Animation Settings", "Movement")]
        [LabelText("Move Easing")]
        [SerializeField] private Ease moveEasing = Ease.OutQuad;
        
        [TabGroup("Animation Settings", "Jump")]
        [PropertyRange(0.2f, 1f)]
        [LabelText("Jump Animation Duration")]
        [SuffixLabel("s", overlay: true)]
        [SerializeField] private float jumpAnimationDuration = 0.5f;
        
        [TabGroup("Animation Settings", "Jump")]
        [PropertyRange(0.5f, 3f)]
        [LabelText("Jump Height")]
        [SuffixLabel("m", overlay: true)]
        [SerializeField] private float jumpHeight = 1.5f;
        
        [TabGroup("Animation Settings", "Jump")]
        [LabelText("Jump Easing")]
        [SerializeField] private Ease jumpEasing = Ease.OutQuad;
        
        [TabGroup("Animation Settings", "Rotation")]
        [PropertyRange(0.1f, 1f)]
        [LabelText("Rotation Animation Duration")]
        [SuffixLabel("s", overlay: true)]
        [SerializeField] private float rotationDuration = 0.2f;
        
        [TabGroup("Animation Settings", "Rotation")]
        [LabelText("Rotation Easing")]
        [SerializeField] private Ease rotationEasing = Ease.OutCubic;
        
        [TabGroup("Animation Settings", "Scale Effects")]
        [PropertyRange(0.05f, 0.3f)]
        [LabelText("Landing Scale Effect Duration")]
        [SuffixLabel("s", overlay: true)]
        [SerializeField] private float landingScaleEffectDuration = 0.15f;
        
        [TabGroup("Animation Settings", "Scale Effects")]
        [PropertyRange(0.8f, 0.95f)]
        [LabelText("Landing Scale")]
        [SerializeField] private float landingScale = 0.9f;

        [TabGroup("Debug Info", "Current State")]
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Is Animating")]
        private bool isAnimating = false;
        
        [TabGroup("Debug Info", "Current State")]
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Current Position")]
        private Vector3 currentPosition;
        
        private Sequence currentMovementSequence;
        private Vector3 originalScale;
        
        private void Awake()
        {
            originalScale = transform.localScale;
        }
        
        private void Update()
        {
            currentPosition = transform.position;
        }
        
        /// <summary>
        /// スムーズな位置移動アニメーションを実行します
        /// </summary>
        public Tweener AnimateMoveTo(Vector3 targetPosition, bool useLocalSpace = false)
        {
            StopCurrentMovement();
            isAnimating = true;
            
            Tweener moveTween = useLocalSpace 
                ? transform.DOLocalMove(targetPosition, moveAnimationDuration)
                : transform.DOMove(targetPosition, moveAnimationDuration);
            
            return moveTween
                .SetEase(moveEasing)
                .OnComplete(() => isAnimating = false);
        }
        
        /// <summary>
        /// ジャンプアニメーションを実行します
        /// </summary>
        public Sequence AnimateJump()
        {
            StopCurrentMovement();
            isAnimating = true;
            
            currentMovementSequence = DOTween.Sequence();
            
            Vector3 jumpTarget = transform.position + Vector3.up * jumpHeight;
            
            currentMovementSequence
                .Append(transform.DOMove(jumpTarget, jumpAnimationDuration * 0.6f).SetEase(jumpEasing))
                .Append(transform.DOMove(transform.position, jumpAnimationDuration * 0.4f).SetEase(jumpEasing))
                .OnComplete(() => {
                    isAnimating = false;
                    PlayLandingEffect();
                });
            
            return currentMovementSequence;
        }
        
        /// <summary>
        /// スムーズな回転アニメーションを実行します
        /// </summary>
        public Tweener AnimateRotateTo(Vector3 direction)
        {
            if (direction == Vector3.zero) return null;
            
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            
            return transform.DORotateQuaternion(targetRotation, rotationDuration)
                .SetEase(rotationEasing);
        }
        
        /// <summary>
        /// スムーズな回転アニメーションを実行します（Y軸のみ）
        /// </summary>
        public Tweener AnimateRotateToY(float targetYRotation)
        {
            Vector3 targetEuler = new Vector3(transform.eulerAngles.x, targetYRotation, transform.eulerAngles.z);
            
            return transform.DORotate(targetEuler, rotationDuration)
                .SetEase(rotationEasing);
        }
        
        /// <summary>
        /// ダメージを受けたときのシェイクアニメーション
        /// </summary>
        public Tweener AnimateDamageShake(float intensity = 0.5f, float duration = 0.3f)
        {
            return transform.DOShakePosition(duration, intensity)
                .SetEase(Ease.OutElastic);
        }
        
        /// <summary>
        /// 着地効果のスケールアニメーション
        /// </summary>
        private void PlayLandingEffect()
        {
            transform.localScale = originalScale * landingScale;
            transform.DOScale(originalScale, landingScaleEffectDuration)
                .SetEase(Ease.OutBounce);
        }
        
        /// <summary>
        /// 現在の移動アニメーションを停止します
        /// </summary>
        public void StopCurrentMovement()
        {
            currentMovementSequence?.Kill();
            transform.DOKill();
            isAnimating = false;
        }
        
        /// <summary>
        /// 現在アニメーション中かどうかを返します
        /// </summary>
        public bool IsAnimating => isAnimating;
        
        private void OnDestroy()
        {
            StopCurrentMovement();
        }
        
        #if UNITY_EDITOR
        [TabGroup("Debug Info", "Test Animations")]
        [Button("Test Jump")]
        private void TestJump()
        {
            if (Application.isPlaying)
                AnimateJump();
        }
        
        [TabGroup("Debug Info", "Test Animations")]
        [Button("Test Damage Shake")]
        private void TestDamageShake()
        {
            if (Application.isPlaying)
                AnimateDamageShake();
        }
        #endif
    }
}