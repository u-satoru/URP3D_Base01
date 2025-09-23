# TASKS.md - Unity 6 3Dゲーム基盤プロジェクト 実装タスクリスト

## アーキテクチャとデザインパターン
- **ServiceLocator + Event駆駆動のハイブリッドアーキテクチャ(最重要)**:グローバルサービスへのアクセスとイベントベースの通信を組み合わせたハイブリッドアプローチ。
- **3層構造 (`Core` ← `Feature` ← `Template`) (最重要)**
  - プロジェクト全体の関心事を3つの層に分離し、`Core` ← `Feature` ← `Template` という一方向の依存関係を徹底することで、高い再利用性と拡張性を実現します。
  - **`Core`層 (ゲームのOS)**: ジャンルを問わない普遍的な「仕組み」を提供します。イベントシステム、コマンドパターン、ServiceLocatorなど、プロジェクトの根幹をなすシステムがここに配置されます。
  - **`Feature`層 (ゲームのアプリケーション)**: `Core`層の仕組みを利用して作られた、具体的なゲーム機能の「部品」です。プレイヤーの移動、AIの視覚センサー、武器システムなど、単体で機能するモジュールがここに属します。
  - **`Template`層 (ゲームのドキュメント)**: `Feature`層の部品を組み合わせて、特定のゲームジャンル（ステルス、FPSなど）の「ひな形」を構築します。主にシーン、設定済みプレハブ、バランス調整用のScriptableObjectで構成され、ゲームデザイナーの作業領域となります。
- **イベント駆動型アーキテクチャ**: `GameEvent` を介したコンポーネント間の疎結合な連携。
- **階層化ステートマシン (HSM)**: 複雑な状態管理を可能にする階層化ステートマシンの実装。
- **Commandパターン**: ゲーム内のアクション（例：ダメージ、回復）をオブジェクトとしてカプセル化し、再利用と管理を容易にします。
- **Observerパターン**: 状態変化を監視し、関連コンポーネントに通知する仕組み。
- **ObjectPool最適化**: 頻繁に作成・破棄されるコマンドオブジェクトをプール化し、メモリ効率とパフォーマンスを大幅に向上させます（95%のメモリ削減効果）。
- **Scriptable Objectベースのデータ管理**: キャラクターのステータスやアイテム情報などをアセットとして管理。
- **Strategyパターン**: アルゴリズムのファミリーを定義し、カプセル化して相互に置き換え可能にする。
- **Factory+Registryパターン**: オブジェクト生成をFactory、管理をRegistryで分離。
- **Lifecycle Managementパターン**: オブジェクトの生成・使用・破棄を一元管理。
- **3Dサウンドシステム**: 3D空間オーディオ、NPCの聴覚センサー、動的環境サウンド、オーディオマスキング機能を含む包括的な音響システム。
- **基本的なプレイヤー機能**: 移動やインタラクションの基盤。
- **エディタ拡張**: コマンドの発行やイベントの流れを視覚化するカスタムウィンドウ。

## アーキテクチャ制約

- **Dependency Injection (DI) フレームワークは使用しない**。

## 文書管理情報

- **種別**: 実装タスクリスト（SDD Phase 4）
- **生成元**: REQUIREMENTS.md v3.0 + DESIGN.md
- **更新日**: 2025年9月18日
- **整合性**: SPEC.md v3.0・REQUIREMENTS.md v3.0・DESIGN.md・Core_Refactoring_Design.md 完全整合
- **ステータス**: ❌未着手 🚧進行中 ✅完了 ⚠️ブロック中

## 🎯 核心ポイント・実行戦略

### ✅ Phase 1 & 2 完了実績
- **TASK-001**: NPCVisualSensor（50体NPC同時稼働・1フレーム0.1ms・4段階警戒） ✅完了
- **TASK-002**: PlayerStateMachine（8状態システム・Camera/Audio連動） ✅完了
- **TASK-003**: Interactive Setup Wizard System（1分セットアップ・7ジャンル対応） ✅完了

