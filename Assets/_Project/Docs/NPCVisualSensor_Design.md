# NPCVisualSensor システム設計書

## アーキテクチャ概要

NPCVisualSensorは、既存の`VisibilityCalculator`を拡張し、`NPCAuditorySensor`と同等のイベント駆動型視覚検出システムとして実装します。

### システム構成図

```
┌─────────────────────────────────────────────────────────────────┐
│                    NPCVisualSensor System                       │
├─────────────────────────────────────────────────────────────────┤
│  NPCVisualSensor (MonoBehaviour)                                │
│  ├── VisualDetection Module     (Core Detection Logic)          │
│  ├── TargetTracking Module      (Target Management)             │
│  ├── AlertSystem Module         (Alert Level Management)        │
│  ├── Memory Module              (Short/Long Term Memory)        │
│  ├── Prediction Module          (Movement Prediction)           │
│  └── Event Module               (GameEvent Integration)         │
├─────────────────────────────────────────────────────────────────┤
│  Supporting Components                                          │
│  ├── VisualSensorSettings (ScriptableObject)                   │
│  ├── DetectedTarget (Data Structure)                           │
│  ├── VisualEventData (Event Data)                              │
│  └── TargetPrediction (Utility Class)                          │
├─────────────────────────────────────────────────────────────────┤
│  Integration Points                                             │
│  ├── AIStateMachine Interface                                  │
│  ├── NPCAuditorySensor Coordination                            │
│  ├── GameEvent System                                          │
│  └── VisibilityCalculator (Utility)                            │
└─────────────────────────────────────────────────────────────────┘
```

## クラス設計詳細

### 1. NPCVisualSensor (Core Class)

#### クラス構造
```csharp
public class NPCVisualSensor : MonoBehaviour, IGameEventListener<VisualEventData>
{
    #region Serialized Fields
    [Header("Configuration")]
    [SerializeField] private DetectionConfiguration detectionConfig;
    [SerializeField] private VisualSensorSettings visualSettings;
    
    [Header("Scan Parameters")]
    [SerializeField] private float scanFrequency = 10f;
    [SerializeField] private LayerMask targetLayerMask = -1;
    [SerializeField] private LayerMask obstacleLayerMask = -1;
    
    [Header("Detection Behavior")]
    [SerializeField] private float memoryDuration = 10f;
    [SerializeField] private int maxSimultaneousTargets = 3;
    [SerializeField] private bool enablePrediction = true;
    
    [Header("Events")]
    [SerializeField] private GameEvent onTargetSpotted;
    [SerializeField] private GameEvent onTargetLost;
    [SerializeField] private GameEvent onAlertLevelChanged;
    [SerializeField] private GameEvent onSuspiciousActivity;
    
    #region Private Fields
    // Target Management
    private List<DetectedTarget> detectedTargets;
    private List<DetectedTarget> targetMemory;
    private Dictionary<Transform, float> targetLastSeen;
    
    // State Management
    private AlertLevel currentAlertLevel;
    private float lastScanTime;
    private float lastAlertChangeTime;
    
    // Performance Optimization
    private Collider[] scanBuffer;
    private VisibilityCalculator visibilityCalculator;
    
    // Debug Information
    #if UNITY_EDITOR || DEVELOPMENT_BUILD
    [Header("Debug Info (Runtime)")]
    [SerializeField] private int activeTargetsCount;
    [SerializeField] private string currentAlertState;
    [SerializeField] private float highestDetectionLevel;
    #endif
    #endregion
}
```

#### 主要メソッド設計

##### スキャンシステム
```csharp
/// <summary>
/// 視界内の全目標をスキャンし、検出処理を実行
/// </summary>
private void PerformVisualScan()
{
    // 1. 範囲内の潜在的目標を取得
    GetTargetsInRange();
    
    // 2. 各目標の検出レベル計算
    foreach (var target in potentialTargets)
    {
        ProcessTarget(target);
    }
    
    // 3. 記憶と履歴の更新
    UpdateTargetMemory();
    
    // 4. 警戒レベルの再計算
    UpdateAlertLevel();
}

/// <summary>
/// 個別目標の検出処理
/// </summary>
private void ProcessTarget(Transform target)
{
    // 基本視認性計算
    float visibility = visibilityCalculator.CalculateVisibility(target, transform);
    
    // 予測位置での追加チェック
    if (enablePrediction && HasTargetHistory(target))
    {
        visibility = Mathf.Max(visibility, CheckPredictedPosition(target));
    }
    
    // 検出判定と状態更新
    if (visibility > visualSettings.detectionThreshold)
    {
        RegisterDetection(target, visibility);
    }
    else
    {
        HandleTargetLoss(target);
    }
}
```

