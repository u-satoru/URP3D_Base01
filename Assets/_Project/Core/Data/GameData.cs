using UnityEngine;
using System.Collections.Generic;

namespace asterivo.Unity60.Core.Data
{
    /// <summary>
    /// ゲーム全体の状態データを管理するペイロードクラス
    /// セーブ/ロード機能やゲーム状態の永続化に使用される
    /// 
    /// 設計思想:
    /// - イベント駆動型アーキテクチャでのデータ伝達に最適化
    /// - JSONシリアライゼーション対応
    /// - 拡張性を考慮した柔軟なカスタムデータ構造
    /// - ゲーム進行に関わる基本情報の一元管理
    /// 
    /// 使用例:
    /// var gameData = new GameData();
    /// gameData.level = 5;
    /// gameData.score = 12000;
    /// gameData.customData["unlocked_levels"] = new List&lt;int&gt; { 1, 2, 3, 4, 5 };
    /// </summary>
    [System.Serializable]
    public class GameData
    {
        /// <summary>現在のゲームレベル（1から開始）</summary>
        public int level;
        
        /// <summary>累積ゲームプレイ時間（秒単位）</summary>
        public float gameTime;
        
        /// <summary>現在のスコア</summary>
        public int score;
        
        /// <summary>残りライフ数</summary>
        public int lives;
        
        /// <summary>最高スコア記録</summary>
        public int highScore;
        
        /// <summary>プレイヤー名</summary>
        public string playerName;
        
        /// <summary>
        /// 拡張用のカスタムデータストレージ
        /// アチーブメント、アンロック情報、設定値などを格納
        /// </summary>
        /// <remarks>
        /// キー: データの識別子（例: "achievements", "settings", "unlocked_items"）
        /// 値: 任意のオブジェクト（プリミティブ型、コレクション、カスタムクラス等）
        /// </remarks>
        public Dictionary<string, object> customData;
        
        /// <summary>
        /// GameDataクラスのデフォルトコンストラクタ
        /// customDataディクショナリを初期化し、即座に使用可能にする
        /// </summary>
        /// <remarks>
        /// 他のフィールドはデフォルト値（int: 0, float: 0f, string: null）で初期化される
        /// </remarks>
        public GameData()
        {
            customData = new Dictionary<string, object>();
        }
    }
    
    /// <summary>
    /// プレイヤーの状態データを管理するペイロードクラス
    /// プレイヤーの位置、ヘルス、スタミナなどのランタイム情報を格納
    /// 
    /// 設計思想:
    /// - リアルタイム同期やチェックポイント機能に対応
    /// - 3D空間での位置・回転情報の完全保存
    /// - ヘルス・スタミナシステムとの統合
    /// - イベント通知でのデータ伝達に最適化
    /// 
    /// 使用例:
    /// var playerData = new PlayerDataPayload();
    /// playerData.playerName = "Hero";
    /// playerData.position = player.transform.position;
    /// playerData.currentHealth = 80.5f;
    /// OnPlayerDataChanged?.Raise(playerData);
    /// </summary>
    [System.Serializable]
    public class PlayerDataPayload
    {
        /// <summary>プレイヤー名またはキャラクター名</summary>
        public string playerName;
        
        /// <summary>3D空間でのプレイヤー位置座標</summary>
        public Vector3 position;
        
        /// <summary>プレイヤーの向きと回転情報</summary>
        public Quaternion rotation;
        
        /// <summary>現在のヘルス値（浮動小数点で精密な計算に対応）</summary>
        public float currentHealth;
        
        /// <summary>最大ヘルス値（レベルアップや装備で変動可能）</summary>
        public float maxHealth;
        
        /// <summary>現在のスタミナ値（走行、攻撃、回避等で消費）</summary>
        public float currentStamina;
        
        /// <summary>最大スタミナ値（トレーニングや装備で向上可能）</summary>
        public float maxStamina;
        
        /// <summary>プレイヤーの獲得スコア</summary>
        public int score;
    }
}
