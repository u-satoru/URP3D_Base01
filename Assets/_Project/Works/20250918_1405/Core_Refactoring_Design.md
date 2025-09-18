# Core層リファクタリング 詳細設計書

## 1. 目的とスコープ

このドキュメントは、「Template_Commonality_Analysis.md」で特定された5つの共通機能を`Core`層へリファクタリングするための、具体的な技術設計を定義します。

各機能について、クラス名、ファイルパス、名前空間、主要なメソッドとプロパティ、そして既存アーキテクチャ（ServiceLocator, Event-Driven, Command Pattern）との連携方法を明確に示し、実装の指針とします。

**設計原則**:
- **関心の分離**: Core層は汎用的な「仕組み」を提供し、Features層は具体的な「振る舞い」を実装する。
- **疎結合**: システム間の通信は原則として`GameEvent`または`ServiceLocator`を介して行う。
- **データ駆動**: `ScriptableObject`を活用し、設定やデータをコードから分離する。

---

## 2. 実装設計（優先度順）

### Priority 1: 汎用的な入力管理システム (Generic Input Management)

#### 2.1. 目的
入力デバイスからの生入力を抽象化し、ゲーム全体で利用可能な`GameEvent`に変換する責務を担う。これにより、入力処理とゲームロジックを完全に分離する。

#### 2.2. Coreコンポーネント

**1. `InputService.cs`**
- **ファイルパス**: `Assets/_Project/Core/Input/InputService.cs`
- **名前空間**: `asterivo.Unity60.Core.Input`
- **クラス定義**:
  ```csharp
  [RequireComponent(typeof(PlayerInput))]
  public class InputService : MonoBehaviour, IGameService
  {
      // --- Inspectorに設定するイベントチャネル ---
      [Header("Output Event Channels")]
      [SerializeField] private GameEvent<Vector2> _onMoveInput;
      [SerializeField] private GameEvent _onJumpInputPressed;
      [SerializeField] private GameEvent _onInteractInputPressed;
      [SerializeField] private GameEvent _onFireInputPressed;
      // ... 他の入力イベント

      private PlayerInput _playerInput;

      private void Awake()
      {
          _playerInput = GetComponent<PlayerInput>();
          // ServiceLocatorへの登録
          ServiceLocator.Register<InputService>(this);
      }

      // --- PlayerInputからのコールバックメソッド ---
      public void OnMove(InputValue value)
      {
          _onMoveInput.Raise(value.Get<Vector2>());
      }

      public void OnJump(InputValue value)
      {
          if (value.isPressed)
          {
              _onJumpInputPressed.Raise();
          }
      }
      // ... 他の入力に対するコールバック
  }
  ```
- **責務**:
    - `PlayerInput`コンポーネントと連携し、入力アクションをリッスンする。
    - 受け取った入力を解釈し、対応する`GameEvent`を発行する。
    - `ServiceLocator`に自身を登録する。

**2. `InputEventChannels.asset` (ScriptableObject)**
- **ファイルパス**: `Assets/_Project/Core/ScriptableObjects/Input/InputEventChannels.asset`
- **作成方法**: `CreateAssetMenu`で`InputEventChannels`クラスを作成し、アセットとしてインスタンス化する。
- **クラス定義**:
  ```csharp
  [CreateAssetMenu(fileName = "InputEventChannels", menuName = "Events/Input Event Channels")]
  public class InputEventChannels : ScriptableObject
  {
      public GameEvent<Vector2> OnMoveInput;
      public GameEvent OnJumpInputPressed;
      public GameEvent OnInteractInputPressed;
      public GameEvent OnFireInputPressed;
      // ...
  }
  ```
- **責務**:
    - 入力関連の`GameEvent`を一元管理し、`InputService`や他のリスナーが参照しやすくする。

#### 2.3. 連携フロー
1. `InputService`を持つGameObjectに`PlayerInput`コンポーネントをアタッチし、`Input Actions`アセットを設定する。
2. `InputService`のInspectorで、`InputEventChannels`アセットから対応する`GameEvent`を割り当てる。
3. `Features`層の`PlayerController`などは、`InputEventChannels`の各イベントをリッスンし、キャラクターの制御などを行う。

---

### Priority 2: 汎用的な体力・ダメージシステム (Generic Health & Damage)

#### 2.1. 目的
あらゆるオブジェクト（プレイヤー、敵、破壊可能オブジェクト）に適用可能な、体力とダメージ処理の仕組みを提供する。`Command Pattern`と連携し、処理の再利用性と管理性を高める。

#### 2.2. Coreコンポーネント

**1. `HealthComponent.cs`**
- **ファイルパス**: `Assets/_Project/Core/Components/HealthComponent.cs`
- **名前空間**: `asterivo.Unity60.Core.Combat`
- **クラス定義**:
  ```csharp
  public class HealthComponent : MonoBehaviour
  {
      [SerializeField] private float _maxHealth = 100f;
      private float _currentHealth;

      // --- UIや他システムへの通知用イベント ---
      public event Action<float, float> OnHealthChanged; // current, max
      public event Action OnDied;

      public bool IsDead => _currentHealth <= 0;

      private void Awake()
      {
          _currentHealth = _maxHealth;
      }

      public void Initialize(float maxHealth)
      {
          _maxHealth = maxHealth;
          _currentHealth = maxHealth;
      }

      public void TakeDamage(float amount)
      {
          if (IsDead) return;
          _currentHealth = Mathf.Max(_currentHealth - amount, 0);
          OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

          if (IsDead)
          {
              OnDied?.Invoke();
          }
      }

      public void Heal(float amount)
      {
          if (IsDead) return;
          _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
          OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
      }
  }
  ```