### 📋 戦略的実装ロードマップ
- **Phase 1** (Week 1): ✅完了 - ステルス基本機能完成
- **Phase 2** (Week 2): ✅完了 - Clone&Create価値実現（1分セットアップ）
- **Phase 3** (Week 2-3): 🎯**最優先** - Core Refactoring & Learn&Grow価値実現
- **Phase 4** (Week 4): 統合・最適化（Ship&Scale + Community&Ecosystem）

### 🏆 マイルストーン
| リリース | 状況 | 価値 |
|---------|-----|------|
| Alpha Release | ✅達成済み | 技術実証完了 |
| Beta Release | 🎯次期目標 | 市場投入可能 |
| Gold Master | 📋計画中 | 商用利用可能 |

---

## 依存関係・優先順位
**Critical**: TASK-010 → TASK-004 → TASK-007
**Medium**: TASK-006, TASK-009
**Low**: TASK-008
**品質検証**: TASK-V1, V2(完了) → TASK-V3～V5（各Phase完了時）

## 📋 アーキテクチャ制約・名前空間規約（全タスク適用）

### TR-2.2 名前空間一貫性制約（REQUIREMENTS.md完全準拠）
**全新規実装に適用必須**:
- **Root名前空間**: `asterivo.Unity60`（プロジェクト統一ルート）
- **Core層**: `asterivo.Unity60.Core.*`（基盤システム、Feature層参照禁止）
- **Feature層**: `asterivo.Unity60.Features.*`（機能実装、Core層依存許可）
- **Test層**: `asterivo.Unity60.Tests.*`（テスト専用）
- **⚠️禁止事項**: `_Project.*`名前空間の新規使用完全禁止（段階的削除実施）

---

# 🏗️ PHASE 1 & 2: 基盤構築とセットアップ ✅ 完了済み

- **TASK-001**: NPCVisualSensor System ✅
- **TASK-002**: PlayerStateMachine System ✅
- **TASK-005**: Visual-Auditory Detection統合システム ✅
- **TASK-003**: Interactive Setup Wizard System ✅

---

# 📚 PHASE 3: Core Refactoring & Learn & Grow価値実現 ✅ **Step 1完了 (100%)**

**Phase目標**: テンプレート共通基盤をCore層にリファクタリングし、それを基に6ジャンルのテンプレートを実装。学習コスト70%削減を実現する。

**進捗状況**:
- ✅ **Step 1: Core Architecture Refactoring (5/5 完了)** - TASK-010 全サブタスク実装完了 **🎉 2025年9月19日検証済み**
- 🎯 **Step 2: Game Genre Template Implementation** - TASK-004 開始可能

---

## 【Phase 3, Step 1: Core Architecture Refactoring】

### 🎯 TASK-010: Core Architecture Refactoring for Template Reusability ✅ **完了 (5/5 完了)**
- **要件ID**: Template_Commonality_Analysis.md, Core_Refactoring_Design.md
- **優先度**: **Critical（最優先）**
- **SPEC.md v3.0価値**: **Learn & Grow, Ship & Scaleの技術的基盤**
- **依存関係**: `TASK-004`はこのタスクの完了に依存する。✅ **完了により依存関係解決**
- **影響範囲**: プロジェクト全体のアーキテクチャ、コード再利用性
- **実工数**: 5-7日 **✅ 達成済み**
- **進捗状況**: **🎉 2025年9月19日 実装状況検証により全サブタスク完了確認**
  - ✅ TASK-010.1 Generic Input Management System (完了)
  - ✅ TASK-010.2 Generic Health & Damage System (完了)
  - ✅ TASK-010.3 Generic Character Control System (完了)
  - ✅ TASK-010.4 Generic Interaction System (完了)
  - ✅ TASK-010.5 Generic Camera Control System (完了)

#### 実装サブタスク（優先度順）

