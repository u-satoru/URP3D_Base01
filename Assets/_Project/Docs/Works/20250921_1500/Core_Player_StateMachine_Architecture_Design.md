# Core Player StateMachine Architecture Design - 120個コンパイルエラー解決設計書

## 📋 ドキュメント情報

- **作成日**: 2025年9月21日 15:00
- **対象エラー**: DetailedPlayerStateMachine関連120個コンパイルエラー
- **設計方針**: 3層アーキテクチャ（Core ← Feature ← Template）準拠
- **名前空間基準**: `asterivo.Unity60.Core.Player.*` 統一

---

## 🔍 問題分析：Core層基盤クラス欠如によるTemplate層エラー

### 現状エラー分類

#### 1. **プレイヤーStateMachine基盤未実装エラー（80個）**
```csharp
// エラー例
DetailedPlayerStateMachine could not be found
IPlayerState could not be found
PlayerStateType could not be found
```

**影響ファイル**:
- `StealthStateManager.cs` (32箇所)
- `StealthInCoverState.cs` (23箇所) 
- `StealthProneState.cs` (18箇所)
- `StealthCrouchingState.cs` (15箇所)

#### 2. **プレイヤー制御基盤未実装エラー（40個）**
```csharp
// エラー例
PlayerStealthController could not be found
StealthMovementController could not be found
```

**根本原因**: Template層が参照するCore層基盤クラスが存在しない
**アーキテクチャ違反**: Template → Core 依存関係の破綻

---

## 🏗️ アーキテクチャ設計方針

### 設計原則

#### 1. **3層アーキテクチャ完全準拠**
```
Core層（基盤）     ← Feature層（機能）  ← Template層（テンプレート）
├── StateMachine    ├── Player          ├── Stealth
├── Player          ├── AI              ├── ActionRPG
└── Services        └── Camera          └── ...
```

#### 2. **ジェネリック基盤設計**
- Template層の多様性をCore層で抽象化
- 型安全なStateMachine基盤
- 拡張可能なPlayer制御アーキテクチャ

#### 3. **名前空間設計**
```csharp
// Core層基盤（普遍的）
asterivo.Unity60.Core.Player.StateMachine
asterivo.Unity60.Core.Player.Control
asterivo.Unity60.Core.Player.States

// Template層実装（特化）
asterivo.Unity60.Features.Templates.Stealth.Player
asterivo.Unity60.Features.Templates.ActionRPG.Player
```

---

## 🎯 実装対象クラス設計

### Phase 1: StateMachine基盤 (Core層)

#### 1.1 `IPlayerState<T>` インターフェース
```csharp
namespace asterivo.Unity60.Core.Player.StateMachine
{
    /// <summary>
    /// プレイヤー状態の基盤インターフェース
    /// Template層で具体的な状態を実装するための契約
    /// </summary>
    public interface IPlayerState<TStateMachine> : IState<TStateMachine>
        where TStateMachine : class
    {
        // 基本状態制御
        void OnEnter(TStateMachine stateMachine);
        void OnUpdate(TStateMachine stateMachine);
        void OnExit(TStateMachine stateMachine);
        
        // プレイヤー特化機能
        bool CanTransitionTo<TTargetState>() where TTargetState : IPlayerState<TStateMachine>;
        void OnPlayerInput(PlayerInputData inputData);
        void OnMovementUpdate(Vector3 movement, float deltaTime);
        
        // 状態メタデータ
        PlayerStateType StateType { get; }
        float TimeInState { get; }
        bool AllowsMovement { get; }
        bool AllowsActions { get; }
    }
}
```

