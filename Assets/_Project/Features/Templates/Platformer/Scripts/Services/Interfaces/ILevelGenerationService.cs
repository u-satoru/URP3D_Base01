using System;

namespace asterivo.Unity60.Features.Templates.Platformer.Services
{
    /// <summary>
    /// レベル生成サービス インターフェース
    /// レベル生成・配置・動的調整
    /// </summary>
    public interface ILevelGenerationService : IPlatformerService
    {
        void GenerateLevel(int levelNumber);
        void ClearCurrentLevel();
        int CurrentLevelNumber { get; }
        bool IsLevelGenerated { get; }
    }
}
