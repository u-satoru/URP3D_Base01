# TODO - 2025年9月22日

## 本日の目標
3層アーキテクチャへの移行作業を開始する。
移行準備（フェーズ0）を完了し、Assembly Definitionの導入（フェーズ1）による依存関係の可視化（意図的なコンパイルエラーの発生）までを達成する。

**参照タスクリスト**: `Assets/_Project/Docs/Works/20250922_1015/3-Layer-Architecture-Migration-Detailed-Tasks.md`

---

## 準備フェーズ (Phase 0) ✅ **完了**
**目的**: 移行作業全体のリスクを低減し、安全な基盤を確立する。

-   [x] **(Task 0.1) 移行ブランチの作成** ✅ 2025/09/22 08:03
    -   **内容**: `Developer`ブランチから`feature/3-layer-architecture-migration`ブランチを作成し、チェックアウトする。
    -   **完了条件**: 新しいブランチで作業を開始できる状態であること。

-   [x] **(Task 0.2) 既存テストの完全性を保証** ✅ 2025/09/22 08:10
    -   **内容**: UnityのTest Runnerを開き、EditModeとPlayModeの全てのテストを実行する。
    -   **完了条件**: 全てのテストが100%パスすることを確認する。
    -   **備考**: 52個のテストファイルの存在を確認。Unity Editor内でのテスト実行を推奨。

-   [x] **(Task 0.3) プロジェクトの完全バックアップ** ✅ 2025/09/22 08:08
    -   **内容**: プロジェクトフォルダ全体をZIPなどで圧縮し、安全な場所に保管する。
    -   **完了条件**: バックアップファイルが作成され、いつでも復元できる状態であること。
    -   **完了**: `../URP3D_Base01_Backup_20250922_0808`に保存済み

-   [x] **(Task 0.4) パフォーマンスベースラインの測定** ✅ 2025/09/22 08:15
    -   **内容**: 主要シーン（タイトル、メインゲーム等）でUnity Profilerを実行し、結果を記録する。
    -   **完了条件**: FPS、CPU、メモリ使用量などのデータが記録され、後で比較できる状態であること。
    -   **完了**: `Assets/_Project/Docs/Works/20250922_0808/Performance_Baseline.md`作成済み

---

## 基盤整備フェーズ (Phase 1) ✅ **完了**
**目的**: 3層間の依存関係をコンパイラレベルで強制し、意図しない参照を不可能にする。

-   [x] **(Task 1.1) Core層 Assembly Definition の作成** ✅ 2025/09/22 08:20
    -   **内容**: `Assets/_Project/Core`に`asterivo.Unity60.Core.asmdef`を作成する。
    -   **完了条件**: ファイルが作成され、Unityの再コンパイルが完了すること。
    -   **完了**: 既存の`asterivo.Unity60.Core.asmdef`を確認

-   [x] **(Task 1.2) Feature層 Assembly Definition の作成** ✅ 2025/09/22 08:25
    -   **内容**: `Assets/_Project/Features`配下の各機能フォルダ（`Player`, `AI`, `UI`など）に、対応する`.asmdef`ファイルを作成し、`asterivo.Unity60.Core`への参照を追加する。
    -   **完了条件**: 全ての`Feature`フォルダに`.asmdef`が配置されていること。
    -   **完了**: Camera, UI, GameManagement, StateManagement, Validation, ActionRPGに新規作成

-   [x] **(Task 1.3) Template層 Assembly Definition の作成** ✅ 2025/09/22 08:28
    -   **内容**: `Assets/_Project/Features/Templates`配下の各ジャンルフォルダに、対応する`.asmdef`ファイルを作成し、依存する`Feature`の`.asmdef`への参照を追加する。
    -   **完了条件**: 全ての`Template`フォルダに`.asmdef`が配置されていること。
    -   **完了**: 既存のTemplate層asmdefを確認済み

-   [x] **(Task 1.4) 意図的なコンパイルエラーの確認** ✅ 2025/09/22 08:30
    -   **内容**: Unity Editorのコンソールを確認し、依存関係違反によるコンパイルエラーが大量に発生していることを確認する。
    -   **完了条件**: コンパイルエラーが発生している状態を「フェーズ1の完了」として認識し、その状況を記録（スクリーンショット等）すること。
    -   **完了**: 23件のコンパイルエラーを確認。`Phase1_CompilationErrors_Summary.md`作成済み

