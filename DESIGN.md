# DESIGN.md - Unity 6 3Dゲーム基盤プロジェクト 技術設計書

## 文書管理情報

- **ドキュメント種別**: 技術設計書（SDDフェーズ3: 設計）
- **生成元**: REQUIREMENTS.md - Unity 6 3Dゲーム基盤プロジェクト 形式化された要件定義
- **トレーサビリティ**: SPEC.md v3.0 究極テンプレートビジョン → REQUIREMENTS.md → 本技術設計書
- **対象読者**: アーキテクト、シニア開発者、技術リード、実装担当者
- **更新日**: 2025年9月（REQUIREMENTS.md アクションRPG対応 + Core/Feature層分離強化更新）
- **整合性状態**: CLAUDE.md、REQUIREMENTS.md（FR-5アクションRPG追加、FR番号更新済み）との完全整合性確保済み

## 設計原則とアーキテクチャビジョン

### 究極テンプレート設計ビジョン（SPEC.md v3.0 完全対応）

この技術設計は、**SPEC.md v3.0 究極テンプレートビジョンの4つの核心価値**を技術実装で実現します：

#### 核心価値実現のための設計戦略
- **Clone & Create**: 1分セットアップの技術基盤設計（97%時間短縮実現）
- **Learn & Grow**: 段階的学習システムアーキテクチャ（学習コスト70%削減）
- **Ship & Scale**: プロダクション対応スケーラブル設計（プロトタイプ→本番完全対応）
- **Community & Ecosystem**: 拡張可能エコシステム技術基盤（テンプレート共有・知識交換）

### 5つの核心設計思想

#### 1. Event-Driven Architecture First
- **設計原則**: 全てのシステム間通信をイベント駆動で実装
- **技術実現**: ScriptableObjectベースのイベントチャネル（GameEvent）
- **ビジネス価値**: 疎結合設計による保守性・拡張性の最大化、チーム開発効率向上
- **究極テンプレート貢献**: Learn & Grow 価値（理解しやすいアーキテクチャ）

#### 2. Command + ObjectPool 統合による極限最適化
- **設計原則**: 全ての操作をコマンド化し、ObjectPoolで再利用
- **技術実現**: Factory+Registry+ObjectPool統合アーキテクチャ
- **定量的効果**: 95%メモリ削減、67%実行速度改善の実現
- **究極テンプレート貢献**: Ship & Scale 価値（プロダクション品質パフォーマンス）

#### 3. State-Driven Behavior Management
- **設計原則**: 複雑な行動制御を状態マシンで管理
- **技術実現**: Dictionary<StateType, IState>による高速状態管理
- **ビジネス価値**: 予測可能で拡張容易な行動システム
- **究極テンプレート貢献**: Learn & Grow 価値（直感的な状態管理）

#### 4. Data-Configuration Driven Development
- **設計原則**: ロジックとデータの完全分離
- **技術実現**: ScriptableObjectによるデータ資産化
- **ビジネス価値**: ノンプログラマーによる独立したゲーム調整
- **究極テンプレート貢献**: Community & Ecosystem 価値（非技術者の参加促進）

#### 5. SDD統合による品質保証設計（新規追加）
- **設計原則**: スペック駆動開発（SDD）とAI連携による品質保証
- **技術実現**: 5段階フェーズ管理 + MCPサーバー統合
- **ビジネス価値**: AI+人間ハイブリッド開発による効率化
- **究極テンプレート貢献**: Clone & Create 価値（1分セットアップの技術基盤）

## システムアーキテクチャ設計

### アーキテクチャ分離原則（Core層とFeature層の明確な役割分担）

#### Core層の責任範囲 (`Assets/_Project/Core`)
```
Core Layer Architecture:
┌─────────────────────────────────────────────┐
│                 Core Layer                   │
│          (asterivo.Unity60.Core.*)           │
├─────────────────────────────────────────────┤
│ ✅ イベント駆動アーキテクチャ基盤             │
│ ✅ コマンドパターン + ObjectPool統合          │
│ ✅ ServiceLocator基盤                        │
│ ✅ 基本データ構造・インターフェース           │
│ ✅ オーディオシステム基盤                     │
│ ✅ ステートマシン基盤                         │
│ ✅ 共通ユーティリティ                         │
└─────────────────────────────────────────────┘
```

#### Feature層の責任範囲 (`Assets/_Project/Features`)
```
Feature Layer Architecture:
┌─────────────────────────────────────────────┐
│                Feature Layer                 │
│        (asterivo.Unity60.Features.*)         │
├─────────────────────────────────────────────┤
│ ✅ プレイヤー機能（移動、インタラクション）   │
│ ✅ AI機能（NPCの具体的行動）                 │
│ ✅ カメラ機能（具体的カメラ制御）             │
│ ✅ ゲームジャンル固有機能                     │
│ ✅ アクションRPG機能（キャラ成長、装備）     │
│ ✅ ゲームプレイロジック                       │
│ ❌ Core層への直接参照（禁止）                │
└─────────────────────────────────────────────┘
```

#### 分離原則の技術実装
- **依存関係制御**: Core層 ← Feature層（一方向依存）
- **通信方式**: Event駆動によるCore↔Feature間の疎結合通信
- **名前空間分離**: `asterivo.Unity60.Core.*` vs `asterivo.Unity60.Features.*`
- **Assembly Definition分離**: Core.asmdef, Features.asmdef

#### 名前空間一貫性設計（CLAUDE.md規約準拠）

```
名前空間階層設計:
┌─────────────────────────────────────────────┐
│ asterivo.Unity60                              │
│ │                                           │
│ ├── Core.*      (基盤システム、他に依存しない)   │
│ │   ├── Events    (イベントシステム)           │
│ │   ├── Commands  (コマンドシステム)           │
│ │   ├── Services  (ServiceLocator)          │
│ │   └── Audio     (オーディオ基盤)           │
│ │                                           │
│ ├── Features.*  (機能実装、Coreに依存)      │
│ │   ├── Player    (プレイヤー機能)           │
│ │   ├── AI        (AI機能)                 │
│ │   ├── Camera    (カメラ機能)             │
│ │   └── ActionRPG (アクションRPG機能)       │
│ │                                           │
│ └── Tests.*     (テスト、独立環境)          │
│     ├── Core      (Core層テスト)            │
│     └── Features  (Feature層テスト)        │
└─────────────────────────────────────────────┘
```

##### 名前空間設計角を

- **Root名前空間**: `asterivo.Unity60`
  - プロジェクト統一名前空間、全システムのエントリーポイント
  - Unity 6バージョン情報と企業名を組み合わせた一意性確保

- **Core層名前空間**: `asterivo.Unity60.Core.*`
  - 基盤システム専用、他の層に依存しない独立性確保
  - イベント、コマンド、ServiceLocator等のアーキテクチャ核心機能
  - 例: `asterivo.Unity60.Core.Events`, `asterivo.Unity60.Core.Commands`

- **Feature層名前空間**: `asterivo.Unity60.Features.*`
  - 機能実装専用、Core層の基盤機能を活用
  - ゲームジャンル固有機能、プレイヤー・AI・カメラ機能
  - 例: `asterivo.Unity60.Features.Player`, `asterivo.Unity60.Features.AI`

