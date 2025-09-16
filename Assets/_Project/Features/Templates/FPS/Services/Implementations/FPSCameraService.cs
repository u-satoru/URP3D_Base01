using System;
using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Features.Templates.FPS.Data;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// FPSカメラサービス実装 (Cinemachine統合は一時的に無効化)
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// TODO: Cinemachine 3.1統合の修復が必要
    /// </summary>
    public class FPSCameraService : IFPSCameraService
    {
        // NOTE: Cinemachineの参照を一時的にコメントアウト
        // private readonly Dictionary<CameraState, CinemachineVirtualCamera> _virtualCameras = new();
        private readonly Dictionary<CameraState, Camera> _virtualCameras = new();
        private CameraState _currentState = CameraState.FirstPerson;
        private Transform _playerTarget;
        private Transform _aimTarget;
        private float _defaultFOV = 60f;

        // プロパティ実装
        public CameraState CurrentState => _currentState;
        public bool IsTransitioning { get; private set; } = false;

        // イベント
        public event Action<CameraState> OnCameraStateChanged;
        public event Action<CameraState, CameraState> OnCameraTransition;

        // 初期化
        public void InitializeCameras(Transform playerTarget, CameraConfiguration config)
        {
            _playerTarget = playerTarget;
            _defaultFOV = config?.DefaultFOV ?? 60f;

            if (_playerTarget == null)
            {
                Debug.LogError("[FPSCameraService] Player target cannot be null");
                return;
            }

            // Cinemachine Virtual Cameraの設定
            SetupVirtualCameras(config);

            Debug.Log("[FPSCameraService] Cameras initialized");
        }

        // カメラ状態制御
        public void SwitchToFirstPerson()
        {
            ChangeState(CameraState.FirstPerson);
        }

        public void SwitchToThirdPerson()
        {
            ChangeState(CameraState.ThirdPerson);
        }

        public void SwitchToAim()
        {
            ChangeState(CameraState.Aim);
        }

        public void SwitchToCover()
        {
            ChangeState(CameraState.Cover);
        }

        public void SetCameraState(CameraState state)
        {
            ChangeState(state);
        }

        // カメラ設定
        public void SetCameraTarget(Transform target)
        {
            if (target == null)
            {
                Debug.LogWarning("[FPSCameraService] Cannot set null camera target");
                return;
            }

            _playerTarget = target;
            UpdateAllCameraTargets();

            Debug.Log($"[FPSCameraService] Camera target updated to: {target.name}");
        }

        public void SetAimTarget(Transform aimTarget)
        {
            _aimTarget = aimTarget;

            if (_virtualCameras.TryGetValue(CameraState.Aim, out var aimCamera) && aimCamera != null)
            {
                aimCamera.LookAt = _aimTarget;
            }

            Debug.Log($"[FPSCameraService] Aim target set to: {aimTarget?.name ?? "null"}");
        }

        public void SetFOV(float fov)
        {
            if (fov <= 0f || fov >= 180f)
            {
                Debug.LogWarning($"[FPSCameraService] Invalid FOV value: {fov}");
                return;
            }

            foreach (var camera in _virtualCameras.Values)
            {
                if (camera != null)
                {
                    camera.m_Lens.FieldOfView = fov;
                }
            }

            _defaultFOV = fov;
            Debug.Log($"[FPSCameraService] FOV set to: {fov}");
        }

        // カメラエフェクト
        public void ApplyCameraShake(float intensity, float duration)
        {
            if (intensity <= 0f || duration <= 0f)
            {
                Debug.LogWarning("[FPSCameraService] Invalid shake parameters");
                return;
            }

            var currentCamera = GetCurrentVirtualCamera();
            if (currentCamera != null)
            {
                // Cinemachine Impulse システムを使用した振動
                var impulse = currentCamera.GetComponent<CinemachineImpulseSource>();
                if (impulse == null)
                {
                    impulse = currentCamera.gameObject.AddComponent<CinemachineImpulseSource>();
                }

                impulse.GenerateImpulse(Vector3.one * intensity);

                // ServiceLocator経由でAudioサービス取得
                var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
                audioService?.PlaySFX("CameraShake", Vector3.zero);
            }

            Debug.Log($"[FPSCameraService] Camera shake applied: intensity={intensity}, duration={duration}");
        }

        public void SetCameraSensitivity(float sensitivity)
        {
            if (sensitivity <= 0f)
            {
                Debug.LogWarning($"[FPSCameraService] Invalid sensitivity value: {sensitivity}");
                return;
            }

            // Cinemachine POV コンポーネントの感度設定
            foreach (var camera in _virtualCameras.Values)
            {
                if (camera != null)
                {
                    var pov = camera.GetCinemachineComponent<CinemachinePOV>();
                    if (pov != null)
                    {
                        pov.m_HorizontalAxis.m_MaxSpeed = sensitivity * 300f;
                        pov.m_VerticalAxis.m_MaxSpeed = sensitivity * 2f;
                    }
                }
            }

            Debug.Log($"[FPSCameraService] Camera sensitivity set to: {sensitivity}");
        }

        // プライベートメソッド
        private void ChangeState(CameraState newState)
        {
            if (_currentState == newState)
                return;

            var previousState = _currentState;
            IsTransitioning = true;

            // 状態遷移イベント発行
            OnCameraTransition?.Invoke(previousState, newState);

            // 現在のカメラを非アクティブ化
            if (_virtualCameras.TryGetValue(_currentState, out var currentCamera) && currentCamera != null)
            {
                currentCamera.Priority = 0;
            }

            // 新しいカメラをアクティブ化
            if (_virtualCameras.TryGetValue(newState, out var newCamera) && newCamera != null)
            {
                newCamera.Priority = 10;
                _currentState = newState;
            }
            else
            {
                Debug.LogWarning($"[FPSCameraService] No virtual camera found for state: {newState}");
                IsTransitioning = false;
                return;
            }

            // カメラ状態変更イベント発行
            OnCameraStateChanged?.Invoke(_currentState);

            // 遷移完了を少し遅らせる（Cinemachineのブレンド時間考慮）
            _ = System.Threading.Tasks.Task.Delay(500).ContinueWith(_ => {
                IsTransitioning = false;
            });

            Debug.Log($"[FPSCameraService] Camera state changed: {previousState} → {newState}");
        }

        private void SetupVirtualCameras(CameraConfiguration config)
        {
            // FirstPerson カメラセットアップ
            var firstPersonCamera = CreateVirtualCamera("FPS_FirstPerson", CameraState.FirstPerson, config);
            SetupFirstPersonCamera(firstPersonCamera, config);

            // ThirdPerson カメラセットアップ
            var thirdPersonCamera = CreateVirtualCamera("FPS_ThirdPerson", CameraState.ThirdPerson, config);
            SetupThirdPersonCamera(thirdPersonCamera, config);

            // Aim カメラセットアップ
            var aimCamera = CreateVirtualCamera("FPS_Aim", CameraState.Aim, config);
            SetupAimCamera(aimCamera, config);

            // Cover カメラセットアップ
            var coverCamera = CreateVirtualCamera("FPS_Cover", CameraState.Cover, config);
            SetupCoverCamera(coverCamera, config);

            // 初期状態設定
            ChangeState(CameraState.FirstPerson);
        }

        private CinemachineVirtualCamera CreateVirtualCamera(string cameraName, CameraState state, CameraConfiguration config)
        {
            var cameraGO = new GameObject(cameraName);
            var virtualCamera = cameraGO.AddComponent<CinemachineVirtualCamera>();

            virtualCamera.Follow = _playerTarget;
            virtualCamera.LookAt = _playerTarget;
            virtualCamera.Priority = (state == CameraState.FirstPerson) ? 10 : 0;

            _virtualCameras[state] = virtualCamera;

            return virtualCamera;
        }

        private void SetupFirstPersonCamera(CinemachineVirtualCamera camera, CameraConfiguration config)
        {
            var pov = camera.AddCinemachineComponent<CinemachinePOV>();
            pov.m_HorizontalAxis.m_MaxSpeed = 300f;
            pov.m_VerticalAxis.m_MaxSpeed = 2f;
            pov.m_VerticalAxis.m_MinValue = -80f;
            pov.m_VerticalAxis.m_MaxValue = 80f;

            camera.m_Lens.FieldOfView = config?.FirstPersonFOV ?? 60f;
        }

        private void SetupThirdPersonCamera(CinemachineVirtualCamera camera, CameraConfiguration config)
        {
            var composer = camera.AddCinemachineComponent<CinemachineComposer>();
            composer.m_TrackedObjectOffset = Vector3.up * 1.5f;

            var orbitalTransposer = camera.AddCinemachineComponent<CinemachineOrbitalTransposer>();
            orbitalTransposer.m_FollowOffset = new Vector3(0, 2f, -3f);

            camera.m_Lens.FieldOfView = config?.ThirdPersonFOV ?? 60f;
        }

        private void SetupAimCamera(CinemachineVirtualCamera camera, CameraConfiguration config)
        {
            var pov = camera.AddCinemachineComponent<CinemachinePOV>();
            pov.m_HorizontalAxis.m_MaxSpeed = 150f; // 照準時は感度を下げる
            pov.m_VerticalAxis.m_MaxSpeed = 1f;

            camera.m_Lens.FieldOfView = config?.AimFOV ?? 40f; // ズーム効果
        }

        private void SetupCoverCamera(CinemachineVirtualCamera camera, CameraConfiguration config)
        {
            var composer = camera.AddCinemachineComponent<CinemachineComposer>();
            composer.m_TrackedObjectOffset = Vector3.up * 1.2f;

            var orbitalTransposer = camera.AddCinemachineComponent<CinemachineOrbitalTransposer>();
            orbitalTransposer.m_FollowOffset = new Vector3(1.5f, 1.5f, -2f); // 遮蔽物の横から覗く

            camera.m_Lens.FieldOfView = config?.CoverFOV ?? 55f;
        }

        private CinemachineVirtualCamera GetCurrentVirtualCamera()
        {
            return _virtualCameras.TryGetValue(_currentState, out var camera) ? camera : null;
        }

        private void UpdateAllCameraTargets()
        {
            foreach (var camera in _virtualCameras.Values)
            {
                if (camera != null)
                {
                    camera.Follow = _playerTarget;
                    if (_currentState != CameraState.Aim) // Aimカメラ以外
                    {
                        camera.LookAt = _playerTarget;
                    }
                }
            }
        }
    }
}