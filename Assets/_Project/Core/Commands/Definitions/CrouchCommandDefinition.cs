using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// しゃがみ�E�クラウチE��コマンド�E定義、E    /// プレイヤーまた�EAIのしゃがみアクションをカプセル化します、E    /// 
    /// 主な機�E�E�E    /// - しゃがみ状態�E開始と終亁E    /// - 移動速度の変更とスチE��ス効极E    /// - コリジョンサイズの調整
    /// - アニメーションとカメラの制御
    /// </summary>
    [System.Serializable]
    public class CrouchCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// しゃがみの種類を定義する列挙垁E        /// </summary>
        public enum CrouchType
        {
            Normal,     // 通常のしゃがみ
            Sneak,      // スチE��ス重視�Eしゃがみ
            Cover,      // 遮蔽物利用のしゃがみ
            Slide       // スライチE��ング
        }

        [Header("Crouch Parameters")]
        public CrouchType crouchType = CrouchType.Normal;
        public bool toggleMode = true; // true: トグル形弁E false: 押し続ける形弁E        public float speedMultiplier = 0.5f;
        public float heightReduction = 0.5f;

        [Header("Stealth Effects")]
        public float noiseReduction = 0.7f; // 音の削減率
        public float visibilityReduction = 0.3f; // 視認性の削減率
        public bool canHideInTallGrass = true;

        [Header("Movement Constraints")]
        public bool canSprint = false;
        public bool canJump = false;
        public float maxSlopeAngle = 30f;

        [Header("Animation")]
        public float transitionDuration = 0.3f;
        public bool adjustCameraHeight = true;
        public float cameraHeightOffset = -0.5f;

        [Header("Physics")]
        public bool adjustColliderHeight = true;
        public bool maintainGroundContact = true;

        /// <summary>
        /// チE��ォルトコンストラクタ
        /// </summary>
        public CrouchCommandDefinition()
        {
        }

        /// <summary>
        /// パラメータ付きコンストラクタ
        /// </summary>
        public CrouchCommandDefinition(CrouchType type, bool isToggle, float speedMult = 0.5f)
        {
            crouchType = type;
            toggleMode = isToggle;
            speedMultiplier = speedMult;
        }

        /// <summary>
        /// しゃがみコマンドが実行可能かどぁE��を判定しまぁE        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本皁E��実行可能性チェチE��
            if (speedMultiplier < 0f || heightReduction < 0f || heightReduction > 1f) 
                return false;
            
            if (transitionDuration < 0f) 
                return false;

            // コンチE��ストがある場合�E追加チェチE��
            if (context != null)
            {
                // 現在の地形チェチE���E�急斜面では不可等！E                // 天井�E高さチェチE���E�立ち上がれなぁE��所での制限！E                // 状態異常チェチE���E�足の負傷等！E                // アニメーション状態チェチE���E�ジャンプ中は不可等！E            }

            return true;
        }

        /// <summary>
        /// しゃがみコマンドを作�EしまぁE        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new CrouchCommand(this, context);
        }
    }

    /// <summary>
    /// CrouchCommandDefinitionに対応する実際のコマンド実裁E    /// </summary>
    public class CrouchCommand : ICommand
    {
        private CrouchCommandDefinition definition;
        private object context;
        private bool executed = false;
        private bool isCrouching = false;
        private float originalHeight;
        private float originalSpeed;
        private Vector3 originalCameraPosition;

        public CrouchCommand(CrouchCommandDefinition crouchDefinition, object executionContext)
        {
            definition = crouchDefinition;
            context = executionContext;
        }

        /// <summary>
        /// しゃがみコマンド�E実衁E        /// </summary>
        public void Execute()
        {
            if (executed) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.crouchType} crouch: toggle={definition.toggleMode}");
#endif

            // トグルモード�E場合�E状態を刁E��替ぁE            if (definition.toggleMode)
            {
                if (isCrouching)
                {
                    StandUp();
                }
                else
                {
                    StartCrouch();
                }
            }
            else
            {
                // 押し続けモード�E場合�E常にしゃがみ開姁E                StartCrouch();
            }

            executed = true;
        }

        /// <summary>
        /// しゃがみ状態�E開姁E        /// </summary>
        private void StartCrouch()
        {
            if (isCrouching) return;

            // 実行前の状態を保存！Endo用�E�E            SaveOriginalState();

            isCrouching = true;

            // 実際のしゃがみ処琁E��ここに実裁E            if (context is MonoBehaviour mono)
            {
                // コライダーの高さ調整
                if (definition.adjustColliderHeight && mono.GetComponent<CapsuleCollider>() != null)
                {
                    var collider = mono.GetComponent<CapsuleCollider>();
                    collider.height *= (1f - definition.heightReduction);
                    collider.center = new Vector3(collider.center.x, collider.center.y - (originalHeight * definition.heightReduction * 0.5f), collider.center.z);
                }

                // 移動速度の調整�E�ElayerControllerとの連携�E�E                // アニメーション制御
                // カメラ位置の調整
                // スチE��ス状態�E適用
                // サウンドエフェクチE            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Started crouching");
#endif
        }

        /// <summary>
        /// 立ち上がり�E琁E        /// </summary>
        private void StandUp()
        {
            if (!isCrouching) return;

            // 天井チェチE���E�立ち上がれるかどぁE���E�E            if (!CanStandUp())
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("Cannot stand up - ceiling too low");
#endif
                return;
            }

            isCrouching = false;

            // 状態�E復允E            RestoreOriginalState();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Stood up from crouch");
#endif
        }

        /// <summary>
        /// 立ち上がり可能かチェチE��
        /// </summary>
        private bool CanStandUp()
        {
            // 実際の実裁E��は、E��上に障害物がなぁE��RaycastでチェチE��
            // 現在は常にtrueを返す
            return true;
        }

        /// <summary>
        /// 允E�E状態を保孁E        /// </summary>
        private void SaveOriginalState()
        {
            if (context is MonoBehaviour mono)
            {
                // コライダーの高さ保孁E                if (mono.GetComponent<CapsuleCollider>() != null)
                {
                    originalHeight = mono.GetComponent<CapsuleCollider>().height;
                }

                // そ�E他�E状態保孁E                // originalSpeed = playerController.moveSpeed;
                // originalCameraPosition = camera.localPosition;
            }
        }

        /// <summary>
        /// 允E�E状態を復允E        /// </summary>
        private void RestoreOriginalState()
        {
            if (context is MonoBehaviour mono)
            {
                // コライダーの復允E                if (definition.adjustColliderHeight && mono.GetComponent<CapsuleCollider>() != null)
                {
                    var collider = mono.GetComponent<CapsuleCollider>();
                    collider.height = originalHeight;
                    collider.center = new Vector3(collider.center.x, 0f, collider.center.z);
                }

                // そ�E他�E状態復允E                // playerController.moveSpeed = originalSpeed;
                // camera.localPosition = originalCameraPosition;
            }
        }

        /// <summary>
        /// 押し続けモードでのしゃがみ終亁E        /// </summary>
        public void EndCrouch()
        {
            if (!definition.toggleMode && isCrouching)
            {
                StandUp();
            }
        }

        /// <summary>
        /// Undo操作（しめE��み状態�E強制解除�E�E        /// </summary>
        public void Undo()
        {
            if (!executed) return;

            if (isCrouching)
            {
                StandUp();
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Crouch command undone");
#endif

            executed = false;
        }

        /// <summary>
        /// こ�EコマンドがUndo可能かどぁE��
        /// </summary>
        public bool CanUndo => executed;

        /// <summary>
        /// 現在しゃがんでぁE��かどぁE��
        /// </summary>
        public bool IsCrouching => isCrouching;
    }
}