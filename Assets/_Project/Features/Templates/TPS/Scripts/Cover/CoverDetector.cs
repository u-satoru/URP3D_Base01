using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using asterivo.Unity60.Features.Templates.TPS.Data;
using asterivo.Unity60.Features.Templates.TPS.Player;

namespace asterivo.Unity60.Features.Templates.TPS.Cover
{
    /// <summary>
    /// Cover Detector - Detects and manages cover points for TPS cover system
    /// Integrates with TPSPlayerData for configuration and ServiceLocator architecture
    /// Follows DESIGN.md Feature layer principles
    /// </summary>
    [System.Serializable]
    public class CoverDetector : MonoBehaviour
    {
        [Header("Detection Configuration")]
        [SerializeField] private float _detectionRadius = 5.0f;
        [SerializeField] private LayerMask _coverLayer = -1;
        [SerializeField] private float _updateInterval = 0.2f; // Update every 0.2 seconds for performance
        [SerializeField] private int _maxDetectedCovers = 10;
        
        [Header("Cover Evaluation")]
        [SerializeField] private float _threatAwarenessRadius = 15.0f;
        [SerializeField] private LayerMask _threatLayer = -1;
        [SerializeField] private bool _evaluateThreatProtection = true;
        [SerializeField] private bool _preferClosestCover = true;
        
        [Header("Debug Settings")]
        [SerializeField] private bool _showDebugGizmos = true;
        [SerializeField] private bool _logDetectionResults = false;
        [SerializeField] private Color _detectionGizmoColor = Color.cyan;
        [SerializeField] private Color _bestCoverGizmoColor = Color.green;

        // Detection state
        private List<CoverPoint> _detectedCoverPoints = new List<CoverPoint>();
        private CoverPoint _bestCoverPoint = null;
        private float _lastUpdateTime = 0f;
        
        // Configuration reference (injected from PlayerController)
        private TPSPlayerData _playerData;
        
        // Cached components
        private Transform _transform;

        // Properties for external access
        public List<CoverPoint> DetectedCoverPoints => _detectedCoverPoints;
        public CoverPoint BestCoverPoint => _bestCoverPoint;
        public float DetectionRadius => _detectionRadius;
        public bool HasAvailableCover => _bestCoverPoint != null && _bestCoverPoint.CanBeOccupied();

        private void Awake()
        {
            _transform = transform;
            
            // Try to get player data from parent PlayerController
            var playerController = GetComponent<TPSPlayerController>();
            if (playerController != null)
            {
                _playerData = playerController.PlayerData;
                
                // Use configuration from PlayerData if available
                if (_playerData != null)
                {
                    _detectionRadius = _playerData.CoverDetectionRange;
                    _coverLayer = _playerData.CoverLayerMask;
                }
            }
        }

        private void Start()
        {
            // Perform initial detection
            UpdateCoverDetection();
        }

        private void Update()
        {
            // Update detection at intervals for performance
            if (Time.time - _lastUpdateTime >= _updateInterval)
            {
                UpdateCoverDetection();
                _lastUpdateTime = Time.time;
            }
        }

        /// <summary>
        /// Update cover point detection and evaluation
        /// </summary>
        public void UpdateCoverDetection()
        {
            _detectedCoverPoints.Clear();
            
            // Find all colliders within detection radius on cover layer
            Collider[] coverColliders = Physics.OverlapSphere(_transform.position, _detectionRadius, _coverLayer);
            
            // Extract cover points from colliders
            foreach (var collider in coverColliders)
            {
                var coverPoint = collider.GetComponent<CoverPoint>();
                if (coverPoint != null && coverPoint.CanBeOccupied())
                {
                    _detectedCoverPoints.Add(coverPoint);
                }
            }
            
            // Limit to max detected covers for performance
            if (_detectedCoverPoints.Count > _maxDetectedCovers)
            {
                _detectedCoverPoints = _detectedCoverPoints.Take(_maxDetectedCovers).ToList();
            }
            
            // Evaluate and select best cover point
            EvaluateBestCover();
            
            if (_logDetectionResults)
            {
                Debug.Log($"[CoverDetector] Detected {_detectedCoverPoints.Count} cover points. Best: {(_bestCoverPoint != null ? _bestCoverPoint.name : "None")}");
            }
        }

