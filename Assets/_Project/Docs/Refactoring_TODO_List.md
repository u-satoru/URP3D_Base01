# リファクタリングTODOリスト

**作成日**: 2025年9月10日  
**基準文書**: Architecture_Validation_Report.md, AntiPattern_Deep_Analysis_Report.md  
**総見積時間**: 約6-8週間（段階的実施可能）

## 🎯 成功指標

- [ ] アーキテクチャ健全性スコア: 45/100 → 80/100
- [ ] 循環依存: 16+ → 0
- [ ] God Object: 3 → 0  
- [ ] FindFirstObjectByType使用: 20+ → 5以下
- [ ] テストカバレッジ: 不明 → 80%以上

---

## 📋 Phase 0: 準備作業（2日間）

### 0.1 現状把握とバックアップ
- [ ] **[P0]** 現在のコードベースの完全バックアップ作成
  - 担当: リードエンジニア
  - 期限: Day 1
  - ブランチ: `backup/pre-refactoring-2025-09`
  
- [ ] **[P0]** 既存テストの実行と記録
  - 依存: 0.1完了後
  - 期限: Day 1
  - 成果物: `Tests/Results/baseline-test-results.xml`

### 0.2 リファクタリング環境整備
- [ ] **[P0]** リファクタリング用ブランチ作成
  - ブランチ名: `refactor/phase1-architecture-cleanup`
  - 期限: Day 1
  
- [ ] **[P0]** Feature Flagの設定
  ```csharp
  FeatureFlags.UseRefactoredArchitecture = false; // 段階的有効化用
  ```
  - 期限: Day 2
  - ファイル: `Core/FeatureFlags.cs`

- [ ] **[P1]** 静的解析ツールのセットアップ
  - Unity Code Analysis
  - Roslyn Analyzers
  - 期限: Day 2

---

## 🚨 Phase 1: 緊急対応（1週間）

### 1.1 循環依存の解消 🔴

#### 1.1.1 名前空間の統一
- [ ] **[P0]** 名前空間規約文書の作成
  - 期限: Day 3
  - 成果物: `Docs/Namespace_Convention.md`
  ```
  asterivo.Unity60.Core.*     // コア機能
  asterivo.Unity60.Features.* // 機能実装
  asterivo.Unity60.Tests.*    // テスト
  ```

- [ ] **[P0]** Core層から_Project.Features参照の削除（16ファイル）
  - 期限: Day 4-5
  - 対象ファイル:
    - [ ] `Core/Audio/AudioManager.cs`
    - [ ] `Core/Audio/EffectManager.cs`
    - [ ] `Core/Audio/SpatialAudioManager.cs`
    - [ ] `Core/Audio/Services/*.cs` (6ファイル)
    - [ ] `Core/Services/*.cs` (7ファイル)
  - 作業内容: `using _Project.Features` を削除

- [ ] **[P0]** インターフェース層の導入
  - 期限: Day 5
  - 新規作成: `Core/Interfaces/`
    - [ ] `IGameSystem.cs`
    - [ ] `IFeatureModule.cs`
    - [ ] `IServiceProvider.cs`

#### 1.1.2 依存方向の修正
- [ ] **[P1]** Features→Core単方向依存の確立
  - 期限: Day 6-7
  - 検証スクリプト作成: `Tools/DependencyChecker.cs`
  - CI/CDパイプラインへの組み込み

### 1.2 ServiceHelperの導入 🟠

- [ ] **[P0]** ServiceHelper.csの作成
  - 期限: Day 3
  - ファイル: `Core/Helpers/ServiceHelper.cs`
  ```csharp
  public static class ServiceHelper
  {
      public static T GetServiceWithFallback<T>() where T : class
      {
          // 統一されたサービス取得ロジック
      }
  }
  ```

- [ ] **[P1]** FindFirstObjectByType の置き換え（20箇所）
  - 期限: Day 4-7
  - 対象:
    - [ ] AudioUpdateCoordinator.cs (6箇所)
    - [ ] StealthAudioCoordinator.cs (3箇所)
    - [ ] AudioManagerAdapter.cs (2箇所)
    - [ ] CameraCommandManager.cs (2箇所)
    - [ ] その他 (7箇所)

### 1.3 定数化とマジックナンバー削除 🟡