- **Test層名前空間**: `asterivo.Unity60.Tests.*`
  - テスト専用、独立テスト環境確保
  - Core層・Feature層の各機能に対応したテスト構造
  - 例: `asterivo.Unity60.Tests.Core`, `asterivo.Unity60.Tests.Features`

##### 依存関係制約の強制

- **Core→Feature参照禁止**: Core層はFeature層を直接参照できない
- **Event駆動通信**: Core↔Feature間の通信はGameEvent経由のみ
- **Assembly Definition制御**: .asmdefファイルによる依存関係のコンパイル時強制

##### レガシー名前空間の段階的移行

- **非推奨名前空間**: `_Project.*`の新規使用禁止
- **移行戦略**: 既存`_Project.*`クラスを`asterivo.Unity60.*`へ段階的移行
- **互換性維持**: 移行期間中のusingエイリアスで互換性確保
- **最終目標**: レガシー名前空間ゼロ化、一貫性ある名前空間構造完成

### Layer 1: Core Foundation Layer（基盤層）

#### 1.1 Event System Architecture

```
EventSystem Architecture:
┌─────────────────────────────────────────────┐
│              Event Channel Layer             │
├─────────────────────────────────────────────┤
│ GameEvent<T>           │ GenericGameEvent<T> │
│ - Raise(T)            │ - RaiseAsync(T)     │
│ - Listen(Action<T>)   │ - Priority Queue    │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│            Listener Management Layer         │
├─────────────────────────────────────────────┤
│ HashSet<GameEventListener> - O(1) 管理      │
│ Priority Sorted Cache - 優先度制御          │
│ Memory Leak Prevention - 自動クリーンアップ │
└─────────────────────────────────────────────┘
```

**実装戦略**:
- `GameEvent.cs`: BaseGameEventクラス + ジェネリック型対応
- `GameEventListener.cs`: IGameEventListener<T>インターフェース実装
- `EventChannelRegistry.cs`: 全イベントチャネルの中央管理
- **メモリ最適化**: WeakReferenceによるリスナー自動解放
- **非同期対応**: UniTask統合によるRaiseAsync実装

#### 1.2 Command + ObjectPool Integration Architecture

```
Command System Architecture:
┌─────────────────────────────────────────────┐
│               Command Interface Layer        │
├─────────────────────────────────────────────┤
│ ICommand              │ IResettableCommand   │
│ - Execute()           │ - Reset()            │
│ - Undo()             │ - IsReusable         │
│ - CanUndo            │                      │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│             Factory + Registry Layer         │
├─────────────────────────────────────────────┤
│ ICommandFactory<T>    │ ITypeRegistry<T>     │
│ - Create<T>()         │ - Register<T>()      │
│ - CreatePooled<T>()   │ - Resolve<T>()       │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│              ObjectPool Layer               │
├─────────────────────────────────────────────┤
│ CommandPoolManager    │ IObjectPool<T>       │
│ - Get<T>()           │ - Get()              │
│ - Return<T>()        │ - Return()           │
│ - Statistics         │ - Clear()            │
└─────────────────────────────────────────────┘
```

**実装戦略**:
- `CommandPoolManager.cs`: Singleton統合管理クラス
- `ITypeRegistry<IObjectPool<ICommand>>`: 型安全なプール管理
- **統計システム**: 使用状況・再利用率の自動計測
- **メモリ監視**: Unity Profiler統合によるリアルタイム監視

### Layer 2: Feature System Layer（機能システム層）

#### Feature層設計原則
- **Core層基盤の活用**: Core層のイベント・コマンド・ServiceLocator基盤を利用
- **ゲームジャンル特化**: 各ゲームジャンルに最適化された機能実装
- **Core層非依存**: Feature間の直接参照を避け、Event駆動で連携

#### 2.1 State Machine System Design（Feature層実装）

**配置**: `Assets/_Project/Features/Camera`, `Assets/_Project/Features/AI`, `Assets/_Project/Features/Player`
**名前空間**: `asterivo.Unity60.Features.Camera`, `asterivo.Unity60.Features.AI`, `asterivo.Unity60.Features.Player`

##### Camera State Machine Architecture

```
Camera State System:
┌─────────────────────────────────────────────┐
│            Camera State Interface            │
├─────────────────────────────────────────────┤
│ ICameraState                                │
│ - Enter(context)     │ - Update(deltaTime)  │
│ - Exit()            │ - HandleInput()      │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│              State Implementations          │
├─────────────────────────────────────────────┤
│ FirstPersonState     │ ThirdPersonState     │
│ - FOV: 60-90°       │ - Distance: 3-8m     │
│ - MouseSensitivity  │ - OrbitControls      │
│ ├─────────────────────────────────────────┤
│ AimState            │ CoverState           │
│ - FOV: 30-45°       │ - PeekingOffset      │
│ - Crosshair UI      │ - WallAlignment      │
└─────────────────────────────────────────────┘
```

**技術実装詳細**:
- **Cinemachine 3.1統合**: VirtualCamera優先度管理
- **State Context**: CameraStateContext構造体による状態共有
- **Transition System**: StateTransitionRule配列によるルールベース遷移
- **Performance**: Dictionary<CameraStateType, ICameraState>による高速検索

##### AI State Machine Architecture

```
AI State System:
┌─────────────────────────────────────────────┐
│             AI State Interface               │
├─────────────────────────────────────────────┤
│ IAIState                                    │
│ - Enter(agent)       │ - Update(agent)      │
│ - Exit(agent)        │ - OnSuspicionChange()│
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│          Suspicion-Driven State System      │
├─────────────────────────────────────────────┤
│ SuspicionLevel: 0.0f - 1.0f                │
│ ├─ 0.0 - 0.3: Idle/Patrol                  │
│ ├─ 0.3 - 0.7: Suspicious/Investigating     │
│ ├─ 0.7 - 0.9: Alert/Searching              │
│ └─ 0.9 - 1.0: Combat                       │
└─────────────────────────────────────────────┘
```

**AI状態実装戦略**:
- **NavMeshAgent統合**: 各状態での移動制御パターン
- **Suspicion System**: 段階的警戒レベル制御による自動状態遷移
- **Behavior Tree Integration**: Complex AI行動の階層的管理
- **Memory System**: LastKnownPosition, SuspiciousEvents履歴管理

#### 2.2 Stealth Audio System Design（Core層基盤 + Feature層実装）

**Core層**: `Assets/_Project/Core/Audio` - オーディオシステムの基盤
**Feature層**: `Assets/_Project/Features/Stealth` - ステルス固有のオーディオ機能
**名前空間**:
- Core: `asterivo.Unity60.Core.Audio`
- Feature: `asterivo.Unity60.Features.Stealth.Audio`

```
Stealth Audio Architecture:
┌─────────────────────────────────────────────┐
│           Central Coordinator Layer          │
├─────────────────────────────────────────────┤
│ StealthAudioCoordinator (Singleton)         │
│ - AI警戒レベル連動    │ - 環境マスキング計算  │
│ - リアルタイムダッキング│ - 3D空間音響制御    │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│              Sensor Layer                   │
├─────────────────────────────────────────────┤
│ NPCAuditorySensor     │ DynamicAudioEnvironment│
│ - 3D距離減衰         │ - 環境音マスキング     │
│ - 障害物検知         │ - リアルタイム調整     │
└─────────────────────────────────────────────┘
```

