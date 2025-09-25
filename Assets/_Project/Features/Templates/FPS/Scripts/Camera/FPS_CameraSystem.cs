using UnityEngine;
// using Cinemachine; // TODO: Cinemachine参照問題を解決後に復活
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.FPS.Data;

namespace asterivo.Unity60.Features.Templates.FPS.Camera
{
    /// <summary>
    /// FPSカメラシステム
    /// 詳細設計書3.3準拠：FirstPersonCameraとWeaponCameraの統合制御
    /// Cinemachine 3.1との統合によるスムーズなカメラ遷移を提供
    /// </summary>
    public class FPS_CameraSystem : MonoBehaviour
    {
        [Header("Camera References (Fallback)")]
        [SerializeField] private UnityEngine.Camera _firstPersonCamera; // Temporary fallback using Unity Camera
        [SerializeField] private UnityEngine.Camera _weaponCamera; // Temporary fallback using Unity Camera
        [SerializeField] private UnityEngine.Camera _aimingCamera; // Temporary fallback using Unity Camera

        // TODO: Cinemachine復活時に以下に切り替え
        // [SerializeField] private CinemachineCamera _firstPersonCamera;
        // [SerializeField] private CinemachineCamera _weaponCamera;
        // [SerializeField] private CinemachineCamera _aimingCamera;

        [Header("Camera Configuration")]
        [SerializeField] private FPSCameraConfig _cameraConfig;

        [Header("Camera Events")]
        [SerializeField] private GameEvent _onCameraModeChanged;
        [SerializeField] private GameEvent _onAimingStateChanged;

        [Header("Player References")]
        [SerializeField] private Transform _playerHead;
        [SerializeField] private Transform _weaponMount;

        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogs = false;

        // カメラ状態管理
        private FPSCameraMode _currentCameraMode = FPSCameraMode.FirstPerson;
        private bool _isAiming = false;
        private bool _isWeaponEquipped = false;

        // カメラ遷移制御
        private float _currentFOV;
        private float _targetFOV;
        private float _fovTransitionSpeed = 5.0f;

        // プロパティ
        public FPSCameraMode CurrentCameraMode => _currentCameraMode;
        public bool IsAiming => _isAiming;
        public bool IsWeaponEquipped => _isWeaponEquipped;

        #region Unity Lifecycle

        private void Awake()
        {
            ValidateConfiguration();
            InitializeCameras();
        }

        private void Start()
        {
            SetCameraMode(FPSCameraMode.FirstPerson);
        }

        private void Update()
        {
            UpdateCameraTransitions();
        }

        #endregion

        #region Camera Mode Management

        /// <summary>
        /// カメラモードの変更
        /// </summary>
        public void SetCameraMode(FPSCameraMode mode)
        {
            if (_currentCameraMode == mode) return;

            FPSCameraMode previousMode = _currentCameraMode;
            _currentCameraMode = mode;

            ApplyCameraMode(mode);

            if (_enableDebugLogs)
            {
                Debug.Log($"[FPS_CameraSystem] Camera mode changed: {previousMode} → {mode}");
            }

            _onCameraModeChanged?.Raise();
        }

        /// <summary>
        /// エイミング状態の切り替え
        /// </summary>
        public void SetAimingState(bool isAiming)
        {
            if (_isAiming == isAiming) return;

            _isAiming = isAiming;
            ApplyAimingState(isAiming);

            if (_enableDebugLogs)
            {
                Debug.Log($"[FPS_CameraSystem] Aiming state: {isAiming}");
            }

            _onAimingStateChanged?.Raise();
        }

        /// <summary>
        /// 武器装備状態の設定
        /// </summary>
        public void SetWeaponEquipped(bool equipped)
        {
            _isWeaponEquipped = equipped;
            UpdateCameraConfiguration();
        }

        #endregion

        #region Camera Configuration

        /// <summary>
        /// カメラモードの適用
        /// </summary>
        private void ApplyCameraMode(FPSCameraMode mode)
        {
            // 全カメラを無効化
            SetAllCamerasEnabled(false);

            switch (mode)
            {
                case FPSCameraMode.FirstPerson:
                    _firstPersonCamera.enabled = true;
                    _targetFOV = _cameraConfig.firstPersonFOV;
                    break;

                case FPSCameraMode.Weapon:
                    if (_isWeaponEquipped && _weaponCamera != null)
                    {
                        _weaponCamera.enabled = true;
                        _targetFOV = _cameraConfig.weaponFOV;
                    }
                    else
                    {
                        // 武器が装備されていない場合はFirstPersonに戻る
                        _firstPersonCamera.enabled = true;
                        _currentCameraMode = FPSCameraMode.FirstPerson;
                        _targetFOV = _cameraConfig.firstPersonFOV;
                    }
                    break;

                case FPSCameraMode.Aiming:
                    if (_aimingCamera != null)
                    {
                        _aimingCamera.enabled = true;
                        _targetFOV = _cameraConfig.aimingFOV;
                    }
                    else
                    {
                        _firstPersonCamera.enabled = true;
                        _targetFOV = _cameraConfig.aimingFOV;
                    }
                    break;
            }
        }

