# TODO.md - Unity 6 3Dゲーム基盤プロジェクト 統合実装管理

## アーキテクチャとデザインパターン
- **ServiceLocator + Event駆駆動のハイブリッドアーキテクチャ（最重要）**:グローバルサービスへのアクセスとイベントベースの通信を組み合わせたハイブリッドアプローチ。
- **イベント駆動型アーキテクチャ**: `GameEvent` を介したコンポーネント間の疎結合な連携。
- **コマンドパターン**: ゲーム内のアクション（例：ダメージ、回復）をオブジェクトとしてカプセル化し、再利用と管理を容易にします。
- **ObjectPool最適化**: 頻繁に作成・破棄されるコマンドオブジェクトをプール化し、メモリ効率とパフォーマンスを大幅に向上させます（95%のメモリ削減効果）。
- **Scriptable Objectベースのデータ管理**: キャラクターのステータスやアイテム情報などをアセットとして管理。

## 文書管理情報

- **ドキュメント種別**: 統合実装管理
- **生成元**: TASKS.md 全体戦略 + 実装進捗統合
- **対象読者**: 開発者、プロジェクトマネージャー
- **更新日**: 2025年9月18日
- **ステータス**: 🎯 **Phase 3, Step 1: Core Architecture Refactoring 進行中**
- **🔄最新更新**: Core層リファクタリング計画を策定し、`TASK-010`として`TASKS.md`に統合。最優先タスクとして入力管理システムの実装を開始。

---

## 🏆 プロジェクト現状サマリー

### ✅ **Phase 1 & 2: 基盤構築とセットアップ完了**
- **成果**: ステルスゲームの基本機能、AI検知システム、プレイヤー状態管理、そして1分でプロジェクトを開始できるインタラクティブセットアップウィザードが完成済み。
- **価値**: `Clone & Create`の核心価値を実現し、プロジェクトの技術的基盤を確立。

---

## 🚀 **Next Actions: Core Architecture Refactoring (Phase 3, Step 1)**

### **🎯【最優先実行】TASK-010.1: 汎用的な入力管理システムの実装 (Priority 1)**

**目的**: 全てのテンプレートで再利用可能な、疎結合された入力システムをCore層に確立する。これにより、入力デバイスとゲームロジックを完全に分離し、今後の開発を加速させる。

**工数**: 1-2日 | **担当**: @AI

---

#### **📋 実装ステップ（Core_Refactoring_Design.md準拠）**

- [ ] **Step 1: `InputEventChannels` ScriptableObjectの作成**
  - `Assets/_Project/Core/ScriptableObjects/Input/` ディレクトリを作成。
  - `InputEventChannels.cs` スクリプトを作成し、`OnMoveInput`, `OnJumpInputPressed` などの`GameEvent`を定義する。
  - 上記スクリプトからアセットファイル (`InputEventChannels.asset`) を作成する。

- [ ] **Step 2: `InputService`クラスの実装**
  - `Assets/_Project/Core/Input/InputService.cs` を作成。
  - **名前空間**: `asterivo.Unity60.Core.Input` を適用。
  - `PlayerInput`コンポーネントへの参照を取得し、`ServiceLocator`に自身を登録する。
  - `PlayerInput`のEventsからコールバック（例: `OnMove`, `OnJump`）を受け取るメソッドを実装する。
  - 各コールバックメソッド内で、対応する`GameEvent`を`Raise()`するロジックを実装する。
  - Inspectorから`InputEventChannels.asset`を割り当てられるように`[SerializeField]`を設定する。

- [ ] **Step 3: シーンへの統合とテスト**
  - 既存のテストシーンまたは新規シーンに空のGameObjectを作成し、`PlayerInput`と`InputService`コンポーネントをアタッチする。
  - `Input Actions`アセットと`InputEventChannels`アセットを`InputService`に設定する。
  - 簡単なテスト用のリスナーコンポーネント（例: `InputDebugger.cs`）を作成し、`OnMoveInput`などのイベントを購読してコンソールにログを出力する。
  - プレイモードで入力を実行し、`InputService`からイベントが正しく発行されることを確認する。

- [ ] **Step 4: 既存Features層への影響確認**
  - `PlayerController`など、これまで直接入力を参照していたクラスを特定する。
  - それらのクラスが、新しい`GameEvent`をリッスンするように修正する準備を行う（実際の修正は後続タスク）。

---

## 📊 **全タスク統合進捗管理**

### **Phase別完了状況サマリー**

| Phase | 名称 | 状況 | 進捗率 | 核心価値 | 主要タスク |
|-------|------|------|--------|----------|----------|
| **1** | 🏗️ 基盤構築 | ✅完了 | 100% | 技術基盤 | TASK-001,002,005 |
| **2** | 🚀 Clone & Create | ✅完了 | 100% | 97%時間短縮 | TASK-003 |
| **3** | 📚 Learn & Grow | 🎯**進行中** | 5% | 70%学習コスト削減 | **TASK-010 (Core Refactoring)**, TASK-004 |
| **4** | 🏭 Ship & Scale | ⏳待機 | 0% | プロダクション対応 | TASK-007 |
| **5** | 🌐 Community & Ecosystem | ⏳待機 | 0% | AI連携・エコシステム | TASK-006,008 |

### **現在の全体状況詳細（Critical優先度タスク中心）**
| Phase | タスク | 優先度 | 状況 | Next Action |
|-------|--------|-------|------|-------------|
| **3** | **TASK-010 Core Refactoring** | **Critical** | 🎯**進行中** | **TASK-010.1 (Input System)の実装** |
| **3** | **TASK-010.6 階層化ステートマシン** ⭐⭐⭐⭐⭐ | **High** | ⏳待機 | **TASK-010.3完了後、5-7日実装** |
| **3** | TASK-004 Template Phase-1 | **Critical** | ⏳待機 | TASK-010完了後 |
| **4** | TASK-007 Template Phase-2 | **Critical** | ⏳待機 | TASK-004完了後 |

---

*このTODO.mdは、現在のスプリントで集中すべき最優先タスクを具体的に示します。Core層リファクタリングの完了が、後続の全テンプレート開発の成功の鍵となります。*