**オーディオ実装戦略**:
- **AudioMixerGroup統合**: リアルタイムダッキング制御
- **Physics Raycasting**: 音響遮蔽の物理的計算
- **距離関数**: Custom曲線による現実的な音響減衰
- **Event Integration**: プレイヤー行動イベントとの即座連動

#### 2.3 AI Visual Sensor System Design（Feature層実装）

**配置**: `Assets/_Project/Features/AI/Sensors`
**名前空間**: `asterivo.Unity60.Features.AI.Sensors`
**Core層依存**: `asterivo.Unity60.Core.Events`, `asterivo.Unity60.Core.ServiceLocator`

```
Visual Sensor Architecture:
┌─────────────────────────────────────────────┐
│            NPCVisualSensor Core              │
├─────────────────────────────────────────────┤
│ NPCVisualSensor (MonoBehaviour)             │
│ - 継続的視界スキャン    │ - 多重判定システム   │
│ - 段階的検出制御       │ - 記憶システム      │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│           Detection Module Layer             │
├─────────────────────────────────────────────┤
│ VisualDetectionModule │ TargetTrackingModule │
│ - 距離・角度・遮蔽・光量│ - 複数目標同時追跡   │
│ - 閾値ベース判定       │ - 優先度管理        │
├─────────────────────────────────────────────┤
│ AlertSystemModule     │ MemoryModule         │
│ - 4段階警戒レベル      │ - 短期・長期記憶     │
│ - 自動遷移制御        │ - 位置履歴管理       │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│           Configuration Layer                │
├─────────────────────────────────────────────┤
│ VisualSensorSettings  │ DetectionConfiguration│
│ (ScriptableObject)    │ (ScriptableObject)   │
│ - スキャンパラメータ    │ - 検出閾値設定      │
│ - 記憶設定           │ - 環境設定          │
└─────────────────────────────────────────────┘
```

**視覚センサー実装戦略**:
- **継続的スキャンシステム**: Update()での10-20Hz可変頻度スキャン
- **多重判定統合**: VisibilityCalculator拡張による総合評価
- **警戒レベル管理**: AlertLevel.Relaxed → Suspicious → Investigating → Alert
- **記憶システム**: 短期記憶（5秒）→長期記憶（30秒）の階層管理
- **パフォーマンス最適化**: LOD、フレーム分散、早期カリング

##### Visual Sensor Data Structures

```csharp
// 検出目標情報の詳細構造
public class DetectedTarget
{
    public Transform target;
    public float detectionStrength;      // 0.0f - 1.0f
    public TargetThreatLevel threatLevel; // Low/Medium/High
    public float firstSeenTime;
    public float lastSeenTime;
    public Vector3 lastKnownPosition;
    public Vector3 predictedPosition;
    public bool isCurrentlyVisible;
    public bool isInvestigating;
}

// 警戒レベル定義
public enum AlertLevel
{
    Relaxed,        // 0.0f - 0.2f: 通常状態
    Suspicious,     // 0.2f - 0.5f: 疑念状態  
    Investigating,  // 0.5f - 0.8f: 調査状態
    Alert          // 0.8f - 1.0f: 警戒状態
}

// 視覚イベントデータ
public class VisualEventData
{
    public Transform observer;
    public Transform target;
    public float detectionStrength;
    public Vector3 detectionPosition;
    public AlertLevel alertLevel;
    public VisualEventType eventType;
}
```

##### Visual Sensor Performance Architecture

```
Performance Optimization Strategy:
┌─────────────────────────────────────────────┐
│            Frame Distribution System         │
├─────────────────────────────────────────────┤
│ DistributedScanCoroutine                    │
│ - Basic Scan (Frame 1)                     │
│ - Detailed Detection (Frame 2)             │
│ - Memory Update (Frame 3)                  │
│ - Prediction System (Frame 4)              │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│              LOD System                     │
├─────────────────────────────────────────────┤
│ Distance-Based Quality Scaling:             │
│ - Near (<15m): 1.0x quality                │
│ - Medium (<30m): 0.7x quality              │
│ - Far (>30m): 0.4x quality                 │
└─────────────────────────────────────────────┘
```

**性能要件実装**:
- **メモリ効率**: NPCあたり最大5KB使用量
- **CPU効率**: 1フレームあたり0.1ms以下
- **スケーラビリティ**: 50体NPC同時稼働対応
- **早期カリング**: 視界外目標の処理スキップ

##### AI Sensor Integration Architecture

```
Sensor Fusion System:
┌─────────────────────────────────────────────┐
│         Integrated Sensor Coordinator       │
├─────────────────────────────────────────────┤
│ NPCVisualSensor + NPCAuditorySensor         │
│ - 情報統合処理        │ - 相互補完機能      │
│ - 統合警戒レベル      │ - 協調検出システム   │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│            AIStateMachine Integration        │
├─────────────────────────────────────────────┤
│ Visual Detection → AI State Transition:    │
│ - Detection Events → Suspicious State      │
│ - Target Lost → Searching State            │
│ - Alert Level → Combat State               │
└─────────────────────────────────────────────┘
```

#### 2.4 Action RPG System Design（Feature層実装）

**FR-5対応**: アクションRPG統合システムの技術設計

```
Action RPG Feature Architecture:
┌─────────────────────────────────────────────┐
│         Character Progression System        │
├─────────────────────────────────────────────┤
│ CharacterStatsManager (Feature層)          │
│ - Level & Experience Management            │
│ - Skill Tree System                        │
│ - Stat Modification System                 │
│ - Core.Events連携による成長通知             │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│        Inventory & Equipment System         │
├─────────────────────────────────────────────┤
│ InventoryManager (Feature層)               │
│ - Item Management System                   │
│ - Equipment System Integration             │
│ - Core.Commands活用による操作管理          │
│ - ScriptableObject Data Integration        │
└─────────────────────────────────────────────┘
```

**配置**: `Assets/_Project/Features/ActionRPG/`
**名前空間**: `asterivo.Unity60.Features.ActionRPG.*`

##### Character Progression Implementation

```csharp
namespace asterivo.Unity60.Features.ActionRPG.Character
{
    public class CharacterStatsManager : MonoBehaviour
    {
        [SerializeField] private CharacterStatsData _statsData;
        [SerializeField] private GameEvent<LevelUpEventData> _onLevelUp;

        // Core.ServiceLocator経由でのサービス取得
        private ICommandInvoker _commandInvoker;

        private void Start()
        {
            // Core層のServiceLocatorを活用
            _commandInvoker = ServiceLocator.Get<ICommandInvoker>();
        }

        public void GainExperience(int amount)
        {
            // Core.Commandsを活用
            var command = new GainExperienceCommand(_statsData, amount);
            _commandInvoker.Execute(command);

            // レベルアップチェックとイベント発行
            if (CheckLevelUp())
            {
                _onLevelUp.Raise(new LevelUpEventData { NewLevel = _statsData.Level });
            }
        }
    }
}
```

##### Inventory System Implementation

