# Phase 3 移行準備ガイド - Feature層リファクタリング

## 作成日: 2025年9月22日

### エグゼクティブサマリー
Phase 2（Core層確立）が95%完了し、Phase 3（Feature層リファクタリング）への移行準備が整いました。本ガイドは、次フェーズへのスムーズな移行を支援します。

---

## Phase 2の成果まとめ

### 達成事項
- **Core層基盤の確立**: ServiceLocator、GameEvent、Commandシステムが安定稼働
- **3層アーキテクチャの実現**: Core ← Feature ← Templateの依存関係を確立
- **コンパイルエラー解消**: 200+エラーを完全解消
- **Unity 6対応**: 制限事項を理解し、回避策を文書化
- **名前空間統一**: `asterivo.Unity60.*`への統一完了

### 作成された主要ドキュメント
1. **Phase2_Completion_Report.md** - 初期完了報告
2. **Phase2_FileMovement_RiskAssessment.md** - ファイル移動リスク評価
3. **TestCoverage_Implementation_Guide.md** - カバレッジ測定ガイド
4. **Phase2_Final_Completion_Report.md** - 最終完了報告
5. **Unity6_Test_Execution_Guide.md** - Unity 6テスト実行ガイド

---

## Phase 3概要

### 目的
Feature層の各機能モジュールを整理し、相互依存関係を解消して、再利用可能で保守性の高い機能部品を実現する。

### スコープ
```
Assets/_Project/Features/
├── Player/           # プレイヤー機能
├── AI/              # AI機能
├── Camera/          # カメラ制御
├── UI/              # UI管理
├── Combat/          # 戦闘システム
├── GameManagement/  # ゲーム管理
├── StateManagement/ # 状態管理
└── ActionRPG/       # アクションRPG機能
```

---

## Phase 3実施計画

### ステップ1: 現状分析（1週目）
#### タスク
- [ ] Feature層の依存関係マップ作成
- [ ] 各モジュールの責務分析
- [ ] 相互参照の特定
- [ ] 問題箇所のリストアップ

#### 成果物
- **FeatureDependencyMap.md**: 依存関係の可視化
- **ModuleResponsibilities.md**: 各モジュールの責務定義
- **RefactoringPriorityList.md**: リファクタリング優先順位

### ステップ2: インターフェース設計（2週目）
#### タスク
- [ ] 各モジュール間のインターフェース定義
- [ ] Core層との連携インターフェース整理
- [ ] イベント通信の設計
- [ ] データフローの明確化

#### 成果物
- **FeatureInterfaceDesign.md**: インターフェース仕様書
- **EventCommunicationMap.md**: イベント通信マップ

### ステップ3: 段階的リファクタリング（3-4週目）
#### 優先順位
1. **高優先度**: Player、AI（ゲームプレイの基盤）
2. **中優先度**: Camera、UI（表示系）
3. **低優先度**: Combat、ActionRPG（特化機能）

#### 実施方法
- モジュール単位での段階的リファクタリング
- 各ステップでのコンパイル確認
- テスト実行による動作検証

### ステップ4: テストと検証（5週目）
#### タスク
- [ ] 各モジュールの単体テスト作成・更新
- [ ] 統合テストの実施
- [ ] パフォーマンス測定
- [ ] カバレッジ測定（Unity Editor内）

---

## 技術的考慮事項

### 依存関係ルール
- **厳守**: Feature層からCore層への参照のみ許可
- **禁止**: Feature層間の直接参照
- **推奨**: GameEventを介した疎結合通信

### 名前空間規約
```csharp
// 各Featureモジュール
namespace asterivo.Unity60.Features.Player { }
namespace asterivo.Unity60.Features.AI { }
namespace asterivo.Unity60.Features.Camera { }
namespace asterivo.Unity60.Features.UI { }
namespace asterivo.Unity60.Features.Combat { }
```

### Assembly Definition更新
各Featureモジュールの.asmdefファイル：
- Core層への参照を維持
- 他Feature層への参照を削除
- 必要に応じて新規作成

---

## リスクと対策

### 識別されたリスク
1. **Feature層間の密結合**
   - 現状: 一部モジュール間で直接参照あり
   - 対策: インターフェース経由またはイベント通信への移行

2. **大規模リファクタリングによる破壊**
   - 現状: 200+エラーを解消したばかり
   - 対策: 段階的実施、各ステップでの検証

3. **テストカバレッジの低下**
   - 現状: 測定方法は確立済み
   - 対策: リファクタリング前後での測定・比較

### 予防措置
- ブランチ戦略: `feature/phase3-feature-refactoring`の作成
- バックアップ: 各マイルストーンでの保存
- ロールバック計画: git履歴による即座の復元

---

## 成功基準

### 定量的指標
- コンパイルエラー: 0件を維持
- テスト成功率: 100%を維持
- カバレッジ: 70%以上（目標80%）
- パフォーマンス: 劣化なし

### 定性的指標
- Feature層の責務が明確に分離されている
- 新機能追加が既存コードに影響しない
- ドキュメントが完備されている
- チーム開発での並行作業が可能

---

## 推奨ツールと手法

### 開発ツール
- **Unity Editor**: バージョン6000.0.42f1を継続使用
- **Visual Studio/Rider**: リファクタリング支援機能を活用
- **Unity Test Runner**: 継続的なテスト実行
- **Unity Code Coverage**: カバレッジ測定（Editor内）

### 手法
- **TDD（テスト駆動開発）**: 新規インターフェースのテスト先行実装
- **リファクタリングカタログ**: Martin Fowlerの手法を参考
- **段階的移行**: Big Bang回避、Small Stepsアプローチ

---

## タイムライン

```
Week 1: 現状分析
Week 2: インターフェース設計
Week 3-4: 段階的リファクタリング
Week 5: テストと検証
Week 6: ドキュメント整備と完了
```

---

## 次のアクション

### 即座に実施
1. Phase 3用ブランチの作成
2. Feature層依存関係の初期調査
3. チームメンバーへの計画共有

### 今週中に実施
1. 依存関係マップの作成開始
2. 最初のモジュール（Player）の分析
3. リファクタリング環境の準備

---

## 参考資料

### Phase 2関連文書
- `Phase2_Final_Completion_Report.md`
- `Unity6_Test_Execution_Guide.md`
- `TestCoverage_Implementation_Guide.md`

### アーキテクチャ文書
- `CLAUDE.md`: プロジェクト全体のガイドライン
- `CODING_CONVENTIONS.md`: コーディング規約
- `CORE_INTERFACE_DESIGN_GUIDELINES.md`: Core層設計原則

---

**作成者**: Claude Code Assistant
**承認待ち**: プロジェクトマネージャー
**推奨**: Phase 3への即時移行開始