- [x] **TASK-010.1**: **Implement Generic Input Management System (Priority 1)** ✅ **COMPLETED**
  - **目的**: 入力処理をCore層に抽象化し、`GameEvent`を通じて各機能と連携する。
  - **主要コンポーネント**:
    - `InputService.cs` (`asterivo.Unity60.Core.Input`) ✅ **実装完了（設計書仕様を大幅に上回る実装）**
    - `InputEventChannels.asset` (`ScriptableObject`) ✅ **実装完了**
  - **完了条件**: `PlayerInput`からの入力を`GameEvent`として発行できること。✅ **達成済み**
  - **実装状況**:
    - ✅ IInputManager完全実装（設計書より包括的）
    - ✅ ServiceLocator統合完済
    - ✅ 全主要入力対応（移動・視点・ジャンプ・攻撃・インタラクト・走行・しゃがみ・メニュー・インベントリ）
    - ✅ 感度制御・デバッグ機能・状態追跡
    - ✅ 専用Input Assembly作成（循環依存解決）

- [x] **TASK-010.2**: **Implement Generic Health & Damage System (Priority 2)**
  - **目的**: 汎用的な体力管理とダメージ処理の仕組みを`Command Pattern`と統合して実装する。
  - **主要コンポーネント**:
    - `HealthComponent.cs` (`asterivo.Unity60.Core.Combat`)
    - `DamageCommand.cs`, `HealCommand.cs` (`asterivo.Unity60.Core.Commands`)
  - **完了条件**: コマンド経由で`HealthComponent`の体力を増減させ、イベントが発行されること。

- [x] **TASK-010.3**: **Implement Generic Character Control System (Priority 3)**
  - **目的**: キャラクターの物理挙動と状態管理の基盤をCore層に実装する。
  - **依存**: `TASK-010.1` (Input System)
  - **主要コンポーネント**:
    - `CharacterMover.cs` (`asterivo.Unity60.Core.Character`)
    - `StateMachine.cs`, `IState.cs` (`asterivo.Unity60.Core.Patterns.StateMachine`)
  - **完了条件**: 外部からの命令でキャラクターの物理的な移動・ジャンプが可能になること。

- [x] **TASK-010.4**: **Implement Generic Interaction System (Priority 4)**
  - **目的**: オブジェクトとのインタラクションの汎用的な仕組みを実装する。
  - **依存**: `TASK-010.1`, `TASK-010.3`
  - **主要コンポーネント**:
    - `IInteractable.cs`, `Interactor.cs` (`asterivo.Unity60.Core.Interaction`)
  - **完了条件**: `Interactor`が`IInteractable`を検知し、インタラクションを実行できること。

- [x] **TASK-010.5**: **Implement Generic Camera Control System (Priority 5)**
  - **目的**: `Cinemachine`と連携する汎用的なカメラ状態管理の基盤を実装する。
  - **依存**: `TASK-010.3`
  - **主要コンポーネント**:
    - `CameraService.cs`, `CameraStateMachine.cs` (`asterivo.Unity60.Core.Camera`)
  - **完了条件**: イベントをトリガーにカメラの状態をスムーズに遷移させられること。

---

## 【Phase 3, Step 2: Game Genre Template Implementation】

### 🎯 TASK-004: Ultimate Template Phase-1統合（Learn&Grow価値実現）🚧 **開始可能**
- **要件ID**: FR-8.1.2
- **優先度**: **Critical**
- **依存関係**: **TASK-010 (Core Architecture Refactoring) の完了が必須** ✅ **解決済み**
- **推定工数**: 6-7日
- **パフォーマンス要件**: テンプレート切り替え3分以内、サンプルシーン起動30秒以内、基本ゲームプレイ15分以内
- **現状**: コンパイルエラーあり（GenreTemplateConfig/ActionRPGテンプレート部分実装）

#### 実装サブタスク（DESIGN.md準拠）

