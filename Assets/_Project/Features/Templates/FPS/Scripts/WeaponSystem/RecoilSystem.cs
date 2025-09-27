using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.FPS.Data;

namespace asterivo.Unity60.Features.Templates.FPS.WeaponSystem
{
    /// <summary>
    /// WeaponDataの反動パターンに基づき、手続き的にカメラと武器モデルの反動を生成するシステム
    /// 詳細設計書3.2準拠: RecoilSystem実装
    /// </summary>
    [System.Serializable]
    public class RecoilSystem
    {
        [Header("Recoil Events")]
        [SerializeField] private GameEvent _onRecoilStarted;
        [SerializeField] private GameEvent _onRecoilCompleted;

        [Header("Recoil Settings")]
        [SerializeField] private float _recoilMultiplier = 1.0f;
        [SerializeField] private float _recoilRecoverySpeed = 2.0f;
        [SerializeField] private AnimationCurve _recoilCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private bool _enableRandomization = true;
        [SerializeField] private float _randomizationFactor = 0.1f;

        [Header("Camera Recoil")]
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private float _cameraRecoilStrength = 1.0f;
        [SerializeField] private float _cameraRecoveryTime = 0.5f;

        [Header("Weapon Model Recoil")]
        [SerializeField] private Transform _weaponModelTransform;
        [SerializeField] private float _weaponRecoilStrength = 1.0f;
        [SerializeField] private Vector3 _weaponKickbackOffset = new Vector3(0, 0, -0.1f);

        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogs = false;

        // 内部状態
        private Vector3 _currentCameraRecoil = Vector3.zero;
        private Vector3 _currentWeaponRecoil = Vector3.zero;
        private Vector3 _targetCameraRecoil = Vector3.zero;
        private Vector3 _targetWeaponRecoil = Vector3.zero;
        private bool _isRecovering = false;
        private float _recoveryProgress = 0f;

        // 反動パターン状態
        private WeaponData _currentWeaponData;
        private int _shotCount = 0;
        private Vector2 _accumulatedRecoil = Vector2.zero;

        /// <summary>
        /// 現在のカメラ反動値（外部から参照用）
        /// </summary>
        public Vector3 CurrentCameraRecoil => _currentCameraRecoil;

        /// <summary>
        /// 現在の武器反動値（外部から参照用）
        /// </summary>
        public Vector3 CurrentWeaponRecoil => _currentWeaponRecoil;

        /// <summary>
        /// 反動復帰中かどうか
        /// </summary>
        public bool IsRecovering => _isRecovering;

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize(Transform cameraTransform, Transform weaponModelTransform)
        {
            _cameraTransform = cameraTransform;
            _weaponModelTransform = weaponModelTransform;

            Reset();

            if (_enableDebugLogs)
            {
                Debug.Log($"[RecoilSystem] Initialized - Camera: {_cameraTransform?.name}, Weapon: {_weaponModelTransform?.name}");
            }
        }

        /// <summary>
        /// 武器データを設定
        /// </summary>
        public void SetWeaponData(WeaponData weaponData)
        {
            _currentWeaponData = weaponData;
            Reset();

            if (_enableDebugLogs)
            {
                Debug.Log($"[RecoilSystem] Weapon data set: {weaponData?.WeaponName}");
            }
        }

        /// <summary>
        /// 反動パターンを設定（WeaponControllerとの互換性のため）
        /// </summary>
        public void SetRecoilPattern(RecoilPattern recoilPattern)
        {
            // Note: RecoilPatternを直接設定する場合は、WeaponDataが必要
            // 現在の実装では、武器データ全体を設定することを推奨
            if (_enableDebugLogs)
            {
                Debug.Log($"[RecoilSystem] Recoil pattern set directly");
            }
        }

