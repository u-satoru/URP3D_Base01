using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// スプリント（ダッシュ）コマンドの定義。
    /// プレイヤーまたはAIの高速移動アクションをカプセル化します。
    /// 
    /// 主な機能：
    /// - スプリント速度と継続時間の管理
    /// - スタミナ消費システムとの連携
    /// - スプリント中の制約（方向転換制限等）
    /// - アニメーションとエフェクトの制御
    /// </summary>
    [System.Serializable]
    public class SprintCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// スプリントの種類を定義する列挙型
        /// </summary>
        public enum SprintType
        {
            Burst,      // 短距離爆発的加速
            Sustained,  // 持続的高速移動
            Dodge,      // 回避ダッシュ
            Charge      // 突進攻撃
        }

        [Header("Sprint Parameters")]
        public SprintType sprintType = SprintType.Burst;
        public float speedMultiplier = 2f;
        public float maxDuration = 3f;
        public Vector3 direction = Vector3.forward;

        [Header("Stamina System")]
        public float staminaConsumptionRate = 10f; // per second
        public float minimumStaminaRequired = 25f;
        public bool canInterruptOnStaminaDepleted = true;

        [Header("Movement Constraints")]
        public bool lockDirection = false;
        public float directionChangeSpeed = 5f;
        public bool maintainVelocityOnEnd = false;

        [Header("Effects")]
        public float accelerationTime = 0.2f;
        public float decelerationTime = 0.5f;
        public bool showTrailEffect = true;

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public SprintCommandDefinition()
        {
        }

        /// <summary>
        /// パラメータ付きコンストラクタ
        /// </summary>
        public SprintCommandDefinition(SprintType type, float multiplier, Vector3 sprintDirection)
        {
            sprintType = type;
            speedMultiplier = multiplier;
            direction = sprintDirection.normalized;
        }

        /// <summary>
        /// スプリントコマンドが実行可能かどうかを判定します
        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本的な実行可能性チェック
            if (speedMultiplier <= 1f || maxDuration <= 0f) return false;
            
            // 方向ベクトルのチェック
            if (direction == Vector3.zero) return false;

            // コンテキストがある場合の追加チェック
            if (context != null)
            {
                // スタミナチェック
                // クールダウンチェック
                // 状態異常チェック（疲労、負傷等）
                // 地形制約チェック（水中、急斜面等でのスプリント制限）
            }

            return true;
        }

        /// <summary>
        /// スプリントコマンドを作成します
        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new SprintCommand(this, context);
        }
    }

    /// <summary>
    /// SprintCommandDefinitionに対応する実際のコマンド実装
    /// </summary>
    public class SprintCommand : ICommand
    {
        private SprintCommandDefinition definition;
        private object context;
        private bool executed = false;
        private bool isActive = false;
        private float originalSpeed;
        private float currentDuration = 0f;

        public SprintCommand(SprintCommandDefinition sprintDefinition, object executionContext)
        {
            definition = sprintDefinition;
            context = executionContext;
        }

        /// <summary>
        /// スプリントコマンドの実行
        /// </summary>
        public void Execute()
        {
            if (executed) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.sprintType} sprint: {definition.speedMultiplier}x speed, {definition.maxDuration}s max duration");
#endif

            // スプリント状態の開始
            isActive = true;
            currentDuration = 0f;

            // 実際のスプリント処理をここに実装
            if (context is MonoBehaviour mono)
            {
                // 移動速度の保存と変更
                // アニメーション制御
                // パーティクルエフェクト開始
                // サウンドエフェクト

                // スタミナ消費の開始（実際の実装では StaminaSystem との連携）
                // 継続的な更新処理の開始（Coroutine またはUpdateLoop）
            }

            executed = true;
        }

        /// <summary>
        /// スプリント状態の更新（外部から定期的に呼び出される）
        /// </summary>
        public void UpdateSprint(float deltaTime)
        {
            if (!isActive) return;

            currentDuration += deltaTime;

            // スタミナ消費処理
            float staminaConsumed = definition.staminaConsumptionRate * deltaTime;
            
            // 最大継続時間チェック
            if (currentDuration >= definition.maxDuration)
            {
                EndSprint();
                return;
            }

            // スタミナ枯渇チェック
            if (definition.canInterruptOnStaminaDepleted)
            {
                // 実際の実装では StaminaSystem からの値を参照
                // if (currentStamina <= 0f) EndSprint();
            }
        }

        /// <summary>
        /// スプリント状態の終了
        /// </summary>
        public void EndSprint()
        {
            if (!isActive) return;

            isActive = false;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Sprint ended after {currentDuration:F1} seconds");
#endif

            // 速度の復元（maintainVelocityOnEndに応じて）
            // アニメーション制御
            // エフェクトの停止
            // クールダウンの開始
        }

        /// <summary>
        /// Undo操作（スプリントの強制停止）
        /// </summary>
        public void Undo()
        {
            if (!executed) return;

            EndSprint();

            // 消費したスタミナの復元（部分的）
            // 状態の完全リセット

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Sprint command undone");
#endif

            executed = false;
        }

        /// <summary>
        /// このコマンドがUndo可能かどうか
        /// </summary>
        public bool CanUndo => executed;

        /// <summary>
        /// スプリントが現在アクティブかどうか
        /// </summary>
        public bool IsActive => isActive;
    }
}