using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Stealth.Services;
using asterivo.Unity60.Features.Player.States;

namespace asterivo.Unity60.Features.Templates.Stealth.Commands
{
    /// <summary>
    /// ステルス移動コマンドシステム
    /// プレイヤーのステルス移動アクションをカプセル化
    /// ServiceLocator統合による中央制御とObjectPool最適化対応
    /// </summary>
    public class StealthMovementCommand : IResettableCommand
    {
        /// <summary>
        /// ステルス移動の種類
        /// </summary>
        public enum StealthMovementType
        {
            SneakMode,          // 忍び足モード開始
            CrouchWalk,         // しゃがみ歩き
            ProneMovement,      // 匍匐移動
            QuickHide,          // 緊急隠蔽
            SilentSprint,       // 無音疾走
            WallHug,            // 壁沿い移動
            CoverTocover,       // カバー間移動
            StealthClimb,       // ステルス登攀
            ShadowMove,         // 影移動
            DistractionMove     // 陽動移動
        }

        private StealthMovementType _movementType;
        private Vector3 _targetPosition;
        private float _duration;
        private float _speedMultiplier;
        private bool _maintainStealth;

        // 前回の状態（Undo用）
        private Vector3 _previousPosition;
        private PlayerStateType _previousPlayerState;
        private float _previousVisibility;
        private float _previousNoiseLevel;

        // サービス参照
        private IStealthService _stealthService;
        private DetailedPlayerStateMachine _playerStateMachine;
        private Transform _playerTransform;

        // 実行結果
        private bool _wasExecuted;
        private bool _stealthMaintained;

        #region ICommand Implementation

        public void Execute()
        {
            if (_wasExecuted) return;

            // ServiceLocator経由でサービス取得
            _stealthService = ServiceLocator.GetService<IStealthService>();
            _playerStateMachine = Object.FindObjectOfType<DetailedPlayerStateMachine>();

            if (_playerStateMachine != null)
            {
                _playerTransform = _playerStateMachine.transform;
            }

            if (_stealthService == null || _playerStateMachine == null)
            {
                Debug.LogError("[StealthMovementCommand] Required services not available");
                return;
            }

            // 現在の状態を保存（Undo用）
            _previousPosition = _playerTransform.position;
            _previousPlayerState = _playerStateMachine.GetCurrentStateType();
            _previousVisibility = _stealthService.PlayerVisibilityFactor;
            _previousNoiseLevel = _stealthService.PlayerNoiseLevel;

            // ステルス移動実行
            ExecuteStealthMovement();
            _wasExecuted = true;

            Debug.Log($"[StealthMovementCommand] Executed: {_movementType} to {_targetPosition}");
        }

        public void Undo()
        {
            if (!_wasExecuted || !CanUndo) return;

            // 位置復元
            if (_playerTransform != null)
            {
                _playerTransform.position = _previousPosition;
            }

            // プレイヤー状態復元
            if (_playerStateMachine != null)
            {
                _playerStateMachine.TransitionToState(_previousPlayerState);
            }

            // ステルス状態復元
            if (_stealthService != null)
            {
                _stealthService.UpdatePlayerVisibility(_previousVisibility);
                _stealthService.UpdatePlayerNoiseLevel(_previousNoiseLevel);
            }

            _wasExecuted = false;
            Debug.Log($"[StealthMovementCommand] Undid: {_movementType}");
        }

        public bool CanUndo => _wasExecuted && _playerTransform != null;

        #endregion

        #region IResettableCommand Implementation

        public void Reset()
        {
            _movementType = StealthMovementType.SneakMode;
            _targetPosition = Vector3.zero;
            _duration = 1.0f;
            _speedMultiplier = 1.0f;
            _maintainStealth = true;

            _previousPosition = Vector3.zero;
            _previousPlayerState = PlayerStateType.Idle;
            _previousVisibility = 1.0f;
            _previousNoiseLevel = 0.0f;

            _stealthService = null;
            _playerStateMachine = null;
            _playerTransform = null;

            _wasExecuted = false;
            _stealthMaintained = false;
        }

        public void Initialize(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
            {
                Debug.LogError("[StealthMovementCommand] Invalid parameters. Expected: (StealthMovementType, Vector3, [float], [float], [bool])");
                return;
            }

            _movementType = (StealthMovementType)parameters[0];
            _targetPosition = (Vector3)parameters[1];

            // オプションパラメータ
            _duration = parameters.Length > 2 ? (float)parameters[2] : 1.0f;
            _speedMultiplier = parameters.Length > 3 ? (float)parameters[3] : 1.0f;
            _maintainStealth = parameters.Length > 4 ? (bool)parameters[4] : true;
        }

