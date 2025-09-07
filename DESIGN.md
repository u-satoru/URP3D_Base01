# DESIGN.md - Unity 6 3Dゲーム基盤プロジェクト 技術設計書

## 文書管理情報

- **ドキュメント種別**: 技術設計書
- **生成元**: REQUIREMENTS.md - Unity 6 3Dゲーム基盤プロジェクト 形式化された要件定義
- **対象読者**: アーキテクト、シニア開発者、技術リード
- **更新日**: 2025年1月（REQUIREMENTS.md v2.0 対応設計）

## 設計原則とアーキテクチャビジョン

### 核心設計思想

#### 1. Event-Driven Architecture First
- **原則**: 全てのシステム間通信をイベント駆動で実装
- **実現手段**: ScriptableObjectベースのイベントチャネル（GameEvent）
- **効果**: 疎結合設計による保守性・拡張性の最大化

#### 2. Command + ObjectPool 統合による極限最適化
- **原則**: 全ての操作をコマンド化し、ObjectPoolで再利用
- **実現手段**: Factory+Registry+ObjectPool統合アーキテクチャ
- **効果**: 95%メモリ削減、67%実行速度改善の実現

#### 3. State-Driven Behavior Management
- **原則**: 複雑な行動制御を状態マシンで管理
- **実現手段**: Dictionary<StateType, IState>による高速状態管理
- **効果**: 予測可能で拡張容易な行動システム

#### 4. Data-Configuration Driven Development
- **原則**: ロジックとデータの完全分離
- **実現手段**: ScriptableObjectによるデータ資産化
- **効果**: ノンプログラマーによる独立したゲーム調整

## システムアーキテクチャ設計

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

#### 2.1 State Machine System Design

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

#### 2.2 Stealth Audio System Design

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

### Layer 3: Integration Layer（統合層）

#### 3.1 Cinemachine Integration Design

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

#### 3.2 Input System Integration

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

この技術設計書は、REQUIREMENTS.mdで定義された要件を、具体的な実装可能な技術仕様に変換し、後続のTASKS.md生成とコード実装の基盤となります。