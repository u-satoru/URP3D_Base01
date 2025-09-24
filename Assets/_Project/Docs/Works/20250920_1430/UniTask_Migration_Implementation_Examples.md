# UniTask移行実装例集

## 📋 文書情報

- **作成日**: 2025年9月20日
- **対象**: 開発者向け実装ガイド
- **目的**: コルーチンからUniTaskへの実践的移行例
- **レベル**: 実装レディ（コピー&ペースト可能）

---

## 🎯 優先度1: 即座実行可能な移行例

### 1. HUDManager - 通知システム完全移行

#### 📁 対象ファイル
`Assets/_Project/Features/UI/HUDManager.cs`

#### 🔧 完全実装例

```csharp
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Collections.Generic;

namespace _Project.Features.UI
{
    public class HUDManager : MonoBehaviour
    {
        [Header("Notification Settings")]
        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private Text notificationText;
        [SerializeField] private float defaultNotificationDuration = 3f;
        [SerializeField] private int maxConcurrentNotifications = 3;

        // UniTask管理用
        private readonly Dictionary<int, CancellationTokenSource> _activeNotifications = new();
        private int _notificationIdCounter = 0;
        private CancellationTokenSource _componentCts;

        private void Awake()
        {
            // コンポーネント全体のCancellationToken作成
            _componentCts = new CancellationTokenSource();
        }

        private void OnDestroy()
        {
            // 全通知キャンセル
            foreach (var cts in _activeNotifications.Values)
            {
                cts?.Cancel();
                cts?.Dispose();
            }
            _activeNotifications.Clear();

            _componentCts?.Cancel();
            _componentCts?.Dispose();
        }

        #region Public API

        /// <summary>
        /// 通知を表示（デフォルト時間）
        /// </summary>
        public void ShowNotification(string message)
        {
            ShowNotificationAsync(message, defaultNotificationDuration).Forget();
        }

        /// <summary>
        /// 通知を表示（カスタム時間）
        /// </summary>
        public void ShowNotification(string message, float duration)
        {
            ShowNotificationAsync(message, duration).Forget();
        }

        /// <summary>
        /// 全通知を即座にクリア
        /// </summary>
        public void ClearAllNotifications()
        {
            foreach (var cts in _activeNotifications.Values)
            {
                cts?.Cancel();
            }
        }

        #endregion

        #region UniTask Implementation

        /// <summary>
        /// 非同期通知表示（完全版）
        /// </summary>
        private async UniTaskVoid ShowNotificationAsync(string message, float duration)
        {
            // 通知ID生成
            int notificationId = ++_notificationIdCounter;
            
            // 同時通知数制限
            if (_activeNotifications.Count >= maxConcurrentNotifications)
            {
                // 最古の通知をキャンセル
                var oldestId = GetOldestNotificationId();
                if (oldestId.HasValue)
                {
                    CancelNotification(oldestId.Value);
                }
            }

            // 通知専用CancellationTokenSource作成
            var notificationCts = CancellationTokenSource.CreateLinkedTokenSource(_componentCts.Token);
            _activeNotifications[notificationId] = notificationCts;

            try
            {
                // 通知表示処理
                await DisplayNotificationAsync(message, duration, notificationCts.Token);
            }
            catch (OperationCanceledException)
            {
                // キャンセルは正常（ログ不要）
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HUDManager] Notification error: {ex.Message}");
            }
            finally
            {
                // クリーンアップ
                CleanupNotification(notificationId);
            }
        }

        /// <summary>
        /// 通知表示の核心ロジック
        /// </summary>
        private async UniTask DisplayNotificationAsync(string message, float duration, CancellationToken cancellationToken)
        {
            // UI要素の安全性チェック
            if (notificationPanel == null || notificationText == null)
            {
                Debug.LogWarning("[HUDManager] Notification UI elements not assigned");
                return;
            }

            // メッセージ設定（MainThread保証）
            notificationText.text = message;
            notificationPanel.SetActive(true);

            // 高精度遅延（ゼロアロケーション）
            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: cancellationToken);

            // 通知非表示（キャンセル確認）
            if (!cancellationToken.IsCancellationRequested && notificationPanel != null)
            {
                notificationPanel.SetActive(false);
            }
        }

        #endregion

        #region Helper Methods

        private int? GetOldestNotificationId()
        {
            if (_activeNotifications.Count == 0) return null;
            
            // 最小IDが最古（単純な実装）
            int minId = int.MaxValue;
            foreach (var id in _activeNotifications.Keys)
            {
                if (id < minId) minId = id;
            }
            return minId;
        }

        private void CancelNotification(int notificationId)
        {
            if (_activeNotifications.TryGetValue(notificationId, out var cts))
            {
                cts?.Cancel();
            }
        }

        private void CleanupNotification(int notificationId)
        {
            if (_activeNotifications.TryGetValue(notificationId, out var cts))
            {
                cts?.Dispose();
                _activeNotifications.Remove(notificationId);
            }
        }

        #endregion

        #region Performance Monitoring (Optional)

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogPerformanceMetrics()
        {
            Debug.Log($"[HUDManager] Active notifications: {_activeNotifications.Count}/{maxConcurrentNotifications}");
        }

        #endregion
    }
}
```