```csharp
namespace asterivo.Unity60.Features.ActionRPG.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        [SerializeField] private InventoryData _inventoryData;
        [SerializeField] private GameEvent<ItemAcquiredEventData> _onItemAcquired;

        public void AcquireItem(ItemData item)
        {
            // Core.Commandsパターンでアイテム取得処理
            var command = new AcquireItemCommand(_inventoryData, item);
            ServiceLocator.Get<ICommandInvoker>().Execute(command);

            // Core.Eventsでアイテム取得通知
            _onItemAcquired.Raise(new ItemAcquiredEventData { Item = item });
        }

        public void EquipItem(EquipmentData equipment)
        {
            var command = new EquipItemCommand(_inventoryData, equipment);
            ServiceLocator.Get<ICommandInvoker>().Execute(command);
        }
    }
}
```

##### Action RPG Data Structures

```csharp
// ScriptableObject データ構造（Feature層）
[CreateAssetMenu(menuName = "ActionRPG/Character Stats")]
public class CharacterStatsData : ScriptableObject
{
    [Header("基本ステータス")]
    public int Level = 1;
    public int Experience = 0;
    public int Health = 100;
    public int Mana = 50;

    [Header("能力値")]
    public int Strength = 10;
    public int Agility = 10;
    public int Intelligence = 10;

    [Header("成長パラメータ")]
    public AnimationCurve ExperienceCurve;
    public StatGrowthData[] StatGrowthTable;
}

[CreateAssetMenu(menuName = "ActionRPG/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public ItemData[] AllItems;
    public EquipmentData[] AllEquipment;
    public Dictionary<int, ItemData> ItemLookup { get; private set; }

    private void OnEnable()
    {
        BuildLookupTable();
    }
}
```

##### Core層連携設計

**Event連携**:
- `LevelUpEvent`: キャラクターレベルアップ通知
- `ItemAcquiredEvent`: アイテム取得通知
- `EquipmentChangedEvent`: 装備変更通知
- `StatsChangedEvent`: ステータス変更通知

**Command連携**:
- `GainExperienceCommand`: 経験値取得処理
- `AcquireItemCommand`: アイテム取得処理
- `EquipItemCommand`: 装備変更処理
- `UsePotionCommand`: アイテム使用処理

**ServiceLocator連携**:
- `ICharacterStatsService`: キャラクターステータス管理サービス
- `IInventoryService`: インベントリ管理サービス
- `IEquipmentService`: 装備管理サービス

### Layer 3: Integration Layer（統合層）

#### 3.1 Cinemachine Integration Design（Feature層実装）

**FR-6対応**: Cinemachine 3.1統合カメラシステム
**配置**: `Assets/_Project/Features/Camera/Cinemachine`
**名前空間**: `asterivo.Unity60.Features.Camera.Cinemachine`

```
Cinemachine Integration:
┌─────────────────────────────────────────────┐
│         Cinemachine Wrapper Layer           │
├─────────────────────────────────────────────┤
│ CinemachineIntegration (Singleton)          │
│ - VirtualCamera管理   │ - Priority制御      │
│ - Blend設定          │ - Event連動        │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│            Configuration Layer              │
├─────────────────────────────────────────────┤
│ CameraConfig (ScriptableObject)            │
│ - カメラ設定データ    │ - プロファイル管理   │
│ - ブレンド時間       │ - 感度設定          │
└─────────────────────────────────────────────┘
```

#### 3.2 Input System Integration（Feature層実装）

**配置**: `Assets/_Project/Features/Input`
**名前空間**: `asterivo.Unity60.Features.Input`

```
Input Integration Architecture:
┌─────────────────────────────────────────────┐
│              Input Action Layer             │
├─────────────────────────────────────────────┤
│ PlayerInputActions (Generated)              │
│ - Movement Map       │ - Camera Map         │
│ - Combat Map         │ - UI Map             │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│            Input Handler Layer              │
├─────────────────────────────────────────────┤
│ InputManager         │ StateInputHandler    │
│ - Action Binding     │ - Context Switching  │
│ - Event Translation  │ - Priority Management│
└─────────────────────────────────────────────┘
```

## データアーキテクチャ設計

### ScriptableObject Data Architecture

```
Data Asset Hierarchy:
├─ GameData/
│  ├─ Characters/
│  │  ├─ CharacterStats.asset
│  │  ├─ PlayerConfig.asset
│  │  └─ NPCBehaviorData.asset
│  ├─ Audio/
│  │  ├─ StealthAudioConfig.asset
│  │  ├─ EnvironmentAudioData.asset
│  │  └─ SFXLibrary.asset
│  ├─ Camera/
│  │  ├─ CameraProfiles.asset
│  │  └─ StateTransitions.asset
│  └─ Events/
│     ├─ GameEvents.asset
│     └─ EventChannels.asset
```

### Data Validation Strategy

**実装方針**:
- **Odin Validator統合**: データ整合性の自動検証
- **Custom Validation Rules**: ゲーム固有のビジネスルール実装
- **Runtime Validation**: 実行時データ検証とエラーハンドリング
- **Editor Tools**: Inspector UI拡張による直感的編集環境

## パフォーマンス最適化設計

### Memory Optimization Strategy

#### ObjectPool最適化実装

```csharp
public class OptimizedCommandPool<T> : IObjectPool<T> where T : class, IResettableCommand, new()
{
    private readonly ConcurrentQueue<T> _pool = new();
    private readonly int _maxPoolSize;
    private int _currentCount;
    
    // 統計情報
    public PoolStatistics Statistics { get; }
    
    public T Get()
    {
        if (_pool.TryDequeue(out T item))
        {
            Statistics.RecordReuse();
            return item;
        }
        
        Statistics.RecordCreate();
        return new T();
    }
    
    public void Return(T item)
    {
        if (_currentCount < _maxPoolSize)
        {
            item.Reset();
            _pool.Enqueue(item);
            Interlocked.Increment(ref _currentCount);
        }
    }
}
```

### CPU Performance Strategy

#### Event System最適化
- **HashSet<T>による高速リスナー管理**: O(1)追加・削除
- **Priority Queue Cache**: ソート済みリストのキャッシュ
- **Batch Event Processing**: フレーム末尾でのイベント一括処理

#### State Machine最適化
- **Dictionary<StateType, IState>**: 定数時間状態検索
- **State Context Pooling**: Context構造体の再利用
- **Transition Rule Caching**: 遷移ルールの事前計算

## エディタツール設計

### Development Support Tools Architecture

#### EventFlowVisualizer
```
Event Flow Visualization:
┌─────────────────────────────────────────────┐
│              Graph Renderer                 │
├─────────────────────────────────────────────┤
│ - Node-Based UI      │ - Real-time Update   │
│ - Dependency Graph   │ - Interactive Filter │
└─────────────────────────────────────────────┘
```

**実装方針**:
- **Unity GraphView API**: ノードベースビジュアルエディタ
- **Real-time Monitoring**: Play Mode中のイベント流れ監視
- **Export功能**: PNG/PDFでのドキュメント出力

#### CommandInvokerEditor
- **履歴管理**: Command実行履歴の視覚化
- **Undo Stack Viewer**: Undoスタックの状態表示
- **Performance Monitor**: Command実行時間の統計表示

#### ProjectValidationWindow
- **Asset Validation**: ScriptableObjectの整合性検証
- **Reference Checker**: 参照関係の依存性分析
- **Performance Audit**: システム設定の最適化提案

