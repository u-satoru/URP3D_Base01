using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core.Audio;  // AudioCoordinatorStats と AudioSystemSyncData の参照用
using asterivo.Unity60.Core.Audio.Interfaces;  // IAudioUpdatable の参照用

namespace asterivo.Unity60.Core.Audio.Interfaces
{
    /// <summary>
    /// オーディオシステムの協調更新を管理するサービスインターフェース
    /// </summary>
    public interface IAudioUpdateService
    {
        /// <summary>
        /// 更新間隔
        /// </summary>
        float UpdateInterval { get; set; }
        
        /// <summary>
        /// 協調更新が有効かどうか
        /// </summary>
        bool IsCoordinatedUpdateEnabled { get; }
        
        /// <summary>
        /// 更新可能なコンポーネントを登録
        /// </summary>
        void RegisterUpdatable(IAudioUpdatable updatable);
        
        /// <summary>
        /// 更新可能なコンポーネントの登録解除
        /// </summary>
        void UnregisterUpdatable(IAudioUpdatable updatable);
        
        /// <summary>
        /// 協調更新の開始
        /// </summary>
        void StartCoordinatedUpdates();
        
        /// <summary>
        /// 協調更新の停止
        /// </summary>
        void StopCoordinatedUpdates();
        
        /// <summary>
        /// 近傍AudioSourceの取得
        /// </summary>
        List<AudioSource> GetNearbyAudioSources(Vector3 center, float radius);
        
        /// <summary>
        /// 空間キャッシュの手動再構築
        /// </summary>
        void ForceRebuildSpatialCache();
        
        /// <summary>
        /// パフォーマンス統計の取得
        /// </summary>
        AudioCoordinatorStats GetPerformanceStats();
        
        /// <summary>
        /// オーディオシステム同期イベント
        /// </summary>
        event System.Action<AudioSystemSyncData> OnAudioSystemSync;
    }
}