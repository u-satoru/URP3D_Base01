using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Stealth.Services;
using asterivo.Unity60.Features.Player.States;

namespace asterivo.Unity60.Features.Templates.Stealth.Player.States
{
    /// <summary>
    /// ステルス特化の匍匐状態
    /// 最大の隠蔽効果を提供する最も低い姿勢
    /// 移動速度は大幅に低下するが、視認性と騒音を最小限に抑制
    /// </summary>
    public class StealthProneState : IPlayerState
    {
        [Header("Stealth Prone Configuration")]
        [SerializeField] private float proneSpeed = 0.8f; // 非常に低速
        [SerializeField] private float maxStealthVisibility = 0.2f; // 80%視認性削減
        [SerializeField] private float maxNoiseReduction = 0.9f; // 90%騒音削減
        [SerializeField] private float originalHeight;
        [SerializeField] private float proneHeight = 0.6f; // 最低姿勢

        private Vector2 _moveInput;
        private IStealthService _stealthService;
        private bool _wasInStealthMode;
        private float _baseNoiseLevel;
        private bool _isCompletlyStill; // 完全静止状態
        private float _stillTimer;
        private float _stillThreshold = 2.0f; // 2秒静止で完全隠蔽

        /// <summary>
        /// ステルス匍匐状態開始
        /// 最大隠蔽モードを確立
        /// </summary>
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            // ServiceLocator経由でStealthServiceを取得
            _stealthService = ServiceLocator.GetService<IStealthService>();

            // 最低姿勢への変更
            if (stateMachine.CharacterController != null)
            {
                originalHeight = stateMachine.CharacterController.height;
                stateMachine.CharacterController.height = proneHeight;

                Vector3 center = stateMachine.CharacterController.center;
                center.y = proneHeight / 2f;
                stateMachine.CharacterController.center = center;
            }

            // 最大ステルスモード開始
            if (_stealthService != null)
            {
                _wasInStealthMode = _stealthService.IsStealthModeActive;
                if (!_wasInStealthMode)
                {
                    _stealthService.SetStealthMode(true);
                }

                // 最大視認性削減
                _stealthService.UpdatePlayerVisibility(maxStealthVisibility);
            }

