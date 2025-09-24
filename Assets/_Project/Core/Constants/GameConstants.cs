using UnityEngine;

namespace asterivo.Unity60.Core.Constants
{
    /// <summary>
    /// ゲーム全体で使用する定数定義
    /// マジックナンバーを排除し、保守性を向上させるための定数クラス
    /// 
    /// 主な定数カテゴリ：
    /// - ヘルス・ダメージ関連定数
    /// - 読み込み・待機時間定数
    /// - UI・表示関連定数
    /// - その他のゲームバランス定数
    /// </summary>
    public static class GameConstants
    {
        #region ヘルス・ダメージ関連定数
        /// <summary>
        /// テスト用の小さな回復量
        /// CommandInvokerEditorのテストコマンドで使用
        /// </summary>
        public const int TEST_HEAL_SMALL = 10;
        
        /// <summary>
        /// テスト用の大きな回復量
        /// CommandInvokerEditorのテストコマンドで使用
        /// </summary>
        public const int TEST_HEAL_LARGE = 25;
        
        /// <summary>
        /// テスト用の小さなダメージ量
        /// CommandInvokerEditorのテストコマンドで使用
        /// </summary>
        public const int TEST_DAMAGE_SMALL = 10;
        
        /// <summary>
        /// テスト用の大きなダメージ量
        /// CommandInvokerEditorのテストコマンドで使用
        /// </summary>
        public const int TEST_DAMAGE_LARGE = 25;
        #endregion

        #region 時間関連定数
        /// <summary>
        /// 最小読み込み表示時間（秒）
        /// ローディング画面の最小表示時間を保証
        /// </summary>
        public const float MIN_LOADING_TIME = 1.0f;
        
        /// <summary>
        /// UI フェード時間（秒）
        /// 標準的なUI要素のフェードイン・アウト時間
        /// </summary>
        public const float UI_FADE_DURATION = 0.3f;
        
        /// <summary>
        /// オーディオフェード時間（秒）
        /// BGM・SE切り替え時のフェード時間
        /// </summary>
        public const float AUDIO_FADE_DURATION = 0.5f;
        #endregion

        #region UI関連定数
        /// <summary>
        /// デフォルトのUIスケール
        /// UI要素の基準スケール値
        /// </summary>
        public const float DEFAULT_UI_SCALE = 1.0f;
        
        /// <summary>
        /// ホバー時のUIスケール倍率
        /// ボタンなどがハイライトされた際のスケール
        /// </summary>
        public const float UI_HOVER_SCALE = 1.1f;
        #endregion

        #region パフォーマンス関連定数
        /// <summary>
        /// オブジェクトプールの初期サイズ
        /// コマンドプールなどの初期オブジェクト数
        /// </summary>
        public const int OBJECT_POOL_INITIAL_SIZE = 50;
        
        /// <summary>
        /// 最大オブジェクトプール容量
        /// メモリ使用量制限のための上限値
        /// </summary>
        public const int OBJECT_POOL_MAX_SIZE = 500;
        #endregion
    }
}