##### 記憶システム
```csharp
/// <summary>
/// 目標の記憶情報を管理
/// </summary>
private void UpdateTargetMemory()
{
    // 短期記憶の更新
    UpdateShortTermMemory();
    
    // 長期記憶への移行
    TransferToLongTermMemory();
    
    // 古い記憶の削除
    CleanupExpiredMemories();
}

/// <summary>
/// 目標の予測位置を計算
/// </summary>
private Vector3 PredictTargetPosition(Transform target, float timeHorizon)
{
    if (!targetMovementHistory.ContainsKey(target)) return target.position;
    
    var history = targetMovementHistory[target];
    if (history.Count < 2) return target.position;
    
    // 移動ベクトルの計算
    Vector3 velocity = CalculateAverageVelocity(history);
    
    // 予測位置の算出
    return target.position + velocity * timeHorizon;
}
```

##### 警戒システム
```csharp
/// <summary>
/// 現在の検出状況に基づいて警戒レベルを更新
/// </summary>
private void UpdateAlertLevel()
{
    float maxDetectionLevel = GetMaxDetectionLevel();
    AlertLevel newLevel = CalculateAlertLevel(maxDetectionLevel);
    
    // レベル変更の処理
    if (newLevel != currentAlertLevel)
    {
        var previousLevel = currentAlertLevel;
        currentAlertLevel = newLevel;
        lastAlertChangeTime = Time.time;
        
        // イベント発行
        OnAlertLevelChanged(previousLevel, newLevel);
    }
    
    // 警戒レベルの自然減衰
    ProcessAlertDecay();
}

/// <summary>
/// 検出レベルから警戒レベルを算出
/// </summary>
private AlertLevel CalculateAlertLevel(float detectionLevel)
{
    if (detectionLevel >= visualSettings.alertThreshold) 
        return AlertLevel.Alert;
    if (detectionLevel >= visualSettings.investigationThreshold) 
        return AlertLevel.Investigating;
    if (detectionLevel >= visualSettings.suspicionThreshold) 
        return AlertLevel.Suspicious;
    return AlertLevel.Relaxed;
}
```

### 2. Supporting Classes

#### DetectedTarget構造体
```csharp
[System.Serializable]
public class DetectedTarget
{
    [Header("Basic Info")]
    public Transform target;
    public float detectionStrength;      // 0.0f - 1.0f
    public TargetThreatLevel threatLevel;
    
    [Header("Timing")]
    public float firstDetectedTime;
    public float lastSeenTime;
    public float continuousVisibilityTime;
    
    [Header("Positioning")]
    public Vector3 lastKnownPosition;
    public Vector3 predictedPosition;
    public Vector3 lastKnownVelocity;
    
    [Header("Status")]
    public bool isCurrentlyVisible;
    public bool isInvestigating;
    public float investigationStartTime;
    
    // Constructor
    public DetectedTarget(Transform targetTransform, float strength)
    {
        target = targetTransform;
        detectionStrength = strength;
        firstDetectedTime = Time.time;
        lastSeenTime = Time.time;
        lastKnownPosition = targetTransform.position;
        isCurrentlyVisible = true;
        threatLevel = CalculateThreatLevel();
    }
    
    // Utility Methods
    public float GetTimeSinceLastSeen() => Time.time - lastSeenTime;
    public bool IsMemoryExpired(float memoryDuration) => GetTimeSinceLastSeen() > memoryDuration;
    
    private TargetThreatLevel CalculateThreatLevel()
    {
        // プレイヤータグや距離などから脅威レベルを判定
        if (target.CompareTag("Player")) return TargetThreatLevel.High;
        if (target.CompareTag("Ally")) return TargetThreatLevel.Low;
        return TargetThreatLevel.Medium;
    }
}

public enum TargetThreatLevel
{
    Low,      // 友軍や中立
    Medium,   // 不明な目標
    High      // プレイヤーや敵対者
}
```

