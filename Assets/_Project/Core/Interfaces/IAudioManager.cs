using UnityEngine;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// 統合オーディオ管理サービスインターフェース（ServiceLocator統合・3層アーキテクチャ対応）
    ///
    /// Unity 6における3層アーキテクチャのCore層インターフェース基盤において、
    /// アプリケーション全体のオーディオ制御を一元管理する中核インターフェースです。
    /// ServiceLocatorパターンとIService統合により、
    /// AudioMixer統合、Template層互換性、高度なオーディオ体験を実現します。
    ///
    /// 【AudioMixer統合オーディオ管理】
    /// - Master Volume: アプリケーション全体の統一音量制御
    /// - SFX Volume: 効果音カテゴリの個別音量管理
    /// - Music Volume: BGMカテゴリの個別音量管理
    /// - Real-time Control: リアルタイム音量調整と統一状態管理
    ///
    /// 【高度なオーディオ再生制御】
    /// - SFX Playback: AudioClipおよび名前ベースの柔軟な再生システム
    /// - Music Management: フェードイン/アウト対応のBGM再生管理
    /// - Volume & Pitch Control: 精密な音量・ピッチコントロール
    /// - Loop Management: ループ再生と一度再生の統一管理
    ///
    /// 【システム状態管理】
    /// - Pause/Resume: ゲームポーズ時の統一音声制御
    /// - Mute/Unmute: システムレベルのミュート機能
    /// - State Monitoring: リアルタイム状態監視とクエリ機能
    /// - Cross-Scene Persistence: シーン遷移を跨いだ状態永続化
    ///
    /// 【Template層互換性】
    /// - TPS Template: PlaySFX(string), StopBGM(), ResumeBGM()等の互換メソッド
    /// - Stealth Template: ステルスゲーム用の特化音響制御
    /// - Action RPG Template: バトルシステム連動のオーディオフィードバック
    /// - Common Interface: 全Templateで共通使用可能な統一API
    ///
    /// 【ServiceLocatorパターン統合】
    /// - Dependency Injection: ServiceLocator.Get<IAudioManager>()による統一アクセス
    /// - Mock Support: ユニットテスト用モックサービス登録対応
    /// - Lifecycle Management: IServiceライフサイクルとの完全統合
    /// - Cross-Layer Access: Core/Feature/Template層からの統一アクセス
    /// </summary>
    public interface IAudioManager : IService
    {
        /// <summary>
        /// マスター音量レベルの設定（アプリケーション全体音量制御）
        /// </summary>
        /// <param name="volume">音量レベル（0.0～1.0の範囲、AudioMixerマスターグループに適用）</param>
        void SetMasterVolume(float volume);

        /// <summary>
        /// SFX音量レベルの設定（効果音カテゴリ個別制御）
        /// </summary>
        /// <param name="volume">音量レベル（0.0～1.0の範囲、SFX AudioMixerGroupに適用）</param>
        void SetSFXVolume(float volume);

        /// <summary>
        /// ミュージック音量レベルの設定（BGMカテゴリ個別制御）
        /// </summary>
        /// <param name="volume">音量レベル（0.0～1.0の範囲、Music AudioMixerGroupに適用）</param>
        void SetMusicVolume(float volume);

        /// <summary>
        /// Get current master volume
        /// </summary>
        float GetMasterVolume();

        /// <summary>
        /// Get current SFX volume
        /// </summary>
        float GetSFXVolume();

        /// <summary>
        /// Get current music volume
        /// </summary>
        float GetMusicVolume();

        /// <summary>
        /// サウンドエフェクトの再生（AudioClip直接指定方式）
        /// </summary>
        /// <param name="clip">再生するAudioClipオブジェクト</param>
        /// <param name="volume">音量倍率（1.0fがデフォルト、SFX音量設定と乗算）</param>
        /// <param name="pitch">ピッチ倍率（1.0fがデフォルト、音の高さ変更）</param>
        void PlaySFX(AudioClip clip, float volume = 1.0f, float pitch = 1.0f);

        /// <summary>
        /// サウンドエフェクトの再生（名前指定方式・TPS Template互換性）
        /// </summary>
        /// <param name="clipName">再生するオーディオクリップ名（ResourcesまたはAddressableから検索）</param>
        /// <param name="volume">音量倍率（1.0fがデフォルト、SFX音量設定と乗算）</param>
        /// <param name="pitch">ピッチ倍率（1.0fがデフォルト、音の高さ変更）</param>
        void PlaySFX(string clipName, float volume = 1.0f, float pitch = 1.0f);

        /// <summary>
        /// Play background music
        /// </summary>
        /// <param name="clip">Music clip to play</param>
        /// <param name="loop">Whether to loop the music</param>
        /// <param name="fadeIn">Fade in duration</param>
        void PlayMusic(AudioClip clip, bool loop = true, float fadeIn = 0.0f);

        /// <summary>
        /// Stop current music
        /// </summary>
        /// <param name="fadeOut">Fade out duration</param>
        void StopMusic(float fadeOut = 0.0f);

        /// <summary>
        /// Pause all audio
        /// </summary>
        void PauseAll();

        /// <summary>
        /// Resume all audio
        /// </summary>
        void ResumeAll();

        /// <summary>
        /// Mute all audio
        /// </summary>
        void MuteAll();

        /// <summary>
        /// Unmute all audio
        /// </summary>
        void UnmuteAll();

        /// <summary>
        /// Check if audio is currently muted
        /// </summary>
        bool IsMuted { get; }

        /// <summary>
        /// Stop background music (TPS Template compatibility)
        /// </summary>
        void StopBGM();

        /// <summary>
        /// Resume background music (TPS Template compatibility)
        /// </summary>
        void ResumeBGM();
    }
}