##### Layer 1: Template Configuration Layer
- [ ] **TASK-004.1**: GenreTemplateConfig (ScriptableObject) システム実装
  - Template Registry System（Dictionary<GenreType, GenreTemplateConfig>による高速管理）
  - Dynamic Genre Switching（実行時ジャンル切り替え機能）
  - State Preservation System（切り替え時のユーザー進捗保持）
  - Asset Bundle Management（ジャンル別アセット効率管理）
  - **名前空間制約**: `asterivo.Unity60.Core.Templates` (ScriptableObjectベース設定)

- [ ] **TASK-004.2**: 7ジャンルテンプレート完全実装（REQUIREMENTS.md優先度準拠）
  - **Stealth Template Configuration**: ✅AI Detection Systems活用、Stealth Mechanics、Environmental Interaction **（最優先）**
    - 名前空間: `asterivo.Unity60.Features.Templates.Stealth`
  - **Platformer Template Configuration**: Jump & Movement Physics、Collectible Systems、Level Design Tools **（高優先）**
    - 名前空間: `asterivo.Unity60.Features.Templates.Platformer`
  - **FPS Template Configuration**: First Person Camera Setup、Shooting Mechanics Presets、Combat UI Configuration **（高優先）**
    - 名前空間: `asterivo.Unity60.Features.Templates.FPS`
  - **TPS Template Configuration**: Third Person Camera System、Cover System Integration、Aiming & Movement Mechanics **（高優先）**
    - 名前空間: `asterivo.Unity60.Features.Templates.TPS`

##### Layer 2: Runtime Template Management + 学習システム統合
- [ ] **TASK-004.3**: TemplateManager (Singleton) + 学習支援システム実装
  - Configuration Synchronization（設定の即座同期）
  - TemplateTransitionSystem（スムーズなシーン遷移）
  - Data Migration Between Genres（ジャンル間データ移行）
  - User Progress Preservation（学習進捗の永続化）
  - **5段階学習システム**: 基礎→応用→実践→カスタマイズ→出版
  - **名前空間制約**: `asterivo.Unity60.Core.Templates.Management` (Singletonシステム管理)

- [ ] **TASK-004.4**: Camera & Input Settings Presets 実装
  - **Cinemachine 3.1統合プリセット**: VirtualCamera構成済み、ジャンル別優先度管理
  - **Input System構成プリセット**: PlayerInputActions自動生成、ジャンル別マッピング
  - **Real-time Configuration**: プリセット即座切り替えとプレビュー機能
  - カスタマイズ対応UI（Inspector拡張）
  - **名前空間制約**:
    - Camera: `asterivo.Unity60.Features.Camera.Templates`
    - Input: `asterivo.Unity60.Features.Input.Templates`

- [ ] **TASK-004.5**: Sample Gameplay + インタラクティブチュートリアル実装
  - **各ジャンル15分ゲームプレイ**: 基本動作確認可能なサンプルシーン
  - **段階的学習チュートリアル**: Unity中級開発者が1週間で基本概念習得可能
  - **インタラクティブチュートリアル統合**: リアルタイムヒントシステム
  - **進捗追跡と成果測定**: 学習コスト70%削減の定量測定
  - **名前空間制約**:
    - Tutorial: `asterivo.Unity60.Features.Tutorial.Interactive`
    - Progress: `asterivo.Unity60.Core.Learning.Progress`

**FR-8.1.2 完全準拠受入条件**:
- ✅各ジャンル15分以内で基本ゲームプレイ実現
- ✅テンプレート切り替え時のデータ整合性保証（3分以内）
- ✅サンプルシーンの完全動作確認（30秒以内起動）
- ✅学習コスト70%削減目標の達成（40時間→12時間、Unity中級開発者1週間習得）
- ✅Learn & Grow価値の実現（実践的学習コンテンツ、段階的成長支援）
- ✅**名前空間規約完全遵守**（asterivo.Unity60.Features.*、Template全システム）
- ✅**レガシー名前空間段階的削除完了**（_Project.*→asterivo.Unity60.*移行）
- ✅**アーキテクチャ制約完全遵守**（Core/Feature層分離、依存関係制御）