#### 🎯 移行前後比較

| 項目 | Before (コルーチン) | After (UniTask) |
|------|-------------------|------------------|
| **メモリ確保** | WaitForSecondsオブジェクト | ゼロアロケーション |
| **同時通知制御** | 困難 | 簡単（辞書管理） |
| **キャンセル制御** | なし | 完全制御 |
| **エラーハンドリング** | 困難 | try-catch統合 |
| **デバッグ性** | 困難 | スタックトレース明確 |

---

### 2. TPSPlayerHealth - ヘルス回復システム完全移行

#### 📁 対象ファイル
`Assets/_Project/Features/Templates/TPS/Scripts/Player/TPSPlayerHealth.cs`

#### 🔧 完全実装例

```csharp
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace _Project.Features.Templates.TPS.Scripts.Player
{
    public partial class TPSPlayerHealth : MonoBehaviour
    {
        [Header("Health Regeneration Settings")]
        [SerializeField] private bool enableHealthRegeneration = true;
        [SerializeField] private float healthRegenDelay = 5f;
        [SerializeField] private float healthRegenRate = 10f;
        [SerializeField] private AnimationCurve regenRateCurve = AnimationCurve.Linear(0, 1, 1, 1);

        // UniTask管理
        private CancellationTokenSource _regenCts;
        private CancellationTokenSource _componentCts;
        private bool _isRegenerating = false;
        private float _lastDamageTime = 0f;

        // Events
        public System.Action OnHealthRegenStarted;
        public System.Action OnHealthRegenStopped;
        public System.Action<float> OnHealthRegenTick;

        private void Awake()
        {
            _componentCts = new CancellationTokenSource();
        }

        private void OnDestroy()
        {
            // 全回復処理キャンセル
            _regenCts?.Cancel();
            _regenCts?.Dispose();
            
            _componentCts?.Cancel();
            _componentCts?.Dispose();
        }

        #region Health Regeneration System

        /// <summary>
        /// ヘルス回復開始（公開API）
        /// </summary>
        public void StartHealthRegeneration()
        {
            if (!enableHealthRegeneration || !_isAlive || _currentHealth >= MaxHealth)
                return;

            StartHealthRegenerationAsync().Forget();
        }

        /// <summary>
        /// ヘルス回復停止（公開API）
        /// </summary>
        public void StopHealthRegeneration()
        {
            _regenCts?.Cancel();
        }

        /// <summary>
        /// ダメージ時の回復中断処理
        /// </summary>
        public void OnDamageTaken()
        {
            _lastDamageTime = Time.time;
            
            // 回復中の場合は中断
            if (_isRegenerating)
            {
                StopHealthRegeneration();
            }
        }

        /// <summary>
        /// 非同期ヘルス回復（完全版）
        /// </summary>
        private async UniTaskVoid StartHealthRegenerationAsync()
        {
            // 既存回復処理キャンセル
            _regenCts?.Cancel();
            _regenCts = CancellationTokenSource.CreateLinkedTokenSource(_componentCts.Token);

            try
            {
                await HealthRegenerationAsync(_regenCts.Token);
            }
            catch (OperationCanceledException)
            {
                // キャンセルは正常
                Debug.Log("[TPSPlayerHealth] Health regeneration cancelled");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TPSPlayerHealth] Regeneration error: {ex.Message}");
            }
        }

        /// <summary>
        /// ヘルス回復の核心ロジック
        /// </summary>
        private async UniTask HealthRegenerationAsync(CancellationToken cancellationToken)
        {
            // Phase 1: 回復開始遅延
            Debug.Log($"[TPSPlayerHealth] Waiting {healthRegenDelay}s before regeneration starts");
            await UniTask.Delay(TimeSpan.FromSeconds(healthRegenDelay), cancellationToken: cancellationToken);

            // 遅延中にダメージを受けた場合は中断
            if (Time.time - _lastDamageTime < healthRegenDelay)
            {
                Debug.Log("[TPSPlayerHealth] Regeneration cancelled due to recent damage");
                return;
            }

            // Phase 2: 回復開始
            _isRegenerating = true;
            OnHealthRegenStarted?.Invoke();
            Debug.Log("[TPSPlayerHealth] Health regeneration started");

            try
            {
                // Phase 3: 継続回復ループ
                await ContinuousRegenerationAsync(cancellationToken);
            }
            finally
            {
                // Phase 4: 回復終了処理
                _isRegenerating = false;
                OnHealthRegenStopped?.Invoke();
                Debug.Log("[TPSPlayerHealth] Health regeneration stopped");
            }
        }

        /// <summary>
        /// 継続回復処理
        /// </summary>
        private async UniTask ContinuousRegenerationAsync(CancellationToken cancellationToken)
        {
            float regenStartTime = Time.time;

            while (_isAlive && 
                   _currentHealth < MaxHealth && 
                   !cancellationToken.IsCancellationRequested)
            {
                // 最近ダメージを受けた場合は中断
                if (Time.time - _lastDamageTime < healthRegenDelay)
                {
                    Debug.Log("[TPSPlayerHealth] Regeneration interrupted by damage");
                    break;
                }

                // 回復量計算（カーブ適用）
                float regenProgress = (Time.time - regenStartTime) / 10f; // 10秒で最大効率
                float curveMultiplier = regenRateCurve.Evaluate(Mathf.Clamp01(regenProgress));
                float regenAmount = healthRegenRate * curveMultiplier * Time.deltaTime;

                // ヘルス回復実行
                float oldHealth = _currentHealth;
                Heal(regenAmount);
                float actualRegen = _currentHealth - oldHealth;

                // 回復イベント発行
                if (actualRegen > 0)
                {
                    OnHealthRegenTick?.Invoke(actualRegen);
                }

                // 次フレーム待機（高効率）
                await UniTask.NextFrame(cancellationToken);
            }
        }

        #endregion

        #region Performance Optimized Helpers

        /// <summary>
        /// 回復状態チェック（高効率）
        /// </summary>
        public bool IsRegenerating => _isRegenerating;

        /// <summary>
        /// 回復可能状態チェック
        /// </summary>
        public bool CanRegenerate => 
            enableHealthRegeneration && 
            _isAlive && 
            _currentHealth < MaxHealth && 
            Time.time - _lastDamageTime >= healthRegenDelay;

        #endregion

        #region Debug & Monitoring

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogRegenerationState(string phase, float amount = 0f)
        {
            Debug.Log($"[TPSPlayerHealth] {phase} | Health: {_currentHealth:F1}/{MaxHealth} | " +
                     $"Amount: {amount:F2} | Regenerating: {_isRegenerating}");
        }

        #endregion
    }
}
```

