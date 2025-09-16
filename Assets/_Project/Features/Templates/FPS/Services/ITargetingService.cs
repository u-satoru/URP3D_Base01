using System;
using System.Collections.Generic;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// ターゲティングサービス（ServiceLocator経由アクセス）
    /// FPS Template専用照準・ターゲット検出システム
    /// </summary>
    public interface ITargetingService
    {
        // ターゲット検出
        GameObject GetCurrentTarget();
        List<GameObject> GetTargetsInRange(float range);
        List<GameObject> GetTargetsInSight(float range, float fieldOfView);
        GameObject GetClosestTarget(float maxRange);

        // レイキャスト照準
        bool GetAimTarget(Vector3 origin, Vector3 direction, float maxRange, out RaycastHit hit);
        bool GetAimTarget(Vector3 origin, Vector3 direction, float maxRange, LayerMask layerMask, out RaycastHit hit);

        // 自動照準支援（コンソール対応用）
        GameObject GetAutoAimTarget(Vector3 origin, Vector3 direction, float assistRange, float assistAngle);
        bool ShouldApplyAutoAim();

        // ターゲット管理
        void SetCurrentTarget(GameObject target);
        void ClearCurrentTarget();
        void AddPotentialTarget(GameObject target);
        void RemovePotentialTarget(GameObject target);

        // ターゲティング設定
        void SetTargetingSettings(TargetingConfiguration config);
        void EnableAutoAim(bool enable);

        // ターゲティングイベント
        event Action<GameObject> OnTargetAcquired;
        event Action<GameObject> OnTargetLost;
        event Action<GameObject> OnTargetChanged;
    }

    /// <summary>
    /// ターゲティング設定
    /// </summary>
    [System.Serializable]
    public class TargetingConfiguration
    {
        [Header("ターゲット検出")]
        public float maxTargetRange = 100f;
        public float fieldOfView = 90f;
        public LayerMask targetLayers = -1;

        [Header("自動照準")]
        public bool enableAutoAim = false;
        public float autoAimRange = 50f;
        public float autoAimAngle = 15f;
        public float autoAimStrength = 0.5f;

        [Header("ターゲット優先度")]
        public TargetPriority targetPriority = TargetPriority.Closest;
        public bool preferEnemies = true;
    }

    /// <summary>
    /// ターゲット優先度
    /// </summary>
    public enum TargetPriority
    {
        Closest,        // 最も近い
        LowestHealth,   // 最も体力が低い
        HighestThreat,  // 最も脅威度が高い
        MostRecent      // 最も最近発見した
    }
}