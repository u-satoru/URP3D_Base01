using System;

namespace asterivo.Unity60.Features.Player
{
    /// <summary>
    /// プレイヤーの状態定義（全体共通）
    /// </summary>
    [Serializable]
    public enum PlayerState
    {
        // 基本状態
        Idle,       // 待機
        Walking,    // 歩き
        Running,    // 走り
        Sprinting,  // スプリント

        // 空中状態
        Jumping,    // ジャンプ
        Falling,    // 落下
        Landing,    // 着地

        // 戦闘状態（階層）
        Combat,             // 戦闘モード
        CombatIdle,         // 戦闘待機
        CombatAttacking,    // 攻撃中
        CombatDefending,    // 防御中
        CombatDodging,      // 回避中

        // 特殊状態
        Interacting,    // インタラクション中
        Swimming,       // 泳ぎ
        Climbing,       // 登り
        Sliding,        // スライディング
        Dead           // 死亡
    }

    // GameState enumは asterivo.Unity60.Core.Audio.AudioManager.cs で定義されています
    // このファイルはPlayerState専用です
}
