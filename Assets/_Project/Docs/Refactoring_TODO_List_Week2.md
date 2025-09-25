# Week 2 TODO リスト - リファクタリング継続とPhase 2準備

**作成日**: 2025年9月11日  
**基準文書**: Week 1実行結果, Refactoring_TODO_List.md  
**週目標**: テストインフラ構築完了とPhase 2移行準備

## 🎯 Week 2 ゴール

**最重要目標**: テストインフラ構築完了とPhase 2開始準備  
**副次目標**: Phase 1残タスク完了とGod Object分割開始

---

## 📋 Phase 1 継続タスク（最優先）

### 🔴 P0: テストインフラ構築（Day 1-2）

#### テスト基盤整備
- [ ] **[P0]** Unity Test Runner環境の最適化
  - 期限: Day 1 午前
  - 内容: Test Assemblyの設定確認
  - 成果物: `Tests/Assembly-Test.asmdef` 設定完了

- [ ] **[P0]** 基本テストテンプレートの作成
  - 期限: Day 1 午後
  - 内容: 単体テスト・統合テストのテンプレート作成
  - 成果物: `Tests/Templates/` ディレクトリ
  ```csharp
  // UnitTestTemplate.cs
  // IntegrationTestTemplate.cs
  // MockServiceTemplate.cs
  ```
  - レポート生成: XML + Markdown両形式対応
  - 出力形式: NUnit標準XML（CI/CD用） + 分析Markdown（人間可読用）

- [ ] **[P0]** テストヘルパークラスの作成
  - 期限: Day 2 午前
  - 内容: モック作成、アサーション支援機能
  - 成果物: `Tests/Helpers/TestHelpers.cs`

#### 既存システムテストの拡充
- [ ] **[P0]** Core/Audio システムテスト作成
  - 期限: Day 2 午後
  - 対象: AudioManager, SpatialAudioManager, EffectManager
  - 成果物: `Tests/Core/Audio/` 配下のテストファイル
  - テスト結果出力: 
    - `Tests/Results/audio-system-test-results.xml` (NUnit形式)
    - `Tests/Results/audio-system-verification.md` (分析レポート形式)

### 🔴 P0: FindFirstObjectByType全体置換（Day 2-3）

- [ ] **[P0]** 残り17箇所のFindFirstObjectByType置換
  - 期限: Day 3 完了
  - 対象ファイル特定済み:
    - [ ] CameraCommandManager.cs (2箇所)
    - [ ] その他15箇所の特定と置換
  - 手法: ServiceHelper.GetServiceWithFallback<T>() に統一

- [ ] **[P0]** 置換結果の動作検証
  - 期限: Day 3 午後
  - 内容: Unity Editor実行テスト
  - 確認項目: サービス取得の正常動作

---

## 🟡 Phase 1 完了タスク（中優先）

### 静的解析環境整備（Day 3-4）

- [ ] **[P1]** Unity Code Analysis セットアップ
  - 期限: Day 3
  - 内容: Unity標準コード解析の有効化
  - 設定: Project Settings > XR Plug-in Management

- [ ] **[P1]** Roslyn Analyzers 導入
  - 期限: Day 4 午前
  - 内容: .NET 標準アナライザーの追加
  - 成果物: `Directory.Build.props` 設定

- [ ] **[P1]** .editorconfig 作成
  - 期限: Day 4 午前
  - 内容: コーディング規約の統一
  - 成果物: `.editorconfig` ファイル

### 依存関係検証の自動化（Day 4）

- [ ] **[P1]** 依存方向検証スクリプト作成
  - 期限: Day 4 午後
  - ファイル: `Tools/DependencyChecker.cs`
  - 機能: Core→Features参照の自動検出
  ```csharp
  public static class DependencyChecker
  {
      public static bool ValidateArchitecture()
      {
          // Core層からFeatures層への参照チェック
      }
  }
  ```

- [ ] **[P1]** CI/CD 統合準備
  - 期限: Day 4 完了
  - 内容: GitHub Actions用スクリプト準備
  - 成果物: `.github/workflows/dependency-check.yml`

