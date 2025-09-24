using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Components;
using asterivo.Unity60.Features.ActionRPG.Data;

namespace asterivo.Unity60.Features.ActionRPG.Components
{
    /// <summary>
    /// プレイヤーの戦闘システムを管理するコンポーネント
    /// 入力イベントを解釈してDamageCommandを生成・実行します
    /// </summary>
    public class PlayerCombatController : MonoBehaviour
    {
        [Header("戦闘設定")]
        [SerializeField] private float _attackRange = 2.0f;
        [SerializeField] private float _attackCooldown = 1.0f;
        [SerializeField] private LayerMask _enemyLayerMask = 1;

        [Header("攻撃データ")]
        [SerializeField] private float _lightAttackMultiplier = 1.0f;
        [SerializeField] private float _heavyAttackMultiplier = 1.5f;
        [SerializeField] private float _heavyAttackCooldown = 2.0f;

        [Header("イベント受信")]
        [SerializeField] private GameEvent _onAttackPressed;
        [SerializeField] private GameEvent _onHeavyAttackPressed;

        [Header("イベント発行")]
        [SerializeField] private GameEvent _onAttackExecuted;
        [SerializeField] private GameEvent _onAttackHit;
        [SerializeField] private GameEvent _onAttackMissed;

        [Header("エフェクト")]
        [SerializeField] private GameObject _attackEffectPrefab;
        [SerializeField] private AudioClip _attackSound;

        // コンポーネント参照
        private StatComponent _statComponent;
        private CommandInvoker _commandInvoker;
        private AudioSource _audioSource;

        // 内部状態
        private float _lastAttackTime;
        private bool _isAttacking;
        private Animator _animator;

        // アニメーションハッシュ
        private readonly int _attackTrigger = Animator.StringToHash("Attack");
        private readonly int _heavyAttackTrigger = Animator.StringToHash("HeavyAttack");
        private readonly int _isAttackingBool = Animator.StringToHash("IsAttacking");

        void Awake()
        {
            _statComponent = GetComponent<StatComponent>();
            _commandInvoker = GetComponent<CommandInvoker>();
            _audioSource = GetComponent<AudioSource>();
            _animator = GetComponent<Animator>();
        }

        void OnEnable()
        {
            // TODO: Fix GameEvent listener API
            // 入力イベントを受信
            // if (_onAttackPressed != null)
            //     _onAttackPressed.RegisterListener(this);
            //
            // if (_onHeavyAttackPressed != null)
            //     _onHeavyAttackPressed.RegisterListener(this);
        }

        void OnDisable()
        {
            // TODO: Fix GameEvent listener API
            // イベント受信解除
            // if (_onAttackPressed != null)
            //     _onAttackPressed.UnregisterListener(this);
            //
            // if (_onHeavyAttackPressed != null)
            //     _onHeavyAttackPressed.UnregisterListener(this);
        }

        /// <summary>
        /// 軽攻撃入力処理
        /// </summary>
        private void OnLightAttackPressed()
        {
            if (CanAttack())
            {
                ExecuteAttack(AttackType.Light);
            }
        }

        /// <summary>
        /// 重攻撃入力処理
        /// </summary>
        private void OnHeavyAttackPressed()
        {
            if (CanAttack(_heavyAttackCooldown))
            {
                ExecuteAttack(AttackType.Heavy);
            }
        }

        /// <summary>
        /// 攻撃可能かチェック
        /// </summary>
        private bool CanAttack(float cooldown = -1f)
        {
            if (_isAttacking) return false;
            
            float actualCooldown = cooldown >= 0 ? cooldown : _attackCooldown;
            return Time.time - _lastAttackTime >= actualCooldown;
        }

        /// <summary>
        /// 攻撃を実行
        /// </summary>
        private void ExecuteAttack(AttackType attackType)
        {
            _isAttacking = true;
            _lastAttackTime = Time.time;

            // アニメーション開始
            if (_animator != null)
            {
                _animator.SetBool(_isAttackingBool, true);
                
                if (attackType == AttackType.Light)
                    _animator.SetTrigger(_attackTrigger);
                else
                    _animator.SetTrigger(_heavyAttackTrigger);
            }

            // 攻撃処理を遅延実行（アニメーションと同期）
            float attackDelay = attackType == AttackType.Light ? 0.3f : 0.5f;
            Invoke(nameof(PerformAttackHitCheck), attackDelay);

            // 攻撃実行イベント発行
            if (_onAttackExecuted != null)
                _onAttackExecuted.Raise();
        }

