using UnityEngine;
using asterivo.Unity60.Core.Data;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Stealth.Detection
{
    public class VisibilityCalculator : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private DetectionConfiguration config;
        
        [Header("Light Detection")]
        [SerializeField] private LayerMask obstacleLayerMask = -1;
        [SerializeField] private int lightSampleCount = 8;
        
        [Header("Debug")]
        [SerializeField] private bool debugDrawRays = false;
        
        public float CalculateVisibility(Transform target, Transform observer)
        {
            if (target == null || observer == null) return 0f;
            
            float distance = Vector3.Distance(target.position, observer.position);
            
            if (distance > config.maxDetectionRange)
                return 0f;
                
            float distanceFactor = config.distanceFalloffCurve.Evaluate(
                distance / config.maxDetectionRange);
                
            float angleFactor = CalculateAngleFactor(target, observer);
            
            float obstructionFactor = CalculateObstructionFactor(target, observer);
            
            float lightFactor = CalculateLightFactor(target.position);
            
            float visibility = distanceFactor * angleFactor * obstructionFactor * lightFactor;
            
            return Mathf.Clamp01(visibility);
        }
        
        private float CalculateAngleFactor(Transform target, Transform observer)
        {
            Vector3 directionToTarget = (target.position - observer.position).normalized;
            float angle = Vector3.Angle(observer.forward, directionToTarget);
            
            if (angle > config.fieldOfView / 2f)
                return 0f;
                
            return 1f - (angle / (config.fieldOfView / 2f));
        }
        
        private float CalculateObstructionFactor(Transform target, Transform observer)
        {
            Vector3[] checkPoints = GetTargetCheckPoints(target);
            int visiblePoints = 0;
            
            foreach (Vector3 point in checkPoints)
            {
                Vector3 direction = point - observer.position;
                float distance = direction.magnitude;
                
                if (!Physics.Raycast(observer.position, direction.normalized, 
                    distance, obstacleLayerMask))
                {
                    visiblePoints++;
                }
                
                if (debugDrawRays)
                {
                    Debug.DrawRay(observer.position, direction, 
                        visiblePoints > 0 ? Color.green : Color.red, 0.1f);
                }
            }
            
            return (float)visiblePoints / checkPoints.Length;
        }
        
        private Vector3[] GetTargetCheckPoints(Transform target)
        {
            Collider targetCollider = target.GetComponent<Collider>();
            if (targetCollider == null)
            {
                return new Vector3[] { target.position };
            }
            
            Bounds bounds = targetCollider.bounds;
            return new Vector3[]
            {
                bounds.center,
                bounds.center + Vector3.up * bounds.extents.y,
                bounds.center - Vector3.up * bounds.extents.y,
                bounds.center + Vector3.right * bounds.extents.x,
                bounds.center - Vector3.right * bounds.extents.x
            };
        }
        
        private float CalculateLightFactor(Vector3 position)
        {
            float totalLight = 0f;
            int validSamples = 0;
            
            for (int i = 0; i < lightSampleCount; i++)
            {
                float angle = (360f / lightSampleCount) * i;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                
                RaycastHit hit;
                if (Physics.Raycast(position + Vector3.up * 0.5f, direction, 
                    out hit, config.lightCheckDistance, obstacleLayerMask))
                {
                    float distance = hit.distance;
                    float lightContribution = 1f - (distance / config.lightCheckDistance);
                    totalLight += lightContribution;
                    validSamples++;
                }
            }
            
            if (validSamples > 0)
            {
                float averageLight = totalLight / validSamples;
                return config.lightVisibilityCurve.Evaluate(averageLight);
            }
            
            return config.defaultLightLevel;
        }
        
        public float CalculateNoiseDetection(Vector3 noiseSource, Transform observer, 
            float noiseLevel)
        {
            if (observer == null) return 0f;
            
            float distance = Vector3.Distance(noiseSource, observer.position);
            float maxNoiseRange = config.maxNoiseDetectionRange * noiseLevel;
            
            if (distance > maxNoiseRange)
                return 0f;
                
            float detectionLevel = 1f - (distance / maxNoiseRange);
            
            float obstructionFactor = 1f;
            if (Physics.Linecast(noiseSource, observer.position, obstacleLayerMask))
            {
                obstructionFactor = config.noiseThroughWallMultiplier;
            }
            
            return detectionLevel * obstructionFactor;
        }
        
        public bool IsInFieldOfView(Vector3 position, Transform observer)
        {
            if (observer == null) return false;
            
            Vector3 direction = (position - observer.position).normalized;
            float angle = Vector3.Angle(observer.forward, direction);
            
            return angle <= config.fieldOfView / 2f;
        }
        
        public bool HasLineOfSight(Vector3 position, Transform observer)
        {
            if (observer == null) return false;
            
            Vector3 direction = position - observer.position;
            return !Physics.Raycast(observer.position, direction.normalized, 
                direction.magnitude, obstacleLayerMask);
        }
    }
}