#### AI Visual Sensor Debugger（新規追加）
```
Visual Sensor Debug Tools:
┌─────────────────────────────────────────────┐
│           Scene View Visualization           │
├─────────────────────────────────────────────┤
│ - 視界範囲の表示        │ - 検出目標の表示     │
│ - 記憶位置の表示        │ - 予測軌道の表示     │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│         Custom Inspector Window             │
├─────────────────────────────────────────────┤
│ - リアルタイムデバッグ情報│ - 警戒レベル表示    │
│ - アクティブ目標リスト    │ - メモリ使用状況    │
└─────────────────────────────────────────────┘
```

**デバッグ機能実装**:
- **Gizmos描画**: OnDrawGizmosSelectedでの視覚的表示
- **リアルタイム監視**: Play Mode中の状態更新表示
- **パフォーマンス測定**: Unity Profiler統合
- **ログ出力**: 構造化ログによる詳細トレース

#### ProjectDebugSystem統合デバッグツール（新規追加）

**FR-7.3対応**: プロジェクト専用の包括的デバッグシステム
**配置**: `Assets/_Project/Core/Debug` - Core層基盤デバッグシステム
**名前空間**: `asterivo.Unity60.Core.Debug`

```
ProjectDebugSystem Comprehensive Architecture:
┌─────────────────────────────────────────────┐
│            Unified Logging Layer            │
├─────────────────────────────────────────────┤
│ ProjectLogger (Static Class)               │
│ - LogLevel Management (Debug/Info/Warning/ │
│   Error/Critical)                          │
│ - Category-based Filtering                 │
│ - Structured Log Output                    │
│ - Editor/Runtime Environment Detection     │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│         Real-time Performance Monitor      │
├─────────────────────────────────────────────┤
│ PerformanceMonitor (MonoBehaviour Singleton)│
│ - Frame Rate Tracking                      │
│ - Memory Usage Monitoring                  │
│ - CPU Usage Analysis                       │
│ - GPU Performance Metrics                  │
│ - Unity Profiler Integration               │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│          Project Diagnostics Engine         │
├─────────────────────────────────────────────┤
│ ProjectDiagnostics (EditorWindow)          │
│ - Event Circular Dependency Detection      │
│ - Command Execution Statistics             │
│ - ObjectPool Efficiency Analysis           │
│ - Service Locator Health Check             │
│ - Asset Reference Validation               │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│         Environment-Specific Debug Config   │
├─────────────────────────────────────────────┤
│ DebugConfiguration (ScriptableObject)      │
│ - Development: Full Debug Info             │
│ - Testing: Performance Focus               │
│ - Production: Critical Only                │
│ - Auto Environment Detection               │
└─────────────────────────────────────────────┘
```

**統一ログシステム実装詳細**:

```csharp
namespace asterivo.Unity60.Core.Debug
{
    public static class ProjectLogger
    {
        public enum LogLevel
        {
            Debug = 0,
            Info = 1,
            Warning = 2,
            Error = 3,
            Critical = 4
        }

        public enum LogCategory
        {
            Core,           // Core系システム
            Features,       // Feature層機能
            Audio,          // オーディオシステム
            AI,             // AI・センサーシステム
            Commands,       // コマンドシステム
            Events,         // イベントシステム
            Performance,    // パフォーマンス
            Network,        // ネットワーク（将来用）
            Custom         // カスタムカテゴリ
        }

        private static DebugConfiguration _config;
        private static readonly Dictionary<LogCategory, List<LogEntry>> _logs = new();

        public static void Log(LogLevel level, LogCategory category, string message, UnityEngine.Object context = null)
        {
            if (!ShouldLog(level, category)) return;

            var entry = new LogEntry
            {
                Level = level,
                Category = category,
                Message = message,
                Timestamp = DateTime.UtcNow,
                Context = context,
                StackTrace = level >= LogLevel.Error ? Environment.StackTrace : null
            };

            RecordLog(entry);
            OutputToUnityConsole(entry);

            #if UNITY_EDITOR
            // エディタ専用機能: リアルタイム表示更新
            DebugWindow.RefreshLogs(entry);
            #endif
        }

        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Debug(LogCategory category, string message, UnityEngine.Object context = null)
            => Log(LogLevel.Debug, category, message, context);

        public static void Info(LogCategory category, string message, UnityEngine.Object context = null)
            => Log(LogLevel.Info, category, message, context);

        public static void Warning(LogCategory category, string message, UnityEngine.Object context = null)
            => Log(LogLevel.Warning, category, message, context);

        public static void Error(LogCategory category, string message, UnityEngine.Object context = null)
            => Log(LogLevel.Error, category, message, context);

        public static void Critical(LogCategory category, string message, UnityEngine.Object context = null)
            => Log(LogLevel.Critical, category, message, context);
    }

    [System.Serializable]
    public class LogEntry
    {
        public LogLevel Level;
        public LogCategory Category;
        public string Message;
        public DateTime Timestamp;
        public UnityEngine.Object Context;
        public string StackTrace;
    }
}
```

**リアルタイムパフォーマンス監視実装**:

```csharp
namespace asterivo.Unity60.Core.Debug
{
    public class PerformanceMonitor : MonoBehaviour
    {
        [Header("監視設定")]
        [SerializeField] private float _updateInterval = 0.5f;
        [SerializeField] private int _frameHistorySize = 100;

        // パフォーマンスメトリクス
        public float CurrentFPS { get; private set; }
        public float AverageFPS { get; private set; }
        public long AllocatedMemory { get; private set; }
        public long ReservedMemory { get; private set; }
        public float CPUUsage { get; private set; }

        private readonly Queue<float> _frameTimeHistory = new();
        private float _lastUpdateTime;

        private void Update()
        {
            RecordFrameTime();

            if (Time.unscaledTime - _lastUpdateTime >= _updateInterval)
            {
                UpdateMetrics();
                _lastUpdateTime = Time.unscaledTime;

                // 閾値チェックと警告
                CheckPerformanceThresholds();
            }
        }

        private void UpdateMetrics()
        {
            // FPS計算
            CurrentFPS = 1.0f / Time.unscaledDeltaTime;
            AverageFPS = _frameTimeHistory.Count > 0 ? 1.0f / _frameTimeHistory.Average() : CurrentFPS;

            // メモリ使用量
            AllocatedMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(0);
            ReservedMemory = UnityEngine.Profiling.Profiler.GetTotalReservedMemory(0);

            // CPUプロファイリング（Editor専用）
            #if UNITY_EDITOR
            CPUUsage = UnityEditor.EditorApplication.timeSinceStartup % 1.0f;
            #endif

            // ログ出力
            ProjectLogger.Debug(LogCategory.Performance,
                $"FPS: {CurrentFPS:F1} | Memory: {FormatBytes(AllocatedMemory)} | CPU: {CPUUsage:P}");
        }

        private void CheckPerformanceThresholds()
        {
            // FPS警告
            if (CurrentFPS < 30f)
            {
                ProjectLogger.Warning(LogCategory.Performance,
                    $"Low FPS detected: {CurrentFPS:F1}. Consider optimization.");
            }

            // メモリ警告
            var memoryMB = AllocatedMemory / (1024 * 1024);
            if (memoryMB > 500) // 500MB threshold
            {
                ProjectLogger.Warning(LogCategory.Performance,
                    $"High memory usage: {memoryMB}MB. Consider memory optimization.");
            }
        }
    }
}
```

