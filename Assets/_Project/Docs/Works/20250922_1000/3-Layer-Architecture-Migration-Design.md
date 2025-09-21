# 3層アーキテクチャ移行 詳細設計書

## 1. 目的

本ドキュメントは、当プロジェクトを既存の構造から、`SPEC.md`および`REQUIREMENTS.md`で定義された**3層アーキテクチャ (`Core` ← `Feature` ← `Template`)** へと移行するための技術的な設計と手順を定義する。

この移行の目的は、以下の価値を実現することである：
- **関心事の完全な分離**: 各層が明確な責務を持つことで、コードの可読性と保守性を最大化する。
- **高い再利用性と拡張性**: `Feature`層のコンポーネントを独立した「部品」として開発し、異なる`Template`で再利用可能にする。
- **明確な依存関係**: `Template` → `Feature` → `Core`という一方向の依存関係をコンパイラレベルで強制し、意図しない結合を防止する。
- **チーム開発の効率化**: 各層の担当者が他の層への影響を最小限に抑えながら、並行して開発を進められるようにする。

## 2. 3層アーキテクチャの定義

移行の前提として、各層の責務を再確認する。

- **`Core`層 (ゲームのOS)**
  - **責務**: ジャンルを問わない、プロジェクト全体で共有される普遍的な「仕組み」を提供する。
  - **配置場所**: `Assets/_Project/Core`
  - **主な内容**:
    - `ServiceLocator`, `GameEvent`システム
    - コマンドパターン、階層化ステートマシン（HSM）などの基盤パターン
    - 汎用的なヘルパー、拡張メソッド
    - オーディオ、入力、シーン管理などの低レベルサービス
  - **制約**: `Feature`層および`Template`層へのいかなる参照も**禁止**する。

- **`Feature`層 (ゲームのアプリケーション)**
  - **責務**: `Core`層の仕組みを利用して作られた、具体的なゲーム機能の「部品」。単体で機能し、再利用可能であることを目指す。
  - **配置場所**: `Assets/_Project/Features/[機能名]` (例: `Assets/_Project/Features/Player`)
  - **主な内容**:
    - プレイヤーの移動、ヘルス管理
    - AIの視覚・聴覚センサー
    - 武器システム、インベントリシステム
    - UIコンポーネント（HUD、メニュー画面など）
  - **制約**: `Core`層への参照のみ許可。他の`Feature`層や`Template`層への直接参照は**禁止**。`Feature`間の連携は`Core`層の`GameEvent`を介して行う。

- **`Template`層 (ゲームのドキュメント)**
  - **責務**: `Feature`層の「部品」を組み合わせて、特定のゲームジャンル（ステルス、FPSなど）の「ひな形」を構築する。
  - **配置場所**: `Assets/_Project/Features/Templates/[ジャンル名]` (例: `Assets/_Project/Features/Templates/Stealth`)
  - **主な内容**:
    - ゲームプレイシーン (`.unity`)
    - 設定済みのプレハブ
    - バランス調整用の`ScriptableObject`アセット
    - ジャンル特有のTimelineアセット
  - **制約**: `Feature`層への参照のみ許可。原則として、`Template`層には新しいロジック（C#スクリプト）を追加せず、`Feature`層のコンポーネントの設定と組み合わせに注力する。

## 3. 移行戦略と手順

移行は以下の4フェーズで段階的に実施する。

### フェーズ1: 基盤整備 - Assembly Definitionの導入

依存関係をコンパイラレベルで強制するため、各層に`Assembly Definition`（`.asmdef`）ファイルを導入する。

1.  **`Core`層の.asmdef作成**:
    - `Assets/_Project/Core/asterivo.Unity60.Core.asmdef` を作成。
    - `Name`: `asterivo.Unity60.Core`
    - `Assembly Definition References`: なし

2.  **`Feature`層の.asmdef作成**:
    - 各機能フォルダ（例: `Assets/_Project/Features/Player`）に`.asmdef`を作成する。
    - `Name`: `asterivo.Unity60.Features.Player`
    - `Assembly Definition References`: `asterivo.Unity60.Core` を追加。

3.  **`Template`層の.asmdef作成**:
    - 各ジャンルフォルダ（例: `Assets/_Project/Features/Templates/Stealth`）に`.asmdef`を作成する。
    - `Name`: `asterivo.Unity60.Features.Templates.Stealth`
    - `Assembly Definition References`: このテンプレートが使用する`Feature`層の`.asmdef`（例: `asterivo.Unity60.Features.Player`, `asterivo.Unity60.Features.AI`）を追加。

### フェーズ2: `Core`層の整理とリファクタリング

既存のコードベースから`Core`層に属するべきスクリプトを整理・移行する。

