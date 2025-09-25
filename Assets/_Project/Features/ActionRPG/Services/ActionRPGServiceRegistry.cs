using System;
using UnityEngine;
using asterivo.Unity60.Core.Services;

namespace asterivo.Unity60.Features.ActionRPG.Services
{
    /// <summary>
    /// ActionRPGサービスの実装
    /// Feature層でActionRPG関連機能を管理
    /// </summary>
    public class ActionRPGServiceRegistry : IActionRPGService
    {
        private GameObject _playerGameObject;
        private IStatSystem _statSystem;
        private int _totalRunesCollected;
        private int _sessionRunesCollected;

        // イベント
        public event Action<int> OnLevelUp;
        public event Action<int> OnExperienceGained;

        public void Initialize()
        {
            Debug.Log("ActionRPGService: 初期化開始");
            _totalRunesCollected = 0;
            _sessionRunesCollected = 0;
        }

        public void Shutdown()
        {
            Debug.Log("ActionRPGService: シャットダウン");
            _playerGameObject = null;
            _statSystem = null;
            OnLevelUp = null;
            OnExperienceGained = null;
        }

        /// <summary>
        /// プレイヤーオブジェクトを設定
        /// </summary>
        public void SetPlayerGameObject(GameObject player)
        {
            _playerGameObject = player;
            if (player != null)
            {
                // StatComponentのインターフェース化が必要
                Debug.Log("ActionRPGService: プレイヤーオブジェクトを設定しました");
            }
        }

        /// <summary>
        /// StatSystemを設定
        /// </summary>
        public void SetStatSystem(IStatSystem statSystem)
        {
            _statSystem = statSystem;
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0) return;

            // StatSystem経由で経験値を追加
            if (_statSystem != null)
            {
                var previousLevel = _statSystem.CurrentLevel;
                // StatSystemに経験値を追加する処理
                
                OnExperienceGained?.Invoke(amount);

                if (_statSystem.CurrentLevel > previousLevel)
                {
                    OnLevelUp?.Invoke(_statSystem.CurrentLevel);
                }
            }

            Debug.Log($"ActionRPGService: {amount} 経験値を追加しました");
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
            
            // 経験値として追加
            AddExperience(amount);
            
            Debug.Log($"ActionRPGService: ルーン {amount} 個を収集 (総計: {_totalRunesCollected})");
        }

        public GameObject GetPlayerGameObject()
        {
            return _playerGameObject;
        }

        /// <summary>
        /// セッション統計を取得
        /// </summary>
        public (int total, int session) GetRuneStatistics()
        {
            return (_totalRunesCollected, _sessionRunesCollected);
        }

        /// <summary>
        /// セッション統計をリセット
        /// </summary>
        public void ResetSessionStats()
        {
            _sessionRunesCollected = 0;
            Debug.Log("ActionRPGService: セッション統計をリセットしました");
        }
    }
}