        /// <summary>
        /// 攻撃ヒット判定を実行
        /// </summary>
        private void PerformAttackHitCheck()
        {
            // 前方の敵を検索
            Collider[] enemies = Physics.OverlapSphere(
                transform.position + transform.forward * (_attackRange * 0.5f),
                _attackRange * 0.5f,
                _enemyLayerMask
            );

            bool hitAnyEnemy = false;

            foreach (var enemy in enemies)
            {
                // 前方判定
                Vector3 directionToEnemy = (enemy.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, directionToEnemy);
                
                if (angle <= 60f) // 120度の攻撃範囲
                {
                    ApplyDamageToEnemy(enemy.gameObject);
                    hitAnyEnemy = true;
                }
            }

            // ヒット結果イベント発行
            if (hitAnyEnemy && _onAttackHit != null)
                _onAttackHit.Raise();
            else if (!hitAnyEnemy && _onAttackMissed != null)
                _onAttackMissed.Raise();

            // エフェクト・音効果
            PlayAttackEffects();

            // 攻撃終了
            _isAttacking = false;
            if (_animator != null)
                _animator.SetBool(_isAttackingBool, false);
        }

        /// <summary>
        /// 敵にダメージを適用
        /// </summary>
        private void ApplyDamageToEnemy(GameObject enemy)
        {
            if (_commandInvoker == null) return;

            // 敵のHealthComponentを取得
            var healthComponent = enemy.GetComponent<asterivo.Unity60.Core.Combat.HealthComponent>();
            if (healthComponent == null)
            {
                Debug.LogWarning($"{enemy.name}にHealthComponentが見つかりません。");
                return;
            }

            // ダメージ計算
            int baseDamage = CalculateAttackDamage();

            // DamageCommandを生成・実行（HealthComponentはIHealthTargetを実装）
            var damageCommand = new DamageCommand(healthComponent, baseDamage);
            _commandInvoker.ExecuteCommand(damageCommand);

            Debug.Log($"{enemy.name}に{baseDamage}のダメージを与えました。");
        }

        /// <summary>
        /// 攻撃力を計算
        /// </summary>
        private int CalculateAttackDamage()
        {
            int baseDamage = 10; // デフォルト攻撃力
            
            // StatComponentから攻撃力取得
            if (_statComponent != null)
            {
                baseDamage = _statComponent.AttackPower;
            }

            // TODO: 装備品からの攻撃力ボーナス（EquipmentManagerから取得）
            
            // ランダム要素追加
            float randomFactor = Random.Range(0.9f, 1.1f);
            return Mathf.RoundToInt(baseDamage * randomFactor);
        }

        /// <summary>
        /// 攻撃エフェクトとサウンドを再生
        /// </summary>
        private void PlayAttackEffects()
        {
            // 攻撃エフェクト生成
            if (_attackEffectPrefab != null)
            {
                Vector3 effectPosition = transform.position + transform.forward * _attackRange;
                GameObject effect = Instantiate(_attackEffectPrefab, effectPosition, transform.rotation);
                Destroy(effect, 2f);
            }

            // 攻撃音再生
            if (_audioSource != null && _attackSound != null)
            {
                _audioSource.PlayOneShot(_attackSound);
            }
        }

        /// <summary>
        /// デバッグ用：攻撃範囲を視覚化
        /// </summary>
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 attackCenter = transform.position + transform.forward * (_attackRange * 0.5f);
            Gizmos.DrawWireSphere(attackCenter, _attackRange * 0.5f);
            
            // 攻撃角度を表示
            Gizmos.color = Color.yellow;
            Vector3 leftBoundary = Quaternion.AngleAxis(-60f, Vector3.up) * transform.forward * _attackRange;
            Vector3 rightBoundary = Quaternion.AngleAxis(60f, Vector3.up) * transform.forward * _attackRange;
            
            Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
            Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        }

        /// <summary>
        /// 攻撃の種類
        /// </summary>
        private enum AttackType
        {
            Light,  // 軽攻撃
            Heavy   // 重攻撃
        }
    }
}