using UnityEngine;

namespace asterivo.Unity60.Core.ScriptableObjects.Data
{
    /// <summary>
    /// デフォルトゲームデータ設定を管理するScriptableObject
    /// プロジェクトの初期値、デフォルト設定、ゲームバランス調整に使用
    /// 
    /// 設計思想:
    /// - ゲームデザイナーがコードを変更せずに調整可能
    /// - 開発環境とプロダクション環境の設定分離
    /// - イベント駆動システムとの統合
    /// - プリセット機能による迅速なテスト環境構築
    /// 
    /// 使用例:
    /// GameDataSettings settings = Resources.Load<GameDataSettings>("DefaultGameData");
    /// GameData gameData = settings.CreateDefaultGameData();
    /// </summary>
    [CreateAssetMenu(fileName = "DefaultGameData", menuName = "Project/Core/Game Data Settings")]
    public class GameDataSettings : ScriptableObject
    {
        [Header("Player Settings")]
        [Tooltip("プレイヤーのデフォルト名")]
        public string defaultPlayerName = "Player";
        
        [Tooltip("開始時のライフ数")]
        [Range(1, 10)]
        public int startingLives = 3;
        
        [Tooltip("開始時のヘルス値")]
        [Range(1f, 1000f)]
        public float startingHealth = 100f;
        
        [Tooltip("最大ヘルス値")]
        [Range(1f, 1000f)]
        public float maxHealth = 100f;
        
        [Tooltip("開始時のスタミナ値")]
        [Range(1f, 1000f)]
        public float startingStamina = 100f;
        
        [Tooltip("最大スタミナ値")]
        [Range(1f, 1000f)]
        public float maxStamina = 100f;
        
        [Header("Game Settings")]
        [Tooltip("開始レベル")]
        [Range(1, 100)]
        public int startingLevel = 1;
        
        [Tooltip("開始スコア")]
        public int startingScore = 0;
        
        [Tooltip("ハイスコア初期値")]
        public int defaultHighScore = 0;
        
        [Header("Physics Settings")]
        [Tooltip("デフォルト重力値")]
        public Vector3 defaultGravity = new Vector3(0f, -9.81f, 0f);
        
        [Tooltip("デフォルト物理タイムステップ")]
        [Range(0.001f, 0.1f)]
        public float defaultFixedTimestep = 0.02f;
        
        [Tooltip("デフォルトの最大角速度")]
        [Range(1f, 50f)]
        public float defaultMaxAngularVelocity = 7f;
        
        [Header("Performance Settings")]
        [Tooltip("デフォルトターゲットフレームレート")]
        [Range(30, 120)]
        public int targetFrameRate = 60;
        
        [Tooltip("垂直同期有効")]
        public bool vSyncEnabled = true;
        
        [Header("Audio Settings")]
        [Tooltip("マスターボリューム")]
        [Range(0f, 1f)]
        public float masterVolume = 1f;
        
        [Tooltip("BGMボリューム")]
        [Range(0f, 1f)]
        public float musicVolume = 0.7f;
        
        [Tooltip("SEボリューム")]
        [Range(0f, 1f)]
        public float sfxVolume = 1f;
        
        [Header("Quality Settings")]
        [Tooltip("デフォルトクオリティレベル")]
        [Range(0, 5)]
        public int defaultQualityLevel = 3;
        
        [Tooltip("アンチエイリアシング")]
        public int antiAliasing = 4;
        
        [Tooltip("テクスチャ品質")]
        [Range(0, 3)]
        public int textureQuality = 0;
        