---

# 🏭 PHASE 4: Ship & Scale価値実現 ⏳ 待機中

**Phase目標**: プロダクション品質システム・プロトタイプ→本番完全対応
**核心価値**: Ship & Scale（プロトタイプからプロダクションまで完全対応）
**完了状況**: ⏳ 0% (Phase 3完了後開始)
**主要タスク**: TASK-007 Ultimate Template Phase-2拡張
**完了条件**: プロダクション品質保証・セーブロード最適化・多言語対応・アセット統合支援
**依存関係**: Phase 3 (TASK-004) 完了必須
**次Phase**: Phase 5 Community & Ecosystem価値実現へ

---

## 【Phase 4 実装タスク詳細】

### TASK-007: Ultimate Template Phase-2拡張（Ship&Scale価値実現）❌
- **要件ID**: FR-8.2.1～FR-8.2.4 | **優先度**: Critical | **工数**: 8-10日
- **DESIGN.md準拠**: プロダクション品質システムの完全実装
- **核心価値**: Ship & Scale（プロトタイプ→プロダクション完全対応）
- **名前空間基盤**: `asterivo.Unity60.Core.Production.*`

#### 実装サブタスク詳細

##### TASK-007.1: Advanced Save/Load System（FR-8.2.1）
- [ ] **SaveSystemManager実装**
  - ScriptableObjectベースの統合管理・モジュラー設計
  - 10スロット複数セーブ対応（スロット間独立性保証）
  - 自動保存機能（チェックポイント+時間間隔ハイブリッド方式）
  - **名前空間**: `asterivo.Unity60.Core.Production.SaveSystem`
- [ ] **暗号化・セキュリティ機能**
  - AES256セーブデータ暗号化・整合性検証
  - チェックサム検証・バックアップ機能
  - データ破損自動復旧機能
- [ ] **クラウド統合**
  - Steam Cloud, iCloud, Google Play Games統合サポート
  - バックグラウンド同期・UIブロックなし処理
  - **パフォーマンス要件**: セーブ100ms以内、ロード50ms以内

##### TASK-007.2: Comprehensive Settings System（FR-8.2.2）
- [ ] **4カテゴリ設定システム**
  - Graphics: 品質レベル・解像度・フレームレート・URP固有設定
  - Audio: マスター音量・カテゴリ別・オーディオデバイス選択
  - Input: キーバインド変更・感度調整・コントローラー対応
  - Gameplay: 難易度・字幕・アクセシビリティ設定
  - **名前空間**: `asterivo.Unity60.Core.Production.Settings`
- [ ] **リアルタイム機能**
  - 再起動不要設定変更反映・即座プレビュー
  - 設定プリセット（Easy/Normal/Expert/Custom）
  - JSON設定インポート/エクスポート

##### TASK-007.3: 多言語ローカライゼーション（FR-8.2.3）
- [ ] **2言語完全対応**
  - Primary: 日本語（基準言語）
  - Secondary: English（国際展開）
  - **名前空間**: `asterivo.Unity60.Core.Production.Localization`
- [ ] **高度機能**
  - 実行時言語切り替え（3秒以内）
  - フォント・レイアウト自動調整・フォールバック
  - 音声翻訳対応・音声アセット管理
  - 翻訳管理エディタツール（CSV/JSON・バージョン管理）

##### TASK-007.4: アセット統合支援（FR-8.1.3拡張）
- [ ] **50+アセット統合システム**
  - アセット互換性チェッカー・依存関係自動解決
  - Asset Store連携・バージョン管理
  - アセット競合自動回避・統合ガイダンス
  - **名前空間**: `asterivo.Unity60.Core.Production.AssetIntegration`

**Ship & Scale価値実現受入条件**:
- ✅プロダクション品質保証（エラー・警告ゼロ）
- ✅パフォーマンス要件全達成（セーブ/ロード・設定変更応答）
- ✅2言語完全サポート・実行時切り替え確認
- ✅50+アセット統合対応・自動競合解決検証

