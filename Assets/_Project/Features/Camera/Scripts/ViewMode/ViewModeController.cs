using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Collections;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Camera
{
    public enum ViewMode
    {
        FirstPerson,
        ThirdPerson,
        Cover,
        Transition
    }
    
    public class ViewModeController : MonoBehaviour
    {
        [Header("Camera References")]
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private Transform cameraRig;
        [SerializeField] private Transform playerTransform;
        
        [Header("View Configurations")]
        [SerializeField] private ViewModeSettings currentSettings;
        [SerializeField] private FirstPersonSettings fpsSettings;
        [SerializeField] private ThirdPersonSettings tpsSettings;
        [SerializeField] private CoverViewSettings coverSettings;
        
        [Header("Events")]
        [SerializeField] private GenericGameEvent<ViewMode> onViewModeChanged;
        
        [Header("Current State")]
        [SerializeField] private ViewMode currentMode = ViewMode.ThirdPerson;
        [SerializeField] private ViewMode previousMode = ViewMode.ThirdPerson;
        [SerializeField] private bool isTransitioning = false;
        
        private Coroutine transitionCoroutine;
        
        private void Start()
        {
            if (mainCamera == null)
                mainCamera = UnityEngine.Camera.main;
                
            if (cameraRig == null)
                cameraRig = transform;
                
            ApplyViewMode(currentMode, true);
        }
        
        public void ToggleViewMode()
        {
            if (isTransitioning) return;
            
            ViewMode targetMode = currentMode == ViewMode.FirstPerson ? 
                ViewMode.ThirdPerson : ViewMode.FirstPerson;
                
            SwitchToView(targetMode);
        }
        
        public void SwitchToView(ViewMode targetMode)
        {
            if (isTransitioning || currentMode == targetMode) return;
            
            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);
                
            transitionCoroutine = StartCoroutine(TransitionToView(targetMode));
        }
        
        public void SwitchToCoverView()
        {
            if (currentMode != ViewMode.Cover)
            {
                previousMode = currentMode;
                SwitchToView(ViewMode.Cover);
            }
        }
        
        public void ExitCoverView()
        {
            if (currentMode == ViewMode.Cover)
            {
                SwitchToView(previousMode);
            }
        }
        
        private IEnumerator TransitionToView(ViewMode targetMode)
        {
            isTransitioning = true;
            ViewMode startMode = currentMode;
            currentMode = ViewMode.Transition;
            
            ViewModeSettings startSettings = GetSettingsForMode(startMode);
            ViewModeSettings targetSettings = GetSettingsForMode(targetMode);
            
            if (targetSettings == null)
            {
                Debug.LogError($"No settings found for view mode: {targetMode}");
                isTransitioning = false;
                yield break;
            }
            
            float elapsed = 0f;
            float duration = targetSettings.transitionDuration;
            
            Vector3 startPos = cameraRig.localPosition;
            Quaternion startRot = cameraRig.localRotation;
            float startFOV = mainCamera.fieldOfView;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = targetSettings.transitionCurve.Evaluate(elapsed / duration);
                
                cameraRig.localPosition = Vector3.Lerp(startPos, targetSettings.cameraOffset, t);
                cameraRig.localRotation = Quaternion.Slerp(startRot, 
                    Quaternion.Euler(targetSettings.cameraRotation), t);
                mainCamera.fieldOfView = Mathf.Lerp(startFOV, targetSettings.fieldOfView, t);
                
                yield return null;
            }
            
            ApplyViewMode(targetMode, false);
            
            currentMode = targetMode;
            isTransitioning = false;
            
            onViewModeChanged?.Raise(currentMode);
            
            transitionCoroutine = null;
        }
        
        private void ApplyViewMode(ViewMode mode, bool immediate)
        {
            ViewModeSettings settings = GetSettingsForMode(mode);
            if (settings == null) return;
            
            currentSettings = settings;
            
            if (immediate)
            {
                cameraRig.localPosition = settings.cameraOffset;
                cameraRig.localRotation = Quaternion.Euler(settings.cameraRotation);
                mainCamera.fieldOfView = settings.fieldOfView;
            }
        }
        
        private ViewModeSettings GetSettingsForMode(ViewMode mode)
        {
            switch (mode)
            {
                case ViewMode.FirstPerson:
                    return fpsSettings;
                case ViewMode.ThirdPerson:
                    return tpsSettings;
                case ViewMode.Cover:
                    return coverSettings;
                default:
                    return tpsSettings;
            }
        }
        
        public void SetAiming(bool isAiming)
        {
            if (currentSettings != null && isAiming)
            {
                mainCamera.fieldOfView = currentSettings.aimFieldOfView;
            }
            else if (currentSettings != null)
            {
                mainCamera.fieldOfView = currentSettings.fieldOfView;
            }
        }
        
        public ViewMode GetCurrentMode() => currentMode;
        public bool IsTransitioning() => isTransitioning;
    }
}