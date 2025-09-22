using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Stealth.Services;
using asterivo.Unity60.Features.Player.States;

namespace asterivo.Unity60.Features.Templates.Stealth.Player.States
{
    /// <summary>
    /// ステルス特化のしゃがみ状態
    /// 基本的なCrouchingStateの機能に加えて、StealthServiceとの完全統合を提供
    /// 視認性の大幅低下、音響レベルの削減、移動速度の最適化
    /// </summary>
    public class StealthCrouchingState : IPlayerState
    {
        [Header("Stealth Crouching Configuration")]
        [SerializeField] private float crouchSpeed = 1.5f; // 通常より更に低速
        [SerializeField] private float stealthVisibilityReduction = 0.6f; // 60%視認性削減
        [SerializeField] private float noiseReduction = 0.7f; // 70%騒音削減
        [SerializeField] private float originalHeight;
        [SerializeField] private float crouchHeight = 1.0f; // 通常より更に低く

        private Vector2 _moveInput;
        private IStealthService _stealthService;
        private bool _wasInStealthMode;
        private float _baseNoiseLevel;

        /// <summary>
        /// ステルスしゃがみ状態開始
        /// StealthServiceと連携し、最適な隠蔽姿勢を確立
        /// </summary>
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            // ServiceLocator経由でStealthServiceを取得
            _stealthService = ServiceLocator.GetService<IStealthService>();

            // 物理的な姿勢変更
            if (stateMachine.CharacterController != null)
            {
                originalHeight = stateMachine.CharacterController.height;
                stateMachine.CharacterController.height = crouchHeight;

                Vector3 center = stateMachine.CharacterController.center;
                center.y = crouchHeight / 2f;
                stateMachine.CharacterController.center = center;
            }

            // ステルスシステム統合
            if (_stealthService != null)
            {
                _wasInStealthMode = _stealthService.IsStealthModeActive;
                if (!_wasInStealthMode)
                {
                    _stealthService.SetStealthMode(true);
                }

                // 視認性大幅削減
                _stealthService.UpdatePlayerVisibility(stealthVisibilityReduction);
            }

            // 既存StealthMovement統合
            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Crouching);
                _baseNoiseLevel = stateMachine.StealthMovement.GetNoiseLevel();
            }

            Debug.Log($"[StealthCrouchingState] Stealth crouching activated. Visibility: {stealthVisibilityReduction:F2}, Noise reduction: {noiseReduction:F2}");
        }

        /// <summary>
        /// ステルスしゃがみ状態終了
        /// ステルス設定を適切にリセット
        /// </summary>
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
            // 物理的姿勢復元
            if (stateMachine.CharacterController != null)
            {
                stateMachine.CharacterController.height = originalHeight;

                Vector3 center = stateMachine.CharacterController.center;
                center.y = originalHeight / 2f;
                stateMachine.CharacterController.center = center;
            }

            // ステルス設定復元
            if (_stealthService != null)
            {
                // 前の状態がステルスモードでなかった場合は解除
                if (!_wasInStealthMode)
                {
                    _stealthService.SetStealthMode(false);
                }

                // 通常の視認性に戻す
                _stealthService.UpdatePlayerVisibility(1.0f);
            }

            Debug.Log("[StealthCrouchingState] Stealth crouching deactivated");
        }

        /// <summary>
        /// フレーム更新
        /// ステルス状態の継続的監視
        /// </summary>
        public void Update(DetailedPlayerStateMachine stateMachine)
        {
            // ステルスサービスとの継続的同期
            if (_stealthService != null)
            {
                // 移動による騒音レベル調整
                float currentNoiseLevel = _moveInput.magnitude > 0.1f ?
                    _baseNoiseLevel * noiseReduction : 0.0f;

                _stealthService.UpdatePlayerNoiseLevel(currentNoiseLevel);
            }
        }

        /// <summary>
        /// 物理更新
        /// 低速で慎重な移動処理
        /// </summary>
        public void FixedUpdate(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController == null) return;

            Transform transform = stateMachine.transform;
            Vector3 moveDirection = transform.right * _moveInput.x + transform.forward * _moveInput.y;
            moveDirection.y = 0;

            // 重力処理
            if (!stateMachine.CharacterController.isGrounded)
            {
                moveDirection.y += Physics.gravity.y * Time.fixedDeltaTime;
            }

            // 更に慎重な移動（通常のCrouchingStateより低速）
            stateMachine.CharacterController.Move(moveDirection.normalized * crouchSpeed * Time.fixedDeltaTime);
        }

        /// <summary>
        /// 入力処理
        /// ステルス特化の状態遷移ロジック
        /// </summary>
        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            _moveInput = moveInput;

            // ジャンプ入力でしゃがみ解除（慎重な立ち上がり）
            if (jumpInput)
            {
                if (CanStandUpSafely(stateMachine))
                {
                    // ステルス状態を維持しながら遷移
                    if (moveInput.magnitude > 0.1f)
                    {
                        // ステルス歩行に遷移
                        stateMachine.TransitionToState(PlayerStateType.Walking);
                    }
                    else
                    {
                        // ステルス待機に遷移
                        stateMachine.TransitionToState(PlayerStateType.Idle);
                    }
                    return;
                }
                else
                {
                    // 立ち上がれない場合の警告（オプション）
                    Debug.Log("[StealthCrouchingState] Cannot stand up safely - obstacle detected");
                }
            }

            // より深いステルス姿勢への遷移
            if (Input.GetKeyDown(KeyCode.X))
            {
                stateMachine.TransitionToState(PlayerStateType.Prone);
            }

            // カバーアクション（遮蔽物への移動）
            if (Input.GetKeyDown(KeyCode.C))
            {
                // 近くの遮蔽物を探してカバー状態に遷移
                if (FindNearestCover(stateMachine) != null)
                {
                    stateMachine.TransitionToState(PlayerStateType.InCover);
                }
            }
        }

        /// <summary>
        /// 安全な立ち上がり判定
        /// 頭上の障害物チェックに加えて、敵の視線も考慮
        /// </summary>
        private bool CanStandUpSafely(DetailedPlayerStateMachine stateMachine)
        {
            // 物理的障害物チェック
            RaycastHit hit;
            Vector3 origin = stateMachine.transform.position + Vector3.up * crouchHeight;
            float checkDistance = originalHeight - crouchHeight;

            bool physicallyCanStand = !Physics.SphereCast(origin, 0.3f, Vector3.up, out hit, checkDistance);

            // ステルス考慮: 立ち上がることで発見リスクが高まるかチェック
            bool stealthSafe = true;
            if (_stealthService != null)
            {
                // 現在の視認性が低い場合のみ安全とみなす
                stealthSafe = _stealthService.PlayerVisibilityFactor < 0.5f;
            }

            return physicallyCanStand && stealthSafe;
        }

        /// <summary>
        /// 最寄りのカバーポイントを探索
        /// </summary>
        private Transform FindNearestCover(DetailedPlayerStateMachine stateMachine)
        {
            // 簡易実装: "Cover"タグのオブジェクトを探索
            GameObject[] coverObjects = GameObject.FindGameObjectsWithTag("Cover");
            Transform nearestCover = null;
            float nearestDistance = float.MaxValue;

            foreach (GameObject cover in coverObjects)
            {
                float distance = Vector3.Distance(stateMachine.transform.position, cover.transform.position);
                if (distance < nearestDistance && distance <= 3.0f) // 3m以内のカバー
                {
                    nearestDistance = distance;
                    nearestCover = cover.transform;
                }
            }

            return nearestCover;
        }
    }
}