        /// <summary>
        /// Evaluate and select the best cover point based on various criteria
        /// </summary>
        private void EvaluateBestCover()
        {
            if (_detectedCoverPoints.Count == 0)
            {
                _bestCoverPoint = null;
                return;
            }

            // Score each cover point
            var scoredCovers = new List<(CoverPoint cover, float score)>();
            
            foreach (var cover in _detectedCoverPoints)
            {
                float score = EvaluateCoverPoint(cover);
                scoredCovers.Add((cover, score));
            }
            
            // Select the highest scoring cover
            var bestScored = scoredCovers.OrderByDescending(x => x.score).FirstOrDefault();
            _bestCoverPoint = bestScored.cover;
        }

        /// <summary>
        /// Evaluate a cover point and return a score (higher is better)
        /// </summary>
        private float EvaluateCoverPoint(CoverPoint cover)
        {
            float score = 0f;
            Vector3 playerPosition = _transform.position;
            Vector3 coverPosition = cover.CoverPosition;
            
            // Distance score (closer is better if preferClosestCover is true)
            float distance = Vector3.Distance(playerPosition, coverPosition);
            float distanceScore = _preferClosestCover ? 
                (1f - Mathf.Clamp01(distance / _detectionRadius)) * 10f :
                Mathf.Clamp01(distance / _detectionRadius) * 10f; // Further is better
            
            score += distanceScore;
            
            // Accessibility score (can we reach it easily?)
            float accessibilityScore = EvaluateAccessibility(coverPosition);
            score += accessibilityScore * 5f;
            
            // Threat protection score
            if (_evaluateThreatProtection)
            {
                float threatProtectionScore = EvaluateThreatProtection(cover);
                score += threatProtectionScore * 15f; // High weight for threat protection
            }
            
            // Cover type bonus
            float coverTypeScore = GetCoverTypeScore(cover.Type);
            score += coverTypeScore;
            
            // Peek capability bonus
            if (cover.AllowPeeking)
            {
                score += 2f;
            }
            
            return score;
        }

        /// <summary>
        /// Evaluate if the cover position is accessible (no obstacles blocking path)
        /// </summary>
        private float EvaluateAccessibility(Vector3 coverPosition)
        {
            Vector3 playerPosition = _transform.position;
            Vector3 direction = (coverPosition - playerPosition).normalized;
            float distance = Vector3.Distance(playerPosition, coverPosition);
            
            // Raycast to check for obstacles
            if (Physics.Raycast(playerPosition + Vector3.up * 0.5f, direction, distance - 0.5f, ~_coverLayer))
            {
                return 0f; // Blocked path
            }
            
            return 1f; // Clear path
        }

        /// <summary>
        /// Evaluate how well this cover protects against current threats
        /// </summary>
        private float EvaluateThreatProtection(CoverPoint cover)
        {
            // Find threats within awareness radius
            Collider[] threats = Physics.OverlapSphere(_transform.position, _threatAwarenessRadius, _threatLayer);
            
            if (threats.Length == 0)
            {
                return 0.5f; // Neutral score if no threats detected
            }
            
            float totalProtection = 0f;
            int threatCount = 0;
            
            foreach (var threat in threats)
            {
                if (threat.transform == _transform) continue; // Skip self
                
                Vector3 threatDirection = (threat.transform.position - cover.CoverPosition).normalized;
                float protection = cover.GetCoverEffectiveness(threatDirection);
                totalProtection += protection;
                threatCount++;
            }
            
            return threatCount > 0 ? totalProtection / threatCount : 0.5f;
        }

