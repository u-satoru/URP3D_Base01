using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Features.Templates.FPS.Data;
using asterivo.Unity60.Player;

namespace asterivo.Unity60.Features.Templates.FPS.Combat
{
    /// <summary>
    /// FPS戦闘システム
    /// 詳細設計書3.4準拠：ダメージ処理とヒット判定を担当
    /// </summary>
    public class CombatSystem : MonoBehaviour
    {
        [Header("Combat Events")]
        [SerializeField] private GameEvent _onPlayerTookDamage;
        [SerializeField] private GameEvent _onPlayerDied;
        [SerializeField] private GameEvent _onEnemyTookDamage;
        [SerializeField] private GameEvent _onEnemyDied;

        [Header("Combat Settings")]
        [SerializeField] private LayerMask _combatLayerMask = -1;
        [SerializeField] private float _damageReductionCooldown = 0.1f; // 連続ダメージ防止
        [SerializeField] private bool _enableFriendlyFire = false;

        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogs = false;
        [SerializeField] private bool _showHitboxGizmos = false;

        // ダメージ履歴管理（連続ダメージ防止用）
        private Dictionary<int, float> _lastDamageTime = new Dictionary<int, float>();

        // 戦闘統計
        private int _totalDamageDealt = 0;
        private int _totalDamageReceived = 0;
        private int _enemiesKilled = 0;

        /// <summary>
        /// 総ダメージ与量
        /// </summary>
        public int TotalDamageDealt => _totalDamageDealt;

        /// <summary>
        /// 総ダメージ被量
        /// </summary>
        public int TotalDamageReceived => _totalDamageReceived;

        /// <summary>
        /// 撃破数
        /// </summary>
        public int EnemiesKilled => _enemiesKilled;

        #region Unity Lifecycle

        private void Awake()
        {
            // 統計リセット
            ResetCombatStats();
        }

        #endregion

        #region Combat Processing

        /// <summary>
        /// ヒット処理メイン関数
        /// 射撃システムから呼ばれるヒット判定と ダメージ計算の統合処理
        /// </summary>
        /// <param name="hitInfo">ヒット情報</param>
        /// <param name="weaponData">武器データ</param>
        /// <param name="shooter">射撃者</param>
        /// <returns>ダメージが適用されたかどうか</returns>
        public bool ProcessHit(RaycastHit hitInfo, WeaponData weaponData, GameObject shooter)
        {
            if (weaponData == null || shooter == null)
            {
                if (_enableDebugLogs)
                {
                    Debug.LogWarning("[CombatSystem] Invalid hit processing parameters");
                }
                return false;
            }

            // ヒットボックス検索
            HitboxComponent hitbox = hitInfo.collider.GetComponent<HitboxComponent>();
            if (hitbox == null)
            {
                // ヒットボックスがない場合は通常のコライダーヒット
                return ProcessNormalHit(hitInfo, weaponData, shooter);
            }

            // ヒットボックス付きヒット処理
            return ProcessHitboxHit(hitbox, hitInfo, weaponData, shooter);
        }

        /// <summary>
        /// ヒットボックス付きヒット処理
        /// 部位別ダメージ倍率を適用したダメージ計算
        /// </summary>
        private bool ProcessHitboxHit(HitboxComponent hitbox, RaycastHit hitInfo, WeaponData weaponData, GameObject shooter)
        {
            HealthComponent health = hitbox.GetHealthComponent();
            if (health == null)
            {
                if (_enableDebugLogs)
                {
                    Debug.LogWarning($"[CombatSystem] No HealthComponent found for hitbox: {hitbox.name}");
                }
                return false;
            }

            // フレンドリーファイア チェック
            if (!_enableFriendlyFire && IsFriendlyFire(shooter, health.gameObject))
            {
                if (_enableDebugLogs)
                {
                    Debug.Log("[CombatSystem] Friendly fire prevented");
                }
                return false;
            }

            // 連続ダメージ防止チェック
            if (IsRecentDamage(health.GetInstanceID()))
            {
                return false;
            }

            // ダメージ計算
            float baseDamage = weaponData.GetDamageAtDistance(Vector3.Distance(shooter.transform.position, hitInfo.point));
            float finalDamage = baseDamage * hitbox.DamageMultiplier;

            // ダメージ適用
            bool damageApplied = ApplyDamage(health, finalDamage, hitbox.HitboxType, shooter);

            if (damageApplied)
            {
                // ヒット効果の表示
                ShowHitEffect(hitInfo.point, hitInfo.normal, hitbox.HitboxType);

                // 統計更新
                UpdateCombatStats(Mathf.RoundToInt(finalDamage), true);

                // 連続ダメージ防止のタイムスタンプ更新
                _lastDamageTime[health.GetInstanceID()] = Time.time;

                if (_enableDebugLogs)
                {
                    Debug.Log($"[CombatSystem] Hitbox hit: {hitbox.HitboxType}, Damage: {finalDamage:F1}, Multiplier: {hitbox.DamageMultiplier}x");
                }
            }

            return damageApplied;
        }

