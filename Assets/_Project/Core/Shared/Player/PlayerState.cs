using System;

namespace asterivo.Unity60.Core.Player
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
    
    /// <summary>
    /// ゲームの状態定義
    /// </summary>
    [Serializable]
    public enum GameState
    {
        MainMenu,   // メインメニュー
        Loading,    // ローディング
        Playing,    // ゲームプレイ中
        Paused,     // 一時停止
        GameOver,   // ゲームオーバー
        Victory     // 勝利
    }
}