# TASKS.md - Unity 6 3Dゲーム基盤プロジェクト 実装タスクリスト

## アーキテクチャとデザインパターン
- **ServiceLocator + Event駆駆動のハイブリッドアーキテクチャ**:グローバルサービスへのアクセスとイベントベースの通信を組み合わせたハイブリッドアプローチ。
- **イベント駆動型アーキテクチャ**: `GameEvent` を介したコンポーネント間の疎結合な連携。
- **コマンドパターン**: ゲーム内のアクション（例：ダメージ、回復）をオブジェクトとしてカプセル化し、再利用と管理を容易にします。
- **ObjectPool最適化**: 頻繁に作成・破棄されるコマンドオブジェクトをプール化し、メモリ効率とパフォーマンスを大幅に向上させます（95%のメモリ削減効果）。
- **Scriptable Objectベースのデータ管理**: キャラクターのステータスやアイテム情報などをアセットとして管理。

## 文書管理情報

- **種別**: 実装タスクリスト（SDD Phase 4）
- **生成元**: REQUIREMENTS.md v3.0 + DESIGN.md
- **更新日**: 2025年9月13日
- **整合性**: SPEC.md v3.0・REQUIREMENTS.md v3.0・DESIGN.md完全整合
- **ステータス**: ❌未着手 🚧進行中 ✅完了 ⚠️ブロック中

## 🎯 核心ポイント・実行戦略

### ✅ Phase 1完了実績
- **TASK-001**: NPCVisualSensor（50体NPC同時稼働・1フレーム0.1ms・4段階警戒） ✅完了
- **TASK-002**: PlayerStateMachine（8状態システム・Camera/Audio連動） ✅完了

### 📋 戦略的実装ロードマップ
- **Phase 1** (Week 1): ✅完了 - ステルス基本機能完成
- **Phase 2** (Week 2): 🎯最優先 - Clone&Create価値実現（1分セットアップ）
- **Phase 3** (Week 2-3): 🎯高優先 - Learn&Grow価値実現（7ジャンル対応）
- **Phase 4-6** (Week 4): 統合・最適化（Ship&Scale + Community&Ecosystem）

### 🏆 マイルストーン
| リリース | 状況 | 価値 |
|---------|-----|------|
| Alpha Release | ✅達成済み | 技術実証完了 |
| Beta Release | 🎯次期目標 | 市場投入可能 |
| Gold Master | 📋計画中 | 商用利用可能 |

---

## 依存関係・優先順位
**Critical**: TASK-001,002(完了) → TASK-003,004 → TASK-007
**Medium**: TASK-005,006,009
**Low**: TASK-008
**品質検証**: TASK-V1(完了) → TASK-V2～V5（各Phase完了時）

## 📋 アーキテクチャ制約・名前空間規約（全タスク適用）

### TR-2.2 名前空間一貫性制約（REQUIREMENTS.md完全準拠）
**全新規実装に適用必須**:
- **Root名前空間**: `asterivo.Unity60`（プロジェクト統一ルート）
- **Core層**: `asterivo.Unity60.Core.*`（基盤システム、Feature層参照禁止）
- **Feature層**: `asterivo.Unity60.Features.*`（機能実装、Core層依存許可）
- **Test層**: `asterivo.Unity60.Tests.*`（テスト専用）
- **⚠️禁止事項**: `_Project.*`名前空間の新規使用完全禁止（段階的削除実施）

### アセンブリ定義ファイル制約（.asmdef）
**依存関係コンパイル時強制**:
- **Core.asmdef**: Feature層参照完全禁止、基盤システムのみ
- **Features.asmdef**: Core層依存許可、Feature間疎結合
- **Tests.asmdef**: Core/Feature両層テスト、独立実行
- **通信方式**: Event駆動システムによる疎結合通信のみ許可

### 実装ガイドライン
1. **新規クラス作成時**: 必ず適切な名前空間を適用
2. **既存コード移行**: `_Project.*`から`asterivo.Unity60.*`へ段階的移行
3. **依存関係チェック**: Core→Feature参照禁止の厳守
4. **コンパイル検証**: アセンブリ定義違反の自動検出

# 🏗️ PHASE 1: 基盤構築 ✅ 完了済み

**成果**: ステルスゲーム基本機能完全動作・Alpha Release達成
- **TASK-001**: NPCVisualSensor（50体NPC同時稼働・1フレーム0.1ms・4段階警戒レベル）
- **TASK-002**: PlayerStateMachine（8状態システム・Camera/Audio/Input統合）
- **価値**: 技術実証完了、業界最高水準AI検出システム

---

# 🌐 PHASE 5: Community & Ecosystem価値実現 ⏳ 待機中

**目標**: 究極テンプレート100%完成・Gold Master品質達成
**主要タスク**:
- **TASK-006**: SDD Workflow Integration（4-5日）- AI連携・文書自動生成
- **TASK-008**: Performance Optimization Suite（3-4日）- 全体パフォーマンス最適化

---

# 🔍 品質検証システム（Phase完了時必須）