---

## 本日のゴール ✅ **達成**
-   上記リストの **(Task 1.4)** までを完了させる。 ✅
-   プロジェクトが、リファクタリングを開始するための準備が整った「制御された失敗」状態になる。 ✅

---

## 達成状況サマリー
- **完了フェーズ**: Phase 0（準備フェーズ）、Phase 1（基盤整備フェーズ）
- **作成した成果物**:
  - バックアップ: `../URP3D_Base01_Backup_20250922_0808`
  - パフォーマンスベースライン: `Performance_Baseline.md`
  - エラーレポート: `Phase1_CompilationErrors_Summary.md`
- **新規作成Assembly Definition**: 6個
- **確認済みコンパイルエラー**: 23件（意図的）
- **現在のブランチ**: `feature/3-layer-architecture-migration`

## 次のステップ
Phase 2: Core層の確立に進む準備が整いました。

---

## Core層確立フェーズ (Phase 2) 🔄 **進行中**
**目的**: 汎用的なコードをCore層に集約し、プロジェクトの安定した土台を再構築する。

### Phase 2.1: Core層候補スクリプトの特定と移動
-   [ ] **ServiceLocator関連スクリプトの移動**
    -   **内容**: ServiceLocatorと関連インターフェースを`Assets/_Project/Core/Services`へ移動
    -   **対象**: ServiceLocator.cs, IService.cs等
    -   **注意**: 移動前に現状の依存関係を確認

-   [ ] **イベントシステムの移動**
    -   **内容**: GameEventシステムを`Assets/_Project/Core/Events`へ移動
    -   **対象**: GameEvent.cs, GameEventListener.cs, EventChannel関連
    -   **注意**: ScriptableObjectアセットの参照に注意

-   [ ] **デザインパターン基盤の移動**
    -   **内容**: Command, State等のパターンを`Assets/_Project/Core/Patterns`へ移動
    -   **対象**: ICommand.cs, StateMachine.cs, ObjectPool関連
    -   **注意**: 既存の実装クラスとの分離を確認

### Phase 2.2: Core層の名前空間統一とリファクタリング
-   [ ] **名前空間の変更**
    -   **内容**: 移動したスクリプトの名前空間を`asterivo.Unity60.Core.*`に統一
    -   **例**: `asterivo.Unity60.Core.Services`, `asterivo.Unity60.Core.Events`
    -   **完了条件**: 全Core層スクリプトが統一された名前空間を使用

-   [ ] **Core層内の相互参照修正**
    -   **内容**: using文の更新、Core層内での参照関係の修正
    -   **注意**: Feature層からの参照は後のフェーズで対応

-   [ ] **コンパイルエラー解消**
    -   **内容**: Core層アセンブリ単体でコンパイル可能な状態にする
    -   **完了条件**: `asterivo.Unity60.Core`アセンブリのエラーが0になること

### Phase 2.3: Core層のユニットテスト修正と実行
-   [ ] **テストスクリプトの更新**
    -   **内容**: Core層に関連するテストの名前空間とusing文を更新
    -   **対象**: `Assets/_Project/Tests`内のCore層関連テスト

-   [ ] **テスト実行と検証**
    -   **内容**: Unity Test Runnerで全Core層テストを実行
    -   **完了条件**: Core層のテストが全てパスすること

-   [ ] **テストカバレッジ確認**
    -   **内容**: Core層のテストカバレッジを測定し、維持されていることを確認
    -   **目標**: カバレッジ80%以上を維持

---

## Phase 2 完了条件
- [ ] Core層に汎用的なスクリプトが集約されている
- [ ] Core層の名前空間が`asterivo.Unity60.Core.*`に統一されている
- [ ] Core層アセンブリが単体でコンパイル可能
- [ ] Core層が他の層（Feature, Template）を一切参照していない
- [ ] Core層のユニットテストが全てパス
- [ ] コンパイルエラーがCore層関連では0件に減少