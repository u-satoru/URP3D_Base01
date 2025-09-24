using UnityEngine;

namespace asterivo.Unity60.Core.Bootstrap
{
    /// <summary>
    /// GameBootstrapperのプレハブ生成ヘルパー
    /// シーンにGameBootstrapperがない場合に自動生成する
    /// </summary>
    [DefaultExecutionOrder(-2000)]
    public static class GameBootstrapperPrefab
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateBootstrapperIfNeeded()
        {
            // 既存のGameBootstrapperをチェック
            var existingBootstrapper = Object.FindObjectOfType<GameBootstrapper>();
            if (existingBootstrapper != null)
            {
                Debug.Log("[GameBootstrapperPrefab] GameBootstrapper already exists in scene");
                return;
            }

            // GameBootstrapperを新規作成
            var bootstrapperGO = new GameObject("[GameBootstrapper]");
            var bootstrapper = bootstrapperGO.AddComponent<GameBootstrapper>();

            Debug.Log("[GameBootstrapperPrefab] Created GameBootstrapper automatically");
        }
    }
}