### インターフェース層整備（Day 5）

- [ ] **[P1]** Core/Interfaces/ ディレクトリ作成
  - 期限: Day 5 午前
  - 成果物: 基本インターフェースファイル
    - [ ] `IGameSystem.cs`
    - [ ] `IFeatureModule.cs` 
    - [ ] `IServiceProvider.cs`

---

## 🚀 Phase 2 開始準備（Day 5）

### GameManager分割準備

- [ ] **[P1]** GameManager現状分析
  - 期限: Day 5 午前
  - 内容: 責任範囲の特定と分割計画策定
  - 成果物: `GameManager_Analysis.md`

- [ ] **[P1]** 分割クラス設計書作成
  - 期限: Day 5 午後
  - 対象: GameStateManager, GameScoreManager 等
  - 成果物: `Phase2_GameManager_Design.md`

### Obsolete属性追加準備

- [ ] **[P2]** レガシーSingleton特定
  - 期限: Day 5 完了
  - 対象: AudioManager.Instance, GameManager.Instance等
  - 成果物: `Legacy_Singleton_List.md`

---

## 📊 Week 2 完了条件

### 必須達成項目
- [ ] テストインフラ構築完了（Unity Test Runner + テンプレート）
- [ ] FindFirstObjectByType置換完了（20箇所 → 0箇所）
- [ ] 静的解析環境整備完了
- [ ] 依存方向検証自動化完了

### Phase 2 移行条件
- [ ] GameManager分割設計完了
- [ ] 基本テストカバレッジ30%以上達成
- [ ] CI/CD基盤準備完了

### 成果物リスト
- [ ] `Tests/Templates/` - テストテンプレート集
- [ ] `Tests/Helpers/TestHelpers.cs` - テストヘルパー
- [ ] `Tools/DependencyChecker.cs` - 依存関係検証ツール
- [ ] `.editorconfig` - コーディング規約
- [ ] `Core/Interfaces/` - 基本インターフェース群
- [ ] `Phase2_GameManager_Design.md` - Phase 2設計書

---

## 🚨 リスク管理

### 高リスク項目
| リスク | 影響度 | 対策 |
|--------|--------|------|
| テストインフラ構築遅延 | 高 | Day 1-2に集中投入、必要に応じて外部支援 |
| FindFirstObjectByType置換の回帰バグ | 中 | 段階的置換、各置換後の動作確認 |
| 静的解析ツール導入の学習コスト | 中 | 基本設定のみ先行、詳細は後日調整 |

### 緊急時対応
- テストインフラ構築が遅延した場合: Phase 2開始を1週間延期
- FindFirstObjectByType置換で問題発生時: Feature Flagによるロールバック

---

## 💡 効率化Tips

### 並行作業推奨
- Day 1: テスト基盤整備 + FindFirstObjectByType箇所特定
- Day 2: テストヘルパー作成 + FindFirstObjectByType置換開始
- Day 3-4: 静的解析環境整備 + 依存関係検証並行
- Day 5: Phase 2準備 + インターフェース整備並行

### 品質確保
- 各Day終了時: 必ず動作確認実施
- Day 3, 5: 中間レビュー実施
- 全置換完了時: 包括的回帰テスト実施

---

## 📞 エスカレーション

### 判断基準
- 2時間以上の技術的ブロック → 即座にエスカレーション
- スケジュール1日遅延 → チームリーダーに報告
- 品質問題発生 → 即座に作業停止・原因調査

### 連絡先
1. 技術課題: [UECHI,Satoru] - Slack #refactoring-support
2. スケジュール課題: プロジェクトマネージャー
3. 品質課題: QAチーム + [UECHI,Satoru]

---

**Week 2 成功への道筋**: テストインフラ → 全体置換完了 → Phase 2移行準備完了  
**がんばりましょう！** 🚀

---

**最終更新**: 2025年9月11日  
**レビュー予定**: 2025年9月13日（中間）、2025年9月15日（完了時）  
**承認者**: [UECHI,Satoru]  
**実施チーム**: [開発チーム名]
