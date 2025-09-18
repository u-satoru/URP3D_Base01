using UnityEngine;

namespace asterivo.Unity60.Features.Templates.TPS.Cover
{
    /// <summary>
    /// Cover Point - Defines a cover position and direction for TPS cover system
    /// Follows DESIGN.md Feature layer principles and architecture separation
    /// </summary>
    [System.Serializable]
    public class CoverPoint : MonoBehaviour
    {
        [Header("Cover Configuration")]
        [SerializeField] private Vector3 _coverPosition = Vector3.zero;
        [SerializeField] private Vector3 _forwardDirection = Vector3.forward;
        [SerializeField] private float _coverWidth = 1.0f;
        [SerializeField] private float _coverHeight = 2.0f;
        [SerializeField] private bool _isLeftEdgeCover = true;
        [SerializeField] private bool _isRightEdgeCover = true;
        
        [Header("Cover Properties")]
        [SerializeField] private CoverType _coverType = CoverType.Full;
        [SerializeField] private bool _allowPeeking = true;
        [SerializeField] private bool _allowBlindFire = false;
        [SerializeField] private float _peekDistance = 0.5f;
        
        [Header("Debug Visualization")]
        [SerializeField] private bool _showDebugGizmos = true;
        [SerializeField] private Color _gizmoColor = Color.green;
        [SerializeField] private Color _peekGizmoColor = Color.yellow;

        // State tracking
        private bool _isOccupied = false;
        private GameObject _occupant = null;
        
        // Properties for external access
        public Vector3 CoverPosition => transform.position + _coverPosition;
        public Vector3 ForwardDirection => transform.TransformDirection(_forwardDirection.normalized);
        public Vector3 RightDirection => Vector3.Cross(Vector3.up, ForwardDirection);
        public Vector3 LeftDirection => -RightDirection;
        public float CoverWidth => _coverWidth;
        public float CoverHeight => _coverHeight;
        public CoverType Type => _coverType;
        public bool AllowPeeking => _allowPeeking;
        public bool AllowBlindFire => _allowBlindFire;
        public bool IsOccupied => _isOccupied;
        public GameObject Occupant => _occupant;
        public bool IsLeftEdgeCover => _isLeftEdgeCover;
        public bool IsRightEdgeCover => _isRightEdgeCover;

        private void Awake()
        {
            // Ensure forward direction is normalized
            _forwardDirection = _forwardDirection.normalized;
            
            // Auto-adjust cover position to be at ground level if not set
            if (_coverPosition == Vector3.zero)
            {
                _coverPosition = new Vector3(0, 0, 0);
            }
        }

        /// <summary>
        /// Check if this cover point can be occupied
        /// </summary>
        public bool CanBeOccupied()
        {
            return !_isOccupied && gameObject.activeInHierarchy;
        }

        /// <summary>
        /// Try to occupy this cover point
        /// </summary>
        public bool TryOccupy(GameObject occupant)
        {
            if (!CanBeOccupied()) return false;

            _isOccupied = true;
            _occupant = occupant;
            
            Debug.Log($"[CoverPoint] {occupant.name} occupied cover point at {CoverPosition}");
            return true;
        }

        /// <summary>
        /// Release occupation of this cover point
        /// </summary>
        public void ReleaseOccupation()
        {
            if (_occupant != null)
            {
                Debug.Log($"[CoverPoint] {_occupant.name} released cover point at {CoverPosition}");
            }
            
            _isOccupied = false;
            _occupant = null;
        }

        /// <summary>
        /// Get the peek position for left side peeking
        /// </summary>
        public Vector3 GetLeftPeekPosition()
        {
            if (!_allowPeeking || !_isLeftEdgeCover) return CoverPosition;
            
            return CoverPosition + LeftDirection * _peekDistance;
        }

        /// <summary>
        /// Get the peek position for right side peeking
        /// </summary>
        public Vector3 GetRightPeekPosition()
        {
            if (!_allowPeeking || !_isRightEdgeCover) return CoverPosition;
            
            return CoverPosition + RightDirection * _peekDistance;
        }

