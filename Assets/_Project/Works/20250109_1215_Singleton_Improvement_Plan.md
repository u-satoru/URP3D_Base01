# Singletonパターン改善計画書

**作成日時**: 2025年1月9日 12:15  
**プロジェクト**: URP3D_Base01 - Unity 6 3Dゲーム基盤プロジェクト  
**対象**: Singletonパターン使用箇所の詳細分析と改善策

---

## エグゼクティブサマリー

本プロジェクトで8つのSingletonパターン使用箇所を特定しました。特にオーディオシステムの5つのSingletonが相互依存により密結合状態にあり、テスタビリティと保守性に重大な影響を与えています。段階的な改善アプローチにより、システムの安定性を保ちながらアーキテクチャを改善する計画を提案します。

---

## 1. Singleton使用ファイル一覧と詳細

### 🔴 最優先改善対象（オーディオシステム - 相互依存により密結合）

#### 1.1 AudioManager.cs
```csharp
// ファイルパス: Assets/_Project/Core/Audio/AudioManager.cs
// 該当行: 17-18行, 71-80行
```
- **現状**: 最上位オーディオ制御の中心的Singleton
- **依存**: SpatialAudioManager, StealthAudioCoordinator, DynamicAudioEnvironment
- **問題**: 他の4つのオーディオSingletonと密結合
- **改善策**:
  ```csharp
  // 現在のコード
  public class AudioManager : MonoBehaviour {
      private static AudioManager instance;
      public static AudioManager Instance => instance;
  }
  
  // 改善案: Service Locatorパターン
  public interface IAudioService {
      void PlaySound(string soundId);
      void StopSound(string soundId);
  }
  
  public class AudioService : IAudioService {
      // Singletonではない通常のサービスクラス
  }
  
  // ServiceLocatorで管理
  ServiceLocator.RegisterService<IAudioService>(new AudioService());
  ```

#### 1.2 SpatialAudioManager.cs
```csharp
// ファイルパス: Assets/_Project/Core/Audio/SpatialAudioManager.cs
// 該当行: 51-52行, 56-85行
```
- **現状**: 空間音響システムの中央管理
- **問題**: AudioManagerとの循環依存
- **改善策**:
  ```csharp
  // 改善案: イベント駆動で疎結合化
  public class SpatialAudioService : MonoBehaviour {
      [SerializeField] private AudioEvent onSpatialSoundRequested;
      
      private void HandleSpatialSound(SpatialSoundData data) {
          // イベント経由で処理
      }
  }
  ```

#### 1.3 AudioUpdateCoordinator.cs
```csharp
// ファイルパス: Assets/_Project/Core/Audio/AudioUpdateCoordinator.cs
// 該当行: 54-55行, 59-92行
```
- **現状**: オーディオシステム全体の更新調整
- **依存**: AudioManager, WeatherAmbientController, TimeAmbientController
- **問題**: 初期化順序依存
- **改善策**:
  ```csharp
  // 改善案: コンポジションによる依存注入
  public class AudioUpdateCoordinator : MonoBehaviour {
      [SerializeField] private List<IAudioUpdatable> audioComponents;
      
      private void Awake() {
          // コンポーネントを直接参照せず、インターフェース経由
          audioComponents = GetComponents<IAudioUpdatable>().ToList();
      }
  }
  ```

#### 1.4 EffectManager.cs
```csharp
// ファイルパス: Assets/_Project/Core/Audio/EffectManager.cs
// 該当行: 61-62行, 66-96行
```
- **現状**: 効果音システム管理
- **依存**: AudioManager, SpatialAudioManager, StealthAudioCoordinator
- **問題**: 3つのSingletonへの依存
- **改善策**:
  ```csharp
  // 改善案: ファクトリーパターン
  public class EffectSystemFactory {
      public IEffectSystem CreateEffectSystem(AudioConfiguration config) {
          return new EffectSystem(
              config.GetAudioService(),
              config.GetSpatialService()
          );
      }
  }
  ```

#### 1.5 StealthAudioCoordinator.cs
```csharp
// ファイルパス: Assets/_Project/Core/Audio/StealthAudioCoordinator.cs
// 該当行: 64-65行, 69-102行
```
- **現状**: ステルスゲーム特化のオーディオ調整
- **依存**: AudioManager, DynamicAudioEnvironment
- **問題**: 特化機能なのに中央システムに依存
- **改善策**:
  ```csharp
  // 改善案: Strategy パターン
  public interface IAudioStrategy {
      void ProcessAudio(AudioContext context);
  }
  
  public class StealthAudioStrategy : IAudioStrategy {
      // Singletonではなく、戦略として実装
  }
  ```

---

### 🟡 中優先度改善対象

#### 1.6 CommandPoolService.cs
```csharp
// ファイルパス: Assets/_Project/Core/Commands/CommandPoolService.cs
// 該当行: 35行, 42行, 51-68行
```
- **現状**: CommandPoolManagerのサービスラッパー
- **問題**: 冗長な実装（CommandPoolManagerと重複）
- **改善策**:
  ```csharp
  // 改善案: CommandPoolManagerに統合
  public static class CommandPool {
      private static readonly CommandPoolManager manager = new();
      
      public static T Get<T>() where T : ICommand, new() {
          return manager.GetCommand<T>();
      }
  }
  ```

#### 1.7 CinemachineIntegration.cs
```csharp
// ファイルパス: Assets/_Project/Features/Camera/Scripts/CinemachineIntegration.cs
// 該当行: 22-23行, 90-102行
```
- **現状**: Cinemachine統合管理
- **問題**: 比較的独立しているが、グローバル状態
- **改善策**:
  ```csharp
  // 改善案: カメラシステムをGameEventで制御
  public class CameraController : MonoBehaviour {
      [SerializeField] private CameraStateEvent onCameraStateChanged;
      
      private void HandleCameraChange(CameraState newState) {
          onCameraStateChanged.Raise(newState);
      }
  }
  ```

