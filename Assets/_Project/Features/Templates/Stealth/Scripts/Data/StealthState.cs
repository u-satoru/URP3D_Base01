using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Features.Templates.Stealth
{
    /// <summary>
    /// ステルス関連の型定義
    /// アーキテクチャ制約対応: Core層型の利用によるServiceLocator統合
    ///
    /// 基本型（StealthState, DetectionType, ConcealmentLevel）はCore.Dataで定義済み
    /// Feature層では追加の特化型のみ定義
    /// </summary>

    /// <summary>
    /// ステルス動作の種類
    /// プレイヤー行動の分類による適切な検知計算
    /// </summary>
    public enum StealthAction
    {
        /// <summary>通常移動</summary>
        Walking,
        
        /// <summary>走行移動（検知リスク高）</summary>
        Running,
        
        /// <summary>しゃがみ移動（検知リスク低）</summary>
        Crouching,
        
        /// <summary>這う移動（検知リスク最低）</summary>
        Crawling,
        
        /// <summary>静止状態</summary>
        Idle,
        
        /// <summary>オブジェクトインタラクション</summary>
        Interacting
    }

}