using UnityEngine;
// using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Audio.Data;
// using asterivo.Unity60.Core.Debug;
using Debug = UnityEngine.Debug;

namespace asterivo.Unity60.Core.Audio.Commands
{
    /// <summary>
    /// PlaySoundCommandの定義ScriptableObject
    /// エチE��タからコマンドを設定�E管琁E��るため�E基盤
    /// </summary>
    [CreateAssetMenu(fileName = "New Play Sound Command", menuName = "asterivo.Unity60/Audio/Commands/Play Sound Command")]
    public class PlaySoundCommandDefinition : ScriptableObject, ICommandDefinition
    {
        [Header("音響コマンド設定")]
        [SerializeField] private SoundDataSO soundData;
        [SerializeField] private AudioEventData defaultAudioData;
        
        [Header("実行設定")]
        [SerializeField] private bool usePooling = true;
        [SerializeField] private int poolSize = 10;
        
        public System.Type CommandType => typeof(PlaySoundCommand);
        public bool UsePooling => usePooling;
        public int PoolSize => poolSize;
        
        /// <summary>
        /// ICommandDefinition.CanExecute実裁E        /// </summary>
        public bool CanExecute(object context = null)
        {
            return soundData != null;
        }
        
        /// <summary>
        /// ICommandDefinition.CreateCommand実裁E        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            return CreateCommand();
        }
        
        /// <summary>
        /// コマンドインスタンスを作�E
        /// 新しいCommandPoolServiceを使用
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
                    // フォールバック�E�直接作�E
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
        /// チE��ォルト設定でコマンドを作�E
        /// </summary>
        public PlaySoundCommand CreatePlaySoundCommand(AudioSource audioSource, Transform listener = null)
        {
            var command = CreateCommand() as PlaySoundCommand;
            command.Initialize(defaultAudioData, soundData, audioSource, listener);
            return command;
        }
        
        /// <summary>
        /// カスタム設定でコマンドを作�E
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
            // デフォルト値の設定
            if (string.IsNullOrEmpty(defaultAudioData.soundID) && soundData != null)
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
