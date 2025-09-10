# リファクタリング Quick Start - Week 1 実行計画

**開始日**: [記入してください]  
**デイリースタンドアップ**: 毎日 10:00  
**週次レビュー**: 金曜 15:00

## 🎯 Week 1 ゴール

**最重要目標**: 循環依存をゼロにする

---

## 📅 Day 1 (月曜日) - 準備とバックアップ

### 午前 (9:00-12:00)
```bash
# 1. バックアップブランチ作成
git checkout -b backup/pre-refactoring-2025-09
git push origin backup/pre-refactoring-2025-09

# 2. リファクタリングブランチ作成
git checkout -b refactor/phase1-architecture-cleanup
```

### 午後 (13:00-17:00)
- [ ] 既存テスト実行
  ```bash
  # Unity Test Runnerで全テスト実行
  # 結果を Tests/Results/baseline-test-results.xml に保存
  ```

- [ ] Feature Flag追加
  ```csharp
  // Core/FeatureFlags.cs に追加
  public static bool UseRefactoredArchitecture 
  {
      get => PlayerPrefs.GetInt("FeatureFlag_UseRefactoredArchitecture", 0) == 1;
      set => SetFlag("FeatureFlag_UseRefactoredArchitecture", value);
  }
  ```

### 成果物チェック
- [ ] バックアップブランチ作成済み
- [ ] リファクタリングブランチ作成済み
- [ ] ベースラインテスト結果保存済み
- [ ] Feature Flag動作確認済み

---

## 📅 Day 2 (火曜日) - ServiceHelper導入

### 午前 (9:00-12:00)
- [ ] `Core/Helpers/ServiceHelper.cs` 作成

```csharp
using UnityEngine;
using _Project.Core;

namespace asterivo.Unity60.Core.Helpers
{
    /// <summary>
    /// サービス取得の統一インターフェース
    /// DRY原則違反を解消し、サービス取得ロジックを一元化
    /// </summary>
    public static class ServiceHelper
    {
        /// <summary>
        /// ServiceLocatorを優先し、失敗時はFindFirstObjectByTypeにフォールバック
        /// </summary>
        public static T GetServiceWithFallback<T>() where T : class
        {
            // ServiceLocator使用（推奨）
            if (FeatureFlags.UseServiceLocator)
            {
                var service = ServiceLocator.GetService<T>();
                if (service != null) 
                {
                    LogServiceAcquisition<T>("ServiceLocator");
                    return service;
                }
            }
            
            // フォールバック: Unity標準検索
            if (typeof(T).IsSubclassOf(typeof(UnityEngine.Object)))
            {
                var unityObject = UnityEngine.Object.FindFirstObjectByType(typeof(T)) as T;
                if (unityObject != null)
                {
                    LogServiceAcquisition<T>("FindFirstObjectByType");
                }
                return unityObject;
            }
            
            LogServiceNotFound<T>();
            return null;
        }
        
        private static void LogServiceAcquisition<T>(string method)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (FeatureFlags.EnableDebugLogging)
            {
                Debug.Log($"[ServiceHelper] {typeof(T).Name} acquired via {method}");
            }
            #endif
        }
        
        private static void LogServiceNotFound<T>()
        {
            Debug.LogWarning($"[ServiceHelper] Failed to acquire service: {typeof(T).Name}");
        }
    }
}
```

### 午後 (13:00-17:00)
- [ ] ServiceHelperのユニットテスト作成
- [ ] 最初の3ファイルでFindFirstObjectByTypeを置換
  1. `Core/Audio/AudioUpdateCoordinator.cs`
  2. `Core/Audio/StealthAudioCoordinator.cs`
  3. `Core/Audio/AudioManagerAdapter.cs`

---

## 📅 Day 3 (水曜日) - 名前空間統一開始

### 午前 (9:00-12:00)
- [ ] 名前空間規約文書作成: `Docs/Namespace_Convention.md`

```markdown
# 名前空間規約

## 基本規則
- Root: `asterivo.Unity60`
- Core機能: `asterivo.Unity60.Core.*`
- 機能実装: `asterivo.Unity60.Features.*`
- テスト: `asterivo.Unity60.Tests.*`

## 禁止事項
- Core層からFeatures層への参照禁止
- _Project.* の新規使用禁止（段階的削除）

## 移行計画
1. 新規コードは新規約に従う
2. 既存コードは修正時に更新
3. 最終的に_Project.*を完全削除
```