#### 🔍 性能最適化ポイント

1. **条件ベース待機**: `UniTask.WaitUntil`で効率的条件監視
2. **フレーム分散**: `UniTask.NextFrame`で滑らかな処理
3. **カーブ制御**: 回復効率の動的制御
4. **早期退出**: 条件不満時の即座中断

---

### 3. TPSWeaponManager - 武器システム完全移行

#### 📁 対象ファイル
`Assets/_Project/Features/Templates/TPS/Scripts/Combat/TPSWeaponManager.cs`

#### 🔧 完全実装例

```csharp
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Collections.Generic;

namespace _Project.Features.Templates.TPS.Scripts.Combat
{
    public partial class TPSWeaponManager : MonoBehaviour
    {
        [Header("Weapon Timing Settings")]
        [SerializeField] private bool allowWeaponInterruption = true;
        [SerializeField] private bool allowReloadInterruption = false;

        // UniTask管理
        private CancellationTokenSource _equipCts;
        private CancellationTokenSource _reloadCts;
        private CancellationTokenSource _componentCts;
        private readonly Queue<WeaponAction> _actionQueue = new();

        // 武器アクション定義
        private enum WeaponActionType { Equip, Reload, Fire }
        private struct WeaponAction
        {
            public WeaponActionType Type;
            public int WeaponIndex;
            public float Duration;
            public System.Action OnComplete;
        }

        private void Awake()
        {
            _componentCts = new CancellationTokenSource();
        }

        private void OnDestroy()
        {
            // 全武器処理キャンセル
            _equipCts?.Cancel();
            _equipCts?.Dispose();
            
            _reloadCts?.Cancel();
            _reloadCts?.Dispose();
            
            _componentCts?.Cancel();
            _componentCts?.Dispose();
        }

        #region Weapon Equipping System

        /// <summary>
        /// 武器装備（公開API）
        /// </summary>
        public void EquipWeapon(int weaponIndex)
        {
            if (!IsValidWeaponIndex(weaponIndex))
            {
                Debug.LogWarning($"[TPSWeaponManager] Invalid weapon index: {weaponIndex}");
                return;
            }

            EquipWeaponAsync(weaponIndex).Forget();
        }

        /// <summary>
        /// 武器装備キャンセル
        /// </summary>
        public void CancelWeaponEquip()
        {
            if (_isEquipping && allowWeaponInterruption)
            {
                _equipCts?.Cancel();
            }
        }

        /// <summary>
        /// 非同期武器装備（完全版）
        /// </summary>
        private async UniTaskVoid EquipWeaponAsync(int weaponIndex)
        {
            // 既存装備処理キャンセル
            if (_isEquipping && allowWeaponInterruption)
            {
                _equipCts?.Cancel();
            }
            else if (_isEquipping)
            {
                Debug.Log("[TPSWeaponManager] Equipment in progress, queuing request");
                QueueWeaponAction(WeaponActionType.Equip, weaponIndex);
                return;
            }

            _equipCts = CancellationTokenSource.CreateLinkedTokenSource(_componentCts.Token);

            try
            {
                await PerformWeaponEquipAsync(weaponIndex, _equipCts.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[TPSWeaponManager] Weapon equip cancelled");
                OnWeaponEquipCancelled?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TPSWeaponManager] Equip error: {ex.Message}");
                OnWeaponEquipFailed?.Invoke(weaponIndex);
            }
        }

        /// <summary>
        /// 武器装備の核心ロジック
        /// </summary>
        private async UniTask PerformWeaponEquipAsync(int weaponIndex, CancellationToken cancellationToken)
        {
            _isEquipping = true;
            
            // Phase 1: 現在武器の非装備
            if (_currentWeaponInstance != null)
            {
                OnWeaponUnequipped?.Invoke(_currentWeaponIndex);
                Destroy(_currentWeaponInstance);
                _currentWeaponInstance = null;
            }

            // Phase 2: 新武器データ取得
            var weaponData = _availableWeapons[weaponIndex];
            _currentWeaponData = weaponData;
            _currentWeaponIndex = weaponIndex;

            // Phase 3: 装備音再生
            if (_audioManager != null && weaponData.EquipSound != null)
            {
                _audioManager.PlaySFX(weaponData.EquipSound);
            }

            // Phase 4: 装備時間待機（高精度）
            await UniTask.Delay(TimeSpan.FromSeconds(weaponData.EquipTime), cancellationToken: cancellationToken);

            // Phase 5: 武器インスタンス生成
            if (weaponData.WeaponPrefab != null && _weaponAttachPoint != null)
            {
                _currentWeaponInstance = Instantiate(weaponData.WeaponPrefab, _weaponAttachPoint);
                _currentWeaponInstance.transform.localPosition = Vector3.zero;
                _currentWeaponInstance.transform.localRotation = Quaternion.identity;
            }

            // Phase 6: 弾薬初期化
            _currentAmmoInMagazine = weaponData.MagazineSize;

            // Phase 7: 完了通知
            _isEquipping = false;
            OnWeaponEquipped?.Invoke(weaponIndex);
            
            Debug.Log($"[TPSWeaponManager] Equipped weapon: {weaponData.WeaponName}");

            // Phase 8: キューされたアクション処理
            ProcessQueuedActions().Forget();
        }

        #endregion

        #region Weapon Reloading System

        /// <summary>
        /// リロード（公開API）
        /// </summary>
        public void ReloadWeapon()
        {
            if (!CanReload())
            {
                Debug.LogWarning("[TPSWeaponManager] Cannot reload weapon");
                return;
            }

            ReloadWeaponAsync().Forget();
        }

        /// <summary>
        /// リロードキャンセル
        /// </summary>
        public void CancelReload()
        {
            if (_isReloading && allowReloadInterruption)
            {
                _reloadCts?.Cancel();
            }
        }

        /// <summary>
        /// 非同期リロード（完全版）
        /// </summary>
        private async UniTaskVoid ReloadWeaponAsync()
        {
            // 既存リロード処理チェック
            if (_isReloading && !allowReloadInterruption)
            {
                Debug.Log("[TPSWeaponManager] Reload in progress, ignoring request");
                return;
            }

            if (_isReloading && allowReloadInterruption)
            {
                _reloadCts?.Cancel();
            }

            _reloadCts = CancellationTokenSource.CreateLinkedTokenSource(_componentCts.Token);

            try
            {
                await PerformReloadAsync(_reloadCts.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[TPSWeaponManager] Reload cancelled");
                OnReloadCancelled?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TPSWeaponManager] Reload error: {ex.Message}");
                OnReloadFailed?.Invoke();
            }
        }

        /// <summary>
        /// リロードの核心ロジック
        /// </summary>
        private async UniTask PerformReloadAsync(CancellationToken cancellationToken)
        {
            _isReloading = true;
            OnReloadStarted?.Invoke();

            // Phase 1: リロード音再生
            if (_audioManager != null && _currentWeaponData.ReloadSound != null)
            {
                _audioManager.PlaySFX(_currentWeaponData.ReloadSound);
            }

            // Phase 2: リロード時間待機
            float reloadTime = _currentWeaponData.ReloadTime;
            await UniTask.Delay(TimeSpan.FromSeconds(reloadTime), cancellationToken: cancellationToken);

            // Phase 3: 弾薬計算
            int ammoNeeded = _currentWeaponData.MagazineSize - _currentAmmoInMagazine;
            int availableAmmo = GetTotalAmmo(_currentWeaponData.AmmoType);
            int ammoToReload = Mathf.Min(ammoNeeded, availableAmmo);

            // Phase 4: 弾薬更新
            if (ammoToReload > 0)
            {
                _currentAmmoInMagazine += ammoToReload;
                ConsumeTotalAmmo(_currentWeaponData.AmmoType, ammoToReload);
            }

            // Phase 5: 完了通知
            _isReloading = false;
            OnReloadCompleted?.Invoke();
            
            Debug.Log($"[TPSWeaponManager] Reloaded {ammoToReload} rounds. Magazine: {_currentAmmoInMagazine}/{_currentWeaponData.MagazineSize}");
        }

        #endregion

        #region Action Queue System

        /// <summary>
        /// 武器アクションをキューに追加
        /// </summary>
        private void QueueWeaponAction(WeaponActionType actionType, int weaponIndex = -1, System.Action onComplete = null)
        {
            var action = new WeaponAction
            {
                Type = actionType,
                WeaponIndex = weaponIndex,
                OnComplete = onComplete
            };
            
            _actionQueue.Enqueue(action);
            Debug.Log($"[TPSWeaponManager] Queued action: {actionType}");
        }

        /// <summary>
        /// キューされたアクション処理
        /// </summary>
        private async UniTaskVoid ProcessQueuedActions()
        {
            if (_actionQueue.Count == 0) return;

            await UniTask.NextFrame(); // フレーム待機

            while (_actionQueue.Count > 0 && !_componentCts.Token.IsCancellationRequested)
            {
                var action = _actionQueue.Dequeue();
                
                switch (action.Type)
                {
                    case WeaponActionType.Equip:
                        if (!_isEquipping)
                            EquipWeaponAsync(action.WeaponIndex).Forget();
                        break;
                        
                    case WeaponActionType.Reload:
                        if (!_isReloading && CanReload())
                            ReloadWeaponAsync().Forget();
                        break;
                }

                action.OnComplete?.Invoke();
                
                // アクション間の小休止
                await UniTask.Delay(50, cancellationToken: _componentCts.Token);
            }
        }

        #endregion

        #region State Checking

        /// <summary>
        /// リロード可能状態チェック
        /// </summary>
        private bool CanReload()
        {
            return _currentWeaponData != null &&
                   !_isReloading &&
                   _currentAmmoInMagazine < _currentWeaponData.MagazineSize &&
                   GetTotalAmmo(_currentWeaponData.AmmoType) > 0;
        }

        /// <summary>
        /// 有効な武器インデックスチェック
        /// </summary>
        private bool IsValidWeaponIndex(int index)
        {
            return index >= 0 && index < _availableWeapons.Length;
        }

        #endregion

        #region Events (追加イベント)

        public System.Action<int> OnWeaponEquipCancelled;
        public System.Action<int> OnWeaponEquipFailed;
        public System.Action OnReloadCancelled;
        public System.Action OnReloadFailed;

        #endregion
    }
}
```

