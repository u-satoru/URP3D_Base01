using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// スプリント（ダチE��ュ�E�コマンド�E定義、E    /// プレイヤーまた�EAIの高速移動アクションをカプセル化します、E    /// 
    /// 主な機�E�E�E    /// - スプリント速度と継続時間�E管琁E    /// - スタミナ消費シスチE��との連携
    /// - スプリント中の制紁E��方向転換制限等！E    /// - アニメーションとエフェクト�E制御
    /// </summary>
    [System.Serializable]
    public class SprintCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// スプリント�E種類を定義する列挙垁E        /// </summary>
        public enum SprintType
        {
            Burst,      // 短距離爁E��皁E��送E            Sustained,  // 持続的高速移勁E            Dodge,      // 回避ダチE��ュ
            Charge      // 突E��攻撁E        }

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
        /// チE��ォルトコンストラクタ
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
        /// スプリントコマンドが実行可能かどぁE��を判定しまぁE        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本皁E��実行可能性チェチE��
            if (speedMultiplier <= 1f || maxDuration <= 0f) return false;
            
            // 方向�EクトルのチェチE��
            if (direction == Vector3.zero) return false;

            // コンチE��ストがある場合�E追加チェチE��
            if (context != null)
            {
                // スタミナチェチE��
                // クールダウンチェチE��
                // 状態異常チェチE���E�疲労、負傷等！E                // 地形制紁E��ェチE���E�水中、急斜面等でのスプリント制限！E            }

            return true;
        }

        /// <summary>
        /// スプリントコマンドを作�EしまぁE        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new SprintCommand(this, context);
        }
    }

    /// <summary>
    /// SprintCommandDefinitionに対応する実際のコマンド実裁E    /// </summary>
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
        /// スプリントコマンド�E実衁E        /// </summary>
        public void Execute()
        {
            if (executed) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.sprintType} sprint: {definition.speedMultiplier}x speed, {definition.maxDuration}s max duration");
#endif

            // スプリント状態�E開姁E            isActive = true;
            currentDuration = 0f;

            // 実際のスプリント�E琁E��ここに実裁E            if (context is MonoBehaviour mono)
            {
                // 移動速度の保存と変更
                // アニメーション制御
                // パ�EチE��クルエフェクト開姁E                // サウンドエフェクチE
                // スタミナ消費の開始（実際の実裁E��は StaminaSystem との連携�E�E                // 継続的な更新処琁E�E開始！Eoroutine また�EUpdateLoop�E�E            }

            executed = true;
        }

        /// <summary>
        /// スプリント状態�E更新�E�外部から定期皁E��呼び出される！E        /// </summary>
        public void UpdateSprint(float deltaTime)
        {
            if (!isActive) return;

            currentDuration += deltaTime;

            // スタミナ消費処琁E            float staminaConsumed = definition.staminaConsumptionRate * deltaTime;
            
            // 最大継続時間チェチE��
            if (currentDuration >= definition.maxDuration)
            {
                EndSprint();
                return;
            }

            // スタミナ枯渁E��ェチE��
            if (definition.canInterruptOnStaminaDepleted)
            {
                // 実際の実裁E��は StaminaSystem からの値を参照
                // if (currentStamina <= 0f) EndSprint();
            }
        }

        /// <summary>
        /// スプリント状態�E終亁E        /// </summary>
        public void EndSprint()
        {
            if (!isActive) return;

            isActive = false;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Sprint ended after {currentDuration:F1} seconds");
#endif

            // 速度の復允E��EaintainVelocityOnEndに応じて�E�E            // アニメーション制御
            // エフェクト�E停止
            // クールダウンの開姁E        }

        /// <summary>
        /// Undo操作（スプリント�E強制停止�E�E        /// </summary>
        public void Undo()
        {
            if (!executed) return;

            EndSprint();

            // 消費したスタミナの復允E��部刁E���E�E            // 状態�E完�EリセチE��

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Sprint command undone");
#endif

            executed = false;
        }

        /// <summary>
        /// こ�EコマンドがUndo可能かどぁE��
        /// </summary>
        public bool CanUndo => executed;

        /// <summary>
        /// スプリントが現在アクチE��ブかどぁE��
        /// </summary>
        public bool IsActive => isActive;
    }
}