- [ ] **[P2]** GameConstants.csの作成
  - 期限: Day 5
  - ファイル: `Core/Constants/GameConstants.cs`
  ```csharp
  public static class GameConstants
  {
      public const int DEFAULT_HEAL_AMOUNT = 10;
      public const int DEFAULT_DAMAGE_AMOUNT = 25;
      public const float MIN_LOADING_TIME = 1f;
  }
  ```

- [ ] **[P2]** AudioCategoryEnum の作成
  - 期限: Day 5
  - ファイル: `Core/Audio/AudioCategory.cs`
  ```csharp
  public enum AudioCategory
  {
      BGM,
      Ambient,
      Effect,
      SFX,
      Stealth
  }
  ```

---

## 🔨 Phase 2: リファクタリング（2-3週間）

### 2.1 God Object の分割 🔴

#### 2.1.1 GameManager分割（Week 2）
- [ ] **[P1]** GameStateManager.cs作成
  - 見積: 4時間
  - 責任: ゲーム状態管理のみ
  - テスト: GameStateManagerTests.cs
  
- [ ] **[P1]** GameScoreManager.cs作成
  - 見積: 2時間
  - 責任: スコア管理のみ
  - テスト: GameScoreManagerTests.cs
  
- [ ] **[P1]** GameLifeManager.cs作成
  - 見積: 2時間
  - 責任: ライフ管理のみ
  - テスト: GameLifeManagerTests.cs
  
- [ ] **[P1]** GamePauseManager.cs作成
  - 見積: 2時間
  - 責任: ポーズ管理のみ
  - テスト: GamePauseManagerTests.cs
  
- [ ] **[P1]** GameSceneLoader.cs作成
  - 見積: 3時間
  - 責任: シーン管理のみ
  - テスト: GameSceneLoaderTests.cs
  
- [ ] **[P1]** GameInputHandler.cs作成
  - 見積: 3時間
  - 責任: 入力処理のみ
  - テスト: GameInputHandlerTests.cs
  
- [ ] **[P1]** GameCommandProcessor.cs作成
  - 見積: 4時間
  - 責任: コマンド処理のみ
  - テスト: GameCommandProcessorTests.cs
  
- [ ] **[P1]** GameEventCoordinator.cs作成
  - 見積: 4時間
  - 責任: イベント調整のみ
  - テスト: GameEventCoordinatorTests.cs

- [ ] **[P1]** GameManagerFacade.cs作成
  - 見積: 4時間
  - 役割: 分割されたマネージャーの統合インターフェース
  - パターン: Facade Pattern

#### 2.1.2 PlayerController分割（Week 3）
- [ ] **[P1]** PlayerInputHandler.cs作成
  - 見積: 4時間
  - 責任: 入力処理のみ
  
- [ ] **[P1]** PlayerAnimationController.cs作成
  - 見積: 3時間
  - 責任: アニメーション制御のみ
  
- [ ] **[P1]** PlayerAudioIntegration.cs作成
  - 見積: 3時間
  - 責任: オーディオ統合のみ
  
- [ ] **[P1]** PlayerPhysicsController.cs作成
  - 見積: 2時間
  - 責任: 物理演算のみ
  
- [ ] **[P1]** PlayerEventManager.cs作成
  - 見積: 2時間
  - 責任: イベント管理のみ

- [ ] **[P1]** PlayerControllerRefactored.cs作成
  - 見積: 4時間
  - 役割: 分割されたコンポーネントの調整役

### 2.2 レガシーコードの段階的削除

#### 2.2.1 Obsolete属性の追加（Week 2）
- [ ] **[P2]** レガシーSingletonにObsolete属性追加
  ```csharp
  [Obsolete("Will be removed in v2.0. Use ServiceLocator instead.", false)]
  ```
  - 対象:
    - [ ] AudioManager.Instance
    - [ ] GameManager.Instance（存在する場合）
    - [ ] その他のSingletonパターン

#### 2.2.2 移行パスの文書化
- [ ] **[P2]** Migration_Guide.md作成
  - 見積: 4時間
  - 内容: 新旧APIの対応表
  - 例: コード変換例

### 2.3 テスト基盤の構築

- [ ] **[P1]** 単体テストの追加（各分割クラス）
  - 見積: 各2時間 × 13クラス = 26時間
  - カバレッジ目標: 80%以上
  