### 午後 (13:00-17:00)
- [ ] Core/Audio配下の名前空間統一（6ファイル）
  - 旧: `namespace _Project.Core`
  - 新: `namespace asterivo.Unity60.Core.Audio`

---

## 📅 Day 4 (木曜日) - 循環依存解消

### 午前 (9:00-12:00)
- [ ] Core/Interfaces/ ディレクトリ作成
- [ ] 基本インターフェース作成
  ```csharp
  // Core/Interfaces/IGameSystem.cs
  namespace asterivo.Unity60.Core.Interfaces
  {
      public interface IGameSystem
      {
          void Initialize();
          void Shutdown();
          bool IsInitialized { get; }
      }
  }
  ```

### 午後 (13:00-17:00)
- [ ] Core層から_Project.Features参照を削除（優先度高の5ファイル）
- [ ] ビルドエラー解消
- [ ] 動作確認テスト

---

## 📅 Day 5 (金曜日) - 仕上げとレビュー

### 午前 (9:00-12:00)
- [ ] 残りのFindFirstObjectByType置換（7箇所）
- [ ] マジックナンバーの定数化
  ```csharp
  // Core/Constants/GameConstants.cs
  public static class GameConstants
  {
      // CommandInvokerEditor.cs から抽出
      public const int TEST_HEAL_SMALL = 10;
      public const int TEST_HEAL_LARGE = 25;
      public const int TEST_DAMAGE_SMALL = 10;
      public const int TEST_DAMAGE_LARGE = 25;
  }
  ```

### 午後 (13:00-15:00)
- [ ] 全テスト実行
- [ ] パフォーマンス測定
- [ ] コミット準備

### 週次レビュー (15:00-17:00)
- [ ] 達成項目の確認
- [ ] 問題点の洗い出し
- [ ] Week 2計画の調整

---

## ✅ Week 1 完了チェックリスト

### 必須達成項目
- [ ] 循環依存: 16 → 0
- [ ] ServiceHelper導入完了
- [ ] FindFirstObjectByType使用: 20 → 10以下
- [ ] 全テストパス
- [ ] ドキュメント更新

### 成果物
- [ ] `Core/Helpers/ServiceHelper.cs`
- [ ] `Core/Interfaces/IGameSystem.cs`
- [ ] `Core/Constants/GameConstants.cs`
- [ ] `Docs/Namespace_Convention.md`
- [ ] テスト結果レポート

### メトリクス記録
| 指標 | 開始時 | 終了時 | 目標 |
|------|--------|--------|------|
| 循環依存数 | 16 | [記録] | 0 |
| FindFirstObjectByType | 20+ | [記録] | 10 |
| ビルド時間 | [記録] | [記録] | -10% |
| テストパス率 | [記録] | [記録] | 100% |

---

## 🚨 トラブルシューティング

### よくある問題と対処法

**Q: 名前空間変更後にビルドエラー**
```bash
# すべてのusing文を一括更新
find . -name "*.cs" -exec sed -i 's/using _Project\.Core/using asterivo.Unity60.Core/g' {} \;
```

**Q: ServiceHelperが見つからない**
```csharp
// 一時的な対処
#if USE_SERVICE_HELPER
    var service = ServiceHelper.GetServiceWithFallback<IAudioService>();
#else
    var service = FindFirstObjectByType<AudioManager>();
#endif
```

**Q: テストが失敗する**
```csharp
// Feature Flagで切り替え
if (FeatureFlags.UseRefactoredArchitecture)
{
    // 新実装
}
else
{
    // 旧実装（フォールバック）
}
```

---

## 📞 エスカレーション

問題が発生した場合の連絡先:
1. Slackチャンネル: #refactoring-support
2. リードエンジニア: [名前]
3. アーキテクト: [名前]

---

## 💡 Tips

1. **コミットは細かく**: 各ファイルの変更ごとにコミット
2. **テストファースト**: 変更前に必ずテストを実行
3. **ペアプロ推奨**: 複雑な箇所は2人で作業
4. **15分ルール**: 15分悩んだら相談

---

**がんばりましょう！** 🚀