- **責務**:
    - 体力データを管理する。
    - ダメージを受ける、回復するAPIを提供する。
    - 体力の変更や死亡時にC#イベントを発行する。

**2. `DamageCommand.cs`**
- **ファイルパス**: `Assets/_Project/Core/Commands/DamageCommand.cs`
- **名前空間**: `asterivo.Unity60.Core.Commands`
- **クラス定義**:
  ```csharp
  public class DamageCommand : ICommand, IResettableCommand
  {
      private HealthComponent _target;
      private float _damageAmount;

      public void Configure(HealthComponent target, float damageAmount)
      {
          _target = target;
          _damageAmount = damageAmount;
      }

      public void Execute() => _target?.TakeDamage(_damageAmount);
      public void Undo() => _target?.Heal(_damageAmount);
      public bool CanUndo => _target != null;

      public void Reset()
      {
          _target = null;
          _damageAmount = 0;
      }
  }
  ```
- **責務**:
    - `ICommand`および`IResettableCommand`を実装し、`CommandPool`での再利用を可能にする。
    - `HealthComponent`に対してダメージを与える処理をカプセル化する。

#### 2.3. 連携フロー
1. `Features`層の`Projectile.cs`などが衝突を検知する。
2. 衝突対象から`HealthComponent`を取得する。
3. `CommandPoolManager`から`DamageCommand`インスタンスを取得する。
4. コマンドに`HealthComponent`とダメージ量を設定（`Configure`）し、`CommandInvoker`を通じて実行する。
5. `Features`層の`HealthBarUI.cs`は、`HealthComponent`の`OnHealthChanged`イベントを購読し、表示を更新する。

---

### Priority 3: 汎用的なキャラクター制御システム (Generic Character Control)

#### 3.1. 目的
キャラクターの物理挙動と状態管理の基盤を提供する。物理演算を`CharacterMover`に、状態遷移の枠組みを`StateMachine`に分離する。

#### 3.2. Coreコンポーネント

**1. `CharacterMover.cs`**
- **ファイルパス**: `Assets/_Project/Core/Components/CharacterMover.cs`
- **名前空間**: `asterivo.Unity60.Core.Character`
- **クラス定義**:
  ```csharp
  [RequireComponent(typeof(Rigidbody))]
  public class CharacterMover : MonoBehaviour
  {
      private Rigidbody _rigidbody;
      // ... 接地判定、速度制限などの内部変数

      public bool IsGrounded { get; private set; }

      private void Awake() => _rigidbody = GetComponent<Rigidbody>();
      private void FixedUpdate()
      {
          // 接地判定や摩擦などの物理更新
      }

      public void Move(Vector3 direction, float speed)
      {
          // Rigidbody.velocity を使った移動処理
      }

      public void Jump(float force)
      {
          if (!IsGrounded) return;
          // Rigidbody.AddForce を使ったジャンプ処理
      }
  }
  ```
- **責務**:
    - `Rigidbody`を介した物理的な移動・ジャンプAPIを提供する。
    - 接地判定などの物理状態を管理・公開する。

**2. `StateMachine.cs` / `IState.cs`**
- **ファイルパス**: `Assets/_Project/Core/Patterns/StateMachine/`
- **名前空間**: `asterivo.Unity60.Core.Patterns.StateMachine`
- **クラス定義**:
  ```csharp
  public interface IState
  {
      void Enter();
      void Update();
      void Exit();
  }

  public class StateMachine
  {
      private IState _currentState;
      public void ChangeState(IState newState)
      {
          _currentState?.Exit();
          _currentState = newState;
          _currentState.Enter();
      }
      public void Update() => _currentState?.Update();
  }
  ```
- **責務**:
    - 状態遷移のロジックを管理する。
    - 現在の状態の`Update`メソッドを呼び出す。

#### 3.3. 連携フロー
1. `Features`層の`PlayerController.cs`が`StateMachine`インスタンスを持つ。
2. `PlayerController`は`InputService`からのイベントを受け取り、`StateMachine.ChangeState()`を呼び出す。
3. 各Stateクラス（例: `Features/Player/States/WalkState.cs`）は、`Update`メソッド内で`CharacterMover`の`Move()`などを呼び出してキャラクターを動かす。

---

### Priority 4 & 5: Interaction & Camera Systems

これらのシステムは上記3つの基盤の上に構築されるため、設計は分析ドキュメントのものを踏襲します。

- **インタラクションシステム**: `Core`層に`IInteractable`インターフェースと`Interactor`コンポーネントを実装。`Interactor`は`InputService`の`OnInteractInputPressed`イベントを購読する。
- **カメラ制御システム**: `Core`層に`CameraService`と汎用`CameraStateMachine`を実装。`CameraService`は`PlayerStateMachine`の状態変更イベントなどを購読し、適切なカメラ状態に遷移させる。

## 4. 結論

この設計書に基づき、優先度1の`入力管理システム`から実装に着手することで、Core層のリファクタリングを体系的かつ効率的に進めることができます。各コンポーネントは単一責任の原則に従い、プロジェクト全体の疎結合性と再利用性を高めます。
