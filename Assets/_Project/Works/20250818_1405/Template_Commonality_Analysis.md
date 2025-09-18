# テンプレート共通機能の分析とCore層へのリファクタリング提案

## 1. 概要

このドキュメントは、`Stealth`、`FPS`、`Platformer`、`TPS`の4つの主要ゲームジャンルテンプレートに共通して必要となる機能を分析し、それらをプロジェクトの`Core`層に移行することの妥当性を検討し、具体的なリファクタリング案を提示するものです。

この提案は、プロジェクトの設計原則である「**関心の分離**」と「**疎結合**」を徹底し、コードの再利用性を最大化することで、将来のテンプレート追加や機能拡張を加速させることを目的とします。

## 2. 分析対象テンプレート

- **Stealth Template**: AIによる検知、隠密行動、環境とのインタラクションが主軸。
- **FPS Template**: 一人称視点での射撃、精密なエイム、戦闘、UI操作が主軸。
- **Platformer Template**: ジャンプや移動などの物理ベース制御、アイテム収集、ギミック操作が主軸。
- **TPS Template**: 三人称視点での戦闘、カバーアクション、肩越し視点での照準が主軸。

## 3. 共通機能の抽出とCore層への移行提案サマリー

| 共通機能 | Core層へ移動すべきか | 理由 | Core実装の責務 | Feature実装の責務 |
| :--- | :--- | :--- | :--- | :--- |
| **キャラクター制御** | ✅ Yes | 移動や状態管理の物理的・論理的基盤は全ジャンルで普遍的。 | 汎用`CharacterMover`、`StateMachine`基盤の実装。 | ジャンル特有の状態（例：カバーステート）や`CharacterMover`を利用する`PlayerController`の実装。 |
| **インタラクション** | ✅ Yes | 「オブジェクトを検知し、アクションを実行する」という概念は普遍的。 | `IInteractable`インターフェースと、それを検知・実行する`Interactor`コンポーネントの実装。 | `IInteractable`を実装した具体的なオブジェクト（ドア、アイテム等）の実装。 |
| **カメラ制御** | ✅ Yes | 視点（一人称/三人称/肩越し）の状態管理と遷移の仕組みは共通化可能。 | `CameraStateMachine`の基盤と、状態遷移イベントの管理。 | 各ジャンル特有のカメラ設定（`Cinemachine`プリセット）や、新しいカメラ状態の実装。 |
| **入力管理** | ✅ Yes | 入力デバイスとゲームロジックを分離し、疎結合を促進するため。 | `InputService`による入力デバイスの抽象化と、`GameEvent`の発行。 | `GameEvent`を購読し、キャラクターを動かすなどの具体的なゲームロジック。 |
| **体力・ダメージ** | ✅ Yes | 体力、ダメージ、回復の概念は多くのゲームで共通の基本要素。 | `HealthComponent`と、`DamageCommand`/`HealCommand`などの汎用コマンドの実装。 | キャラクターごとの具体的なステータス定義（`ScriptableObject`）、ダメージ計算式、UI表示。 |

---

## 4. 各共通機能の詳細な分析と提案

### 4.1. 汎用的な入力管理システム (Generic Input Management System)

- **現状の課題**: UnityのInput Systemは強力ですが、各機能が直接参照すると密結合になります。入力処理を抽象化する層が必要です。
- **提案**:
    1.  `InputService`を`Core`層に作成し、`ServiceLocator`に登録します。
    2.  このサービスは、Unityの`PlayerInput`からのコールバックを受け取り、それを抽象的なゲーム内イベント（例: `OnMoveInput(Vector2)`)に変換して`GameEvent`として発行します。
- **Core層への配置理由**: 入力デバイスからの生データを解釈し、システム全体で利用可能なイベントに変換するプロセスは、アプリケーションの最下層に位置するべきコアな関心事です。これにより、どの`Features`も入力デバイスの詳細を意識することなく、意味のあるアクションに集中できます。
- **実装方針**:
    - **Core層 (`asterivo.Unity60.Core.Input`)**:
        - `InputService.cs`: `PlayerInput`のアクションにコールバックを登録し、対応する`GameEvent`を発行する責務を持つ。
        - `InputEvents.asset`: `OnMoveInput`, `OnJumpInputPressed`などのイベントを定義した`ScriptableObject`群。
    - **Features層**:
        - `PlayerController.cs`: `InputEvents`を購読し、キャラクターの制御を行う。
        - `WeaponController.cs`: `OnFireInputPressed`を購読し、射撃処理を行う。