        /// <summary>
        /// エイミング状態の適用
        /// </summary>
        private void ApplyAimingState(bool isAiming)
        {
            if (isAiming)
            {
                SetCameraMode(FPSCameraMode.Aiming);
            }
            else
            {
                // 武器が装備されている場合はWeaponモード、そうでなければFirstPersonモード
                SetCameraMode(_isWeaponEquipped ? FPSCameraMode.Weapon : FPSCameraMode.FirstPerson);
            }
        }

        /// <summary>
        /// 全カメラの有効/無効設定
        /// </summary>
        private void SetAllCamerasEnabled(bool enabled)
        {
            if (_firstPersonCamera != null)
                _firstPersonCamera.enabled = enabled;

            if (_weaponCamera != null)
                _weaponCamera.enabled = enabled;

            if (_aimingCamera != null)
                _aimingCamera.enabled = enabled;
        }

        #endregion

        #region Camera Transitions

        /// <summary>
        /// カメラ遷移の更新
        /// </summary>
        private void UpdateCameraTransitions()
        {
            // FOV遷移の処理
            if (Mathf.Abs(_currentFOV - _targetFOV) > 0.1f)
            {
                _currentFOV = Mathf.Lerp(_currentFOV, _targetFOV, _fovTransitionSpeed * Time.deltaTime);
                ApplyFOVToActiveCamera(_currentFOV);
            }
        }

        /// <summary>
        /// アクティブカメラにFOVを適用
        /// </summary>
        private void ApplyFOVToActiveCamera(float fov)
        {
            // TODO: Cinemachine統合後に復活
            // 一時的に基本Camera実装
            var mainCamera = UnityEngine.Camera.main;
            if (mainCamera != null)
            {
                mainCamera.fieldOfView = fov;
            }
        }

        /*
        /// <summary>
        /// 現在アクティブなカメラの取得
        /// </summary>
        private CinemachineCamera GetActiveCamera()
        {
            if (_firstPersonCamera != null && _firstPersonCamera.enabled)
                return _firstPersonCamera;

            if (_weaponCamera != null && _weaponCamera.enabled)
                return _weaponCamera;

            if (_aimingCamera != null && _aimingCamera.enabled)
                return _aimingCamera;

            return null;
        }
        */ // TODO: Cinemachine復活時に有効化

        #endregion

        #region Camera Effects

        /// <summary>
        /// 武器の反動によるカメラシェイク
        /// </summary>
        public void ApplyRecoilShake(float intensity, float duration)
        {
            if (_cameraConfig != null && _cameraConfig.enableRecoilShake)
            {
                // TODO: Cinemachine統合後に復活
                // 一時的に基本的なカメラシェイク実装
                StartCoroutine(ApplyBasicCameraShake(intensity, duration));
            }
        }

        /// <summary>
        /// 基本的なカメラシェイクのコルーチン（Cinemachine代替）
        /// </summary>
        private System.Collections.IEnumerator ApplyBasicCameraShake(float intensity, float duration)
        {
            var mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null) yield break;

            Vector3 originalPosition = mainCamera.transform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float currentIntensity = Mathf.Lerp(intensity, 0f, elapsed / duration);

                Vector3 randomOffset = new Vector3(
                    Random.Range(-1f, 1f) * currentIntensity * 0.1f,
                    Random.Range(-1f, 1f) * currentIntensity * 0.1f,
                    0f
                );

                mainCamera.transform.localPosition = originalPosition + randomOffset;
                yield return null;
            }