        /// <summary>
        /// Calculate the best approach position for entering this cover
        /// </summary>
        public Vector3 GetApproachPosition(Vector3 fromPosition)
        {
            // Calculate approach from behind the cover
            Vector3 approachDirection = -ForwardDirection;
            Vector3 approachPosition = CoverPosition + approachDirection * 1.5f;
            
            return approachPosition;
        }

        /// <summary>
        /// Check if a position is within cover protection
        /// </summary>
        public bool IsPositionInCover(Vector3 position, Vector3 threatDirection)
        {
            // Calculate relative position to cover
            Vector3 toCover = CoverPosition - position;
            Vector3 projectedThreat = Vector3.Project(threatDirection.normalized, ForwardDirection);
            
            // If threat is coming from behind the cover, position is protected
            return Vector3.Dot(projectedThreat, ForwardDirection) > 0.5f && 
                   toCover.magnitude <= _coverWidth * 0.5f;
        }

        /// <summary>
        /// Get cover effectiveness against a threat direction (0-1)
        /// </summary>
        public float GetCoverEffectiveness(Vector3 threatDirection)
        {
            float alignment = Vector3.Dot(threatDirection.normalized, ForwardDirection);
            
            // Full effectiveness when threat is directly in front of cover
            // Reduced effectiveness when threat is from the side
            return Mathf.Clamp01(alignment);
        }

        private void OnValidate()
        {
            // Ensure forward direction is normalized
            if (_forwardDirection != Vector3.zero)
            {
                _forwardDirection = _forwardDirection.normalized;
            }
            
            // Ensure reasonable values
            _coverWidth = Mathf.Max(0.5f, _coverWidth);
            _coverHeight = Mathf.Max(1.0f, _coverHeight);
            _peekDistance = Mathf.Max(0.1f, _peekDistance);
        }

        private void OnDrawGizmos()
        {
            if (!_showDebugGizmos) return;

            // Draw cover position and direction
            Gizmos.color = _gizmoColor;
            Vector3 coverPos = CoverPosition;
            
            // Draw cover box
            Gizmos.DrawWireCube(coverPos, new Vector3(_coverWidth, _coverHeight, 0.2f));
            
            // Draw forward direction
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(coverPos, ForwardDirection * 2.0f);
            
            // Draw peek positions if allowed
            if (_allowPeeking)
            {
                Gizmos.color = _peekGizmoColor;
                
                if (_isLeftEdgeCover)
                {
                    Vector3 leftPeek = GetLeftPeekPosition();
                    Gizmos.DrawWireSphere(leftPeek, 0.2f);
                    Gizmos.DrawLine(coverPos, leftPeek);
                }
                
                if (_isRightEdgeCover)
                {
                    Vector3 rightPeek = GetRightPeekPosition();
                    Gizmos.DrawWireSphere(rightPeek, 0.2f);
                    Gizmos.DrawLine(coverPos, rightPeek);
                }
            }
            
            // Draw occupation status
            if (_isOccupied)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(coverPos + Vector3.up * 0.5f, 0.3f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw more detailed information when selected
            Gizmos.color = Color.white;
            Vector3 coverPos = CoverPosition;
            
            // Draw protection arc
            Gizmos.DrawRay(coverPos, ForwardDirection * 3.0f);
            
            // Draw coverage area
            Vector3 left = coverPos + LeftDirection * (_coverWidth * 0.5f);
            Vector3 right = coverPos + RightDirection * (_coverWidth * 0.5f);
            
            Gizmos.DrawLine(left, right);
            Gizmos.DrawLine(left, left + ForwardDirection * 2.0f);
            Gizmos.DrawLine(right, right + ForwardDirection * 2.0f);
        }
    }

    /// <summary>
    /// Types of cover available
    /// </summary>
    public enum CoverType
    {
        Full,       // Full body protection
        Half,       // Half body protection (crouch needed)
        Low,        // Low cover (prone needed)
        Corner      // Corner cover (side protection)
    }
}