        /// <summary>
        /// 設定値からデフォルトGameDataを作成
        /// </summary>
        /// <returns>設定値を反映したGameDataインスタンス</returns>
        public asterivo.Unity60.Core.Data.GameData CreateDefaultGameData()
        {
            var gameData = new asterivo.Unity60.Core.Data.GameData
            {
                level = startingLevel,
                gameTime = 0f,
                score = startingScore,
                lives = startingLives,
                highScore = defaultHighScore,
                playerName = defaultPlayerName
            };
            
            // カスタムデータに追加設定を保存
            gameData.customData["audio_master_volume"] = masterVolume;
            gameData.customData["audio_music_volume"] = musicVolume;
            gameData.customData["audio_sfx_volume"] = sfxVolume;
            gameData.customData["quality_level"] = defaultQualityLevel;
            gameData.customData["target_framerate"] = targetFrameRate;
            gameData.customData["vsync_enabled"] = vSyncEnabled;
            gameData.customData["anti_aliasing"] = antiAliasing;
            gameData.customData["texture_quality"] = textureQuality;
            
            return gameData;
        }
        
        /// <summary>
        /// 設定値からデフォルトPlayerDataPayloadを作成
        /// </summary>
        /// <param name="spawnPosition">スポーン位置（デフォルト: Vector3.zero）</param>
        /// <param name="spawnRotation">スポーン回転（デフォルト: Quaternion.identity）</param>
        /// <returns>設定値を反映したPlayerDataPayloadインスタンス</returns>
        public asterivo.Unity60.Core.Data.PlayerDataPayload CreateDefaultPlayerData(Vector3 spawnPosition = default, Quaternion spawnRotation = default)
        {
            if (spawnPosition == default) spawnPosition = Vector3.zero;
            if (spawnRotation == default) spawnRotation = Quaternion.identity;
            
            return new asterivo.Unity60.Core.Data.PlayerDataPayload
            {
                playerName = defaultPlayerName,
                position = spawnPosition,
                rotation = spawnRotation,
                currentHealth = startingHealth,
                maxHealth = maxHealth,
                currentStamina = startingStamina,
                maxStamina = maxStamina,
                score = startingScore
            };
        }
        
        /// <summary>
        /// 物理設定を適用
        /// </summary>
        public void ApplyPhysicsSettings()
        {
            Physics.gravity = defaultGravity;
            Time.fixedDeltaTime = defaultFixedTimestep;
            Physics.defaultMaxAngularSpeed = defaultMaxAngularVelocity;
        }
        
        /// <summary>
        /// パフォーマンス設定を適用
        /// </summary>
        public void ApplyPerformanceSettings()
        {
            Application.targetFrameRate = targetFrameRate;
            QualitySettings.vSyncCount = vSyncEnabled ? 1 : 0;
            QualitySettings.SetQualityLevel(defaultQualityLevel, true);
            QualitySettings.antiAliasing = antiAliasing;
            QualitySettings.globalTextureMipmapLimit = textureQuality;
        }
        
        /// <summary>
        /// 設定値の妥当性を検証
        /// </summary>
        /// <returns>すべての設定が妥当な場合はtrue</returns>
        public bool ValidateSettings()
        {
            bool isValid = true;
            
            if (maxHealth <= 0f)
            {
                UnityEngine.Debug.LogError("MaxHealth must be greater than 0");
                isValid = false;
            }
            
            if (startingHealth > maxHealth)
            {
                UnityEngine.Debug.LogError("StartingHealth cannot exceed MaxHealth");
                isValid = false;
            }
            
            if (maxStamina <= 0f)
            {
                UnityEngine.Debug.LogError("MaxStamina must be greater than 0");
                isValid = false;
            }
            
            if (startingStamina > maxStamina)
            {
                UnityEngine.Debug.LogError("StartingStamina cannot exceed MaxStamina");
                isValid = false;
            }
            
            if (startingLives <= 0)
            {
                UnityEngine.Debug.LogError("StartingLives must be greater than 0");
                isValid = false;
            }
            
            if (targetFrameRate <= 0)
            {
                UnityEngine.Debug.LogError("TargetFrameRate must be greater than 0");
                isValid = false;
            }
            
            return isValid;
        }
        
        private void OnValidate()
        {
            // インスペクターで値が変更された時の自動検証
            ValidateSettings();
            
            // 範囲外の値を自動補正
            startingHealth = Mathf.Clamp(startingHealth, 1f, maxHealth);
            startingStamina = Mathf.Clamp(startingStamina, 1f, maxStamina);
        }
    }
}