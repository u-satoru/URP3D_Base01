using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Stealth.Services;
using asterivo.Unity60.Features.Player.States;

namespace asterivo.Unity60.Features.Templates.Stealth.Player.States
{
    /// <summary>
    /// ステルス特化のカバー状態
    /// 遮蔽物を利用した戦術的隠蔽とピーキング機能
    /// 動的な視界制御と敵位置の偵察能力を提供
    /// </summary>
    public class StealthInCoverState : IPlayerState
    {
        [Header("Stealth Cover Configuration")]
        [SerializeField] private float coverMoveSpeed = 1.0f; // カバー間移動速度
        [SerializeField] private float coverVisibilityReduction = 0.8f; // 80%視認性削減
        [SerializeField] private float peekVisibilityIncrease = 0.4f; // ピーク時40%視認性増加
        [SerializeField] private float coverNoiseReduction = 0.8f; // 80%騒音削減

        private Vector2 _moveInput;
        private IStealthService _stealthService;
        private bool _wasInStealthMode;
        private Transform _currentCover;
        private bool _isPeeking;
        private bool _canPeekLeft;
        private bool _canPeekRight;
        private Vector3 _originalCoverPosition;
        private UnityEngine.Camera _playerCamera;

        /// <summary>
        /// ステルスカバー状態開始
        /// 遮蔽物との位置関係を確立し、最適な隠蔽姿勢を設定
        /// </summary>
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            // ServiceLocator経由でStealthServiceを取得
            _stealthService = ServiceLocator.GetService<IStealthService>();

            // カバーオブジェクトを特定
            _currentCover = FindOptimalCoverPosition(stateMachine);
            if (_currentCover != null)
            {
                _originalCoverPosition = _currentCover.position;
                AlignTocover(stateMachine, _currentCover);
            }

            // ステルスモード確認・開始
            if (_stealthService != null)
            {
                _wasInStealthMode = _stealthService.IsStealthModeActive;
                if (!_wasInStealthMode)
                {
                    _stealthService.SetStealthMode(true);
                }

                // カバー状態での視認性削減
                _stealthService.UpdatePlayerVisibility(coverVisibilityReduction);
                _stealthService.UpdatePlayerNoiseLevel(0.0f); // カバー中は静音
            }