        /// <summary>
        /// 通常のヒット処理（ヒットボックスなし）
        /// </summary>
        private bool ProcessNormalHit(RaycastHit hitInfo, WeaponData weaponData, GameObject shooter)
        {
            HealthComponent health = hitInfo.collider.GetComponent<HealthComponent>();
            if (health == null)
            {
                // HealthComponentがない場合は環境オブジェクト
                ProcessEnvironmentHit(hitInfo, weaponData);
                return false;
            }

            // フレンドリーファイア チェック
            if (!_enableFriendlyFire && IsFriendlyFire(shooter, health.gameObject))
            {
                return false;
            }

            // 連続ダメージ防止チェック
            if (IsRecentDamage(health.GetInstanceID()))
            {
                return false;
            }

            // ダメージ計算（等倍）
            float damage = weaponData.GetDamageAtDistance(Vector3.Distance(shooter.transform.position, hitInfo.point));

            // ダメージ適用
            bool damageApplied = ApplyDamage(health, damage, HitboxType.Chest, shooter);

            if (damageApplied)
            {
                ShowHitEffect(hitInfo.point, hitInfo.normal, HitboxType.Chest);
                UpdateCombatStats(Mathf.RoundToInt(damage), true);
                _lastDamageTime[health.GetInstanceID()] = Time.time;

                if (_enableDebugLogs)
                {
                    Debug.Log($"[CombatSystem] Normal hit: Damage: {damage:F1}");
                }
            }

            return damageApplied;
        }

        /// <summary>
        /// 環境オブジェクトヒット処理
        /// </summary>
        private void ProcessEnvironmentHit(RaycastHit hitInfo, WeaponData weaponData)
        {
            // 環境エフェクトの表示
            ShowEnvironmentHitEffect(hitInfo.point, hitInfo.normal, hitInfo.collider.material);

            if (_enableDebugLogs)
            {
                Debug.Log($"[CombatSystem] Environment hit: {hitInfo.collider.name}");
            }
        }

        /// <summary>
        /// ダメージ適用
        /// </summary>
        private bool ApplyDamage(HealthComponent health, float damage, HitboxType hitType, GameObject shooter)
        {
            if (health.IsDead)
            {
                return false;
            }

            int healthBefore = health.CurrentHealth;

            // DamageCommandを作成して実行
            var damageCommand = new DamageCommand
            {
                TargetHealth = health,
                DamageAmount = Mathf.RoundToInt(damage),
                DamageSource = shooter,
                HitType = hitType.ToString()
            };

            damageCommand.Execute();

            bool damageApplied = health.CurrentHealth < healthBefore;

            if (damageApplied)
            {
                // イベント発行
                if (health.CompareTag("Player"))
                {
                    _onPlayerTookDamage?.Raise();

                    if (health.IsDead)
                    {
                        _onPlayerDied?.Raise();
                    }
                }
                else
                {
                    _onEnemyTookDamage?.Raise();

                    if (health.IsDead)
                    {
                        _onEnemyDied?.Raise();
                        _enemiesKilled++;
                    }
                }
            }

            return damageApplied;
        }

        #endregion

        #region Hit Effects

        /// <summary>
        /// ヒット効果の表示
        /// </summary>
        private void ShowHitEffect(Vector3 hitPoint, Vector3 hitNormal, HitboxType hitType)
        {
            // 部位別エフェクトの選択
            GameObject effectPrefab = GetHitEffectPrefab(hitType);

            if (effectPrefab != null)
            {
                GameObject effect = Instantiate(effectPrefab, hitPoint, Quaternion.LookRotation(hitNormal));
                Destroy(effect, 2.0f); // 2秒後に削除
            }

            // パーティクル血痕エフェクト（ヘッドショット時は特別）
            if (hitType == HitboxType.Head)
            {
                // ヘッドショット専用エフェクト
                ShowHeadshotEffect(hitPoint, hitNormal);
            }
        }

        /// <summary>
        /// 環境ヒット効果の表示
        /// </summary>
        private void ShowEnvironmentHitEffect(Vector3 hitPoint, Vector3 hitNormal, PhysicsMaterial material)
        {
            // マテリアル別エフェクト
            GameObject effectPrefab = GetEnvironmentEffectPrefab(material);

            if (effectPrefab != null)
            {
                GameObject effect = Instantiate(effectPrefab, hitPoint, Quaternion.LookRotation(hitNormal));
                Destroy(effect, 3.0f);
            }
        }