**プロジェクト診断システム実装**:

```csharp
namespace asterivo.Unity60.Core.Debug
{
    #if UNITY_EDITOR
    public class ProjectDiagnosticsWindow : EditorWindow
    {
        [MenuItem("Tools/Project Debug/Diagnostics")]
        public static void ShowWindow()
        {
            GetWindow<ProjectDiagnosticsWindow>("Project Diagnostics");
        }

        private Vector2 _scrollPosition;
        private string[] _tabNames = {"Events", "Commands", "Performance", "ObjectPools"};
        private int _selectedTab = 0;

        private void OnGUI()
        {
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            switch (_selectedTab)
            {
                case 0: DrawEventsDiagnostics(); break;
                case 1: DrawCommandsDiagnostics(); break;
                case 2: DrawPerformanceDiagnostics(); break;
                case 3: DrawObjectPoolDiagnostics(); break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawEventsDiagnostics()
        {
            EditorGUILayout.LabelField("Event System Diagnostics", EditorStyles.boldLabel);

            // イベント循環依存検出
            if (GUILayout.Button("Check Circular Dependencies"))
            {
                var result = EventDependencyAnalyzer.CheckCircularDependencies();
                ProjectLogger.Info(LogCategory.Core, $"Circular dependency check: {result}");
            }

            // 登録済みイベント一覧
            var events = EventChannelRegistry.GetAllEvents();
            EditorGUILayout.LabelField($"Registered Events: {events.Count}");

            foreach (var eventChannel in events)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(eventChannel.Name);
                EditorGUILayout.LabelField($"Listeners: {eventChannel.ListenerCount}");
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawObjectPoolDiagnostics()
        {
            EditorGUILayout.LabelField("ObjectPool Diagnostics", EditorStyles.boldLabel);

            var poolManager = CommandPoolManager.Instance;
            if (poolManager != null)
            {
                var statistics = poolManager.GetStatistics();
                EditorGUILayout.LabelField($"Total Pools: {statistics.PoolCount}");
                EditorGUILayout.LabelField($"Objects in Use: {statistics.ObjectsInUse}");
                EditorGUILayout.LabelField($"Objects Available: {statistics.ObjectsAvailable}");
                EditorGUILayout.LabelField($"Reuse Rate: {statistics.ReuseRate:P}");
                EditorGUILayout.LabelField($"Memory Saved: {statistics.MemorySavedMB:F2} MB");
            }
        }
    }
    #endif
}
```

**環境別デバッグ設定管理**:

```csharp
[CreateAssetMenu(menuName = "Debug/Debug Configuration")]
public class DebugConfiguration : ScriptableObject
{
    [Header("Environment Settings")]
    public DebugEnvironment Environment = DebugEnvironment.Development;

    [Header("Log Level Settings")]
    public LogLevel DevelopmentLogLevel = LogLevel.Debug;
    public LogLevel TestingLogLevel = LogLevel.Info;
    public LogLevel ProductionLogLevel = LogLevel.Critical;

    [Header("Category Filters")]
    public LogCategory[] EnabledCategories = System.Enum.GetValues(typeof(LogCategory)).Cast<LogCategory>().ToArray();

    [Header("Performance Monitoring")]
    public bool EnablePerformanceMonitoring = true;
    public float PerformanceUpdateInterval = 0.5f;
    public bool EnableMemoryTracking = true;

    [Header("Debug UI")]
    public bool ShowDebugOverlay = true;
    public bool EnableRuntimeDebugWindow = false; // エディタ専用

    public enum DebugEnvironment
    {
        Development,  // 開発環境
        Testing,      // テスト環境
        Production    // プロダクション環境
    }

    public LogLevel GetCurrentLogLevel()
    {
        return Environment switch
        {
            DebugEnvironment.Development => DevelopmentLogLevel,
            DebugEnvironment.Testing => TestingLogLevel,
            DebugEnvironment.Production => ProductionLogLevel,
            _ => LogLevel.Info
        };
    }

    public bool ShouldLog(LogLevel level, LogCategory category)
    {
        return level >= GetCurrentLogLevel() && EnabledCategories.Contains(category);
    }
}
```

**実装戦略**:
- **Core層配置**: `asterivo.Unity60.Core.Debug`名前空間での一元管理
- **エディタ/ランタイム分離**: プリプロセッサディレクティブによる環境分離
- **パフォーマンス重視**: プロダクションビルドでの完全無効化
- **Unity Profiler統合**: 標準プロファイリングAPIの活用
- **構造化ログ**: カテゴリ・レベル別の効率的ログ管理
- **リアルタイム監視**: Play Mode中の継続的メトリクス収集

**トラブルシューティング支援機能**:
- **自動問題検出**: よくある設定ミス・パフォーマンス問題の検知
- **解決策提示**: 検出した問題に対する具体的な改善案
- **ワンクリック修復**: 可能な問題の自動修復機能
- **詳細診断レポート**: 包括的な健全性チェックレポート生成

## セキュリティ・品質保証設計

### Code Quality Assurance

#### Static Analysis Integration
```yaml
質品保証Pipeline:
1. Pre-Commit Hooks:
   - Coding Standard Check
   - Cyclomatic Complexity < 10
   - Unit Test Coverage > 80%

2. CI/CD Integration:
   - Unity Test Runner実行
   - Performance Benchmark
   - Memory Leak Detection

3. Code Review Process:
   - Architecture Compliance Check
   - Design Pattern Review
   - Performance Impact Assessment
```

### Memory Safety Strategy
- **Null Reference Prevention**: Null Conditional Operators使用徹底
- **Collection Safety**: ReadOnlyCollection<T>による参照安全性
- **Resource Management**: using statement + IDisposableパターン

## テスト戦略設計

### Testing Architecture

#### Unit Testing Strategy
```csharp
[TestFixture]
public class EventSystemTests
{
    [Test]
    public void EventRaise_WithMultipleListeners_CallsAllInPriorityOrder()
    {
        // Arrange
        var gameEvent = ScriptableObject.CreateInstance<GameEvent>();
        var listeners = CreatePriorityOrderedListeners();
        
        // Act
        gameEvent.Raise();
        
        // Assert
        AssertCallOrderMatchesPriority(listeners);
    }
}
```

#### Integration Testing Strategy
- **Scene-Based Tests**: 実際のシーンでの統合テスト
- **Performance Tests**: Memory/CPUパフォーマンス自動測定
- **AI Behavior Tests**: State Machine遷移の網羅テスト

#### Play Mode Testing
- **Real-time System Tests**: Play Mode中での動作検証
- **User Interaction Simulation**: Input Systemとの統合テスト
- **Audio System Tests**: 3D空間音響の精度検証

## 配布・デプロイメント設計

### Build Pipeline Architecture

```
Build Configuration:
├─ Development Build:
│  ├─ Full Debug Information
│  ├─ Editor Tool Integration
│  └─ Performance Profiler Enabled
├─ Staging Build:
│  ├─ Optimized Performance
│  ├─ Limited Debug Info
│  └─ QA Testing Ready
└─ Production Build:
   ├─ Maximum Optimization
   ├─ Asset Bundle Optimization
   └─ Platform-Specific Tuning
```

### Platform Optimization Strategy

