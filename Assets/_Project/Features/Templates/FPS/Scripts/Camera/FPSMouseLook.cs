using UnityEngine;
using UnityEngine.InputSystem;

namespace asterivo.Unity60.Features.Templates.FPS.Camera
{
    /// <summary>
    /// FPS専用マウスルック制御
    /// 一人称視点でのマウスによる視点移動を管理
    /// </summary>
    public class FPSMouseLook : MonoBehaviour
    {
        [Header("Look Settings")]
        [SerializeField] private float mouseSensitivity = 2.0f;
        [SerializeField] private float smoothTime = 0.03f;
        [SerializeField] private bool invertY = false;
        
        [Header("Constraints")]
        [SerializeField] private float minYRotation = -90f;
        [SerializeField] private float maxYRotation = 90f;
        [SerializeField] private bool enableMouseLook = true;
        
        [Header("Target")]
        [SerializeField] private Transform playerBody;
        [SerializeField] private Transform cameraTransform;
        
        private Vector2 currentMouseDelta;
        private Vector2 currentMouseVelocity;
        private float yRotation = 0f;
        private PlayerInput playerInput;
        private InputAction lookAction;
        
        // Properties
        public float MouseSensitivity
        {
            get => mouseSensitivity;
            set => mouseSensitivity = Mathf.Max(0.1f, value);
        }
        
        public bool InvertY
        {
            get => invertY;
            set => invertY = value;
        }
        
        public bool EnableMouseLook
        {
            get => enableMouseLook;
            set => enableMouseLook = value;
        }
        
        private void Awake()
        {
            // カメラTransformが設定されていない場合は自身のTransformを使用
            if (cameraTransform == null)
                cameraTransform = transform;
                
            // プレイヤーボディが設定されていない場合は親オブジェクトを検索
            if (playerBody == null && transform.parent != null)
                playerBody = transform.parent;
                
            playerInput = GetComponentInParent<PlayerInput>();
            
            // カーソルをロック
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        private void Start()
        {
            SetupInputActions();
        }
        
        private void Update()
        {
            if (!enableMouseLook) return;
            
            HandleMouseLook();
        }
        
        /// <summary>
        /// 入力アクション設定
        /// </summary>
        private void SetupInputActions()
        {
            if (playerInput != null && playerInput.actions != null)
            {
                lookAction = playerInput.actions["Look"];
                if (lookAction != null)
                {
                    lookAction.performed += OnLookPerformed;
                    lookAction.canceled += OnLookCanceled;
                }
            }
        }
        
        /// <summary>
        /// マウスルック処理
        /// </summary>
        private void HandleMouseLook()
        {
            // スムーズなマウス移動を計算
            Vector2 targetMouseDelta = currentMouseDelta * mouseSensitivity;
            currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseVelocity, smoothTime);
            
            // Y軸（上下）回転の計算
            float mouseY = invertY ? currentMouseDelta.y : -currentMouseDelta.y;
            yRotation += mouseY;
            yRotation = Mathf.Clamp(yRotation, minYRotation, maxYRotation);
            
            // X軸（左右）回転の計算
            float mouseX = currentMouseDelta.x;
            
            // カメラの上下回転を適用
            if (cameraTransform != null)
            {
                cameraTransform.localRotation = Quaternion.Euler(yRotation, 0f, 0f);
            }
            
            // プレイヤーボディの左右回転を適用
            if (playerBody != null)
            {
                playerBody.Rotate(Vector3.up * mouseX);
            }
        }
        
        /// <summary>
        /// マウス感度を設定
        /// </summary>
        public void SetMouseSensitivity(float sensitivity)
        {
            MouseSensitivity = sensitivity;
        }
        
        /// <summary>
        /// Y軸反転を設定
        /// </summary>
        public void SetInvertY(bool invert)
        {
            InvertY = invert;
        }
        
        /// <summary>
        /// マウスルックの有効/無効を切り替え
        /// </summary>
        public void SetMouseLookEnabled(bool enabled)
        {
            EnableMouseLook = enabled;
            
            if (enabled)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        
        /// <summary>
        /// 回転をリセット
        /// </summary>
        public void ResetRotation()
        {
            yRotation = 0f;
            currentMouseDelta = Vector2.zero;
            
            if (cameraTransform != null)
                cameraTransform.localRotation = Quaternion.identity;
        }
        
        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize(Transform playerBodyTransform, Transform cameraTransform, float sensitivity)
        {
            playerBody = playerBodyTransform;
            this.cameraTransform = cameraTransform;
            mouseSensitivity = sensitivity;
            
            ResetRotation();
        }
        
        // Input Callbacks
        private void OnLookPerformed(InputAction.CallbackContext context)
        {
            if (!enableMouseLook) return;
            
            Vector2 lookInput = context.ReadValue<Vector2>();
            currentMouseDelta = lookInput;
        }
        
        private void OnLookCanceled(InputAction.CallbackContext context)
        {
            currentMouseDelta = Vector2.zero;
        }
        
        private void OnDestroy()
        {
            // 入力アクションのコールバック解除
            if (lookAction != null)
            {
                lookAction.performed -= OnLookPerformed;
                lookAction.canceled -= OnLookCanceled;
            }
            
            // カーソルの状態を復元
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && enableMouseLook)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            // エディタでの値変更を反映
            mouseSensitivity = Mathf.Max(0.1f, mouseSensitivity);
            minYRotation = Mathf.Clamp(minYRotation, -90f, 0f);
            maxYRotation = Mathf.Clamp(maxYRotation, 0f, 90f);
        }
        #endif
    }
}