#### VisualSensorSettings (ScriptableObject)
```csharp
[CreateAssetMenu(menuName = "asterivo/AI/Visual Sensor Settings", fileName = "VisualSensorSettings")]
public class VisualSensorSettings : ScriptableObject
{
    [Header("Basic Parameters")]
    [SerializeField] private float baseScanRange = 25f;
    [SerializeField] private float scanFrequency = 10f;
    [SerializeField] private LayerMask defaultTargetLayers = -1;
    
    [Header("Detection Thresholds")]
    [Range(0f, 1f)] public float detectionThreshold = 0.1f;
    [Range(0f, 1f)] public float suspicionThreshold = 0.2f;
    [Range(0f, 1f)] public float investigationThreshold = 0.5f;
    [Range(0f, 1f)] public float alertThreshold = 0.8f;
    
    [Header("Memory Configuration")]
    public float shortTermMemoryDuration = 5f;
    public float longTermMemoryDuration = 30f;
    public int maxShortTermTargets = 5;
    public int maxLongTermTargets = 10;
    
    [Header("Alert System")]
    public float alertBuildUpSpeed = 2f;
    public float alertDecaySpeed = 1f;
    public float investigationTimeout = 10f;
    
    [Header("Prediction Settings")]
    public bool enableMovementPrediction = true;
    public float predictionTimeHorizon = 3f;
    public int movementHistorySize = 5;
    public AnimationCurve predictionAccuracyCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.5f);
    
    [Header("Performance")]
    [Range(1, 20)] public int targetsPerFrameLimit = 5;
    [Range(0.1f, 2f)] public float distanceLODMultiplier = 1f;
    
    // プロパティ
    public float BaseScanRange => baseScanRange;
    public float ScanFrequency => scanFrequency;
    public LayerMask DefaultTargetLayers => defaultTargetLayers;
}
```

#### VisualEventData構造体
```csharp
[System.Serializable]
public class VisualEventData
{
    public Transform observer;           // 観察者
    public Transform target;             // 目標
    public float detectionStrength;      // 検出強度
    public Vector3 detectionPosition;    // 検出位置
    public AlertLevel alertLevel;        // 警戒レベル
    public float timestamp;              // 検出時刻
    public VisualEventType eventType;    // イベント種類
    
    public VisualEventData(Transform obs, Transform tgt, float strength, AlertLevel level, VisualEventType type)
    {
        observer = obs;
        target = tgt;
        detectionStrength = strength;
        detectionPosition = tgt.position;
        alertLevel = level;
        timestamp = Time.time;
        eventType = type;
    }
}

public enum VisualEventType
{
    TargetSpotted,      // 目標発見
    TargetLost,         // 目標喪失  
    AlertLevelChanged,  // 警戒レベル変更
    SuspiciousActivity, // 疑わしい活動
    Investigation,      // 調査開始
    MemoryUpdated      // 記憶更新
}
```

## パフォーマンス最適化設計

### 1. フレーム分散処理
```csharp
/// <summary>
/// 重い処理をフレーム間で分散
/// </summary>
private IEnumerator DistributedScanCoroutine()
{
    while (enabled)
    {
        // 基本スキャン
        PerformBasicScan();
        yield return null; // 1フレーム待機
        
        // 詳細検出
        PerformDetailedDetection();
        yield return null;
        
        // 記憶処理
        UpdateMemorySystem();
        yield return null;
        
        // 予測処理
        UpdatePredictionSystem();
        
        // 次回スキャンまで待機
        yield return new WaitForSeconds(1f / visualSettings.ScanFrequency);
    }
}
```

### 2. LOD (Level of Detail) システム
```csharp
/// <summary>
/// 距離に応じた処理品質調整
/// </summary>
private float GetLODMultiplier(float distanceToPlayer)
{
    if (distanceToPlayer < visualSettings.highDetailRange) return 1f;
    if (distanceToPlayer < visualSettings.mediumDetailRange) return 0.7f;
    return 0.4f;
}
```

### 3. オブジェクトプールの活用
```csharp
// 頻繁に作成される一時オブジェクトのプール化
private static readonly ObjectPool<DetectedTarget> TargetPool = 
    new ObjectPool<DetectedTarget>(() => new DetectedTarget());
```

## 統合インターフェース設計