---

### 🟢 低優先度改善対象

#### 1.8 EventLogger.cs
```csharp
// ファイルパス: Assets/_Project/Core/Debug/EventLogger.cs
// 該当行: 14行, 22-34行, 89-101行
```
- **現状**: デバッグ用イベントロギング
- **問題**: 影響は限定的（デバッグツール）
- **改善策**:
  ```csharp
  // 改善案: 条件付きコンパイルで本番環境では除外
  #if UNITY_EDITOR || DEVELOPMENT_BUILD
  public class EventLogger : MonoBehaviour {
      // デバッグビルドのみで有効
  }
  #endif
  ```

---

## 2. 段階的改善実装計画

### Phase 1: 基盤準備（1週間）

#### Step 1.1: Service Locatorの実装
```csharp
// Assets/_Project/Core/Services/ServiceLocator.cs
public static class ServiceLocator {
    private static readonly Dictionary<Type, object> services = new();
    
    public static void RegisterService<T>(T service) where T : class {
        services[typeof(T)] = service;
    }
    
    public static T GetService<T>() where T : class {
        return services.TryGetValue(typeof(T), out var service) 
            ? service as T 
            : null;
    }
    
    public static void Clear() {
        services.Clear();
    }
}
```

#### Step 1.2: オーディオインターフェース定義
```csharp
// Assets/_Project/Core/Audio/Interfaces/
public interface IAudioService { }
public interface ISpatialAudioService { }
public interface IEffectService { }
public interface IStealthAudioService { }
```

### Phase 2: オーディオシステム改善（2週間）

#### Step 2.1: AudioManagerの改善
1. IAudioServiceインターフェース実装
2. Singleton削除、ServiceLocator登録
3. 依存関係をコンストラクタ注入に変更

#### Step 2.2: 依存関係の解消
1. 循環依存をイベント駆動に置換
2. 初期化順序を明示的に制御
3. DontDestroyOnLoadの見直し

### Phase 3: その他システム改善（1週間）

#### Step 3.1: CommandPoolService統合
1. CommandPoolManagerに機能統合
2. 静的ユーティリティクラス化

#### Step 3.2: CinemachineIntegration改善
1. GameEvent経由の制御に移行
2. カメラステート管理の分離

### Phase 4: テスト実装（1週間）

#### Step 4.1: 単体テスト追加
```csharp
[Test]
public void AudioService_PlaySound_Success() {
    // Arrange
    var mockService = new Mock<IAudioService>();
    ServiceLocator.RegisterService(mockService.Object);
    
    // Act
    var service = ServiceLocator.GetService<IAudioService>();
    service.PlaySound("test_sound");
    
    // Assert
    mockService.Verify(x => x.PlaySound("test_sound"), Times.Once);
}
```

---

## 3. リスク管理と対策

### リスク1: 既存システムの破壊
- **対策**: Feature Flagによる段階的切り替え
```csharp
public static class FeatureFlags {
    public static bool UseNewAudioSystem = false;
}
```

### リスク2: パフォーマンス劣化
- **対策**: Unity Profilerでの継続的測定
- **基準**: フレームあたり0.1ms以下の影響

### リスク3: 初期化順序の混乱
- **対策**: 明示的な初期化マネージャー
```csharp
public class SystemInitializer : MonoBehaviour {
    [SerializeField] private List<IInitializable> systems;
    
    private void Awake() {
        foreach (var system in systems.OrderBy(s => s.Priority)) {
            system.Initialize();
        }
    }
}
```

---

## 4. 成功指標

### 定量的指標
- Singletonの使用数: 8個 → 1個以下（GameManagerのみ）
- テストカバレッジ: 19% → 60%以上
- 循環依存: 5箇所 → 0箇所

### 定性的指標
- 新機能追加時の影響範囲最小化
- モックを使った単体テストの容易化
- チーム開発での並行作業可能性向上

---

## 5. 推奨実装順序

1. **Week 1**: Service Locator実装とインターフェース定義
2. **Week 2-3**: AudioManager, SpatialAudioManagerの改善
3. **Week 4**: EffectManager, StealthAudioCoordinatorの改善
4. **Week 5**: CommandPoolService, CinemachineIntegrationの改善
5. **Week 6**: テスト実装と品質保証

---

## 6. 代替案検討

### Option A: 完全なDependency Injection (Zenject/VContainer)
- **メリット**: 業界標準、強力な機能
- **デメリット**: 学習コスト、SPEC.mdの「DI不使用」方針に反する
- **判定**: ❌ 採用しない

### Option B: 純粋なイベント駆動
- **メリット**: 完全な疎結合
- **デメリット**: デバッグ困難、パフォーマンスオーバーヘッド
- **判定**: △ 部分採用

### Option C: Service Locator + Event駆動ハイブリッド
- **メリット**: バランスの良い解決策
- **デメリット**: 2つのパターンの学習必要
- **判定**: ✅ 推奨案として採用

---

## まとめ

Singletonパターンの過剰使用は、本プロジェクトの最も重要な技術的負債です。特にオーディオシステムの5つのSingletonは相互依存により密結合状態にあり、早急な改善が必要です。

提案した段階的改善計画により、システムの安定性を保ちながら、6週間でアーキテクチャの大幅な改善が可能です。Service LocatorとEvent駆動のハイブリッドアプローチにより、SPEC.mdの「DI不使用」方針を守りつつ、テスタビリティと保守性を向上させることができます。
