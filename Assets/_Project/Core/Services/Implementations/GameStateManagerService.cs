using UnityEngine;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Services.Interfaces;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Types; // GameState enum

namespace asterivo.Unity60.Core.Services.Implementations
{
    /// <summary>
    /// GameManagerのゲーム状態関連を委譲するサービス実装
    /// ServiceLocatorパターンに準拠したIService実装
    /// </summary>
    public class GameStateManagerService : MonoBehaviour, IGameStateManager, IService, IServiceLocatorRegistrable
    {
        [Header("Events")]
        [SerializeField] private GameEvent<GameState> gameStateChangedEvent;

        [Header("Runtime")]
        [SerializeField] private GameState currentGameState = GameState.MainMenu;
        [SerializeField] private GameState previousGameState = GameState.MainMenu;

        [SerializeField] private int priority = 50;

        // IService実装
        public string ServiceName => "GameStateManagerService";
        public bool IsServiceActive { get; private set; }

        // IServiceLocatorRegistrable実装
        public int Priority => priority;

        // IGameStateManager実装
        public GameState CurrentGameState => currentGameState;
        public GameState PreviousGameState => previousGameState;
        public bool IsGameOver => false; // game over判定はScoreService側で行う

        private IEventManager _eventManager;

        /// <summary>
        /// サービス登録時の初期化処理
        /// </summary>
        public void OnServiceRegistered()
        {
            // 依存サービスの取得
            ServiceLocator.TryGet<IEventManager>(out _eventManager);

            // 初期状態設定
            currentGameState = GameState.MainMenu;
            previousGameState = GameState.MainMenu;

            IsServiceActive = true;
            Debug.Log($"[{ServiceName}] Service registered and initialized");
        }

        /// <summary>
        /// サービス登録解除時の終了処理
        /// </summary>
        public void OnServiceUnregistered()
        {
            IsServiceActive = false;
            Debug.Log($"[{ServiceName}] Service unregistered");
        }

        /// <summary>
        /// ServiceLocator登録処理（後方互換性のため残す）
        /// </summary>
        public void RegisterServices()
        {
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.Register<IGameStateManager>(this);
            }
        }

        /// <summary>
        /// ServiceLocator登録解除処理（後方互換性のため残す）
        /// </summary>
        public void UnregisterServices()
        {
            if (FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.Unregister<IGameStateManager>();
            }
        }

        /// <summary>
        /// ゲーム状態の変更
        /// </summary>
        public void ChangeGameState(GameState newState)
        {
            if (currentGameState == newState)
                return;

            previousGameState = currentGameState;
            currentGameState = newState;

            // ScriptableObject経由のイベント発行
            gameStateChangedEvent?.Raise(currentGameState);

            // EventManager経由のイベント発行（新方式）
            if (_eventManager != null)
            {
                var changeData = new GameStateChangeEventData
                {
                    PreviousState = previousGameState,
                    NewState = currentGameState
                };
                _eventManager.RaiseEvent("GameStateChanged", changeData);
            }

            Debug.Log($"[{ServiceName}] Game state changed from {previousGameState} to {currentGameState}");
        }
    }

    /// <summary>
    /// ゲーム状態変更イベントデータ
    /// </summary>
    public struct GameStateChangeEventData
    {
        public GameState PreviousState;
        public GameState NewState;
    }
}