### NPCAuditorySensorとの連携
```csharp
/// <summary>
/// 聴覚センサーとの情報統合
/// </summary>
public void IntegrateAuditoryInformation(AudioEventData audioData)
{
    // 聴覚情報を視覚捜索に活用
    if (currentAlertLevel >= AlertLevel.Suspicious)
    {
        // 音源方向への視線集中
        FocusVisualAttention(audioData.worldPosition);
        
        // 調査行動の誘発
        TriggerInvestigation(audioData.worldPosition);
    }
}

/// <summary>
/// 統合センサー情報の提供
/// </summary>
public SensorFusionData GetIntegratedSensorData()
{
    return new SensorFusionData
    {
        visualTargets = GetCurrentTargets(),
        auditoryTargets = auditorySensor?.GetRecentSounds() ?? new List<DetectedSound>(),
        combinedAlertLevel = CalculateCombinedAlertLevel(),
        lastSignificantEvent = GetLastSignificantEvent()
    };
}
```

### AIStateMachineとの連携
```csharp
/// <summary>
/// AIステートマシン向けインターフェース
/// </summary>
public interface IVisualSensorForAI
{
    AlertLevel GetCurrentAlertLevel();
    List<DetectedTarget> GetActiveTargets();
    Vector3 GetLastKnownPlayerPosition();
    bool HasActiveInvestigation();
    float GetTimeInCurrentAlertLevel();
    
    // 状態変更通知
    void OnStateTransition(AIStateType newState);
    void OnPatrolPointReached(Vector3 position);
    void OnCombatStateEntered();
}
```

## デバッグ・可視化システム

### エディター拡張
```csharp
#if UNITY_EDITOR
/// <summary>
/// Scene Viewでの可視化
/// </summary>
private void OnDrawGizmosSelected()
{
    if (!Application.isPlaying) return;
    
    // 視界範囲の表示
    DrawFieldOfView();
    
    // 検出された目標の表示
    DrawDetectedTargets();
    
    // 記憶位置の表示
    DrawMemoryPositions();
    
    // 予測軌道の表示
    DrawPredictionPaths();
}

/// <summary>
/// カスタムエディターウィンドウ
/// </summary>
[CustomEditor(typeof(NPCVisualSensor))]
public class NPCVisualSensorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        if (Application.isPlaying)
        {
            DrawRuntimeDebugInfo();
        }
    }
    
    private void DrawRuntimeDebugInfo()
    {
        var sensor = (NPCVisualSensor)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime Debug Info", EditorStyles.boldLabel);
        
        EditorGUILayout.LabelField($"Alert Level: {sensor.GetCurrentAlertLevel()}");
        EditorGUILayout.LabelField($"Active Targets: {sensor.GetActiveTargetCount()}");
        EditorGUILayout.LabelField($"Memory Entries: {sensor.GetMemoryCount()}");
        
        // リアルタイム更新
        if (GUILayout.Button("Force Scan"))
        {
            sensor.ForceVisualScan();
        }
    }
}
#endif
```

## テスト戦略

### 1. 単体テスト設計
```csharp
[TestFixture]
public class NPCVisualSensorTests
{
    [Test]
    public void DetectionThreshold_WhenBelowThreshold_ShouldNotDetect()
    {
        // Arrange
        var sensor = CreateTestSensor();
        var target = CreateTestTarget();
        
        // Act
        sensor.SetVisibilityOverride(0.05f); // Below threshold
        sensor.PerformVisualScan();
        
        // Assert
        Assert.IsFalse(sensor.HasDetectedTarget(target));
    }
    
    [Test]
    public void AlertLevel_WhenTargetLost_ShouldDecayOverTime()
    {
        // Arrange & Act & Assert
        // 時間経過による警戒レベル減衰のテスト
    }
}
```

### 2. 統合テスト設計
```csharp
[TestFixture]
public class VisualSensorIntegrationTests
{
    [Test]
    public void VisualAndAuditory_WhenBothActive_ShouldCoordinateCorrectly()
    {
        // 視覚・聴覚センサーの連携動作テスト
    }
    
    [Test]
    public void AIStateMachine_WhenAlertLevelChanges_ShouldTransitionCorrectly()
    {
        // AIステートマシンとの連携テスト
    }
}
```

## 実装フェーズ計画

### Phase 1: Core Implementation
1. NPCVisualSensorクラスの基本構造実装
2. DetectedTarget構造体とデータ管理
3. 基本的な検出ロジックの実装

### Phase 2: Advanced Features
1. 記憶システムの実装
2. 予測システムの追加
3. 警戒レベル管理の実装

### Phase 3: Integration
1. NPCAuditorySensorとの連携
2. AIStateMachineとの統合
3. GameEventシステムとの接続

### Phase 4: Optimization & Polish
1. パフォーマンス最適化
2. デバッグシステムの充実
3. ドキュメント整備と単体テスト