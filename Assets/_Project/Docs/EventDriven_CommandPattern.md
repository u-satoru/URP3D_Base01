# イベント駆動とCommandパターンの融合：高度な設計解説

## 1. はじめに

このドキュメントは、Unityのイベント駆動アーキテクチャ（EDA）に**ScriptableObjectベースのイベントシステム**と**Commandパターン**を統合する手法について解説します。

既存の堅牢なイベントシステムを基盤とし、Commandパターンを導入することで、入力処理と実行ロジックをさらに分離し、より柔軟で拡張性の高いアーキテクチャを構築することを目的とします。

## 2. なぜこの組み合わせなのか？ 利点の探求

ScriptableObjectイベントとCommandパターンを組み合わせることで、以下のような強力な利点を享受できます。

| 利点 | 説明 |
| :--- | :--- |
| **究極の関心事の分離** | **入力受付**（`PlayerController`）、**コマンド生成**（`PlayerController`内）、**コマンド伝達**（`CommandEvent`）、**コマンド実行**（`CommandProcessor`）、そして**状態別処理**（`PlayerStateMachine`）が完全に分離されます。 |
| **Undo/Redo機能の実装** | `CommandProcessor`が実行したコマンドの履歴をスタックに保持することで、Undo/Redoが容易に実装可能になります。`ICommand`に`Undo()`メソッドを追加するだけです。 |
| **テスト容易性の向上** | 各コマンドは単体でテスト可能な独立したクラスです。また、`PlayerController`のテストでは、イベントが発行されたかを確認するだけで済み、実際の動作を気にする必要がありません。 |
| **入力ソースの抽象化** | プレイヤー入力だけでなく、AI、ネットワーク、リプレイデータなど、様々なソースから同じコマンドを発行するだけでキャラクターを操作できます。 |
| **遅延実行とキューイング** | コマンドを即時実行せず、キューに溜めておき、特定のタイミングで一括処理する、といった制御が容易になります。 |
| **デバッグの容易さ** | `CommandEvent`を監視するだけで、システム内を流れる全コマンドをログに出力でき、何が起きているかを正確に追跡できます。 |

## 3. 実装例：Stateパターンとの連携

`StatePattern_Migration_Guide.md`で提案されている**ステートパターン**と連携させることで、Commandパターンは真価を発揮します。入力コマンドを`PlayerStateMachine`に渡し、現在の状態（`IdleState`, `WalkingState`など）がその入力をどう処理するかを決定します。

### データフロー
`入力` -> `PlayerController` -> `コマンド生成` -> `CommandEvent` -> `CommandProcessor` -> `PlayerStateMachine.HandleInput()` -> `現在のステートクラスが処理`

---

### Step 1: Commandの基本構造を定義する

まず、すべてのコマンドの基礎となるインターフェースと、それを運ぶためのイベントを作成します。

#### `ICommand.cs`
すべてのコマンドが実装すべき`Execute`メソッドを定義します。

```csharp
// Assets/Core/Commands/ICommand.cs
namespace Unity6.Core.Commands
{
    public interface ICommand
    {
        void Execute();
    }
}
```

#### `CommandGameEvent.cs`
`ICommand`をペイロードとして運ぶための専用イベントチャネルです。

```csharp
// Assets/Core/Events/CommandGameEvent.cs
using UnityEngine;
using Unity6.Core.Commands;

namespace Unity6.Core.Events
{
    [CreateAssetMenu(fileName = "Command Event", menuName = "Unity6/Events/Command Event")]
    public class CommandGameEvent : GenericGameEvent<ICommand> { }
}
```
**忘れずに、`Assets/ScriptableObjects/Events`フォルダ内にこのアセットを作成してください。**

### Step 2: 具体的なコマンドクラスを作成する (Stateパターン連携版)

コマンドは`PlayerController`ではなく、**`PlayerStateMachine`** への参照を保持し、その`HandleInput`メソッドを呼び出します。

#### `MoveCommand.cs`

```csharp
// Assets/Player/Commands/MoveCommand.cs
using UnityEngine;
using Unity6.Core.Commands;

namespace Unity6.Player.Commands
{
    public class MoveCommand : ICommand
    {
        private readonly PlayerStateMachine _stateMachine;
        private readonly Vector2 _direction;

        public MoveCommand(PlayerStateMachine stateMachine, Vector2 direction)
        {
            _stateMachine = stateMachine;
            _direction = direction;
        }

        public void Execute()
        {
            // StateMachineに移動入力を渡す
            _stateMachine.HandleInput(_direction, false);
        }
    }
}
```

#### `JumpCommand.cs`

```csharp
// Assets/Player/Commands/JumpCommand.cs
using UnityEngine;
using Unity6.Core.Commands;

namespace Unity6.Player.Commands
{
    public class JumpCommand : ICommand
    {
        private readonly PlayerStateMachine _stateMachine;

        public JumpCommand(PlayerStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Execute()
        {
            // StateMachineにジャンプ入力を渡す
            _stateMachine.HandleInput(Vector2.zero, true);
        }
    }
}
```

