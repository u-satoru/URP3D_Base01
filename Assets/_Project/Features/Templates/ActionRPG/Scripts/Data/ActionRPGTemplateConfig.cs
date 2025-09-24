using UnityEngine;
using asterivo.Unity60.Features.Templates.Common;

namespace asterivo.Unity60.Features.ActionRPG.Data
{
    /// <summary>
    /// ActionRPGテンプレートの設定データ
    /// プレイヤー、敵、アイテムなどの設定を統合管理します
    /// </summary>
    [CreateAssetMenu(fileName = "ActionRPG_TemplateConfig", menuName = "Templates/ActionRPG Template Config")]
    public class ActionRPGTemplateConfig : GenreTemplateConfig
    {
        [Header("プレイヤー設定")]
        [SerializeField] private CharacterClassData _playerClass;
        [SerializeField] private GameObject _playerPrefab;

        [Header("敵設定")]
        [SerializeField] private EnemyData[] _enemyTypes;
        [SerializeField] private GameObject[] _enemyPrefabs;

        [Header("アイテム設定")]
        [SerializeField] private ItemData _runeItemData;
        [SerializeField] private GameObject _runePickupPrefab;

        [Header("レベリング設定")]
        [SerializeField] private LevelUpCurveData _levelCurve;
        [SerializeField] private GameObject _levelUpShrinePrefab;

        [Header("シーン設定")]
        [SerializeField] private string _sampleSceneName = "ActionRPG_SampleScene";
        [SerializeField] private string _uiSceneName = "ActionRPG_UIScene";

        [Header("ゲームプレイ設定")]
        [SerializeField] private float _combatRange = 2.0f;
        [SerializeField] private float _interactionRange = 3.0f;
        [SerializeField] private int _initialRuneAmount = 0;
        [SerializeField] private bool _enableAutoSave = true;

        // プロパティ
        public CharacterClassData PlayerClass => _playerClass;
        public GameObject PlayerPrefab => _playerPrefab;
        public EnemyData[] EnemyTypes => _enemyTypes;
        public GameObject[] EnemyPrefabs => _enemyPrefabs;
        public ItemData RuneItemData => _runeItemData;
        public GameObject RunePickupPrefab => _runePickupPrefab;
        public LevelUpCurveData LevelCurve => _levelCurve;
        public GameObject LevelUpShrinePrefab => _levelUpShrinePrefab;
        public string SampleSceneName => _sampleSceneName;
        public string UISceneName => _uiSceneName;
        public float CombatRange => _combatRange;
        public float InteractionRange => _interactionRange;
        public int InitialRuneAmount => _initialRuneAmount;
        public bool EnableAutoSave => _enableAutoSave;

        /// <summary>
        /// 指定タイプの敵データを取得
        /// </summary>
        public EnemyData GetEnemyData(EnemyType enemyType)
        {
            foreach (var enemy in _enemyTypes)
            {
                if (enemy != null && enemy.EnemyType == enemyType)
                {
                    return enemy;
                }
            }
            return null;
        }

        /// <summary>
        /// 指定タイプの敵プレファブを取得
        /// </summary>
        public GameObject GetEnemyPrefab(EnemyType enemyType)
        {
            var enemyData = GetEnemyData(enemyType);
            if (enemyData == null) return null;

            // EnemyTypesとEnemyPrefabsの対応関係を利用
            for (int i = 0; i < _enemyTypes.Length && i < _enemyPrefabs.Length; i++)
            {
                if (_enemyTypes[i] == enemyData)
                {
                    return _enemyPrefabs[i];
                }
            }
            return null;
        }

        /// <summary>
        /// テンプレートの初期化チェック
        /// </summary>
        public bool IsConfigValid()
        {
            if (_playerClass == null) return false;
            if (_playerPrefab == null) return false;
            if (_enemyTypes == null || _enemyTypes.Length == 0) return false;
            if (_enemyPrefabs == null || _enemyPrefabs.Length == 0) return false;
            if (_runeItemData == null) return false;
            if (_runePickupPrefab == null) return false;
            if (_levelCurve == null) return false;
            if (_levelUpShrinePrefab == null) return false;
            
            return true;
        }

        /// <summary>
        /// プレイヤーの初期ステータスを計算
        /// </summary>
        public (int health, int focus, int attack) CalculateInitialStats()
        {
            if (_playerClass == null) return (100, 50, 10);
            
            int health = _playerClass.GetMaxHealth();
            int focus = _playerClass.GetMaxFocus();
            int attack = _playerClass.GetAttackPower();
            
            return (health, focus, attack);
        }
    }
}