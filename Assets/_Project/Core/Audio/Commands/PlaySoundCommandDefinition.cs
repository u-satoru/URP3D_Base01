using UnityEngine;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Audio.Data;

namespace asterivo.Unity60.Core.Audio.Commands
{
    /// <summary>
    /// PlaySoundCommandの定義ScriptableObject
    /// エディタからコマンドを設定・管理するための基盤
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
        /// ICommandDefinition.CanExecute実装
        /// </summary>
        public bool CanExecute(object context = null)
        {
            return soundData != null;
        }
        
        /// <summary>
        /// ICommandDefinition.CreateCommand実装
        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            return CreateCommand();
        }
        
        /// <summary>
        /// コマンドインスタンスを作成
        /// </summary>
        public ICommand CreateCommand()
        {
            var command = usePooling ? 
                CommandPool.Instance.GetCommand<PlaySoundCommand>() : 
                new PlaySoundCommand();
            
            return command;
        }
        
        /// <summary>
        /// デフォルト設定でコマンドを作成
        /// </summary>
        public PlaySoundCommand CreatePlaySoundCommand(AudioSource audioSource, Transform listener = null)
        {
            var command = CreateCommand() as PlaySoundCommand;
            command.Initialize(defaultAudioData, soundData, audioSource, listener);
            return command;
        }
        
        /// <summary>
        /// カスタム設定でコマンドを作成
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