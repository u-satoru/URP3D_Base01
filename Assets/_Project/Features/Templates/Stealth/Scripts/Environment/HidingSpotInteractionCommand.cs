using UnityEngine;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Features.Templates.Stealth.Environment;

namespace asterivo.Unity60.Features.Templates.Stealth.Commands
{
    /// <summary>
    /// Command for player interactions with hiding spots
    /// Implements Command Pattern with ObjectPool optimization for memory efficiency
    /// Part of Layer 5: Environment Interaction System
    /// </summary>
    public class HidingSpotInteractionCommand : ICommand, IResettableCommand
    {
        // Command state
        private HidingSpot _hidingSpot;
        private bool _isEntering;
        private bool _wasSuccessful;
        private Transform _playerTransform;
        private Vector3 _previousPlayerPosition;
        private StealthState _previousStealthState;

        // Execution context for undo support
        private bool _hasExecuted = false;

        #region ICommand Implementation

        public bool CanUndo => _hasExecuted && _wasSuccessful;

        public void Execute()
        {
            if (_hidingSpot == null)
            {
                Debug.LogWarning("[HidingSpotInteractionCommand] Cannot execute: HidingSpot is null");
                _wasSuccessful = false;
                return;
            }

            // Find player if not already cached
            if (_playerTransform == null)
            {
                var playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                {
                    _playerTransform = playerObject.transform;
                }
                else
                {
                    Debug.LogWarning("[HidingSpotInteractionCommand] Cannot find player");
                    _wasSuccessful = false;
                    return;
                }
            }

            // Store previous state for undo
            _previousPlayerPosition = _playerTransform.position;
            
            // Get previous stealth state from stealth mechanics controller
            var stealthController = ServiceLocator.GetService<StealthMechanicsController>();
            if (stealthController != null)
            {
                _previousStealthState = stealthController.CurrentState;
            }

            // Execute the hiding spot interaction
            if (_isEntering)
            {
                _wasSuccessful = ExecuteEnter();
            }
            else
            {
                _wasSuccessful = ExecuteExit();
            }

            _hasExecuted = true;

            if (_wasSuccessful)
            {
                LogDebug($"Successfully {(_isEntering ? "entered" : "exited")} hiding spot: {_hidingSpot.name}");
            }
            else
            {
                LogDebug($"Failed to {(_isEntering ? "enter" : "exit")} hiding spot: {_hidingSpot.name}");
            }
        }

        public void Undo()
        {
            if (!CanUndo)
            {
                Debug.LogWarning("[HidingSpotInteractionCommand] Cannot undo: Command was not successfully executed");
                return;
            }

            // Reverse the hiding spot interaction
            if (_isEntering)
            {
                // Undo enter by exiting
                _hidingSpot.TryExit(_playerTransform);
            }
            else
            {
                // Undo exit by entering
                _hidingSpot.TryEnter(_playerTransform);
            }

            // Restore previous player position
            if (_playerTransform != null)
            {
                _playerTransform.position = _previousPlayerPosition;
            }

            // Restore previous stealth state
            var stealthController = ServiceLocator.GetService<StealthMechanicsController>();
            if (stealthController != null)
            {
                stealthController.SetState(_previousStealthState);
            }

            LogDebug($"Undid hiding spot interaction for: {_hidingSpot.name}");
        }

        #endregion

        #region IResettableCommand Implementation

        public void Reset()
        {
            _hidingSpot = null;
            _isEntering = false;
            _wasSuccessful = false;
            _playerTransform = null;
            _previousPlayerPosition = Vector3.zero;
            _previousStealthState = StealthState.Visible;
            _hasExecuted = false;
        }

        #endregion

        #region Constructor and Initialization

        /// <summary>
        /// Default constructor for ObjectPool creation
        /// </summary>
        public HidingSpotInteractionCommand()
        {
            Reset();
        }

        /// <summary>
        /// Initialize the command with specific parameters
        /// </summary>
        public HidingSpotInteractionCommand(HidingSpot hidingSpot, bool isEntering)
        {
            Initialize(hidingSpot, isEntering);
        }

        /// <summary>
        /// Initialize or reinitialize the command (for ObjectPool reuse)
        /// </summary>
        public void Initialize(HidingSpot hidingSpot, bool isEntering)
        {
            Reset();
            _hidingSpot = hidingSpot;
            _isEntering = isEntering;
        }

