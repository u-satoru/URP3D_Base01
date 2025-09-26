using System;
using UnityEngine;
using asterivo.Unity60.Core;

namespace asterivo.Unity60.Features.ActionRPG.Services
{
    /// <summary>
    /// ActionRPG繧ｵ繝ｼ繝薙せ縺ｮ螳溯｣・
    /// Feature螻､縺ｧActionRPG髢｢騾｣讖溯・繧堤ｮ｡逅・
    /// </summary>
    public class ActionRPGServiceRegistry : IActionRPGService
    {
        private GameObject _playerGameObject;
        private IStatSystem _statSystem;
        private int _totalRunesCollected;
        private int _sessionRunesCollected;

        // 繧､繝吶Φ繝・
        public event Action<int> OnLevelUp;
        public event Action<int> OnExperienceGained;

        public void Initialize()
        {
            Debug.Log("ActionRPGService: 蛻晄悄蛹夜幕蟋・);
            _totalRunesCollected = 0;
            _sessionRunesCollected = 0;
        }

        public void Shutdown()
        {
            Debug.Log("ActionRPGService: 繧ｷ繝｣繝・ヨ繝繧ｦ繝ｳ");
            _playerGameObject = null;
            _statSystem = null;
            OnLevelUp = null;
            OnExperienceGained = null;
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ繧ｪ繝悶ず繧ｧ繧ｯ繝医ｒ險ｭ螳・
        /// </summary>
        public void SetPlayerGameObject(GameObject player)
        {
            _playerGameObject = player;
            if (player != null)
            {
                // StatComponent縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ蛹悶′蠢・ｦ・
                Debug.Log("ActionRPGService: 繝励Ξ繧､繝､繝ｼ繧ｪ繝悶ず繧ｧ繧ｯ繝医ｒ險ｭ螳壹＠縺ｾ縺励◆");
            }
        }

        /// <summary>
        /// StatSystem繧定ｨｭ螳・
        /// </summary>
        public void SetStatSystem(IStatSystem statSystem)
        {
            _statSystem = statSystem;
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0) return;

            // StatSystem邨檎罰縺ｧ邨碁ｨ灘､繧定ｿｽ蜉
            if (_statSystem != null)
            {
                var previousLevel = _statSystem.CurrentLevel;
                // StatSystem縺ｫ邨碁ｨ灘､繧定ｿｽ蜉縺吶ｋ蜃ｦ逅・
                
                OnExperienceGained?.Invoke(amount);

                if (_statSystem.CurrentLevel > previousLevel)
                {
                    OnLevelUp?.Invoke(_statSystem.CurrentLevel);
                }
            }

            Debug.Log($"ActionRPGService: {amount} 邨碁ｨ灘､繧定ｿｽ蜉縺励∪縺励◆");
        }

        public (int currentExp, int currentLevel, int expToNext) GetExperienceInfo()
        {
            if (_statSystem != null)
            {
                return (_statSystem.CurrentExperience, _statSystem.CurrentLevel, 0);
            }
            return (0, 1, 0);
        }

        public void NotifyResourceCollected(int amount)
        {
            _totalRunesCollected += amount;
            _sessionRunesCollected += amount;
            
            // 邨碁ｨ灘､縺ｨ縺励※霑ｽ蜉
            AddExperience(amount);
            
            Debug.Log($"ActionRPGService: 繝ｫ繝ｼ繝ｳ {amount} 蛟九ｒ蜿朱寔 (邱剰ｨ・ {_totalRunesCollected})");
        }

        public GameObject GetPlayerGameObject()
        {
            return _playerGameObject;
        }

        /// <summary>
        /// 繧ｻ繝・す繝ｧ繝ｳ邨ｱ險医ｒ蜿門ｾ・
        /// </summary>
        public (int total, int session) GetRuneStatistics()
        {
            return (_totalRunesCollected, _sessionRunesCollected);
        }

        /// <summary>
        /// 繧ｻ繝・す繝ｧ繝ｳ邨ｱ險医ｒ繝ｪ繧ｻ繝・ヨ
        /// </summary>
        public void ResetSessionStats()
        {
            _sessionRunesCollected = 0;
            Debug.Log("ActionRPGService: 繧ｻ繝・す繝ｧ繝ｳ邨ｱ險医ｒ繝ｪ繧ｻ繝・ヨ縺励∪縺励◆");
        }
    }
}


