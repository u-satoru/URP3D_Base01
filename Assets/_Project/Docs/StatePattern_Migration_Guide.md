# ステートパターン移行ガイド

## 📋 概要

現在の`PlayerStateMachine`は巨大なswitch文で状態管理を行っていますが、既に実装済みの`BasePlayerState`クラス群を活用してステートパターンへ移行することで、保守性と拡張性を大幅に向上させることができます。

## 🎯 移行の目的

### 現状の問題点
- **switch文の肥大化**: EnterState/ExitStateメソッドに全状態分のcase文が存在
- **責務の集中**: PlayerStateMachineに全状態のロジックが集中
- **拡張時の影響範囲**: 新しい状態追加時に複数箇所の修正が必要
- **デッドコード**: 実装済みのステートクラスが未使用

### 移行後のメリット
- **単一責任原則**: 各状態が自身の振る舞いを管理
- **オープン/クローズド原則**: 新状態追加時、既存コードの変更不要
- **テスタビリティ向上**: 各状態を独立してテスト可能
- **可読性向上**: 状態ごとのロジックが明確に分離

## 🔧 必要な実装内容

### 1. PlayerStateMachineの改修

#### 1.1 ステートインスタンスの管理
```csharp
public class PlayerStateMachine : MonoBehaviour
{
    // 追加が必要なフィールド
    private Dictionary<PlayerState, BasePlayerState> states;
    private BasePlayerState currentStateInstance;
    private Animator animator;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        InitializeStates();
    }
    
    private void InitializeStates()
    {
        states = new Dictionary<PlayerState, BasePlayerState>
        {
            { PlayerState.Idle, new IdleState(this, animator) },
            { PlayerState.Walking, new WalkingState(this, animator) },
            { PlayerState.Running, new RunningState(this, animator) },
            { PlayerState.Sprinting, new SprintingState(this, animator) },
            { PlayerState.Jumping, new JumpingState(this, animator) },
            { PlayerState.Falling, new FallingState(this, animator) },
            { PlayerState.Landing, new LandingState(this, animator) },
            { PlayerState.Combat, new CombatState(this, animator) },
            { PlayerState.CombatAttacking, new CombatAttackingState(this, animator) },
            { PlayerState.Interacting, new InteractingState(this, animator) },
            { PlayerState.Dead, new DeadState(this, animator) }
        };
        
        // 初期状態のインスタンスを設定
        currentStateInstance = states[currentState];
        currentStateInstance.Enter();
    }
}
```

#### 1.2 Update系メソッドの追加
```csharp
private void Update()
{
    currentStateInstance?.Update();
}

private void FixedUpdate()
{
    currentStateInstance?.FixedUpdate();
}

private void LateUpdate()
{
    currentStateInstance?.LateUpdate();
}
```

#### 1.3 ChangeStateメソッドの改修
```csharp
public void ChangeState(PlayerState newState)
{
    if (currentState == newState)
        return;
    
    // 遷移可能かチェック
    if (!currentStateInstance.CanTransitionTo(newState))
    {
        if (enableDebugLog)
            Debug.LogWarning($"Cannot transition from {currentState} to {newState}");
        return;
    }
    
    PlayerState oldState = currentState;
    
    // 現在の状態を終了
    currentStateInstance?.Exit();
    
    // 状態を更新
    previousState = currentState;
    currentState = newState;
    
    // 新しい状態のインスタンスを取得して開始
    currentStateInstance = states[newState];
    currentStateInstance.Enter();
    
    // イベント通知
    OnStateChanged?.Invoke(oldState, currentState);
    
    if (stateChangedEvent != null)
    {
        stateChangedEvent.Raise(currentState);
    }
    
    if (enableDebugLog)
    {
        Debug.Log($"State changed: {oldState} -> {currentState}");
    }
}
```

#### 1.4 不要なメソッドの削除
以下のメソッドとリージョンを削除：
- `EnterState(PlayerState state)` メソッド全体
- `ExitState(PlayerState state)` メソッド全体
- `#region State Enter Methods` 全体
- `#region State Exit Methods` 全体

### 2. BasePlayerStateクラスの拡張

#### 2.1 共通機能の追加
```csharp
public abstract class BasePlayerState
{
    // PlayerControllerへの参照を追加（必要に応じて）
    protected PlayerController playerController;
    
    // イベントチャネルへの参照（必要に応じて）
    protected GameEvent onStateEntered;
    protected GameEvent onStateExited;
    
    protected BasePlayerState(PlayerStateMachine sm, Animator anim)
    {
        stateMachine = sm;
        animator = anim;
        // PlayerControllerの取得
        playerController = sm.GetComponent<PlayerController>();
    }
    
    // 物理演算用のメソッド
    public virtual void HandleInput(Vector2 moveInput, bool jumpPressed) { }
}
```

