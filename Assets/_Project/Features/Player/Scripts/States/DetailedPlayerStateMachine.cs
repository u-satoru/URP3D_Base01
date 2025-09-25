using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Data;
using asterivo.Unity60.Features.Player;

namespace asterivo.Unity60.Features.Player.States
{
    /// <summary>
    /// プレイヤーの行動状態を定義します。
    /// </summary>
    public enum PlayerStateType
    {
        Idle,       // 待機状態
        Walking,    // 歩行状態
        Running,    // 走行状態
        Jumping,    // ジャンプ状態
        Crouching,  // しゃがみ状態
        Prone,      // 伏せ状態
        InCover,    // カバー中状態
        Climbing,   // 登り状態
        Swimming,   // 水泳状態
        Rolling,    // ローリング状態
        Dead        // 死亡状態
    }

    /// <summary>
    /// プレイヤーの詳細な状態を管理するステートマシンです。
    /// ステートパターンを利用して、各状態（歩行、ジャンプ、カバーなど）の振る舞いをカプセル化し、状態遷移を制御します。
    /// 
    /// レガシーPlayerStateMachineとの互換性を提供し、段階的な移行をサポートします。
    /// </summary>
    public class DetailedPlayerStateMachine : MonoBehaviour
    {
        [Header("State Management")]
        /// <summary>
        /// 現在のプレイヤーの状態タイプ。
        /// </summary>
        [SerializeField] private PlayerStateType currentStateType;
        /// <summary>
        /// 直前のプレイヤーの状態タイプ。
        /// </summary>
        [SerializeField] private PlayerStateType previousStateType;
        
        [Header("Components")]
        /// <summary>
        /// プレイヤーのCharacterControllerコンポーネント。
        /// </summary>
        [SerializeField] private CharacterController characterController;
        /// <summary>
        /// プレイヤーのステルス移動を制御するコントローラー。
        /// </summary>
        [SerializeField] private StealthMovementController stealthMovement;
        /// <summary>
        /// プレイヤーのカバーシステムを制御するコンポーネント。
        /// </summary>
        [SerializeField] private CoverSystem coverSystem;
        
        [Header("Events")]
        /// <summary>
        /// プレイヤーの状態が変更されたときに発行されるイベント。
        /// </summary>
        [SerializeField] private GameEvent<PlayerStateType> onStateChanged;
        /// <summary>
        /// プレイヤーの移動姿勢（立位、しゃがみなど）が変更されたときに発行されるイベント。
        /// </summary>
        [SerializeField] private MovementStanceEvent onStanceChanged;
        
        [Header("Legacy Event Compatibility")]
        /// <summary>
        /// レガシーPlayerStateイベント（enum値）との互換性用。
        /// </summary>
        [SerializeField] private PlayerStateEvent legacyStateChangeRequestEvent;
        /// <summary>
        /// レガシーPlayerState変更通知（enum値）。
        /// </summary>
        [SerializeField] private PlayerStateEvent legacyStateChangedEvent;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLog = true;
        
        private IPlayerState currentState;
        private Dictionary<PlayerStateType, IPlayerState> states;
        
        // レガシー互換性のためのPlayerState追跡
        private PlayerState legacyCurrentState = PlayerState.Idle;
        private PlayerState legacyPreviousState = PlayerState.Idle;
        
        // イベントリスナー管理 - 循環依存回避のため一時的にコメントアウト
        // private PlayerStateEventListener stateChangeListener;
        
        // 状態変更イベント
        public event System.Action<PlayerState, PlayerState> OnLegacyStateChanged;

        /// <summary>
        /// プレイヤー状態変更イベント (新しいPlayerStateType用)
        /// StealthMechanicsController等の他システムが購読可能
        /// </summary>
        public event System.Action<PlayerStateType> OnStateChanged;
        
        /// <summary>
        /// プレイヤーのCharacterControllerコンポーネント。
        /// </summary>
        public CharacterController CharacterController => characterController;
        
        /// <summary>
        /// プレイヤーのステルス移動コントローラー。
        /// </summary>
        public StealthMovementController StealthMovement => stealthMovement;
        
        /// <summary>
        /// プレイヤーのカバーシステム。
        /// </summary>
        public CoverSystem CoverSystem => coverSystem;
        