- [ ] **[P1]** 統合テストの作成
  - 見積: 8時間
  - 対象: 分割後のシステム間連携

---

## ⚡ Phase 3: 最適化とテスト強化（2-3週間）

### 3.1 パフォーマンス最適化（Week 4）

- [ ] **[P2]** Update()処理のイベント駆動化
  - 対象: PlayerController.Update()
  - 見積: 4時間
  - 手法: 状態変更時のみ更新

- [ ] **[P2]** オブジェクトプールの拡張
  - 対象: UI要素、エフェクト
  - 見積: 6時間
  - 目標: メモリ使用量30%削減

- [ ] **[P2]** プロファイリングとベンチマーク
  - Unity Profiler設定
  - 見積: 4時間
  - 成果物: Performance_Report.md

### 3.2 コード品質向上（Week 5）

- [ ] **[P2]** コードレビューチェックリスト作成
  - 見積: 2時間
  - 成果物: Code_Review_Checklist.md

- [ ] **[P2]** 静的解析ルールの設定
  - .editorconfig作成
  - StyleCop設定
  - 見積: 3時間

- [ ] **[P3]** TODOコメントの解消（10箇所）
  - 優先度低のため、余裕があれば実施
  - 見積: 各1時間

### 3.3 ドキュメント整備

- [ ] **[P2]** アーキテクチャドキュメント更新
  - 見積: 4時間
  - 内容: 新アーキテクチャの図解

- [ ] **[P2]** APIドキュメント生成
  - DocFXまたはSandcastle設定
  - 見積: 3時間

---

## 🚀 Phase 4: 長期改善（1ヶ月以降）

### 4.1 アーキテクチャ継続改善

- [ ] **[P3]** Service Locatorの性能最適化
  - 対象: 現在のServiceLocatorクラス
  - 見積: 1週間（プロファイリング・最適化）
  - 成果物: ServiceLocator_Performance_Report.md
  
- [ ] **[P3]** イベント駆動パターンの強化
  - 対象: GameEventシステムの拡張
  - 見積: 2週間
  - 成果物: Enhanced_Event_System.md

### 4.2 既存パターンの完成度向上

- [ ] **[P3]** ObjectPoolパターンの拡張
  - 対象: コマンドプール以外のゲームオブジェクト
  - 見積: 2週間
  - 目標: 既存95%メモリ削減効果の維持・向上

- [ ] **[P3]** ScriptableObjectベースデータ管理の強化
  - 対象: ゲームバランス調整システム
  - 見積: 2週間
  - 成果物: Enhanced_Data_Management.md

- [ ] **[P3]** Service Locator + Event駆動ハイブリッドの最適化
  - Unity特化アーキテクチャの完成
  - 見積: 3週間
  - 成果物: Hybrid_Architecture_Guide.md

---

## 📊 進捗管理

### 週次チェックポイント

#### Week 1 完了条件
- [ ] 循環依存ゼロ達成
- [ ] ServiceHelper導入完了
- [ ] 基本的な定数化完了

#### Week 2-3 完了条件
- [ ] GameManager分割完了
- [ ] PlayerController分割完了
- [ ] 単体テスト追加（カバレッジ50%以上）

#### Week 4-5 完了条件
- [ ] パフォーマンス最適化完了
- [ ] テストカバレッジ80%達成
- [ ] ドキュメント更新完了

### リスク管理

| リスク | 対策 | 担当 |
|--------|------|------|
| 機能破壊 | Feature Flag使用、段階的リリース | QAチーム |
| スケジュール遅延 | 優先度による取捨選択 | PM |
| パフォーマンス劣化 | 継続的プロファイリング | パフォーマンスエンジニア |

---

## 🎯 完了定義（Definition of Done）

各タスクの完了条件:
1. ✅ コードレビュー通過
2. ✅ 単体テスト作成・通過
3. ✅ 統合テスト通過
4. ✅ ドキュメント更新
5. ✅ パフォーマンス基準クリア

---

## 📝 注記

- このTODOリストは週次で見直しを行う
- 優先度は状況に応じて調整可能
- 各フェーズは並行実施可能な部分もある
- チーム規模によって期間は調整が必要

---

**最終更新**: 2025年9月10日  
**次回レビュー**: 2025年9月17日  
**承認者**: [UECHI,Satoru]  
**実施チーム**: [開発チーム名]