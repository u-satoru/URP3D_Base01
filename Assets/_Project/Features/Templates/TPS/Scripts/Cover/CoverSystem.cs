using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.TPS.Player;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.TPS.Cover
{
    /// <summary>
    /// TPS専用カバーシステム中核
    /// カバーポイント自動検出システム、カバー状態管理、ピーキング動作制御、カバー間移動システムを実装
    /// Raycastによる壁・障害物検出、カバーポイント自動生成・管理、プレイヤー・カメラとの統合制御を提供
    /// TPSの戦術的ゲームプレイの中核となる高度なカバー機能を実現
    /// </summary>
    public class CoverSystem : MonoBehaviour
    {
        [TabGroup("Cover System", "Detection")]
        [BoxGroup("Cover System/Detection/Detection Settings")]
        [LabelText("Auto Detection")]
        [SerializeField] private bool autoDetection = true;

        [BoxGroup("Cover System/Detection/Detection Settings")]
        [LabelText("Detection Range")]
        [PropertyRange(1f, 10f)]
        [SerializeField] private float detectionRange = 5f;

        [BoxGroup("Cover System/Detection/Detection Settings")]
        [LabelText("Detection Layer Mask")]
        [SerializeField] private LayerMask detectionLayerMask = 1;

        [BoxGroup("Cover System/Detection/Cover Validation")]
        [LabelText("Min Cover Height")]
        [PropertyRange(1f, 3f)]
        [SerializeField] private float minCoverHeight = 1.5f;

        [BoxGroup("Cover System/Detection/Cover Validation")]
        [LabelText("Min Cover Width")]
        [PropertyRange(0.5f, 2f)]
        [SerializeField] private float minCoverWidth = 1f;

        [BoxGroup("Cover System/Detection/Cover Validation")]
        [LabelText("Cover Thickness Check")]
        [PropertyRange(0.1f, 1f)]
        [SerializeField] private float coverThickness = 0.3f;

        [TabGroup("Cover System", "Positioning")]
        [BoxGroup("Cover System/Positioning/Player Position")]
        [LabelText("Cover Distance")]
        [PropertyRange(0.2f, 1f)]
        [SerializeField] private float coverDistance = 0.5f;

        [BoxGroup("Cover System/Positioning/Player Position")]
        [LabelText("Position Smoothness")]
        [PropertyRange(2f, 20f)]
        [SerializeField] private float positionSmoothness = 10f;

        [BoxGroup("Cover System/Positioning/Peek Settings")]
        [LabelText("Peek Distance")]
        [PropertyRange(0.3f, 1.5f)]
        [SerializeField] private float peekDistance = 0.8f;

        [BoxGroup("Cover System/Positioning/Peek Settings")]
        [LabelText("Peek Speed")]
        [PropertyRange(2f, 15f)]
        [SerializeField] private float peekSpeed = 8f;

        [BoxGroup("Cover System/Positioning/Peek Settings")]
        [LabelText("Peek Smoothness")]
        [PropertyRange(2f, 20f)]
        [SerializeField] private float peekSmoothness = 12f;

        [TabGroup("Cover System", "Movement")]
        [BoxGroup("Cover System/Movement/Cover Movement")]
        [LabelText("Cover Move Speed")]
        [PropertyRange(1f, 8f)]
        [SerializeField] private float coverMoveSpeed = 3f;

        [BoxGroup("Cover System/Movement/Cover Movement")]
        [LabelText("Max Move Distance")]
        [PropertyRange(2f, 10f)]
        [SerializeField] private float maxMoveDistance = 5f;

        [BoxGroup("Cover System/Movement/Transition")]
        [LabelText("Transition Speed")]
        [PropertyRange(2f, 15f)]
        [SerializeField] private float transitionSpeed = 8f;

        [TabGroup("Cover System", "Visual")]
        [BoxGroup("Cover System/Visual/Debug Visualization")]
        [LabelText("Show Cover Points")]
        [SerializeField] private bool showCoverPoints = true;

        [BoxGroup("Cover System/Visual/Debug Visualization")]
        [LabelText("Show Detection Range")]
        [SerializeField] private bool showDetectionRange = true;

        [BoxGroup("Cover System/Visual/Debug Visualization")]
        [LabelText("Cover Point Size")]
        [PropertyRange(0.1f, 0.5f)]
        [SerializeField] private float coverPointSize = 0.2f;

        [TabGroup("Events", "Cover Events")]
        [LabelText("On Cover Point Found")]
        [SerializeField] private GameEvent onCoverPointFound;

        [LabelText("On Player Enter Cover")]
        [SerializeField] private GameEvent onPlayerEnterCover;

        [LabelText("On Player Exit Cover")]
        [SerializeField] private GameEvent onPlayerExitCover;

        [LabelText("On Peek Started")]
        [SerializeField] private GameEvent onPeekStarted;

        [LabelText("On Peek Stopped")]
        [SerializeField] private GameEvent onPeekStopped;

        // Cover point data structure
        [System.Serializable]
        public class CoverPoint
        {
            public Vector3 position;
            public Vector3 normal;
            public float width;
            public float height;
            public CoverType type;
            public bool isOccupied;
            
            public CoverPoint(Vector3 pos, Vector3 norm, float w, float h, CoverType t)
            {
                position = pos;
                normal = norm;
                width = w;
                height = h;
                type = t;
                isOccupied = false;
            }
        }

        public enum CoverType
        {
            Low,      // Crouching cover
            High,     // Standing cover
            Corner    // Corner cover (peeking)
        }

        public enum PeekDirection
        {
            None,
            Left,
            Right
        }

        // Private variables
        private List<CoverPoint> availableCoverPoints = new List<CoverPoint>();
        private CoverPoint currentCoverPoint;
        private TPSPlayerController playerController;
        private bool isInCover = false;
        private bool isPeeking = false;
        private PeekDirection currentPeekDirection = PeekDirection.None;
        
        // Position and movement
        private Vector3 coverPosition;
        private Vector3 peekPosition;
        private Vector3 originalPosition;
        
        // Detection system
        private float lastDetectionTime = 0f;
        private const float DETECTION_INTERVAL = 0.5f; // 0.5 seconds

        [TabGroup("Debug", "Cover State")]
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Is In Cover")]
        private bool debugIsInCover => isInCover;

        [ReadOnly]
        [ShowInInspector]
        [LabelText("Is Peeking")]
        private bool debugIsPeeking => isPeeking;

        [ReadOnly]
        [ShowInInspector]
        [LabelText("Peek Direction")]
        private PeekDirection debugPeekDirection => currentPeekDirection;

        [ReadOnly]
        [ShowInInspector]
        [LabelText("Available Cover Points")]
        private int debugCoverPointsCount => availableCoverPoints.Count;

        [ReadOnly]
        [ShowInInspector]
        [LabelText("Current Cover Type")]
        private CoverType debugCurrentCoverType => currentCoverPoint?.type ?? CoverType.High;

        private void Start()
        {
            playerController = FindFirstObjectByType<TPSPlayerController>();
            if (playerController == null)
            {
                UnityEngine.Debug.LogWarning("[TPS] TPSPlayerController not found for CoverSystem");
            }

            if (autoDetection)
            {
                StartCoroutine(DetectionLoop());
            }
        }

        private void Update()
        {
            if (playerController == null) return;

            HandleCoverInput();
            UpdateCoverState();
            
            if (isInCover)
            {
                HandlePeeking();
                HandleCoverMovement();
            }
        }

        private System.Collections.IEnumerator DetectionLoop()
        {
            while (true)
            {
                if (Time.time - lastDetectionTime >= DETECTION_INTERVAL)
                {
                    DetectCoverPoints();
                    lastDetectionTime = Time.time;
                }
                yield return new WaitForSeconds(DETECTION_INTERVAL);
            }
        }

        private void HandleCoverInput()
        {
            // Cover toggle input (Q key by default)
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (isInCover)
                {
                    ExitCover();
                }
                else
                {
                    TryEnterCover();
                }
            }
        }

        private void UpdateCoverState()
        {
            if (isInCover && currentCoverPoint != null)
            {
                // Update player position to stay in cover
                Vector3 targetPosition = currentCoverPoint.position - currentCoverPoint.normal * coverDistance;
                
                if (!isPeeking)
                {
                    playerController.transform.position = Vector3.Lerp(
                        playerController.transform.position,
                        targetPosition,
                        Time.deltaTime * positionSmoothness
                    );
                }
            }
        }

        private void HandlePeeking()
        {
            if (!isInCover || currentCoverPoint == null) return;

            // Peek input (A and D keys for left/right peeking)
            bool peekLeft = Input.GetKey(KeyCode.A);
            bool peekRight = Input.GetKey(KeyCode.D);
            bool aimInput = Input.GetMouseButton(1);

            PeekDirection newPeekDirection = PeekDirection.None;
            
            if (aimInput && (peekLeft || peekRight))
            {
                newPeekDirection = peekLeft ? PeekDirection.Left : PeekDirection.Right;
            }

            if (newPeekDirection != currentPeekDirection)
            {
                SetPeekDirection(newPeekDirection);
            }

            // Update peek position
            if (isPeeking)
            {
                UpdatePeekPosition();
            }
        }

        private void SetPeekDirection(PeekDirection direction)
        {
            currentPeekDirection = direction;
            
            if (direction != PeekDirection.None)
            {
                if (!isPeeking)
                {
                    StartPeeking();
                }
            }
            else
            {
                if (isPeeking)
                {
                    StopPeeking();
                }
            }
        }

        private void StartPeeking()
        {
            isPeeking = true;
            originalPosition = playerController.transform.position;
            onPeekStarted?.Raise();
            
            UnityEngine.Debug.Log("[TPS] Started peeking " + currentPeekDirection);
        }

        private void StopPeeking()
        {
            isPeeking = false;
            currentPeekDirection = PeekDirection.None;
            onPeekStopped?.Raise();
            
            UnityEngine.Debug.Log("[TPS] Stopped peeking");
        }

        private void UpdatePeekPosition()
        {
            if (currentCoverPoint == null) return;

            Vector3 peekOffset = CalculatePeekOffset();
            Vector3 targetPosition = currentCoverPoint.position - currentCoverPoint.normal * (coverDistance * 0.3f) + peekOffset;
            
            playerController.transform.position = Vector3.Lerp(
                playerController.transform.position,
                targetPosition,
                Time.deltaTime * peekSmoothness
            );
        }

        private Vector3 CalculatePeekOffset()
        {
            Vector3 rightDirection = Vector3.Cross(currentCoverPoint.normal, Vector3.up);
            float peekMultiplier = currentPeekDirection == PeekDirection.Right ? 1f : -1f;
            
            return rightDirection * peekMultiplier * peekDistance;
        }

        private void HandleCoverMovement()
        {
            // Cover movement along the cover line
            float horizontalInput = Input.GetAxis("Horizontal");
            
            if (Mathf.Abs(horizontalInput) > 0.1f && !isPeeking)
            {
                MoveCoverPosition(horizontalInput);
            }
        }

        private void MoveCoverPosition(float direction)
        {
            if (currentCoverPoint == null) return;

            Vector3 rightDirection = Vector3.Cross(currentCoverPoint.normal, Vector3.up);
            Vector3 moveDirection = rightDirection * direction;
            
            // Check if movement is valid (still along the cover)
            Vector3 newPosition = playerController.transform.position + moveDirection * coverMoveSpeed * Time.deltaTime;
            
            if (IsValidCoverPosition(newPosition))
            {
                playerController.transform.position = newPosition;
            }
        }

        private bool IsValidCoverPosition(Vector3 position)
        {
            // Check if the new position still has valid cover
            RaycastHit hit;
            Vector3 rayDirection = currentCoverPoint.normal;
            
            return Physics.Raycast(position + Vector3.up, -rayDirection, out hit, coverDistance + 0.5f, detectionLayerMask);
        }

        private void DetectCoverPoints()
        {
            if (playerController == null) return;

            availableCoverPoints.Clear();
            Vector3 playerPosition = playerController.transform.position;

            // Use sphere cast to detect potential cover around the player
            Collider[] colliders = Physics.OverlapSphere(playerPosition, detectionRange, detectionLayerMask);

            foreach (var collider in colliders)
            {
                AnalyzeCoverCollider(collider, playerPosition);
            }

            // Notify if new cover points found
            if (availableCoverPoints.Count > 0)
            {
                onCoverPointFound?.Raise();
            }
        }

        private void AnalyzeCoverCollider(Collider collider, Vector3 playerPosition)
        {
            // Get the closest point on the collider to the player
            Vector3 closestPoint = collider.ClosestPoint(playerPosition);
            
            // Calculate the normal from the collider surface
            Vector3 directionToPlayer = (playerPosition - closestPoint).normalized;
            
            RaycastHit hit;
            if (Physics.Raycast(closestPoint + directionToPlayer * 0.1f, -directionToPlayer, out hit, 1f, detectionLayerMask))
            {
                Vector3 coverNormal = hit.normal;
                Vector3 coverPosition = hit.point;
                
                // Validate cover dimensions
                if (ValidateCoverPoint(coverPosition, coverNormal))
                {
                    CoverType coverType = DetermineCoverType(coverPosition, coverNormal);
                    float width = CalculateCoverWidth(coverPosition, coverNormal);
                    float height = CalculateCoverHeight(coverPosition, coverNormal);
                    
                    CoverPoint newCoverPoint = new CoverPoint(coverPosition, coverNormal, width, height, coverType);
                    
                    // Check if this cover point already exists
                    if (!CoverPointExists(newCoverPoint))
                    {
                        availableCoverPoints.Add(newCoverPoint);
                    }
                }
            }
        }

        private bool ValidateCoverPoint(Vector3 position, Vector3 normal)
        {
            // Check minimum height requirement
            RaycastHit upHit;
            if (!Physics.Raycast(position, Vector3.up, out upHit, minCoverHeight + 1f))
            {
                return false; // No ceiling found, assume sufficient height
            }
            
            if (Vector3.Distance(position, upHit.point) < minCoverHeight)
            {
                return false; // Too low for cover
            }

            // Check cover thickness
            RaycastHit thicknessHit;
            if (!Physics.Raycast(position + normal * 0.1f, -normal, out thicknessHit, coverThickness + 0.2f))
            {
                return false; // Not thick enough
            }

            return true;
        }

        private CoverType DetermineCoverType(Vector3 position, Vector3 normal)
        {
            // Determine cover type based on height and surroundings
            RaycastHit upHit;
            
            if (Physics.Raycast(position, Vector3.up, out upHit, 2.5f))
            {
                float heightToCeiling = Vector3.Distance(position, upHit.point);
                
                if (heightToCeiling < 1.2f)
                {
                    return CoverType.Low; // Low cover, requires crouching
                }
            }
            
            // Check for corner conditions
            Vector3 rightDirection = Vector3.Cross(normal, Vector3.up);
            RaycastHit leftHit, rightHit;
            
            bool hasLeftWall = Physics.Raycast(position, -rightDirection, out leftHit, 1f);
            bool hasRightWall = Physics.Raycast(position, rightDirection, out rightHit, 1f);
            
            if (!hasLeftWall || !hasRightWall)
            {
                return CoverType.Corner; // Corner cover for peeking
            }
            
            return CoverType.High; // Standard high cover
        }

        private float CalculateCoverWidth(Vector3 position, Vector3 normal)
        {
            Vector3 rightDirection = Vector3.Cross(normal, Vector3.up);
            float totalWidth = 0f;
            
            // Check left and right extent
            for (int i = 1; i <= 5; i++)
            {
                Vector3 testPos = position + rightDirection * (i * 0.2f);
                if (Physics.Raycast(testPos + normal * 0.1f, -normal, coverThickness + 0.2f, detectionLayerMask))
                {
                    totalWidth += 0.2f;
                }
                else break;
            }
            
            for (int i = 1; i <= 5; i++)
            {
                Vector3 testPos = position - rightDirection * (i * 0.2f);
                if (Physics.Raycast(testPos + normal * 0.1f, -normal, coverThickness + 0.2f, detectionLayerMask))
                {
                    totalWidth += 0.2f;
                }
                else break;
            }
            
            return totalWidth;
        }

        private float CalculateCoverHeight(Vector3 position, Vector3 normal)
        {
            RaycastHit upHit;
            if (Physics.Raycast(position, Vector3.up, out upHit, 5f))
            {
                return Vector3.Distance(position, upHit.point);
            }
            
            return 3f; // Default height if no ceiling found
        }

        private bool CoverPointExists(CoverPoint newCoverPoint)
        {
            return availableCoverPoints.Any(cp => Vector3.Distance(cp.position, newCoverPoint.position) < 0.5f);
        }

        private void TryEnterCover()
        {
            CoverPoint nearestCover = GetNearestCoverPoint();
            
            if (nearestCover != null)
            {
                EnterCover(nearestCover);
            }
            else
            {
                UnityEngine.Debug.Log("[TPS] No suitable cover point found");
            }
        }

        private CoverPoint GetNearestCoverPoint()
        {
            if (availableCoverPoints.Count == 0) return null;

            Vector3 playerPosition = playerController.transform.position;
            CoverPoint nearestCover = null;
            float nearestDistance = float.MaxValue;

            foreach (var coverPoint in availableCoverPoints)
            {
                if (coverPoint.isOccupied) continue;

                float distance = Vector3.Distance(playerPosition, coverPoint.position);
                if (distance < nearestDistance && distance <= detectionRange)
                {
                    nearestDistance = distance;
                    nearestCover = coverPoint;
                }
            }

            return nearestCover;
        }

        private void EnterCover(CoverPoint coverPoint)
        {
            currentCoverPoint = coverPoint;
            currentCoverPoint.isOccupied = true;
            isInCover = true;
            
            // Position player at cover point
            Vector3 targetPosition = coverPoint.position - coverPoint.normal * coverDistance;
            playerController.transform.position = targetPosition;
            playerController.transform.rotation = Quaternion.LookRotation(-coverPoint.normal);
            
            // Notify other systems
            onPlayerEnterCover?.Raise();
            
            UnityEngine.Debug.Log("[TPS] Entered cover - Type: " + coverPoint.type);
        }

        private void ExitCover()
        {
            if (currentCoverPoint != null)
            {
                currentCoverPoint.isOccupied = false;
                currentCoverPoint = null;
            }

            isInCover = false;
            
            if (isPeeking)
            {
                StopPeeking();
            }

            // Notify other systems
            onPlayerExitCover?.Raise();
            
            UnityEngine.Debug.Log("[TPS] Exited cover");
        }

        // Public methods for external access
        public bool IsInCover => isInCover;
        public bool IsPeeking => isPeeking;
        public PeekDirection CurrentPeekDirection => currentPeekDirection;
        public CoverPoint CurrentCoverPoint => currentCoverPoint;
        public List<CoverPoint> AvailableCoverPoints => new List<CoverPoint>(availableCoverPoints);

        public void ForceDetection()
        {
            DetectCoverPoints();
        }

        public void SetAutoDetection(bool enabled)
        {
            autoDetection = enabled;
            
            if (enabled && !isActiveAndEnabled)
            {
                StartCoroutine(DetectionLoop());
            }
        }

        // Gizmos for visualization
        private void OnDrawGizmos()
        {
            if (!showCoverPoints && !showDetectionRange) return;

            // Draw detection range
            if (showDetectionRange && playerController != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(playerController.transform.position, detectionRange);
            }

            // Draw cover points
            if (showCoverPoints)
            {
                foreach (var coverPoint in availableCoverPoints)
                {
                    DrawCoverPoint(coverPoint);
                }
            }

            // Draw current cover point
            if (isInCover && currentCoverPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(currentCoverPoint.position, coverPointSize * 1.5f);
                Gizmos.DrawRay(currentCoverPoint.position, currentCoverPoint.normal);
            }
        }

        private void DrawCoverPoint(CoverPoint coverPoint)
        {
            Color coverColor = coverPoint.type switch
            {
                CoverType.Low => Color.blue,
                CoverType.High => Color.red,
                CoverType.Corner => Color.magenta,
                _ => Color.white
            };

            Gizmos.color = coverColor;
            Gizmos.DrawSphere(coverPoint.position, coverPointSize);
            Gizmos.DrawRay(coverPoint.position, coverPoint.normal * 0.5f);
        }
    }
}