            mainCamera.transform.localPosition = originalPosition;
        }

        /// <summary>
        /// ヒットマーカー表示
        /// </summary>
        public void ShowHitMarker(bool isHeadshot = false)
        {
            // UI システムとの連携でヒットマーカーを表示
            // 実装はUIシステム完成後に追加
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// カメラ設定のリロード
        /// </summary>
        public void ReloadCameraConfiguration()
        {
            if (_cameraConfig != null)
            {
                UpdateCameraConfiguration();

                if (_enableDebugLogs)
                {
                    Debug.Log("[FPS_CameraSystem] Camera configuration reloaded");
                }
            }
        }

        /// <summary>
        /// カメラ統計情報の取得
        /// </summary>
        public FPSCameraStats GetCameraStats()
        {
            return new FPSCameraStats
            {
                CurrentMode = _currentCameraMode,
                IsAiming = _isAiming,
                IsWeaponEquipped = _isWeaponEquipped,
                CurrentFOV = _currentFOV,
                TargetFOV = _targetFOV
            };
        }

        #endregion

        #region Initialization & Validation

        /// <summary>
        /// カメラの初期化
        /// </summary>
        private void InitializeCameras()
        {
            if (_cameraConfig != null)
            {
                _currentFOV = _cameraConfig.firstPersonFOV;
                _targetFOV = _currentFOV;
                _fovTransitionSpeed = _cameraConfig.fovTransitionSpeed;
            }

            // カメラの初期設定
            SetupCameras();
        }

        /// <summary>
        /// カメラのセットアップ（Fallback to Unity Camera）
        /// </summary>
        private void SetupCameras()
        {
            // FirstPersonCameraの設定
            if (_firstPersonCamera != null && _playerHead != null)
            {
                _firstPersonCamera.transform.SetParent(_playerHead);
                _firstPersonCamera.transform.localPosition = Vector3.zero;
                _firstPersonCamera.transform.localRotation = Quaternion.identity;
            }

            // WeaponCameraの設定
            if (_weaponCamera != null && _weaponMount != null)
            {
                _weaponCamera.transform.SetParent(_weaponMount);
                _weaponCamera.transform.localPosition = Vector3.zero;
                _weaponCamera.transform.localRotation = Quaternion.identity;
            }

            // 初期状態では全て無効化
            SetAllCamerasEnabled(false);
        }

        /// <summary>
        /// 設定の検証
        /// </summary>
        private void ValidateConfiguration()
        {
            if (_cameraConfig == null)
            {
                Debug.LogWarning("[FPS_CameraSystem] Camera configuration is missing!");
                return;
            }

            if (_firstPersonCamera == null)
            {
                Debug.LogWarning("[FPS_CameraSystem] First person camera is not assigned!");
            }

            if (_playerHead == null)
            {
                Debug.LogWarning("[FPS_CameraSystem] Player head transform is not assigned!");
            }
        }

        /// <summary>
        /// カメラ設定の更新
        /// </summary>
        private void UpdateCameraConfiguration()
        {
            if (_cameraConfig == null) return;

            _fovTransitionSpeed = _cameraConfig.fovTransitionSpeed;

            // 現在のモードに応じてFOVを更新
            switch (_currentCameraMode)
            {
                case FPSCameraMode.FirstPerson:
                    _targetFOV = _cameraConfig.firstPersonFOV;
                    break;
                case FPSCameraMode.Weapon:
                    _targetFOV = _cameraConfig.weaponFOV;
                    break;
                case FPSCameraMode.Aiming:
                    _targetFOV = _cameraConfig.aimingFOV;
                    break;
            }
        }

        #endregion

        #region Debug

        /// <summary>
        /// デバッグ情報の取得
        /// </summary>
        public string GetDebugInfo()
        {
            var stats = GetCameraStats();
            return $"FPS_CameraSystem - Mode: {stats.CurrentMode}, Aiming: {stats.IsAiming}, " +
                   $"Weapon: {stats.IsWeaponEquipped}, FOV: {stats.CurrentFOV:F1}";
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_enableDebugLogs) return;

            // カメラの視野角を可視化
            var mainCamera = UnityEngine.Camera.main;
            if (mainCamera != null)
            {
                Vector3 position = mainCamera.transform.position;
                Vector3 forward = mainCamera.transform.forward;

                // FOVに基づく視野角の可視化
                float fovRad = _currentFOV * Mathf.Deg2Rad;
                float distance = 10f;

                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(position, forward * distance);

                // 視野角の端を描画
                Vector3 right = mainCamera.transform.right;
                Vector3 up = mainCamera.transform.up;

                float halfFOV = fovRad * 0.5f;
                Vector3 topLeft = Quaternion.AngleAxis(-halfFOV, right) * forward * distance;
                Vector3 topRight = Quaternion.AngleAxis(halfFOV, right) * forward * distance;
                Vector3 bottomLeft = Quaternion.AngleAxis(-halfFOV, right) * forward * distance;
                Vector3 bottomRight = Quaternion.AngleAxis(halfFOV, right) * forward * distance;

                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(position, topLeft);
                Gizmos.DrawRay(position, topRight);
                Gizmos.DrawRay(position, bottomLeft);
                Gizmos.DrawRay(position, bottomRight);
            }
        }
#endif

        #endregion
    }

    /// <summary>
    /// FPSカメラ統計情報
    /// </summary>
    [System.Serializable]
    public struct FPSCameraStats
    {
        public FPSCameraMode CurrentMode;
        public bool IsAiming;
        public bool IsWeaponEquipped;
        public float CurrentFOV;
        public float TargetFOV;
    }
}