            // 既存StealthMovement統合
            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Crouching);
            }

            // ピーキング可能性判定
            EvaluatePeekingOptions(stateMachine);

            // カメラ参照取得
            _playerCamera = UnityEngine.Camera.main ?? Object.FindFirstObjectByType<UnityEngine.Camera>();

            _isPeeking = false;

            Debug.Log($"[StealthInCoverState] Cover position established. Visibility reduction: {coverVisibilityReduction:F2}");
        }

        /// <summary>
        /// ステルスカバー状態終了
        /// </summary>
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
            // ピーキング状態解除
            if (_isPeeking)
            {
                StopPeeking(stateMachine);
            }

            // ステルス設定復元
            if (_stealthService != null)
            {
                if (!_wasInStealthMode)
                {
                    _stealthService.SetStealthMode(false);
                }

                // 通常視認性に戻す
                _stealthService.UpdatePlayerVisibility(1.0f);
            }

            _currentCover = null;

            Debug.Log("[StealthInCoverState] Cover position abandoned");
        }

        /// <summary>
        /// フレーム更新
        /// カバー状態の維持と環境監視
        /// </summary>
        public void Update(DetailedPlayerStateMachine stateMachine)
        {
            if (_stealthService == null) return;

            // カバーオブジェクトとの距離監視
            if (_currentCover != null)
            {
                float distanceToCover = Vector3.Distance(
                    stateMachine.transform.position, _currentCover.position);

                // カバーから離れすぎた場合
                if (distanceToCover > 2.0f)
                {
                    Debug.Log("[StealthInCoverState] Too far from cover - transitioning out");
                    stateMachine.TransitionToState(PlayerStateType.Crouching);
                    return;
                }
            }

            // ピーキング状態の視認性管理
            if (_isPeeking && _stealthService != null)
            {
                float peekingVisibility = coverVisibilityReduction + peekVisibilityIncrease;
                _stealthService.UpdatePlayerVisibility(Mathf.Clamp01(peekingVisibility));
            }
        }

        /// <summary>
        /// 物理更新
        /// カバー間の慎重な移動
        /// </summary>
        public void FixedUpdate(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController == null) return;

            Transform transform = stateMachine.transform;
            Vector3 moveDirection = Vector3.zero;

            // カバー沿いの移動のみ許可
            if (_currentCover != null)
            {
                Vector3 coverDirection = (_currentCover.position - transform.position).normalized;
                Vector3 coverRight = Vector3.Cross(Vector3.up, coverDirection);

                moveDirection = coverRight * _moveInput.x;
            }

            // 重力処理
            if (!stateMachine.CharacterController.isGrounded)
            {
                moveDirection.y += Physics.gravity.y * Time.fixedDeltaTime;
            }

            // カバー移動
            stateMachine.CharacterController.Move(moveDirection * coverMoveSpeed * Time.fixedDeltaTime);
        }

        /// <summary>
        /// 入力処理
        /// カバー状態での特殊操作
        /// </summary>
        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            _moveInput = moveInput;

            // ジャンプ入力でカバー離脱
            if (jumpInput)
            {
                if (IsSafeToLeaveCover(stateMachine))
                {
                    stateMachine.TransitionToState(PlayerStateType.Crouching);
                    return;
                }
                else
                {
                    Debug.Log("[StealthInCoverState] Not safe to leave cover");
                }
            }

            // ピーキング操作
            if (Input.GetKeyDown(KeyCode.Q) && _canPeekLeft)
            {
                TogglePeeking(stateMachine, PeekDirection.Left);
            }
            else if (Input.GetKeyDown(KeyCode.E) && _canPeekRight)
            {
                TogglePeeking(stateMachine, PeekDirection.Right);
            }

            // ピーキング終了
            if (Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.E))
            {
                if (_isPeeking)
                {
                    StopPeeking(stateMachine);
                }
            }

            // 他のカバーへの移動
            if (Input.GetKeyDown(KeyCode.F))
            {
                Transform nextCover = FindNearestAlternateCover(stateMachine);
                if (nextCover != null)
                {
                    MoveToCover(stateMachine, nextCover);
                }
            }

            // カバーからの環境相互作用
            if (Input.GetKeyDown(KeyCode.R))
            {
                PerformCoverAction(stateMachine);
            }
        }

        /// <summary>
        /// 最適なカバー位置を特定
        /// </summary>
        private Transform FindOptimalCoverPosition(DetailedPlayerStateMachine stateMachine)
        {
            GameObject[] coverObjects = GameObject.FindGameObjectsWithTag("Cover");
            Transform optimalCover = null;
            float nearestDistance = float.MaxValue;

            foreach (GameObject cover in coverObjects)
            {
                float distance = Vector3.Distance(stateMachine.transform.position, cover.transform.position);
                if (distance < nearestDistance && distance <= 3.0f)
                {
                    nearestDistance = distance;
                    optimalCover = cover.transform;
                }
            }

            return optimalCover;
        }

        /// <summary>
        /// カバーオブジェクトに整列
        /// </summary>
        private void AlignTocover(DetailedPlayerStateMachine stateMachine, Transform cover)
        {
            Vector3 directionToCover = (cover.position - stateMachine.transform.position).normalized;
            Vector3 alignPosition = cover.position - directionToCover * 1.2f; // カバーから1.2m距離

            stateMachine.transform.position = alignPosition;
            stateMachine.transform.LookAt(cover.position);
        }

        /// <summary>
        /// ピーキング可能性評価
        /// </summary>
        private void EvaluatePeekingOptions(DetailedPlayerStateMachine stateMachine)
        {
            if (_currentCover == null) return;

            Vector3 playerPos = stateMachine.transform.position;
            Vector3 coverPos = _currentCover.position;

            // 左右のピーキング可能性をレイキャストで確認
            Vector3 rightDirection = stateMachine.transform.right;
            Vector3 leftDirection = -stateMachine.transform.right;

            _canPeekRight = !Physics.Raycast(playerPos, rightDirection, 1.5f);
            _canPeekLeft = !Physics.Raycast(playerPos, leftDirection, 1.5f);
        }

        /// <summary>
        /// ピーキング切り替え
        /// </summary>
        private void TogglePeeking(DetailedPlayerStateMachine stateMachine, PeekDirection direction)
        {
            if (_isPeeking)
            {
                StopPeeking(stateMachine);
            }
            else
            {
                StartPeeking(stateMachine, direction);
            }
        }

        /// <summary>
        /// ピーキング開始
        /// </summary>
        private void StartPeeking(DetailedPlayerStateMachine stateMachine, PeekDirection direction)
        {
            _isPeeking = true;

            // カメラオフセット適用（簡易実装）
            if (_playerCamera != null)
            {
                Vector3 peekOffset = direction == PeekDirection.Left ?
                    -stateMachine.transform.right * 0.8f :
                    stateMachine.transform.right * 0.8f;

                _playerCamera.transform.position += peekOffset;
            }

            // 視認性増加（ピーキングリスク）
            if (_stealthService != null)
            {
                float peekingVisibility = coverVisibilityReduction + peekVisibilityIncrease;
                _stealthService.UpdatePlayerVisibility(Mathf.Clamp01(peekingVisibility));
            }

            Debug.Log($"[StealthInCoverState] Started peeking {direction}");
        }

        /// <summary>
        /// ピーキング終了
        /// </summary>
        private void StopPeeking(DetailedPlayerStateMachine stateMachine)
        {
            _isPeeking = false;

            // カメラ位置復元（簡易実装）
            if (_playerCamera != null)
            {
                // 本来はカメラの元位置を保存しておくべき
                Vector3 playerPos = stateMachine.transform.position;
                _playerCamera.transform.position = playerPos + Vector3.up * 1.7f;
            }

            // 視認性をカバー状態に戻す
            if (_stealthService != null)
            {
                _stealthService.UpdatePlayerVisibility(coverVisibilityReduction);
            }

            Debug.Log("[StealthInCoverState] Stopped peeking");
        }

        /// <summary>
        /// 安全なカバー離脱判定
        /// </summary>
        private bool IsSafeToLeaveCover(DetailedPlayerStateMachine stateMachine)
        {
            if (_stealthService == null) return true;

            // 現在の視認性と隠蔽状態を評価
            bool lowVisibility = _stealthService.PlayerVisibilityFactor < 0.5f;
            bool isConcealed = _stealthService.IsPlayerConcealed;

            return lowVisibility || isConcealed;
        }

        /// <summary>
        /// 代替カバー探索
        /// </summary>
        private Transform FindNearestAlternateCover(DetailedPlayerStateMachine stateMachine)
        {
            GameObject[] coverObjects = GameObject.FindGameObjectsWithTag("Cover");
            Transform alternateCover = null;
            float nearestDistance = float.MaxValue;

            foreach (GameObject cover in coverObjects)
            {
                if (cover.transform == _currentCover) continue; // 現在のカバーは除外

                float distance = Vector3.Distance(stateMachine.transform.position, cover.transform.position);
                if (distance < nearestDistance && distance <= 5.0f)
                {
                    nearestDistance = distance;
                    alternateCover = cover.transform;
                }
            }

            return alternateCover;
        }

        /// <summary>
        /// 別カバーへの移動
        /// </summary>
        private void MoveToCover(DetailedPlayerStateMachine stateMachine, Transform newCover)
        {
            _currentCover = newCover;
            AlignTocover(stateMachine, newCover);
            EvaluatePeekingOptions(stateMachine);

            Debug.Log("[StealthInCoverState] Moved to new cover position");
        }

        /// <summary>
        /// カバーからの環境相互作用
        /// </summary>
        private void PerformCoverAction(DetailedPlayerStateMachine stateMachine)
        {
            if (_stealthService == null) return;

            // カバー位置からの相互作用範囲拡張
            Collider[] interactables = Physics.OverlapSphere(
                stateMachine.transform.position, 3.0f,
                LayerMask.GetMask("Interactable"));

            foreach (var collider in interactables)
            {
                if (collider.CompareTag("SecurityCamera"))
                {
                    _stealthService.InteractWithEnvironment(collider.gameObject, StealthInteractionType.DisableCamera);
                    Debug.Log("[StealthInCoverState] Disabled camera from cover");
                    break;
                }
                else if (collider.CompareTag("Guard"))
                {
                    // 陽動音の作成
                    Vector3 distractionPos = collider.transform.position + Vector3.right * 5f;
                    _stealthService.CreateDistraction(distractionPos, 0.8f);
                    Debug.Log("[StealthInCoverState] Created distraction from cover");
                    break;
                }
            }
        }

        /// <summary>
        /// ピーキング方向
        /// </summary>
        private enum PeekDirection
        {
            Left,
            Right
        }
    }
}