        /// <summary>
        /// スクリプトインスタンスがロードされたときに呼び出され、コンポーネントと状態を初期化します。
        /// </summary>
        private void Awake()
        {
            InitializeComponents();
            InitializeStates();
        }
        
        /// <summary>
        /// オブジェクトが有効になったときにイベントリスナーを登録します。
        /// </summary>
        private void OnEnable()
        {
            RegisterEventListeners();
        }
        
        /// <summary>
        /// オブジェクトが無効になったときにイベントリスナーを解除します。
        /// </summary>
        private void OnDisable()
        {
            UnregisterEventListeners();
        }
        
        /// <summary>
        /// 必要なコンポーネントを初期化または取得します。
        /// </summary>
        private void InitializeComponents()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
                
            if (stealthMovement == null)
                stealthMovement = GetComponent<StealthMovementController>();
                
            if (coverSystem == null)
                coverSystem = GetComponent<CoverSystem>();
        }
        
        /// <summary>
        /// 各状態クラスのインスタンスを生成し、ディクショナリに登録します。
        /// </summary>
        private void InitializeStates()
        {
            states = new Dictionary<PlayerStateType, IPlayerState>
            {
                { PlayerStateType.Idle, new IdleState() },
                { PlayerStateType.Walking, new WalkingState() },
                { PlayerStateType.Running, new RunningState() },
                { PlayerStateType.Jumping, new JumpingState() },
                { PlayerStateType.Crouching, new CrouchingState() },
                { PlayerStateType.Prone, new ProneState() },
                { PlayerStateType.InCover, new CoverState() },
                { PlayerStateType.Rolling, new RollingState() }
            };
        }
        
        /// <summary>
        /// 最初のフレーム更新の前に呼び出され、初期状態に遷移します。
        /// </summary>
        private void Start()
        {
            TransitionToState(PlayerStateType.Idle);
        }
        
        /// <summary>
        /// フレームごとに呼び出され、現在の状態のUpdateロジックを実行します。
        /// </summary>
        private void Update()
        {
            currentState?.Update(this);
        }

        /// <summary>
        /// 現在のステートに入力情報を渡して処理させます。
        /// </summary>
        /// <param name="moveInput">移動入力。</param>
        /// <param name="jumpInput">ジャンプ入力。</param>
        public void HandleInput(Vector2 moveInput, bool jumpInput)
        {
            currentState?.HandleInput(this, moveInput, jumpInput);
        }
        
        /// <summary>
        /// 固定フレームレートで呼び出され、現在の状態のFixedUpdateロジックを実行します。
        /// 主に物理演算の更新に使用されます。
        /// </summary>
        private void FixedUpdate()
        {
            currentState?.FixedUpdate(this);
        }
        
        /// <summary>
        /// 指定された新しい状態に遷移します。
        /// </summary>
        /// <param name="newStateType">遷移先の新しい状態タイプ。</param>
        public void TransitionToState(PlayerStateType newStateType)
        {
            if (currentStateType == newStateType && currentState != null)
                return;
                
            if (!states.ContainsKey(newStateType))
            {
                Debug.LogWarning($"State {newStateType} not found in state machine");
                return;
            }
            
            previousStateType = currentStateType;
            currentState?.Exit(this);
            
            currentStateType = newStateType;
            currentState = states[newStateType];
            currentState.Enter(this);
            
            // 新しいイベントシステム
            onStateChanged?.Raise(currentStateType);

            // C#イベント（StealthMechanicsController等向け）
            OnStateChanged?.Invoke(currentStateType);

            // レガシー互換性の状態変更
            UpdateLegacyState(newStateType);
            
            UpdateMovementStance(newStateType);
            
            if (enableDebugLog)
            {
                Debug.Log($"State transitioned: {previousStateType} -> {currentStateType}");
            }
        }
        
        /// <summary>
        /// 状態の変更に応じて、キャラクターの姿勢（立位、しゃがみなど）を更新し、イベントを発行します。
        /// </summary>
        /// <param name="stateType">現在の状態タイプ。</param>
        private void UpdateMovementStance(PlayerStateType stateType)
        {
            MovementStance stance = stateType switch
            {
                PlayerStateType.Crouching => MovementStance.Crouching,
                PlayerStateType.Prone => MovementStance.Prone,
                PlayerStateType.InCover => MovementStance.Crouching, // Cover state uses crouching posture
                _ => MovementStance.Standing
            };
            
            onStanceChanged?.Raise(stance);
        }
        