### 4.2. 汎用的な体力・ダメージシステム (Generic Health & Damage System)

- **現状の課題**: FPSでの被弾、Stealthでの発見後のダメージ、Platformerでの敵との接触など、ダメージを受ける概念は広く共通しています。
- **提案**:
    1.  `HealthComponent`を`Core`層に作成します。これには、現在の体力、最大体力、ダメージを受ける処理、死亡時のイベントなどが含まれます。
    2.  `DamageCommand`や`HealCommand`といったコマンドを`Core`層で具体的に実装し、`ObjectPool`から利用できるようにします。
- **Core層への配置理由**: 体力とダメージは、多くのゲームにおける基本的なステータス管理の一部です。プロジェクトの`Command Pattern`アーキテクチャと親和性が非常に高く、`Core`のコマンドシステムを拡張する自然な形となります。
- **実装方針**:
    - **Core層 (`asterivo.Unity60.Core.Combat`)**:
        - `HealthComponent.cs`: 体力データを持ち、`TakeDamage(float amount)`メソッドを提供する。体力が変化した際に`OnHealthChanged`イベントを発行する。
        - `DamageCommand.cs`: `ICommand`を実装し、ターゲットの`HealthComponent`にダメージを与える。
    - **Features層**:
        - `CharacterStats.asset`: プレイヤーや敵の最大体力などを定義する`ScriptableObject`。`HealthComponent`は初期化時にこれを参照する。
        - `HealthBarUI.cs`: `OnHealthChanged`イベントを購読し、UIの表示を更新する。
        - `Projectile.cs`: 敵に衝突した際に、`CommandInvoker`を通じて`DamageCommand`を発行する。

### 4.3. 汎用的なキャラクター制御システム (Generic Character Controller)

- **現状の課題**: 各テンプレートで個別にプレイヤーの移動や物理挙動を実装すると、類似コードが散在し、保守性が低下します。特に、接地判定や坂道での挙動などは共通化すべきロジックです。
- **提案**:
    1.  キャラクターの物理的な移動（歩行、走行、ジャンプ等）を扱う、物理ベースの`CharacterMover`コンポーネントを`Core`層に作成します。
    2.  キャラクターの状態（待機、歩行、ジャンプ、カバーなど）を管理する`StateMachine`の**基盤**（`IState`インターフェース、`BaseState`クラス、`StateMachine`クラス）を`Core`層に配置します。
- **Core層への配置理由**: キャラクターの物理的な移動と状態管理の論理的枠組みは、全ての3Dゲームで必要となる最も基本的な機能です。これらを`Core`に置くことで、`Features`層は「どのような状態を持つか」「入力に対してどう振る舞うか」という、よりゲームデザインに近い部分に集中できます。
- **実装方針**:
    - **Core層 (`asterivo.Unity60.Core.Character`)**:
        - `CharacterMover.cs`: `Rigidbody`への力適用、接地判定、速度制限などの物理挙動を提供。外部から`Move(direction)`や`Jump(force)`といった命令を受け付けます。
        - `StateMachine.cs`, `IState.cs`: 状態遷移を管理する汎用的なステートマシンシステム。
    - **Features層 (`asterivo.Unity60.Features.Player`)**:
        - `PlayerController.cs`: `InputService`からのイベントを購読し、`CharacterMover`に具体的な移動命令を与えます。
        - `PlayerIdleState.cs`, `PlayerWalkState.cs`, `PlayerCoverState.cs`など: `IState`を実装した、ジャンル特有の具体的な状態クラス群。

### 4.4. 汎用的なインタラクションシステム (Generic Interaction System)

- **現状の課題**: アイテム取得、ドアの開閉、スイッチ操作など、オブジェクトとのインタラクションは各テンプレートで必要ですが、個別の実装は非効率です。
- **提案**:
    1.  `IInteractable`インターフェースを`Core`層に定義します。
    2.  プレイヤー側に、前方にある`IInteractable`オブジェクトを検知し、入力に応じてインタラクションを実行する`Interactor`コンポーネントを`Core`層に作成します。