        /// <summary>
        /// ヘッドショット特別エフェクト
        /// </summary>
        private void ShowHeadshotEffect(Vector3 hitPoint, Vector3 hitNormal)
        {
            // ヘッドショット専用パーティクル・UI表示
            if (_enableDebugLogs)
            {
                Debug.Log("[CombatSystem] HEADSHOT!");
            }
        }

        /// <summary>
        /// ヒットタイプ別エフェクト取得
        /// </summary>
        private GameObject GetHitEffectPrefab(HitboxType hitType)
        {
            // 実際の実装では Resources や AddressableAssets から取得
            return null; // プレフィックス実装時に差し替え
        }

        /// <summary>
        /// 環境マテリアル別エフェクト取得
        /// </summary>
        private GameObject GetEnvironmentEffectPrefab(PhysicsMaterial material)
        {
            // マテリアル名に基づいてエフェクトを選択
            return null; // プレフィックス実装時に差し替え
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// フレンドリーファイア判定
        /// </summary>
        private bool IsFriendlyFire(GameObject shooter, GameObject target)
        {
            // タグベースの判定
            if (shooter.CompareTag("Player") && target.CompareTag("Player"))
            {
                return true;
            }

            if (shooter.CompareTag("Enemy") && target.CompareTag("Enemy"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 連続ダメージ防止チェック
        /// </summary>
        private bool IsRecentDamage(int targetInstanceId)
        {
            if (!_lastDamageTime.ContainsKey(targetInstanceId))
            {
                return false;
            }

            return Time.time - _lastDamageTime[targetInstanceId] < _damageReductionCooldown;
        }

        /// <summary>
        /// 戦闘統計の更新
        /// </summary>
        private void UpdateCombatStats(int damage, bool isDealingDamage)
        {
            if (isDealingDamage)
            {
                _totalDamageDealt += damage;
            }
            else
            {
                _totalDamageReceived += damage;
            }
        }

        /// <summary>
        /// 戦闘統計のリセット
        /// </summary>
        public void ResetCombatStats()
        {
            _totalDamageDealt = 0;
            _totalDamageReceived = 0;
            _enemiesKilled = 0;
            _lastDamageTime.Clear();

            if (_enableDebugLogs)
            {
                Debug.Log("[CombatSystem] Combat stats reset");
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// プレイヤーダメージ処理（外部から呼び出し用）
        /// </summary>
        public void DamagePlayer(float damage, GameObject source = null)
        {
            HealthComponent playerHealth = GameObject.FindGameObjectWithTag("Player")?.GetComponent<HealthComponent>();
            if (playerHealth != null)
            {
                ApplyDamage(playerHealth, damage, HitboxType.Chest, source);
                UpdateCombatStats(Mathf.RoundToInt(damage), false);
            }
        }

        /// <summary>
        /// 戦闘統計の取得
        /// </summary>
        public CombatStats GetCombatStats()
        {
            return new CombatStats
            {
                TotalDamageDealt = _totalDamageDealt,
                TotalDamageReceived = _totalDamageReceived,
                EnemiesKilled = _enemiesKilled,
                Accuracy = CalculateAccuracy(),
                KillDeathRatio = CalculateKDRatio()
            };
        }

        /// <summary>
        /// 精度計算
        /// </summary>
        private float CalculateAccuracy()
        {
            // 実装時に射撃数をトラッキングして計算
            return 0.0f;
        }

        /// <summary>
        /// K/D比計算
        /// </summary>
        private float CalculateKDRatio()
        {
            // 死亡数をトラッキングして計算（現在は仮実装）
            return _enemiesKilled;
        }

        #endregion

        #region Debug

        /// <summary>
        /// デバッグ情報の取得
        /// </summary>
        public string GetDebugInfo()
        {
            return $"CombatSystem - Dealt: {_totalDamageDealt}, Received: {_totalDamageReceived}, Kills: {_enemiesKilled}";
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_showHitboxGizmos) return;

            // ヒットボックスの可視化
            HitboxComponent[] hitboxes = FindObjectsOfType<HitboxComponent>();
            foreach (var hitbox in hitboxes)
            {
                Gizmos.color = GetHitboxColor(hitbox.HitboxType);
                Gizmos.DrawWireCube(hitbox.transform.position, hitbox.transform.lossyScale);
            }
        }

        private Color GetHitboxColor(HitboxType type)
        {
            return type switch
            {
                HitboxType.Head => Color.red,
                HitboxType.Chest => Color.yellow,
                HitboxType.Arms => Color.blue,
                HitboxType.Legs => Color.green,
                _ => Color.white
            };
        }
#endif

        #endregion
    }

    /// <summary>
    /// 戦闘統計データ
    /// </summary>
    [System.Serializable]
    public struct CombatStats
    {
        public int TotalDamageDealt;
        public int TotalDamageReceived;
        public int EnemiesKilled;
        public float Accuracy;
        public float KillDeathRatio;
    }
}