### Step 3: コマンドを実行する処理系を実装する

`GameManager`にコマンドを処理する機能を追加します。

#### `GameManager.cs` の修正

```csharp
// Assets/Systems/GameManager.cs
using UnityEngine;
using Unity6.Core.Commands;
using Unity6.Core.Events;

namespace Unity6.Systems
{
    public class GameManager : MonoBehaviour
    {
        [Header("Command System")]
        [SerializeField] private CommandGameEvent onCommandReceived;
        
        // 他のプロパティ...

        private void OnEnable()
        {
            if (onCommandReceived != null)
            {
                onCommandReceived.RegisterListener(this);
            }
        }

        private void OnDisable()
        {
            if (onCommandReceived != null)
            {
                onCommandReceived.UnregisterListener(this);
            }
        }

        // IGameEventListener<ICommand>を実装していると仮定
        public void OnEventRaised(ICommand command)
        {
            command?.Execute();
        }
        
        // ...既存のコード...
    }
}
```
*注: `GameManager`が`IGameEventListener<ICommand>`を実装するように修正し、`OnEventRaised(ICommand command)`メソッドを持たせる必要があります。*

### Step 4: `PlayerController` を修正し、コマンドを発行する

`PlayerController`は`PlayerStateMachine`への参照を保持し、コマンド生成時にそれを渡します。

```csharp
// Assets/Player/PlayerController.cs
using UnityEngine;
using UnityEngine.InputSystem;
using Unity6.Core.Events;
using Unity6.Core.Commands;
using Unity6.Player.Commands;

namespace Unity6.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerStateMachine playerStateMachine; // StateMachineへの参照

        #region Events - Output
        [Header("Command Events (Output)")]
        [SerializeField] private CommandGameEvent onCommandIssued;
        #endregion

        // ...

        private void Awake()
        {
            // playerStateMachineがnullの場合のエラーチェックを追加
            if (playerStateMachine == null)
            {
                Debug.LogError("PlayerStateMachine is not assigned!", this);
                enabled = false;
                return;
            }
        }

        #region Input Callbacks
        private void OnMove(InputAction.CallbackContext context)
        {
            if (movementFrozen) return;
            
            var moveInput = context.ReadValue<Vector2>();
            var command = new MoveCommand(playerStateMachine, moveInput);
            onCommandIssued?.Raise(command);
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            var command = new MoveCommand(playerStateMachine, Vector2.zero);
            onCommandIssued?.Raise(command);
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            if (movementFrozen) return;
            
            var command = new JumpCommand(playerStateMachine);
            onCommandIssued?.Raise(command);
        }
        #endregion

        // ... PlayerControllerから移動やジャンプの直接的な処理ロジックは削除され、
        //     StateMachineと各Stateクラスに移行される ...
    }
}
```

## 4. イベント駆動アーキテクチャの注意点とベストプラクティス

このアーキテクチャを効果的に活用するためには、以下の点に注意が必要です。

1.  **イベントの乱用を避ける**:
    *   何でもかんでもイベントにすると、処理の流れが追いにくくなります。密結合が許容される、あるいは直接参照が自然な場合は、無理にイベントを使わない判断も重要です。

2.  **イベントチェーンのデバッグ**:
    *   `A -> B -> C` のようにイベントが連鎖すると、問題発生時の原因特定が困難になることがあります。`GameEvent`のデバッグ機能を活用し、どのイベントがどの順序で発行されたかをログに出力できるようにしておくと良いでしょう。

3.  **パフォーマンスへの配慮**:
    *   `Update`内で毎フレーム大量のイベントを発行すると、オーバーヘッドが問題になる可能性があります。特に文字列や複雑なオブジェクトをペイロードとして渡す場合は注意が必要です。値の変更があった場合のみイベントを発行するなどの工夫が有効です。

4.  **命名規則の統一**:
    *   イベントアセット名は `OnPlayerJumped` や `RequestPauseMenu` のように、**過去形**（通知）か**命令形**（要求）かを明確に区別すると、意図が分かりやすくなります。

5.  **イベントとデータの分離**:
    *   イベントは「何かが起こった」という事実を伝えるトリガーとして使い、永続的なデータは`ScriptableObject`などで別途管理する方が見通しが良くなります。

## 5. まとめ

ScriptableObjectイベントとCommandパターン、そしてStateパターンを組み合わせることで、Unityプロジェクトの設計は新たな次元に到達します。**入力、ロジック、実行、状態管理**が綺麗に分離され、テスト、拡張、デバッグが格段に容易になります。

最初は少し複雑に感じるかもしれませんが、この設計に慣れると、大規模で複雑なゲームロジックも驚くほどクリーンに保つことができるようになります。