### Phase別検証タスク
- **TASK-V1**: Phase 1検証 ✅完了（アーキテクチャ・パフォーマンス・統合確認）
- **TASK-V2**: Phase 2検証 ✅完了（Setup Wizard・98%アーキテクチャ適合・Clone&Create価値実現）
- **TASK-V3**: Phase 3検証 ⏳待機（Template System・Learn&Grow価値実現）
- **TASK-V4**: Phase 4検証 ⏳待機（Production Systems・Ship&Scale価値実現）
- **TASK-V5**: Phase 5検証 ⏳待機（最終品質・Gold Master品質達成）

---

# 🚀 PHASE 2: Clone & Create価値実現 ✅ 完了済み

**成果**: 1分セットアップシステム完全実現・97%時間短縮達成
**TASK-003**: Interactive Setup Wizard System（3層アーキテクチャ・7ジャンル対応・名前空間規約遵守）
**価値**: Clone & Create実現（30分→1分・エラー処理・自動修復機能）


---

# 📚 PHASE 3: Learn & Grow価値実現 ⏳ 待機中

**Phase目標**: 7ジャンル完全対応・学習コスト70%削減実現
**核心価値**: Learn & Grow（40時間→12時間、70%削減）
**完了状況**: ⏳ 0% (Phase 2完了後開始)
**主要タスク**: TASK-004 Ultimate Template Phase-1統合
**完了条件**: 各ジャンル15分ゲームプレイ実現・学習コスト70%削減・市場投入可能品質達成
**依存関係**: Phase 2 (TASK-003) 完了必須
**次Phase**: Phase 4 Ship & Scale価値実現へ

---

## 【Phase 3 実装タスク詳細】

### 🎯 TASK-004: Ultimate Template Phase-1統合（Learn&Grow価値実現）❌
- **要件ID**: FR-8.1.2（究極テンプレート Phase A-2最優先）
- **優先度**: **Critical（最優先）** ← High から昇格
- **SPEC.md v3.0価値**: **Learn & Grow実現**（学習コスト70%削減、7ジャンル対応）
- **技術基盤**: NPCVisualSensor ✅, PlayerStateMachine ✅（ステルス基盤完備）
- **依存関係**: TASK-003 (Setup Wizard), DESIGN.md Comprehensive Genre Template Architecture
- **影響範囲**: **究極テンプレート核心価値実現**
- **推定工数**: 6-7日
- **パフォーマンス要件**: テンプレート切り替え3分以内、サンプルシーン起動30秒以内、基本ゲームプレイ15分以内

#### 実装サブタスク（DESIGN.md 2層アーキテクチャ準拠）

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
  - **Action RPG Template Configuration**: Character Progression System、Equipment Management、Skill Trees Integration **（中優先）**
    - 名前空間: `asterivo.Unity60.Features.Templates.ActionRPG`
  - **Adventure Template Configuration**: Dialogue Systems、Inventory Management、Quest System Framework
    - 名前空間: `asterivo.Unity60.Features.Templates.Adventure`
  - **Strategy Template Configuration**: RTS Camera Controls、Unit Selection Systems、Resource Management UI
    - 名前空間: `asterivo.Unity60.Features.Templates.Strategy`

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

## 【Phase 3 統合・最適化タスク】

### TASK-005: Visual-Auditory Detection統合システム ✅ 完了済み
- **要件ID**: FR-4.1, FR-4.2
- **成果**: NPCVisualSensor + NPCAuditorySensor統合、センサー融合システム完成
- **価値**: 統合検出システム精度向上、AI反応の自然性向上

---

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

### TASK-008: Performance Optimization Suite ❌
- **要件ID**: NFR-1.1～NFR-1.4 | **優先度**: Medium ← Low昇格 | **工数**: 4-5日
- **DESIGN.md準拠**: 究極テンプレート最終パフォーマンス最適化
- **核心価値**: 60FPS安定・メモリ効率・ビルド最適化
- **名前空間基盤**: `asterivo.Unity60.Core.Performance.Optimization`

#### 実装サブタスク詳細

##### TASK-008.1: Memory最適化システム
- [ ] **ObjectPool拡張最適化**
  - 現行95%メモリ削減効果の維持・向上
  - ガベージコレクション最適化（70%削減効果維持）
  - メモリリーク検出・自動修正システム
  - **名前空間**: `asterivo.Unity60.Core.Performance.Memory`
- [ ] **メモリプロファイリング統合**
  - Unity Profiler API統合・リアルタイム監視
  - メモリ使用量20%追加削減目標
  - 自動メモリ最適化提案システム

##### TASK-008.2: CPU最適化システム
- [ ] **フレーム分散処理拡張**
  - 現行1フレーム0.1ms維持・全システム拡張適用
  - 計算負荷分散・マルチスレッド処理対応
  - CPU使用率最適化・動的負荷制御
  - **名前空間**: `asterivo.Unity60.Core.Performance.CPU`
- [ ] **60FPS安定動作保証**
  - 全ジャンルテンプレート60FPS安定確認
  - パフォーマンス劣化自動検出・警告システム

