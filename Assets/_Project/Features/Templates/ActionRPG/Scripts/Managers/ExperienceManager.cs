using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.ActionRPG.Components;

namespace asterivo.Unity60.Features.ActionRPG.Managers
{
    /// <summary>
    /// 経験値とルーン収集を管理するマネージャー
    /// ResourceCollectedEventを監視してStatComponentに経験値を追加します
    /// </summary>
    public class ExperienceManager : MonoBehaviour, IGameEventListener<int>
    {
        [Header("イベント受信")]
        [SerializeField] private GameEvent<int> _onResourceCollected;

        [Header("イベント発行")]
        [SerializeField] private GameEvent _onExperienceUpdated;
        [SerializeField] private GameEvent<int> _onTotalExperienceChanged;

        [Header("統計情報")]
        [SerializeField] private int _totalRunesCollected = 0;
        [SerializeField] private int _sessionRunesCollected = 0;
        
        // プレイヤー参照
        private StatComponent _playerStatComponent;
        private bool _isInitialized;

        // プロパティ
        public int TotalRunesCollected => _totalRunesCollected;
        public int SessionRunesCollected => _sessionRunesCollected;

        void Start()
        {
            InitializeManager();
        }

        void OnEnable()
        {
            // リソース収集イベントを受信
            if (_onResourceCollected != null)
                _onResourceCollected.RegisterListener(this);
        }

        void OnDisable()
        {
            // イベント受信解除
            if (_onResourceCollected != null)
                _onResourceCollected.UnregisterListener(this);
        }

        /// <summary>
        /// マネージャーを初期化（ServiceLocatorパターン使用）
        /// </summary>
        private void InitializeManager()
        {
            // ServiceLocatorを使用してプレイヤーサービスを取得
            try
            {
                // ActionRPGTemplateManagerがプレイヤーを管理している場合はそれを利用
                var templateManager = ServiceLocator.GetService<ActionRPGTemplateManager>();
                if (templateManager != null)
                {
                    var player = templateManager.GetPlayer();
                    if (player != null)
                    {
                        _playerStatComponent = player.GetComponent<StatComponent>();
                    }
                }

                // フォールバック：直接StatComponentを検索
                if (_playerStatComponent == null)
                {
                    _playerStatComponent = FindObjectOfType<StatComponent>();
                }

                if (_playerStatComponent != null)
                {
                    _isInitialized = true;
                    Debug.Log("ExperienceManager: ServiceLocator経由で初期化完了");
                }
                else
                {
                    Debug.LogWarning("ExperienceManager: StatComponentが見つかりません。1秒後に再試行します。");
                    Invoke(nameof(InitializeManager), 1f);
                }
            }
            catch (System.Exception)
            {
                // ServiceLocatorにActionRPGTemplateManagerが登録されていない場合のフォールバック
                _playerStatComponent = FindObjectOfType<StatComponent>();
                if (_playerStatComponent != null)
                {
                    _isInitialized = true;
                    Debug.Log("ExperienceManager: フォールバック方式で初期化完了");
                }
                else
                {
                    Debug.LogWarning("ExperienceManager: StatComponentが見つかりません。1秒後に再試行します。");
                    Invoke(nameof(InitializeManager), 1f);
                }
            }
        }

        /// <summary>
        /// リソース収集時のイベントハンドラ
        /// </summary>
        public void OnEventRaised(int amount)
        {
            if (!_isInitialized || _playerStatComponent == null) return;

            // 経験値として追加
            AddExperience(amount);
            
            // 統計更新
            _totalRunesCollected += amount;
            _sessionRunesCollected += amount;

            Debug.Log($"ルーン {amount} 個を経験値として獲得しました。（総収集数: {_totalRunesCollected}）");
        }

        /// <summary>
        /// プレイヤーに経験値を追加
        /// </summary>
        public void AddExperience(int amount)
        {
            if (_playerStatComponent == null) return;

            _playerStatComponent.AddExperience(amount);
            
            // 経験値更新イベント発行
            if (_onExperienceUpdated != null)
                _onExperienceUpdated.Raise();
                
            if (_onTotalExperienceChanged != null)
                _onTotalExperienceChanged.Raise(_playerStatComponent.CurrentExperience);
        }

        /// <summary>
        /// ボーナス経験値を付与
        /// </summary>
        public void GrantBonusExperience(int amount, string reason = "")
        {
            if (amount <= 0) return;

            AddExperience(amount);
            
            string message = string.IsNullOrEmpty(reason) 
                ? $"ボーナス経験値 {amount} を獲得しました！"
                : $"{reason}により ボーナス経験値 {amount} を獲得しました！";
                
            Debug.Log(message);
        }

        /// <summary>
        /// セッション統計をリセット
        /// </summary>
        public void ResetSessionStats()
        {
            _sessionRunesCollected = 0;
            Debug.Log("セッション統計をリセットしました。");
        }

        /// <summary>
        /// 経験値倍率イベント（将来の拡張用）
        /// </summary>
        public void ApplyExperienceMultiplier(float multiplier, float duration)
        {
            // TODO: 経験値倍率システムの実装
            Debug.Log($"経験値倍率 {multiplier}x を {duration} 秒間適用します。（未実装）");
        }

        /// <summary>
        /// プレイヤーの現在の経験値情報を取得
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
        /// デバッグ用：強制的に経験値を追加
        /// </summary>
        [ContextMenu("Add Test Experience (100)")]
        public void AddTestExperience()
        {
            AddExperience(100);
        }

        /// <summary>
        /// デバッグ用：大量経験値を追加
        /// </summary>
        [ContextMenu("Add Large Experience (1000)")]
        public void AddLargeExperience()
        {
            AddExperience(1000);
        }

        /// <summary>
        /// デバッグ情報を表示
        /// </summary>
        public void LogDebugInfo()
        {
            var expInfo = GetExperienceInfo();
            Debug.Log($"=== Experience Manager Debug Info ===\n" +
                     $"総ルーン収集数: {_totalRunesCollected}\n" +
                     $"セッションルーン収集数: {_sessionRunesCollected}\n" +
                     $"現在の経験値: {expInfo.currentExp}\n" +
                     $"現在のレベル: {expInfo.currentLevel}\n" +
                     $"次のレベルまで: {expInfo.expToNext}\n" +
                     $"初期化状態: {_isInitialized}");
        }
    }
}