using UnityEngine;
// using asterivo.Unity60.Core.Helpers;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Services; // Temporarily commented to avoid circular dependency
using Debug = UnityEngine.Debug;

namespace asterivo.Unity60.Core.Commands
{
    
    /// CommandPoolManager縺ｮ繧ｵ繝ｼ繝薙せ繝ｩ繝・ヱ繝ｼ繧ｯ繝ｩ繧ｹ
    /// ServiceLocator繝代ち繝ｼ繝ｳ縺ｧ繧ｳ繝槭Φ繝峨・繝ｼ繝ｫ縺ｸ縺ｮ繧｢繧ｯ繧ｻ繧ｹ繧呈署萓帙☆繧・    /// 
    /// 險ｭ險域晄Φ:
    /// - ObjectPool繝代ち繝ｼ繝ｳ縺ｫ繧医ｋ繝｡繝｢繝ｪ蜉ｹ邇・喧・・5%縺ｮ繝｡繝｢繝ｪ蜑頑ｸ帛柑譫懶ｼ・    /// - 繧ｳ繝槭Φ繝峨が繝悶ず繧ｧ繧ｯ繝医・蜀榊茜逕ｨ縺ｧ繧ｬ繝吶・繧ｸ繧ｳ繝ｬ繧ｯ繧ｷ繝ｧ繝ｳ繧貞炎貂・    /// - Unity MonoBehaviour縺ｮ繝ｩ繧､繝輔し繧､繧ｯ繝ｫ縺ｫ邨ｱ蜷医＆繧後◆螳牙・縺ｪ繧ｵ繝ｼ繝薙せ邂｡逅・    /// - ServiceLocator繝代ち繝ｼ繝ｳ縺ｫ繧医ｋ萓晏ｭ俶ｧ豕ｨ蜈･蟇ｾ蠢・    /// - 蠕梧婿莠呈鋤諤ｧ繧堤ｶｭ謖√＠縺ｪ縺後ｉ谿ｵ髫守噪遘ｻ陦後ｒ謾ｯ謠ｴ
    /// 
    /// 謗ｨ螂ｨ菴ｿ逕ｨ萓・
    /// var service = ServiceLocator.GetService&lt;ICommandPoolService&gt;();
    /// var damageCommand = service.GetCommand&lt;DamageCommand&gt;();
    /// // 繧ｳ繝槭Φ繝我ｽｿ逕ｨ蠕・    /// service.ReturnCommand(damageCommand);
    /// </summary>
    public class CommandPoolService : MonoBehaviour, ICommandPoolService, IInitializable
    {
        [Header("Pool Service Settings")]
        /// <summary>繝・ヰ繝・げ邨ｱ險域ュ蝣ｱ縺ｮ譛牙柑蛹悶ヵ繝ｩ繧ｰ</summary>
        [SerializeField] private bool enableDebugStats = true;
        
        /// <summary>Awake譎ゅ・閾ｪ蜍募・譛溷喧繝輔Λ繧ｰ</summary>
        [SerializeField] private bool autoRegisterOnAwake = true;
        
        /// <summary>繧ｳ繝槭Φ繝峨・繝ｼ繝ｫ縺ｮ螳滄圀縺ｮ邂｡逅・ｒ陦後≧繝槭ロ繝ｼ繧ｸ繝｣繝ｼ</summary>
        private CommandPoolManager _poolManager;
        
        /// <summary>蛻晄悄蛹也憾諷九ヵ繝ｩ繧ｰ</summary>
        private bool _isInitialized = false;
        
        // 笨・ServiceLocator遘ｻ陦・ Legacy Singleton隴ｦ蜻翫す繧ｹ繝・Β・亥ｾ梧婿莠呈鋤諤ｧ縺ｮ縺溘ａ・・        


        
        /// <summary>
        /// CommandPoolManager縺ｸ縺ｮ逶ｴ謗･繧｢繧ｯ繧ｻ繧ｹ
        /// 鬮伜ｺｦ縺ｪ蛻ｶ蠕｡繧・き繧ｹ繧ｿ繝繝励・繝ｫ謫堺ｽ懊↓菴ｿ逕ｨ
        /// </summary>
        /// <returns>蜀・Κ縺ｧ邂｡逅・＆繧後※縺・ｋCommandPoolManager縺ｮ繧､繝ｳ繧ｹ繧ｿ繝ｳ繧ｹ</returns>
        public CommandPoolManager PoolManager => _poolManager;
        
