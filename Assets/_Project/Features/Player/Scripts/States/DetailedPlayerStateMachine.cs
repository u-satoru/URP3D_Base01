using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Player.States
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
    /// </summary>
    public class DetailedPlayerStateMachine : MonoBehaviour
    {
        [Header("State Management")]
        [SerializeField] private PlayerStateType currentStateType;
        [SerializeField] private PlayerStateType previousStateType;
        
        [Header("Components")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private StealthMovementController stealthMovement;
        [SerializeField] private CoverSystem coverSystem;
        
        [Header("Events")]
        [SerializeField] private GenericGameEvent<PlayerStateType> onStateChanged;
        [SerializeField] private MovementStanceEvent onStanceChanged;
        
        private IPlayerState currentState;
        private Dictionary<PlayerStateType, IPlayerState> states;
        
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
        
        private void Awake()
        {
            InitializeComponents();
            InitializeStates();
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
        
        private void Start()
        {
            TransitionToState(PlayerStateType.Idle);
        }
        
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
            
            onStateChanged?.Raise(currentStateType);
            
            UpdateMovementStance(newStateType);
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
                PlayerStateType.InCover => MovementStance.Cover,
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
    }
}