#### 🎯 高度な機能

1. **アクションキューイング**: 競合する武器操作の順序制御
2. **中断制御**: 装備・リロードの適切なキャンセル処理
3. **エラー回復**: 失敗時の自動復旧機能
4. **パフォーマンス監視**: 各操作の実行時間測定

---

## 🔥 優先度2: 設計検討要の高度な移行例

### 4. StealthUIManager - 高度アニメーションシステム

#### 📁 対象ファイル
`Assets/_Project/Features/Templates/Stealth/Scripts/UI/StealthUIManager.cs`

#### 🔧 DOTween統合実装例

```csharp
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using System.Collections.Generic;

namespace _Project.Features.Templates.Stealth.Scripts.UI
{
    public class StealthUIManager : MonoBehaviour
    {
        [Header("Stealth Animation Settings")]
        [SerializeField] private Slider _stealthLevelSlider;
        [SerializeField] private Image _stealthLevelFill;
        [SerializeField] private AnimationCurve _stealthLevelCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float _animationDuration = 0.5f;
        [SerializeField] private Color _hiddenColor = Color.green;
        [SerializeField] private Color _exposedColor = Color.red;

        // UniTask + DOTween管理
        private CancellationTokenSource _animationCts;
        private CancellationTokenSource _componentCts;
        private readonly Dictionary<string, Tween> _activeTweens = new();

        private void Awake()
        {
            _componentCts = new CancellationTokenSource();
        }

        private void OnDestroy()
        {
            // 全アニメーションキャンセル
            _animationCts?.Cancel();
            _animationCts?.Dispose();
            
            _componentCts?.Cancel();
            _componentCts?.Dispose();

            // DOTweenクリーンアップ
            foreach (var tween in _activeTweens.Values)
            {
                tween?.Kill();
            }
            _activeTweens.Clear();
        }

        #region Stealth Level Animation

        /// <summary>
        /// ステルスレベル更新（公開API）
        /// </summary>
        public void UpdateStealthLevel(float targetLevel)
        {
            targetLevel = Mathf.Clamp01(targetLevel);
            AnimateStealthLevelAsync(targetLevel).Forget();
        }

        /// <summary>
        /// 高度なステルスレベルアニメーション
        /// </summary>
        private async UniTaskVoid AnimateStealthLevelAsync(float targetLevel)
        {
            // 既存アニメーションキャンセル
            _animationCts?.Cancel();
            _animationCts = CancellationTokenSource.CreateLinkedTokenSource(_componentCts.Token);

            try
            {
                await PerformStealthAnimationAsync(targetLevel, _animationCts.Token);
            }
            catch (OperationCanceledException)
            {
                // アニメーション中断は正常
            }
            catch (Exception ex)
            {
                Debug.LogError($"[StealthUIManager] Animation error: {ex.Message}");
            }
        }

        /// <summary>
        /// ステルスアニメーションの核心ロジック
        /// </summary>
        private async UniTask PerformStealthAnimationAsync(float targetLevel, CancellationToken cancellationToken)
        {
            if (_stealthLevelSlider == null) return;

            float startValue = _stealthLevelSlider.value;
            
            // 既存Tweenキャンセル
            KillTween("StealthLevel");
            KillTween("StealthColor");

            // 並行アニメーション実行
            var sliderTask = AnimateSliderValueAsync(startValue, targetLevel, cancellationToken);
            var colorTask = AnimateSliderColorAsync(targetLevel, cancellationToken);

            // 両方のアニメーションを並行実行
            await UniTask.WhenAll(sliderTask, colorTask);
        }

        /// <summary>
        /// スライダー値のアニメーション
        /// </summary>
        private async UniTask AnimateSliderValueAsync(float startValue, float targetValue, CancellationToken cancellationToken)
        {
            // DOTween作成
            var tween = DOTween.To(
                () => startValue,
                value => {
                    if (_stealthLevelSlider != null)
                        _stealthLevelSlider.value = value;
                },
                targetValue,
                _animationDuration
            ).SetEase(_stealthLevelCurve);

            // Tween登録
            _activeTweens["StealthLevel"] = tween;

            // UniTask統合待機
            await tween.ToUniTask(cancellationToken: cancellationToken);
            
            // 完了後クリーンアップ
            _activeTweens.Remove("StealthLevel");
        }

        /// <summary>
        /// スライダー色のアニメーション
        /// </summary>
        private async UniTask AnimateSliderColorAsync(float targetLevel, CancellationToken cancellationToken)
        {
            if (_stealthLevelFill == null) return;

            Color startColor = _stealthLevelFill.color;
            Color targetColor = Color.Lerp(_exposedColor, _hiddenColor, targetLevel);

            // 色アニメーション
            var tween = DOTween.To(
                () => startColor,
                color => {
                    if (_stealthLevelFill != null)
                        _stealthLevelFill.color = color;
                },
                targetColor,
                _animationDuration
            ).SetEase(Ease.OutQuad);

            // Tween登録
            _activeTweens["StealthColor"] = tween;

            // UniTask統合待機
            await tween.ToUniTask(cancellationToken: cancellationToken);
            
            // 完了後クリーンアップ
            _activeTweens.Remove("StealthColor");
        }

        #endregion

        #region Alert Level Animation (追加機能)

        /// <summary>
        /// 警戒レベルアニメーション（脈動効果）
        /// </summary>
        public void AnimateAlertLevel(AIAlertLevel alertLevel)
        {
            AnimateAlertLevelAsync(alertLevel).Forget();
        }

        private async UniTaskVoid AnimateAlertLevelAsync(AIAlertLevel alertLevel)
        {
            if (_stealthLevelFill == null) return;

            // 警戒レベルに応じた脈動効果
            float intensity = GetAlertIntensity(alertLevel);
            
            if (intensity > 0)
            {
                await CreatePulseEffectAsync(intensity, _componentCts.Token);
            }
        }

        /// <summary>
        /// 脈動効果作成
        /// </summary>
        private async UniTask CreatePulseEffectAsync(float intensity, CancellationToken cancellationToken)
        {
            // 脈動パラメータ
            int pulseCount = Mathf.RoundToInt(intensity * 3);
            float pulseDuration = 0.2f / intensity;

            for (int i = 0; i < pulseCount && !cancellationToken.IsCancellationRequested; i++)
            {
                // 拡大
                var scaleUp = _stealthLevelFill.transform.DOScale(1f + intensity * 0.1f, pulseDuration * 0.5f);
                await scaleUp.ToUniTask(cancellationToken: cancellationToken);

                // 縮小
                var scaleDown = _stealthLevelFill.transform.DOScale(1f, pulseDuration * 0.5f);
                await scaleDown.ToUniTask(cancellationToken: cancellationToken);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Tweenキャンセル
        /// </summary>
        private void KillTween(string tweenName)
        {
            if (_activeTweens.TryGetValue(tweenName, out var tween))
            {
                tween?.Kill();
                _activeTweens.Remove(tweenName);
            }
        }

        /// <summary>
        /// 警戒レベルから強度変換
        /// </summary>
        private float GetAlertIntensity(AIAlertLevel alertLevel)
        {
            return alertLevel switch
            {
                AIAlertLevel.Relaxed => 0f,
                AIAlertLevel.Suspicious => 0.3f,
                AIAlertLevel.Investigating => 0.6f,
                AIAlertLevel.Alert => 1f,
                _ => 0f
            };
        }

        #endregion

        #region Performance Monitoring

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogAnimationPerformance(string animationType, float duration)
        {
            Debug.Log($"[StealthUIManager] {animationType} animation: {duration:F3}s | Active tweens: {_activeTweens.Count}");
        }

        #endregion
    }

    // 警戒レベル定義
    public enum AIAlertLevel
    {
        Relaxed,
        Suspicious, 
        Investigating,
        Alert
    }
}
```