        /// <summary>
        /// 蛻晄悄蛹門━蜈亥ｺｦ・・Initializable繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ螳溯｣・ｼ・        /// CommandPoolService縺ｯ蝓ｺ逶､繧ｵ繝ｼ繝薙せ縺ｪ縺ｮ縺ｧ譌ｩ譛溷・譛溷喧繧定ｨｭ螳・        /// </summary>
        public int Priority => 10;
        
        /// <summary>
        /// 蛻晄悄蛹悶′螳御ｺ・＠縺溘°縺ｩ縺・°・・Initializable繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ螳溯｣・ｼ・        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        private void Awake()
        {
            InitializeService();
            
            // ServiceLocator縺ｸ縺ｮ逋ｻ骭ｲ
            if (autoRegisterOnAwake)
            {
                RegisterToServiceLocator();
                LogServiceStatus();
            }
            
            DontDestroyOnLoad(gameObject);
        }
        
        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ縺ｮ蜀・Κ蛻晄悄蛹門・逅・        /// CommandPoolManager縺ｮ繧､繝ｳ繧ｹ繧ｿ繝ｳ繧ｹ菴懈・縺ｨ蛻晄悄蛹悶ｒ陦後≧
        /// </summary>
        /// <remarks>
        /// Awake繧ｿ繧､繝溘Φ繧ｰ縺ｧ閾ｪ蜍募ｮ溯｡後＆繧後ｋ
        /// 繝・ヰ繝・げ邨ｱ險域怏蜉ｹ譎ゅ・邨ｱ險域ュ蝣ｱ縺ｮ蜿朱寔縺碁幕蟋九＆繧後ｋ
        /// </remarks>
        private void InitializeService()
        {
            _poolManager = new CommandPoolManager(enableDebugStats);
            _poolManager.Initialize();
            _isInitialized = true;
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("CommandPoolService initialized with modern CommandPoolManager");
#endif
        }
        
        
        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ縺ｮ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・蜃ｦ逅・ｒ螳溯｡後＠縺ｾ縺・        /// 蜈ｨ縺ｦ縺ｮ繧ｳ繝槭Φ繝峨・繝ｼ繝ｫ繧偵け繝ｪ繧｢縺励√Μ繧ｽ繝ｼ繧ｹ繧定ｧ｣謾ｾ縺吶ｋ
        /// </summary>
        /// <remarks>
        /// GameObject縺檎ｴ譽・＆繧後ｋ髫帙↓閾ｪ蜍募ｮ溯｡後＆繧後ｋ
        /// 謇句虚縺ｧ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・縺悟ｿ・ｦ√↑蝣ｴ蜷医↓繧ゆｽｿ逕ｨ蜿ｯ閭ｽ
        /// </remarks>
        public void Cleanup()
        {
            _poolManager?.Cleanup();
            _isInitialized = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("CommandPoolService cleaned up");
#endif
        }
        
        #region Service Registration
        
        /// <summary>
        /// IInitializable螳溯｣・ 繧ｵ繝ｼ繝薙せ縺ｮ蛻晄悄蛹門・逅・        /// ServiceLocator縺九ｉ蜻ｼ縺ｳ蜃ｺ縺輔ｌ繧区ｨ呎ｺ也噪縺ｪ蛻晄悄蛹悶Γ繧ｽ繝・ラ
        /// </summary>
        public void Initialize()
        {
            if (_poolManager == null)
            {
                InitializeService();
            }
            RegisterToServiceLocator();
            _isInitialized = true;
        }