        /// <summary>
        /// 直前の状態に戻ります。
        /// </summary>
        public void ReturnToPreviousState()
        {
            TransitionToState(previousStateType);
        }
        
        /// <summary>
        /// 現在の状態タイプを取得します。
        /// </summary>
        /// <returns>現在の状態タイプ。</returns>
        public PlayerStateType GetCurrentStateType() => currentStateType;

        /// <summary>
        /// 直前の状態タイプを取得します。
        /// </summary>
        /// <returns>直前の状態タイプ。</returns>
        public PlayerStateType GetPreviousStateType() => previousStateType;
        
        /// <summary>
        /// 指定された状態であるかどうかを確認します。
        /// </summary>
        /// <param name="stateType">確認したい状態タイプ。</param>
        /// <returns>指定された状態であればtrue、そうでなければfalse。</returns>
        public bool IsInState(PlayerStateType stateType)
        {
            return currentStateType == stateType;
        }
        
        /// <summary>
        /// 指定された状態に遷移可能かどうかを判定します。
        /// </summary>
        /// <param name="targetState">遷移先の目標状態。</param>
        /// <returns>遷移可能であればtrue、そうでなければfalse。</returns>
        public bool CanTransitionTo(PlayerStateType targetState)
        {
            return targetState switch
            {
                PlayerStateType.Dead => true,
                PlayerStateType.InCover => coverSystem != null && coverSystem.GetAvailableCovers().Count > 0,
                PlayerStateType.Prone => currentStateType != PlayerStateType.InCover,
                _ => true
            };
        }
        
        #region Legacy Compatibility Methods
        
        /// <summary>
        /// イベントリスナーを登録します。
        /// </summary>
        private void RegisterEventListeners()
        {
            // Temporarily disabled due to circular dependency with PlayerStateEventListener
            // if (legacyStateChangeRequestEvent != null)
            // {
            //     stateChangeListener = gameObject.GetComponent<PlayerStateEventListener>();
            //     if (stateChangeListener == null)
            //     {
            //         stateChangeListener = gameObject.AddComponent<PlayerStateEventListener>();
            //     }
            //     stateChangeListener.GameEvent = legacyStateChangeRequestEvent;
            //     stateChangeListener.Response.AddListener(OnLegacyStateChangeRequested);
            // }
        }
        
        /// <summary>
        /// イベントリスナーを解除します。
        /// </summary>
        private void UnregisterEventListeners()
        {
            // Temporarily disabled due to circular dependency with PlayerStateEventListener
            // if (stateChangeListener != null)
            // {
            //     stateChangeListener.Response.RemoveListener(OnLegacyStateChangeRequested);
            //     Destroy(stateChangeListener);
            //     stateChangeListener = null;
            // }
        }
        
        /// <summary>
        /// レガシー状態変更リクエストを受信します。
        /// </summary>
        /// <param name="newState">変更先の状態</param>
        private void OnLegacyStateChangeRequested(PlayerState newState)
        {
            PlayerStateType newStateType = ConvertLegacyToNewState(newState);
            TransitionToState(newStateType);
        }
        
        /// <summary>
        /// PlayerStateTypeの変更に応じてレガシーPlayerStateを更新します。
        /// </summary>
        /// <param name="newStateType">新しい状態タイプ</param>
        private void UpdateLegacyState(PlayerStateType newStateType)
        {
            PlayerState newLegacyState = ConvertNewToLegacyState(newStateType);
            
            if (legacyCurrentState != newLegacyState)
            {
                PlayerState oldLegacyState = legacyCurrentState;
                legacyPreviousState = legacyCurrentState;
                legacyCurrentState = newLegacyState;
                
                OnLegacyStateChanged?.Invoke(oldLegacyState, legacyCurrentState);
                legacyStateChangedEvent?.Raise((int)legacyCurrentState);
                
                if (enableDebugLog)
                {
                    Debug.Log($"Legacy state updated: {oldLegacyState} -> {legacyCurrentState}");
                }
            }
        }
        
