using UnityEngine;

namespace asterivo.Unity60.Core.Data
{
    [System.Serializable]
    public struct DetectionInfo
    {
        public float visibility;
        public float noiseLevel;
        public Vector3 lastKnownPosition;
        public float suspicionLevel;
        public float timeSinceLastSeen;
        public int detectorId;
        
        public DetectionInfo(float visibility, float noiseLevel, Vector3 lastKnownPosition)
        {
            this.visibility = visibility;
            this.noiseLevel = noiseLevel;
            this.lastKnownPosition = lastKnownPosition;
            this.suspicionLevel = 0f;
            this.timeSinceLastSeen = 0f;
            this.detectorId = -1;
        }
    }
    
    [System.Serializable]
    public enum AlertLevel
    {
        Unaware = 0,
        Suspicious = 1,
        Investigating = 2,
        Searching = 3,
        Alert = 4,
        Combat = 5
    }
    
    [System.Serializable]
    public struct AlertStateInfo
    {
        public AlertLevel currentLevel;
        public AlertLevel previousLevel;
        public float alertTimer;
        public Vector3 investigationPoint;
        public bool isGlobalAlert;
        
        public AlertStateInfo(AlertLevel level)
        {
            currentLevel = level;
            previousLevel = AlertLevel.Unaware;
            alertTimer = 0f;
            investigationPoint = Vector3.zero;
            isGlobalAlert = false;
        }
    }
    
    [System.Serializable]
    public enum MovementStance
    {
        Standing = 0,
        Crouching = 1,
        Prone = 2,
        Cover = 3
    }
    
    [System.Serializable]
    public struct StealthMovementInfo
    {
        public MovementStance stance;
        public float noiseRadius;
        public float visibilityModifier;
        public float moveSpeedModifier;
        public bool isInShadow;
        public float lightLevel;
    }
}