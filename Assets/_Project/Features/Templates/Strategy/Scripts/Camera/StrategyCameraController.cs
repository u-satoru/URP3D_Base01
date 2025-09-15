using UnityEngine;
using UnityEngine.InputSystem;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Templates.Strategy.Camera
{
    /// <summary>
    /// ストラテジーゲーム用RTSカメラコントローラー
    /// 俯瞰視点でのカメラ移動、ズーム、回転を管理
    /// </summary>
    public class StrategyCameraController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float fastMoveSpeed = 20f;
        [SerializeField] private float edgeScrollSpeed = 8f;
        [SerializeField] private bool enableEdgeScrolling = true;
        [SerializeField] private float edgeScrollBorderSize = 10f;
        
        [Header("Zoom Settings")]
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 30f;
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float currentZoom = 15f;
        
        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 50f;
        [SerializeField] private bool enableRotation = true;
        
        [Header("Bounds")]
        [SerializeField] private bool useBounds = true;
        [SerializeField] private Vector3 boundsMin = new Vector3(-50, 0, -50);
        [SerializeField] private Vector3 boundsMax = new Vector3(50, 0, 50);
        
        [Header("Input")]
        [SerializeField] private KeyCode fastMoveKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode rotateKey = KeyCode.Q;
        [SerializeField] private KeyCode counterRotateKey = KeyCode.E;
        
        [Header("Events")]
        [SerializeField] private GameEvent onCameraMoved;
        [SerializeField] private GameEvent onCameraZoomed;
        
        private UnityEngine.Camera cam;
        private PlayerInput playerInput;
        private Vector2 moveInput;
        private float zoomInput;
        private bool isRotating;
        private Vector3 lastMousePosition;
        
        // Input Actions
        private InputAction moveAction;
        private InputAction zoomAction;
        private InputAction rotateAction;
        
        // Properties
        public float CurrentZoom => currentZoom;
        public Vector3 CameraPosition => transform.position;
        public bool EnableEdgeScrolling
        {
            get => enableEdgeScrolling;
            set => enableEdgeScrolling = value;
        }
        
        private void Awake()
        {
            cam = GetComponent<UnityEngine.Camera>();
            playerInput = GetComponent<PlayerInput>();
            
            if (cam == null)
                cam = UnityEngine.Camera.main;
        }
        
        private void Start()
        {
            SetupInputActions();
            SetInitialZoom();
        }
        
        private void Update()
        {
            HandleMovement();
            HandleZoom();
            HandleRotation();
            HandleEdgeScrolling();
        }
        
        /// <summary>
        /// 入力アクション設定
        /// </summary>
        private void SetupInputActions()
        {
            if (playerInput != null && playerInput.actions != null)
            {
                moveAction = playerInput.actions["Move"];
                zoomAction = playerInput.actions["Zoom"];
                rotateAction = playerInput.actions["Rotate"];
                
                if (moveAction != null)
                {
                    moveAction.performed += OnMovePerformed;
                    moveAction.canceled += OnMoveCanceled;
                }
                
                if (zoomAction != null)
                    zoomAction.performed += OnZoomPerformed;
                    
                if (rotateAction != null)
                {
                    rotateAction.performed += OnRotatePerformed;
                    rotateAction.canceled += OnRotateCanceled;
                }
            }
        }
        
        /// <summary>
        /// 初期ズーム設定
        /// </summary>
        private void SetInitialZoom()
        {
            if (cam != null)
            {
                if (cam.orthographic)
                    cam.orthographicSize = currentZoom;
                else
                    cam.fieldOfView = currentZoom;
            }
        }
        
        /// <summary>
        /// カメラ移動処理
        /// </summary>
        private void HandleMovement()
        {
            Vector3 movement = Vector3.zero;
            
            // キーボード入力
            movement.x = moveInput.x;
            movement.z = moveInput.y;
            
            // 移動速度調整
            float speed = Input.GetKey(fastMoveKey) ? fastMoveSpeed : moveSpeed;
            movement *= speed * Time.deltaTime;
            
            // ワールド座標での移動に変換
            movement = transform.TransformDirection(movement);
            movement.y = 0; // Y軸は固定
            
            // 境界チェック
            Vector3 newPosition = transform.position + movement;
            if (useBounds)
            {
                newPosition.x = Mathf.Clamp(newPosition.x, boundsMin.x, boundsMax.x);
                newPosition.z = Mathf.Clamp(newPosition.z, boundsMin.z, boundsMax.z);
            }
            
            transform.position = newPosition;
            
            if (movement.magnitude > 0.01f)
            {
                onCameraMoved?.Raise();
            }
        }
        
        /// <summary>
        /// ズーム処理
        /// </summary>
        private void HandleZoom()
        {
            if (Mathf.Abs(zoomInput) > 0.01f)
            {
                currentZoom = Mathf.Clamp(currentZoom - zoomInput * zoomSpeed * Time.deltaTime, minZoom, maxZoom);
                
                if (cam != null)
                {
                    if (cam.orthographic)
                        cam.orthographicSize = currentZoom;
                    else
                        cam.fieldOfView = currentZoom;
                }
                
                onCameraZoomed?.Raise();
            }
        }
        
        /// <summary>
        /// 回転処理
        /// </summary>
        private void HandleRotation()
        {
            if (!enableRotation) return;
            
            float rotation = 0f;
            
            if (Input.GetKey(rotateKey))
                rotation = -rotationSpeed * Time.deltaTime;
            else if (Input.GetKey(counterRotateKey))
                rotation = rotationSpeed * Time.deltaTime;
            
            if (Mathf.Abs(rotation) > 0.01f)
            {
                transform.Rotate(0, rotation, 0);
            }
        }
        
        /// <summary>
        /// 画面端スクロール処理
        /// </summary>
        private void HandleEdgeScrolling()
        {
            if (!enableEdgeScrolling) return;
            
            Vector3 mousePosition = Input.mousePosition;
            Vector3 movement = Vector3.zero;
            
            // 画面端での移動
            if (mousePosition.x <= edgeScrollBorderSize)
                movement.x = -1;
            else if (mousePosition.x >= Screen.width - edgeScrollBorderSize)
                movement.x = 1;
                
            if (mousePosition.y <= edgeScrollBorderSize)
                movement.z = -1;
            else if (mousePosition.y >= Screen.height - edgeScrollBorderSize)
                movement.z = 1;
            
            movement = movement.normalized * edgeScrollSpeed * Time.deltaTime;
            movement = transform.TransformDirection(movement);
            movement.y = 0;
            
            Vector3 newPosition = transform.position + movement;
            if (useBounds)
            {
                newPosition.x = Mathf.Clamp(newPosition.x, boundsMin.x, boundsMax.x);
                newPosition.z = Mathf.Clamp(newPosition.z, boundsMin.z, boundsMax.z);
            }
            
            transform.position = newPosition;
        }
        
        /// <summary>
        /// カメラを指定位置に移動
        /// </summary>
        public void MoveToPosition(Vector3 position, bool instant = false)
        {
            if (useBounds)
            {
                position.x = Mathf.Clamp(position.x, boundsMin.x, boundsMax.x);
                position.z = Mathf.Clamp(position.z, boundsMin.z, boundsMax.z);
            }
            
            if (instant)
            {
                transform.position = position;
            }
            else
            {
                StartCoroutine(SmoothMoveToPosition(position));
            }
        }
        
        /// <summary>
        /// スムーズな位置移動
        /// </summary>
        private System.Collections.IEnumerator SmoothMoveToPosition(Vector3 targetPosition)
        {
            Vector3 startPosition = transform.position;
            float journey = 0f;
            float journeyTime = 1f;
            
            while (journey < journeyTime)
            {
                journey += Time.deltaTime;
                float fraction = journey / journeyTime;
                transform.position = Vector3.Lerp(startPosition, targetPosition, fraction);
                yield return null;
            }
            
            transform.position = targetPosition;
        }
        
        /// <summary>
        /// ズームレベル設定
        /// </summary>
        public void SetZoom(float zoom)
        {
            currentZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
            
            if (cam != null)
            {
                if (cam.orthographic)
                    cam.orthographicSize = currentZoom;
                else
                    cam.fieldOfView = currentZoom;
            }
        }
        
        /// <summary>
        /// 境界設定
        /// </summary>
        public void SetBounds(Vector3 min, Vector3 max)
        {
            boundsMin = min;
            boundsMax = max;
            useBounds = true;
        }
        
        // Input Callbacks
        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }
        
        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            moveInput = Vector2.zero;
        }
        
        private void OnZoomPerformed(InputAction.CallbackContext context)
        {
            zoomInput = context.ReadValue<float>();
        }
        
        private void OnRotatePerformed(InputAction.CallbackContext context)
        {
            isRotating = true;
        }
        
        private void OnRotateCanceled(InputAction.CallbackContext context)
        {
            isRotating = false;
        }
        
        private void OnDestroy()
        {
            // 入力アクションコールバックの解除
            if (moveAction != null)
            {
                moveAction.performed -= OnMovePerformed;
                moveAction.canceled -= OnMoveCanceled;
            }
            
            if (zoomAction != null)
                zoomAction.performed -= OnZoomPerformed;
                
            if (rotateAction != null)
            {
                rotateAction.performed -= OnRotatePerformed;
                rotateAction.canceled -= OnRotateCanceled;
            }
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (useBounds)
            {
                Gizmos.color = Color.yellow;
                Vector3 center = (boundsMin + boundsMax) / 2;
                Vector3 size = boundsMax - boundsMin;
                Gizmos.DrawWireCube(center, size);
            }
        }
        #endif
    }
}