# Phase 3 Singleton移行 - 残作業リスト

**作成日時**: 2025年1月10日  
**プロジェクト**: URP3D_Base01 - Unity 6 3Dゲーム基盤プロジェクト  
**進捗状況**: 約60%完了 (Week 1-2 完了、Week 3-4 が残作業)

---

## 📊 現在の実装状況

### ✅ 完了済み部分

#### Week 1: 移行準備とテスト基盤構築
- ✅ **FeatureFlags強化版**実装完了
- ✅ **MigrationMonitor**実装完了  
- ✅ **MigrationValidator**実装完了
- ✅ **MigrationTests**実装完了

#### Week 2: StealthAudioCoordinator完全移行
- ✅ **StealthAudioCoordinator ServiceLocator登録**完了
- ✅ **StealthAudioService新サービス**実装完了

#### Week 4基盤: Singleton完全廃止システム
- ✅ **EmergencyRollback**システム実装完了

#### Phase 4基盤: 品質保証と最適化
- ✅ **パフォーマンステストスイート**実装済み

---

## 🎯 優先順位付き残作業リスト

### 🔴 最優先 (Week 3の完了)

#### Task 1: Step 3.7 - FeatureFlagsの段階的有効化
**ファイル**: `Assets/_Project/Core/FeatureFlags.cs`  
**作業内容**:
```csharp
// 現在の状態を以下に変更
FeatureFlags.UseNewAudioService = true;     // ❌→✅ 有効化
FeatureFlags.UseNewSpatialService = true;   // ❌→✅ 有効化  
FeatureFlags.UseNewStealthService = true;   // ❌→✅ 有効化
```
**検証方法**: MigrationValidatorを実行し、全サービスが登録されていることを確認

#### Task 2: Step 3.6 - 主要利用箇所の段階的移行実装
**対象ファイル** (最低5ファイル以上):
- `Assets/_Project/Features/Player/Scripts/PlayerController.cs` (新規作成または修正)
- `Assets/_Project/Features/UI/Scripts/AudioSettingsUI.cs` (新規作成または修正)
- その他のSingleton利用箇所を特定・移行

**実装パターン**:
```csharp
// 従来の方法 (削除対象)
audioService = AudioManager.Instance;

// 新しい方法 (実装目標)
audioService = ServiceLocator.GetService<IAudioService>();
```

**検証方法**: 
- 機能的互換性100%確保
- パフォーマンス劣化なし確認

---

### 🟡 高優先 (Week 4の開始)

#### Task 3: Step 3.9 - Legacy Singleton警告システム追加
**対象ファイル**:
- `Assets/_Project/Core/Audio/AudioManager.cs`
- `Assets/_Project/Core/Audio/SpatialAudioManager.cs`
- その他のSingletonクラス

**実装内容**: 各クラスのInstanceプロパティに警告システム追加
```csharp
/// <summary>
/// 後方互換性のためのInstance（非推奨）
/// ServiceLocator.GetService<IAudioService>()を使用してください
/// </summary>
[System.Obsolete("Use ServiceLocator.GetService<IAudioService>() instead")]
public static AudioManager Instance 
{
    get 
    {
        // Legacy Singleton完全無効化フラグの確認
        if (FeatureFlags.DisableLegacySingletons) 
        {
            EventLogger.LogError("[DEPRECATED] AudioManager.Instance is disabled...");
            return null;
        }
        
        // 移行警告の表示
        if (FeatureFlags.EnableMigrationWarnings) 
        {
            EventLogger.LogWarning("[DEPRECATED] AudioManager.Instance usage detected...");
            // MigrationMonitorに使用状況を記録
        }
        
        return instance;
    }
}
```

#### Task 4: Step 3.10 - DisableLegacySingletons段階的有効化
**スケジュール**:
1. **Day 1**: テスト環境で警告システム有効化
2. **Day 2-3**: 警告が表示される箇所を特定・修正
3. **Day 4**: 本番環境でSingleton段階的無効化
   ```csharp
   FeatureFlags.DisableLegacySingletons = true;   // ✅ Singleton無効化
   ```

---