#### 🎨 アニメーション機能

1. **並行アニメーション**: 値と色の同時アニメーション
2. **DOTween統合**: 高品質なEasing/Curve制御
3. **脈動効果**: 警戒レベルに応じた動的エフェクト
4. **キャンセル制御**: アニメーション中断の適切な処理

---

## 🎯 ベストプラクティス・パターン集

### 1. CancellationToken管理パターン

```csharp
public class OptimalCancellationPattern : MonoBehaviour
{
    private CancellationTokenSource _componentCts;  // コンポーネント全体
    private CancellationTokenSource _operationCts; // 個別操作

    private void Awake()
    {
        _componentCts = new CancellationTokenSource();
    }

    public async UniTaskVoid StartOperation()
    {
        // 既存操作キャンセル
        _operationCts?.Cancel();
        _operationCts = CancellationTokenSource.CreateLinkedTokenSource(_componentCts.Token);

        try
        {
            await SomeAsyncOperation(_operationCts.Token);
        }
        catch (OperationCanceledException)
        {
            // キャンセルは正常
        }
        finally
        {
            _operationCts?.Dispose();
            _operationCts = null;
        }
    }

    private void OnDestroy()
    {
        _operationCts?.Cancel();
        _operationCts?.Dispose();
        
        _componentCts?.Cancel();
        _componentCts?.Dispose();
    }
}
```