##### TASK-008.3: Build・I/O最適化
- [ ] **ビルド最適化システム**
  - ビルド時間50%短縮（現行Assembly Definition活用拡張）
  - アセット読み込み最適化・ストリーミング
  - ロード時間50%短縮目標達成
  - **名前空間**: `asterivo.Unity60.Core.Performance.Build`
- [ ] **統合ベンチマークシステム**
  - 全パフォーマンス指標自動測定・レポート生成
  - 性能退行検出・継続的最適化監視

**究極パフォーマンス実現受入条件**:
- ✅60FPS安定動作確認（全ジャンルテンプレート）
- ✅メモリ使用量20%追加削減達成
- ✅ロード時間50%短縮確認
- ✅全体性能ベンチマーク合格（Gold Master品質）

### TASK-009: ProjectDebugSystem統合デバッグツール ❌
- **要件ID**: FR-7.3 | **優先度**: Medium | **工数**: 4-5日
- **DESIGN.md準拠**: Core層配置（asterivo.Unity60.Core.Debug）による基盤デバッグシステム
- **名前空間制約**: TR-2.2完全準拠（asterivo.Unity60.Core.Debug.*）
- **価値**: 開発効率向上、統合デバッグ環境、プロダクションビルド完全分離

#### 実装サブタスク詳細

##### TASK-009.1: 統一ログシステム実装
- [ ] **ProjectLogger静的クラス実装**
  - LogLevel管理（Debug/Info/Warning/Error/Critical）
  - LogCategory分類システム（Core/Features/Audio/AI/Commands/Events/Performance）
  - 構造化ログエントリ（LogEntry構造体）
  - 環境検出（エディタ/ランタイム自動判別）
  - 条件付きコンパイル（DEVELOPMENT_BUILD対応）

##### TASK-009.2: リアルタイムパフォーマンス監視システム
- [ ] **PerformanceMonitor MonoBehaviour Singleton実装**
  - Frame Rate追跡（CurrentFPS/AverageFPS算出）
  - Memory監視（AllocatedMemory/ReservedMemory）
  - CPU使用率分析（Unity Profiler API統合）
  - GPU Performance Metrics取得
  - 閾値チェック・警告システム（FPS<30警告、Memory>500MB警告）

##### TASK-009.3: プロジェクト診断エンジン
- [ ] **ProjectDiagnosticsWindow EditorWindow実装**
  - Event循環依存検出システム
  - Command実行統計分析
  - ObjectPool効率解析
  - ServiceLocatorヘルスチェック
  - Asset参照検証システム
  - タブ式UI（Events/Commands/Performance/ObjectPools）

##### TASK-009.4: 環境別デバッグ設定管理
- [ ] **DebugConfiguration ScriptableObject実装**
  - 環境自動検出（Development/Testing/Production）
  - LogLevel環境別設定
  - Category Filter管理
  - Performance監視設定
  - デバッグUI表示制御

**Core層配置制約**:
- 配置: `Assets/_Project/Core/Debug`
- 名前空間: `asterivo.Unity60.Core.Debug`
- アセンブリ定義: Core.asmdef配下（Feature層参照禁止）
- プロダクション自動無効化: コンパイル指示子活用

---

## 📊 実行順序・工数サマリー

### Phase完了状況
| Phase | 状況 | 主要タスク | 工数 | 価値 |
|-------|------|-----------|------|------|
| Phase 1 | ✅完了 | TASK-001,002,V1 | 6-8日 | 基盤構築 |
| Phase 2 | ✅完了 | TASK-003,V2 | 6-7日 | Clone & Create |
| Phase 3 | ⏳待機 | TASK-004,V3 | 7-9日 | Learn & Grow |
| Phase 4 | ⏳待機 | TASK-007,V4 | 10-12日 | Ship & Scale |
| Phase 5 | ⏳待機 | TASK-006,008,V5 | 7-10日 | Community & Ecosystem |

**総残り工数**: 24-31日（Phase 3～5）

---

## 🚀 次のアクション

### 最優先実行（Phase 3）
**TASK-004**: Ultimate Template Phase-1統合（6-7日）
- 7ジャンル完全対応テンプレート実装
- 70%学習コスト削減システム
- Learn & Grow価値実現

### 価値実現ロードマップ
- **Phase 3完了**: Learn & Grow価値（7ジャンル対応・学習支援）
- **Phase 4完了**: Ship & Scale価値（プロダクション品質・アセット統合）
- **Phase 5完了**: Community & Ecosystem価値（AI連携・エコシステム）

### 成功指標
- ✅ **Phase 1-2達成済み**: ステルス基本機能 + 1分セットアップシステム（Clone & Create価値実現）
- 🎯 **Phase 3目標**: 学習コスト70%削減（40時間→12時間）
- 🎯 **最終目標**: 究極テンプレート4つの核心価値完全実現 + Gold Master品質

---

*各Phase完了時は必ず品質検証タスク（TASK-V1～V5）を実行し、アーキテクチャ整合性を確認してください。*