            // 既存StealthMovement統合
            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Prone);
                _baseNoiseLevel = stateMachine.StealthMovement.GetNoiseLevel();
            }

            // 静止状態初期化
            _isCompletlyStill = false;
            _stillTimer = 0f;

            Debug.Log($"[StealthProneState] Maximum stealth activated. Visibility: {maxStealthVisibility:F2}, Noise reduction: {maxNoiseReduction:F2}");
        }

        /// <summary>
        /// ステルス匍匐状態終了
        /// ステルス設定を適切にリセット
        /// </summary>
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
            // 姿勢復元
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
                if (!_wasInStealthMode)
                {
                    _stealthService.SetStealthMode(false);
                }

                // 通常の視認性に戻す
                _stealthService.UpdatePlayerVisibility(1.0f);
                _stealthService.UpdatePlayerNoiseLevel(0.0f);
            }

            Debug.Log("[StealthProneState] Maximum stealth deactivated");
        }

        /// <summary>
        /// フレーム更新
        /// 静止時間による完全隠蔽判定
        /// </summary>
        public void Update(DetailedPlayerStateMachine stateMachine)
        {
            if (_stealthService == null) return;

            // 静止状態の判定と管理
            bool isMoving = _moveInput.magnitude > 0.01f;

            if (!isMoving)
            {
                _stillTimer += Time.deltaTime;

                // 完全静止状態達成
                if (_stillTimer >= _stillThreshold && !_isCompletlyStill)
                {
                    _isCompletlyStill = true;
                    // 完全隠蔽状態: 視認性をさらに削減
                    _stealthService.UpdatePlayerVisibility(maxStealthVisibility * 0.5f); // さらに50%削減
                    _stealthService.UpdatePlayerNoiseLevel(0.0f); // 完全無音

                    Debug.Log("[StealthProneState] Complete concealment achieved - nearly invisible");
                }
            }
            else
            {
                _stillTimer = 0f;
                if (_isCompletlyStill)
                {
                    _isCompletlyStill = false;
                    // 通常の匍匐隠蔽に戻す
                    _stealthService.UpdatePlayerVisibility(maxStealthVisibility);
                }
            }

            // 移動時の騒音レベル調整
            if (isMoving)
            {
                float currentNoiseLevel = _baseNoiseLevel * (1.0f - maxNoiseReduction);
                _stealthService.UpdatePlayerNoiseLevel(currentNoiseLevel);
            }
        }

        /// <summary>
        /// 物理更新
        /// 非常に慎重で低速な匍匐移動
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

            // 匍匐移動（最低速度）
            stateMachine.CharacterController.Move(moveDirection.normalized * proneSpeed * Time.fixedDeltaTime);
        }

        /// <summary>
        /// 入力処理
        /// 匍匐状態からの慎重な遷移制御
        /// </summary>
        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            _moveInput = moveInput;

            // ジャンプ入力でしゃがみ状態に遷移
            if (jumpInput)
            {
                if (IsSafeToRise(stateMachine))
                {
                    // まずはしゃがみ状態に遷移（段階的な姿勢変更）
                    stateMachine.TransitionToState(PlayerStateType.Crouching);
                    return;
                }
                else
                {
                    Debug.Log("[StealthProneState] Cannot rise safely - remaining in prone position");
                }
            }

            // カバーアクション
            if (Input.GetKeyDown(KeyCode.C))
            {
                Transform nearestCover = FindNearestCover(stateMachine);
                if (nearestCover != null)
                {
                    // カバーが利用可能な場合のみ遷移
                    stateMachine.TransitionToState(PlayerStateType.InCover);
                }
            }

            // 特殊ステルスアクション（オプション）
            if (Input.GetKeyDown(KeyCode.E))
            {
                PerformStealthAction(stateMachine);
            }
        }

        /// <summary>
        /// 安全な起き上がり判定
        /// 敵の視線と物理的制約の両方を考慮
        /// </summary>
        private bool IsSafeToRise(DetailedPlayerStateMachine stateMachine)
        {
            // 物理的障害物チェック
            RaycastHit hit;
            Vector3 origin = stateMachine.transform.position + Vector3.up * proneHeight;
            float checkDistance = originalHeight - proneHeight;

            bool physicallyCanRise = !Physics.SphereCast(origin, 0.4f, Vector3.up, out hit, checkDistance);

            // ステルス安全性チェック
            bool stealthSafe = true;
            if (_stealthService != null)
            {
                // 現在の視認性が非常に低い場合のみ安全
                stealthSafe = _stealthService.PlayerVisibilityFactor < 0.3f;

                // 隠蔽ゾーン内にいる場合は追加の安全性
                if (_stealthService.IsPlayerConcealed)
                {
                    stealthSafe = true;
                }
            }

            return physicallyCanRise && stealthSafe;
        }

        /// <summary>
        /// 最寄りのカバーポイント探索
        /// </summary>
        private Transform FindNearestCover(DetailedPlayerStateMachine stateMachine)
        {
            GameObject[] coverObjects = GameObject.FindGameObjectsWithTag("Cover");
            Transform nearestCover = null;
            float nearestDistance = float.MaxValue;

            foreach (GameObject cover in coverObjects)
            {
                float distance = Vector3.Distance(stateMachine.transform.position, cover.transform.position);
                if (distance < nearestDistance && distance <= 2.0f) // 匍匐では2m以内
                {
                    nearestDistance = distance;
                    nearestCover = cover.transform;
                }
            }

            return nearestCover;
        }

        /// <summary>
        /// 特殊ステルスアクション実行
        /// 匍匐状態からの環境相互作用
        /// </summary>
        private void PerformStealthAction(DetailedPlayerStateMachine stateMachine)
        {
            if (_stealthService == null) return;

            // 近くのインタラクト可能オブジェクトを探索
            Collider[] interactables = Physics.OverlapSphere(
                stateMachine.transform.position, 1.5f,
                LayerMask.GetMask("Interactable"));

            foreach (var collider in interactables)
            {
                // 環境相互作用の実行（例：カメラ無効化、ライト破壊等）
                if (collider.CompareTag("SecurityCamera"))
                {
                    _stealthService.InteractWithEnvironment(collider.gameObject, StealthInteractionType.DisableCamera);
                    Debug.Log("[StealthProneState] Disabled security camera from prone position");
                    break;
                }
                else if (collider.CompareTag("Light"))
                {
                    _stealthService.InteractWithEnvironment(collider.gameObject, StealthInteractionType.SabotageLight);
                    Debug.Log("[StealthProneState] Sabotaged light source from prone position");
                    break;
                }
            }
        }
    }
}
