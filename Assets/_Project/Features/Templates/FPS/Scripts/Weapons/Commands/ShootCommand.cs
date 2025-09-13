using UnityEngine;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Features.Templates.FPS.Weapons.Commands
{
    /// <summary>
    /// FPS射撃コマンド - レイキャストによる即座ヒット射撃を実装
    /// Command Patternに従い、射撃動作をカプセル化
    /// ObjectPool対応のためIResettableCommandを実装
    /// </summary>
    public class ShootCommand : IResettableCommand
    {
        private Vector3 _origin;
        private Vector3 _direction;
        private WeaponData _weaponData;
        private IHealthTarget _hitTarget;
        private float _damageDealt;
        private Vector3 _hitPoint;
        private bool _wasHit;
        
        public bool CanUndo => false; // 射撃は巻き戻し不可
        
        public ShootCommand()
        {
            // プール化対応：パラメーターなしコンストラクタ
        }
        
        public ShootCommand(Vector3 origin, Vector3 direction, WeaponData weaponData)
        {
            Initialize(origin, direction, weaponData);
        }
        
        public void Execute()
        {
            if (_weaponData == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning("ShootCommand: WeaponData is null, cannot execute shoot");
#endif
                return;
            }
            
            // レイキャスト射撃実行
            PerformRaycastShoot();
            
            // ヒットした場合はダメージ処理
            if (_wasHit && _hitTarget != null)
            {
                ApplyDamage();
            }
            
            // エフェクト再生
            PlayShootEffects();
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"Fired {_weaponData.WeaponName} - Hit: {_wasHit}, Damage: {_damageDealt}");
#endif
        }
        
        public void Undo()
        {
            // 射撃は巻き戻し不可
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning("ShootCommand: Undo is not supported for shoot commands");
#endif
        }
        
        public void Reset()
        {
            _origin = Vector3.zero;
            _direction = Vector3.forward;
            _weaponData = null;
            _hitTarget = null;
            _damageDealt = 0f;
            _hitPoint = Vector3.zero;
            _wasHit = false;
        }
        
        public void Initialize(params object[] parameters)
        {
            if (parameters.Length >= 3)
            {
                _origin = (Vector3)parameters[0];
                _direction = (Vector3)parameters[1];
                _weaponData = (WeaponData)parameters[2];
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
            {
                Debug.LogError("ShootCommand: Invalid parameters count. Expected: origin, direction, weaponData");
            }
#endif
        }
        
        private void PerformRaycastShoot()
        {
            // 射撃精度に基づく方向のずれを計算
            Vector3 shootDirection = ApplyAccuracySpread(_direction);
            
            // レイキャスト実行
            if (Physics.Raycast(_origin, shootDirection, out RaycastHit hit, _weaponData.MaxRange))
            {
                _wasHit = true;
                _hitPoint = hit.point;
                _hitTarget = hit.collider.GetComponent<IHealthTarget>();
                
                // ヘッドショット判定（"Head"タグまたは特定のコライダー）
                bool isHeadshot = IsHeadshot(hit.collider);
                _damageDealt = isHeadshot ? 
                    _weaponData.BaseDamage * _weaponData.HeadshotMultiplier : 
                    _weaponData.BaseDamage;
                
                // 距離による威力減衰
                float distance = Vector3.Distance(_origin, _hitPoint);
                _damageDealt = ApplyDistanceFalloff(_damageDealt, distance);
            }
            else
            {
                _wasHit = false;
                _hitPoint = _origin + shootDirection * _weaponData.MaxRange;
            }
            
            // デバッグ用の射撃ライン描画
#if UNITY_EDITOR
            Debug.DrawLine(_origin, _hitPoint, Color.red, 0.1f);
#endif
        }
        
        private Vector3 ApplyAccuracySpread(Vector3 baseDirection)
        {
            // 武器の精度に基づいてランダムなスプレッドを適用
            float spreadAngle = _weaponData.BaseAccuracy * 180f; // 度数法に変換
            
            // ランダムな角度を生成
            float randomX = Random.Range(-spreadAngle, spreadAngle);
            float randomY = Random.Range(-spreadAngle, spreadAngle);
            
            // 基準方向からの回転を適用
            Quaternion spread = Quaternion.Euler(randomX, randomY, 0);
            return spread * baseDirection;
        }
        
        private bool IsHeadshot(Collider hitCollider)
        {
            return hitCollider.CompareTag("Head") || 
                   hitCollider.name.ToLower().Contains("head");
        }
        
        private float ApplyDistanceFalloff(float baseDamage, float distance)
        {
            // 有効射程内では100%威力、最大射程で50%威力
            if (distance <= _weaponData.EffectiveRange)
            {
                return baseDamage;
            }
            
            float falloffRange = _weaponData.MaxRange - _weaponData.EffectiveRange;
            float falloffRatio = (distance - _weaponData.EffectiveRange) / falloffRange;
            float damageMultiplier = Mathf.Lerp(1f, 0.5f, falloffRatio);
            
            return baseDamage * damageMultiplier;
        }
        
        private void ApplyDamage()
        {
            if (_hitTarget != null && _damageDealt > 0)
            {
                _hitTarget.TakeDamage(Mathf.RoundToInt(_damageDealt), "ballistic");
            }
        }
        
        private void PlayShootEffects()
        {
            // エフェクト再生はWeaponSystemが担当
            // ここでは射撃データの記録のみ
        }
    }
}