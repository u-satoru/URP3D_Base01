using UnityEngine;
using System.Collections;
using asterivo.Unity60.Core.Audio;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Audio.Events;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Player;

namespace asterivo.Unity60.Features.Player.Audio
{
    /// <summary>
    /// プレイヤーの音源管理システム
    /// 足音、インタラクション音、呼吸音などを管理
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerAudioSystem : MonoBehaviour
    {
        [Header("Audio Events")]
        [SerializeField] private AudioEvent playerFootstepEvent;
        [SerializeField] private AudioEvent playerInteractionEvent;
        [SerializeField] private AudioEvent playerBreathingEvent;
        
        [Header("Sound Data")]
        [SerializeField] private SoundDataSO[] footstepSounds;
        [SerializeField] private SoundDataSO[] interactionSounds;
        [SerializeField] private SoundDataSO[] breathingSounds;
        
        [Header("Footstep Settings")]
        [SerializeField] private float footstepInterval = 0.5f;
        [SerializeField] private float minimumMovementSpeed = 0.1f;
        [SerializeField] private LayerMask groundLayerMask = -1;
        [SerializeField] private float groundCheckDistance = 1.1f;
        
        [Header("Surface Detection")]
        [SerializeField] private SurfaceTypeDetector surfaceDetector;
        
        [Header("Movement Speed Modifiers")]
        [SerializeField] private MovementAudioSettings walkingSettings = new MovementAudioSettings(1.0f, 1.0f, 5.0f);
        [SerializeField] private MovementAudioSettings runningSettings = new MovementAudioSettings(1.5f, 1.2f, 8.0f);
        [SerializeField] private MovementAudioSettings crouchingSettings = new MovementAudioSettings(0.3f, 0.8f, 2.0f);
        
        // コンポーネント参照
        private CharacterController characterController;
        private PlayerController playerController;
        
        // 状態管理
        private float lastFootstepTime;
        private bool isMoving;
        private MovementState currentMovementState = MovementState.Walking;
        private SurfaceMaterial currentSurface = SurfaceMaterial.Default;
        
        // 音響計算用
        private Vector3 lastPosition;
        private float currentMovementSpeed;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            playerController = GetComponent<PlayerController>();
            
            if (surfaceDetector == null)
            {
                surfaceDetector = GetComponent<SurfaceTypeDetector>();
            }
        }
        
        private void Start()
        {
            lastPosition = transform.position;
            
            // プレイヤーの移動状態変更イベントをリッスン（存在する場合）
            if (playerController != null)
            {
                // プレイヤーの状態変更イベントに登録
                // 注意: PlayerControllerクラスが状態変更イベントを持っている場合
            }
        }
        
        private void Update()
        {
            UpdateMovementTracking();
            UpdateSurfaceDetection();
            HandleFootsteps();
        }
        
        #endregion
        
        #region Movement Tracking
        
        /// <summary>
        /// 移動状態の追跡と更新
        /// </summary>
        private void UpdateMovementTracking()
        {
            Vector3 currentPosition = transform.position;
            Vector3 movement = currentPosition - lastPosition;
            currentMovementSpeed = movement.magnitude / Time.deltaTime;
            
            // 移動状態の判定
            isMoving = currentMovementSpeed > minimumMovementSpeed && characterController.isGrounded;
            
            // 移動速度に基づく状態判定（PlayerControllerから取得できる場合はそちらを優先）
            if (playerController != null)
            {
                currentMovementState = GetMovementStateFromPlayerController();
            }
            else
            {
                currentMovementState = DetermineMovementStateFromSpeed();
            }
            
            lastPosition = currentPosition;
        }
        
        /// <summary>
        /// 速度から移動状態を推定
        /// </summary>
        private MovementState DetermineMovementStateFromSpeed()
        {
            if (currentMovementSpeed < 1f) return MovementState.Crouching;
            if (currentMovementSpeed > 4f) return MovementState.Running;
            return MovementState.Walking;
        }
        
