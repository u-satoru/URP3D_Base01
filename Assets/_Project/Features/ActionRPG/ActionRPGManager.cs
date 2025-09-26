using UnityEngine;
using asterivo.Unity60.Core;

namespace asterivo.Unity60.Features.ActionRPG
{
    /// <summary>
    /// ActionRPG繝槭ロ繝ｼ繧ｸ繝｣繝ｼ繧ｳ繝ｳ繝昴・繝阪Φ繝・
    /// GameObject縺ｫ繧｢繧ｿ繝・メ縺励※菴ｿ逕ｨ縺励、ctionRPG讖溯・縺ｮ邂｡逅・ｒ陦後≧
    /// </summary>
    public class ActionRPGManager : MonoBehaviour
    {
        private IActionRPGService _actionRPGService;
        private bool _isInitialized;

        // 繝励Ο繝代ユ繧｣
        public bool IsInitialized => _isInitialized;

        void Awake()
        {
            // Bootstrapper繧剃ｽｿ縺｣縺ｦ蛻晄悄蛹・
            if (!ActionRPGBootstrapper.IsInitialized)
            {
                ActionRPGBootstrapper.Initialize();
            }

            // ServiceLocator縺九ｉ繧ｵ繝ｼ繝薙せ繧貞叙蠕・
            if (ServiceLocator.TryGet<IActionRPGService>(out var service))
            {
                _actionRPGService = service;
                _isInitialized = true;
                Debug.Log("ActionRPGManager: ServiceLocator縺九ｉActionRPG繧ｵ繝ｼ繝薙せ繧貞叙蠕励＠縺ｾ縺励◆");
            }
            else
            {
                Debug.LogError("ActionRPGManager: ActionRPG繧ｵ繝ｼ繝薙せ縺檎匳骭ｲ縺輔ｌ縺ｦ縺・∪縺帙ｓ");
            }
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ繧定ｨｭ螳・
        /// </summary>
        public void SetPlayer(GameObject playerObject)
        {
            if (_actionRPGService != null && playerObject != null)
            {
                var registry = ActionRPGBootstrapper.GetServiceRegistry();
                registry?.SetPlayerGameObject(playerObject);
                Debug.Log($"ActionRPGManager: 繝励Ξ繧､繝､繝ｼ繧ｪ繝悶ず繧ｧ繧ｯ繝医ｒ險ｭ螳・ {playerObject.name}");
            }
        }

        /// <summary>
        /// 邨碁ｨ灘､繧定ｿｽ蜉
        /// </summary>
        public void AddExperience(int amount)
        {
            _actionRPGService?.AddExperience(amount);
        }

        /// <summary>
        /// 繝ｫ繝ｼ繝ｳ蜿朱寔繧帝夂衍
        /// </summary>
        public void NotifyResourceCollected(int amount)
        {
            _actionRPGService?.NotifyResourceCollected(amount);
        }

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ邨碁ｨ灘､諠・ｱ繧貞叙蠕・
        /// </summary>
        public (int currentExp, int currentLevel, int expToNext) GetExperienceInfo()
        {
            if (_actionRPGService != null)
            {
                return _actionRPGService.GetExperienceInfo();
            }
            return (0, 1, 0);
        }

        /// <summary>
        /// 繧ｻ繝・す繝ｧ繝ｳ邨ｱ險医ｒ蜿門ｾ・
        /// </summary>
        public (int total, int session) GetRuneStatistics()
        {
            var registry = ActionRPGBootstrapper.GetServiceRegistry();
            if (registry != null)
            {
                return registry.GetRuneStatistics();
            }
            return (0, 0);
        }

        /// <summary>
        /// 繧ｻ繝・す繝ｧ繝ｳ邨ｱ險医ｒ繝ｪ繧ｻ繝・ヨ
        /// </summary>
        public void ResetSessionStats()
        {
            var registry = ActionRPGBootstrapper.GetServiceRegistry();
            registry?.ResetSessionStats();
        }

        void OnDestroy()
        {
            // 繧ｳ繝ｳ繝昴・繝阪Φ繝育ｴ譽・凾縺ｮ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・
            _actionRPGService = null;
            _isInitialized = false;
        }

        /// <summary>
        /// 繝・ヰ繝・げ逕ｨ: 繝・せ繝育ｵ碁ｨ灘､繧定ｿｽ蜉
        /// </summary>
        [ContextMenu("Add Test Experience (100)")]
        public void AddTestExperience()
        {
            AddExperience(100);
            Debug.Log("ActionRPGManager: 繝・せ繝育ｵ碁ｨ灘､ 100 繧定ｿｽ蜉縺励∪縺励◆");
        }

        /// <summary>
        /// 繝・ヰ繝・げ逕ｨ: 螟ｧ驥冗ｵ碁ｨ灘､繧定ｿｽ蜉
        /// </summary>
        [ContextMenu("Add Large Experience (1000)")]
        public void AddLargeExperience()
        {
            AddExperience(1000);
            Debug.Log("ActionRPGManager: 繝・せ繝育ｵ碁ｨ灘､ 1000 繧定ｿｽ蜉縺励∪縺励◆");
        }

        /// <summary>
        /// 繝・ヰ繝・げ諠・ｱ繧偵Ο繧ｰ蜃ｺ蜉・
        /// </summary>
        [ContextMenu("Log Debug Info")]
        public void LogDebugInfo()
        {
            var expInfo = GetExperienceInfo();
            var runeStats = GetRuneStatistics();
            Debug.Log($"=== ActionRPG Manager Debug Info ===\n" +
                     $"迴ｾ蝨ｨ縺ｮ邨碁ｨ灘､: {expInfo.currentExp}\n" +
                     $"迴ｾ蝨ｨ縺ｮ繝ｬ繝吶Ν: {expInfo.currentLevel}\n" +
                     $"谺｡縺ｮ繝ｬ繝吶Ν縺ｾ縺ｧ: {expInfo.expToNext}\n" +
                     $"邱上Ν繝ｼ繝ｳ蜿朱寔謨ｰ: {runeStats.total}\n" +
                     $"繧ｻ繝・す繝ｧ繝ｳ繝ｫ繝ｼ繝ｳ蜿朱寔謨ｰ: {runeStats.session}\n" +
                     $"蛻晄悄蛹也憾諷・ {_isInitialized}");
        }
    }
}


