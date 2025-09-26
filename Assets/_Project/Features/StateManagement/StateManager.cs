using System.Collections.Generic;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Patterns;
using asterivo.Unity60.Features.Player;
using UnityEngine;

namespace asterivo.Unity60.Features.StateManagement
{
    /// <summary>
    /// StateManagement讖溯・縺ｮ繝倥Ν繝代・繧ｯ繝ｩ繧ｹ
    /// 迥ｶ諷矩・遘ｻ縺ｮ邂｡逅・→螳溯｡後ｒ邁｡蜊倥↓陦後≧縺溘ａ縺ｮ萓ｿ蛻ｩ繧ｯ繝ｩ繧ｹ
    /// </summary>
    public class StateManager : MonoBehaviour, IStateContext
    {
        [SerializeField] private bool isDebugEnabled = false;
        [SerializeField] private PlayerState currentState = PlayerState.Idle;

        private IStateService stateService;
        private IStateHandler currentHandler;
        private readonly List<PlayerState> stateHistory = new List<PlayerState>();

        /// <summary>
        /// 繝・ヰ繝・げ繝ｭ繧ｰ縺梧怏蜉ｹ縺九←縺・°
        /// </summary>
        public bool IsDebugEnabled => isDebugEnabled;

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ迥ｶ諷・
        /// </summary>
        public PlayerState CurrentState => currentState;

        /// <summary>
        /// 迥ｶ諷句ｱ･豁ｴ
        /// </summary>
        public IReadOnlyList<PlayerState> StateHistory => stateHistory;

        void Awake()
        {
            // StateService繧貞叙蠕励∪縺溘・Bootstrapper縺ｧ蛻晄悄蛹・
            if (!ServiceLocator.TryGet<IStateService>(out stateService))
            {
                Debug.LogWarning("[StateManager] StateService not found. Initializing StateManagement...");
                StateManagementBootstrapper.Initialize();
                stateService = ServiceLocator.Get<IStateService>();
            }
        }

        void Start()
        {
            // 蛻晄悄迥ｶ諷九・險ｭ螳・
            ChangeState(PlayerState.Idle);
        }

        /// <summary>
        /// 迥ｶ諷九ｒ螟画峩縺吶ｋ
        /// </summary>
        /// <param name="newState">譁ｰ縺励＞迥ｶ諷・/param>
        public void ChangeState(PlayerState newState)
        {
            if (currentState == newState)
            {
                Log($"State is already {newState}. Skipping transition.");
                return;
            }

            // 迴ｾ蝨ｨ縺ｮ迥ｶ諷九°繧蛾蜃ｺ
            if (currentHandler != null)
            {
                currentHandler.OnExit(this);
                currentHandler = null;
            }

            // 螻･豁ｴ縺ｫ霑ｽ蜉
            stateHistory.Add(currentState);
            if (stateHistory.Count > 10) // 譛譁ｰ10莉ｶ縺ｮ縺ｿ菫晄戟
            {
                stateHistory.RemoveAt(0);
            }

            // 譁ｰ縺励＞迥ｶ諷九↓驕ｷ遘ｻ
            currentState = newState;
            currentHandler = stateService.GetHandler((int)newState);

            if (currentHandler != null)
            {
                currentHandler.OnEnter(this);
            }
            else
            {
                Log($"No handler found for state: {newState}");
            }
        }

        /// <summary>
        /// 繝ｭ繧ｰ繝｡繝・そ繝ｼ繧ｸ繧貞・蜉・
        /// </summary>
        /// <param name="message">繝ｭ繧ｰ繝｡繝・そ繝ｼ繧ｸ</param>
        public void Log(string message)
        {
            if (isDebugEnabled)
            {
                Debug.Log($"[StateManager] {message}");
            }
        }

        /// <summary>
        /// 謖・ｮ壹＠縺溽憾諷九↓驕ｷ遘ｻ蜿ｯ閭ｽ縺九メ繧ｧ繝・け
        /// </summary>
        /// <param name="targetState">繝√ぉ繝・け縺吶ｋ迥ｶ諷・/param>
        /// <returns>驕ｷ遘ｻ蜿ｯ閭ｽ縺ｪ蝣ｴ蜷医・true</returns>
        public bool CanTransitionTo(PlayerState targetState)
        {
            return stateService.HasHandler((int)targetState);
        }

        /// <summary>
        /// 蜑阪・迥ｶ諷九↓謌ｻ繧・
        /// </summary>
        public void RevertToPreviousState()
        {
            if (stateHistory.Count > 0)
            {
                var previousState = stateHistory[stateHistory.Count - 1];
                stateHistory.RemoveAt(stateHistory.Count - 1);
                ChangeState(previousState);
            }
            else
            {
                Log("No previous state in history");
            }
        }

        void OnDestroy()
        {
            // 迥ｶ諷九°繧蛾蜃ｺ
            if (currentHandler != null)
            {
                currentHandler.OnExit(this);
                currentHandler = null;
            }
        }
    }
}


