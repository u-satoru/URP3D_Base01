namespace asterivo.Unity60.Core.Components
{
    /// <summary>
    /// ヘルス関連の操作を受け取ることができるオブジェクトのインターフェース
    /// コマンドパターンによる疎結合設計を実現し、直接的な依存関係なしに任意のヘルスシステムと連携可能
    /// 
    /// 設計思想:
    /// - インターフェース分離の原則に基づく軽量な設計
    /// - プレイヤー、敵、建物など多様なオブジェクトで共通利用
    /// - コマンドシステムとの統合による拡張性の確保
    /// - 属性ダメージシステムへの対応
    /// 
    /// 実装例:
    /// public class PlayerHealth : MonoBehaviour, IHealthTarget
    /// {
    ///     public int CurrentHealth { get; private set; }
    ///     public int MaxHealth { get; private set; }
    ///     public void Heal(int amount) { /* 回復処理 */ }
    ///     public void TakeDamage(int amount) { /* ダメージ処理 */ }
    ///     public void TakeDamage(int amount, string elementType) { /* 属性ダメージ処理 */ }
    /// }
    /// </summary>
    public interface IHealthTarget
    {
        /// <summary>
        /// 指定された量だけターゲットを回復させます
        /// 最大ヘルスを超えることはできません
        /// </summary>
        /// <param name="amount">回復量（正の整数）</param>
        /// <remarks>
        /// - 負の値が渡された場合の動作は実装に依存
        /// - 回復時にエフェクトやサウンドの再生を含むことが推奨される
        /// - 最大ヘルス制限の処理は各実装クラスで行う
        /// </remarks>
        void Heal(int amount);
        
        /// <summary>
        /// 指定された量のダメージをターゲットに与えます
        /// 物理ダメージや基本ダメージの処理に使用
        /// </summary>
        /// <param name="amount">ダメージ量（正の整数）</param>
        /// <remarks>
        /// - 防御力計算やダメージ軽減は実装クラス内で処理
        /// - 死亡判定やノックバック処理の契機となる可能性がある
        /// - ダメージイベントの発火は実装クラスの責任
        /// </remarks>
        void TakeDamage(int amount);
        
        /// <summary>
        /// 属性タイプ付きのダメージをターゲットに与えます
        /// 火、氷、雷などの属性ダメージシステムで使用
        /// </summary>
        /// <param name="amount">ダメージ量（正の整数）</param>
        /// <param name="elementType">属性タイプ（例: "fire", "ice", "thunder", "physical"）</param>
        /// <remarks>
        /// - 属性に応じた耐性や弱点の計算が可能
        /// - 属性別のエフェクトや状態異常の発動契機
        /// - elementTypeは標準化された文字列の使用を推奨
        /// - nullまたは空文字列の場合は物理ダメージとして扱う
        /// </remarks>
        void TakeDamage(int amount, string elementType);
        
        /// <summary>
        /// ターゲットの現在のヘルス値を取得します
        /// </summary>
        /// <value>現在のヘルス（0以上の整数）</value>
        /// <remarks>
        /// - 0の場合は死亡状態を示すことが一般的
        /// - UIやAIの判断材料として使用される
        /// - リアルタイムでの変更が反映される必要がある
        /// </remarks>
        int CurrentHealth { get; }
        
        /// <summary>
        /// ターゲットの最大ヘルス値を取得します
        /// </summary>
        /// <value>最大ヘルス（1以上の整数）</value>
        /// <remarks>
        /// - 回復の上限値として使用される
        /// - HPバーの表示計算に使用される
        /// - レベルアップや装備変更で変動する可能性がある
        /// </remarks>
        int MaxHealth { get; }
    }
}
