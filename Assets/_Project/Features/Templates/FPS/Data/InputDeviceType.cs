namespace asterivo.Unity60.Features.Templates.FPS.Data
{
    /// <summary>
    /// 入力デバイスタイプ定義
    /// アーキテクチャ準拠: データ管理とInput System統合
    /// </summary>
    [System.Serializable]
    public enum InputDeviceType
    {
        /// <summary>
        /// キーボード＆マウス入力（PC標準）
        /// </summary>
        KeyboardMouse = 0,

        /// <summary>
        /// ゲームパッド入力（コントローラー）
        /// </summary>
        Gamepad = 1,

        /// <summary>
        /// タッチ入力（モバイル）
        /// </summary>
        Touch = 2,

        /// <summary>
        /// 不明なデバイス
        /// </summary>
        Unknown = 99
    }

    /// <summary>
    /// InputDeviceType拡張メソッド
    /// </summary>
    public static class InputDeviceTypeExtensions
    {
        /// <summary>
        /// デバイスタイプの説明を取得
        /// </summary>
        public static string GetDescription(this InputDeviceType deviceType)
        {
            return deviceType switch
            {
                InputDeviceType.KeyboardMouse => "キーボード＆マウス（PC標準入力）",
                InputDeviceType.Gamepad => "ゲームパッド（コントローラー入力）",
                InputDeviceType.Touch => "タッチ入力（モバイル端末）",
                InputDeviceType.Unknown => "不明なデバイス",
                _ => "サポート外デバイス"
            };
        }

        /// <summary>
        /// デバイスが精密操作対応かどうか判定
        /// </summary>
        public static bool SupportsPreciseAiming(this InputDeviceType deviceType)
        {
            return deviceType switch
            {
                InputDeviceType.KeyboardMouse => true,
                InputDeviceType.Gamepad => true,
                InputDeviceType.Touch => false,
                _ => false
            };
        }

        /// <summary>
        /// デバイスが連続入力対応かどうか判定
        /// </summary>
        public static bool SupportsContinuousInput(this InputDeviceType deviceType)
        {
            return deviceType switch
            {
                InputDeviceType.KeyboardMouse => true,
                InputDeviceType.Gamepad => true,
                InputDeviceType.Touch => true,
                _ => false
            };
        }

        /// <summary>
        /// デバイスの推奨感度倍率を取得
        /// </summary>
        public static float GetRecommendedSensitivityMultiplier(this InputDeviceType deviceType)
        {
            return deviceType switch
            {
                InputDeviceType.KeyboardMouse => 1.0f,
                InputDeviceType.Gamepad => 1.5f,
                InputDeviceType.Touch => 0.8f,
                _ => 1.0f
            };
        }
    }
}