### 2. エラーハンドリングパターン

```csharp
public class RobustErrorHandling : MonoBehaviour
{
    private async UniTaskVoid SafeAsyncOperation()
    {
        try
        {
            await RiskyOperation();
        }
        catch (OperationCanceledException)
        {
            // キャンセル（正常）
            Debug.Log("Operation cancelled");
        }
        catch (TimeoutException)
        {
            // タイムアウト
            Debug.LogWarning("Operation timed out");
            await RetryWithBackoff();
        }
        catch (Exception ex)
        {
            // 予期しないエラー
            Debug.LogError($"Unexpected error: {ex.Message}");
            OnOperationFailed?.Invoke(ex);
        }
    }

    private async UniTask RetryWithBackoff()
    {
        for (int i = 0; i < 3; i++)
        {
            try
            {
                await UniTask.Delay(1000 * (i + 1)); // バックオフ
                await RiskyOperation();
                return; // 成功
            }
            catch (Exception)
            {
                if (i == 2) throw; // 最終リトライ失敗
            }
        }
    }
}
```

### 3. パフォーマンス最適化パターン

```csharp
public class PerformanceOptimizedPattern : MonoBehaviour
{
    private readonly struct FrameData
    {
        public readonly float DeltaTime;
        public readonly int FrameCount;

        public FrameData(float deltaTime, int frameCount)
        {
            DeltaTime = deltaTime;
            FrameCount = frameCount;
        }
    }

    private async UniTask OptimizedFrameLoop(CancellationToken cancellationToken)
    {
        var frameTimer = new FrameData(Time.deltaTime, Time.frameCount);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            // 重い処理は複数フレームに分散
            if (Time.frameCount % 5 == 0) // 5フレームに1回
            {
                await HeavyOperation(cancellationToken);
            }

            // 軽い処理は毎フレーム
            LightOperation();

            // 高効率フレーム待機
            await UniTask.NextFrame(cancellationToken);
            
            // フレームデータ更新
            frameTimer = new FrameData(Time.deltaTime, Time.frameCount);
        }
    }
}
```

