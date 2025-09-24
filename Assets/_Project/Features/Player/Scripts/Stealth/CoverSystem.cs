using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Features.Player
{
    /// <summary>
    /// プレイヤーのカバーアクション（物陰に隠れる）を管理するシステムです。
    /// 周囲のカバーポイントを検出し、カバーへの出入りやカバー中の移動を処理します。
    /// </summary>
    public class CoverSystem : MonoBehaviour
    {
        [Header("Detection Settings")]
        [Tooltip("カバーを検出する範囲")]
        [SerializeField] private float coverDetectionRange = 2f;
        [Tooltip("カバーとして認識するオブジェクトのレイヤー")]
        [SerializeField] private LayerMask coverLayers = -1;
        [Tooltip("カバーの再検出を行う間隔（秒）")]
        [SerializeField] private float coverCheckInterval = 0.2f;
        
        [Header("Cover Settings")]
        [Tooltip("カバーポイントに吸着する際の距離")]
        [SerializeField] private float snapToDistance = 0.3f;
        [Tooltip("カバー中の移動速度")]
        [SerializeField] private float coverMoveSpeed = 2f;
        [Tooltip("覗き見（Peek）を行う際のオフセット距離")]
        [SerializeField] private float peekOffset = 0.5f;
        
        [Header("Components")]
        [Tooltip("プレイヤーのTransform")]
        [SerializeField] private Transform playerTransform;
        [Tooltip("プレイヤーのCharacterController")]
        [SerializeField] private CharacterController characterController;
        
        [Header("Events")]
        [Tooltip("カバーに入った時に発行されるイベント")]
        [SerializeField] private CoverEnterEvent onCoverEnter;
        [Tooltip("カバーから出た時に発行されるイベント")]
        [SerializeField] private CoverExitEvent onCoverExit;
        
        [Header("Debug")]
        [Tooltip("現在カバー状態かどうか")]
        [SerializeField] private bool isInCover = false;
        [Tooltip("現在使用しているカバーポイントのTransform")]
        [SerializeField] private Transform currentCoverPoint;
        [Tooltip("現在使用しているカバーの法線ベクトル")]
        [SerializeField] private Vector3 coverNormal;
        
        private float nextCheckTime;
        private List<CoverPoint> availableCoverPoints = new List<CoverPoint>();
        
        /// <summary>
        /// カバーポイントの情報を格納するクラスです。
        /// </summary>
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
        
        /// <summary>
        /// カバーの種類を定義します。
        /// </summary>
        public enum CoverType
        {
            Low,    // 低い遮蔽物（乗り越え可能）
            High,   // 高い遮蔽物
            Corner  // 角
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
        
        /// <summary>
        /// プレイヤーの周囲にあるカバーポイントを検出します。
        /// </summary>
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
        
        /// <summary>
        /// 検出したコライダーを分析し、カバーポイント情報を生成します。
        /// </summary>
        /// <param name="collider">分析対象のコライダー。</param>
        /// <returns>生成されたカバーポイント情報。不適切な場合はnull。</returns>
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
        
        /// <summary>
        /// 遮蔽物の高さに基づいてカバーの種類を決定します。
        /// </summary>
        /// <param name="height">遮蔽物の高さ。</param>
        /// <returns>決定されたカバーの種類。</returns>
        private CoverType DetermineCoverType(float height)
        {
            if (height < 1.2f)
                return CoverType.Low;
            else if (height < 2f)
                return CoverType.High;
            else
                return CoverType.High;
        }
        
        /// <summary>
        /// 指定された方向への覗き見が可能かどうかをチェックします。
        /// </summary>
        /// <param name="cover">チェック対象のカバーポイント。</param>
        /// <param name="direction">チェックする方向。</param>
        /// <returns>覗き見可能であればtrue。</returns>
        private bool CheckPeekDirection(CoverPoint cover, Vector3 direction)
        {
            Vector3 peekPosition = cover.position + direction * peekOffset;
            return !Physics.Raycast(peekPosition, -cover.normal, 1f, coverLayers);
        }
        
        /// <summary>
        /// 利用可能な最も近いカバーポイントに入ります。
        /// </summary>
        public void TryEnterCover()
        {
            if (isInCover || availableCoverPoints.Count == 0) return;
            
            CoverPoint nearestCover = GetNearestCoverPoint();
            if (nearestCover != null)
            {
                EnterCover(nearestCover);
            }
        }
        
        /// <summary>
        /// 現在のカバー状態から離脱します。
        /// </summary>
        public void ExitCover()
        {
            if (!isInCover) return;
            
            isInCover = false;
            currentCoverPoint = null;
            
            onCoverExit?.Raise();
        }
        
        /// <summary>
        /// 指定されたカバーポイントに入り、プレイヤーの位置と向きを調整します。
        /// </summary>
        /// <param name="coverPoint">入るカバーポイント。</param>
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
        
        /// <summary>
        /// 利用可能なカバーポイントの中から最も近いものを取得します。
        /// </summary>
        /// <returns>最も近いカバーポイント。なければnull。</returns>
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
        
        /// <summary>
        /// カバー状態でのプレイヤーの位置を更新します。
        /// </summary>
        private void UpdateCoverPosition()
        {
            if (currentCoverPoint == null) return;
            
            // カバー状態での移動処理（カバーに沿った移動）
            Vector3 inputDirection = GetCoverMovementInput();
            if (inputDirection.magnitude > 0.1f)
            {
                MovAlongCover(inputDirection);
            }
        }
        
        /// <summary>
        /// 入力に基づいてカバーに沿った移動を実行します。
        /// </summary>
        /// <param name="inputDirection">移動入力方向。</param>
        private void MovAlongCover(Vector3 inputDirection)
        {
            if (characterController == null) return;
            
            // カバーの法線に垂直な方向への移動のみ許可
            Vector3 rightDirection = Vector3.Cross(coverNormal, Vector3.up).normalized;
            float horizontalInput = Vector3.Dot(inputDirection, rightDirection);
            
            Vector3 moveDirection = rightDirection * horizontalInput;
            Vector3 movement = moveDirection * coverMoveSpeed * Time.deltaTime;
            
            // カバーから離れないように位置を調整
            characterController.Move(movement);
            
            // カバーとの距離を維持
            MaintainCoverDistance();
        }
        
        /// <summary>
        /// プレイヤーがカバーから離れすぎないように、適切な距離を維持します。
        /// </summary>
        private void MaintainCoverDistance()
        {
            RaycastHit hit;
            if (Physics.Raycast(playerTransform.position, -coverNormal, out hit, snapToDistance + 0.5f, coverLayers))
            {
                Vector3 targetPosition = hit.point + hit.normal * snapToDistance;
                Vector3 adjustment = targetPosition - playerTransform.position;
                
                // Y軸方向の調整は除外（地面との接触を維持）
                adjustment.y = 0;
                
                characterController.Move(adjustment);
            }
        }
        
        /// <summary>
        /// カバー中の移動入力を取得します（この実装は仮です）。
        /// </summary>
        /// <returns>水平方向の移動入力。</returns>
        private Vector3 GetCoverMovementInput()
        {
            // 実際のプロジェクトでは Input System や PlayerController から取得
            float horizontal = Input.GetAxis("Horizontal");
            return new Vector3(horizontal, 0, 0);
        }
        
        /// <summary>
        /// 左への覗き見を実行します。
        /// </summary>
        public void PeekLeft()
        {
            if (!isInCover) return;
        }
        
        /// <summary>
        /// 右への覗き見を実行します。
        /// </summary>
        public void PeekRight()
        {
            if (!isInCover) return;
        }
        
        /// <summary>
        /// 上への覗き見を実行します。
        /// </summary>
        public void PeekOver()
        {
            if (!isInCover) return;
        }
        
        /// <summary>
        /// 現在カバー状態かどうかを返します。
        /// </summary>
        public bool IsInCover() => isInCover;

        /// <summary>
        /// 現在利用可能なカバーポイントのリストを返します。
        /// </summary>
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
