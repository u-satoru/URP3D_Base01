using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Types;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Features.GameManagement.Interfaces
{
    /// <summary>
    /// 繧ｲ繝ｼ繝邂｡逅・し繝ｼ繝薙せ縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ
    /// 繧ｲ繝ｼ繝縺ｮ蜈ｨ菴鍋噪縺ｪ蛻ｶ蠕｡縺ｨ迥ｶ諷狗ｮ｡逅・ｒ謠蝉ｾ・
    /// </summary>
    public interface IGameManager : IService
    {
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ繧ｲ繝ｼ繝迥ｶ諷・
        /// </summary>
        GameState CurrentGameState { get; }

        /// <summary>
        /// 蜑阪・繧ｲ繝ｼ繝迥ｶ諷・
        /// </summary>
        GameState PreviousGameState { get; }

        /// <summary>
        /// 繧ｲ繝ｼ繝邨碁℃譎る俣
        /// </summary>
        float GameTime { get; }

        /// <summary>
        /// 繧ｲ繝ｼ繝縺後・繝ｼ繧ｺ荳ｭ縺九←縺・°
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        /// 繧ｲ繝ｼ繝繧ｪ繝ｼ繝舌・迥ｶ諷九°縺ｩ縺・°
        /// </summary>
        bool IsGameOver { get; }

        /// <summary>
        /// 繧ｲ繝ｼ繝迥ｶ諷九ｒ螟画峩
        /// </summary>
        /// <param name="newState">譁ｰ縺励＞繧ｲ繝ｼ繝迥ｶ諷・/param>
        void ChangeGameState(GameState newState);

        /// <summary>
        /// 繧ｲ繝ｼ繝繧帝幕蟋・
        /// </summary>
        void StartGame();

        /// <summary>
        /// 繧ｲ繝ｼ繝繧剃ｸ譎ょ●豁｢
        /// </summary>
        void PauseGame();

        /// <summary>
        /// 繧ｲ繝ｼ繝繧貞・髢・
        /// </summary>
        void ResumeGame();

        /// <summary>
        /// 繧ｲ繝ｼ繝繧偵Μ繧ｹ繧ｿ繝ｼ繝・
        /// </summary>
        void RestartGame();

        /// <summary>
        /// 繧ｲ繝ｼ繝繧堤ｵゆｺ・
        /// </summary>
        void QuitGame();

        /// <summary>
        /// 繝｡繧､繝ｳ繝｡繝九Η繝ｼ縺ｫ謌ｻ繧・
        /// </summary>
        void ReturnToMenu();

        /// <summary>
        /// 繧ｲ繝ｼ繝繧ｪ繝ｼ繝舌・蜃ｦ逅・
        /// </summary>
        void TriggerGameOver();

        /// <summary>
        /// 蜍晏茜蜃ｦ逅・
        /// </summary>
        void TriggerVictory();

        /// <summary>
        /// 繧ｳ繝槭Φ繝峨ｒ螳溯｡・
        /// </summary>
        /// <param name="command">螳溯｡後☆繧九さ繝槭Φ繝・/param>
        void ExecuteCommand(ICommand command);

        /// <summary>
        /// 譛蠕後・繧ｳ繝槭Φ繝峨ｒ蜿悶ｊ豸医＠
        /// </summary>
        void UndoLastCommand();

        /// <summary>
        /// 譛蠕後↓蜿悶ｊ豸医＠縺溘さ繝槭Φ繝峨ｒ繧・ｊ逶ｴ縺・
        /// </summary>
        void RedoLastCommand();

        /// <summary>
        /// 繝昴・繧ｺ迥ｶ諷九ｒ蛻・ｊ譖ｿ縺・
        /// </summary>
        void TogglePause();
    }
}


