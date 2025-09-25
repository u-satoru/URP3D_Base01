using UnityEngine;
using asterivo.Unity60.Core.Services;

namespace asterivo.Unity60.Features.ActionRPG
{
    /// <summary>
    /// ActionRPGマネージャーコンポーネント
    /// GameObjectにアタッチして使用し、ActionRPG機能の管理を行う
    /// </summary>
    public class ActionRPGManager : MonoBehaviour
    {
        private IActionRPGService _actionRPGService;
        private bool _isInitialized;

        // プロパティ
        public bool IsInitialized => _isInitialized;

        void Awake()
        {
            // Bootstrapperを使って初期化
            if (!ActionRPGBootstrapper.IsInitialized)
            {
                ActionRPGBootstrapper.Initialize();
            }

            // ServiceLocatorからサービスを取得
            if (ServiceLocator.TryGet<IActionRPGService>(out var service))
            {
                _actionRPGService = service;
                _isInitialized = true;
                Debug.Log("ActionRPGManager: ServiceLocatorからActionRPGサービスを取得しました");
            }
            else
            {
                Debug.LogError("ActionRPGManager: ActionRPGサービスが登録されていません");
            }
        }

        /// <summary>
        /// プレイヤーを設定
        /// </summary>
        public void SetPlayer(GameObject playerObject)
        {
            if (_actionRPGService != null && playerObject != null)
            {
                var registry = ActionRPGBootstrapper.GetServiceRegistry();
                registry?.SetPlayerGameObject(playerObject);
                Debug.Log($"ActionRPGManager: プレイヤーオブジェクトを設定: {playerObject.name}");
            }
        }

        /// <summary>
        /// 経験値を追加
        /// </summary>
        public void AddExperience(int amount)
        {
            _actionRPGService?.AddExperience(amount);
        }

        /// <summary>
        /// ルーン収集を通知
        /// </summary>
        public void NotifyResourceCollected(int amount)
        {
            _actionRPGService?.NotifyResourceCollected(amount);
        }

        /// <summary>
        /// 現在の経験値情報を取得
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
        /// セッション統計を取得
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
        /// セッション統計をリセット
        /// </summary>
        public void ResetSessionStats()
        {
            var registry = ActionRPGBootstrapper.GetServiceRegistry();
            registry?.ResetSessionStats();
        }

        void OnDestroy()
        {
            // コンポーネント破棄時のクリーンアップ
            _actionRPGService = null;
            _isInitialized = false;
        }

        /// <summary>
        /// デバッグ用: テスト経験値を追加
        /// </summary>
        [ContextMenu("Add Test Experience (100)")]
        public void AddTestExperience()
        {
            AddExperience(100);
            Debug.Log("ActionRPGManager: テスト経験値 100 を追加しました");
        }

        /// <summary>
        /// デバッグ用: 大量経験値を追加
        /// </summary>
        [ContextMenu("Add Large Experience (1000)")]
        public void AddLargeExperience()
        {
            AddExperience(1000);
            Debug.Log("ActionRPGManager: テスト経験値 1000 を追加しました");
        }

        /// <summary>
        /// デバッグ情報をログ出力
        /// </summary>
        [ContextMenu("Log Debug Info")]
        public void LogDebugInfo()
        {
            var expInfo = GetExperienceInfo();
            var runeStats = GetRuneStatistics();
            Debug.Log($"=== ActionRPG Manager Debug Info ===\n" +
                     $"現在の経験値: {expInfo.currentExp}\n" +
                     $"現在のレベル: {expInfo.currentLevel}\n" +
                     $"次のレベルまで: {expInfo.expToNext}\n" +
                     $"総ルーン収集数: {runeStats.total}\n" +
                     $"セッションルーン収集数: {runeStats.session}\n" +
                     $"初期化状態: {_isInitialized}");
        }
    }
}