1.  **ファイル移動**: `Assets/_Project/Scripts` や他の場所にある汎用的なスクリプト（`ServiceLocator`, イベントシステム, 各種デザインパターン基盤など）を `Assets/_Project/Core` 以下の適切なサブフォルダに移動する。
2.  **名前空間の統一**: `Core`層に配置されたすべてのスクリプトの名前空間を `asterivo.Unity60.Core.*` に統一する。
3.  **依存関係の検証**: `Core`層のコードが`Feature`や`Template`層のコードを一切参照していないことを確認する。コンパイルエラーが出れば、依存関係違反の可能性が高い。

### フェーズ3: `Feature`層の抽出と疎結合化

ゲームの具体的な機能を`Feature`として独立させる、移行作業の中心的なフェーズ。

1.  **機能単位の特定**: 既存のロジックを機能単位（Player, AI, Inventory, UIなど）に分割する。
2.  **ファイル移動と.asmdef適用**:
    - 特定した機能に関連するスクリプト、プレハブ、モデルなどを `Assets/_Project/Features/[機能名]` フォルダに移動する。
    - フェーズ1で作成した`Feature`層の`.asmdef`を適用する。
3.  **名前空間の統一**: 各`Feature`層のスクリプトの名前空間を `asterivo.Unity60.Features.[機能名]` に統一する。
4.  **疎結合リファクタリング**:
    - **最重要**: `Feature`間の直接参照をすべて洗い出す。例えば、`Player`スクリプトが`UIManager`を直接参照している箇所など。
    - これらの直接参照を、`Core`層の`GameEvent`を介した間接的な通信に置き換える。

#### リファクタリング具体例: プレイヤーのヘルスがUIに通知される場合

-   **移行前 (Before)**:
    ```csharp
    // In: Assets/_Project/Features/Player/PlayerHealth.cs
    public class PlayerHealth : MonoBehaviour
    {
        private UIManager _uiManager; // 直接参照

        void Start() {
            _uiManager = FindObjectOfType<UIManager>();
        }

        public void TakeDamage(float amount) {
            currentHealth -= amount;
            _uiManager.UpdateHealthBar(currentHealth); // UIマネージャーを直接呼び出し
        }
    }
    ```

-   **移行後 (After)**:
    ```csharp
    // In: Assets/_Project/Core/Events/PlayerEvents.cs
    [CreateAssetMenu(menuName = "Game/Events/Float Event")]
    public class FloatEvent : GameEvent<float> {} // 汎用的なイベントをCoreに定義

    // In: Assets/_Project/Features/Player/PlayerHealth.cs
    using asterivo.Unity60.Core.Events;
    public class PlayerHealth : MonoBehaviour
    {
        public FloatEvent OnHealthChanged; // Core層のイベントを参照

        public void TakeDamage(float amount) {
            currentHealth -= amount;
            OnHealthChanged.Raise(currentHealth); // イベントを発行するだけ
        }
    }

    // In: Assets/_Project/Features/UI/HealthBarView.cs
    using asterivo.Unity60.Core.Events;
    public class HealthBarView : MonoBehaviour
    {
        public GameEventListener<float> healthChangedListener; // イベントリスナー
        public Slider healthSlider;

        void OnEnable() {
            healthChangedListener.Response.AddListener(UpdateHealthBar);
        }
        void OnDisable() {
            healthChangedListener.Response.RemoveListener(UpdateHealthBar);
        }

        public void UpdateHealthBar(float newHealth) {
            healthSlider.value = newHealth;
        }
    }
    ```

### フェーズ4: `Template`層の構築

`Feature`層の部品を組み合わせて、最終的なゲームの形を構築する。

1.  **アセットの移動**: 各ゲームジャンルに対応するシーン、設定済みプレハブ、`ScriptableObject`を `Assets/_Project/Features/Templates/[ジャンル名]` フォルダに移動する。
2.  **シーンとプレハブの再接続**: 移動したシーンやプレハブ内の参照が、`Feature`層に移動したスクリプトやアセットに正しくリンクされているか確認し、必要であれば再設定する。
3.  **動作確認**: 各`Template`のサンプルシーンを再生し、すべての機能が意図通りに連携して動作することを確認する。

## 4. 検証計画

移行が正しく完了したことを確認するための計画。

1.  **コンパイル**: プロジェクト全体でコンパイルエラーが発生しないこと。`.asmdef`による依存関係の制約が正しく機能している証拠となる。
2.  **ユニットテスト**: 既存のユニットテストがすべてパスすること。
3.  **プレイモードテスト**:
    - 各`Feature`が単体で動作することを確認するテストシーンを作成する。
    - 各`Template`のメインシーンを再生し、エンドツーエンドでの動作が移行前と変わらないことを確認する。
4.  **コードレビュー**:
    - `Feature`間の直接参照が残っていないか、静的解析や目視で確認する。
    - 新しい名前空間規約が遵守されているか確認する。

以上をもって、3層アーキテクチャへの移行を完了とする。