using UnityEngine;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.ActionRPG.Services;

namespace asterivo.Unity60.Features.ActionRPG
{
    /// <summary>
    /// ActionRPG機能のブートストラッパー
    /// ServiceLocatorへのサービス登録と初期化を担当
    /// </summary>
    public static class ActionRPGBootstrapper
    {
        private static bool _isInitialized = false;
        private static ActionRPGServiceRegistry _serviceRegistry;

        /// <summary>
        /// ActionRPG機能を初期化
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("ActionRPGBootstrapper: 既に初期化されています");
                return;
            }

            Debug.Log("ActionRPGBootstrapper: 初期化開始");

            // サービスを作成して登録
            _serviceRegistry = new ActionRPGServiceRegistry();
            ServiceLocator.Register<IActionRPGService>(_serviceRegistry);

            _isInitialized = true;
            Debug.Log("ActionRPGBootstrapper: 初期化完了 - ActionRPGサービスがServiceLocatorに登録されました");
        }

        /// <summary>
        /// ActionRPG機能をシャットダウン
        /// </summary>
        public static void Shutdown()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("ActionRPGBootstrapper: 初期化されていません");
                return;
            }

            Debug.Log("ActionRPGBootstrapper: シャットダウン開始");

            // ServiceLocatorから解除
            if (ServiceLocator.TryGet<IActionRPGService>(out var service))
            {
                service.Shutdown();
                // ServiceLocatorに解除メソッドがあれば呼び出す
                // ServiceLocator.Unregister<IActionRPGService>();
            }

            _serviceRegistry = null;
            _isInitialized = false;
            Debug.Log("ActionRPGBootstrapper: シャットダウン完了");
        }

        /// <summary>
        /// 現在のサービスインスタンスを取得
        /// </summary>
        public static ActionRPGServiceRegistry GetServiceRegistry()
        {
            return _serviceRegistry;
        }

        /// <summary>
        /// 初期化状態を取得
        /// </summary>
        public static bool IsInitialized => _isInitialized;
    }
}