        /// <summary>
        /// PlayerStateをPlayerStateTypeに変換します。
        /// </summary>
        /// <param name="legacyState">レガシー状態</param>
        /// <returns>対応する新しい状態タイプ</returns>
        private PlayerStateType ConvertLegacyToNewState(PlayerState legacyState)
        {
            return legacyState switch
            {
                PlayerState.Idle => PlayerStateType.Idle,
                PlayerState.Walking => PlayerStateType.Walking,
                PlayerState.Running => PlayerStateType.Running,
                PlayerState.Sprinting => PlayerStateType.Running, // スプリントは走行に統合
                PlayerState.Jumping => PlayerStateType.Jumping,
                PlayerState.Falling => PlayerStateType.Jumping, // 落下はジャンプに統合
                PlayerState.Landing => PlayerStateType.Idle, // 着地は待機に遷移
                PlayerState.Combat => PlayerStateType.Idle, // 戦闘は待機に統合
                PlayerState.CombatAttacking => PlayerStateType.Idle,
                PlayerState.Interacting => PlayerStateType.Idle,
                PlayerState.Dead => PlayerStateType.Dead,
                _ => PlayerStateType.Idle
            };
        }
        
        /// <summary>
        /// PlayerStateTypeをPlayerStateに変換します。
        /// </summary>
        /// <param name="newStateType">新しい状態タイプ</param>
        /// <returns>対応するレガシー状態</returns>
        private PlayerState ConvertNewToLegacyState(PlayerStateType newStateType)
        {
            return newStateType switch
            {
                PlayerStateType.Idle => PlayerState.Idle,
                PlayerStateType.Walking => PlayerState.Walking,
                PlayerStateType.Running => PlayerState.Running,
                PlayerStateType.Jumping => PlayerState.Jumping,
                PlayerStateType.Crouching => PlayerState.Idle, // しゃがみは待機として扱う
                PlayerStateType.Prone => PlayerState.Idle, // 伏せは待機として扱う
                PlayerStateType.InCover => PlayerState.Idle, // カバーは待機として扱う
                PlayerStateType.Climbing => PlayerState.Idle,
                PlayerStateType.Swimming => PlayerState.Idle,
                PlayerStateType.Rolling => PlayerState.Idle,
                PlayerStateType.Dead => PlayerState.Dead,
                _ => PlayerState.Idle
            };
        }
        
        /// <summary>
        /// レガシーAPIのサポート: 現在のPlayerState（enum）を取得します。
        /// </summary>
        public PlayerState GetLegacyCurrentState() => legacyCurrentState;
        
        /// <summary>
        /// レガシーAPIのサポート: 直前のPlayerState（enum）を取得します。
        /// </summary>
        public PlayerState GetLegacyPreviousState() => legacyPreviousState;
        
        /// <summary>
        /// レガシーAPIのサポート: 指定された状態かどうかを確認します。
        /// </summary>
        /// <param name="state">確認するレガシー状態</param>
        /// <returns>現在の状態が指定した状態と一致するかどうか</returns>
        public bool IsInLegacyState(PlayerState state)
        {
            return legacyCurrentState == state;
        }
        
        /// <summary>
        /// レガシーAPIのサポート: 複数の状態のいずれかに該当するかを確認します。
        /// </summary>
        /// <param name="states">確認する状態の配列</param>
        /// <returns>現在の状態が指定した状態のいずれかと一致するかどうか</returns>
        public bool IsInAnyLegacyState(params PlayerState[] states)
        {
            foreach (var state in states)
            {
                if (legacyCurrentState == state)
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// レガシーAPIのサポート: 前の状態に戻ります。
        /// </summary>
        public void RevertToLegacyPreviousState()
        {
            PlayerStateType targetState = ConvertLegacyToNewState(legacyPreviousState);
            TransitionToState(targetState);
        }
        
        /// <summary>
        /// レガシーAPIのサポート: PlayerState経由で状態遷移を行います。
        /// </summary>
        /// <param name="newState">遷移先のレガシー状態</param>
        public void TransitionToLegacyState(PlayerState newState)
        {
            PlayerStateType newStateType = ConvertLegacyToNewState(newState);
            TransitionToState(newStateType);
        }
        
        #endregion
    }
}