        /// <summary>
        /// 反動を適用
        /// </summary>
        public void ApplyRecoil()
        {
            if (_currentWeaponData == null || _currentWeaponData.RecoilPattern == null)
            {
                if (_enableDebugLogs)
                {
                    Debug.LogWarning("[RecoilSystem] No weapon data or recoil pattern available");
                }
                return;
            }

            // 反動パターンから現在のショット用の反動値を取得
            var recoilPattern = _currentWeaponData.RecoilPattern;
            Vector2 recoilVector = GetRecoilVector(recoilPattern);

            // 連射による反動の蓄積
            _accumulatedRecoil += recoilVector * _recoilMultiplier;
            _shotCount++;

            // ランダム化の適用
            if (_enableRandomization)
            {
                float randomX = UnityEngine.Random.Range(-_randomizationFactor, _randomizationFactor);
                float randomY = UnityEngine.Random.Range(-_randomizationFactor, _randomizationFactor);
                recoilVector += new Vector2(randomX, randomY);
            }

            // カメラ反動の計算
            Vector3 cameraRecoil = new Vector3(
                -recoilVector.y * _cameraRecoilStrength, // 垂直反動（ピッチ）
                recoilVector.x * _cameraRecoilStrength,  // 水平反動（ヨー）
                0f
            );

            // 武器モデル反動の計算
            Vector3 weaponRecoil = new Vector3(
                recoilVector.x * _weaponRecoilStrength,
                recoilVector.y * _weaponRecoilStrength,
                0f
            ) + _weaponKickbackOffset * Mathf.Abs(recoilVector.magnitude);

            // 目標反動値を設定
            _targetCameraRecoil += cameraRecoil;
            _targetWeaponRecoil += weaponRecoil;

            // 反動開始イベント
            _onRecoilStarted?.Raise();

            // 反動適用処理を開始
            ApplyRecoilAsync().Forget();

            if (_enableDebugLogs)
            {
                Debug.Log($"[RecoilSystem] Applied recoil - Shot: {_shotCount}, Camera: {cameraRecoil}, Weapon: {weaponRecoil}");
            }
        }

        /// <summary>
        /// 反動パターンから現在のショット用の反動値を取得
        /// </summary>
        private Vector2 GetRecoilVector(RecoilPattern recoilPattern)
        {
            if (recoilPattern.verticalRecoil == null || recoilPattern.verticalRecoil.Length == 0)
                return Vector2.zero;

            // 反動パターンのインデックスを循環させる
            int index = _shotCount % recoilPattern.verticalRecoil.Length;
            float verticalRecoil = recoilPattern.verticalRecoil[index];

            // 水平反動の計算（配列がない場合は垂直反動から推定）
            float horizontalRecoil = 0f;
            if (recoilPattern.horizontalRecoil != null && recoilPattern.horizontalRecoil.Length > 0)
            {
                int hIndex = _shotCount % recoilPattern.horizontalRecoil.Length;
                horizontalRecoil = recoilPattern.horizontalRecoil[hIndex];
            }
            else
            {
                // 水平反動がない場合は、垂直反動の10%をランダム方向に適用
                horizontalRecoil = verticalRecoil * 0.1f * (UnityEngine.Random.value - 0.5f) * 2f;
            }

            return new Vector2(horizontalRecoil, verticalRecoil);
        }

        /// <summary>
        /// 非同期反動適用処理
        /// </summary>
        private async UniTaskVoid ApplyRecoilAsync()
        {
            _isRecovering = false;

            // 即座に反動を適用（スナップ動作）
            if (_cameraTransform != null)
            {
                _currentCameraRecoil = _targetCameraRecoil;
                ApplyCameraRecoil();
            }

            if (_weaponModelTransform != null)
            {
                _currentWeaponRecoil = _targetWeaponRecoil;
                ApplyWeaponRecoil();
            }

            // 少し待ってから復帰開始
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));