        /// <summary>
        /// Initialize with generic parameters (IResettableCommand interface requirement)
        /// </summary>
        public void Initialize(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
            {
                Debug.LogError("[HidingSpotInteractionCommand] Initialize requires at least 2 parameters: HidingSpot and bool");
                return;
            }

            if (parameters[0] is HidingSpot hidingSpot && parameters[1] is bool isEntering)
            {
                Initialize(hidingSpot, isEntering);
            }
            else
            {
                Debug.LogError("[HidingSpotInteractionCommand] Invalid parameter types. Expected HidingSpot and bool");
            }
        }

        #endregion

        #region Execution Implementation

        private bool ExecuteEnter()
        {
            // Check if hiding spot is available
            if (!_hidingSpot.IsAvailable)
            {
                LogDebug($"Hiding spot {_hidingSpot.name} is not available (occupied: {_hidingSpot.CurrentOccupants}/{_hidingSpot.Capacity})");
                return false;
            }

            // Check distance if required
            var distance = Vector3.Distance(_playerTransform.position, _hidingSpot.transform.position);
            if (distance > _hidingSpot.InfluenceRadius)
            {
                LogDebug($"Player too far from hiding spot {_hidingSpot.name} (distance: {distance:F1}, required: {_hidingSpot.InfluenceRadius:F1})");
                return false;
            }

            // Try to enter the hiding spot
            bool entered = _hidingSpot.TryEnter(_playerTransform);
            if (!entered)
            {
                LogDebug($"Failed to enter hiding spot {_hidingSpot.name}");
                return false;
            }

            // Update stealth mechanics
            var stealthController = ServiceLocator.GetService<StealthMechanicsController>();
            if (stealthController != null)
            {
                var concealment = _hidingSpot.GetConcealmentAt(_playerTransform.position);
                stealthController.EnterHidingSpot(_hidingSpot, concealment);
            }

            // Notify audio system about concealment
            NotifyAudioSystemConcealment(true);

            return true;
        }

        private bool ExecuteExit()
        {
            // Try to exit the hiding spot
            bool exited = _hidingSpot.TryExit(_playerTransform);
            if (!exited)
            {
                LogDebug($"Failed to exit hiding spot {_hidingSpot.name}");
                return false;
            }

            // Update stealth mechanics
            var stealthController = ServiceLocator.GetService<StealthMechanicsController>();
            if (stealthController != null)
            {
                stealthController.ExitHidingSpot(_hidingSpot);
            }

            // Notify audio system about loss of concealment
            NotifyAudioSystemConcealment(false);

            return true;
        }

        #endregion

        #region System Integration

        private void NotifyAudioSystemConcealment(bool isConcealed)
        {
            // Integrate with stealth audio system
            var audioCoordinator = ServiceLocator.GetService<StealthAudioCoordinator>();
            if (audioCoordinator != null)
            {
                if (isConcealed)
                {
                    audioCoordinator.OnPlayerConcealed(_hidingSpot.ConcealmentLevel);
                }
                else
                {
                    audioCoordinator.OnPlayerExposed();
                }
            }
        }

        #endregion

        #region Debug Support

        private void LogDebug(string message)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[HidingSpotInteractionCommand] {message}");
            #endif
        }

        #endregion

        #region Static Factory Methods (ObjectPool Integration)

        /// <summary>
        /// Create a command for entering a hiding spot (ObjectPool optimized)
        /// </summary>
        public static HidingSpotInteractionCommand CreateEnterCommand(HidingSpot hidingSpot)
        {
            var poolManager = CommandPoolManager.Instance;
            if (poolManager != null)
            {
                var command = poolManager.GetCommand<HidingSpotInteractionCommand>();
                command.Initialize(hidingSpot, true);
                return command;
            }

            // Fallback to direct instantiation
            return new HidingSpotInteractionCommand(hidingSpot, true);
        }

        /// <summary>
        /// Create a command for exiting a hiding spot (ObjectPool optimized)
        /// </summary>
        public static HidingSpotInteractionCommand CreateExitCommand(HidingSpot hidingSpot)
        {
            var poolManager = CommandPoolManager.Instance;
            if (poolManager != null)
            {
                var command = poolManager.GetCommand<HidingSpotInteractionCommand>();
                command.Initialize(hidingSpot, false);
                return command;
            }

            // Fallback to direct instantiation
            return new HidingSpotInteractionCommand(hidingSpot, false);
        }

        #endregion
    }
}