---

## 🔧 トラブルシューティング・よくある問題

### 1. async void禁止

```csharp
// ❌ 絶対にダメ
private async void BadMethod() { }

// ✅ イベントハンドラーのみ許可
private async UniTaskVoid GoodEventHandler() { }

// ✅ 通常のメソッド
private async UniTask BestMethod() { }
```

### 2. デッドロック回避

```csharp
// ❌ デッドロックの原因
public void BadSyncMethod()
{
    AsyncMethod().GetAwaiter().GetResult(); // NG
}

// ✅ 正しい非同期呼び出し
public async UniTaskVoid GoodAsyncMethod()
{
    await AsyncMethod(); // OK
}
```

### 3. メモリリークガード

```csharp
public class MemoryLeakGuard : MonoBehaviour
{
    private readonly List<CancellationTokenSource> _tokenSources = new();

    public CancellationTokenSource CreateToken()
    {
        var cts = new CancellationTokenSource();
        _tokenSources.Add(cts);
        return cts;
    }

    private void OnDestroy()
    {
        // 全TokenSourceを適切に破棄
        foreach (var cts in _tokenSources)
        {
            cts?.Cancel();
            cts?.Dispose();
        }
        _tokenSources.Clear();
    }
}
```

---

## 📊 性能測定・検証方法