            // 反動復帰処理を開始
            StartRecovery();
        }

        /// <summary>
        /// 反動復帰処理を開始
        /// </summary>
        private void StartRecovery()
        {
            if (_isRecovering) return;

            _isRecovering = true;
            _recoveryProgress = 0f;

            RecoveryAsync().Forget();
        }

        /// <summary>
        /// 非同期反動復帰処理
        /// </summary>
        private async UniTaskVoid RecoveryAsync()
        {
            while (_isRecovering && (_currentCameraRecoil.magnitude > 0.01f || _currentWeaponRecoil.magnitude > 0.01f))
            {
                _recoveryProgress += Time.deltaTime / _cameraRecoveryTime;
                _recoveryProgress = Mathf.Clamp01(_recoveryProgress);

                float curveValue = _recoilCurve.Evaluate(_recoveryProgress);
                float recoveryFactor = 1f - curveValue;

                // カメラ反動復帰
                if (_cameraTransform != null)
                {
                    _currentCameraRecoil = Vector3.Lerp(_targetCameraRecoil, Vector3.zero, recoveryFactor);
                    ApplyCameraRecoil();
                }

                // 武器反動復帰
                if (_weaponModelTransform != null)
                {
                    _currentWeaponRecoil = Vector3.Lerp(_targetWeaponRecoil, Vector3.zero, recoveryFactor);
                    ApplyWeaponRecoil();
                }

                await UniTask.Yield();
            }

            // 復帰完了
            _isRecovering = false;
            _currentCameraRecoil = Vector3.zero;
            _currentWeaponRecoil = Vector3.zero;
            _targetCameraRecoil = Vector3.zero;
            _targetWeaponRecoil = Vector3.zero;

            // 反動完了イベント
            _onRecoilCompleted?.Raise();

            if (_enableDebugLogs)
            {
                Debug.Log("[RecoilSystem] Recoil recovery completed");
            }
        }

        /// <summary>
        /// カメラに反動を適用
        /// </summary>
        private void ApplyCameraRecoil()
        {
            if (_cameraTransform == null) return;

            // 注意: 実際のカメラ制御はCameraSystemが担当する想定
            // ここでは反動値を保持し、外部システムが参照できるようにする
            // 必要に応じてCameraSystemにイベント経由で反動データを送信
        }

        /// <summary>
        /// 武器モデルに反動を適用
        /// </summary>
        private void ApplyWeaponRecoil()
        {
            if (_weaponModelTransform == null) return;

            // 武器モデルの位置・回転に反動を適用
            Vector3 originalPosition = _weaponModelTransform.localPosition;
            Vector3 originalRotation = _weaponModelTransform.localEulerAngles;

            // 位置の反動（キックバック）
            Vector3 recoilPosition = originalPosition + _currentWeaponRecoil;

            // 回転の反動
            Vector3 recoilRotation = originalRotation + _currentWeaponRecoil;

            _weaponModelTransform.localPosition = recoilPosition;
            _weaponModelTransform.localEulerAngles = recoilRotation;
        }

        /// <summary>
        /// 反動をリセット
        /// </summary>
        public void Reset()
        {
            _shotCount = 0;
            _accumulatedRecoil = Vector2.zero;
            _currentCameraRecoil = Vector3.zero;
            _currentWeaponRecoil = Vector3.zero;
            _targetCameraRecoil = Vector3.zero;
            _targetWeaponRecoil = Vector3.zero;
            _isRecovering = false;
            _recoveryProgress = 0f;

            if (_enableDebugLogs)
            {
                Debug.Log("[RecoilSystem] Reset completed");
            }
        }

        /// <summary>
        /// 反動システムを停止
        /// </summary>
        public void Stop()
        {
            _isRecovering = false;
            Reset();

            if (_enableDebugLogs)
            {
                Debug.Log("[RecoilSystem] Stopped");
            }
        }

        /// <summary>
        /// デバッグ情報を取得
        /// </summary>
        public string GetDebugInfo()
        {
            return $"RecoilSystem - Shots: {_shotCount}, Camera: {_currentCameraRecoil}, Weapon: {_currentWeaponRecoil}, Recovering: {_isRecovering}";
        }

        /// <summary>
        /// Inspector での設定確認
        /// </summary>
        public bool ValidateSettings()
        {
            bool isValid = true;

            if (_recoilMultiplier < 0f)
            {
                Debug.LogWarning("[RecoilSystem] Recoil multiplier should be positive");
                isValid = false;
            }

            if (_recoilRecoverySpeed <= 0f)
            {
                Debug.LogWarning("[RecoilSystem] Recoil recovery speed should be positive");
                isValid = false;
            }

            if (_cameraRecoveryTime <= 0f)
            {
                Debug.LogWarning("[RecoilSystem] Camera recovery time should be positive");
                isValid = false;
            }

            return isValid;
        }
    }
}
