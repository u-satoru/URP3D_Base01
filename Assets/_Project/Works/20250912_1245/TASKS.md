# タスクリスト - アーキテクチャ準拠性向上

**作成日**: 2025-09-12  
**基準レポート**: Architecture_Compliance_Verification_Report.md  
**目標**: プロジェクト準拠率 94.7% → 100%  

## 🔥 高優先度タスク（今週中完了目標）

### Task 1: namespace統一作業
**目標**: `_Project.*` → `asterivo.Unity60.*` 完全移行  
**準拠率向上**: 93% → 100%  
**作業量**: 1-2時間  
**リスク**: 低  

#### 1.1 namespace定義修正 (7ファイル)
- [ ] `Assets/_Project/Tests/Core/Services/GradualUpdatePatternTest.cs`
  - `namespace _Project.Tests.Core.Services` → `namespace asterivo.Unity60.Tests.Core.Services`
- [ ] `Assets/_Project/Features/Player/Scripts/PlayerStealthController.cs`  
  - `namespace _Project.Features.Player.Scripts` → `namespace asterivo.Unity60.Features.Player.Scripts`
- [ ] `Assets/_Project/Tests/Core/Services/StealthAudioServiceTest.cs`
- [ ] `Assets/_Project/Tests/Core/Services/StealthAudioCoordinatorServiceLocatorTest.cs`
- [ ] `Assets/_Project/Tests/Core/Services/MigrationValidatorTest.cs`
- [ ] `Assets/_Project/Core/SystemInitializer.cs`
- [ ] `Assets/_Project/Core/Lifecycle/IServiceLocatorRegistrable.cs`

#### 1.2 using文修正 (10ファイル)
- [ ] `Assets/_Project/Core/Helpers/ServiceHelper.cs`
- [ ] `Assets/_Project/Tests/Runtime/ProductionValidationTests.cs`
- [ ] `Assets/_Project/Features/Player/Scripts/PlayerStealthController.cs`
- [ ] `Assets/_Project/Tests/Performance/ServiceLocatorStressTests.cs`
- [ ] その他6ファイル

#### 1.3 検証・テスト
- [ ] コンパイル確認
- [ ] ユニットテスト実行
- [ ] エディタ動作確認

## 🟡 中優先度タスク（来週中完了目標）

### Task 2: GameObject.Find()最適化
**目標**: パフォーマンス向上とアーキテクチャ準拠  
**対象**: 8ファイル  
**作業量**: 2-4時間  
**リスク**: 中  

#### 2.1 UI系ファイル→直接参照化
- [ ] **HUDManager.cs**
  - `GameObject.Find("Canvas/HealthBar")` → `[SerializeField] private HealthBarUI healthBar;`
  - Inspector設定必要
  - 実行時エラーハンドリング追加

- [ ] **NPCVisualSensor.cs**
  - `GameObject.Find("Player")` → `[SerializeField] private Transform playerTransform;`
  - シーン設定更新必要

#### 2.2 サービス系ファイル→ServiceLocator化
- [ ] **StealthAudioService.cs**
  - AudioManager検索 → `ServiceLocator.GetService<IAudioService>()`
  - 初期化タイミング調整

- [ ] **StealthAudioCoordinator.cs**
  - 複数サービス検索 → ServiceLocator経由
  - エラーハンドリング強化

#### 2.3 残り4ファイル分析・対応
- [ ] 各ファイルの使用パターン分析
- [ ] 最適な置き換え手法選定
- [ ] 実装・テスト

#### 2.4 パフォーマンステスト
- [ ] 最適化前後の性能測定
- [ ] メモリ使用量比較
- [ ] 結果ドキュメント化

## 🟢 低優先度タスク（継続実施）

### Task 3: 品質維持体制構築
**目標**: 再発防止と継続的品質向上  

#### 3.1 開発プロセス改善
- [ ] コードレビューチェックリスト更新
  - namespace規約チェック項目追加
  - GameObject.Find()使用禁止項目追加
- [ ] PR テンプレート更新

#### 3.2 自動化・CI/CD強化
- [ ] namespace規約チェックスクリプト作成
- [ ] GameObject.Find()検出スクリプト作成
- [ ] GitHub Actions統合

#### 3.3 ドキュメント整備
- [ ] アーキテクチャガイドライン更新
- [ ] 新人向けオンボーディング資料作成
- [ ] ベストプラクティス集作成

## 📊 進捗トラッキング

### 完了基準
- [ ] **Task 1**: 全ファイルが`asterivo.Unity60.*`namespace使用
- [ ] **Task 2**: GameObject.Find()使用ファイル数 8 → 0
- [ ] **Task 3**: 品質維持体制運用開始

### 成功指標
- 準拠率: 94.7% → 100%
- コンパイルエラー: 0件維持
- パフォーマンス: 初期化時間短縮
- 開発効率: レビュー指摘事項削減

## 🚨 リスク管理

### 高リスク事項
1. **namespace変更による参照エラー**
   - 軽減策: 段階的変更、テスト強化
2. **GameObject.Find()置き換えでの実行時エラー**
   - 軽減策: null チェック強化、フォールバック実装

### 品質担保
- 各Task完了時に動作確認必須
- 変更前後のパフォーマンス測定
- ユニットテスト・統合テスト実行

## 📅 スケジュール

```
Week 1 (今週):
├── Day 1-2: Task 1.1-1.2 (namespace統一)
├── Day 3: Task 1.3 (検証・テスト)
└── Day 4-5: Task 2準備・設計

Week 2 (来週):
├── Day 1-3: Task 2.1-2.2 (GameObject.Find()最適化)
├── Day 4: Task 2.3-2.4 (残りファイル・テスト)  
└── Day 5: Task 3開始

継続:
└── Task 3 (品質維持体制)
```

---
**更新履歴**: 
- 2025-09-12: 初回作成（Architecture_Compliance_Verification_Report.md基準）