        /// <summary>
        /// ServiceLocator縺ｸ縺ｮ繧ｵ繝ｼ繝薙せ逋ｻ骭ｲ
        /// ICommandPoolService繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ縺ｨ縺励※閾ｪ霄ｫ繧堤匳骭ｲ
        /// </summary>
        private void RegisterToServiceLocator()
        {
            try
            {
                ServiceLocator.RegisterService<ICommandPoolService>(this);
                
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log("CommandPoolService registered to ServiceLocator as ICommandPoolService");
#endif
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to register CommandPoolService: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ縺ｮ迥ｶ諷狗｢ｺ隱阪→繝・ヰ繝・げ諠・ｱ繧偵Ο繧ｰ縺ｫ蜃ｺ蜉帙＠縺ｾ縺・        /// 髢狗匱譎ゅ・蝠城｡瑚ｨｺ譁ｭ繧・し繝ｼ繝薙せ迥ｶ諷九・遒ｺ隱阪↓菴ｿ逕ｨ
        /// </summary>
        /// <remarks>
        /// UNITY_EDITOR 縺ｾ縺溘・ DEVELOPMENT_BUILD 縺ｧ縺ｮ縺ｿ螳溯｡後＆繧後ｋ
        /// 繝励・繝ｫ邂｡逅・・迥ｶ諷九→繧ｵ繝ｼ繝薙せ遞ｼ蜒咲憾豕√ｒ遒ｺ隱榊庄閭ｽ
        /// </remarks>
        public void LogServiceStatus()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"CommandPoolService is active. Pool manager initialized: {_poolManager != null}");
            var isRegistered = ServiceLocator.IsServiceRegistered<ICommandPoolService>();
            UnityEngine.Debug.Log($"CommandPoolService ServiceLocator registration: {isRegistered}");
#endif
        }
        
        #endregion
        
        #region Convenient Access Methods
        
        /// <summary>
        /// 謖・ｮ壹＠縺溷梛縺ｮ繧ｳ繝槭Φ繝峨ｒ繝励・繝ｫ縺九ｉ蜿門ｾ励＠縺ｾ縺・        /// 繝励・繝ｫ縺檎ｩｺ縺ｮ蝣ｴ蜷医・譁ｰ縺励＞繧､繝ｳ繧ｹ繧ｿ繝ｳ繧ｹ繧剃ｽ懈・
        /// </summary>
        /// <typeparam name="T">蜿門ｾ励☆繧九さ繝槭Φ繝峨・蝙九・Command繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ繧貞ｮ溯｣・＠縲√ヱ繝ｩ繝｡繝ｼ繧ｿ縺ｪ縺励さ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ繧呈戟縺､蠢・ｦ√′縺ゅｋ</typeparam>
        /// <returns>菴ｿ逕ｨ蜿ｯ閭ｽ縺ｪ繧ｳ繝槭Φ繝峨う繝ｳ繧ｹ繧ｿ繝ｳ繧ｹ</returns>
        /// <remarks>
        /// ObjectPool繝代ち繝ｼ繝ｳ縺ｫ繧医ｊ繝｡繝｢繝ｪ蜉ｹ邇・喧繧貞ｮ溽樟
        /// 菴ｿ逕ｨ蠕後・蠢・★ReturnCommand縺ｧ繝励・繝ｫ縺ｫ霑泌唆縺吶ｋ縺薙→
        /// </remarks>
        /// <example>
        /// var damageCmd = GetCommand&lt;DamageCommand&gt;();
        /// damageCmd.Initialize(target, damage);
        /// damageCmd.Execute();
        /// ReturnCommand(damageCmd);
        /// </example>
        public T GetCommand<T>() where T : class, ICommand, new()
        {
            return _poolManager.GetCommand<T>();
        }
        
