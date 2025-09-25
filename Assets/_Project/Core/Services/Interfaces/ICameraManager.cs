using UnityEngine;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Interface for camera management service
    /// Provides centralized camera control and state management
    /// </summary>
    public interface ICameraManager : IService
    {
        /// <summary>
        /// Set third person camera distance from target
        /// </summary>
        /// <param name="distance">Distance from target</param>
        void SetThirdPersonDistance(float distance);

        /// <summary>
        /// Set third person camera height offset
        /// </summary>
        /// <param name="height">Height offset from target</param>
        void SetThirdPersonHeight(float height);

        /// <summary>
        /// Set camera field of view
        /// </summary>
        /// <param name="fov">Field of view in degrees</param>
        void SetFieldOfView(float fov);

        /// <summary>
        /// Set camera transition speed
        /// </summary>
        /// <param name="speed">Transition speed</param>
        void SetTransitionSpeed(float speed);

        /// <summary>
        /// Get current third person distance
        /// </summary>
        float GetThirdPersonDistance();

        /// <summary>
        /// Get current third person height
        /// </summary>
        float GetThirdPersonHeight();

        /// <summary>
        /// Get current field of view
        /// </summary>
        float GetFieldOfView();

        /// <summary>
        /// Get current transition speed
        /// </summary>
        float GetTransitionSpeed();

        /// <summary>
        /// Set camera follow target
        /// </summary>
        /// <param name="target">Transform to follow</param>
        void SetFollowTarget(Transform target);

        /// <summary>
        /// Set camera look at target
        /// </summary>
        /// <param name="target">Transform to look at</param>
        void SetLookAtTarget(Transform target);

        /// <summary>
        /// Get current follow target
        /// </summary>
        Transform GetFollowTarget();

        /// <summary>
        /// Get current look at target
        /// </summary>
        Transform GetLookAtTarget();

        /// <summary>
        /// Switch to first person camera
        /// </summary>
        void SwitchToFirstPerson();

        /// <summary>
        /// Switch to third person camera
        /// </summary>
        void SwitchToThirdPerson();

        /// <summary>
        /// Switch to aim camera mode
        /// </summary>
        void SwitchToAimMode();

        /// <summary>
        /// Switch to cover camera mode
        /// </summary>
        void SwitchToCoverMode();

        /// <summary>
        /// Check if camera is in first person mode
        /// </summary>
        bool IsFirstPerson();

        /// <summary>
        /// Check if camera is in third person mode
        /// </summary>
        bool IsThirdPerson();

        /// <summary>
        /// Check if camera is in aim mode
        /// </summary>
        bool IsAimMode();

        /// <summary>
        /// Check if camera is in cover mode
        /// </summary>
        bool IsCoverMode();

        /// <summary>
        /// Set camera shake intensity
        /// </summary>
        /// <param name="intensity">Shake intensity</param>
        /// <param name="duration">Shake duration</param>
        void AddCameraShake(float intensity, float duration);

        /// <summary>
        /// Stop camera shake
        /// </summary>
        void StopCameraShake();

        /// <summary>
        /// Set camera movement damping
        /// </summary>
        /// <param name="damping">Damping value</param>
        void SetMovementDamping(float damping);

        /// <summary>
        /// Set camera rotation damping
        /// </summary>
        /// <param name="damping">Damping value</param>
        void SetRotationDamping(float damping);

        /// <summary>
        /// Get current camera position
        /// </summary>
        Vector3 GetCameraPosition();

        /// <summary>
        /// Get current camera rotation
        /// </summary>
        Quaternion GetCameraRotation();

        /// <summary>
        /// Get main camera component
        /// </summary>
        UnityEngine.Camera GetMainCamera();

        /// <summary>
        /// Apply screen shake effect (TPS Template compatibility)
        /// </summary>
        /// <param name="intensity">Shake intensity</param>
        /// <param name="duration">Shake duration</param>
        void ApplyScreenShake(float intensity, float duration = 0.5f);

        /// <summary>
        /// Apply recoil effect to camera (TPS Template compatibility)
        /// </summary>
        /// <param name="recoilAmount">Recoil intensity</param>
        void ApplyRecoil(float recoilAmount);

        /// <summary>
        /// Handle player death camera behavior (TPS Template compatibility)
        /// </summary>
        void OnPlayerDeath();

        /// <summary>
        /// Handle player respawn camera behavior (TPS Template compatibility)
        /// </summary>
        void OnPlayerRespawn();
    }
}
