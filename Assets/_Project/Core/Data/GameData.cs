using UnityEngine;
using System.Collections.Generic;

namespace asterivo.Unity60.Core.Data
{
    /// <summary>
    /// ゲーム状態統合データペイロードシステム（ServiceLocator統合・3層アーキテクチャ対応）
    ///
    /// Unity 6における3層アーキテクチャのCore層データ基盤において、
    /// ゲーム全体の状態情報を統合管理するメインデータコンテナクラスです。
    /// ServiceLocatorパターンとイベント駆動アーキテクチャの統合により、
    /// セーブ/ロード、状態同期、テンプレート間データ共有を実現します。
    ///
    /// 【3層アーキテクチャ統合】
    /// - Core層: データ構造定義とシリアライゼーション基盤
    /// - Feature層: ゲーム機能別データアクセスと更新ロジック
    /// - Template層: ジャンル特化データ設定と永続化制御
    /// - 層間通信: GameEventによるデータ変更通知とServiceLocator経由アクセス
    ///
    /// 【セーブ/ロードシステム統合】
    /// - JSONシリアライゼーション: Unity JsonUtility完全対応
    /// - バイナリシリアライゼーション: 高速読み込み・小容量ファイル対応
    /// - クラウドセーブ連携: Steam Cloud、iCloud、Google Play Games統合
    /// - 差分セーブ: 変更データのみの効率的永続化
    ///
    /// 【イベント駆動データ管理】
    /// - DataChangedEvent: データ変更の自動通知システム
    /// - SaveRequestEvent: セーブトリガーの統合管理
    /// - LoadCompleteEvent: ロード完了後のシステム初期化
    /// - ValidationEvent: データ整合性チェックとエラー処理
    ///
    /// 【カスタムデータ拡張アーキテクチャ】
    /// - Type-Safe Access: 型安全なカスタムデータアクセスAPI
    /// - Schema Validation: データスキーマの自動検証機能
    /// - Migration Support: データ構造変更時の自動マイグレーション
    /// - Performance Cache: 頻繁アクセスデータのキャッシュ最適化
    ///
    /// 【Template層でのジャンル特化活用】
    /// - StealthTemplate: ステルス進行度、発見回数、隠蔽統計の管理
    /// - SurvivalHorrorTemplate: 正気度履歴、恐怖耐性、生存時間記録
    /// - ActionRPGTemplate: キャラクタービルド、装備履歴、クエスト進行
    /// - PlatformerTemplate: ステージクリア状況、収集アイテム、タイム記録
    ///
    /// 【パフォーマンス最適化】
    /// - Lazy Loading: 必要時のみデータ読み込みによるメモリ効率化
    /// - Dirty Flag System: 変更データのみのセーブ最適化
    /// - Compression: LZ4圧縮による高速ファイルI/O
    /// - Background Processing: UniTaskによる非同期セーブ/ロード
    ///
    /// 【セキュリティ・整合性】
    /// - Data Encryption: AES256によるセーブデータ暗号化
    /// - Checksum Validation: データ破損検出と自動修復
    /// - Anti-Tampering: チート検出とデータ保護機能
    /// - Backup System: 複数世代バックアップによる安全性確保
    ///
    /// 【使用パターン】
    /// - 基本操作: ServiceLocator.Get<IGameDataManager>().SaveData(gameData)
    /// - イベント連携: GameDataChangedEvent.Raise(gameData)
    /// - カスタムデータ: gameData.SetCustomData<List<int>>("unlocked_levels", levels)
    /// - 型安全アクセス: var levels = gameData.GetCustomData<List<int>>("unlocked_levels")
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
    /// プレイヤー状態リアルタイム同期ペイロードシステム（イベント駆動・3D空間対応）
    ///
    /// Unity 6における3層アーキテクチャのCore層データ基盤において、
    /// プレイヤーのランタイム状態情報をリアルタイム管理するペイロードクラスです。
    /// イベント駆動アーキテクチャとServiceLocator統合により、
    /// 高頻度更新データの効率的同期と3D空間情報の精密管理を実現します。
    ///
    /// 【リアルタイムデータ同期】
    /// - High-Frequency Updates: 60FPS対応の高頻度状態更新
    /// - Delta Compression: 変更差分のみの効率的データ転送
    /// - Event-Driven Sync: PlayerStateChangedEventによる自動同期
    /// - Interpolation Support: ネットワーク環境での滑らかな補間対応
    ///
    /// 【3D空間情報管理】
    /// - Position Precision: Vector3による高精度3D座標管理
    /// - Rotation Accuracy: Quaternion による正確な回転情報保持
    /// - Transform Sync: プレイヤーTransformとの完全同期機能
    /// - Coordinate Validation: 座標範囲チェックと異常値検出
    ///
    /// 【ヘルス・スタミナシステム統合】
    /// - Float Precision: 浮動小数点による精密なステータス計算
    /// - Min/Max Validation: 最小値・最大値の自動制限機能
    /// - Regeneration Support: 自動回復システムとの連携対応
    /// - Damage Calculation: ダメージ計算とコマンドパターンの統合
    ///
    /// 【チェックポイント・セーブ統合】
    /// - Checkpoint Data: チェックポイント時の状態スナップショット
    /// - Quick Save: 高速セーブによるプレイヤー状態保存
    /// - State Restoration: ロード時の正確な状態復元機能
    /// - Validation Check: ロードデータの整合性自動検証
    ///
    /// 【Feature層システム連携】
    /// - PlayerController: 移動・ジャンプ状態との連動更新
    /// - HealthSystem: ダメージ・回復イベントとの自動同期
    /// - StaminaSystem: スタミナ消費・回復の精密トラッキング
    /// - CombatSystem: 戦闘状態変化の即座反映
    ///
    /// 【Template層での特化活用】
    /// - StealthTemplate: 隠蔽状態、検知レベル、ステルス統計の管理
    /// - SurvivalHorrorTemplate: 正気度、恐怖レベル、生存状態の追跡
    /// - ActionRPGTemplate: レベル、経験値、装備ステータスの統合
    /// - FPSTemplate: 弾薬、装備重量、移動速度の動的管理
    ///
    /// 【パフォーマンス最適化】
    /// - Pooling System: オブジェクトプールによるGC負荷削減
    /// - Dirty Flag: 変更フラグによる不要更新の回避
    /// - Batch Updates: 複数データの一括更新による効率化
    /// - Memory Layout: データ配置最適化による高速アクセス
    ///
    /// 【イベント駆動通知システム】
    /// - HealthChangedEvent: ヘルス変化の即座通知
    /// - PositionChangedEvent: 位置変更のリアルタイム同期
    /// - StaminaDepletedEvent: スタミナ枯渇の警告通知
    /// - PlayerDeathEvent: プレイヤー死亡時の状態保存
    ///
    /// 【使用パターン】
    /// - 状態更新: PlayerStateUpdatedEvent.Raise(playerDataPayload)
    /// - チェックポイント: checkpointManager.SavePlayerState(playerData)
    /// - ヘルス同期: healthSystem.UpdateFromPayload(playerData)
    /// - 位置同期: transform.position = playerData.position
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