        /// <summary>
        /// PlayerControllerコンポーネントから移動状態を取得
        /// </summary>
        private MovementState GetMovementStateFromPlayerController()
        {
            // 実際のPlayerControllerの実装に応じて調整
            // 例: return playerController.CurrentMovementState;
            return DetermineMovementStateFromSpeed(); // フォールバック
        }
        
        #endregion
        
        #region Surface Detection
        
        /// <summary>
        /// 足元の表面材質を検出
        /// </summary>
        private void UpdateSurfaceDetection()
        {
            if (surfaceDetector != null)
            {
                currentSurface = surfaceDetector.GetCurrentSurface();
            }
            else
            {
                // 基本的なRaycast検出
                currentSurface = DetectSurfaceWithRaycast();
            }
        }
        
        /// <summary>
        /// Raycastによる表面検出
        /// </summary>
        private SurfaceMaterial DetectSurfaceWithRaycast()
        {
            Vector3 rayStart = transform.position + Vector3.up * 0.5f;
            
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayerMask))
            {
                // タグやマテリアルに基づく表面判定
                return GetSurfaceFromHit(hit);
            }
            
            return SurfaceMaterial.Default;
        }
        
        /// <summary>
        /// RaycastHitから表面材質を判定
        /// </summary>
        private SurfaceMaterial GetSurfaceFromHit(RaycastHit hit)
        {
            // タグベースの判定
            string tag = hit.collider.tag;
            switch (tag)
            {
                case "Concrete": return SurfaceMaterial.Concrete;
                case "Metal": return SurfaceMaterial.Metal;
                case "Wood": return SurfaceMaterial.Wood;
                case "Carpet": return SurfaceMaterial.Carpet;
                case "Grass": return SurfaceMaterial.Grass;
                case "Gravel": return SurfaceMaterial.Gravel;
                case "Water": return SurfaceMaterial.Water;
                default: return SurfaceMaterial.Default;
            }
        }
        
        #endregion
        
        #region Footstep System
        
        /// <summary>
        /// 足音処理のメインループ
        /// </summary>
        private void HandleFootsteps()
        {
            if (!isMoving || !characterController.isGrounded) return;
            
            // 移動状態に応じた足音間隔の調整
            MovementAudioSettings currentSettings = GetCurrentMovementSettings();
            float adjustedInterval = footstepInterval / currentSettings.intervalMultiplier;
            
            if (Time.time - lastFootstepTime >= adjustedInterval)
            {
                PlayFootstep(currentSettings);
                lastFootstepTime = Time.time;
            }
        }
        
        /// <summary>
        /// 現在の移動状態に対応する音響設定を取得
        /// </summary>
        private MovementAudioSettings GetCurrentMovementSettings()
        {
            return currentMovementState switch
            {
                MovementState.Running => runningSettings,
                MovementState.Crouching => crouchingSettings,
                _ => walkingSettings
            };
        }
        
        /// <summary>
        /// 足音を再生
        /// </summary>
        private void PlayFootstep(MovementAudioSettings settings)
        {
            if (footstepSounds == null || footstepSounds.Length == 0 || playerFootstepEvent == null) return;
            
            // 表面材質に適した足音を選択
            SoundDataSO selectedSound = SelectFootstepSoundForSurface();
            
            if (selectedSound != null)
            {
                // AudioEventDataの作成
                AudioEventData footstepData = new AudioEventData
                {
                    soundID = selectedSound.SoundID,
                    worldPosition = transform.position,
                    volume = settings.volumeMultiplier,
                    pitch = settings.pitchMultiplier,
                    use3D = true,
                    sourceType = AudioSourceType.Player,
                    isPlayerGenerated = true,
                    hearingRadius = settings.hearingRadius,
                    surfaceType = currentSurface,
                    canBemasked = true,
                    priority = CalculateFootstepPriority(),
                    timestamp = Time.time
                };
                
                // イベント発火
                playerFootstepEvent.Raise(footstepData);
                
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"<color=green>[PlayerAudio]</color> Footstep on {currentSurface} " +
                         $"(Volume: {footstepData.volume:F2}, Hearing: {footstepData.hearingRadius:F1}m)");
                #endif
            }
        }
        
        /// <summary>
        /// 表面材質に適した足音を選択
        /// </summary>
        private SoundDataSO SelectFootstepSoundForSurface()
        {
            // 表面材質に対応する足音を検索
            foreach (var sound in footstepSounds)
            {
                if (sound.AffectedSurfaces != null)
                {
                    foreach (var surface in sound.AffectedSurfaces)
                    {
                        if (surface == currentSurface)
                        {
                            return sound;
                        }
                    }
                }
            }
            
            // デフォルト足音を返す
            return footstepSounds.Length > 0 ? footstepSounds[0] : null;
        }
        
        /// <summary>
        /// 足音の優先度を計算
        /// </summary>
        private float CalculateFootstepPriority()
        {
            // 移動状態と表面材質に基づく優先度計算
            float basePriority = currentMovementState switch
            {
                MovementState.Running => 0.8f,
                MovementState.Walking => 0.5f,
                MovementState.Crouching => 0.2f,
                _ => 0.5f
            };
            
            // 表面材質による調整
            float surfaceModifier = currentSurface switch
            {
                SurfaceMaterial.Metal => 1.5f,
                SurfaceMaterial.Gravel => 1.3f,
                SurfaceMaterial.Water => 1.2f,
                SurfaceMaterial.Carpet => 0.3f,
                SurfaceMaterial.Grass => 0.5f,
                _ => 1.0f
            };
            
            return Mathf.Clamp01(basePriority * surfaceModifier);
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// インタラクション音を再生
        /// </summary>
        public void PlayInteractionSound(string interactionType)
        {
            if (playerInteractionEvent == null) return;
            
            // インタラクションタイプに応じた音を選択
            SoundDataSO interactionSound = SelectInteractionSound(interactionType);
            
            if (interactionSound != null)
            {
                AudioEventData interactionData = AudioEventData.CreateDefault(interactionSound.SoundID);
                interactionData.worldPosition = transform.position;
                interactionData.sourceType = AudioSourceType.Interactive;
                interactionData.isPlayerGenerated = true;
                
                playerInteractionEvent.Raise(interactionData);
            }
        }
        
        /// <summary>
        /// 呼吸音を再生（緊張状態など）
        /// </summary>
        public void PlayBreathingSound(float intensity = 1f)
        {
            if (playerBreathingEvent == null || breathingSounds == null || breathingSounds.Length == 0) return;
            
            AudioEventData breathingData = AudioEventData.CreateDefault(breathingSounds[0].SoundID);
            breathingData.worldPosition = transform.position;
            breathingData.volume = intensity;
            breathingData.sourceType = AudioSourceType.Player;
            breathingData.hearingRadius = 2f; // 呼吸音は近距離のみ
            
            playerBreathingEvent.Raise(breathingData);
        }
        
        #endregion
        
        #region Private Helpers
        
        /// <summary>
        /// インタラクションタイプに応じた音を選択
        /// </summary>
        private SoundDataSO SelectInteractionSound(string interactionType)
        {
            if (interactionSounds == null) return null;
            
            foreach (var sound in interactionSounds)
            {
                if (sound.SoundID.Contains(interactionType))
                {
                    return sound;
                }
            }
            
            return interactionSounds.Length > 0 ? interactionSounds[0] : null;
        }
        
        #endregion
    }
    
    #region Supporting Classes
    
    /// <summary>
    /// 移動状態の定義
    /// </summary>
    public enum MovementState
    {
        Walking,
        Running,
        Crouching
    }
    
    /// <summary>
    /// 移動状態ごとの音響設定
    /// </summary>
    [System.Serializable]
    public struct MovementAudioSettings
    {
        public float volumeMultiplier;
        public float pitchMultiplier;
        public float hearingRadius;
        public float intervalMultiplier;
        
        public MovementAudioSettings(float volume, float pitch, float hearing, float interval = 1f)
        {
            volumeMultiplier = volume;
            pitchMultiplier = pitch;
            hearingRadius = hearing;
            intervalMultiplier = interval;
        }
    }
    
    #endregion
}
