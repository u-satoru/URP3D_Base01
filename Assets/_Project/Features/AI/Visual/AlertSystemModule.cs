using UnityEngine;
using System;
using System.Collections;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Data;
using asterivo.Unity60.Features.AI.States;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.AI.Visual
{
    /// <summary>
    /// 4段階警戒レベル管理システム
    /// Unaware → Suspicious → Investigating → Alert の自動遷移制御を実装
    /// </summary>
    [System.Serializable]
    public class AlertSystemModule
    {
        #region Inspector Fields
        
        [BoxGroup("Alert Levels")]
        [LabelText("Alert Level Transition Settings")]
        [SerializeField] private AlertLevelSettings alertSettings;
        
        [BoxGroup("Thresholds")]
        [PropertyRange(0f, 1f)]
        [LabelText("Suspicious Threshold")]
        [SerializeField] private float suspiciousThreshold = 0.2f;
        
        [BoxGroup("Thresholds")]
        [PropertyRange(0f, 1f)]
        [LabelText("Investigating Threshold")]
        [SerializeField] private float investigatingThreshold = 0.5f;
        
        [BoxGroup("Thresholds")]
        [PropertyRange(0f, 1f)]
        [LabelText("Alert Threshold")]
        [SerializeField] private float alertThreshold = 0.8f;
        
        [BoxGroup("Decay Settings")]
        [PropertyRange(0.1f, 2f)]
        [LabelText("Alert Decay Rate")]
        [SuffixLabel("/s")]
        [SerializeField] private float alertDecayRate = 0.3f;
        
        [BoxGroup("Decay Settings")]
        [PropertyRange(1f, 10f)]
        [LabelText("Calm Down Time")]
        [SuffixLabel("s")]
        [SerializeField] private float calmDownTime = 5f;
        
        [BoxGroup("Timers")]
        [PropertyRange(2f, 15f)]
        [LabelText("Investigation Time")]
        [SuffixLabel("s")]
        [SerializeField] private float investigationTime = 8f;
        
        [BoxGroup("Timers")]
        [PropertyRange(5f, 30f)]
        [LabelText("Search Time")]
        [SuffixLabel("s")]
        [SerializeField] private float searchTime = 15f;
        
        #endregion
        
        #region Runtime State
        
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Current Alert Level")]
        private AlertLevel currentAlertLevel = AlertLevel.Unaware;
        
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Previous Alert Level")]
        private AlertLevel previousAlertLevel = AlertLevel.Unaware;
        
        [ReadOnly]
        [ShowInInspector]
        [ProgressBar(0f, 1f, ColorGetter = "GetAlertIntensityColor")]
        [LabelText("Alert Intensity")]
        private float alertIntensity = 0f;
        
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Time in Current Level")]
        private float timeInCurrentLevel = 0f;
        
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Time Since Last Detection")]
        private float timeSinceLastDetection = 0f;
        
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Is Auto Decaying")]
        private bool isAutoDecaying = false;
        
        #endregion
        
        #region Events
        
        public event Action<AlertLevel, AlertLevel> OnAlertLevelChanged;
        public event Action<AlertStateInfo> OnAlertStateChanged;
        public event Action<float> OnAlertIntensityChanged;
        
        #endregion
        
        #region Properties
        
        public AlertLevel CurrentAlertLevel => currentAlertLevel;
        public AlertLevel PreviousAlertLevel => previousAlertLevel;
        public float AlertIntensity => alertIntensity;
                public float AlertTimer => timeInCurrentLevel;
        public float AlertDecayRate => alertDecayRate;
public float TimeInCurrentLevel => timeInCurrentLevel;
        public bool IsInAlertState => currentAlertLevel >= AlertLevel.Alert;
        public bool IsInvestigating => currentAlertLevel == AlertLevel.Investigating;
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// AlertSystemModuleの初期化
        /// </summary>
        public void Initialize(AlertLevelSettings settings = null)
        {
            if (settings != null)
                alertSettings = settings;
            
            currentAlertLevel = AlertLevel.Unaware;
            previousAlertLevel = AlertLevel.Unaware;
            alertIntensity = 0f;
            timeInCurrentLevel = 0f;
            timeSinceLastDetection = 0f;
            isAutoDecaying = false;
            
            // デフォルト設定の適用
            if (alertSettings == null)
            {
                alertSettings = CreateDefaultSettings();
            }
            
            Debug.Log("[AlertSystem] Alert System Module initialized");
        }
        
        private AlertLevelSettings CreateDefaultSettings()
        {
            return new AlertLevelSettings
            {
                unawareToSuspiciousThreshold = 0.2f,
                suspiciousToInvestigatingThreshold = 0.5f,
                investigatingToAlertThreshold = 0.8f,
                decayRate = 0.3f,
                calmDownTime = 5f,
                investigationTime = 8f,
                searchTime = 15f
            };
        }
        
        #endregion
        
        #region Update Methods
        
        /// <summary>
        /// 警戒システムの更新（毎フレーム呼び出し）
        /// </summary>
        public void UpdateAlertSystem(float deltaTime)
        {
            timeInCurrentLevel += deltaTime;
            timeSinceLastDetection += deltaTime;
            
            // 自動減衰処理
            if (isAutoDecaying)
            {
                ProcessAutoDecay(deltaTime);
            }
            
            // 警戒レベル固有の処理
            ProcessCurrentAlertLevel(deltaTime);
        }
        
        private void ProcessAutoDecay(float deltaTime)
        {
            if (timeSinceLastDetection >= calmDownTime)
            {
                DecayAlertIntensity(deltaTime);
                
                // 警戒レベルの自動降下チェック
                CheckForAlertLevelDecrease();
            }
        }
        
        private void DecayAlertIntensity(float deltaTime)
        {
            if (alertIntensity > 0f)
            {
                alertIntensity -= alertDecayRate * deltaTime;
                alertIntensity = Mathf.Max(0f, alertIntensity);
                OnAlertIntensityChanged?.Invoke(alertIntensity);
            }
        }
        
        private void ProcessCurrentAlertLevel(float deltaTime)
        {
            switch (currentAlertLevel)
            {
                case AlertLevel.Investigating:
                    ProcessInvestigatingState(deltaTime);
                    break;
                    
                case AlertLevel.Searching:
                    ProcessSearchingState(deltaTime);
                    break;
                    
                case AlertLevel.Alert:
                    ProcessAlertState(deltaTime);
                    break;
            }
        }
        
        private void ProcessInvestigatingState(float deltaTime)
        {
            if (timeInCurrentLevel >= investigationTime)
            {
                // 調査時間終了 → Searching状態へ
                TransitionToSearching();
            }
        }
        
        private void ProcessSearchingState(float deltaTime)
        {
            if (timeInCurrentLevel >= searchTime)
            {
                // 索敵時間終了 → 警戒レベル低下開始
                StartAutoDecay();
            }
        }
        
        private void ProcessAlertState(float deltaTime)
        {
            // Alert状態では手動でしか警戒レベルは下がらない
            // または非常に長い時間（戦闘終了後など）
        }
        
        #endregion
        
        #region Alert Level Management
        
        /// <summary>
        /// 検出スコアに基づく警戒レベル更新
        /// </summary>
        public void UpdateAlertLevel(float detectionScore, Vector3? lastKnownPosition = null)
        {
            timeSinceLastDetection = 0f;
            isAutoDecaying = false;
            
            // 検出スコアを警戒強度に加算
            float intensityIncrease = detectionScore * Time.deltaTime;
            alertIntensity = Mathf.Clamp01(alertIntensity + intensityIncrease);
            OnAlertIntensityChanged?.Invoke(alertIntensity);
            
            // 警戒レベルの遷移チェック
            AlertLevel newLevel = CalculateAlertLevelFromIntensity();
            
            if (newLevel != currentAlertLevel)
            {
                TransitionToAlertLevel(newLevel, lastKnownPosition);
            }
        }
        
        private AlertLevel CalculateAlertLevelFromIntensity()
        {
            if (alertIntensity >= alertThreshold)
                return AlertLevel.Alert;
            else if (alertIntensity >= investigatingThreshold)
                return AlertLevel.Investigating;
            else if (alertIntensity >= suspiciousThreshold)
                return AlertLevel.Suspicious;
            else
                return AlertLevel.Unaware;
        }
        
        /// <summary>
        /// 指定された警戒レベルへの遷移
        /// </summary>
        public void TransitionToAlertLevel(AlertLevel newLevel, Vector3? investigationPoint = null)
        {
            if (currentAlertLevel == newLevel) return;
            
            previousAlertLevel = currentAlertLevel;
            currentAlertLevel = newLevel;
            timeInCurrentLevel = 0f;
            
            // 状態変更の通知
            OnAlertLevelChanged?.Invoke(previousAlertLevel, currentAlertLevel);
            
            AlertStateInfo stateInfo = new AlertStateInfo(currentAlertLevel)
            {
                previousLevel = previousAlertLevel,
                alertTimer = timeInCurrentLevel,
                investigationPoint = investigationPoint ?? Vector3.zero,
                isGlobalAlert = currentAlertLevel >= AlertLevel.Alert
            };
            
            OnAlertStateChanged?.Invoke(stateInfo);
            
            Debug.Log($"[AlertSystem] Alert Level changed: {previousAlertLevel} → {currentAlertLevel} (Intensity: {alertIntensity:F2})");
        }
        
        private void CheckForAlertLevelDecrease()
        {
            AlertLevel newLevel = CalculateAlertLevelFromIntensity();
            
            if (newLevel < currentAlertLevel)
            {
                TransitionToAlertLevel(newLevel);
            }
        }
        
        #endregion
        
        #region State Control
        
        /// <summary>
        /// 自動減衰開始
        /// </summary>
        public void StartAutoDecay()
        {
            isAutoDecaying = true;
            Debug.Log("[AlertSystem] Auto decay started");
        }
        
        /// <summary>
        /// 自動減衰停止
        /// </summary>
        public void StopAutoDecay()
        {
            isAutoDecaying = false;
            timeSinceLastDetection = 0f;
        }
        
        /// <summary>
        /// 警戒状態リセット
        /// </summary>
        public void ResetAlertState()
        {
            TransitionToAlertLevel(AlertLevel.Unaware);
            alertIntensity = 0f;
            isAutoDecaying = false;
            timeSinceLastDetection = 0f;
            OnAlertIntensityChanged?.Invoke(alertIntensity);
            Debug.Log("[AlertSystem] Alert state reset");
        }
        
        /// <summary>
        /// 最高警戒状態に移行
        /// </summary>
        public void TriggerMaxAlert(Vector3 alertPosition)
        {
            alertIntensity = 1f;
            TransitionToAlertLevel(AlertLevel.Alert, alertPosition);
            OnAlertIntensityChanged?.Invoke(alertIntensity);
            Debug.Log("[AlertSystem] Max alert triggered");
        }
        
        private void TransitionToSearching()
        {
            TransitionToAlertLevel(AlertLevel.Searching);
        }
        
        #endregion
        
        #region Odin Inspector Support
        
        private Color GetAlertIntensityColor()
        {
            return alertIntensity switch
            {
                < 0.2f => Color.green,
                < 0.5f => Color.yellow,
                < 0.8f => new Color(1f, 0.5f, 0f), // Orange
                _ => Color.red
            };
        }
        
        #endregion
    }
    
    #region Support Classes
    
    /// <summary>
    /// 警戒レベル設定データ
    /// </summary>
    [System.Serializable]
    public class AlertLevelSettings
    {
        [LabelText("Unaware → Suspicious")]
        public float unawareToSuspiciousThreshold = 0.2f;
        
        [LabelText("Suspicious → Investigating")]
        public float suspiciousToInvestigatingThreshold = 0.5f;
        
        [LabelText("Investigating → Alert")]
        public float investigatingToAlertThreshold = 0.8f;
        
        [LabelText("Decay Rate")]
        public float decayRate = 0.3f;
        
        [LabelText("Calm Down Time")]
        public float calmDownTime = 5f;
        
        [LabelText("Investigation Time")]
        public float investigationTime = 8f;
        
        [LabelText("Search Time")]
        public float searchTime = 15f;
    }
    
    #endregion
}