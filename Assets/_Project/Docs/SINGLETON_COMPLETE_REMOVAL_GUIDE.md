# 🚀 Complete Singleton Removal Guide

このガイドでは、Singleton Pattern Migration準拠済みのクラスから完全にSingletonパターンを削除する手順を説明します。

## 📋 前提条件チェックリスト

- [ ] ✅ ServiceLocator.UseServiceLocator = true
- [ ] ✅ 全サービスがServiceLocatorに正常登録済み
- [ ] ✅ システム健全性スコア 90%以上
- [ ] ✅ 包括的バックアップ作成済み
- [ ] ✅ 緊急ロールバック手順確認済み

## 🎯 完全削除プロセス

### Phase 1: 準備フェーズ

#### 1.1 事前バックアップ作成

```csharp
// SingletonRemovalPlan コンポーネントを使用
var removalPlan = FindFirstObjectByType<SingletonRemovalPlan>();
removalPlan.CreateComprehensiveBackup();
```

#### 1.2 FeatureFlags最終設定

```csharp
FeatureFlags.DisableLegacySingletons = true;        // Singleton完全無効化
FeatureFlags.EnableMigrationWarnings = false;       // 警告システム無効化
FeatureFlags.EnableMigrationMonitoring = false;     // 監視システム無効化
```

### Phase 2: 物理的コード削除

以下の6クラスから指定コードを削除します：

## 🔧 AudioManager.cs

### ❌ 削除対象コード

```csharp
// 1. Singleton instance フィールド
private static AudioManager instance;

// 2. Instance プロパティ（全体）
[System.Obsolete("Use ServiceLocator.GetService<IAudioService>() instead")]
public static AudioManager Instance 
{
    get 
    {
        // Legacy Singleton完全無効化フラグの確認
        if (FeatureFlags.DisableLegacySingletons) 
        {
            var eventLogger = ServiceLocator.GetService<IEventLogger>(); 
            if (eventLogger != null) eventLogger.LogError("[DEPRECATED] AudioManager.Instance is disabled. Use ServiceLocator.GetService<IAudioService>() instead");
            return null;
        }
        
        // 移行警告の表示
        if (FeatureFlags.EnableMigrationWarnings) 
        {
            var eventLogger = ServiceLocator.GetService<IEventLogger>(); 
            if (eventLogger != null) eventLogger.LogWarning("[DEPRECATED] AudioManager.Instance usage detected. Please migrate to ServiceLocator.GetService<IAudioService>()");
            
            // MigrationMonitorに使用状況を記録
            if (FeatureFlags.EnableMigrationMonitoring)
            {
                var migrationMonitor = ServiceHelper.GetServiceWithFallback<asterivo.Unity60.Core.Services.MigrationMonitor>();
                migrationMonitor?.LogSingletonUsage(typeof(AudioManager), "AudioManager.Instance");
            }
        }
        
        return instance;
    }
}

// 3. Awake()内のinstance代入
instance = this;

// 4. OnDestroy()内のinstance解除
if (instance == this)
{
    // ServiceLocatorからの登録解除
    // ...
    instance = null;
}
```

### ✅ 保持対象コード

```csharp
// ServiceLocator登録（絶対保持）
ServiceLocator.RegisterService<IAudioService>(this);

// DontDestroyOnLoad（保持）
DontDestroyOnLoad(gameObject);

// IAudioService インターフェース実装（保持）
public void PlaySound(string soundId, Vector3 position = default, float volume = 1f) { ... }
public void SetMasterVolume(float volume) { ... }
// ... 他のインターフェースメソッド
```

## 🔧 SpatialAudioManager.cs

### ❌ 削除対象コード

```csharp
// 同様のパターンでSingleton関連コードを削除
private static SpatialAudioManager instance;
public static SpatialAudioManager Instance { get; }  // プロパティ全体
instance = this;  // Awake()内
instance = null;  // OnDestroy()内
```

### ✅ 保持対象コード

```csharp
ServiceLocator.RegisterService<ISpatialAudioService>(this);
// ISpatialAudioService インターフェース実装
```

## 🔧 EffectManager.cs

### ❌ 削除対象コード

```csharp
private static EffectManager instance;
public static EffectManager Instance { get; }  // プロパティ全体
instance = this;  // Awake()内
instance = null;  // OnDestroy()内
```

### ✅ 保持対象コード

```csharp
ServiceLocator.RegisterService<IEffectService>(this);
// IEffectService インターフェース実装
```

## 🔧 CommandPoolService.cs

### ❌ 削除対象コード

