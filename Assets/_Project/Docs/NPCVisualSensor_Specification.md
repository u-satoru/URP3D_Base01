# NPCVisualSensor システム仕様書

## 概要

NPCVisualSensorは、ステルスゲームにおけるAIの視覚検出システムを提供するコンポーネントです。現在の`VisibilityCalculator`を拡張し、`NPCAuditorySensor`と同様のイベント駆動型アーキテクチャで実装します。

## 設計目標

### 1. アーキテクチャの統一性
- `NPCAuditorySensor`と同様のイベント駆動設計
- ScriptableObjectベースの設定管理
- GameEventシステムとの連携

### 2. 高度な視覚検出機能
- 継続的な視界スキャンと目標追跡
- 段階的な警戒レベル管理
- 視認対象の記憶と履歴システム
- 複数目標の同時追跡

### 3. ゲームプレイの向上
- プレイヤーの戦術的選択肢の拡大
- 予測可能で一貫した AI 行動
- デバッグとバランス調整の容易さ

## 機能要件

### Core機能

#### 1. 視覚検出システム
- **継続的スキャン**: Update()での定期的な視界チェック
- **多重判定**: 距離、角度、遮蔽、光量の総合評価
- **段階的検出**: 疑念 → 調査 → 警戒の段階的状態遷移
- **記憶システム**: 視界から外れた目標の位置記憶

#### 2. 目標管理システム
- **DetectedTarget構造体**: 検出された目標の詳細情報
- **履歴管理**: 最大同時追跡数の制限と古いデータの削除
- **優先度付け**: 脅威レベルによる目標の優先順位付け

#### 3. 警戒レベルシステム
```
AlertLevel.Relaxed      // 通常状態
AlertLevel.Suspicious   // 疑念（何かがいるかも）
AlertLevel.Investigating // 調査（確認中）
AlertLevel.Alert        // 警戒（目標確認済み）
```

#### 4. イベント通知システム
- `onTargetSpotted`: 目標を初めて視認した時
- `onTargetLost`: 目標を見失った時
- `onAlertLevelChanged`: 警戒レベル変更時
- `onSuspiciousActivity`: 不審な動きを検知時

### Advanced機能

#### 1. 予測システム
- **移動予測**: 目標の移動方向と速度から位置予測
- **隠蔽場所推定**: 見失った目標の潜伏可能位置計算
- **パトロール最適化**: 効率的な索敵経路計算

#### 2. 協調検出システム
- **情報共有**: 他のNPCとの視覚情報共有
- **連携捜索**: 複数NPCによる協調的な目標捜索
- **警戒伝播**: 近隣NPCへの警戒状態伝播

#### 3. 学習システム
- **行動パターン学習**: プレイヤーの行動傾向の記録
- **隠蔽場所記憶**: よく使われる隠蔽場所の記憶
- **適応的警戒**: 過去の経験に基づく警戒レベル調整

## 技術仕様

### クラス設計

#### NPCVisualSensor : MonoBehaviour, IGameEventListener
```csharp
public class NPCVisualSensor : MonoBehaviour, IGameEventListener<VisualEventData>
{
    // Configuration
    [SerializeField] private DetectionConfiguration config;
    [SerializeField] private VisualSensorSettings visualSettings;
    
    // Detection Parameters
    [SerializeField] private float scanFrequency = 10f; // Hz
    [SerializeField] private float memoryDuration = 10f; // seconds
    [SerializeField] private int maxSimultaneousTargets = 3;
    
    // Events
    [SerializeField] private GameEvent onTargetSpotted;
    [SerializeField] private GameEvent onTargetLost;
    [SerializeField] private GameEvent onAlertLevelChanged;
    
    // State Management
    private List<DetectedTarget> detectedTargets;
    private List<DetectedTarget> targetMemory;
    private AlertLevel currentAlertLevel;
    private float lastScanTime;
}
```

#### DetectedTarget構造体
```csharp
[System.Serializable]
public class DetectedTarget
{
    public Transform target;
    public float detectionStrength;  // 0.0f - 1.0f
    public float firstSeenTime;
    public float lastSeenTime;
    public Vector3 lastKnownPosition;
    public Vector3 predictedPosition;
    public TargetThreatLevel threatLevel;
    public bool isCurrentlyVisible;
}
```