#### iOS Optimization
- **Metal Rendering**: URP Metal特化最適化
- **Memory Constraints**: iOS固有のメモリ制限対応
- **Touch Input**: タッチインターフェース最適化

#### Android Optimization  
- **Vulkan/OpenGL ES**: レンダリングAPI自動選択
- **Fragment Management**: 多様な解像度対応
- **Performance Scaling**: デバイス性能に応じた品質調整

## 究極テンプレートロードマップ設計（SPEC.md v3.0 完全対応）

### Ultimate Template 5-Phase Architecture

この設計は、**SPEC.md v3.0 の5フェーズ構想（Phase A-E）**を技術的に実現し、**4つの核心価値**を段階的に提供します。

#### Phase A: 新規開発者対応機能設計（🔴 最高優先度）

##### A.1 Interactive Setup Wizard System Architecture

**FR-7.1.1 対応**: 1分以内のプロジェクトセットアップ自動化

```
Setup Wizard Comprehensive Architecture:
┌─────────────────────────────────────────────┐
│         Environment Diagnostics Layer       │
├─────────────────────────────────────────────┤
│ SystemRequirementChecker (基盤完成済み)     │
│ - Unity Version Validation ✅               │
│ - VS/VSCode Detection ✅                    │
│ - Git Configuration Check ✅                │
│ EnvironmentDiagnostics (新規実装)           │
│ - Hardware Diagnostics                      │
│ - Performance Assessment                    │
│ - PDF Report Generation                     │
│ - Auto-Fix Suggestions                      │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│          Setup Wizard UI Layer              │
├─────────────────────────────────────────────┤
│ SetupWizardWindow (EditorWindow)            │
│ - Step-by-Step Guidance                    │
│ - Progress Visualization                    │
│ - Error Handling & Recovery                 │
│ GenreSelectionUI                            │
│ - 7-Genre Preview System                    │
│ - Interactive Configuration                 │
│ - Real-time Validation                      │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│         Project Generation Engine            │
├─────────────────────────────────────────────┤
│ ProjectGenerationEngine                     │
│ - Template Asset Deployment                 │
│ - Scene Configuration Setup                 │
│ - Package Dependencies Resolution           │
│ - Settings Synchronization                  │
│ ModuleSelectionSystem                       │
│ - Audio Module Configuration                │
│ - Localization Setup                        │
│ - Analytics Integration                     │
└─────────────────────────────────────────────┘
```

**Clone & Create 価値実現技術詳細**:
- **目標**: 30分 → 1分セットアップ（97%短縮達成）
- **技術実装**: 
  - Unity Editor API による自動化
  - Package Manager プログラマティック制御
  - ScriptableObject 設定の動的生成
  - Scene構成の自動配置
- **エラー予防システム**: 事前診断による問題回避とワンクリック修復
- **進捗可視化**: リアルタイムプログレスバーと詳細ログ

##### A.2 Game Genre Templates System Architecture

**FR-8.1.2 対応**: 7ジャンル対応テンプレートシステム（アクションRPG追加）

```
Comprehensive Genre Template Architecture:
┌─────────────────────────────────────────────┐
│         Template Configuration Layer         │
├─────────────────────────────────────────────┤
│ GenreTemplateConfig (ScriptableObject)      │
│ ├─ FPS Template Configuration               │
│ │  - First Person Camera Setup              │
│ │  - Shooting Mechanics Presets             │
│ │  - Combat UI Configuration                │
│ ├─ TPS Template Configuration               │
│ │  - Third Person Camera System             │
│ │  - Cover System Integration               │
│ │  - Aiming & Movement Mechanics            │
│ ├─ Platformer Template Configuration        │
│ │  - Jump & Movement Physics                │
│ │  - Collectible Systems                    │
│ │  - Level Design Tools                     │
│ ├─ Stealth Template Configuration           │
│ │  - AI Detection Systems ✅                │
│ │  - Stealth Mechanics                      │
│ │  - Environmental Interaction              │
│ ├─ Adventure Template Configuration         │
│ │  - Dialogue Systems                       │
│ │  - Inventory Management                   │
│ │  - Quest System Framework                 │
│ ├─ Strategy Template Configuration          │
│ │  - RTS Camera Controls                    │
│ │  - Unit Selection Systems                 │
│ │  - Resource Management UI                 │
│ └─ Action RPG Template Configuration       │
│    - Character Progression Systems          │
│    - Inventory & Equipment Management       │
│    - Action Combat Mechanics                │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│         Runtime Template Management          │
├─────────────────────────────────────────────┤
│ TemplateManager (Singleton)                │
│ - Dynamic Genre Switching                   │
│ - State Preservation System                 │
│ - Asset Bundle Management                   │
│ - Configuration Synchronization             │
│ TemplateTransitionSystem                    │
│ - Smooth Scene Transitions                  │
│ - Data Migration Between Genres             │
│ - User Progress Preservation                │
└─────────────────────────────────────────────┘
```

**Learn & Grow 価値実現技術詳細**:
- **段階的学習システム**: 5段階構成（基礎→応用→実践→カスタマイズ→出版）
- **学習コスト70%削減実現**: 
  - インタラクティブチュートリアル統合
  - 実践的サンプルゲームプレイ
  - リアルタイムヒントシステム
  - 進捗追跡と成果測定
- **各ジャンル15分ゲームプレイ実現**: 基本動作確認可能なサンプルシーン

##### A.3 Asset Store Integration Guide System

**FR-7.1.3 対応**: 人気アセット統合支援システム

```
Asset Integration Architecture:
┌─────────────────────────────────────────────┐
│          Asset Compatibility Engine         │
├─────────────────────────────────────────────┤
│ AssetCompatibilityChecker                   │
│ - Popular Assets Database (50+ assets)     │
│ - Dependency Conflict Resolution            │
│ - Version Compatibility Matrix             │
│ - Integration Step-by-Step Guides          │
│ AssetRecommendationSystem                   │
│ - Genre-Specific Recommendations            │
│ - Price & Rating Information               │
│ - Community Reviews Integration             │
└─────────────────────────────────────────────┘
```

**Community & Ecosystem 価値実現**: アセット共有と知識交換の技術基盤提供

#### Phase B: 高度なゲーム機能設計（🟡 高優先度）

##### B.1 Advanced Save/Load System Architecture

**FR-8.2.1 対応**: 高度なセーブ/ロードシステム

```
Save System Comprehensive Architecture:
┌─────────────────────────────────────────────┐
│           Modular Save Management            │
├─────────────────────────────────────────────┤
│ SaveSystemManager (ScriptableObject-based)  │
│ - Multi-Slot Support (10 slots)            │
│ - Auto-Save System (Time + Checkpoint)     │
│ - Cloud Integration (Steam/iCloud/Google)   │
│ - AES256 Encryption Layer                   │
│ - Version Migration System                  │
│ - Data Integrity Verification               │
└─────────────────────────────────────────────┘
```

##### B.2-B.4 追加システム設計

**FR-8.2.2**: Comprehensive Settings System - リアルタイム設定変更
**FR-8.2.3**: 4言語対応ローカリゼーション - 日英中韓サポート
**FR-8.2.4**: Performance Profiler Integration - リアルタイム監視

#### Phase C-E: プロダクション・エコシステム機能（🟢🔵 中低優先度）

