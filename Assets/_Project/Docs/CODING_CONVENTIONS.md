# プロジェクトコーディング規約 - URP3D_Base01

## 1. 基本原則

- **3層アーキテクチャの遵守**: すべてのコードは `Core`, `Feature`, `Template` のいずれかの層に属し、`Template` → `Feature` → `Core` の一方向の依存関係を厳守すること。
- **疎結合の維持**: クラス間の直接参照は極力避け、`ServiceLocator`を通じたサービス利用と、`GameEvent`を通じたイベント駆動通信を基本とする。
- **可読性の重視**: 他の開発者が読んで理解しやすい、一貫性のあるコードを記述する。
- **パフォーマンスへの配慮**: `UniTask`による非同期処理、`ObjectPool`の活用など、パフォーマンスを意識した実装を心がける。

## 2. ディレクトリ構成

- **`Assets/_Project/Core`**: ゲームジャンルに依存しない、プロジェクトの基盤となるシステムを配置する。（例: `ServiceLocator`, `GameEvent`, `ICommand`）
- **`Assets/_Project/Features`**: 特定の機能を実現するための具体的な実装を配置する。（例: `PlayerController`, `AIStateMachine`, `WeaponSystem`）
- **`Assets/_Project/Features/Templates`**: 特定のゲームジャンルを構成するためのアセット（シーン、プレハブ、設定用`ScriptableObject`）を配置する。原則として、この層に新規のC#スクリプトは配置しない。
- **`Assets/_Project/Tests`**: `Core`層と`Feature`層に対応するユニットテスト、プレイモードテストを配置する。

## 3. 名前空間

- **Root**: `asterivo.Unity60`
- **Core層**: `asterivo.Unity60.Core`
  - サブシステムごとに分割する。（例: `asterivo.Unity60.Core.Commands`, `asterivo.Unity60.Core.Events`）
- **Feature層**: `asterivo.Unity60.Features`
  - 機能ごとに分割する。（例: `asterivo.Unity60.Features.Player`, `asterivo.Unity60.Features.AI`）
- **Template層**: `asterivo.Unity60.Features.Templates`
  - ジャンルごとに分割する。（例: `asterivo.Unity60.Features.Templates.Stealth`）

## 4. 命名規則

| 要素 | 規約 | 例 |
| :--- | :--- | :--- |
| **ファイル名 (C#)** | `PascalCase` | `PlayerController.cs` |
| **ファイル名 (.asmdef)** | `PascalCase` | `asterivo.Unity60.Core.asmdef` |
| **クラス・構造体・Enum** | `PascalCase` | `GameEvent`, `PlayerState` |
| **インターフェース** | `PascalCase` + `I`プレフィックス | `ICommand`, `IInitializable` |
| **public/protected メンバー** | `PascalCase` | `Execute()`, `CurrentHealth` |
| **private/internal メンバー** | `_camelCase` (アンダースコア + camelCase) | `_playerTransform`, `_CalculateDamage()` |
| **ローカル変数** | `camelCase` | `damageAmount` |
| **定数 (`const`, `static readonly`)** | `PascalCase` | `DefaultPlayerSpeed` |
| **Enumのメンバー** | `PascalCase` | `DamageType.Physical` |

## 5. コーディングスタイル

- **括弧 `{}`**: `if`, `for`, `foreach`, `while` などのステートメントでは、処理が1行であっても必ず括弧を使用する。括弧は次の行に記述する（Allmanスタイル）。
  ```csharp
  if (condition)
  {
      DoSomething();
  }
  ```
- **`this`キーワード**: メンバー変数へのアクセスであることを明示したい場合にのみ使用する。必須ではない。
- **`var`キーワード**: 型が右辺から自明である場合にのみ使用する。
  ```csharp
  // Good
  var player = new PlayerController();

  // Bad (型が自明でない)
  var result = GetSomeValue();
  ```
- **アクセス修飾子**: すべてのメンバー（フィールド、メソッド、プロパティ）に明示的にアクセス修飾子（`public`, `protected`, `private`）を指定する。

## 6. コメントとドキュメント

- **XMLドキュメントコメント**: すべての `public` なクラス、メソッド、プロパティには `///` を用いたXMLドキュメントコメントを記述する。
- **コメントの内容**: 「何をしているか(What)」ではなく、「なぜそうしているか(Why)」を記述する。複雑なアルゴリズムや、一見して意図が分かりにくい処理の背景を説明する。
- **TODOコメント**: `// TODO: [内容]` の形式で、将来的に修正・実装が必要な箇所を記述する。

## 7. Unity特有の規約

- **`[SerializeField]`の活用**: Inspectorに表示したい変数は、`public`にするのではなく、`private`にして `[SerializeField]` 属性を付与することを原則とする。
  ```csharp
  [SerializeField]
  private float _speed = 5.0f;
  ```
- **`[Header]`と`[Tooltip]`**: Inspectorの可読性を向上させるため、関連するフィールドを `[Header]` でグループ化し、各フィールドには `[Tooltip]` で説明を付与することを推奨する。
- **`ScriptableObject`**:
  - データとロジックを分離するために積極的に活用する。
  - ファイル名はデータの内容が分かるように `PascalCase` で命名する。（例: `PlayerDefaultStats.asset`）
  - 作成メニューは `[CreateAssetMenu]` 属性を用いて、一貫性のあるパスにまとめる。
- **`Awake`, `OnEnable`, `Start`**:
  - `Awake()`: 自身のコンポーネントの初期化（`GetComponent`など）。
  - `OnEnable()`: イベントの登録など、オブジェクトが有効になるたびに行う処理。
  - `Start()`: 他のコンポーネントを参照する初期化。

## 8. アーキテクチャ規約

- **`ServiceLocator`**: グローバルなサービス（`AudioManager`, `InputManager`など）へのアクセスは、必ず`ServiceLocator.Get<T>()`を介して行う。Singletonパターンは原則禁止。
- **`GameEvent`**: システム間の通信は`GameEvent`を介して行う。`Feature`層のクラスが、別の`Feature`層のクラスを直接参照してはならない。
- **`UniTask`**: 非同期処理は、パフォーマンスと可読性の観点から、コルーチンではなく`UniTask`の使用を原則とする。
- **`ICommand`**: ユーザーのアクションやゲーム内の主要な操作は、`ICommand`を実装したコマンドクラスとしてカプセル化する。

