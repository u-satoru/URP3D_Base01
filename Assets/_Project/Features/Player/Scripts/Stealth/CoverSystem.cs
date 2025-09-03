using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Player
{
    public class CoverSystem : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float coverDetectionRange = 2f;
        [SerializeField] private LayerMask coverLayers = -1;
        [SerializeField] private float coverCheckInterval = 0.2f;
        
        [Header("Cover Settings")]
        [SerializeField] private float snapToDistance = 0.3f;
        [SerializeField] private float coverMoveSpeed = 2f;
        [SerializeField] private float peekOffset = 0.5f;
        
        [Header("Components")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private CharacterController characterController;
        
        [Header("Events")]
        [SerializeField] private CoverEnterEvent onCoverEnter;
        [SerializeField] private CoverExitEvent onCoverExit;
        
        [Header("Debug")]
        [SerializeField] private bool isInCover = false;
        [SerializeField] private Transform currentCoverPoint;
        [SerializeField] private Vector3 coverNormal;
        
        private float nextCheckTime;
        private List<CoverPoint> availableCoverPoints = new List<CoverPoint>();
        
        [System.Serializable]
        public class CoverPoint
        {
            public Vector3 position;
            public Vector3 normal;
            public CoverType type;
            public float height;
            public bool canPeekLeft;
            public bool canPeekRight;
            public bool canPeekOver;
        }
        
        public enum CoverType
        {
            Low,
            High,
            Corner
        }
        
        private void Awake()
        {
            if (playerTransform == null)
                playerTransform = transform;
                
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
        }
        
        private void Update()
        {
            if (Time.time >= nextCheckTime)
            {
                DetectNearbyCovers();
                nextCheckTime = Time.time + coverCheckInterval;
            }
            
            if (isInCover)
            {
                UpdateCoverPosition();
            }
        }
        
        private void DetectNearbyCovers()
        {
            availableCoverPoints.Clear();
            
            Collider[] colliders = Physics.OverlapSphere(
                playerTransform.position, coverDetectionRange, coverLayers);
                
            foreach (Collider col in colliders)
            {
                CoverPoint coverPoint = AnalyzeCoverPoint(col);
                if (coverPoint != null)
                {
                    availableCoverPoints.Add(coverPoint);
                }
            }
        }
        
        private CoverPoint AnalyzeCoverPoint(Collider collider)
        {
            Vector3 closestPoint = collider.ClosestPoint(playerTransform.position);
            Vector3 directionToCover = (closestPoint - playerTransform.position).normalized;
            
            RaycastHit hit;
            if (Physics.Raycast(playerTransform.position, directionToCover, 
                out hit, coverDetectionRange, coverLayers))
            {
                if (hit.collider == collider)
                {
                    CoverPoint cover = new CoverPoint
                    {
                        position = hit.point,
                        normal = hit.normal,
                        height = collider.bounds.size.y,
                        type = DetermineCoverType(collider.bounds.size.y)
                    };
                    
                    cover.canPeekLeft = CheckPeekDirection(cover, -playerTransform.right);
                    cover.canPeekRight = CheckPeekDirection(cover, playerTransform.right);
                    cover.canPeekOver = cover.type == CoverType.Low;
                    
                    return cover;
                }
            }
            
            return null;
        }
        
        private CoverType DetermineCoverType(float height)
        {
            if (height < 1.2f)
                return CoverType.Low;
            else if (height < 2f)
                return CoverType.High;
            else
                return CoverType.High;
        }
        
        private bool CheckPeekDirection(CoverPoint cover, Vector3 direction)
        {
            Vector3 peekPosition = cover.position + direction * peekOffset;
            return !Physics.Raycast(peekPosition, -cover.normal, 1f, coverLayers);
        }
        
        public void TryEnterCover()
        {
            if (isInCover || availableCoverPoints.Count == 0) return;
            
            CoverPoint nearestCover = GetNearestCoverPoint();
            if (nearestCover != null)
            {
                EnterCover(nearestCover);
            }
        }
        
        public void ExitCover()
        {
            if (!isInCover) return;
            
            isInCover = false;
            currentCoverPoint = null;
            
            onCoverExit?.Raise();
        }
        
        private void EnterCover(CoverPoint coverPoint)
        {
            isInCover = true;
            coverNormal = coverPoint.normal;
            
            Vector3 coverPosition = coverPoint.position + coverPoint.normal * snapToDistance;
            playerTransform.position = coverPosition;
            
            Quaternion lookRotation = Quaternion.LookRotation(-coverPoint.normal);
            playerTransform.rotation = lookRotation;
            
            onCoverEnter?.Raise();
        }
        
        private CoverPoint GetNearestCoverPoint()
        {
            if (availableCoverPoints.Count == 0) return null;
            
            CoverPoint nearest = null;
            float minDistance = float.MaxValue;
            
            foreach (CoverPoint point in availableCoverPoints)
            {
                float distance = Vector3.Distance(playerTransform.position, point.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = point;
                }
            }
            
            return nearest;
        }
        
        private void UpdateCoverPosition()
        {
            if (currentCoverPoint == null) return;
        }
        
        public void PeekLeft()
        {
            if (!isInCover) return;
        }
        
        public void PeekRight()
        {
            if (!isInCover) return;
        }
        
        public void PeekOver()
        {
            if (!isInCover) return;
        }
        
        public bool IsInCover() => isInCover;
        public List<CoverPoint> GetAvailableCovers() => availableCoverPoints;
        
        private void OnDrawGizmosSelected()
        {
            if (playerTransform == null) return;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, coverDetectionRange);
            
            if (availableCoverPoints != null)
            {
                foreach (CoverPoint point in availableCoverPoints)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(point.position, 0.1f);
                    Gizmos.DrawRay(point.position, point.normal * 0.5f);
                }
            }
            
            if (isInCover && currentCoverPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(currentCoverPoint.position, Vector3.one * 0.2f);
            }
        }
    }
}