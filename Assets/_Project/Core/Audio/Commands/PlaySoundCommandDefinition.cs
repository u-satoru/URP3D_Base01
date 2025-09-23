using UnityEngine;
// using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Audio.Data;
// using asterivo.Unity60.Core.Debug;
using Debug = UnityEngine.Debug;

namespace asterivo.Unity60.Core.Audio.Commands
{
    /// <summary>
    /// PlaySoundCommand縺ｮ螳夂ｾｩScriptableObject
    /// 繧ｨ繝・ぅ繧ｿ縺九ｉ繧ｳ繝槭Φ繝峨ｒ險ｭ螳壹・邂｡逅・☆繧九◆繧√・蝓ｺ逶､
    /// </summary>
    [CreateAssetMenu(fileName = "New Play Sound Command", menuName = "asterivo.Unity60/Audio/Commands/Play Sound Command")]
    public class PlaySoundCommandDefinition : ScriptableObject, ICommandDefinition
    {
        [Header("髻ｳ髻ｿ繧ｳ繝槭Φ繝芽ｨｭ螳・)]
        [SerializeField] private SoundDataSO soundData;
        [SerializeField] private AudioEventData defaultAudioData;
        
        [Header("螳溯｡瑚ｨｭ螳・)]
        [SerializeField] private bool usePooling = true;
        [SerializeField] private int poolSize = 10;
        
        public System.Type CommandType => typeof(PlaySoundCommand);
        public bool UsePooling => usePooling;
        public int PoolSize => poolSize;
        
        /// <summary>
        /// ICommandDefinition.CanExecute螳溯｣・        /// </summary>
        public bool CanExecute(object context = null)
        {
            return soundData != null;
        }
        
        /// <summary>
        /// ICommandDefinition.CreateCommand螳溯｣・        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            return CreateCommand();
        }
        
        /// <summary>
        /// 繧ｳ繝槭Φ繝峨う繝ｳ繧ｹ繧ｿ繝ｳ繧ｹ繧剃ｽ懈・
        /// 譁ｰ縺励＞CommandPoolService繧剃ｽｿ逕ｨ
        /// </summary>
        public ICommand CreateCommand()
        {
            PlaySoundCommand command = null;
            
            if (usePooling)
            {
                var poolService = ServiceLocator.GetService<ICommandPoolService>();
                if (poolService != null)
                {
                    command = poolService.GetCommand<PlaySoundCommand>();
                }
                else
                {
                    // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ・夂峩謗･菴懈・
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    ProjectDebug.LogWarning("CommandPoolService not available, creating PlaySoundCommand directly");
#endif
                    command = new PlaySoundCommand();
                }
            }
            else
            {
                command = new PlaySoundCommand();
            }
            
            return command;
        }
        
        /// <summary>
        /// 繝・ヵ繧ｩ繝ｫ繝郁ｨｭ螳壹〒繧ｳ繝槭Φ繝峨ｒ菴懈・
        /// </summary>
        public PlaySoundCommand CreatePlaySoundCommand(AudioSource audioSource, Transform listener = null)
        {
            var command = CreateCommand() as PlaySoundCommand;
            command.Initialize(defaultAudioData, soundData, audioSource, listener);
            return command;
        }
        
        /// <summary>
        /// 繧ｫ繧ｹ繧ｿ繝險ｭ螳壹〒繧ｳ繝槭Φ繝峨ｒ菴懈・
        /// </summary>
        public PlaySoundCommand CreatePlaySoundCommand(AudioEventData customData, AudioSource audioSource, Transform listener = null)
        {
            var command = CreateCommand() as PlaySoundCommand;
            command.Initialize(customData, soundData, audioSource, listener);
            return command;
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            // 繝・ヵ繧ｩ繝ｫ繝亥､縺ｮ險ｭ螳・            if (string.IsNullOrEmpty(defaultAudioData.soundID) && soundData != null)
            {
                defaultAudioData.soundID = soundData.SoundID;
                defaultAudioData.volume = soundData.BaseVolume;
                defaultAudioData.pitch = soundData.BasePitch;
                defaultAudioData.use3D = soundData.Is3D;
                defaultAudioData.hearingRadius = soundData.BaseHearingRadius;
                defaultAudioData.priority = soundData.Priority;
                defaultAudioData.canBemasked = soundData.CanBeMasked;
            }
        }
        #endif
    }
}