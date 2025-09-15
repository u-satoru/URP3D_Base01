using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Platformer
{
    /// <summary>
    /// プラットフォーマーコレクタブル設定クラス
    /// コレクタブル・進行システム・スコア管理
    /// </summary>
    [System.Serializable]
    public class PlatformerCollectibleSettings
    {
        [BoxGroup("Basic Collectibles")]
        [LabelText("コレクタブル有効")]
        [SerializeField] private bool enableCollectibles = true;

        [LabelText("コイン価値"), Range(10, 500)]
        [ShowIf("enableCollectibles")]
        [SerializeField] private int coinValue = 100;

        [LabelText("ジェム価値"), Range(100, 2000)]
        [ShowIf("enableCollectibles")]
        [SerializeField] private int gemValue = 500;

        [LabelText("スペシャルアイテム価値"), Range(500, 5000)]
        [ShowIf("enableCollectibles")]
        [SerializeField] private int specialItemValue = 1000;

        [BoxGroup("Score System")]
        [LabelText("目標スコア"), Range(1000, 50000)]
        [SerializeField] private int targetScore = 10000;

        [LabelText("コンボシステム有効")]
        [SerializeField] private bool enableComboSystem = true;

        [LabelText("コンボ倍率"), Range(1.1f, 3f)]
        [ShowIf("enableComboSystem")]
        [SerializeField] private float comboMultiplier = 1.5f;

        [LabelText("コンボ最大値"), Range(5, 50)]
        [ShowIf("enableComboSystem")]
        [SerializeField] private int maxCombo = 20;

        [LabelText("コンボタイムアウト"), Range(1f, 10f)]
        [ShowIf("enableComboSystem")]
        [SerializeField] private float comboTimeout = 3f;

        [BoxGroup("Magnet System")]
        [LabelText("マグネット効果有効")]
        [SerializeField] private bool enableMagnetEffect = true;

        [LabelText("マグネット範囲"), Range(2f, 15f)]
        [ShowIf("enableMagnetEffect")]
        [SerializeField] private float magnetRange = 5f;

        [LabelText("マグネット力"), Range(5f, 30f)]
        [ShowIf("enableMagnetEffect")]
        [SerializeField] private float magnetForce = 15f;

        [LabelText("マグネット継続時間"), Range(5f, 30f)]
        [ShowIf("enableMagnetEffect")]
        [SerializeField] private float magnetDuration = 10f;

        [BoxGroup("Power-ups")]
        [LabelText("パワーアップ有効")]
        [SerializeField] private bool enablePowerUps = true;

        [LabelText("スピードブースト継続時間"), Range(3f, 20f)]
        [ShowIf("enablePowerUps")]
        [SerializeField] private float speedBoostDuration = 8f;

        [LabelText("スピードブースト倍率"), Range(1.2f, 3f)]
        [ShowIf("enablePowerUps")]
        [SerializeField] private float speedBoostMultiplier = 1.8f;

        [LabelText("ジャンプブースト継続時間"), Range(3f, 20f)]
        [ShowIf("enablePowerUps")]
        [SerializeField] private float jumpBoostDuration = 10f;

        [LabelText("ジャンプブースト倍率"), Range(1.2f, 2.5f)]
        [ShowIf("enablePowerUps")]
        [SerializeField] private float jumpBoostMultiplier = 1.5f;

        [BoxGroup("Progression")]
        [LabelText("進行システム有効")]
        [SerializeField] private bool enableProgressionSystem = true;

        [LabelText("レベル完了に必要なアイテム数"), Range(5, 100)]
        [ShowIf("enableProgressionSystem")]
        [SerializeField] private int requiredItemsForCompletion = 30;

        [LabelText("ボーナスアイテム数"), Range(5, 50)]
        [ShowIf("enableProgressionSystem")]
        [SerializeField] private int bonusItemCount = 15;

        [LabelText("完璧完了ボーナス"), Range(1000, 10000)]
        [ShowIf("enableProgressionSystem")]
        [SerializeField] private int perfectCompletionBonus = 5000;

        [BoxGroup("Visual & Audio")]
        [LabelText("収集エフェクト有効")]
        [SerializeField] private bool enableCollectionEffects = true;

        [LabelText("収集パーティクル有効")]
        [ShowIf("enableCollectionEffects")]
        [SerializeField] private bool enableCollectionParticles = true;

        [LabelText("収集サウンド有効")]
        [SerializeField] private bool enableCollectionSounds = true;

        [LabelText("コンボサウンド有効")]
        [ShowIf("enableComboSystem")]
        [SerializeField] private bool enableComboSounds = true;

        [LabelText("UIアニメーション有効")]
        [SerializeField] private bool enableUIAnimations = true;

        [BoxGroup("Spawn Settings")]
        [LabelText("動的生成有効")]
        [SerializeField] private bool enableDynamicSpawning = true;

        [LabelText("生成間隔"), Range(1f, 10f)]
        [ShowIf("enableDynamicSpawning")]
        [SerializeField] private float spawnInterval = 3f;

        [LabelText("最大同時生成数"), Range(5, 50)]
        [ShowIf("enableDynamicSpawning")]
        [SerializeField] private int maxActiveCollectibles = 25;

        [LabelText("生成範囲"), Range(5f, 30f)]
        [ShowIf("enableDynamicSpawning")]
        [SerializeField] private float spawnRange = 15f;

        [BoxGroup("Difficulty Scaling")]
        [LabelText("難易度スケーリング有効")]
        [SerializeField] private bool enableDifficultyScaling = true;

        [LabelText("スケーリング係数"), Range(1.1f, 2f)]
        [ShowIf("enableDifficultyScaling")]
        [SerializeField] private float scalingFactor = 1.3f;

        [LabelText("最大難易度レベル"), Range(5, 20)]
        [ShowIf("enableDifficultyScaling")]
        [SerializeField] private int maxDifficultyLevel = 10;

        #region Public Properties
        public bool EnableCollectibles => enableCollectibles;
        public int CoinValue => coinValue;
        public int GemValue => gemValue;
        public int SpecialItemValue => specialItemValue;
        public int TargetScore => targetScore;
        public bool EnableComboSystem => enableComboSystem;
        public float ComboMultiplier => comboMultiplier;
        public int MaxCombo => maxCombo;
        public float ComboTimeout => comboTimeout;
        public bool EnableMagnetEffect => enableMagnetEffect;
        public float MagnetRange => magnetRange;
        public float MagnetForce => magnetForce;
        public float MagnetDuration => magnetDuration;
        public bool EnablePowerUps => enablePowerUps;
        public float SpeedBoostDuration => speedBoostDuration;
        public float SpeedBoostMultiplier => speedBoostMultiplier;
        public float JumpBoostDuration => jumpBoostDuration;
        public float JumpBoostMultiplier => jumpBoostMultiplier;
        public bool EnableProgressionSystem => enableProgressionSystem;
        public int RequiredItemsForCompletion => requiredItemsForCompletion;
        public int BonusItemCount => bonusItemCount;
        public int PerfectCompletionBonus => perfectCompletionBonus;
        public bool EnableCollectionEffects => enableCollectionEffects;
        public bool EnableCollectionParticles => enableCollectionParticles;
        public bool EnableCollectionSounds => enableCollectionSounds;
        public bool EnableComboSounds => enableComboSounds;
        public bool EnableUIAnimations => enableUIAnimations;
        public bool EnableDynamicSpawning => enableDynamicSpawning;
        public float SpawnInterval => spawnInterval;
        public int MaxActiveCollectibles => maxActiveCollectibles;
        public float SpawnRange => spawnRange;
        public bool EnableDifficultyScaling => enableDifficultyScaling;
        public float ScalingFactor => scalingFactor;
        public int MaxDifficultyLevel => maxDifficultyLevel;
        #endregion

        #region Initialization & Validation
        public void Initialize()
        {
            // コレクタブル設定の妥当性確認
            coinValue = Mathf.Clamp(coinValue, 10, 500);
            gemValue = Mathf.Clamp(gemValue, 100, 2000);
            specialItemValue = Mathf.Clamp(specialItemValue, 500, 5000);
            targetScore = Mathf.Clamp(targetScore, 1000, 50000);

            Debug.Log($"[PlatformerCollectibles] Initialized: Target={targetScore}, Combo={enableComboSystem}, Magnet={enableMagnetEffect}");
        }

        public bool Validate()
        {
            bool isValid = true;

            // 基本値検証
            if (coinValue <= 0 || gemValue <= 0 || specialItemValue <= 0)
            {
                Debug.LogError("[PlatformerCollectibles] All collectible values must be positive");
                isValid = false;
            }

            // 価値階層検証
            if (gemValue <= coinValue || specialItemValue <= gemValue)
            {
                Debug.LogError("[PlatformerCollectibles] Collectible values must follow hierarchy: Coin < Gem < Special");
                isValid = false;
            }

            // 目標スコア検証
            if (targetScore <= 0)
            {
                Debug.LogError("[PlatformerCollectibles] Target score must be positive");
                isValid = false;
            }

            // コンボシステム検証
            if (enableComboSystem)
            {
                if (comboMultiplier <= 1f)
                {
                    Debug.LogError("[PlatformerCollectibles] Combo multiplier must be greater than 1");
                    isValid = false;
                }

                if (maxCombo <= 1)
                {
                    Debug.LogError("[PlatformerCollectibles] Max combo must be greater than 1");
                    isValid = false;
                }

                if (comboTimeout <= 0)
                {
                    Debug.LogError("[PlatformerCollectibles] Combo timeout must be positive");
                    isValid = false;
                }
            }

            // マグネット設定検証
            if (enableMagnetEffect)
            {
                if (magnetRange <= 0 || magnetForce <= 0 || magnetDuration <= 0)
                {
                    Debug.LogError("[PlatformerCollectibles] Magnet settings must be positive");
                    isValid = false;
                }
            }

            // パワーアップ設定検証
            if (enablePowerUps)
            {
                if (speedBoostMultiplier <= 1f || jumpBoostMultiplier <= 1f)
                {
                    Debug.LogError("[PlatformerCollectibles] Power-up multipliers must be greater than 1");
                    isValid = false;
                }

                if (speedBoostDuration <= 0 || jumpBoostDuration <= 0)
                {
                    Debug.LogError("[PlatformerCollectibles] Power-up durations must be positive");
                    isValid = false;
                }
            }

            // 進行システム検証
            if (enableProgressionSystem)
            {
                if (requiredItemsForCompletion <= 0)
                {
                    Debug.LogError("[PlatformerCollectibles] Required items for completion must be positive");
                    isValid = false;
                }
            }

            return isValid;
        }

        public void ApplyRecommendedSettings()
        {
            // 15分ゲームプレイ最適化設定
            enableCollectibles = true;
            coinValue = 100;                  // 標準コイン価値
            gemValue = 500;                   // 高価値ジェム
            specialItemValue = 1500;          // 特別アイテム

            targetScore = 12000;              // 15分で達成可能
            enableComboSystem = true;
            comboMultiplier = 1.6f;           // 魅力的なコンボ
            maxCombo = 15;                    // 適度な最大値
            comboTimeout = 2.5f;              // 寛容なタイミング

            enableMagnetEffect = true;
            magnetRange = 6f;                 // 使いやすい範囲
            magnetForce = 18f;                // 快適な吸引力
            magnetDuration = 12f;             // 十分な持続時間

            enablePowerUps = true;
            speedBoostDuration = 10f;         // 適度な持続時間
            speedBoostMultiplier = 2f;        // 明確な効果
            jumpBoostDuration = 12f;          // やや長めの効果
            jumpBoostMultiplier = 1.6f;       // バランス良い強化

            enableProgressionSystem = true;
            requiredItemsForCompletion = 35;  // 適度な難易度
            bonusItemCount = 20;              // 挑戦的ボーナス
            perfectCompletionBonus = 8000;    // 高いインセンティブ

            // 全エフェクト有効
            enableCollectionEffects = true;
            enableCollectionParticles = true;
            enableCollectionSounds = true;
            enableComboSounds = true;
            enableUIAnimations = true;

            enableDynamicSpawning = true;
            spawnInterval = 2.5f;             // 程よい生成頻度
            maxActiveCollectibles = 30;       // 十分な数
            spawnRange = 18f;                 // 適度な範囲

            enableDifficultyScaling = true;
            scalingFactor = 1.25f;            // 緩やかなスケーリング
            maxDifficultyLevel = 8;           // 適度な上限

            Debug.Log("[PlatformerCollectibles] Applied recommended settings for engaging collection gameplay");
        }
        #endregion

        #region Score Calculations
        /// <summary>
        /// コンボ適用後のスコア計算
        /// </summary>
        /// <param name="baseScore">基本スコア</param>
        /// <param name="currentCombo">現在のコンボ数</param>
        /// <returns>最終スコア</returns>
        public int CalculateScoreWithCombo(int baseScore, int currentCombo)
        {
            if (!enableComboSystem || currentCombo <= 1)
                return baseScore;

            float comboBonus = Mathf.Pow(comboMultiplier, Mathf.Min(currentCombo - 1, maxCombo));
            return Mathf.RoundToInt(baseScore * comboBonus);
        }

        /// <summary>
        /// 完了率の計算
        /// </summary>
        /// <param name="collectedItems">収集アイテム数</param>
        /// <param name="includeBonus">ボーナスアイテム含むか</param>
        /// <returns>完了率（0-1）</returns>
        public float CalculateCompletionRate(int collectedItems, bool includeBonus = false)
        {
            if (!enableProgressionSystem) return 1f;

            int totalRequired = includeBonus ?
                requiredItemsForCompletion + bonusItemCount :
                requiredItemsForCompletion;

            return Mathf.Clamp01((float)collectedItems / totalRequired);
        }

        /// <summary>
        /// 難易度レベルの計算
        /// </summary>
        /// <param name="currentScore">現在のスコア</param>
        /// <returns>難易度レベル</returns>
        public int CalculateDifficultyLevel(int currentScore)
        {
            if (!enableDifficultyScaling) return 1;

            float scoreRatio = (float)currentScore / targetScore;
            int level = Mathf.FloorToInt(scoreRatio * scalingFactor) + 1;
            return Mathf.Clamp(level, 1, maxDifficultyLevel);
        }

        /// <summary>
        /// アイテム価値の取得（タイプ別）
        /// </summary>
        /// <param name="itemType">アイテムタイプ</param>
        /// <returns>アイテム価値</returns>
        public int GetItemValue(CollectibleType itemType)
        {
            return itemType switch
            {
                CollectibleType.Coin => coinValue,
                CollectibleType.Gem => gemValue,
                CollectibleType.SpecialItem => specialItemValue,
                _ => 0
            };
        }

        /// <summary>
        /// マグネット影響範囲内判定
        /// </summary>
        /// <param name="playerPosition">プレイヤー位置</param>
        /// <param name="itemPosition">アイテム位置</param>
        /// <returns>影響範囲内かどうか</returns>
        public bool IsWithinMagnetRange(Vector3 playerPosition, Vector3 itemPosition)
        {
            if (!enableMagnetEffect) return false;

            float distance = Vector3.Distance(playerPosition, itemPosition);
            return distance <= magnetRange;
        }

        /// <summary>
        /// マグネット力の計算
        /// </summary>
        /// <param name="distance">距離</param>
        /// <returns>適用マグネット力</returns>
        public float CalculateMagnetForce(float distance)
        {
            if (!enableMagnetEffect || distance > magnetRange) return 0f;

            // 距離に反比例する磁力（近いほど強い）
            float normalizedDistance = distance / magnetRange;
            return magnetForce * (1f - normalizedDistance);
        }
        #endregion

        #region Spawn System
        /// <summary>
        /// 生成間隔の計算（難易度調整）
        /// </summary>
        /// <param name="difficultyLevel">難易度レベル</param>
        /// <returns>調整後生成間隔</returns>
        public float CalculateAdjustedSpawnInterval(int difficultyLevel)
        {
            if (!enableDynamicSpawning || !enableDifficultyScaling)
                return spawnInterval;

            // 難易度が上がるほど生成間隔が短くなる
            float adjustment = 1f / Mathf.Pow(scalingFactor, difficultyLevel - 1);
            return spawnInterval * adjustment;
        }

        /// <summary>
        /// 生成位置の計算
        /// </summary>
        /// <param name="playerPosition">プレイヤー位置</param>
        /// <param name="excludeNearPlayer">プレイヤー近辺除外</param>
        /// <returns>生成位置</returns>
        public Vector3 CalculateSpawnPosition(Vector3 playerPosition, bool excludeNearPlayer = true)
        {
            if (!enableDynamicSpawning) return Vector3.zero;

            Vector3 spawnPosition;
            int attempts = 0;
            float minDistanceFromPlayer = excludeNearPlayer ? 3f : 0f;

            do
            {
                // ランダムな位置を生成
                Vector2 randomCircle = Random.insideUnitCircle * spawnRange;
                spawnPosition = playerPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
                attempts++;

                // 最大試行回数に達したら強制終了
                if (attempts >= 10) break;

            } while (excludeNearPlayer &&
                     Vector3.Distance(spawnPosition, playerPosition) < minDistanceFromPlayer);

            return spawnPosition;
        }
        #endregion

        #region Editor Support
#if UNITY_EDITOR
        [Button("Test Score Calculation")]
        [PropertySpace(10)]
        public void TestScoreCalculation()
        {
            int[] testCombos = { 1, 3, 5, 10, 15 };
            foreach (int combo in testCombos)
            {
                int score = CalculateScoreWithCombo(coinValue, combo);
                Debug.Log($"Combo {combo}: Score = {score} (Base: {coinValue})");
            }
        }

        [Button("Test Completion Rate")]
        public void TestCompletionRate()
        {
            int[] testItems = { 10, 20, 35, 50, 55 };
            foreach (int items in testItems)
            {
                float rate = CalculateCompletionRate(items, false);
                float bonusRate = CalculateCompletionRate(items, true);
                Debug.Log($"Items {items}: Basic = {rate:P}, With Bonus = {bonusRate:P}");
            }
        }

        [Button("Validate Collectible Settings")]
        public void EditorValidate()
        {
            bool isValid = Validate();
            string message = isValid ?
                "✅ Collectible settings are valid!" :
                "❌ Collectible settings validation failed!";
            Debug.Log($"[PlatformerCollectibles] {message}");
        }
#endif
        #endregion
    }

    /// <summary>
    /// コレクタブルアイテムタイプ
    /// </summary>
    public enum CollectibleType
    {
        Coin,
        Gem,
        SpecialItem,
        PowerUp,
        HealthBonus,
        ScoreMultiplier
    }
}