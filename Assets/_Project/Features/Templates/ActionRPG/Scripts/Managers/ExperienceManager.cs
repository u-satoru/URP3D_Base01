using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.ActionRPG.Components;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Managers
{
    /// <summary>
    /// 邨碁ｨ灘､縺ｨ繝ｫ繝ｼ繝ｳ蜿朱寔繧堤ｮ｡逅・☆繧九・繝阪・繧ｸ繝｣繝ｼ
    /// ResourceCollectedEvent繧堤屮隕悶＠縺ｦStatComponent縺ｫ邨碁ｨ灘､繧定ｿｽ蜉縺励∪縺・
    /// </summary>
    public class ExperienceManager : MonoBehaviour, IGameEventListener<int>
    {
        [Header("繧､繝吶Φ繝亥女菫｡")]
        [SerializeField] private GameEvent<int> _onResourceCollected;

        [Header("繧､繝吶Φ繝育匱陦・)]
        [SerializeField] private GameEvent _onExperienceUpdated;
        [SerializeField] private GameEvent<int> _onTotalExperienceChanged;

        [Header("邨ｱ險域ュ蝣ｱ")]
        [SerializeField] private int _totalRunesCollected = 0;
        [SerializeField] private int _sessionRunesCollected = 0;
        
        // 繝励Ξ繧､繝､繝ｼ蜿ら・
        private StatComponent _playerStatComponent;
        private bool _isInitialized;

        // 繝励Ο繝代ユ繧｣
        public int TotalRunesCollected => _totalRunesCollected;
        public int SessionRunesCollected => _sessionRunesCollected;

        void Start()
        {
            InitializeManager();
        }

        void OnEnable()
        {
            // 繝ｪ繧ｽ繝ｼ繧ｹ蜿朱寔繧､繝吶Φ繝医ｒ蜿嶺ｿ｡
            if (_onResourceCollected != null)
                _onResourceCollected.RegisterListener(this);
        }

        void OnDisable()
        {
            // 繧､繝吶Φ繝亥女菫｡隗｣髯､
            if (_onResourceCollected != null)
                _onResourceCollected.UnregisterListener(this);
        }

        /// <summary>
        /// 繝槭ロ繝ｼ繧ｸ繝｣繝ｼ繧貞・譛溷喧・・erviceLocator繝代ち繝ｼ繝ｳ菴ｿ逕ｨ・・
        /// </summary>
        private void InitializeManager()
        {
            // ServiceLocator繧剃ｽｿ逕ｨ縺励※ActionRPG繧ｵ繝ｼ繝薙せ繧貞叙蠕・
            try
            {
                // IActionRPGService邨檎罰縺ｧ繝励Ξ繧､繝､繝ｼ繧貞叙蠕・
                if (ServiceLocator.TryGet<IActionRPGService>(out var actionRPGService))
                {
                    var player = actionRPGService.GetPlayerGameObject();
                    if (player != null)
                    {
                        _playerStatComponent = player.GetComponent<StatComponent>();
                    }
                }

                // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ・夂峩謗･StatComponent繧呈､懃ｴ｢
                if (_playerStatComponent == null)
                {
                    _playerStatComponent = FindObjectOfType<StatComponent>();
                }

                if (_playerStatComponent != null)
                {
                    _isInitialized = true;
                    Debug.Log("ExperienceManager: ActionRPG繧ｵ繝ｼ繝薙せ邨檎罰縺ｧ蛻晄悄蛹門ｮ御ｺ・);
                }
                else
                {
                    Debug.LogWarning("ExperienceManager: StatComponent縺瑚ｦ九▽縺九ｊ縺ｾ縺帙ｓ縲・遘貞ｾ後↓蜀崎ｩｦ陦後＠縺ｾ縺吶・);
                    Invoke(nameof(InitializeManager), 1f);
                }
            }
            catch (System.Exception ex)
            {
                // 繧ｨ繝ｩ繝ｼ蜃ｦ逅・
                Debug.LogWarning($"ExperienceManager: 蛻晄悄蛹悶お繝ｩ繝ｼ: {ex.Message}");

                // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ・夂峩謗･StatComponent繧呈､懃ｴ｢
                _playerStatComponent = FindObjectOfType<StatComponent>();
                if (_playerStatComponent != null)
                {
                    _isInitialized = true;
                    Debug.Log("ExperienceManager: 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ譁ｹ蠑上〒蛻晄悄蛹門ｮ御ｺ・);
                }
                else
                {
                    Debug.LogWarning("ExperienceManager: StatComponent縺瑚ｦ九▽縺九ｊ縺ｾ縺帙ｓ縲・遘貞ｾ後↓蜀崎ｩｦ陦後＠縺ｾ縺吶・);
                    Invoke(nameof(InitializeManager), 1f);
                }
            }
        }

        /// <summary>
        /// 繝ｪ繧ｽ繝ｼ繧ｹ蜿朱寔譎ゅ・繧､繝吶Φ繝医ワ繝ｳ繝峨Λ
        /// </summary>
        public void OnEventRaised(int amount)
        {
            if (!_isInitialized || _playerStatComponent == null) return;

            // 邨碁ｨ灘､縺ｨ縺励※霑ｽ蜉
            AddExperience(amount);
            
            // 邨ｱ險域峩譁ｰ
            _totalRunesCollected += amount;
            _sessionRunesCollected += amount;

            Debug.Log($"繝ｫ繝ｼ繝ｳ {amount} 蛟九ｒ邨碁ｨ灘､縺ｨ縺励※迯ｲ蠕励＠縺ｾ縺励◆縲ゑｼ育ｷ丞庶髮・焚: {_totalRunesCollected}・・);
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺ｫ邨碁ｨ灘､繧定ｿｽ蜉
        /// </summary>
        public void AddExperience(int amount)
        {
            if (_playerStatComponent == null) return;

            _playerStatComponent.AddExperience(amount);
            
            // 邨碁ｨ灘､譖ｴ譁ｰ繧､繝吶Φ繝育匱陦・
            if (_onExperienceUpdated != null)
                _onExperienceUpdated.Raise();
                
            if (_onTotalExperienceChanged != null)
                _onTotalExperienceChanged.Raise(_playerStatComponent.CurrentExperience);
        }

        /// <summary>
        /// 繝懊・繝翫せ邨碁ｨ灘､繧剃ｻ倅ｸ・
        /// </summary>
        public void GrantBonusExperience(int amount, string reason = "")
        {
            if (amount <= 0) return;

            AddExperience(amount);
            
            string message = string.IsNullOrEmpty(reason) 
                ? $"繝懊・繝翫せ邨碁ｨ灘､ {amount} 繧堤佐蠕励＠縺ｾ縺励◆・・
                : $"{reason}縺ｫ繧医ｊ 繝懊・繝翫せ邨碁ｨ灘､ {amount} 繧堤佐蠕励＠縺ｾ縺励◆・・;
                
            Debug.Log(message);
        }

        /// <summary>
        /// 繧ｻ繝・す繝ｧ繝ｳ邨ｱ險医ｒ繝ｪ繧ｻ繝・ヨ
        /// </summary>
        public void ResetSessionStats()
        {
            _sessionRunesCollected = 0;
            Debug.Log("繧ｻ繝・す繝ｧ繝ｳ邨ｱ險医ｒ繝ｪ繧ｻ繝・ヨ縺励∪縺励◆縲・);
        }

        /// <summary>
        /// 邨碁ｨ灘､蛟咲紫繧､繝吶Φ繝茨ｼ亥ｰ・擂縺ｮ諡｡蠑ｵ逕ｨ・・
        /// </summary>
        public void ApplyExperienceMultiplier(float multiplier, float duration)
        {
            // TODO: 邨碁ｨ灘､蛟咲紫繧ｷ繧ｹ繝・Β縺ｮ螳溯｣・
            Debug.Log($"邨碁ｨ灘､蛟咲紫 {multiplier}x 繧・{duration} 遘帝俣驕ｩ逕ｨ縺励∪縺吶ゑｼ域悴螳溯｣・ｼ・);
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺ｮ迴ｾ蝨ｨ縺ｮ邨碁ｨ灘､諠・ｱ繧貞叙蠕・
        /// </summary>
        public (int currentExp, int currentLevel, int expToNext) GetExperienceInfo()
        {
            if (_playerStatComponent == null)
                return (0, 1, 0);

            return (
                _playerStatComponent.CurrentExperience,
                _playerStatComponent.CurrentLevel,
                _playerStatComponent.GetExperienceToNextLevel()
            );
        }

        /// <summary>
        /// 繝・ヰ繝・げ逕ｨ・壼ｼｷ蛻ｶ逧・↓邨碁ｨ灘､繧定ｿｽ蜉
        /// </summary>
        [ContextMenu("Add Test Experience (100)")]
        public void AddTestExperience()
        {
            AddExperience(100);
        }

        /// <summary>
        /// 繝・ヰ繝・げ逕ｨ・壼､ｧ驥冗ｵ碁ｨ灘､繧定ｿｽ蜉
        /// </summary>
        [ContextMenu("Add Large Experience (1000)")]
        public void AddLargeExperience()
        {
            AddExperience(1000);
        }

        /// <summary>
        /// 繝・ヰ繝・げ諠・ｱ繧定｡ｨ遉ｺ
        /// </summary>
        public void LogDebugInfo()
        {
            var expInfo = GetExperienceInfo();
            Debug.Log($"=== Experience Manager Debug Info ===\n" +
                     $"邱上Ν繝ｼ繝ｳ蜿朱寔謨ｰ: {_totalRunesCollected}\n" +
                     $"繧ｻ繝・す繝ｧ繝ｳ繝ｫ繝ｼ繝ｳ蜿朱寔謨ｰ: {_sessionRunesCollected}\n" +
                     $"迴ｾ蝨ｨ縺ｮ邨碁ｨ灘､: {expInfo.currentExp}\n" +
                     $"迴ｾ蝨ｨ縺ｮ繝ｬ繝吶Ν: {expInfo.currentLevel}\n" +
                     $"谺｡縺ｮ繝ｬ繝吶Ν縺ｾ縺ｧ: {expInfo.expToNext}\n" +
                     $"蛻晄悄蛹也憾諷・ {_isInitialized}");
        }
    }
}