### 🟢 最終段階 (Week 4の完了)

#### Task 5: Step 3.11 - Singletonコードの物理的削除
**作業内容**: 各Singletonクラスから以下コードを削除
```csharp
// ❌ 削除対象
private static AudioManager instance;
public static AudioManager Instance => instance;

// Awake()内のSingleton制御コード
if (instance != null && instance != this) 
{
    Destroy(gameObject);
    return;
}
instance = this;
DontDestroyOnLoad(gameObject);
```

**残すコード**: ServiceLocator専用実装のみ
```csharp
// ✅ ServiceLocator専用実装のみ残す
private void Awake()
{
    ServiceLocator.RegisterService<IAudioService>(this);
    
    if (FeatureFlags.EnableDebugLogging)
    {
        EventLogger.Log("[AudioManager] Registered to ServiceLocator as IAudioService");
    }
}
```

#### Task 6: Phase 4 - 残りの品質保証テスト完了
**対象**:
- メモリリークテストの完全実装
- 統合テストスイートの完全実装
- 本番環境での安定動作確認
- ドキュメント更新完了

---

## ⚠️ 重要な注意事項

### 安全な実行順序
1. **絶対に一度に全てを変更しない** - 段階的実行が必須
2. **各ステップで十分なテストを実施** - MigrationValidatorとMigrationMonitorを活用
3. **問題発生時の即座の対応** - EmergencyRollbackシステムを準備

### 検証方法
- **機能的互換性**: 既存機能が全て正常動作することを確認
- **パフォーマンス**: 劣化がないことを測定・確認
- **Singleton参照数**: MigrationMonitorで0箇所になることを確認

### 緊急時対応
```csharp
// 緊急ロールバック実行
EmergencyRollback.ExecuteEmergencyRollback("理由");

// 部分ロールバック
EmergencyRollback.RollbackSpecificService("audio", "理由");
```

---

## 📋 作業チェックリスト

### 最優先タスク
- [ ] **Task 1**: FeatureFlags段階的有効化
  - [ ] UseNewAudioService = true
  - [ ] UseNewSpatialService = true  
  - [ ] UseNewStealthService = true
  - [ ] MigrationValidatorで検証
  
- [ ] **Task 2**: 主要利用箇所の移行実装
  - [ ] PlayerController実装
  - [ ] AudioSettingsUI実装
  - [ ] その他3ファイル以上の移行
  - [ ] 機能的互換性100%確認
  - [ ] パフォーマンス劣化なし確認

### 高優先タスク  
- [ ] **Task 3**: Legacy Singleton警告システム
  - [ ] AudioManager.cs修正
  - [ ] SpatialAudioManager.cs修正
  - [ ] その他Singletonクラス修正
  
- [ ] **Task 4**: DisableLegacySingletons有効化
  - [ ] Day 1-2: 警告システムテスト
  - [ ] Day 2-3: 問題箇所修正
  - [ ] Day 4: Singleton無効化実行

### 最終段階タスク
- [ ] **Task 5**: Singletonコード物理削除
  - [ ] instance フィールド削除
  - [ ] Instance プロパティ削除
  - [ ] Singleton制御ロジック削除
  
- [ ] **Task 6**: Phase 4品質保証完了
  - [ ] メモリリークテスト完成
  - [ ] 統合テスト完成
  - [ ] 本番環境動作確認
  - [ ] ドキュメント更新

---

## 📊 成功指標

### 定量的目標
- **Singletonの使用数**: 現状8個 → 目標0個
- **ServiceLocator登録サービス数**: 現状3個 → 目標6個以上  
- **Singleton参照箇所**: MigrationMonitorで0箇所達成
- **パフォーマンス**: 劣化±5%以内維持

### 完了判定条件
1. ✅ 全FeatureFlags正しく設定済み
2. ✅ MigrationValidator全項目PASSED  
3. ✅ MigrationMonitor Singleton使用0件
4. ✅ パフォーマンステスト全項目合格
5. ✅ 緊急ロールバック不要な安定状態

**この作業リストに従って段階的に実装することで、安全にSingleton完全廃止を達成できます。**