```csharp
private static CommandPoolService instance;
public static CommandPoolService Instance { get; }  // プロパティ全体
instance = this;  // Awake()内
instance = null;  // OnDestroy()内
```

### ✅ 保持対象コード

```csharp
ServiceLocator.RegisterService<ICommandPoolService>(this);
// ICommandPoolService インターフェース実装
```

## 🔧 EventLogger.cs

### ❌ 削除対象コード

```csharp
private static EventLogger instance;
public static EventLogger Instance { get; }  // プロパティ全体
instance = this;  // Awake()内
instance = null;  // OnDestroy()内
```

### ✅ 保持対象コード

```csharp
ServiceLocator.RegisterService<IEventLogger>(this);
// IEventLogger インターフェース実装
```

## 🔧 CinemachineIntegration.cs

### ❌ 削除対象コード

```csharp
private static CinemachineIntegration instance;
public static CinemachineIntegration Instance { get; }  // プロパティ全体
instance = this;  // Awake()内
instance = null;  // OnDestroy()内
```

### ✅ 保持対象コード

```csharp
ServiceLocator.RegisterService<CinemachineIntegration>(this);
// カメラ管理メソッド
```

### Phase 3: 削除後検証

#### 3.1 コンパイルチェック

```bash
# Unity Editor でコンパイルエラーがないことを確認
# 特に以下に注意：
# - Instance プロパティへの参照が残っていないか
# - Singleton.instance = this への参照が残っていないか
```

#### 3.2 ランタイムテスト

```csharp
// ServiceLocator経由でのアクセステスト
var audioService = ServiceLocator.GetService<IAudioService>();
audioService.PlaySound("test-sound");

var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
spatialService.PlaySpatialSound("test", Vector3.zero);

// 各サービスが正常動作することを確認
```

#### 3.3 最終検証

```csharp
// 1. SystemHealthチェック
var healthStatus = EmergencyRollback.CheckSystemHealth();
Debug.Log($"System Health: {healthStatus.HealthScore}%");

// 2. サービス登録状況確認
var removalPlan = FindFirstObjectByType<SingletonRemovalPlan>();
bool allServicesOK = removalPlan.ValidateServiceRegistration();
Debug.Log($"All Services OK: {allServicesOK}");
```

## 🚨 緊急時対応

### 問題発生時のロールバック手順

```csharp
// 1. 緊急ロールバック実行
var removalPlan = FindFirstObjectByType<SingletonRemovalPlan>();
removalPlan.EmergencyRollback();

// 2. FeatureFlagsを安全状態に戻す
FeatureFlags.DisableLegacySingletons = false;
FeatureFlags.EnableMigrationWarnings = true;
FeatureFlags.EnableMigrationMonitoring = true;

// 3. バックアップからコード復旧
// （手動でバックアップからコードを復元）
```

## ✅ 完了後の状態

### 最終的なクラス構成例

```csharp
// AudioManager.cs (削除後)
public class AudioManager : MonoBehaviour, IAudioService
{
    // ❌ Singleton関連は全削除
    // private static AudioManager instance;      // 削除
    // public static AudioManager Instance {...}  // 削除
    
    // ✅ ServiceLocator登録は保持
    private void Awake()
    {
        // instance = this;  // 削除
        DontDestroyOnLoad(gameObject);
        
        ServiceLocator.RegisterService<IAudioService>(this);
        InitializeAudioSystem();
    }
    
    private void OnDestroy()
    {
        ServiceLocator.UnregisterService<IAudioService>();
        // instance = null;  // 削除
    }
    
    // ✅ IAudioService実装は完全保持
    public void PlaySound(string soundId, Vector3 position = default, float volume = 1f)
    {
        // 実装内容は変更なし
    }
    
    // ... 他のインターフェースメソッド
}
```

## 📊 削除効果

- **コード簡素化**: Singleton関連コード約30-50行削除
- **保守性向上**: Instance プロパティ管理不要
- **パフォーマンス**: 軽微な向上（Singleton チェック処理削除）
- **アーキテクチャ**: 純粋なServiceLocatorパターンに移行完了

## ⚠️ 重要な注意事項

1. **段階的実行**: 全クラス一度に削除せず、1クラスずつ削除・テスト・確認
2. **バックアップ必須**: 各削除前に必ずバックアップ作成
3. **ServiceLocator保持**: 削除対象はSingleton関連のみ
4. **インターフェース完全保持**: IService実装は絶対変更しない
5. **緊急ロールバック準備**: 問題発生時の即座復旧手順確保

これで完全にSingletonパターンから脱却し、純粋なServiceLocatorベースのアーキテクチャに移行できます。