#### 1.2 `PlayerStateType` Enum
```csharp
namespace asterivo.Unity60.Core.Player.StateMachine
{
    /// <summary>
    /// プレイヤー状態タイプの統一定義
    /// 全Template層で共通使用される状態識別子
    /// </summary>
    [System.Flags]
    public enum PlayerStateType
    {
        // 基本移動状態
        Idle = 1 << 0,
        Walking = 1 << 1,
        Running = 1 << 2,
        Jumping = 1 << 3,
        Falling = 1 << 4,
        
        // 戦闘状態
        Attacking = 1 << 5,
        Blocking = 1 << 6,
        Dodging = 1 << 7,
        
        // ステルス特化状態
        Crouching = 1 << 8,
        Prone = 1 << 9,
        InCover = 1 << 10,
        Sneaking = 1 << 11,
        
        // ActionRPG特化状態
        Casting = 1 << 12,
        Channeling = 1 << 13,
        Stunned = 1 << 14,
        
        // メタ状態
        Transitioning = 1 << 15,
        
        // 組み合わせ状態
        Moving = Walking | Running,
        Stealth = Crouching | Prone | InCover | Sneaking,
        Combat = Attacking | Blocking | Dodging,
        Magic = Casting | Channeling,
        Disabled = Stunned | Transitioning
    }
}
```

#### 1.3 `DetailedPlayerStateMachine<T>` 基盤クラス
```csharp
namespace asterivo.Unity60.Core.Player.StateMachine
{
    /// <summary>
    /// プレイヤー用詳細StateMachine基盤クラス
    /// Template層の多様な要求に対応するジェネリック実装
    /// </summary>
    public abstract class DetailedPlayerStateMachine<TDerived> : MonoBehaviour
        where TDerived : DetailedPlayerStateMachine<TDerived>
    {
        // State Management
        private Dictionary<PlayerStateType, IPlayerState<TDerived>> _states;
        private IPlayerState<TDerived> _currentState;
        private IPlayerState<TDerived> _previousState;
        private PlayerStateType _currentStateType;
        
        // Transition Management
        private Queue<StateTransition<TDerived>> _pendingTransitions;
        private bool _isTransitioning;
        private float _transitionStartTime;
        
        // Input & Movement Integration
        private PlayerInputData _lastInputData;
        private Vector3 _currentMovement;
        private CharacterController _characterController;
        
        // Events
        public System.Action<PlayerStateType, PlayerStateType> OnStateChanged;
        public System.Action<PlayerStateType> OnStateEntered;
        public System.Action<PlayerStateType> OnStateExited;
        
        // Properties
        public PlayerStateType CurrentStateType => _currentStateType;
        public IPlayerState<TDerived> CurrentState => _currentState;
        public IPlayerState<TDerived> PreviousState => _previousState;
        public bool IsTransitioning => _isTransitioning;
        public float TimeInCurrentState { get; private set; }
        
        // Core Methods
        protected virtual void Awake()
        {
            _states = new Dictionary<PlayerStateType, IPlayerState<TDerived>>();
            _pendingTransitions = new Queue<StateTransition<TDerived>>();
            _characterController = GetComponent<CharacterController>();
            InitializeStateMachine();
        }
        
        protected virtual void Update()
        {
            ProcessPendingTransitions();
            UpdateCurrentState();
            TimeInCurrentState += Time.deltaTime;
        }
        
        // State Registration (Template層で使用)
        public void RegisterState<TState>(PlayerStateType stateType, TState state)
            where TState : IPlayerState<TDerived>
        {
            _states[stateType] = state;
        }
        
        // State Transitions
        public bool TryChangeState(PlayerStateType newStateType, bool forced = false)
        {
            if (_states.TryGetValue(newStateType, out var newState))
            {
                if (forced || CanTransitionTo(newStateType))
                {
                    QueueTransition(new StateTransition<TDerived>
                    {
                        FromState = _currentState,
                        ToState = newState,
                        StateType = newStateType,
                        IsForced = forced
                    });
                    return true;
                }
            }
            return false;
        }
        
        public bool CanTransitionTo(PlayerStateType stateType)
        {
            return _currentState?.CanTransitionTo<IPlayerState<TDerived>>() ?? true;
        }
        
        // Input Processing
        public virtual void ProcessInput(PlayerInputData inputData)
        {
            _lastInputData = inputData;
            _currentState?.OnPlayerInput(inputData);
        }
        
        // Movement Processing
        public virtual void ProcessMovement(Vector3 movement)
        {
            _currentMovement = movement;
            _currentState?.OnMovementUpdate(movement, Time.deltaTime);
        }
        
        // Template層でオーバーライド可能な抽象メソッド
        protected abstract void InitializeStateMachine();
        protected abstract void OnStateTransitionCompleted(PlayerStateType from, PlayerStateType to);
        protected abstract bool IsValidTransition(PlayerStateType from, PlayerStateType to);
        
        // Internal Implementation
        private void ProcessPendingTransitions()
        {
            if (_pendingTransitions.Count > 0 && !_isTransitioning)
            {
                var transition = _pendingTransitions.Dequeue();
                ExecuteTransition(transition);
            }
        }
        
        private void QueueTransition(StateTransition<TDerived> transition)
        {
            _pendingTransitions.Enqueue(transition);
        }
        
        private void ExecuteTransition(StateTransition<TDerived> transition)
        {
            _isTransitioning = true;
            _transitionStartTime = Time.time;
            
            // Exit current state
            _currentState?.OnExit(this as TDerived);
            OnStateExited?.Invoke(_currentStateType);
            
            // Update state references
            _previousState = _currentState;
            _currentState = transition.ToState;
            _currentStateType = transition.StateType;
            TimeInCurrentState = 0f;
            
            // Enter new state
            _currentState?.OnEnter(this as TDerived);
            OnStateEntered?.Invoke(_currentStateType);
            OnStateChanged?.Invoke(_previousState?.StateType ?? PlayerStateType.Idle, _currentStateType);
            
            // Complete transition
            _isTransitioning = false;
            OnStateTransitionCompleted(_previousState?.StateType ?? PlayerStateType.Idle, _currentStateType);
        }
        
        private void UpdateCurrentState()
        {
            if (_currentState != null && !_isTransitioning)
            {
                _currentState.OnUpdate(this as TDerived);
            }
        }
    }
    
    // Transition Helper Class
    internal class StateTransition<T>
    {
        public IPlayerState<T> FromState { get; set; }
        public IPlayerState<T> ToState { get; set; }
        public PlayerStateType StateType { get; set; }
        public bool IsForced { get; set; }
        public float TransitionDuration { get; set; } = 0.1f;
    }
}
```