        /// <summary>
        /// 菴ｿ逕ｨ螳御ｺ・＠縺溘さ繝槭Φ繝峨ｒ繝励・繝ｫ縺ｫ霑泌唆縺励∪縺・        /// 繧ｳ繝槭Φ繝峨・迥ｶ諷九ｒ繝ｪ繧ｻ繝・ヨ縺励※蜀榊茜逕ｨ蜿ｯ閭ｽ縺ｫ縺吶ｋ
        /// </summary>
        /// <typeparam name="T">霑泌唆縺吶ｋ繧ｳ繝槭Φ繝峨・蝙・/typeparam>
        /// <param name="command">繝励・繝ｫ縺ｫ霑泌唆縺吶ｋ繧ｳ繝槭Φ繝峨う繝ｳ繧ｹ繧ｿ繝ｳ繧ｹ</param>
        /// <remarks>
        /// IResettableCommand繧貞ｮ溯｣・＠縺ｦ縺・ｋ蝣ｴ蜷医ヽeset()繝｡繧ｽ繝・ラ縺瑚・蜍募ｮ溯｡後＆繧後ｋ
        /// null縺ｾ縺溘・譌｢縺ｫ霑泌唆貂医∩縺ｮ繧ｳ繝槭Φ繝峨・辟｡隕悶＆繧後ｋ
        /// </remarks>
        public void ReturnCommand<T>(T command) where T : ICommand
        {
            _poolManager.ReturnCommand(command);
        }
        
        /// <summary>
        /// 謖・ｮ壹＠縺溘さ繝槭Φ繝牙梛縺ｮ繝励・繝ｫ邨ｱ險域ュ蝣ｱ繧貞叙蠕励＠縺ｾ縺・        /// 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ逶｣隕悶ｄ繝｡繝｢繝ｪ菴ｿ逕ｨ驥上・譛驕ｩ蛹悶↓菴ｿ逕ｨ
        /// </summary>
        /// <typeparam name="T">邨ｱ險域ュ蝣ｱ繧貞叙蠕励☆繧九さ繝槭Φ繝峨・蝙・/typeparam>
        /// <returns>繧ｳ繝槭Φ繝峨・繝ｼ繝ｫ縺ｮ邨ｱ險域ュ蝣ｱ・井ｽ懈・謨ｰ縲√・繝ｼ繝ｫ謨ｰ縲∽ｽｿ逕ｨ荳ｭ謨ｰ縺ｪ縺ｩ・・/returns>
        /// <remarks>
        /// 繝・ヰ繝・げ邨ｱ險医′譛牙柑縺ｪ蝣ｴ蜷医・縺ｿ豁｣遒ｺ縺ｪ諠・ｱ繧呈署萓・        /// 辟｡蜉ｹ縺ｪ蝣ｴ蜷医・蝓ｺ譛ｬ諠・ｱ縺ｮ縺ｿ蜿門ｾ怜庄閭ｽ
        /// </remarks>
        public CommandStatistics GetStatistics<T>() where T : ICommand
        {
            return _poolManager.GetStatistics<T>();
        }
        
        /// <summary>
        /// 蜈ｨ繧ｳ繝槭Φ繝峨・繝ｼ繝ｫ縺ｮ繝・ヰ繝・げ諠・ｱ繧偵Ο繧ｰ縺ｫ蜃ｺ蜉帙＠縺ｾ縺・        /// 蜷・・繝ｼ繝ｫ縺ｮ菴ｿ逕ｨ迥ｶ豕√∫ｵｱ險域ュ蝣ｱ縲√Γ繝｢繝ｪ菴ｿ逕ｨ驥上ｒ遒ｺ隱榊庄閭ｽ
        /// </summary>
        /// <remarks>
        /// 髢狗匱繝薙Ν繝峨〒縺ｮ縺ｿ螳溯｡後＆繧後ｋ
        /// 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ蝠城｡後・迚ｹ螳壹ｄ繝励・繝ｫ繧ｵ繧､繧ｺ縺ｮ隱ｿ謨ｴ縺ｫ菴ｿ逕ｨ
        /// </remarks>
        public void LogDebugInfo()
        {
            _poolManager?.LogDebugInfo();
        }
        
        #endregion
        
        #region Static Access (Legacy Support)
        
