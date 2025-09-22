using UnityEngine;
using asterivo.Unity60.Features.Templates.FPS.Data;
using asterivo.Unity60.Features.Templates.FPS.Player;
using asterivo.Unity60.Features.Templates.FPS.Configuration;

namespace asterivo.Unity60.Features.Templates.FPS.Player
{
    /// <summary>
    /// PlayerStatsConfigの拡張メソッド群
    /// FPS_PlayerControllerで使用する状態別の速度とノイズレベル計算を提供
    /// 詳細設計書3.1準拠：状態に応じて移動速度や足音の大きさを動的に変更
    /// </summary>
    public static class PlayerStatsConfigExtensions
    {
        /// <summary>
        /// FPS状態に基づく移動速度を取得
        /// </summary>
        /// <param name="config">FPSMovementConfig</param>
        /// <param name="state">FPS状態</param>
        /// <returns>移動速度</returns>
        public static float GetSpeedForState(this FPSMovementConfig config, FPSPlayerState state)
        {
            if (config == null) return 5.0f;

            return state switch
            {
                FPSPlayerState.Walking => config.WalkSpeed,
                FPSPlayerState.Running => config.RunSpeed,
                FPSPlayerState.Crouching => config.CrouchSpeed,
                FPSPlayerState.Idle => 0f,
                _ => config.WalkSpeed
            };
        }

        /// <summary>
        /// FPS状態に基づくノイズレベルを取得（ステルス連動用）
        /// </summary>
        /// <param name="config">PlayerStatsConfig</param>
        /// <param name="state">FPS状態</param>
        /// <returns>ノイズレベル（0.0-1.0）</returns>
        public static float GetNoiseLevelForState(this PlayerStatsConfig config, FPSPlayerState state)
        {
            if (config == null) return 1.0f;

            return state switch
            {
                FPSPlayerState.Idle => 0.0f,
                FPSPlayerState.Crouching => 0.3f,
                FPSPlayerState.Walking => 0.6f,
                FPSPlayerState.Running => 1.0f,
                _ => 0.5f
            };
        }

        /// <summary>
        /// 従来のPlayerStateTypeに基づく移動速度を取得（互換性）
        /// </summary>
        /// <param name="config">FPSMovementConfig</param>
        /// <param name="state">PlayerStateType</param>
        /// <returns>移動速度</returns>
        public static float GetSpeedForLegacyState(this FPSMovementConfig config, asterivo.Unity60.Features.Player.States.PlayerStateType state)
        {
            if (config == null) return 5.0f;

            return state switch
            {
                asterivo.Unity60.Features.Player.States.PlayerStateType.Idle => 0f,
                asterivo.Unity60.Features.Player.States.PlayerStateType.Walking => config.WalkSpeed,
                asterivo.Unity60.Features.Player.States.PlayerStateType.Running => config.RunSpeed,
                asterivo.Unity60.Features.Player.States.PlayerStateType.Crouching => config.CrouchSpeed,
                asterivo.Unity60.Features.Player.States.PlayerStateType.Jumping => config.WalkSpeed, // ジャンプ中は歩行速度
                asterivo.Unity60.Features.Player.States.PlayerStateType.Prone => config.CrouchSpeed * 0.5f, // 伏せは更に遅い
                _ => config.WalkSpeed
            };
        }

        /// <summary>
        /// 従来のPlayerStateTypeに基づくノイズレベルを取得（互換性）
        /// </summary>
        /// <param name="config">PlayerStatsConfig</param>
        /// <param name="state">PlayerStateType</param>
        /// <returns>ノイズレベル（0.0-1.0）</returns>
        public static float GetNoiseLevelForLegacyState(this PlayerStatsConfig config, asterivo.Unity60.Features.Player.States.PlayerStateType state)
        {
            if (config == null) return 1.0f;

            return state switch
            {
                asterivo.Unity60.Features.Player.States.PlayerStateType.Idle => 0.0f,
                asterivo.Unity60.Features.Player.States.PlayerStateType.Prone => 0.1f,
                asterivo.Unity60.Features.Player.States.PlayerStateType.Crouching => 0.2f,
                asterivo.Unity60.Features.Player.States.PlayerStateType.Walking => 0.5f,
                asterivo.Unity60.Features.Player.States.PlayerStateType.Running => 1.0f,
                asterivo.Unity60.Features.Player.States.PlayerStateType.Jumping => 0.8f, // ジャンプは大きな音
                asterivo.Unity60.Features.Player.States.PlayerStateType.Rolling => 0.7f, // ローリングも音がする
                _ => 0.5f
            };
        }

        /// <summary>
        /// 移動状態とエイミング状態を組み合わせた総合速度計算
        /// </summary>
        /// <param name="config">FPSMovementConfig</param>
        /// <param name="baseState">基本移動状態</param>
        /// <param name="isAiming">エイミング中かどうか</param>
        /// <param name="aimingSpeedMultiplier">エイミング時の速度倍率</param>
        /// <returns>調整された移動速度</returns>
        public static float GetAdjustedSpeed(this FPSMovementConfig config, FPSPlayerState baseState, bool isAiming, float aimingSpeedMultiplier = 0.5f)
        {
            float baseSpeed = config.GetSpeedForState(baseState);
            return isAiming ? baseSpeed * aimingSpeedMultiplier : baseSpeed;
        }

