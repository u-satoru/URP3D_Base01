using UnityEngine;
using asterivo.Unity60.Features.Templates.TPS.Cover;

namespace asterivo.Unity60.Features.Templates.TPS.Player.StateMachine.States
{
    /// <summary>
    /// Cover state - Player is taking cover behind objects
    /// Integrates with CoverDetector and CoverPoint systems for tactical positioning
    /// </summary>
    public class CoverState : BasePlayerState
    {
        public override PlayerState StateType => PlayerState.InCover;

        // Cover system components
        private CoverDetector _coverDetector;
        private CoverPoint _currentCoverPoint;

        // Cover state tracking
        private bool _isInCover;
        private bool _isPeeking;
        private bool _isMovingToCover;
        private Vector3 _targetCoverPosition;
        private float _moveToPositionSpeed;

        // Events - TODO: Implement when GameEvent system is available
        // private GameEvent<PlayerState> _playerStateChangedEvent;

        public override void Initialize(TPSPlayerController controller)
        {
            base.Initialize(controller);

            // TODO: Event system integration will be implemented when interfaces are available
            // For now, using direct component communication

            _moveToPositionSpeed = _playerData.WalkSpeed;
        }

        public override void Enter()
        {
            Debug.Log("[CoverState] Entering cover state");

            // Find nearby cover if not already in cover
            if (_currentCoverPoint == null)
            {
                _coverDetector = _controller.GetComponent<CoverDetector>();
                if (_coverDetector == null)
                {
                    _coverDetector = _controller.gameObject.AddComponent<CoverDetector>();
                    _coverDetector.SetPlayerData(_playerData);
                }

                _currentCoverPoint = _coverDetector.FindBestCoverPoint();
                if (_currentCoverPoint == null)
                {
                    Debug.LogWarning("[CoverState] No cover point found, exiting cover state");
                    // Exit cover state if no cover found
                    return;
                }
            }

            // Occupy the cover point
            if (_currentCoverPoint.TryOccupy(_controller.gameObject))
            {
                _isInCover = true;
                _targetCoverPosition = _currentCoverPoint.CoverPosition;

                // Move to cover position
                StartMoveToPosition(_targetCoverPosition);

                // TODO: Raise cover entered event when GameEvent system is implemented
                // if (_playerStateChangedEvent != null)
                // {
                //     _playerStateChangedEvent.Raise(PlayerState.InCover);
                // }
            }
            else
            {
                Debug.LogWarning("[CoverState] Failed to occupy cover point");
            }
        }

        public override void Update()
        {
            if (!_isInCover || _currentCoverPoint == null) return;

            HandleCoverMovement();
            HandlePeeking();
            HandleCoverExit();
        }

        public override void Exit()
        {
            Debug.Log("[CoverState] Exiting cover state");

            // Release the cover point
            if (_currentCoverPoint != null)
            {
                _currentCoverPoint.ReleaseOccupation();
                _currentCoverPoint = null;
            }

            _isInCover = false;
            _isPeeking = false;
            _isMovingToCover = false;

            // TODO: Raise cover exited event when GameEvent system is implemented
            // if (_playerStateChangedEvent != null)
            // {
            //     _playerStateChangedEvent.Raise(PlayerState.Idle);
            // }
        }

        /// <summary>
        /// Handle movement while in cover (repositioning along cover)
        /// </summary>
        private void HandleCoverMovement()
        {
            if (_isMovingToCover)
            {
                // Move towards target cover position
                Vector3 direction = (_targetCoverPosition - _controller.transform.position).normalized;
                float distance = Vector3.Distance(_controller.transform.position, _targetCoverPosition);

                if (distance > 0.1f)
                {
                    ApplyMovement(direction, _moveToPositionSpeed);
                    RotateTowardsMovement(direction, _playerData.RotationSpeed);
                }
                else
                {
                    _isMovingToCover = false;
                    Debug.Log("[CoverState] Reached cover position");
                }
            }
            else
            {
                // Handle movement input for repositioning along cover
                Vector2 movementInput = GetMovementInput();
                if (movementInput.magnitude > 0.1f)
                {
                    // Move along cover direction (simplified implementation)
                    Vector3 moveDirection = GetCameraRelativeMovement(movementInput);
                    ApplyMovement(moveDirection, _playerData.CrouchSpeed); // Slower movement in cover
                }
            }
        }

        /// <summary>
        /// Handle peeking mechanics
        /// </summary>
        private void HandlePeeking()
        {
            // Simplified peeking - could be extended with input mapping
            Vector2 lookInput = GetLookInput();

            // Basic peek logic based on look direction
            if (Mathf.Abs(lookInput.x) > 0.5f && !_isPeeking)
            {
                StartPeeking(lookInput.x > 0);
            }
            else if (Mathf.Abs(lookInput.x) < 0.2f && _isPeeking)
            {
                StopPeeking();
            }
        }

        /// <summary>
        /// Handle exit conditions from cover
        /// </summary>
        private void HandleCoverExit()
        {
            // Exit cover if jump is pressed
            if (IsJumpPressed())
            {
                ExitCover();
                return;
            }

            // Exit cover if sprint is held (for quick movement)
            if (IsSprintHeld())
            {
                ExitCover();
                return;
            }

            // Exit cover if moving away from cover significantly
            Vector2 movementInput = GetMovementInput();
            if (movementInput.magnitude > 0.8f)
            {
                Vector3 moveDirection = GetCameraRelativeMovement(movementInput);
                Vector3 awayFromCover = (_controller.transform.position - _currentCoverPoint.CoverPosition).normalized;

                // If moving away from cover
                if (Vector3.Dot(moveDirection.normalized, awayFromCover) > 0.7f)
                {
                    ExitCover();
                }
            }
        }

        /// <summary>
        /// Start moving to a cover position
        /// </summary>
        private void StartMoveToPosition(Vector3 position)
        {
            _targetCoverPosition = position;
            _isMovingToCover = true;
        }

        /// <summary>
        /// Start peeking from cover
        /// </summary>
        private void StartPeeking(bool peekRight)
        {
            if (_currentCoverPoint == null) return;

            _isPeeking = true;
            Vector3 peekPosition = peekRight ? _currentCoverPoint.GetRightPeekPosition() : _currentCoverPoint.GetLeftPeekPosition();

            // Move to peek position
            StartMoveToPosition(peekPosition);

            Debug.Log($"[CoverState] Started peeking {(peekRight ? "right" : "left")}");
        }

        /// <summary>
        /// Stop peeking and return to cover
        /// </summary>
        private void StopPeeking()
        {
            if (!_isPeeking || _currentCoverPoint == null) return;

            _isPeeking = false;

            // Return to main cover position
            StartMoveToPosition(_currentCoverPoint.CoverPosition);

            Debug.Log("[CoverState] Stopped peeking");
        }

        /// <summary>
        /// Exit cover state
        /// </summary>
        private void ExitCover()
        {
            // This will be handled by the state machine transition logic
            // The actual state change should be managed by TPSPlayerStateMachine
            Debug.Log("[CoverState] Requesting exit from cover");
        }
    }
}