- **Core層への配置理由**: 「オブジェクトに近づいて何かアクションを起こす」という概念は、ゲームジャンルを問わない普遍的なメカニクスです。この仕組みを`Core`で提供することで、`Features`層はインタラクションの「具体的な中身」を実装することに集中できます。
- **実装方針**:
    - **Core層 (`asterivo.Unity60.Core.Interaction`)**:
        - `IInteractable.cs`: `void Interact(Interactor interactor);`と、UI表示用のプロパティ（例: `string InteractionPrompt { get; }`）を持つインターフェース。
        - `Interactor.cs`: カメラの前方にRaycastを飛ばし、`IInteractable`を検知して`Interact()`を呼び出す。検知した際に`OnInteractableFound`イベントを発行します。
    - **Features層**:
        - `Door.cs`, `PickupItem.cs`: `IInteractable`を実装した、ゲーム固有のオブジェクト。
        - `InteractionUI.cs`: `OnInteractableFound`イベントを購読し、「Eキーで開く」のようなUIを表示します。

### 4.5. 汎用的なカメラ制御システム (Generic Camera Control System)

- **現状の課題**: FPSの一人称視点、TPSの三人称・肩越し視点、Platformerの追従カメラなど、カメラ要件は多様ですが、状態の切り替えや管理の仕組みは共通化できます。
- **提案**:
    1.  `DESIGN.md`で言及されている`CinemachineIntegration`および`CameraStateMachine`の基盤部分を`Core`層に配置します。
    2.  `Core`層では、カメラ状態（例: `FirstPerson`, `ThirdPerson`, `Aim`, `Cover`）を管理し、イベントに応じてスムーズに切り替えるための汎用的なステートマシンを提供します。
- **Core層への配置理由**: カメラの状態管理と遷移の仕組みは、特定のゲームロジックから独立した技術的な基盤です。データ駆動設計（`ScriptableObject`）と組み合わせることで、`Core`の汎用システムと`Features`の個別設定を綺麗に分離できます。
- **実装方針**:
    - **Core層 (`asterivo.Unity60.Core.Camera`)**:
        - `CameraService.cs`: 現在のカメラ状態を管理し、`GameEvent`に応じて`CameraStateMachine`の状態を遷移させる`ServiceLocator`登録サービス。
        - `CameraStateMachine.cs`, `ICameraState.cs`: カメラ用の汎用ステートマシン。
    - **Features層 (`asterivo.Unity60.Features.Camera`)**:
        - `ThirdPersonCameraState.cs`, `AimingCameraState.cs`: `ICameraState`を実装した具体的なカメラ状態。
        - `CameraSettings.asset`: 各状態に対応する`CinemachineVirtualCamera`のプレハブや設定を保持する`ScriptableObject`。

## 5. 優先度付けとリファクタリング戦略

以下に、依存関係と影響範囲を考慮したリファクタリングの推奨順序を示します。

| 優先度 | 機能 | 理由 |
| :--- | :--- | :--- |
| **1 (Highest)** | **入力管理システム** | 全てのキャラクター操作やUIインタラクションの起点となる最も基本的なシステム。他のシステムがこのシステムのイベントを購読するため、最初に確立すべき。 |
| **2 (High)** | **体力・ダメージシステム** | ゲームのコアメカニクスでありながら、他のシステムとの依存関係が比較的薄く、独立して実装・テストしやすい。コマンドパターンとの親和性も高い。 |
| **3 (Medium)** | **キャラクター制御システム** | `入力管理システム`に依存。キャラクターの物理的な挙動と状態管理の基盤であり、ゲームプレイの根幹をなすため、早期の実装が望ましい。 |
| **4 (Low)** | **インタラクションシステム** | `入力管理システム`と`キャラクター制御システム`に依存。「キャラクターが、特定の入力で、何かをする」という流れを確立した後に実装するのが自然。 |
| **5 (Lowest)** | **カメラ制御システム** | `キャラクター制御システム`（キャラクターの位置）と`StateMachine`（キャラクターの状態）に強く依存。キャラクターが完全に動作するようになった後に実装するのが最も効率的。 |

## 6. 結論と次のステップ

上記で提案した5つのシステムを`Core`層にリファクタリングすることにより、`Features`層はより高レベルなゲームロジックの実装に集中でき、テンプレート間のコード重複を劇的に削減できます。これにより、開発効率と保守性が大幅に向上し、「**究極のUnity 6ベーステンプレート**」というプロジェクトのビジョン達成に大きく貢献します。

**次のステップ**:
1.  この分析結果と優先度に基づき、具体的なリファクタリング計画を`TASKS.md`に落とし込む。
2.  優先度1の`入力管理システム`から着手し、段階的にリファクタリングを進める。
3.  各リファクタリング完了後、既存のテンプレートが正常に動作することを保証するためのテストを整備する。