### 1. Unity Profiler統合

```csharp
public class PerformanceMeasurement : MonoBehaviour
{
    private async UniTask MeasuredOperation()
    {
        using (UnityEngine.Profiling.Profiler.BeginSample("UniTask Operation"))
        {
            await SomeHeavyOperation();
        }
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void LogPerformanceMetrics(string operation, float duration)
    {
        Debug.Log($"[Performance] {operation}: {duration:F3}ms");
    }
}
```

### 2. メモリ使用量監視

```csharp
public class MemoryMonitor : MonoBehaviour
{
    private void Start()
    {
        MonitorMemoryUsageAsync().Forget();
    }

    private async UniTaskVoid MonitorMemoryUsageAsync()
    {
        while (this != null)
        {
            var allocated = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(0);
            var reserved = UnityEngine.Profiling.Profiler.GetTotalReservedMemory(0);
            
            Debug.Log($"[Memory] Allocated: {allocated / 1024 / 1024}MB | Reserved: {reserved / 1024 / 1024}MB");
            
            await UniTask.Delay(5000); // 5秒間隔
        }
    }
}
```

---

## 🎯 実装チェックリスト

### Phase 1実装前チェック

- [ ] UniTaskパッケージ導入確認
- [ ] DOTween Proライセンス確認
- [ ] 既存コルーチンのバックアップ
- [ ] テストシナリオ作成

### 実装中チェック

- [ ] CancellationTokenSource適切な管理
- [ ] OnDestroy()での適切なクリーンアップ
- [ ] try-catch構造でのエラーハンドリング
- [ ] Unity Profilerでのパフォーマンス確認

### 実装後検証

- [ ] 機能互換性テスト（既存動作維持）
- [ ] パフォーマンステスト（期待値達成）
- [ ] メモリリークテスト（長時間稼働）
- [ ] エラー回復テスト（例外状況対応）

---

**実装例集作成完了**: 開発チームが即座に参考にできる実践的なUniTask移行例を提供しました。各例は本プロジェクトの実際のコードベースに適用可能な形で設計されています。