### 3. 個別ステートクラスの充実化

#### 3.1 各ステートクラスのロジック実装
現在は基本的なアニメーター制御のみですが、以下を追加：

```csharp
public class WalkingState : BasePlayerState
{
    public override void Update()
    {
        base.Update();
        
        // 移動速度のチェック
        float currentSpeed = playerController?.CurrentSpeed ?? 0f;
        
        // 速度に応じた状態遷移の判定
        if (currentSpeed < 0.1f)
        {
            stateMachine.TransitionTo(PlayerState.Idle);
        }
        else if (currentSpeed > 4.5f)
        {
            stateMachine.TransitionTo(PlayerState.Running);
        }
        
        // アニメーション速度の更新
        animator?.SetFloat("Speed", currentSpeed);
    }
    
    public override void HandleInput(Vector2 moveInput, bool jumpPressed)
    {
        if (jumpPressed)
        {
            stateMachine.TransitionTo(PlayerState.Jumping);
        }
    }
}
```

### 4. PlayerControllerとの連携

#### 4.1 状態依存の処理を委譲
```csharp
// PlayerController.csの修正
private void Update()
{
    // 入力の取得
    Vector2 moveInput = GetMoveInput();
    bool jumpPressed = GetJumpInput();
    
    // 現在の状態に入力を渡す（ステートマシン経由）
    playerStateMachine?.HandleInput(moveInput, jumpPressed);
    
    // 状態に依存しない共通処理のみ実行
    UpdatePhysics();
}
```

## 📝 移行手順

### Phase 1: 基盤準備（リスク: 低）
1. ✅ BasePlayerStateクラスは既に実装済み
2. ⬜ PlayerStateMachineにステート管理用のDictionaryを追加
3. ⬜ InitializeStatesメソッドを実装
4. ⬜ Update/FixedUpdate/LateUpdateメソッドを追加

### Phase 2: 段階的移行（リスク: 中）
1. ⬜ ChangeStateメソッドをステートインスタンス使用に変更
2. ⬜ 1つの状態（例: IdleState）から段階的に移行
3. ⬜ 動作確認後、順次他の状態も移行
4. ⬜ 全状態の移行完了後、switch文を削除

### Phase 3: 機能拡張（リスク: 低）
1. ⬜ 各ステートクラスに固有のロジックを実装
2. ⬜ PlayerControllerから状態依存の処理を移動
3. ⬜ 状態遷移条件をステートクラス内に実装

### Phase 4: 最適化（オプション）
1. ⬜ ステートプールの実装（頻繁な生成/破棄を避ける）
2. ⬜ 状態遷移のバリデーション強化
3. ⬜ デバッグビジュアライザーの追加

## ⚠️ 注意事項

### 移行時の考慮点
- **後方互換性**: イベントシステムとの連携は維持
- **段階的移行**: 一度に全て変更せず、段階的に実施
- **テスト**: 各フェーズ後に動作確認を実施
- **バックアップ**: 移行前の状態を保存

### リスク管理
- **Phase 1**: 既存動作に影響なし（新規追加のみ）
- **Phase 2**: 慎重な動作確認が必要
- **Phase 3**: 機能追加のため、リスクは限定的
- **Phase 4**: パフォーマンス向上のため、オプション

## 🎯 期待される成果

### 定量的効果
- コード行数: 約40%削減（switch文の除去）
- 新状態追加時の修正箇所: 3箇所 → 1箇所
- テストケース作成時間: 約50%削減

### 定性的効果
- 可読性の大幅向上
- デバッグの容易化
- チーム開発での並行作業が可能
- 将来の拡張が容易

## 📚 参考資料

- [State Pattern - Gang of Four](https://en.wikipedia.org/wiki/State_pattern)
- [Unity State Machine Best Practices](https://docs.unity3d.com/Manual/StateMachineBehaviours.html)
- [SOLID Principles in Game Development](https://unity.com/how-to/architect-game-code-scriptable-objects)

---

**作成日**: 2025-08-30  
**バージョン**: 1.0  
**対象**: Unity 6 (6000.0.42f1)  
**前提条件**: BasePlayerStateクラス群が実装済み