### Phase 2: Player制御基盤 (Core層)

#### 2.1 `PlayerInputData` 構造体
```csharp
namespace asterivo.Unity60.Core.Player
{
    /// <summary>
    /// プレイヤー入力データの統一構造
    /// 全Template層で共通使用される入力情報
    /// </summary>
    [System.Serializable]
    public struct PlayerInputData
    {
        // Movement
        public Vector2 MovementInput;
        public Vector2 LookInput;
        public bool IsRunning;
        public bool IsCrouching;
        
        // Actions
        public bool JumpPressed;
        public bool JumpHeld;
        public bool ActionPressed;
        public bool ActionHeld;
        public bool InteractPressed;
        
        // Combat
        public bool AttackPressed;
        public bool AttackHeld;
        public bool BlockPressed;
        public bool BlockHeld;
        
        // Stealth Specific
        public bool StealthTogglePressed;
        public bool CoverPressed;
        
        // ActionRPG Specific
        public bool CastPressed;
        public bool InventoryPressed;
        
        // Meta
        public float DeltaTime;
        public bool IsValid;
        
        // Helper Methods
        public bool HasMovementInput => MovementInput.magnitude > 0.1f;
        public bool HasLookInput => LookInput.magnitude > 0.1f;
        public bool HasAnyActionInput => JumpPressed || ActionPressed || AttackPressed || InteractPressed;
        
        public static PlayerInputData Empty => new PlayerInputData { IsValid = false };
    }
}
```