#### VisualSensorSettings : ScriptableObject
```csharp
[CreateAssetMenu(menuName = "asterivo/AI/Visual Sensor Settings")]
public class VisualSensorSettings : ScriptableObject
{
    [Header("Scan Parameters")]
    public float baseScanRange = 25f;
    public float scanFrequency = 10f;
    public LayerMask targetLayerMask;
    
    [Header("Detection Thresholds")]
    public float suspicionThreshold = 0.2f;
    public float investigationThreshold = 0.5f;
    public float alertThreshold = 0.8f;
    
    [Header("Memory Settings")]
    public float shortTermMemory = 5f;
    public float longTermMemory = 30f;
    public int maxTargetHistory = 10;
    
    [Header("Prediction")]
    public bool enablePrediction = true;
    public float predictionTimeHorizon = 2f;
    public AnimationCurve predictionAccuracy;
}
```

### パフォーマンス仕様

#### 最適化要件
- **フレームレート影響**: 1フレームあたり0.1ms以下
- **メモリ使用量**: NPCあたり最大5KB
- **同時処理数**: 50体のNPC同時稼働対応

#### スケーラビリティ
- **LOD対応**: 距離に応じた検出精度の段階調整
- **Update頻度調整**: プレイヤーからの距離による更新頻度制御
- **早期カリング**: 明らかに視界外の目標の処理スキップ

## ゲームプレイ仕様

### プレイヤー体験

#### 1. 段階的緊張感
- **疑念段階**: NPCが首をかしげる、歩き方が変わる
- **調査段階**: 近づいてくる、周囲を見回す
- **警戒段階**: 積極的な捜索、他NPCとの連携

#### 2. 予測可能性
- **視線表示**: デバッグ時のGizmo表示
- **警戒レベルUI**: 現在の検出状況の可視化
- **一貫した反応**: 同じ状況では同じ反応を示す

#### 3. 戦術的深度
- **光と影の活用**: 照明を考慮した隠蔽戦術
- **タイミング戦略**: NPCの視線パターンを読んだ移動
- **環境利用**: 遮蔽物を活用した視線回避

### バランス設計

#### 難易度調整パラメータ
- `scanFrequency`: 検出頻度（易しい: 5Hz, 難しい: 20Hz）
- `detectionThresholds`: 警戒移行の閾値
- `memoryDuration`: 記憶保持時間
- `predictionAccuracy`: 移動予測の精度

## 品質要件

### 信頼性
- **エラー処理**: null参照や範囲外アクセスの防止
- **状態一貫性**: 警戒レベルと行動の整合性保証
- **メモリ管理**: リークの防止と適切なクリーンアップ

### 保守性
- **設定の外部化**: ScriptableObjectによるパラメータ管理
- **デバッグ支援**: 詳細なログとビジュアル表示
- **モジュール化**: 他システムとの疎結合

### 拡張性
- **センサー統合**: 聴覚センサーとの連携インターフェース
- **AIステート連携**: 既存ステートマシンとの互換性
- **カスタマイゼーション**: ゲーム固有の要件への対応

## 実装制約

### Unity固有の制約
- **MonoBehaviour継承**: Unity標準のライフサイクル準拠
- **SerializeField**: エディター上での設定可能性
- **レイヤーマスク**: Unityの物理システムとの連携

### プロジェクト制約
- **DOTS非対応**: 既存MonoBehaviourベース設計の維持
- **URP対応**: Universal Render Pipelineでの動作保証
- **パフォーマンス**: モバイル環境での動作要件

## 検証要件

### 単体テスト
- 各検出アルゴリズムの精度検証
- 警戒レベル遷移の正確性テスト
- メモリ管理の正常性確認

### 統合テスト
- AIステートマシンとの連携動作
- 複数NPCでの同時稼働テスト
- パフォーマンス測定とボトルネック特定

### ゲームプレイテスト
- プレイヤー体験の質的評価
- バランス調整の効果測定
- エッジケースでの動作確認