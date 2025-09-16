namespace asterivo.Unity60.Features.Templates.FPS.Data
{
    /// <summary>
    /// FPSカメラ状態定義
    /// アーキテクチャ準拠: ステートマシンパターン + ScriptableObjectベースのデータ管理
    /// </summary>
    public enum CameraState
    {
        /// <summary>
        /// 一人称視点（プレイヤー目線での精密操作）
        /// </summary>
        FirstPerson = 0,

        /// <summary>
        /// 三人称視点（後方視点での環境把握）
        /// </summary>
        ThirdPerson = 1,

        /// <summary>
        /// 照準・狙撃専用視点（FOV調整・手ブレ補正・照準UI表示）
        /// </summary>
        Aim = 2,

        /// <summary>
        /// 遮蔽物利用時の専用視点（ピーキング動作・周囲警戒視野調整）
        /// </summary>
        Cover = 3,

        /// <summary>
        /// 死亡時のカメラ視点
        /// </summary>
        Death = 4,

        /// <summary>
        /// カットシーン用カメラ視点
        /// </summary>
        Cinematic = 5
    }

    /// <summary>
    /// CameraState拡張メソッド
    /// </summary>
    public static class CameraStateExtensions
    {
        /// <summary>
        /// カメラ状態の説明を取得
        /// </summary>
        public static string GetDescription(this CameraState state)
        {
            return state switch
            {
                CameraState.FirstPerson => "プレイヤー目線での一人称視点、精密操作対応",
                CameraState.ThirdPerson => "後方視点での三人称視点、環境把握重視",
                CameraState.Aim => "照準・狙撃専用視点、FOV調整・手ブレ補正対応",
                CameraState.Cover => "遮蔽物利用時の専用視点、ピーキング動作対応",
                CameraState.Death => "死亡時のカメラ視点",
                CameraState.Cinematic => "カットシーン用カメラ視点",
                _ => "不明なカメラ状態"
            };
        }

        /// <summary>
        /// カメラ状態の推奨FOVを取得
        /// </summary>
        public static float GetRecommendedFOV(this CameraState state)
        {
            return state switch
            {
                CameraState.FirstPerson => 60f,
                CameraState.ThirdPerson => 60f,
                CameraState.Aim => 40f,        // ズーム効果
                CameraState.Cover => 55f,
                CameraState.Death => 70f,      // 広い視野
                CameraState.Cinematic => 50f,  // 映画的視野
                _ => 60f
            };
        }

        /// <summary>
        /// カメラ状態が移動可能かどうか判定
        /// </summary>
        public static bool CanMove(this CameraState state)
        {
            return state switch
            {
                CameraState.FirstPerson => true,
                CameraState.ThirdPerson => true,
                CameraState.Aim => true,
                CameraState.Cover => true,
                CameraState.Death => false,
                CameraState.Cinematic => false,
                _ => true
            };
        }

        /// <summary>
        /// カメラ状態が入力可能かどうか判定
        /// </summary>
        public static bool CanReceiveInput(this CameraState state)
        {
            return state switch
            {
                CameraState.FirstPerson => true,
                CameraState.ThirdPerson => true,
                CameraState.Aim => true,
                CameraState.Cover => true,
                CameraState.Death => false,
                CameraState.Cinematic => false,
                _ => true
            };
        }

        /// <summary>
        /// カメラ状態の優先度を取得（Cinemachine Priority用）
        /// </summary>
        public static int GetPriority(this CameraState state)
        {
            return state switch
            {
                CameraState.FirstPerson => 10,
                CameraState.ThirdPerson => 10,
                CameraState.Aim => 15,         // 照準時は高優先度
                CameraState.Cover => 12,
                CameraState.Death => 20,       // 死亡時は最高優先度
                CameraState.Cinematic => 25,   // カットシーンは最優先
                _ => 5
            };
        }
    }
}