---

# 🌐 PHASE 5: Community & Ecosystem価値実現 ⏳ 待機中

**目標**: 究極テンプレート100%完成・Gold Master品質達成
**主要タスク**:
- **TASK-006**: SDD Workflow Integration（4-5日）- AI連携・文書自動生成
- **TASK-008**: Performance Optimization Suite（3-4日）- 全体パフォーマンス最適化
- **TASK-009**: ProjectDebugSystem統合デバッグツール（4-5日）

---

## 【Phase 5 実装タスク詳細】

### TASK-006: SDD Workflow Integration System ❌
- **要件ID**: SDD-1.1, SDD-1.2 | **優先度**: Medium | **工数**: 4-5日
- **DESIGN.md準拠**: Community & Ecosystem価値実現の核心システム
- **核心価値**: AI連携開発・エコシステム基盤・コミュニティ形成
- **名前空間基盤**: `asterivo.Unity60.Core.Community.SDD`

#### 実装サブタスク詳細

##### TASK-006.1: SDD 5フェーズ管理システム
- [ ] **MarkdownDocumentManager実装**
  - 5フェーズドキュメント自動生成（SPEC→REQUIREMENTS→DESIGN→TASKS→実装）
  - バージョン管理・変更追跡システム
  - フェーズ間整合性自動検証
  - **名前空間**: `asterivo.Unity60.Core.Community.SDD.DocumentManager`
- [ ] **AI統合ワークフロー**
  - Claude Code MCP Server統合（unityMCP・context7・git連携）
  - 自動コード生成統合・品質保証AI統合
  - 開発効率測定システム（50%向上目標）

##### TASK-006.2: 拡張機能管理システム
- [ ] **サードパーティ統合API**
  - プラグインアーキテクチャ基盤
  - APIゲートウェイ・セキュリティポリシー
  - サンドボックス実行環境
  - **名前空間**: `asterivo.Unity60.Core.Community.Extensions`
- [ ] **開発者向けドキュメント生成**
  - API仕様自動生成・コード例統合
  - インタラクティブドキュメント・実行可能サンプル

### TASK-008: Performance Optimization Suite ❌
- **要件ID**: NFR-1.1～NFR-1.4 | **優先度**: Medium | **工数**: 4-5日
- **DESIGN.md準拠**: 究極テンプレート最終パフォーマンス最適化
- **核心価値**: 60FPS安定・メモリ効率・ビルド最適化
- **名前空間基盤**: `asterivo.Unity60.Core.Performance.Optimization`

#### 実装サブタスク詳細

##### TASK-008.1: Memory最適化システム
- [ ] **ObjectPool拡張最適化**
- [ ] **ガベージコレクション最適化**
- [ ] **メモリリーク検出・自動修正システム**
- [ ] **メモリプロファイリング統合**

##### TASK-008.2: CPU最適化システム
- [ ] **フレーム分散処理拡張**
- [ ] **計算負荷分散・マルチスレッド処理対応**
- [ ] **60FPS安定動作保証**

##### TASK-008.3: Build・I/O最適化
- [ ] **ビルド最適化システム**
- [ ] **統合ベンチマークシステム**

### TASK-009: ProjectDebugSystem統合デバッグツール ❌
- **要件ID**: FR-7.3 | **優先度**: Medium | **工数**: 4-5日
- **DESIGN.md準拠**: Core層配置（asterivo.Unity60.Core.Debug）による基盤デバッグシステム
- **名前空間制約**: TR-2.2完全準拠（asterivo.Unity60.Core.Debug.*）
- **価値**: 開発効率向上、統合デバッグ環境、プロダクションビルド完全分離

#### 実装サブタスク詳細

##### TASK-009.1: 統一ログシステム実装
- [ ] **ProjectLogger静的クラス実装**
- [ ] **リアルタイムパフォーマンス監視システム**
- [ ] **プロジェクト診断エンジン**
- [ ] **環境別デバッグ設定管理**