# 3層アーキテクチャ移行計画書 (3-Layer Architecture Migration Plan)

- **バージョン**: 1.0
- **作成日**: 2025年9月20日
- **作成者**: Gemini
- **ステータス**: 提案

---

## 1. 目的 (Purpose)

このドキュメントは、現在の2層構造 (`Core` ← `Feature`) から、新たに `Template` 層を追加した3層アーキテクチャ (`Core` ← `Feature` ← `Template`) へと移行するための具体的な手順とベストプラクティスを定義する。

この移行により、`SPEC.md`に掲げる「究極のUnity 6ベーステンプレート」の核心価値、特に **再利用性の最大化**、**プロトタイピング速度の向上**、そして **非プログラマーとの連携強化** を実現することを目的とする。

## 2. 新アーキテクチャ定義 (New Architecture Definition)

新しいアーキテクチャは以下の3層で構成され、依存関係は常に一方向 (`Template` → `Feature` → `Core`) とする。

-   **Core層**: ゲームの「OS」。ジャンルを問わない普遍的な仕組み。
    -   例: `ServiceLocator`, `GameEvent`, `ICommand`, `HealthComponent`
-   **Feature層**: ゲームの「アプリケーション」。Core層を利用した具体的な機能部品。
    -   例: `PlayerController`, `NPCVisualSensor`, `Rifle.cs`, `HUDManager`
-   **Template層**: ゲームの「ドキュメント」。Feature層の部品を組み合わせたジャンル特化のひな形。
    -   例: シーン, 設定済みプレハブ, バランス調整用ScriptableObject

## 3. 移行戦略と優先度 (Migration Strategy & Priority)

移行は、プロジェクトへの影響を最小限に抑えつつ段階的に行う。

| フェーズ | 名称 | 優先度 | 目的 |
| :--- | :--- | :--- | :--- |
| **Phase 1** | 準備フェーズ (Preparation) | **Critical** | 移行の技術的な基盤を構築する。 |
| **Phase 2** | Feature層の整理 (Feature Layer Organization) | **High** | `Feature` 層の責務を明確化する。 |
| **Phase 3** | Template層の構築 (Template Layer Construction) | **Medium** | 最初のゲームテンプレートを構築し、新構造を実証する。 |

---

## 4. 具体的な移行手順 (Step-by-Step Migration Plan)

### Phase 1: 準備フェーズ (Preparation Phase) - 優先度: Critical

#### **タスク1.1: `Template`層のディレクトリ構造を作成する**
-   **方法**: `Assets/_Project/Features/` 配下に `Templates` ディレクトリを作成する。さらにその下に、各ジャンル用のディレクトリと、共通アセット用の `Common` ディレクトリを作成する。
-   **実行コマンド**:
    ```
    mkdir Assets/_Project/Features/Templates
    mkdir Assets/_Project/Features/Templates/Common
    mkdir Assets/_Project/Features/Templates/Stealth
    mkdir Assets/_Project/Features/Templates/Platformer
    mkdir Assets/_Project/Features/Templates/FPS
    mkdir Assets/_Project/Features/Templates/TPS
    ```
-   **完了条件**: 上記のディレクトリが正しく作成されていること。

#### **タスク1.2: `Template`層用のAssembly Definitionを作成する**
-   **方法**: `Assets/_Project/Features/Templates` ディレクトリに、`asterivo.Unity60.Features.Templates.asmdef` を作成する。この `asmdef` は `asterivo.Unity60.Features.asmdef` を参照するように設定する。
-   **`asterivo.Unity60.Features.Templates.asmdef` の内容**:
    ```json
    {
        "name": "asterivo.Unity60.Features.Templates",
        "rootNamespace": "asterivo.Unity60.Features.Templates",
        "references": [
            "asterivo.Unity60.Core",
            "asterivo.Unity60.Features"
        ],
        "includePlatforms": [],
        "excludePlatforms": [],
        "allowUnsafeCode": false,
        "overrideReferences": false,
        "precompiledReferences": [],
        "autoReferenced": true,
        "defineConstraints": [],
        "versionDefines": [],
        "noEngineReferences": false
    }
    ```
-   **完了条件**: `asmdef` が作成され、依存関係が正しく設定されていること。`Template` 層のスクリプトから `Feature` 層と `Core` 層のクラスを参照でき、逆ができないことを確認する。

