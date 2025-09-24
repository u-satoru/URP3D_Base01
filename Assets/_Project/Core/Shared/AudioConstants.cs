using UnityEngine;

namespace asterivo.Unity60.Core.Shared
{
    /// <summary>
    /// 音響システム全体で使用する定数定義
    /// Magic Numberを排除し、一元管理で保守性を向上
    /// </summary>
    public static class AudioConstants
    {
        #region Volume Settings
        /// <summary>
        /// デフォルトのマスターボリューム
        /// </summary>
        public const float DEFAULT_MASTER_VOLUME = 1f;
        
        /// <summary>
        /// デフォルトのBGMボリューム
        /// </summary>
        public const float DEFAULT_BGM_VOLUME = 0.8f;
        
        /// <summary>
        /// デフォルトの環境音ボリューム
        /// </summary>
        public const float DEFAULT_AMBIENT_VOLUME = 0.6f;
        
        /// <summary>
        /// デフォルトの効果音ボリューム
        /// </summary>
        public const float DEFAULT_EFFECT_VOLUME = 1f;
        
        /// <summary>
        /// デフォルトのステルス音響ボリューム
        /// </summary>
        public const float DEFAULT_STEALTH_VOLUME = 1f;
        #endregion
        
        #region Fade and Transition
        /// <summary>
        /// デフォルトのフェード時間（秒）
        /// </summary>
        public const float DEFAULT_FADE_DURATION = 0.3f;
        
        /// <summary>
        /// 環境音の遷移時間（秒）
        /// </summary>
        public const float ENVIRONMENT_TRANSITION_TIME = 2f;
        
        /// <summary>
        /// 天気音響の遷移時間（秒）
        /// </summary>
        public const float WEATHER_TRANSITION_TIME = 3f;
        
        /// <summary>
        /// BGM遷移の最小時間（秒）
        /// </summary>
        public const float BGM_TRANSITION_MIN_TIME = 1f;
        
        /// <summary>
        /// BGM遷移の最大時間（秒）
        /// </summary>
        public const float BGM_TRANSITION_MAX_TIME = 10f;
        #endregion
        
        #region Pool Settings
        /// <summary>
        /// AudioSourceプールのデフォルトサイズ
        /// </summary>
        public const int DEFAULT_AUDIOSOURCE_POOL_SIZE = 4;
        
        /// <summary>
        /// 最大同時発音数
        /// </summary>
        public const int MAX_CONCURRENT_SOUNDS = 32;
        
        /// <summary>
        /// コマンドプールのデフォルトサイズ
        /// </summary>
        public const int DEFAULT_COMMAND_POOL_SIZE = 10;
        
        /// <summary>
        /// コマンドプールの最大サイズ
        /// </summary>
        public const int MAX_COMMAND_POOL_SIZE = 100;
        #endregion
        
        #region Spatial Audio
        /// <summary>
        /// 3D音響の空間ブレンド値（完全に3D）
        /// </summary>
        public const float SPATIAL_BLEND_3D = 1f;
        
        /// <summary>
        /// 2D音響の空間ブレンド値（完全に2D）
        /// </summary>
        public const float SPATIAL_BLEND_2D = 0f;
        
        /// <summary>
        /// デフォルトの最小距離
        /// </summary>
        public const float DEFAULT_MIN_DISTANCE = 1f;
        
        /// <summary>
        /// デフォルトの最大距離
        /// </summary>
        public const float DEFAULT_MAX_DISTANCE = 500f;
        #endregion
        
        #region Stealth Audio
        /// <summary>
        /// グローバルマスキング強度のデフォルト値
        /// </summary>
        public const float DEFAULT_MASKING_STRENGTH = 0.3f;
        
        /// <summary>
        /// マスキング効果の半径（メートル）
        /// </summary>
        public const float DEFAULT_MASKING_RADIUS = 15f;
        
        /// <summary>
        /// オクルージョン計算のインターバル（秒）
        /// </summary>
        public const float OCCLUSION_CHECK_INTERVAL = 0.1f;
        
        /// <summary>
        /// 最大オクルージョン減衰率
        /// </summary>
        public const float MAX_OCCLUSION_REDUCTION = 0.8f;
        #endregion
        
        #region Mixer Parameters
        /// <summary>
        /// AudioMixerのマスターボリュームパラメータ名
        /// </summary>
        public const string MIXER_MASTER_VOLUME = "MasterVolume";
        
        /// <summary>
        /// AudioMixerのBGMボリュームパラメータ名
        /// </summary>
        public const string MIXER_BGM_VOLUME = "BGMVolume";
        
        /// <summary>
        /// AudioMixerの環境音ボリュームパラメータ名
        /// </summary>
        public const string MIXER_AMBIENT_VOLUME = "AmbientVolume";
        
        /// <summary>
        /// AudioMixerの効果音ボリュームパラメータ名
        /// </summary>
        public const string MIXER_EFFECT_VOLUME = "EffectVolume";
        
        /// <summary>
        /// AudioMixerのステルス音響ボリュームパラメータ名
        /// </summary>
        public const string MIXER_STEALTH_VOLUME = "StealthVolume";
        #endregion
        
        #region Audio Categories Priority
        /// <summary>
        /// UI音響の優先度（最高優先度）
        /// </summary>
        public const int UI_AUDIO_PRIORITY = 0;
        
        /// <summary>
        /// 効果音の優先度（高優先度）
        /// </summary>
        public const int EFFECT_AUDIO_PRIORITY = 64;
        
        /// <summary>
        /// BGMの優先度（中優先度）
        /// </summary>
        public const int BGM_AUDIO_PRIORITY = 128;
        
        /// <summary>
        /// 環境音の優先度（低優先度）
        /// </summary>
        public const int AMBIENT_AUDIO_PRIORITY = 192;
        
        /// <summary>
        /// 最低優先度
        /// </summary>
        public const int LOWEST_AUDIO_PRIORITY = 256;
        #endregion
        
        #region Validation Constants
        /// <summary>
        /// 音量の最小閾値（これ以下は無音とみなす）
        /// </summary>
        public const float MIN_AUDIBLE_VOLUME = 0.01f;
        
        /// <summary>
        /// dB変換時の最小値（-80dB）
        /// </summary>
        public const float MIN_DB_VALUE = -80f;
        
        /// <summary>
        /// dB変換のための最小音量値
        /// </summary>
        public const float MIN_VOLUME_FOR_DB = 0.0001f;
        #endregion
        
        #region Update and Performance
        /// <summary>
        /// オーディオシステムの更新間隔（秒）
        /// </summary>
        public const float AUDIO_UPDATE_INTERVAL = 0.1f;
        
        /// <summary>
        /// 空間キャッシュの更新間隔（秒）
        /// </summary>
        public const float SPATIAL_CACHE_UPDATE_INTERVAL = 5f;
        
        /// <summary>
        /// 1回の更新で処理する最大AudioSource数
        /// </summary>
        public const int MAX_AUDIOSOURCES_PER_UPDATE = 5;
        #endregion
    }
}