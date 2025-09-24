using UnityEngine;
using UnityEngine.SceneManagement;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.Common;
using asterivo.Unity60.Features.Templates.ActionRPG.Data;
using asterivo.Unity60.Features.Templates.ActionRPG.Components;
using asterivo.Unity60.Features.ActionRPG;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Managers
{
    /// <summary>
    /// ActionRPGテンプレートの管理とサンプルシーンの構築を行うマネージャー
    /// TemplateConfigに基づいてプレイヤーと敵を動的に生成します
    /// </summary>
    public class ActionRPGTemplateManager : BaseTemplateManager
    {
        #region Abstract Implementation

        public override GenreType GenreType => GenreType.ActionRPG;
        public override string TemplateName => "Action RPG Template";
        public override string TemplateDescription => "RPG要素統合システム - キャラクター成長・装備・戦略的戦闘のアクションRPGテンプレート";

        #endregion

        [Header("ActionRPG設定")]
        [SerializeField] private ActionRPGTemplateConfig _templateConfig;
        
        [Header("スポーン位置")]
        [SerializeField] private Transform _playerSpawnPoint;
        [SerializeField] private Transform[] _enemySpawnPoints;
        [SerializeField] private Transform[] _shrineSpawnPoints;

        [Header("イベント")]
        [SerializeField] private GameEvent _onPlayerSpawned;
        [SerializeField] private GameEvent _onEnemiesSpawned;
        [SerializeField] private GameEvent _onTemplateInitialized;

        // 生成されたオブジェクト参照
        private GameObject _spawnedPlayer;
        private GameObject[] _spawnedEnemies;
        private GameObject[] _spawnedShrines;
        private ExperienceManager _experienceManager;
        private ActionRPGManager _actionRPGManager;

        void Awake()
        {
            // ActionRPGManagerコンポーネントを追加または取得
            _actionRPGManager = GetComponent<ActionRPGManager>();
            if (_actionRPGManager == null)
            {
                _actionRPGManager = gameObject.AddComponent<ActionRPGManager>();
            }
            Debug.Log("ActionRPGTemplateManager: ActionRPGManager経由でServiceLocatorパターンを活用します");
        }

        protected override void Start()
        {
            base.Start();
            InitializeTemplate();
        }

        void OnDestroy()
        {
            // ActionRPGManagerがクリーンアップを処理
            Debug.Log("ActionRPGTemplateManager: 破棄処理を実行しました");
        }

        /// <summary>
        /// テンプレートを初期化
        /// </summary>
        public override void InitializeTemplate()
        {
            if (_templateConfig == null)
            {
                Debug.LogError("ActionRPGTemplateManager: TemplateConfigが設定されていません。");
                return;
            }

            if (!_templateConfig.IsConfigValid())
            {
                Debug.LogError("ActionRPGTemplateManager: TemplateConfigの設定が不完全です。");
                return;
            }

            // テンプレート初期化シーケンス
            StartCoroutine(InitializeSequence());
        }

        /// <summary>
        /// ActionRPG固有の初期化処理
        /// BaseTemplateManagerから呼び出される抽象メソッドの実装
        /// </summary>
        protected override void OnInitializeTemplate()
        {
            Debug.Log("ActionRPG: 固有初期化を実行中...");

            // ActionRPG特有の設定処理
            if (_templateConfig != null)
            {
                Debug.Log($"ActionRPG設定: プレイヤークラス={_templateConfig.PlayerClass?.name ?? "未設定"}");
                Debug.Log($"ActionRPG設定: 敵種類数={_templateConfig.EnemyTypes?.Length ?? 0}");
                Debug.Log($"ActionRPG設定: 戦闘範囲={_templateConfig.CombatRange}m");
            }
        }

        /// <summary>
        /// 初期化シーケンス
        /// </summary>
        private System.Collections.IEnumerator InitializeSequence()
        {
            Debug.Log("ActionRPG テンプレート初期化開始...");

            // 1. プレイヤー生成
            yield return new WaitForSeconds(0.1f);
            SpawnPlayer();

            // 2. 敵生成
            yield return new WaitForSeconds(0.2f);
            SpawnEnemies();

            // 3. 神殿生成
            yield return new WaitForSeconds(0.1f);
            SpawnShrines();

            // 4. ExperienceManager初期化
            yield return new WaitForSeconds(0.1f);
            InitializeExperienceManager();

            // 5. UI初期化
            yield return new WaitForSeconds(0.1f);
            InitializeUI();

            // 完了通知
            if (_onTemplateInitialized != null)
                _onTemplateInitialized.Raise();

            Debug.Log("ActionRPG テンプレート初期化完了！");
            LogTemplateStatus();
        }

        /// <summary>
        /// プレイヤーを生成
        /// </summary>
        private void SpawnPlayer()
        {
            if (_templateConfig.PlayerPrefab == null)
            {
                Debug.LogWarning("プレイヤープレファブが設定されていません。");
                return;
            }

            Vector3 spawnPosition = _playerSpawnPoint != null ? 
                _playerSpawnPoint.position : Vector3.zero;

            _spawnedPlayer = Instantiate(_templateConfig.PlayerPrefab, spawnPosition, Quaternion.identity);
            _spawnedPlayer.name = "ARPG_Player";

            // ActionRPGManagerにプレイヤーを設定
            if (_actionRPGManager != null)
            {
                _actionRPGManager.SetPlayer(_spawnedPlayer);
            }

            // プレイヤーにStatComponentを設定
            var statComponent = _spawnedPlayer.GetComponent<StatComponent>();
            if (statComponent != null)
            {
                // プライベートフィールドに初期クラスデータを設定
                var field = typeof(StatComponent).GetField("_initialClassData", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(statComponent, _templateConfig.PlayerClass);

                var levelCurveField = typeof(StatComponent).GetField("_levelCurve", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                levelCurveField?.SetValue(statComponent, _templateConfig.LevelCurve);
            }

            // インベントリに初期ルーンを追加
            var inventory = _spawnedPlayer.GetComponent<InventoryComponent>();
            if (inventory != null && _templateConfig.InitialRuneAmount > 0)
            {
                inventory.AddItem(_templateConfig.RuneItemData, _templateConfig.InitialRuneAmount);
            }

            if (_onPlayerSpawned != null)
                _onPlayerSpawned.Raise();

            Debug.Log($"プレイヤー生成完了: {spawnPosition}");
        }

        /// <summary>
        /// 敵を生成
        /// </summary>
        private void SpawnEnemies()
        {
            if (_enemySpawnPoints == null || _enemySpawnPoints.Length == 0)
            {
                Debug.LogWarning("敵のスポーン地点が設定されていません。");
                return;
            }

            _spawnedEnemies = new GameObject[_enemySpawnPoints.Length];

            for (int i = 0; i < _enemySpawnPoints.Length; i++)
            {
                if (_enemySpawnPoints[i] == null) continue;

                // スウォーマータイプの敵プレファブを取得
                GameObject enemyPrefab = _templateConfig.GetEnemyPrefab(EnemyType.Swarmer);
                if (enemyPrefab == null)
                {
                    Debug.LogWarning($"スウォーマー敵プレファブが見つかりません。インデックス: {i}");
                    continue;
                }

                Vector3 spawnPosition = _enemySpawnPoints[i].position;
                GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                enemy.name = $"ARPG_Swarmer_{i}";

                // 敵データを設定
                var lootDrop = enemy.GetComponent<LootDropComponent>();
                if (lootDrop != null)
                {
                    var swarmerData = _templateConfig.GetEnemyData(EnemyType.Swarmer);
                    lootDrop.SetEnemyData(swarmerData);
                }

                _spawnedEnemies[i] = enemy;
            }

            if (_onEnemiesSpawned != null)
                _onEnemiesSpawned.Raise();

            Debug.Log($"敵生成完了: {_spawnedEnemies.Length}体");
        }

        /// <summary>
        /// 神殿を生成
        /// </summary>
        private void SpawnShrines()
        {
            if (_shrineSpawnPoints == null || _shrineSpawnPoints.Length == 0)
            {
                Debug.LogWarning("神殿のスポーン地点が設定されていません。");
                return;
            }

            if (_templateConfig.LevelUpShrinePrefab == null)
            {
                Debug.LogWarning("神殿プレファブが設定されていません。");
                return;
            }

            _spawnedShrines = new GameObject[_shrineSpawnPoints.Length];

            for (int i = 0; i < _shrineSpawnPoints.Length; i++)
            {
                if (_shrineSpawnPoints[i] == null) continue;

                Vector3 spawnPosition = _shrineSpawnPoints[i].position;
                GameObject shrine = Instantiate(_templateConfig.LevelUpShrinePrefab, spawnPosition, Quaternion.identity);
                shrine.name = $"ARPG_LevelUpShrine_{i}";

                _spawnedShrines[i] = shrine;
            }

            Debug.Log($"神殿生成完了: {_spawnedShrines.Length}個");
        }

        /// <summary>
        /// ExperienceManagerを初期化
        /// </summary>
        private void InitializeExperienceManager()
        {
            _experienceManager = FindObjectOfType<ExperienceManager>();
            if (_experienceManager == null)
            {
                // ExperienceManagerが存在しない場合は作成
                GameObject expManagerObj = new GameObject("ExperienceManager");
                _experienceManager = expManagerObj.AddComponent<ExperienceManager>();
                Debug.Log("ExperienceManagerを作成しました。");
            }

            // ActionRPGManagerを使用してExperienceManagerを初期化
            // ExperienceManagerも疎結合化の対象となる可能性があります
        }

        /// <summary>
        /// UIを初期化
        /// </summary>
        private void InitializeUI()
        {
            // TODO: ActionRPG専用UIの初期化
            Debug.Log("ActionRPG UI初期化（実装予定）");
        }

        /// <summary>
        /// テンプレート状態をログ出力
        /// </summary>
        private void LogTemplateStatus()
        {
            var playerStats = _templateConfig.CalculateInitialStats();
            
            Debug.Log($"=== ActionRPG Template Status ===\n" +
                     $"プレイヤークラス: {_templateConfig.PlayerClass?.ClassName ?? "未設定"}\n" +
                     $"初期ステータス - HP: {playerStats.health}, FP: {playerStats.focus}, ATK: {playerStats.attack}\n" +
                     $"敵の種類数: {_templateConfig.EnemyTypes?.Length ?? 0}\n" +
                     $"生成された敵: {_spawnedEnemies?.Length ?? 0}体\n" +
                     $"生成された神殿: {_spawnedShrines?.Length ?? 0}個\n" +
                     $"初期ルーン量: {_templateConfig.InitialRuneAmount}\n" +
                     $"戦闘範囲: {_templateConfig.CombatRange}m\n" +
                     $"インタラクション範囲: {_templateConfig.InteractionRange}m");
        }


        /// <summary>
        /// プレイヤーオブジェクトを取得
        /// </summary>
        public GameObject GetPlayer()
        {
            return _spawnedPlayer;
        }

        /// <summary>
        /// 生成された敵オブジェクトを取得
        /// </summary>
        public GameObject[] GetEnemies()
        {
            return _spawnedEnemies ?? new GameObject[0];
        }

        /// <summary>
        /// 生成された神殿オブジェクトを取得
        /// </summary>
        public GameObject[] GetShrines()
        {
            return _spawnedShrines ?? new GameObject[0];
        }

        /// <summary>
        /// 現在のテンプレート設定を取得
        /// </summary>
        public ActionRPGTemplateConfig GetTemplateConfig()
        {
            return _templateConfig;
        }

        void OnDrawGizmosSelected()
        {
            // スポーン地点を視覚化
            if (_playerSpawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_playerSpawnPoint.position, 1f);
                Gizmos.DrawIcon(_playerSpawnPoint.position, "d_Avatar Icon");
            }

            if (_enemySpawnPoints != null)
            {
                Gizmos.color = Color.red;
                foreach (var spawnPoint in _enemySpawnPoints)
                {
                    if (spawnPoint != null)
                    {
                        Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                    }
                }
            }

            if (_shrineSpawnPoints != null)
            {
                Gizmos.color = Color.cyan;
                foreach (var spawnPoint in _shrineSpawnPoints)
                {
                    if (spawnPoint != null)
                    {
                        Gizmos.DrawWireCube(spawnPoint.position, Vector3.one);
                    }
                }
            }
        }
    }
}
