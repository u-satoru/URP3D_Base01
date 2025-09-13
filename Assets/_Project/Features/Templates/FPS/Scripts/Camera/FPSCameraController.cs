using UnityEngine;
using asterivo.Unity60.Camera.States;
using asterivo.Unity60.Features.Templates.FPS.Weapons;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.FPS.Camera
{
    /// <summary>
    /// FPS専用カメラコントローラー
    /// 武器の反動、武器スウェイ、視点移動の滑らかさなど
    /// FPSゲーム特有のカメラ動作を実装
    /// </summary>
    public class FPSCameraController : MonoBehaviour
    {
        [TabGroup("FPS Camera", "Base Components")]
        [LabelText("Camera State Machine")]
        [SerializeField] private CameraStateMachine cameraStateMachine;
        
        [LabelText("Main Camera")]
        [SerializeField] private UnityEngine.Camera mainCamera;
        
        [LabelText("Weapon Holder")]
        [SerializeField] private Transform weaponHolder;
        
        [TabGroup("FPS Camera", "Mouse Look")]
        [BoxGroup("FPS Camera/Mouse Look/Sensitivity")]
        [LabelText("Mouse Sensitivity X")]
        [PropertyRange(0.5f, 5f)]
        [SerializeField] private float mouseSensitivityX = 2f;
        
        [BoxGroup("FPS Camera/Mouse Look/Sensitivity")]
        [LabelText("Mouse Sensitivity Y")]
        [PropertyRange(0.5f, 5f)]
        [SerializeField] private float mouseSensitivityY = 2f;
        
        [BoxGroup("FPS Camera/Mouse Look/Limits")]
        [LabelText("Max Look Angle")]
        [PropertyRange(60f, 90f)]
        [SerializeField] private float maxLookAngle = 80f;
        
        [BoxGroup("FPS Camera/Mouse Look/Limits")]
        [LabelText("Min Look Angle")]
        [PropertyRange(-90f, -60f)]
        [SerializeField] private float minLookAngle = -80f;
        
        [TabGroup("FPS Camera", "Weapon Sway")]
        [BoxGroup("FPS Camera/Weapon Sway/Movement")]
        [LabelText("Sway Amount")]
        [PropertyRange(0.01f, 0.1f)]
        [SerializeField] private float swayAmount = 0.05f;
        
        [BoxGroup("FPS Camera/Weapon Sway/Movement")]
        [LabelText("Max Sway Amount")]
        [PropertyRange(0.05f, 0.2f)]
        [SerializeField] private float maxSwayAmount = 0.1f;
        
        [BoxGroup("FPS Camera/Weapon Sway/Smoothing")]
        [LabelText("Sway Smooth")]
        [PropertyRange(1f, 20f)]
        [SerializeField] private float swaySmooth = 10f;
        
        [BoxGroup("FPS Camera/Weapon Sway/Reset")]
        [LabelText("Sway Reset Smooth")]
        [PropertyRange(1f, 10f)]
        [SerializeField] private float swayResetSmooth = 5f;
        
        [TabGroup("FPS Camera", "Recoil")]
        [BoxGroup("FPS Camera/Recoil/Strength")]
        [LabelText("Recoil Strength")]
        [PropertyRange(0.1f, 2f)]
        [SerializeField] private float recoilStrength = 0.5f;
        
        [BoxGroup("FPS Camera/Recoil/Recovery")]
        [LabelText("Recoil Recovery Speed")]
        [PropertyRange(1f, 10f)]
        [SerializeField] private float recoilRecoverySpeed = 4f;
        
        [BoxGroup("FPS Camera/Recoil/Limits")]
        [LabelText("Max Recoil")]
        [PropertyRange(5f, 30f)]
        [SerializeField] private float maxRecoil = 15f;
        
        [TabGroup("FPS Camera", "Bob & Effects")]
        [BoxGroup("FPS Camera/Bob & Effects/Head Bob")]
        [LabelText("Bob Frequency")]
        [PropertyRange(10f, 20f)]
        [SerializeField] private float bobFrequency = 15f;
        
        [BoxGroup("FPS Camera/Bob & Effects/Head Bob")]
        [LabelText("Bob Horizontal Amplitude")]
        [PropertyRange(0.01f, 0.1f)]
        [SerializeField] private float bobHorizontalAmplitude = 0.05f;
        
        [BoxGroup("FPS Camera/Bob & Effects/Head Bob")]
        [LabelText("Bob Vertical Amplitude")]
        [PropertyRange(0.01f, 0.1f)]
        [SerializeField] private float bobVerticalAmplitude = 0.03f;
        
        // Private variables
        private float verticalRotation = 0f;
        private Vector2 currentSway;
        private Vector2 targetSway;
        private float currentRecoil;
        private float targetRecoil;
        private float bobTimer;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Vector2 mouseInput;
        private bool isAiming;
        
        // References
        private WeaponSystem currentWeapon;
        
        [TabGroup("Debug", "Status")]
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Current Sway")]
        private Vector2 debugSway;
        
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Current Recoil")]
        private float debugRecoil;
        
        private void Start()
        {
            InitializeCamera();
        }
        
        private void Update()
        {
            HandleMouseLook();
            HandleWeaponSway();
            HandleRecoil();
            HandleHeadBob();
            UpdateDebugInfo();
        }
        
        private void LateUpdate()
        {
            ApplyCameraEffects();
        }
        
        private void InitializeCamera()
        {
            if (mainCamera == null)
                mainCamera = UnityEngine.Camera.main;
            
            if (cameraStateMachine == null)
                cameraStateMachine = GetComponent<CameraStateMachine>();
            
            // カーソルをロック
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // 初期位置を記録
            originalPosition = transform.localPosition;
            originalRotation = transform.localRotation;
        }
        
        private void HandleMouseLook()
        {
            // マウス入力を取得
            mouseInput.x = Input.GetAxis("Mouse X") * mouseSensitivityX;
            mouseInput.y = Input.GetAxis("Mouse Y") * mouseSensitivityY;
            
            // 水平回転（Y軸）
            transform.parent.Rotate(Vector3.up * mouseInput.x);
            
            // 垂直回転（X軸）- 角度制限付き
            verticalRotation -= mouseInput.y;
            verticalRotation = Mathf.Clamp(verticalRotation, minLookAngle, maxLookAngle);
            
            // 基本回転を適用
            Quaternion baseRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
            transform.localRotation = baseRotation;
        }
        
        private void HandleWeaponSway()
        {
            // マウス入力に基づくスウェイの計算
            float swayMultiplier = isAiming ? 0.3f : 1f; // エイム中はスウェイを減少
            
            targetSway.x = -mouseInput.x * swayAmount * swayMultiplier;
            targetSway.y = -mouseInput.y * swayAmount * swayMultiplier;
            
            // スウェイの制限
            targetSway.x = Mathf.Clamp(targetSway.x, -maxSwayAmount, maxSwayAmount);
            targetSway.y = Mathf.Clamp(targetSway.y, -maxSwayAmount, maxSwayAmount);
            
            // スムーズな補間
            currentSway = Vector2.Lerp(currentSway, targetSway, swaySmooth * Time.deltaTime);
            
            // マウス入力がない時のリセット
            if (Mathf.Approximately(mouseInput.x, 0f) && Mathf.Approximately(mouseInput.y, 0f))
            {
                currentSway = Vector2.Lerp(currentSway, Vector2.zero, swayResetSmooth * Time.deltaTime);
            }
        }
        
        private void HandleRecoil()
        {
            // 反動の減衰
            targetRecoil = Mathf.Lerp(targetRecoil, 0f, recoilRecoverySpeed * Time.deltaTime);
            currentRecoil = Mathf.Lerp(currentRecoil, targetRecoil, 10f * Time.deltaTime);
        }
        
        private void HandleHeadBob()
        {
            // 移動中のヘッドボブ
            float speed = GetMovementSpeed();
            if (speed > 0.1f)
            {
                bobTimer += Time.deltaTime * bobFrequency * speed;
            }
            else
            {
                // 停止時はボブをリセット
                bobTimer = Mathf.Lerp(bobTimer, 0f, 5f * Time.deltaTime);
            }
        }
        
        private void ApplyCameraEffects()
        {
            // 基本位置からの計算開始
            Vector3 newPosition = originalPosition;
            Vector3 newRotation = transform.localRotation.eulerAngles;
            
            // スウェイ効果をpositionに適用
            newPosition.x += currentSway.x;
            newPosition.y += currentSway.y;
            
            // 反動効果をrotationに適用
            newRotation.x += currentRecoil;
            
            // ヘッドボブ効果
            float bobOffsetX = Mathf.Sin(bobTimer) * bobHorizontalAmplitude;
            float bobOffsetY = Mathf.Cos(bobTimer * 2f) * bobVerticalAmplitude;
            
            newPosition.x += bobOffsetX;
            newPosition.y += bobOffsetY;
            
            // 武器ホルダーにエフェクトを適用
            if (weaponHolder != null)
            {
                weaponHolder.localPosition = newPosition;
                weaponHolder.localRotation = Quaternion.Euler(newRotation.x, newRotation.y, currentSway.x * 2f);
            }
        }
        
        public void AddRecoil(float vertical, float horizontal)
        {
            // 武器からの反動を追加
            targetRecoil += vertical * recoilStrength;
            targetRecoil = Mathf.Clamp(targetRecoil, 0f, maxRecoil);
            
            // 水平反動（ランダム方向）
            float horizontalRecoil = Random.Range(-horizontal, horizontal);
            transform.parent.Rotate(Vector3.up * horizontalRecoil * recoilStrength);
        }
        
        public void SetAiming(bool aiming)
        {
            isAiming = aiming;
            
            if (isAiming)
            {
                // エイム中のカメラ設定
                mouseSensitivityX *= 0.7f;
                mouseSensitivityY *= 0.7f;
            }
            else
            {
                // 通常時のカメラ設定に戻す
                mouseSensitivityX /= 0.7f;
                mouseSensitivityY /= 0.7f;
            }
        }
        
        public void SetWeapon(WeaponSystem weapon)
        {
            currentWeapon = weapon;
        }
        
        private float GetMovementSpeed()
        {
            // プレイヤーの移動速度を取得（CharacterControllerから）
            CharacterController controller = GetComponentInParent<CharacterController>();
            return controller != null ? controller.velocity.magnitude : 0f;
        }
        
        private void UpdateDebugInfo()
        {
            debugSway = currentSway;
            debugRecoil = currentRecoil;
        }
        
        // 外部からの呼び出し用メソッド
        public void OnWeaponFired()
        {
            if (currentWeapon != null)
            {
                AddRecoil(currentWeapon.WeaponData.VerticalRecoil, currentWeapon.WeaponData.HorizontalRecoil);
            }
        }
        
        private void OnEnable()
        {
            // 武器発射イベントのリスニング（イベントシステムと統合）
        }
        
        private void OnDisable()
        {
            // イベントリスナーの解除
        }
    }
}