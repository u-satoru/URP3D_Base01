using System.Collections;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// シーン読み込み管理サービスインターフェース（非同期・最小時間制御対応）
    ///
    /// Unity 6における3層アーキテクチャのCore層シーン管理基盤において、
    /// ゲームシーンの効率的な読み込みと最小表示時間制御を提供するインターフェースです。
    /// 非同期読み込みとユーザー体験最適化により、
    /// スムーズなシーン遷移とロード時間の一貫性を実現します。
    ///
    /// 【非同期シーン読み込みシステム】
    /// - Async Scene Loading: Unity SceneManagerによる非同期読み込み制御
    /// - Progress Tracking: ロード進捗のリアルタイム監視と表示
    /// - Memory Management: 旧シーンのアンロードによるメモリ最適化
    /// - Resource Preloading: 必要リソースの事前読み込みによる高速化
    ///
    /// 【最小時間制御システム】
    /// - Minimum Display Time: ロード画面の最小表示時間保証
    /// - User Experience: あまりに高速なロードによるちらつき防止
    /// - Progress Smoothing: 進捗バーの自然な更新による視覚的快適性
    /// - Transition Consistency: 一貫したシーン遷移体験の提供
    ///
    /// 【3層アーキテクチャ連携】
    /// - Core Layer Foundation: シーン読み込みの基盤サービス提供
    /// - Feature Layer Integration: 具体機能でのシーン遷移制御
    /// - Template Layer Navigation: ジャンル特化シーン遷移フロー
    /// - Cross-Scene Persistence: シーン間でのデータ永続化管理
    ///
    /// 【パフォーマンス最適化】
    /// - Additive Loading: アディティブシーン読み込みによる効率化
    /// - Asset Bundle Integration: AssetBundleと連携した動的読み込み
    /// - GC Optimization: ガベージコレクション最適化による滑らかな遷移
    /// - Thread Safety: マルチスレッド対応による安全な非同期処理
    ///
    /// 【Template層での活用】
    /// - Game Scene: メインゲームプレイシーンへの遷移
    /// - Menu Navigation: メニュー画面間のスムーズな切り替え
    /// - Level Transition: レベル間遷移での一貫した体験
    /// - Loading Screens: ジャンル特化ローディング画面の表示
    /// </summary>
    public interface ISceneLoadingService
    {
        /// <summary>
        /// ゲームプレイシーンを最小時間制御で読み込み
        /// </summary>
        void LoadGameplaySceneWithMinTime();

        /// <summary>
        /// 指定シーンを最小時間制御で読み込み
        /// </summary>
        /// <param name="sceneName">読み込み対象シーン名</param>
        void LoadSceneWithMinTime(string sceneName);
    }
}
