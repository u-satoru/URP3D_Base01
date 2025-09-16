using System;

namespace asterivo.Unity60.Features.Templates.Platformer.Services
{
    /// <summary>
    /// チェックポイントサービス インターフェース
    /// セーブ・ロード・リスポーン管理
    /// </summary>
    public interface ICheckpointService : IPlatformerService
    {
        void SaveProgress(int level, int score, int lives);
        void LoadFromCheckpoint();
        void SetCheckpoint(UnityEngine.Vector3 position);
        bool HasCheckpoint { get; }
        UnityEngine.Vector3 LastCheckpointPosition { get; }

        // エラー解決用：メソッド形式でのアクセス
        UnityEngine.Vector3 GetLastCheckpointPosition();
    }
}