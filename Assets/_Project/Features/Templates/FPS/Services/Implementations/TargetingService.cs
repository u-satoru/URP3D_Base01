using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// ターゲティングサービス実装
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// </summary>
    public class TargetingService : ITargetingService
    {
        private readonly List<GameObject> _potentialTargets = new();
        private GameObject _currentTarget;
        private TargetingConfiguration _configuration;

        // イベント
        public event Action<GameObject> OnTargetAcquired;
        public event Action<GameObject> OnTargetLost;
        public event Action<GameObject> OnTargetChanged;

        // ターゲット検出
        public GameObject GetCurrentTarget()
        {
            return _currentTarget;
        }

        public List<GameObject> GetTargetsInRange(float range)
        {
            if (range <= 0f)
            {
                Debug.LogWarning("[TargetingService] Invalid range specified");
                return new List<GameObject>();
            }

            var targetsInRange = new List<GameObject>();
            var playerPosition = GetPlayerPosition();

            if (playerPosition == Vector3.zero)
            {
                return targetsInRange;
            }

            foreach (var target in _potentialTargets)
            {
                if (target == null)
                    continue;

                float distance = Vector3.Distance(playerPosition, target.transform.position);
                if (distance <= range)
                {
                    targetsInRange.Add(target);
                }
            }

            // 距離でソート（近い順）
            targetsInRange.Sort((a, b) =>
                Vector3.Distance(playerPosition, a.transform.position)
                .CompareTo(Vector3.Distance(playerPosition, b.transform.position)));

            return targetsInRange;
        }

        public List<GameObject> GetTargetsInSight(float range, float fieldOfView)
        {
            if (range <= 0f || fieldOfView <= 0f)
            {
                Debug.LogWarning("[TargetingService] Invalid sight parameters");
                return new List<GameObject>();
            }

            var targetsInSight = new List<GameObject>();
            var playerPosition = GetPlayerPosition();
            var playerForward = GetPlayerForward();

            if (playerPosition == Vector3.zero)
            {
                return targetsInSight;
            }

            foreach (var target in _potentialTargets)
            {
                if (target == null)
                    continue;

                Vector3 directionToTarget = (target.transform.position - playerPosition).normalized;
                float angle = Vector3.Angle(playerForward, directionToTarget);
                float distance = Vector3.Distance(playerPosition, target.transform.position);

                // 視野角と距離のチェック
                if (angle <= fieldOfView * 0.5f && distance <= range)
                {
                    // 遮蔽物チェック
                    if (HasLineOfSight(playerPosition, target.transform.position))
                    {
                        targetsInSight.Add(target);
                    }
                }
            }

            return targetsInSight;
        }

        public GameObject GetClosestTarget(float maxRange)
        {
            var targetsInRange = GetTargetsInRange(maxRange);
            return targetsInRange.FirstOrDefault();
        }

        // レイキャスト照準
        public bool GetAimTarget(Vector3 origin, Vector3 direction, float maxRange, out RaycastHit hit)
        {
            return GetAimTarget(origin, direction, maxRange, _configuration?.targetLayers ?? -1, out hit);
        }

        public bool GetAimTarget(Vector3 origin, Vector3 direction, float maxRange, LayerMask layerMask, out RaycastHit hit)
        {
            if (maxRange <= 0f)
            {
                hit = new RaycastHit();
                Debug.LogWarning("[TargetingService] Invalid aim range");
                return false;
            }

            bool hasHit = Physics.Raycast(origin, direction, out hit, maxRange, layerMask);

            if (hasHit)
            {
                Debug.Log($"[TargetingService] Aim target found: {hit.collider.name} at distance {hit.distance:F2}");
            }

            return hasHit;
        }

        // 自動照準支援（コンソール対応用）
        public GameObject GetAutoAimTarget(Vector3 origin, Vector3 direction, float assistRange, float assistAngle)
        {
            if (!ShouldApplyAutoAim())
            {
                return null;
            }

            var candidateTargets = GetTargetsInRange(assistRange);
            GameObject bestTarget = null;
            float bestScore = float.MaxValue;

            foreach (var target in candidateTargets)
            {
                if (target == null)
                    continue;

                Vector3 directionToTarget = (target.transform.position - origin).normalized;
                float angle = Vector3.Angle(direction, directionToTarget);

                if (angle <= assistAngle)
                {
                    // スコア計算（角度 + 距離）
                    float distance = Vector3.Distance(origin, target.transform.position);
                    float score = angle * 2f + distance * 0.1f; // 角度を重視

                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestTarget = target;
                    }
                }
            }

            if (bestTarget != null)
            {
                Debug.Log($"[TargetingService] Auto-aim target: {bestTarget.name}");
            }

            return bestTarget;
        }

        public bool ShouldApplyAutoAim()
        {
            if (_configuration == null)
                return false;

            // 設定が有効 かつ コントローラー使用時
            return _configuration.enableAutoAim && IsUsingController();
        }

        // ターゲット管理
        public void SetCurrentTarget(GameObject target)
        {
            var previousTarget = _currentTarget;
            _currentTarget = target;

            if (previousTarget != target)
            {
                if (previousTarget != null)
                {
                    OnTargetLost?.Invoke(previousTarget);
                }

                if (target != null)
                {
                    OnTargetAcquired?.Invoke(target);

                    // ServiceLocator経由でAudioサービス取得
                    var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
                    audioService?.PlaySFX("TargetAcquired", Vector3.zero);
                }

                OnTargetChanged?.Invoke(_currentTarget);

                Debug.Log($"[TargetingService] Target changed: {previousTarget?.name ?? "null"} → {target?.name ?? "null"}");
            }
        }

        public void ClearCurrentTarget()
        {
            SetCurrentTarget(null);
        }

        public void AddPotentialTarget(GameObject target)
        {
            if (target == null)
            {
                Debug.LogWarning("[TargetingService] Cannot add null target");
                return;
            }

            if (!_potentialTargets.Contains(target))
            {
                _potentialTargets.Add(target);
                Debug.Log($"[TargetingService] Added potential target: {target.name}");
            }
        }

        public void RemovePotentialTarget(GameObject target)
        {
            if (target == null)
                return;

            if (_potentialTargets.Remove(target))
            {
                // 現在のターゲットが削除された場合
                if (_currentTarget == target)
                {
                    ClearCurrentTarget();
                }

                Debug.Log($"[TargetingService] Removed potential target: {target.name}");
            }
        }

        // ターゲティング設定
        public void SetTargetingSettings(TargetingConfiguration config)
        {
            _configuration = config ?? throw new ArgumentNullException(nameof(config));

            Debug.Log("[TargetingService] Targeting configuration updated");
        }

        public void EnableAutoAim(bool enable)
        {
            if (_configuration != null)
            {
                _configuration.enableAutoAim = enable;
                Debug.Log($"[TargetingService] Auto-aim {(enable ? "enabled" : "disabled")}");
            }
        }

        // プライベートメソッド
        private Vector3 GetPlayerPosition()
        {
            // ServiceLocator経由でプレイヤー情報取得
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            return playerGO?.transform.position ?? Vector3.zero;
        }

        private Vector3 GetPlayerForward()
        {
            // ServiceLocator経由でカメラサービス取得
            var cameraService = asterivo.Unity60.Core.ServiceLocator.GetService<IFPSCameraService>();
            if (cameraService != null)
            {
                // カメラの向きを取得
                var mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    return mainCamera.transform.forward;
                }
            }

            var playerGO = GameObject.FindGameObjectWithTag("Player");
            return playerGO?.transform.forward ?? Vector3.forward;
        }

        private bool HasLineOfSight(Vector3 from, Vector3 to)
        {
            Vector3 direction = (to - from).normalized;
            float distance = Vector3.Distance(from, to);

            // 遮蔽物チェック（障害物レイヤーを除外）
            LayerMask obstacleLayerMask = _configuration?.targetLayers ?? -1;

            return !Physics.Raycast(from, direction, distance, ~obstacleLayerMask);
        }

        private bool IsUsingController()
        {
            // ServiceLocator経由で入力サービス取得
            var inputService = asterivo.Unity60.Core.ServiceLocator.GetService<IFPSInputService>();
            if (inputService != null)
            {
                return inputService.GetActiveInputDevice() == InputDeviceType.Gamepad;
            }

            return false;
        }

        // 優先度付けロジック
        private GameObject SelectTargetByPriority(List<GameObject> targets)
        {
            if (targets.Count == 0)
                return null;

            if (_configuration == null)
                return targets.First();

            var playerPosition = GetPlayerPosition();

            switch (_configuration.targetPriority)
            {
                case TargetPriority.Closest:
                    return targets.OrderBy(t => Vector3.Distance(playerPosition, t.transform.position)).First();

                case TargetPriority.LowestHealth:
                    return targets.OrderBy(t => GetTargetHealth(t)).First();

                case TargetPriority.HighestThreat:
                    return targets.OrderByDescending(t => GetTargetThreatLevel(t)).First();

                case TargetPriority.MostRecent:
                    return targets.Last(); // 最後に追加されたもの

                default:
                    return targets.First();
            }
        }

        private float GetTargetHealth(GameObject target)
        {
            // ヘルスコンポーネント取得（実装予定）
            // var health = target.GetComponent<IHealth>();
            // return health?.CurrentHealth ?? 100f;
            return 100f; // デフォルト値
        }

        private float GetTargetThreatLevel(GameObject target)
        {
            // 脅威レベル計算（実装予定）
            // 距離、武器、アクティブ状態などを考慮
            var playerPosition = GetPlayerPosition();
            float distance = Vector3.Distance(playerPosition, target.transform.position);

            // 近いほど脅威度が高い
            return 1000f / Mathf.Max(1f, distance);
        }

        // 自動ターゲティング更新（定期実行用）
        public void UpdateAutoTargeting()
        {
            if (_configuration == null || !_configuration.enableAutoAim)
                return;

            var playerPosition = GetPlayerPosition();
            var playerForward = GetPlayerForward();

            var candidateTargets = GetTargetsInSight(
                _configuration.autoAimRange,
                _configuration.fieldOfView
            );

            var bestTarget = SelectTargetByPriority(candidateTargets);

            if (bestTarget != _currentTarget)
            {
                SetCurrentTarget(bestTarget);
            }
        }

        // 統計情報
        public TargetingStatistics GetTargetingStatistics()
        {
            return new TargetingStatistics
            {
                PotentialTargetCount = _potentialTargets.Count(t => t != null),
                CurrentTarget = _currentTarget,
                AutoAimEnabled = _configuration?.enableAutoAim ?? false,
                MaxTargetRange = _configuration?.maxTargetRange ?? 0f,
                FieldOfView = _configuration?.fieldOfView ?? 0f
            };
        }
    }

    /// <summary>
    /// ターゲティング統計情報
    /// </summary>
    [System.Serializable]
    public class TargetingStatistics
    {
        public int PotentialTargetCount;
        public GameObject CurrentTarget;
        public bool AutoAimEnabled;
        public float MaxTargetRange;
        public float FieldOfView;
    }
}