        /// <summary>
        /// 型安全な初期化メソッド
        /// </summary>
        public void Initialize(StealthMovementType movementType, Vector3 targetPosition,
                              float duration = 1.0f, float speedMultiplier = 1.0f, bool maintainStealth = true)
        {
            _movementType = movementType;
            _targetPosition = targetPosition;
            _duration = duration;
            _speedMultiplier = speedMultiplier;
            _maintainStealth = maintainStealth;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// ステルス移動の具体的実行
        /// </summary>
        private void ExecuteStealthMovement()
        {
            switch (_movementType)
            {
                case StealthMovementType.SneakMode:
                    ExecuteSneakMode();
                    break;

                case StealthMovementType.CrouchWalk:
                    ExecuteCrouchWalk();
                    break;

                case StealthMovementType.ProneMovement:
                    ExecuteProneMovement();
                    break;

                case StealthMovementType.QuickHide:
                    ExecuteQuickHide();
                    break;

                case StealthMovementType.SilentSprint:
                    ExecuteSilentSprint();
                    break;

                case StealthMovementType.WallHug:
                    ExecuteWallHug();
                    break;

                case StealthMovementType.CoverTocover:
                    ExecuteCoverTocover();
                    break;

                case StealthMovementType.StealthClimb:
                    ExecuteStealthClimb();
                    break;

                case StealthMovementType.ShadowMove:
                    ExecuteShadowMove();
                    break;

                case StealthMovementType.DistractionMove:
                    ExecuteDistractionMove();
                    break;
            }
        }

        private void ExecuteSneakMode()
        {
            // 忍び足モード：視認性30%削減、移動速度70%削減
            _playerStateMachine.TransitionToState(PlayerStateType.Walking);
            _stealthService.UpdatePlayerVisibility(_previousVisibility * 0.7f);
            _stealthService.UpdatePlayerNoiseLevel(_previousNoiseLevel * 0.3f);

            MoveToTarget(0.3f); // 30%速度
            _stealthMaintained = true;
        }

        private void ExecuteCrouchWalk()
        {
            // しゃがみ歩き：視認性50%削減、移動速度50%削減
            _playerStateMachine.TransitionToState(PlayerStateType.Crouching);
            _stealthService.UpdatePlayerVisibility(_previousVisibility * 0.5f);
            _stealthService.UpdatePlayerNoiseLevel(_previousNoiseLevel * 0.2f);

            MoveToTarget(0.5f); // 50%速度
            _stealthMaintained = true;
        }

        private void ExecuteProneMovement()
        {
            // 匍匐移動：視認性80%削減、移動速度90%削減
            _playerStateMachine.TransitionToState(PlayerStateType.Prone);
            _stealthService.UpdatePlayerVisibility(_previousVisibility * 0.2f);
            _stealthService.UpdatePlayerNoiseLevel(_previousNoiseLevel * 0.1f);

            MoveToTarget(0.1f); // 10%速度
            _stealthMaintained = true;
        }

        private void ExecuteQuickHide()
        {
            // 緊急隠蔽：最寄りの隠蔽ポイントへ移動
            Vector3 hidePosition = FindNearestHidingSpot();
            if (hidePosition != Vector3.zero)
            {
                _targetPosition = hidePosition;
                _playerStateMachine.TransitionToState(PlayerStateType.Crouching);
                _stealthService.UpdatePlayerVisibility(0.1f); // 90%視認性削減

                MoveToTarget(1.5f); // 150%速度（緊急）
                _stealthMaintained = true;
            }
        }

        private void ExecuteSilentSprint()
        {
            // 無音疾走：高速移動だが音響制御
            _playerStateMachine.TransitionToState(PlayerStateType.Running);
            _stealthService.UpdatePlayerVisibility(_previousVisibility * 1.2f); // 20%視認性増加
            _stealthService.UpdatePlayerNoiseLevel(_previousNoiseLevel * 0.4f); // 60%騒音削減

            MoveToTarget(2.0f); // 200%速度
            _stealthMaintained = _maintainStealth;
        }

        private void ExecuteWallHug()
        {
            // 壁沿い移動：壁に沿った安全な移動
            Vector3 wallDirection = FindWallDirection();
            if (wallDirection != Vector3.zero)
            {
                _playerStateMachine.TransitionToState(PlayerStateType.Crouching);
                _stealthService.UpdatePlayerVisibility(_previousVisibility * 0.6f);
                _stealthService.UpdatePlayerNoiseLevel(_previousNoiseLevel * 0.3f);

                MoveAlongWall(wallDirection);
                _stealthMaintained = true;
            }
        }

        private void ExecuteCoverTocover()
        {
            // カバー間移動：遮蔽物間の戦術的移動
            if (ValidateCoverPath())
            {
                _playerStateMachine.TransitionToState(PlayerStateType.InCover);
                _stealthService.UpdatePlayerVisibility(_previousVisibility * 0.4f);
                _stealthService.UpdatePlayerNoiseLevel(_previousNoiseLevel * 0.2f);

                MoveToTarget(1.0f);
                _stealthMaintained = true;
            }
        }

        private void ExecuteStealthClimb()
        {
            // ステルス登攀：静かな登攀移動
            _stealthService.UpdatePlayerVisibility(_previousVisibility * 0.8f);
            _stealthService.UpdatePlayerNoiseLevel(_previousNoiseLevel * 0.5f);

            ClimbToTarget();
            _stealthMaintained = true;
        }

        private void ExecuteShadowMove()
        {
            // 影移動：影を利用した移動
            if (IsInShadow(_targetPosition))
            {
                _playerStateMachine.TransitionToState(PlayerStateType.Crouching);
                _stealthService.UpdatePlayerVisibility(_previousVisibility * 0.3f); // 70%視認性削減
                _stealthService.UpdatePlayerNoiseLevel(_previousNoiseLevel * 0.2f);

                MoveToTarget(0.8f);
                _stealthMaintained = true;
            }
        }

        private void ExecuteDistractionMove()
        {
            // 陽動移動：注意を逸らしながらの移動
            CreateDistraction();
            _playerStateMachine.TransitionToState(PlayerStateType.Crouching);
            _stealthService.UpdatePlayerVisibility(_previousVisibility * 0.9f);

            MoveToTarget(1.2f);
            _stealthMaintained = _maintainStealth;
        }

        #endregion

        #region Helper Methods

        private void MoveToTarget(float speedModifier)
        {
            if (_playerTransform == null) return;

            float adjustedSpeed = _speedMultiplier * speedModifier;
            Vector3 direction = (_targetPosition - _playerTransform.position).normalized;

            // 簡易移動実装（実際にはCharacterControllerやNavMeshAgentを使用）
            _playerTransform.position = Vector3.MoveTowards(
                _playerTransform.position,
                _targetPosition,
                adjustedSpeed * Time.deltaTime
            );
        }

        private Vector3 FindNearestHidingSpot()
        {
            // 最寄りの隠蔽ポイントを探索
            GameObject[] hidingSpots = GameObject.FindGameObjectsWithTag("HidingSpot");
            Vector3 nearestSpot = Vector3.zero;
            float nearestDistance = float.MaxValue;

            foreach (GameObject spot in hidingSpots)
            {
                float distance = Vector3.Distance(_playerTransform.position, spot.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestSpot = spot.transform.position;
                }
            }

            return nearestSpot;
        }

        private Vector3 FindWallDirection()
        {
            // 壁方向の検出（レイキャストによる簡易実装）
            RaycastHit hit;
            Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

            foreach (Vector3 dir in directions)
            {
                if (Physics.Raycast(_playerTransform.position, dir, out hit, 2.0f))
                {
                    if (hit.collider.CompareTag("Wall"))
                    {
                        return Vector3.Cross(hit.normal, Vector3.up);
                    }
                }
            }

            return Vector3.zero;
        }

        private void MoveAlongWall(Vector3 wallDirection)
        {
            Vector3 targetAlongWall = _playerTransform.position + wallDirection * 5.0f;
            MoveToTarget(0.8f);
        }

        private bool ValidateCoverPath()
        {
            // カバー間の経路検証
            RaycastHit hit;
            Vector3 direction = (_targetPosition - _playerTransform.position).normalized;
            float distance = Vector3.Distance(_playerTransform.position, _targetPosition);

            return !Physics.Raycast(_playerTransform.position, direction, out hit, distance, LayerMask.GetMask("Enemy"));
        }

        private void ClimbToTarget()
        {
            // 登攀移動の実装
            Vector3 climbDirection = Vector3.up + (_targetPosition - _playerTransform.position).normalized;
            _playerTransform.position = Vector3.MoveTowards(
                _playerTransform.position,
                _targetPosition,
                _speedMultiplier * 0.5f * Time.deltaTime
            );
        }

        private bool IsInShadow(Vector3 position)
        {
            // 影にいるかどうかの判定（簡易実装）
            return _stealthService?.IsPlayerConcealed ?? false;
        }

        private void CreateDistraction()
        {
            // 陽動音の作成
            Vector3 distractionPoint = _playerTransform.position + Vector3.right * 5.0f;
            _stealthService?.CreateDistraction(distractionPoint, 0.6f);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// 移動タイプ
        /// </summary>
        public StealthMovementType MovementType => _movementType;

        /// <summary>
        /// 目標位置
        /// </summary>
        public Vector3 TargetPosition => _targetPosition;

        /// <summary>
        /// ステルスが維持されたかどうか
        /// </summary>
        public bool StealthMaintained => _stealthMaintained;

        /// <summary>
        /// 実行済みかどうか
        /// </summary>
        public bool WasExecuted => _wasExecuted;

        #endregion
    }
}