        /// <summary>
        /// 髱咏噪繧｢繧ｯ繧ｻ繧ｹ逕ｨ縺ｮ繧ｳ繝槭Φ繝牙叙蠕励Γ繧ｽ繝・ラ・医Ξ繧ｬ繧ｷ繝ｼ莠呈鋤諤ｧ逕ｨ・・        /// 繧ｵ繝ｼ繝薙せ繧､繝ｳ繧ｹ繧ｿ繝ｳ繧ｹ縺悟茜逕ｨ縺ｧ縺阪↑縺・ｴ蜷医・繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ讖溯・莉倥″
        /// </summary>
        /// <typeparam name="T">蜿門ｾ励☆繧九さ繝槭Φ繝峨・蝙・/typeparam>
        /// <returns>繧ｳ繝槭Φ繝峨う繝ｳ繧ｹ繧ｿ繝ｳ繧ｹ</returns>
        /// <remarks>
        /// 繧ｵ繝ｼ繝薙せ縺悟・譛溷喧縺輔ｌ縺ｦ縺・↑縺・ｴ蜷医・譁ｰ縺励＞繧､繝ｳ繧ｹ繧ｿ繝ｳ繧ｹ繧剃ｽ懈・
        /// 縺薙・蝣ｴ蜷医・繝励・繝ｫ讖溯・縺ｯ蛻ｩ逕ｨ縺輔ｌ縺ｪ縺・◆繧√√ヱ繝輔か繝ｼ繝槭Φ繧ｹ荳翫・蛻ｩ轤ｹ縺ｯ蠕励ｉ繧後↑縺・        /// 蜿ｯ閭ｽ縺ｪ髯舌ｊServiceLocator邨檎罰縺ｧ縺ｮ繧｢繧ｯ繧ｻ繧ｹ繧呈耳螂ｨ
        /// </remarks>
        [System.Obsolete("Use ServiceLocator.GetService<ICommandPoolService>().GetCommand<T>() instead")]
        public static T GetCommandStatic<T>() where T : class, ICommand, new()
        {
            var service = ServiceLocator.GetService<ICommandPoolService>();
            if (service != null)
            {
                return service.GetCommand<T>();
            }
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.LogWarning("CommandPoolService not available, creating new command instance");
#endif
            return new T();
        }
        
        /// <summary>
        /// 髱咏噪繧｢繧ｯ繧ｻ繧ｹ逕ｨ縺ｮ繧ｳ繝槭Φ繝芽ｿ泌唆繝｡繧ｽ繝・ラ・医Ξ繧ｬ繧ｷ繝ｼ莠呈鋤諤ｧ逕ｨ・・        /// 繧ｵ繝ｼ繝薙せ繧､繝ｳ繧ｹ繧ｿ繝ｳ繧ｹ縺悟茜逕ｨ縺ｧ縺阪↑縺・ｴ蜷医・菴輔ｂ螳溯｡後＠縺ｪ縺・        /// </summary>
        /// <typeparam name="T">霑泌唆縺吶ｋ繧ｳ繝槭Φ繝峨・蝙・/typeparam>
        /// <param name="command">繝励・繝ｫ縺ｫ霑泌唆縺吶ｋ繧ｳ繝槭Φ繝峨う繝ｳ繧ｹ繧ｿ繝ｳ繧ｹ</param>
        /// <remarks>
        /// 繧ｵ繝ｼ繝薙せ縺悟・譛溷喧縺輔ｌ縺ｦ縺・↑縺・ｴ蜷医・繧ｳ繝槭Φ繝峨ｒ遐ｴ譽・        /// 隴ｦ蜻翫Ο繧ｰ縺悟・蜉帙＆繧後ｋ縺溘ａ縲・幕逋ｺ譎ゅ↓蝠城｡後ｒ迚ｹ螳壼庄閭ｽ
        /// </remarks>
        [System.Obsolete("Use ServiceLocator.GetService<ICommandPoolService>().ReturnCommand<T>() instead")]
        public static void ReturnCommandStatic<T>(T command) where T : ICommand
        {
            var service = ServiceLocator.GetService<ICommandPoolService>();
            if (service != null)
            {
                service.ReturnCommand(command);
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("CommandPoolService not available, cannot return command to pool");
#endif
            }
        }
        
        #endregion
        
        private void OnDestroy()
        {
            // ServiceLocator縺九ｉ縺ｮ逋ｻ骭ｲ隗｣髯､
            try
            {
                ServiceLocator.UnregisterService<ICommandPoolService>();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log("CommandPoolService unregistered from ServiceLocator");
#endif
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to unregister CommandPoolService: {ex.Message}");
            }
            
            Cleanup();
        }
    }
}