#### **タスク1.3: 名前空間の移行**
-   **方法**: 既存の全スクリプトの名前空間を `_Project.*` から `asterivo.Unity60.*` 階層へ移行する。
-   **具体的なルール**:
    -   **Core層**: `namespace asterivo.Unity60.Core;` もしくは `namespace asterivo.Unity60.Core.{SubCategory};`
    -   **Feature層**: `namespace asterivo.Unity60.Features.{FeatureName};`
    -   **Template層**: `namespace asterivo.Unity60.Features.Templates.{GenreName};`
-   **作業内容**:
    1.  全 C# スクリプトファイル内の `namespace` 宣言を新しい規約に従って変更する。
    2.  名前空間の変更に伴い、影響を受けるすべてのスクリプトで `using` ディレクティブを更新する。
    3.  `Core`, `Feature`, `Templates` の各 `asmdef` ファイルに `"rootNamespace"` を設定し、エディタでの新規スクリプト作成時に正しい名前空間が自動的に付与されるようにする。
-   **完了条件**: プロジェクトから `namespace _Project` で始まる名前空間が一掃され、コンパイルエラーが発生しないこと。

### Phase 2: Feature層の整理 (Feature Layer Organization) - 優先度: High

#### **タスク2.1: ジャンル固有のアセットを特定する**
-   **方法**: 現在 `Assets/_Project/Features/` 配下にあるアセット（特にシーン、プレハブ、ScriptableObject）を確認し、特定のゲームジャンル（例: ステルス）でのみ使用されているものをリストアップする。
-   **対象例**:
    -   ステルスゲームのデモシーン
    -   巡回ルートが設定済みの敵NPCプレハブ
    -   ステルスゲーム用に調整された武器パラメータ (ScriptableObject)
-   **完了条件**: `Template` 層に移動すべきアセットのリストが完成していること。

### Phase 3: Template層の構築 (Template Layer Construction) - 優先度: Medium

#### **タスク3.1: 最初のテンプレートとして「Stealth」を構築する**
-   **方法**: タスク2.1で特定したステルスゲーム関連アセットを、`Assets/_Project/Features/Templates/Stealth/` ディレクトリ配下に移動する。ディレクトリ構造は `Scenes`, `Prefabs`, `Data` のように機能で分類する。
-   **移動例**:
    -   `Assets/_Project/Scenes/StealthDemo.unity` → `Assets/_Project/Features/Templates/Stealth/Scenes/StealthDemo.unity`
    -   `Assets/_Project/Features/AI/Prefabs/Guard_NPC_Patrol.prefab` → `Assets/_Project/Features/Templates/Stealth/Prefabs/Guard_NPC_Patrol.prefab`
    -   `Assets/_Project/Features/Weapons/Data/SilencedPistol.asset` → `Assets/_Project/Features/Templates/Stealth/Data/SilencedPistol.asset`
-   **完了条件**: ステルスゲームを構成するアセットが `Template` 層に移動され、ゲームが問題なく動作すること。

#### **タスク3.2: テンプレートの動作を検証する**
-   **方法**: `StealthDemo.unity` シーンを再生し、すべての機能が移行前と同様に動作することを確認する。プレハブの参照切れなどが発生した場合は修正する。
-   **完了条件**: ステルスゲームテンプレートが完全に動作し、他の開発者がこのテンプレートをベースに開発を開始できる状態になっていること。

## 5. ベストプラクティス (Best Practices)

1.  **依存関係の厳守**: `Core` 層は `Feature` や `Template` を絶対参照しない。`Feature` 層は `Template` を絶対参照しない。このルールは `Assembly Definition` によって強制する。
2.  **責務の分離**:
    -   **ロジックは `Core` か `Feature` へ**: 新しいC#スクリプトを追加する場合、それが汎用的な「仕組み」なら `Core` へ、具体的な「機能」なら `Feature` へ配置する。`Template` 層には原則として新しいロジックを追加しない。
    -   **データと構成は `Template` へ**: シーン、プレハブ、ゲームバランスを定義するScriptableObjectは `Template` 層に配置する。
3.  **`Common` の活用**: 複数のテンプレートで共有したいアセット（例: メインメニューのプレハブ）は、`Assets/_Project/Features/Templates/Common/` に配置し、重複を防ぐ。

## 6. 期待される効果 (Expected Outcomes)

-   **モジュール性の向上**: 各層が独立しているため、機能の追加や変更が容易になる。
-   **再利用性の最大化**: `Core` と `Feature` は、将来の異なるプロジェクトでも再利用可能な資産となる。
-   **開発サイクルの高速化**: 新規プロジェクトやプロトタイプは、既存の `Template` を複製するだけで迅速に開始できる。
-   **チーム連携の円滑化**: デザイナーは `Template` 層、プログラマーは `Core`/`Feature` 層と、作業領域が明確になり、コンフリクトが減少する。

