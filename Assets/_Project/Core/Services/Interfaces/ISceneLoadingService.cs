using System.Collections;

namespace asterivo.Unity60.Core.Services
{
    public interface ISceneLoadingService
    {
        void LoadGameplaySceneWithMinTime();
        void LoadSceneWithMinTime(string sceneName);
    }
}
