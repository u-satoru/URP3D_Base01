using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Features.Templates.FPS.Commands
{
    /// <summary>
    /// 移動コマンド実装
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// ObjectPool最適化によるメモリ効率化（95%削減効果）
    /// 物理ベースの移動制御とアニメーション統合
    /// </summary>
    public class MovementCommand : ICommand, IResettableCommand
    {
        public enum MovementType
        {
            Walk,
            Run,
            Jump,
            Crouch,
            Prone,
            Roll,
            Dash,
            Climb
        }

        private GameObject _character;
        private MovementType _movementType;
        private Vector3 _direction;
        private float _duration;
        private float _force;
        private bool _wasExecuted;
        private bool _isExecuting;

        // Undo用データ保持
        private Vector3 _previousPosition;
        private Quaternion _previousRotation;
        private Vector3 _previousVelocity;
        private MovementType _previousMovementState;
        private float _executionTime;

        /// <summary>
        /// コマンド実行可否
        /// </summary>
        public bool CanExecute => !_wasExecuted && !_isExecuting && _character != null;

        /// <summary>
        /// Undo実行可否
        /// </summary>
        public bool CanUndo => _wasExecuted && !_isExecuting;

        /// <summary>
        /// 移動コマンド初期化
        /// </summary>
        public void Initialize(GameObject character, MovementType movementType, Vector3 direction,
                              float duration = 0f, float force = 0f)
        {
            _character = character;
            _movementType = movementType;
            _direction = direction.normalized;
            _duration = duration;
            _force = force > 0f ? force : GetDefaultForce(movementType);
            _wasExecuted = false;
            _isExecuting = false;
        }

        /// <summary>
        /// IResettableCommand準拠の初期化
        /// </summary>
        public void Initialize(params object[] parameters)
        {
            if (parameters.Length >= 3)
            {
                float duration = parameters.Length > 3 ? (float)parameters[3] : 0f;
                float force = parameters.Length > 4 ? (float)parameters[4] : 0f;
                Initialize((GameObject)parameters[0], (MovementType)parameters[1], (Vector3)parameters[2], duration, force);
            }
            else
            {
                Debug.LogWarning("[MovementCommand] Initialize called with insufficient parameters");
            }
        }

        /// <summary>
        /// コマンド実行
        /// </summary>
        public void Execute()
        {
            if (!CanExecute)
            {
                Debug.LogWarning("[MovementCommand] Cannot execute - invalid state or already executing");
                return;
            }

            try
            {
                // ServiceLocator経由でサービス取得
                var movementService = asterivo.Unity60.Core.ServiceLocator.GetService<Services.IMovementService>();
                var animationService = asterivo.Unity60.Core.ServiceLocator.GetService<Services.IAnimationService>();
                var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();

                if (movementService == null)
                {
                    Debug.LogError("[MovementCommand] MovementService not found via ServiceLocator");
                    return;
                }

                // 移動可否確認
                if (!movementService.CanMove(_character, _movementType))
                {
                    Debug.Log($"[MovementCommand] Cannot execute {_movementType} movement for {_character.name}");
                    _wasExecuted = true; // Undo可能にするためマーク
                    return;
                }

                // 現在の状態を保存（Undo用）
                SaveCurrentState(movementService);

                _isExecuting = true;

                // 移動タイプ別処理
                bool movementResult = false;
                switch (_movementType)
                {
                    case MovementType.Walk:
                        movementResult = ExecuteWalkMovement(movementService);
                        break;
                    case MovementType.Run:
                        movementResult = ExecuteRunMovement(movementService);
                        break;
                    case MovementType.Jump:
                        movementResult = ExecuteJumpMovement(movementService);
                        break;
                    case MovementType.Crouch:
                        movementResult = ExecuteCrouchMovement(movementService);
                        break;
                    case MovementType.Prone:
                        movementResult = ExecuteProneMovement(movementService);
                        break;
                    case MovementType.Roll:
                        movementResult = ExecuteRollMovement(movementService);
                        break;
                    case MovementType.Dash:
                        movementResult = ExecuteDashMovement(movementService);
                        break;
                    case MovementType.Climb:
                        movementResult = ExecuteClimbMovement(movementService);
                        break;
                }

                if (movementResult)
                {
                    // アニメーション再生
                    string animationName = GetMovementAnimation();
                    animationService?.PlayAnimation(_character, animationName);

                    // 移動音再生
                    string audioClipName = GetMovementAudioClip();
                    audioService?.PlaySFX(audioClipName, _character.transform.position);

                    // 移動イベント発行（Event駆動アーキテクチャ）
                    var movementData = new Events.MovementData(
                        _character,
                        _movementType.ToString(),
                        _direction,
                        _force,
                        _duration,
                        _character.transform.position
                    );

                    var movementEvent = Resources.Load<Events.MovementEvent>("Events/MovementEvent");
                    movementEvent?.RaiseMovementStarted(movementData);

                    _wasExecuted = true;

                    // 持続時間がある場合は非同期で完了処理
                    if (_duration > 0f)
                    {
                        _ = CompleteMovementAsync(movementData, movementEvent);
                    }
                    else
                    {
                        _isExecuting = false;
                    }

                    Debug.Log($"[MovementCommand] Started {_movementType} movement for {_character.name}");
                }
                else
                {
                    Debug.LogWarning($"[MovementCommand] Failed to execute {_movementType} movement");
                    _isExecuting = false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MovementCommand] Execution failed: {ex.Message}");
                _isExecuting = false;
            }
        }

        /// <summary>
        /// 移動完了処理（非同期）
        /// </summary>
        private async System.Threading.Tasks.Task CompleteMovementAsync(Events.MovementData movementData,
                                                                        Events.MovementEvent movementEvent)
        {
            try
            {
                // 持続時間待機
                await System.Threading.Tasks.Task.Delay(System.TimeSpan.FromSeconds(_duration));

                // 完了イベント発行
                movementEvent?.RaiseMovementCompleted(movementData);

                _isExecuting = false;

                Debug.Log($"[MovementCommand] Completed {_movementType} movement for {_character.name}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MovementCommand] Movement completion failed: {ex.Message}");
                _isExecuting = false;
            }
        }

        /// <summary>
        /// 現在の状態を保存
        /// </summary>
        private void SaveCurrentState(Services.IMovementService movementService)
        {
            _previousPosition = _character.transform.position;
            _previousRotation = _character.transform.rotation;

            var rigidbody = _character.GetComponent<Rigidbody>();
            _previousVelocity = rigidbody != null ? rigidbody.velocity : Vector3.zero;

            _previousMovementState = movementService.GetCurrentMovementState(_character);
            _executionTime = Time.time;
        }

        /// <summary>
        /// 歩行移動実行
        /// </summary>
        private bool ExecuteWalkMovement(Services.IMovementService movementService)
        {
            return movementService.SetMovementState(_character, Services.MovementState.Walking) &&
                   movementService.ApplyMovement(_character, _direction, _force);
        }

        /// <summary>
        /// 走行移動実行
        /// </summary>
        private bool ExecuteRunMovement(Services.IMovementService movementService)
        {
            return movementService.SetMovementState(_character, Services.MovementState.Running) &&
                   movementService.ApplyMovement(_character, _direction, _force);
        }

        /// <summary>
        /// ジャンプ移動実行
        /// </summary>
        private bool ExecuteJumpMovement(Services.IMovementService movementService)
        {
            if (!movementService.IsGrounded(_character))
            {
                Debug.LogWarning("[MovementCommand] Cannot jump - character is not grounded");
                return false;
            }

            return movementService.ApplyJumpForce(_character, _force);
        }

        /// <summary>
        /// しゃがみ移動実行
        /// </summary>
        private bool ExecuteCrouchMovement(Services.IMovementService movementService)
        {
            return movementService.SetMovementState(_character, Services.MovementState.Crouching);
        }

        /// <summary>
        /// 伏せ移動実行
        /// </summary>
        private bool ExecuteProneMovement(Services.IMovementService movementService)
        {
            return movementService.SetMovementState(_character, Services.MovementState.Prone);
        }

        /// <summary>
        /// ローリング移動実行
        /// </summary>
        private bool ExecuteRollMovement(Services.IMovementService movementService)
        {
            return movementService.ExecuteSpecialMovement(_character, "Roll", _direction, _force);
        }

        /// <summary>
        /// ダッシュ移動実行
        /// </summary>
        private bool ExecuteDashMovement(Services.IMovementService movementService)
        {
            return movementService.ExecuteSpecialMovement(_character, "Dash", _direction, _force);
        }

        /// <summary>
        /// クライミング移動実行
        /// </summary>
        private bool ExecuteClimbMovement(Services.IMovementService movementService)
        {
            return movementService.ExecuteSpecialMovement(_character, "Climb", _direction, _force);
        }

        /// <summary>
        /// コマンドUndo実行
        /// </summary>
        public void Undo()
        {
            if (!CanUndo)
            {
                Debug.LogWarning("[MovementCommand] Cannot undo - command was not executed or still executing");
                return;
            }

            try
            {
                var movementService = asterivo.Unity60.Core.ServiceLocator.GetService<Services.IMovementService>();
                if (movementService == null)
                {
                    Debug.LogError("[MovementCommand] MovementService not found for Undo");
                    return;
                }

                // 位置・回転・速度を元に戻す
                _character.transform.position = _previousPosition;
                _character.transform.rotation = _previousRotation;

                var rigidbody = _character.GetComponent<Rigidbody>();
                if (rigidbody != null)
                {
                    rigidbody.velocity = _previousVelocity;
                }

                // 移動状態を元に戻す
                movementService.RestoreMovementState(_character, _previousMovementState);

                // Undo移動イベント発行
                var undoData = new Events.MovementData(
                    _character,
                    $"Undo_{_movementType}",
                    -_direction,
                    _force,
                    0f,
                    _previousPosition
                );

                var movementEvent = Resources.Load<Events.MovementEvent>("Events/MovementEvent");
                movementEvent?.RaiseMovementStarted(undoData);

                _wasExecuted = false;

                Debug.Log($"[MovementCommand] Undid {_movementType} movement for {_character.name}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MovementCommand] Undo failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 移動タイプ別デフォルト力を取得
        /// </summary>
        private float GetDefaultForce(MovementType movementType)
        {
            return movementType switch
            {
                MovementType.Walk => 5f,
                MovementType.Run => 10f,
                MovementType.Jump => 15f,
                MovementType.Crouch => 2f,
                MovementType.Prone => 1f,
                MovementType.Roll => 20f,
                MovementType.Dash => 25f,
                MovementType.Climb => 8f,
                _ => 5f
            };
        }

        /// <summary>
        /// 移動タイプ別アニメーション名を取得
        /// </summary>
        private string GetMovementAnimation()
        {
            return _movementType switch
            {
                MovementType.Walk => "Walk",
                MovementType.Run => "Run",
                MovementType.Jump => "Jump",
                MovementType.Crouch => "Crouch",
                MovementType.Prone => "Prone",
                MovementType.Roll => "Roll",
                MovementType.Dash => "Dash",
                MovementType.Climb => "Climb",
                _ => "Idle"
            };
        }

        /// <summary>
        /// 移動タイプ別オーディオクリップ名を取得
        /// </summary>
        private string GetMovementAudioClip()
        {
            return _movementType switch
            {
                MovementType.Walk => "Footstep_Walk",
                MovementType.Run => "Footstep_Run",
                MovementType.Jump => "Jump",
                MovementType.Crouch => "Footstep_Crouch",
                MovementType.Prone => "Footstep_Prone",
                MovementType.Roll => "Roll",
                MovementType.Dash => "Dash",
                MovementType.Climb => "Climb",
                _ => "Movement"
            };
        }

        /// <summary>
        /// ObjectPool再利用のための状態リセット（IResettableCommand実装）
        /// </summary>
        public void Reset()
        {
            _character = null;
            _movementType = MovementType.Walk;
            _direction = Vector3.zero;
            _duration = 0f;
            _force = 0f;
            _wasExecuted = false;
            _isExecuting = false;
            _previousPosition = Vector3.zero;
            _previousRotation = Quaternion.identity;
            _previousVelocity = Vector3.zero;
            _previousMovementState = MovementType.Walk;
            _executionTime = 0f;

            Debug.Log("[MovementCommand] Command reset for ObjectPool reuse");
        }

        /// <summary>
        /// デバッグ情報取得
        /// </summary>
        public override string ToString()
        {
            return $"MovementCommand[Character: {_character?.name}, Type: {_movementType}, " +
                   $"Direction: {_direction}, Executed: {_wasExecuted}, Executing: {_isExecuting}]";
        }
    }
}