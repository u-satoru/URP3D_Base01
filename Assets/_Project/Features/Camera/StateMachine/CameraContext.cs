using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Camera;
using asterivo.Unity60.Features.Camera.ViewMode;
using asterivo.Unity60.Features.Camera.States;

namespace asterivo.Unity60.Features.Camera.StateMachine
{
    /// <summary>
    /// Camera階層化ステートマシンのコンテキスト
    /// Camera状態間で共有されるデータとシステム参照を管理
    /// </summary>
    public class CameraContext
    {
        // Core Camera Properties
        public Transform Transform { get; }
        public UnityEngine.Camera MainCamera { get; }
        public Transform CameraRig { get; }
        public Transform FollowTarget { get; set; }

        // Camera Settings
        public FirstPersonSettings FirstPersonSettings { get; }
        public ThirdPersonSettings ThirdPersonSettings { get; }
        public CoverViewSettings CoverSettings { get; }
        public AimSettings AimSettings { get; }

        // State Management
        public float StateTimer { get; set; }
        public string PreviousStateKey { get; set; }
        public bool IsActive { get; set; }

        // Input Data
        public Vector2 LookInput { get; set; }
        public float ZoomInput { get; set; }
        public bool CameraModeTogglePressed { get; set; }
        public bool AimPressed { get; set; }
        public bool AimReleased { get; set; }
        public bool ResetCameraPressed { get; set; }

        // Camera State
        public Vector3 CurrentPosition { get; set; }
        public Quaternion CurrentRotation { get; set; }
        public float CurrentFOV { get; set; }
        public float TargetDistance { get; set; }

        // Events
        public GameEvent<System.Enum> OnCameraStateChanged { get; }

        // System Interfaces (for future expansion)
        public ICameraController CameraController { get; set; }
        public ICameraService CameraService { get; set; }

        public CameraContext(
            Transform transform,
            UnityEngine.Camera mainCamera,
            Transform cameraRig,
            Transform followTarget,
            FirstPersonSettings firstPersonSettings,
            ThirdPersonSettings thirdPersonSettings,
            CoverViewSettings coverSettings,
            AimSettings aimSettings,
            GameEvent<System.Enum> onCameraStateChanged)
        {
            Transform = transform;
            MainCamera = mainCamera;
            CameraRig = cameraRig;
            FollowTarget = followTarget;
            FirstPersonSettings = firstPersonSettings;
            ThirdPersonSettings = thirdPersonSettings;
            CoverSettings = coverSettings;
            AimSettings = aimSettings;
            OnCameraStateChanged = onCameraStateChanged;

            // Initialize state
            IsActive = true;
            StateTimer = 0f;
            CurrentFOV = mainCamera?.fieldOfView ?? 60f;
            TargetDistance = 5f;

            if (mainCamera != null)
            {
                CurrentPosition = mainCamera.transform.position;
                CurrentRotation = mainCamera.transform.rotation;
            }
        }

        // Camera Helper Methods
        public Vector3 GetCameraPosition()
        {
            return MainCamera != null ? MainCamera.transform.position : Transform.position;
        }

        public Quaternion GetCameraRotation()
        {
            return MainCamera != null ? MainCamera.transform.rotation : Transform.rotation;
        }

        public Vector3 GetCameraForward()
        {
            return MainCamera != null ? MainCamera.transform.forward : Transform.forward;
        }

        public bool CanSeePoint(Vector3 worldPoint)
        {
            if (MainCamera == null) return false;

            Vector3 viewportPoint = MainCamera.WorldToViewportPoint(worldPoint);
            return viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                   viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
                   viewportPoint.z > 0;
        }

        // Input Management
        public void UpdateInput(Vector2 lookInput, float zoomInput, bool cameraModeToggle, bool aimPressed, bool aimReleased, bool resetCamera)
        {
            LookInput = lookInput;
            ZoomInput = zoomInput;
            CameraModeTogglePressed = cameraModeToggle;
            AimPressed = aimPressed;
            AimReleased = aimReleased;
            ResetCameraPressed = resetCamera;
        }

        public void ClearInput()
        {
            LookInput = Vector2.zero;
            ZoomInput = 0f;
            CameraModeTogglePressed = false;
            AimPressed = false;
            AimReleased = false;
            ResetCameraPressed = false;
        }

        // Camera Transform Management
        public void SetCameraPosition(Vector3 position)
        {
            CurrentPosition = position;
            if (MainCamera != null)
            {
                MainCamera.transform.position = position;
            }
        }

        public void SetCameraRotation(Quaternion rotation)
        {
            CurrentRotation = rotation;
            if (MainCamera != null)
            {
                MainCamera.transform.rotation = rotation;
            }
        }

        public void SetCameraFOV(float fov)
        {
            CurrentFOV = fov;
            if (MainCamera != null)
            {
                MainCamera.fieldOfView = fov;
            }
        }

        // Target Management
        public void SetTarget(Transform target)
        {
            FollowTarget = target;
        }

        public Vector3 GetTargetPosition()
        {
            return FollowTarget != null ? FollowTarget.position : Vector3.zero;
        }

        public Vector3 GetTargetForward()
        {
            return FollowTarget != null ? FollowTarget.forward : Vector3.forward;
        }

        // Time Management
        public void ResetStateTimer()
        {
            StateTimer = 0f;
        }

        public void UpdateStateTimer()
        {
            StateTimer += Time.deltaTime;
        }

        // Utility Methods
        public float GetMouseSensitivity()
        {
            // Return appropriate sensitivity based on current context
            return 1f; // TODO: Implement settings-based sensitivity
        }

        public float GetControllerSensitivity()
        {
            // Return appropriate sensitivity for controller input
            return 1f; // TODO: Implement settings-based sensitivity
        }

        public bool IsInputAllowed()
        {
            return IsActive && MainCamera != null;
        }

        // Distance Management for Third Person
        public void SetTargetDistance(float distance)
        {
            TargetDistance = Mathf.Clamp(distance, 1f, 20f);
        }

        public void AdjustDistance(float deltaDistance)
        {
            TargetDistance = Mathf.Clamp(TargetDistance + deltaDistance, 1f, 20f);
        }

        // Camera Collision Detection
        public bool CheckCameraCollision(Vector3 desiredPosition, out Vector3 adjustedPosition)
        {
            adjustedPosition = desiredPosition;

            if (FollowTarget == null) return false;

            // Simple raycast from target to desired camera position
            Vector3 direction = (desiredPosition - GetTargetPosition()).normalized;
            float distance = Vector3.Distance(GetTargetPosition(), desiredPosition);

            if (Physics.Raycast(GetTargetPosition(), direction, out RaycastHit hit, distance, LayerMask.GetMask("Default", "Environment")))
            {
                // Adjust position to avoid collision
                adjustedPosition = hit.point - direction * 0.2f; // Small offset
                return true;
            }

            return false;
        }

        // Debug Information
        public string GetDebugInfo()
        {
            return $"Camera Context - Position: {CurrentPosition:F2}, Rotation: {CurrentRotation.eulerAngles:F1}, " +
                   $"FOV: {CurrentFOV:F1}, Target: {(FollowTarget != null ? FollowTarget.name : "None")}, " +
                   $"Distance: {TargetDistance:F1}, Active: {IsActive}";
        }
    }

}