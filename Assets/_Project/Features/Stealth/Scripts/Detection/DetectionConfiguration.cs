using UnityEngine;

namespace asterivo.Unity60.Stealth.Detection
{
    [CreateAssetMenu(menuName = "asterivo/Stealth/Detection Config", fileName = "DetectionConfig")]
    public class DetectionConfiguration : ScriptableObject
    {
        [Header("Detection Ranges")]
        public float maxDetectionRange = 30f;
        public float maxNoiseDetectionRange = 20f;
        public float instantDetectionRange = 3f;
        
        [Header("Field of View")]
        public float fieldOfView = 110f;
        public float peripheralVisionMultiplier = 0.5f;
        
        [Header("Light Detection")]
        public float lightCheckDistance = 10f;
        public float defaultLightLevel = 0.5f;
        public AnimationCurve lightVisibilityCurve = AnimationCurve.Linear(0f, 0.3f, 1f, 1f);
        
        [Header("Distance Falloff")]
        public AnimationCurve distanceFalloffCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        
        [Header("Noise Detection")]
        public float noiseThroughWallMultiplier = 0.3f;
        public float baseHearingRange = 10f;
        
        [Header("Detection Speed")]
        public float detectionBuildUpSpeed = 1f;
        public float detectionDecaySpeed = 0.5f;
        public float suspicionThreshold = 0.3f;
        public float alertThreshold = 0.7f;
        
        [Header("Movement Modifiers")]
        public float runningVisibilityMultiplier = 1.5f;
        public float crouchingVisibilityMultiplier = 0.6f;
        public float proneVisibilityMultiplier = 0.3f;
        public float stillnessBonus = 0.8f;
        
        private void Reset()
        {
            maxDetectionRange = 30f;
            maxNoiseDetectionRange = 20f;
            instantDetectionRange = 3f;
            fieldOfView = 110f;
            peripheralVisionMultiplier = 0.5f;
            lightCheckDistance = 10f;
            defaultLightLevel = 0.5f;
            noiseThroughWallMultiplier = 0.3f;
            baseHearingRange = 10f;
            detectionBuildUpSpeed = 1f;
            detectionDecaySpeed = 0.5f;
            suspicionThreshold = 0.3f;
            alertThreshold = 0.7f;
            
            lightVisibilityCurve = AnimationCurve.Linear(0f, 0.3f, 1f, 1f);
            distanceFalloffCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        }
    }
}