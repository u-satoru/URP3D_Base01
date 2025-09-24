// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// 繧ｳ繝槭Φ繝峨・繝ｼ繝ｫ繧ｵ繝ｼ繝薙せ縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ
    /// ServiceLocator繝代ち繝ｼ繝ｳ縺ｧ縺ｮ萓晏ｭ俶ｧ豕ｨ蜈･繧呈髪謠ｴ
    /// </summary>
    public interface ICommandPoolService
    {
        /// <summary>
        /// CommandPoolManager縺ｸ縺ｮ逶ｴ謗･繧｢繧ｯ繧ｻ繧ｹ
        /// 鬮伜ｺｦ縺ｪ蛻ｶ蠕｡繧・き繧ｹ繧ｿ繝繝励・繝ｫ謫堺ｽ懊↓菴ｿ逕ｨ
        /// </summary>
        CommandPoolManager PoolManager { get; }

        /// <summary>
        /// 謖・ｮ壹＠縺溷梛縺ｮ繧ｳ繝槭Φ繝峨ｒ繝励・繝ｫ縺九ｉ蜿門ｾ励＠縺ｾ縺・        /// </summary>
        /// <typeparam name="T">蜿門ｾ励☆繧九さ繝槭Φ繝峨・蝙・/typeparam>
        /// <returns>菴ｿ逕ｨ蜿ｯ閭ｽ縺ｪ繧ｳ繝槭Φ繝峨う繝ｳ繧ｹ繧ｿ繝ｳ繧ｹ</returns>
        T GetCommand<T>() where T : class, ICommand, new();

        /// <summary>
        /// 菴ｿ逕ｨ螳御ｺ・＠縺溘さ繝槭Φ繝峨ｒ繝励・繝ｫ縺ｫ霑泌唆縺励∪縺・        /// </summary>
        /// <typeparam name="T">霑泌唆縺吶ｋ繧ｳ繝槭Φ繝峨・蝙・/typeparam>
        /// <param name="command">繝励・繝ｫ縺ｫ霑泌唆縺吶ｋ繧ｳ繝槭Φ繝峨う繝ｳ繧ｹ繧ｿ繝ｳ繧ｹ</param>
        void ReturnCommand<T>(T command) where T : ICommand;

        /// <summary>
        /// 謖・ｮ壹＠縺溘さ繝槭Φ繝牙梛縺ｮ繝励・繝ｫ邨ｱ險域ュ蝣ｱ繧貞叙蠕励＠縺ｾ縺・        /// </summary>
        /// <typeparam name="T">邨ｱ險域ュ蝣ｱ繧貞叙蠕励☆繧九さ繝槭Φ繝峨・蝙・/typeparam>
        /// <returns>繧ｳ繝槭Φ繝峨・繝ｼ繝ｫ縺ｮ邨ｱ險域ュ蝣ｱ</returns>
        CommandStatistics GetStatistics<T>() where T : ICommand;

        /// <summary>
        /// 蜈ｨ繧ｳ繝槭Φ繝峨・繝ｼ繝ｫ縺ｮ繝・ヰ繝・げ諠・ｱ繧偵Ο繧ｰ縺ｫ蜃ｺ蜉帙＠縺ｾ縺・        /// </summary>
        void LogDebugInfo();

        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ縺ｮ迥ｶ諷狗｢ｺ隱阪→繝・ヰ繝・げ諠・ｱ繧偵Ο繧ｰ縺ｫ蜃ｺ蜉帙＠縺ｾ縺・        /// </summary>
        void LogServiceStatus();

        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ縺ｮ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・蜃ｦ逅・ｒ螳溯｡後＠縺ｾ縺・        /// </summary>
        void Cleanup();
    }
}