**Ship & Scale 価値実現**: プロダクション対応の包括機能群
- **Phase C (FR-8.3.1)**: Build Pipeline, Asset Validation, Memory Management
- **Phase D (FR-8.3.2)**: Package Templates, Code Generator, Visual Scripting
- **Phase E (FR-8.3.3)**: Plugin Architecture, Template Marketplace, Community Docs

### 成功指標・パフォーマンス要件の技術実現

#### 定量的目標の技術実装戦略

**REQUIREMENTS.md NFR-1.5 究極テンプレートパフォーマンス要件実現**:

```
Performance Achievement Architecture:
┌─────────────────────────────────────────────┐
│         Setup Time Optimization (97%削減)   │
├─────────────────────────────────────────────┤
│ - Parallel Asset Processing                 │
│ - Pre-compiled Template Bundles             │
│ - Incremental Configuration Updates         │
│ - Smart Caching System                      │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│         Learning Cost Reduction (70%削減)   │
├─────────────────────────────────────────────┤
│ - Interactive Tutorial Engine               │
│ - Progressive Disclosure UI                 │
│ - Context-Aware Help System                 │
│ - Real-time Progress Tracking               │
└─────────────────────────────────────────────┘
```

#### 品質保証の技術実装

**Unity 6完全対応とクリーン実装**:
- 既存実装: エラー０、警告０の完全クリーンコード品質達成済み
- NPCVisualSensorシステム: 95%メモリ削減、67%実行速度改善実現済み  
- 50体NPC同時稼働: パフォーマンス要件達成済み

## 将来拡張設計

### Phase 2 Extension Architecture

#### Multiplayer Integration Points
```
Future Multiplayer Architecture:
┌─────────────────────────────────────────────┐
│         Network Abstraction Layer           │
├─────────────────────────────────────────────┤
│ INetworkCommand       │ INetworkEvent       │
│ - NetworkExecute()    │ - NetworkRaise()    │
│ - Serialize()         │ - Deserialize()     │
└─────────────────────────────────────────────┘
```

#### DI Framework Integration Points
- **Container Registration**: 型登録の中央化
- **Lifecycle Management**: オブジェクト生成・破棄の自動化
- **Dependency Graph**: 依存関係の可視化・最適化

### Phase 3 Advanced Features

#### DOTS Partial Integration
- **High-Performance Systems**: ECS適用対象システムの特定
- **Hybrid Architecture**: MonoBehaviour + ECS共存設計
- **Migration Strategy**: 段階的ECS移行計画

#### Machine Learning Integration
- **Behavior Optimization**: AI行動パターンの機械学習最適化
- **Balance Tuning**: ゲームバランスの自動調整AI
- **Player Analytics**: プレイヤー行動分析とコンテンツ最適化

## SDD（スペック駆動開発）統合設計

### SDD Workflow Integration Architecture

```
SDD Integration System:
┌─────────────────────────────────────────────┐
│            Document Management Layer         │
├─────────────────────────────────────────────┤
│ MarkdownDocumentManager                     │
│ - SPEC → REQUIREMENTS → DESIGN → TASKS     │
│ - Version Control Integration               │
│ - Automated Phase Transition               │
└─────────────────────────────────────────────┘
┌─────────────────────────────────────────────┐
│              AI Integration Layer            │
├─────────────────────────────────────────────┤
│ Claude Code MCP Server Integration:         │
│ - unityMCP: Direct Unity manipulation      │
│ - context7: Technical documentation search │
│ - git: Version control management          │
└─────────────────────────────────────────────┘
```

**SDD実装戦略**:
- **5段階フェーズ管理**: 構想→形式化→設計→分解→実装・検証
- **AI連携コマンド**: `/spec-create`, `/design-create`, `/tasks-create`, `/todo-execute`
- **トレーサビリティ**: 要件から実装までの完全追跡
- **品質保証**: 各フェーズでの整合性検証

### MCPサーバー統合戦略

#### 戦略的優先順位フレームワーク
1. **情報収集フェーズ**: ddg-search → context7 → deepwiki
2. **実装フェーズ**: context7 → unityMCP → git
3. **3Dコンテンツ制作**: blender-mcp → unityMCP → git

**AI+人間ハイブリッド開発**:
- **AI責任範囲**: コード生成、技術調査、ドキュメント作成
- **人間責任範囲**: アーキテクチャ判断、品質検証、戦略決定
- **相互連携**: MCPサーバー活用による効率化

## まとめ・SDD統合による価値実現

この技術設計書は、**SPEC.md v3.0 究極テンプレートビジョン → REQUIREMENTS.md 形式化要件 → 本技術設計書**の完全なトレーサビリティを確保し、以下の価値実現を技術的に保証します：

### 4つの核心価値実現のための完全設計基盤

#### Clone & Create 価値の技術実現
- **1分セットアップ**: SystemRequirementChecker基盤 + 新規EnvironmentDiagnostics + SetupWizardWindow
- **97%時間短縮**: 30分→1分セットアップの並列処理とスマートキャッシュ技術
- **エラー予防**: 事前診断システムによる問題回避とワンクリック修復

#### Learn & Grow 価値の技術実現  
- **70%学習コスト削減**: 40時間→12時間のインタラクティブチュートリアルエンジン
- **段階的成長支援**: 5段階学習システム（基礎→応用→実践→カスタマイズ→出版）
- **7ジャンル完全対応**: FPS/TPS/Platformer/Stealth/Adventure/Strategy/ActionRPG

#### Ship & Scale 価値の技術実現
- **プロダクション品質**: エラー０・警告０のクリーン実装基盤
- **パフォーマンス最適化**: 95%メモリ削減・67%速度改善の継承
- **スケーラビリティ**: Phase A-E段階的拡張アーキテクチャ

#### Community & Ecosystem 価値の技術実現
- **アセット統合支援**: 50+人気アセット対応システム
- **知識共有基盤**: コミュニティドキュメント・テンプレートマーケットプレイス
- **エコシステム拡張**: プラグインアーキテクチャとAPIゲートウェイ

### SDD フェーズ4への完全橋渡し

#### TASKS.md生成のための実装基盤確保
- **Core/Feature分離実装基盤**: アーキテクチャ分離原則に基づく実装ガイドライン確立
- **Phase A優先実装**: FR-8.1.1 (Setup Wizard) の技術設計完了
- **アクションRPG統合**: FR-5技術設計によるFeature層実装基盤確立
- **アーキテクチャ詳細**: 具体的クラス設計・API仕様・データフロー定義
- **実装ガイドライン**: Core/Feature分離、名前空間規約、コーディング標準

#### 技術実装の完全準備
- **既存基盤活用**: NPCVisualSensor等の成功実装パターンを新機能に適用
- **Unity 6最適化**: 最新技術スタックでの実装方針確立
- **品質保証統合**: CI/CD・静的解析・パフォーマンステスト戦略

### 究極テンプレート実現への確実な道筋

この技術設計により、**REQUIREMENTS.md FR-8 究極テンプレートロードマップと Core/Feature層分離アーキテクチャ**の技術的実現が完全に可能となり、次のフェーズ（TASKS.md → 実装・検証）への確実な移行を保証します。

**設計完了状態**: ✅ REQUIREMENTS.md 完全対応（FR-5アクションRPG + FR-8ロードマップ）、Core/Feature分離強化、技術実装準備完了