        /// <summary>
        /// Get score based on cover type
        /// </summary>
        private float GetCoverTypeScore(CoverType coverType)
        {
            return coverType switch
            {
                CoverType.Full => 3f,
                CoverType.Half => 2f,
                CoverType.Corner => 2.5f,
                CoverType.Low => 1f,
                _ => 1f
            };
        }

        /// <summary>
        /// Find the best cover point within detection range
        /// </summary>
        public CoverPoint FindBestCoverPoint()
        {
            UpdateCoverDetection();
            return _bestCoverPoint;
        }

        /// <summary>
        /// Find the best cover point for protection against a specific threat
        /// </summary>
        public CoverPoint FindBestCoverAgainstThreat(Vector3 threatPosition)
        {
            UpdateCoverDetection();
            
            if (_detectedCoverPoints.Count == 0) return null;
            
            CoverPoint bestCover = null;
            float bestScore = float.MinValue;
            
            foreach (var cover in _detectedCoverPoints)
            {
                if (!cover.CanBeOccupied()) continue;
                
                Vector3 threatDirection = (threatPosition - cover.CoverPosition).normalized;
                float protection = cover.GetCoverEffectiveness(threatDirection);
                float distance = Vector3.Distance(_transform.position, cover.CoverPosition);
                
                // Score: protection effectiveness - distance penalty
                float score = protection * 10f - (distance / _detectionRadius) * 3f;
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestCover = cover;
                }
            }
            
            return bestCover;
        }

        /// <summary>
        /// Check if a specific position would be considered good cover
        /// </summary>
        public bool IsGoodCoverPosition(Vector3 position, Vector3 threatDirection)
        {
            // Find cover point at or near the position
            var nearbyCovers = _detectedCoverPoints.Where(c => 
                Vector3.Distance(c.CoverPosition, position) <= 1.0f).ToList();
            
            foreach (var cover in nearbyCovers)
            {
                if (cover.GetCoverEffectiveness(threatDirection) > 0.7f)
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Force immediate cover detection update
        /// </summary>
        public void ForceUpdate()
        {
            UpdateCoverDetection();
        }

        /// <summary>
        /// Set configuration from player data
        /// </summary>
        public void SetPlayerData(TPSPlayerData playerData)
        {
            _playerData = playerData;
            
            if (_playerData != null)
            {
                _detectionRadius = _playerData.CoverDetectionRange;
                _coverLayer = _playerData.CoverLayerMask;
            }
        }

        private void OnDrawGizmos()
        {
            if (!_showDebugGizmos) return;
            
            // Draw detection radius
            Gizmos.color = _detectionGizmoColor;
            Gizmos.DrawWireSphere(transform.position, _detectionRadius);
            
            // Draw detected cover points
            Gizmos.color = Color.white;
            foreach (var cover in _detectedCoverPoints)
            {
                if (cover != null)
                {
                    Gizmos.DrawLine(transform.position, cover.CoverPosition);
                    Gizmos.DrawWireCube(cover.CoverPosition, Vector3.one * 0.3f);
                }
            }
            
            // Highlight best cover point
            if (_bestCoverPoint != null)
            {
                Gizmos.color = _bestCoverGizmoColor;
                Gizmos.DrawWireSphere(_bestCoverPoint.CoverPosition, 0.5f);
                Gizmos.DrawLine(transform.position, _bestCoverPoint.CoverPosition);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw threat awareness radius
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _threatAwarenessRadius);
            
            // Draw cover evaluation details
            if (_bestCoverPoint != null)
            {
                Gizmos.color = Color.yellow;
                Vector3 approachPos = _bestCoverPoint.GetApproachPosition(transform.position);
                Gizmos.DrawWireSphere(approachPos, 0.3f);
                Gizmos.DrawLine(_bestCoverPoint.CoverPosition, approachPos);
            }
        }
    }
}