        /// <summary>
        /// スタミナ消費率を状態に基づいて計算
        /// </summary>
        /// <param name="config">PlayerStatsConfig</param>
        /// <param name="state">FPS状態</param>
        /// <returns>スタミナ消費率（per second）</returns>
        public static float GetStaminaDrainRate(this PlayerStatsConfig config, FPSPlayerState state)
        {
            if (config == null) return 0f;

            return state switch
            {
                FPSPlayerState.Idle => 0f,
                FPSPlayerState.Crouching => config.StaminaDrainRate * 0.3f,
                FPSPlayerState.Walking => config.StaminaDrainRate * 0.5f,
                FPSPlayerState.Running => config.StaminaDrainRate,
                _ => 0f
            };
        }

        /// <summary>
        /// 装備重量による速度調整を計算
        /// </summary>
        /// <param name="config">PlayerStatsConfig</param>
        /// <param name="baseSpeed">基本速度</param>
        /// <param name="equipmentWeight">装備重量</param>
        /// <param name="maxWeight">最大重量</param>
        /// <returns>重量調整後の速度</returns>
        public static float ApplyWeightPenalty(this PlayerStatsConfig config, float baseSpeed, float equipmentWeight, float maxWeight = 100f)
        {
            if (equipmentWeight <= 0f || maxWeight <= 0f) return baseSpeed;

            float weightRatio = Mathf.Clamp01(equipmentWeight / maxWeight);
            float speedMultiplier = Mathf.Lerp(1.0f, 0.6f, weightRatio); // 重量100%で40%速度減少

            return baseSpeed * speedMultiplier;
        }

        /// <summary>
        /// 地形タイプによる移動速度調整
        /// </summary>
        /// <param name="config">PlayerStatsConfig</param>
        /// <param name="baseSpeed">基本速度</param>
        /// <param name="terrainType">地形タイプ</param>
        /// <returns>地形調整後の速度</returns>
        public static float ApplyTerrainModifier(this PlayerStatsConfig config, float baseSpeed, TerrainType terrainType)
        {
            float modifier = terrainType switch
            {
                TerrainType.Normal => 1.0f,
                TerrainType.Mud => 0.7f,
                TerrainType.Sand => 0.8f,
                TerrainType.Snow => 0.6f,
                TerrainType.Water => 0.4f,
                TerrainType.Stairs => 0.8f,
                _ => 1.0f
            };

            return baseSpeed * modifier;
        }

        /// <summary>
        /// 体力によるパフォーマンス調整
        /// </summary>
        /// <param name="config">PlayerStatsConfig</param>
        /// <param name="baseSpeed">基本速度</param>
        /// <param name="currentHealth">現在の体力</param>
        /// <param name="maxHealth">最大体力</param>
        /// <returns>体力調整後の速度</returns>
        public static float ApplyHealthPenalty(this PlayerStatsConfig config, float baseSpeed, float currentHealth, float maxHealth)
        {
            if (currentHealth <= 0f || maxHealth <= 0f) return 0f;

            float healthRatio = currentHealth / maxHealth;

            // 体力50%以下で速度低下開始
            if (healthRatio > 0.5f) return baseSpeed;

            float speedMultiplier = Mathf.Lerp(0.5f, 1.0f, (healthRatio - 0f) / 0.5f);
            return baseSpeed * speedMultiplier;
        }

        /// <summary>
        /// 総合的な移動速度計算（全ての要素を考慮）
        /// </summary>
        /// <param name="playerConfig">PlayerStatsConfig</param>
        /// <param name="movementConfig">FPSMovementConfig</param>
        /// <param name="state">FPS状態</param>
        /// <param name="isAiming">エイミング中</param>
        /// <param name="equipmentWeight">装備重量</param>
        /// <param name="terrainType">地形タイプ</param>
        /// <param name="currentHealth">現在体力</param>
        /// <param name="maxHealth">最大体力</param>
        /// <returns>総合調整後の移動速度</returns>
        public static float GetComprehensiveSpeed(this PlayerStatsConfig playerConfig,
            FPSMovementConfig movementConfig,
            FPSPlayerState state,
            bool isAiming = false,
            float equipmentWeight = 0f,
            TerrainType terrainType = TerrainType.Normal,
            float currentHealth = 100f,
            float maxHealth = 100f)
        {
            float baseSpeed = movementConfig.GetAdjustedSpeed(state, isAiming);

            baseSpeed = playerConfig.ApplyWeightPenalty(baseSpeed, equipmentWeight);
            baseSpeed = playerConfig.ApplyTerrainModifier(baseSpeed, terrainType);
            baseSpeed = playerConfig.ApplyHealthPenalty(baseSpeed, currentHealth, maxHealth);

            return Mathf.Max(0f, baseSpeed);
        }
    }

    /// <summary>
    /// 地形タイプの定義
    /// </summary>
    public enum TerrainType
    {
        Normal,     // 通常地面
        Mud,        // 泥
        Sand,       // 砂
        Snow,       // 雪
        Water,      // 水（浅瀬）
        Stairs,     // 階段
        Ice,        // 氷
        Grass,      // 草地
        Metal,      // 金属床
        Wood        // 木床
    }
}