#### 2.2 `BasePlayerController` 抽象基盤
```csharp
namespace asterivo.Unity60.Core.Player.Control
{
    /// <summary>
    /// プレイヤー制御の抽象基盤クラス
    /// Template層で具体的な制御を実装するための基盤
    /// </summary>
    public abstract class BasePlayerController : MonoBehaviour
    {
        [Header("Base Player Settings")]
        [SerializeField] protected float baseMovementSpeed = 5f;
        [SerializeField] protected float baseJumpHeight = 1.2f;
        [SerializeField] protected float baseGravity = -9.81f;
        
        // Core Components
        protected CharacterController characterController;
        protected UnityEngine.Camera playerCamera;
        protected Transform cameraTransform;
        
        // State
        protected Vector3 velocity;
        protected bool isGrounded;
        protected float groundedTime;
        
        // Abstract Interface (Template層で実装)
        public abstract void ProcessMovement(PlayerInputData inputData);
        public abstract void ProcessActions(PlayerInputData inputData);
        public abstract void ProcessCameraControl(PlayerInputData inputData);
        
        // Common Implementation
        protected virtual void Awake()
        {
            characterController = GetComponent<CharacterController>();
            playerCamera = UnityEngine.Camera.main;
            if (playerCamera != null)
                cameraTransform = playerCamera.transform;
        }
        
        protected virtual void Update()
        {
            UpdateGroundedState();
            ApplyGravity();
        }
        
        protected virtual void UpdateGroundedState()
        {
            isGrounded = characterController.isGrounded;
            if (isGrounded)
                groundedTime += Time.deltaTime;
            else
                groundedTime = 0f;
        }
        
        protected virtual void ApplyGravity()
        {
            if (isGrounded && velocity.y < 0)
                velocity.y = -2f;
            else
                velocity.y += baseGravity * Time.deltaTime;
        }
        
        // Utility Methods
        protected Vector3 CalculateMovementDirection(Vector2 input)
        {
            if (cameraTransform == null) return Vector3.zero;
            
            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            
            return forward * input.y + right * input.x;
        }
        
        protected void MoveCharacter(Vector3 movement)
        {
            characterController.Move(movement * Time.deltaTime);
        }
        
        protected void Jump(float jumpHeight = 0f)
        {
            if (isGrounded)
            {
                float height = jumpHeight > 0 ? jumpHeight : baseJumpHeight;
                velocity.y = Mathf.Sqrt(height * -2f * baseGravity);
            }
        }
    }
}
```

---

## 🗂️ ファイル構成と実装順序

### 実装順序（依存関係考慮）

#### **Step 1: Core基盤インターフェース（1日目）**
```
Assets/_Project/Core/Player/
├── StateMachine/
│   ├── IPlayerState.cs
│   ├── PlayerStateType.cs
│   └── PlayerInputData.cs
└── Control/
    └── BasePlayerController.cs
```

#### **Step 2: Core実装クラス（2日目）**
```
Assets/_Project/Core/Player/
└── StateMachine/
    ├── DetailedPlayerStateMachine.cs
    ├── StateTransition.cs
    └── PlayerStateMachineEvents.cs
```

#### **Step 3: Template層適用（3日目）**
```
Assets/_Project/Features/Templates/Stealth/Scripts/Player/
├── StealthPlayerStateMachine.cs    # DetailedPlayerStateMachine<StealthPlayerStateMachine>を継承
├── StealthPlayerController.cs      # BasePlayerControllerを継承
└── States/
    ├── StealthIdleState.cs         # IPlayerState<StealthPlayerStateMachine>を実装
    ├── StealthCrouchingState.cs
    ├── StealthProneState.cs
    └── StealthInCoverState.cs
```

---

## 🔧 実装ガイドライン

### Template層での使用例

#### Stealth Template適用例
```csharp
namespace asterivo.Unity60.Features.Templates.Stealth.Player
{
    public class StealthPlayerStateMachine : DetailedPlayerStateMachine<StealthPlayerStateMachine>
    {
        [Header("Stealth Specific Settings")]
        [SerializeField] private float detectionRadius = 10f;
        [SerializeField] private LayerMask enemyLayers;
        
        protected override void InitializeStateMachine()
        {
            // ステルス専用状態を登録
            RegisterState(PlayerStateType.Idle, new StealthIdleState());
            RegisterState(PlayerStateType.Crouching, new StealthCrouchingState());
            RegisterState(PlayerStateType.Prone, new StealthProneState());
            RegisterState(PlayerStateType.InCover, new StealthInCoverState());
            
            // 初期状態設定
            TryChangeState(PlayerStateType.Idle, forced: true);
        }
        
        protected override bool IsValidTransition(PlayerStateType from, PlayerStateType to)
        {
            // ステルス特化の遷移ルール
            if (from == PlayerStateType.InCover && to == PlayerStateType.Running)
                return false; // カバー中は走行不可
                
            return base.IsValidTransition(from, to);
        }
        
        protected override void OnStateTransitionCompleted(PlayerStateType from, PlayerStateType to)
        {
            // ステルス特化の遷移後処理
            if (to == PlayerStateType.InCover)
                NotifyStealthSystem("EnteredCover");
        }
        
        // ステルス特化メソッド
        public bool IsDetectedByEnemies()
        {
            // 敵検知ロジック
            return Physics.OverlapSphere(transform.position, detectionRadius, enemyLayers).Length > 0;
        }
    }
}
```

