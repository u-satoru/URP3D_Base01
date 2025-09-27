using UnityEngine;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Data
{
    /// <summary>
    /// 敵キャラクターの設定データ
    /// HP、攻撃力、ルーンドロップ量などを定義します
    /// </summary>
    [CreateAssetMenu(fileName = "New Enemy Data", menuName = "ActionRPG/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("基本ステータス")]
        [SerializeField] private int _maxHealth = 50;
        [SerializeField] private int _attackPower = 5;
        [SerializeField] private float _moveSpeed = 3.5f;
        [SerializeField] private float _attackRange = 1.5f;
        [SerializeField] private float _detectionRange = 8.0f;

        [Header("ドロップアイテム")]
        [SerializeField] private int _runeDropAmount = 100;
        [SerializeField] private float _runeDropChance = 1.0f;  // ドロップ確率（0.0-1.0）
        [SerializeField] private GameObject _dropItemPrefab;     // ドロップするアイテムプレファブ

        [Header("敵タイプ情報")]
        [SerializeField] private string _enemyName = "スウォーマー";
        [SerializeField] private string _enemyDescription = "群れをなして襲いかかる基本的な敵";
        [SerializeField] private EnemyType _enemyType = EnemyType.Swarmer;

        [Header("AI設定")]
        [SerializeField] private float _attackCooldown = 2.0f;
        [SerializeField] private float _patrolRadius = 5.0f;
        [SerializeField] private bool _returnsToSpawnPoint = true;

        // プロパティ
        public int MaxHealth => _maxHealth;
        public int AttackPower => _attackPower;
        public float MoveSpeed => _moveSpeed;
        public float AttackRange => _attackRange;
        public float DetectionRange => _detectionRange;
        
        public int RuneDropAmount => _runeDropAmount;
        public float RuneDropChance => _runeDropChance;
        public GameObject DropItemPrefab => _dropItemPrefab;
        
        public string EnemyName => _enemyName;
        public string EnemyDescription => _enemyDescription;
        public EnemyType EnemyType => _enemyType;
        
        public float AttackCooldown => _attackCooldown;
        public float PatrolRadius => _patrolRadius;
        public bool ReturnsToSpawnPoint => _returnsToSpawnPoint;

        /// <summary>
        /// レベルスケーリングを考慮したHP
        /// </summary>
        public int GetScaledHealth(int level)
        {
            return Mathf.RoundToInt(_maxHealth * (1 + (level - 1) * 0.2f));
        }

        /// <summary>
        /// レベルスケーリングを考慮した攻撃力
        /// </summary>
        public int GetScaledAttackPower(int level)
        {
            return Mathf.RoundToInt(_attackPower * (1 + (level - 1) * 0.15f));
        }

        /// <summary>
        /// レベルスケーリングを考慮したルーンドロップ量
        /// </summary>
        public int GetScaledRuneDropAmount(int level)
        {
            return Mathf.RoundToInt(_runeDropAmount * (1 + (level - 1) * 0.1f));
        }
    }

    /// <summary>
    /// 敵の種類を定義する列挙型
    /// </summary>
    public enum EnemyType
    {
        Swarmer,      // 群れ敵
        Guard,        // 守備型敵
        Archer,       // 遠距離攻撃型敵
        Elite,        // エリート敵
        Boss          // ボス敵
    }
}
