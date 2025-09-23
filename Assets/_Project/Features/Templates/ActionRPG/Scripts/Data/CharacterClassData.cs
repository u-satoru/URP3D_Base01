using UnityEngine;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Data
{
    /// <summary>
    /// プレイヤーのキャラクタークラス設定データ
    /// 初期ステータスと装備を定義します
    /// </summary>
    [CreateAssetMenu(fileName = "New Character Class", menuName = "ActionRPG/Character Class Data")]
    public class CharacterClassData : ScriptableObject
    {
        [Header("基礎ステータス")]
        [SerializeField] private int _vitality = 10;         // 生命力
        [SerializeField] private int _strength = 10;         // 筋力
        [SerializeField] private int _dexterity = 5;         // 器用さ
        [SerializeField] private int _intelligence = 5;      // 知力
        [SerializeField] private int _faith = 5;             // 信仰
        [SerializeField] private int _luck = 5;              // 運

        [Header("派生ステータス")]
        [SerializeField] private int _maxHealthBase = 100;    // 基本最大HP
        [SerializeField] private int _maxFocusBase = 50;      // 基本最大フォーカス

        [Header("初期装備")]
        [SerializeField] private GameObject _startingWeapon;   // 初期武器

        [Header("クラス情報")]
        [SerializeField] private string _className = "戦士";
        [SerializeField] private string _classDescription = "バランスの取れた近接戦闘クラス";

        // プロパティ
        public int Vitality => _vitality;
        public int Strength => _strength;
        public int Dexterity => _dexterity;
        public int Intelligence => _intelligence;
        public int Faith => _faith;
        public int Luck => _luck;
        
        public int MaxHealthBase => _maxHealthBase;
        public int MaxFocusBase => _maxFocusBase;
        public GameObject StartingWeapon => _startingWeapon;
        
        public string ClassName => _className;
        public string ClassDescription => _classDescription;

        /// <summary>
        /// 筋力ボーナスを考慮した攻撃力を計算
        /// </summary>
        public int GetAttackPower()
        {
            return Mathf.FloorToInt(_strength * 1.5f); // 筋力 * 1.5 = 攻撃力
        }

        /// <summary>
        /// 生命力ボーナスを考慮した最大HPを計算
        /// </summary>
        public int GetMaxHealth()
        {
            return _maxHealthBase + (_vitality * 10);
        }

        /// <summary>
        /// 知力ボーナスを考慮した最大フォーカスを計算
        /// </summary>
        public int GetMaxFocus()
        {
            return _maxFocusBase + (_intelligence * 5);
        }
    }
}