### ActionRPG Template適用例
```csharp
namespace asterivo.Unity60.Features.Templates.ActionRPG.Player
{
    public class ActionRPGPlayerStateMachine : DetailedPlayerStateMachine<ActionRPGPlayerStateMachine>
    {
        [Header("ActionRPG Specific Settings")]
        [SerializeField] private float mana = 100f;
        [SerializeField] private float stamina = 100f;
        
        protected override void InitializeStateMachine()
        {
            // ActionRPG専用状態を登録
            RegisterState(PlayerStateType.Idle, new ActionRPGIdleState());
            RegisterState(PlayerStateType.Attacking, new ActionRPGAttackingState());
            RegisterState(PlayerStateType.Casting, new ActionRPGCastingState());
            RegisterState(PlayerStateType.Blocking, new ActionRPGBlockingState());
            
            TryChangeState(PlayerStateType.Idle, forced: true);
        }
        
        protected override bool IsValidTransition(PlayerStateType from, PlayerStateType to)
        {
            // ActionRPG特化の遷移ルール
            if (to == PlayerStateType.Casting && mana < 10f)
                return false; // マナ不足で詠唱不可
                
            return base.IsValidTransition(from, to);
        }
        
        // ActionRPG特化メソッド
        public bool CanCastSpell(float manaCost) => mana >= manaCost;
        public bool CanPerformAction(float staminaCost) => stamina >= staminaCost;
    }
}
```

---

## 📊 エラー削減予測

### 実装による削減効果

| Phase | 実装内容 | 削減エラー数 | 残存エラー |
|-------|----------|--------------|------------|
| **現状** | - | - | 270個 |
| **Step 1** | Core基盤インターフェース | 80個 | 190個 |
| **Step 2** | DetailedPlayerStateMachine実装 | 100個 | 90個 |
| **Step 3** | Template層適用 | 70個 | 20個 |
| **統合テスト** | 最終調整 | 15個 | 5個 |

### 期待効果
- **コンパイル成功率**: 270エラー → 5エラー（98%改善）
- **アーキテクチャ整合性**: 3層アーキテクチャ完全準拠
- **拡張性**: 新規Template追加時の基盤完備

---

## ⚠️ リスク分析と軽減策

### 主要リスク

#### 1. **実装複雑度リスク**
- **リスク**: ジェネリック型システムの複雑化
- **軽減策**: 段階的実装とユニットテスト並行実施

#### 2. **パフォーマンスリスク**
- **リスク**: StateMachine処理のオーバーヘッド
- **軽減策**: ObjectPool統合とプロファイリング

#### 3. **既存コード影響リスク**
- **リスク**: 既存Template層との非互換性
- **軽減策**: 漸進的移行とレガシー互換レイヤー

### 成功条件
✅ 全Template層でDetailedPlayerStateMachineが正常動作
✅ パフォーマンス劣化なし（60FPS維持）
✅ 新規Template作成時の開発効率向上

---

## 📋 次のアクション

### 即座実行項目
1. **Core基盤インターフェース実装開始**
2. **Stealth Template適用テスト**
3. **コンパイルエラー段階的削減検証**

この設計書に基づいてCore層基盤クラスを実装することで、120個のアーキテクチャ設計要エラーを根本的に解決し、3層アーキテクチャの完全性を確保できます。