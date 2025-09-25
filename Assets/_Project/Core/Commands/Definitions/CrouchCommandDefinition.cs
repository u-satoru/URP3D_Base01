using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// しゃがみ（クラウチ）コマンドの定義。
    /// プレイヤーまたはAIのしゃがみアクションをカプセル化します。
    /// 
    /// 主な機能：
    /// - しゃがみ状態の開始と終了
    /// - 移動速度の変更とステルス効果
    /// - コリジョンサイズの調整
    /// - アニメーションとカメラの制御
    /// </summary>
    [System.Serializable]
    public class CrouchCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// しゃがみの種類を定義する列挙型
        /// </summary>
        public enum CrouchType
        {
            Normal,     // 通常のしゃがみ
            Sneak,      // ステルス重視のしゃがみ
            Cover,      // 遮蔽物利用のしゃがみ
            Slide       // スライディング
        }

        [Header("Crouch Parameters")]
        public CrouchType crouchType = CrouchType.Normal;
        public bool toggleMode = true; // true: トグル形式, false: 押し続ける形式
        public float speedMultiplier = 0.5f;
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
        /// デフォルトコンストラクタ
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
        /// しゃがみコマンドが実行可能かどうかを判定します
        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本的な実行可能性チェック
            if (speedMultiplier < 0f || heightReduction < 0f || heightReduction > 1f) 
                return false;
            
            if (transitionDuration < 0f) 
                return false;

            // コンテキストがある場合の追加チェック
            if (context != null)
            {
                // 現在の地形チェック（急斜面では不可等）
                // 天井の高さチェック（立ち上がれない場所での制限）
                // 状態異常チェック（足の負傷等）
                // アニメーション状態チェック（ジャンプ中は不可等）
            }

            return true;
        }

        /// <summary>
        /// しゃがみコマンドを作成します
        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new CrouchCommand(this, context);
        }
    }

    /// <summary>
    /// CrouchCommandDefinitionに対応する実際のコマンド実装
    /// </summary>
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
        /// しゃがみコマンドの実行
        /// </summary>
        public void Execute()
        {
            if (executed) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.crouchType} crouch: toggle={definition.toggleMode}");
#endif

            // トグルモードの場合は状態を切り替え
            if (definition.toggleMode)
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
                // 押し続けモードの場合は常にしゃがみ開始
                StartCrouch();
            }

            executed = true;
        }

        /// <summary>
        /// しゃがみ状態の開始
        /// </summary>
        private void StartCrouch()
        {
            if (isCrouching) return;

            // 実行前の状態を保存（Undo用）
            SaveOriginalState();

            isCrouching = true;

            // 実際のしゃがみ処理をここに実装
            if (context is MonoBehaviour mono)
            {
                // コライダーの高さ調整
                if (definition.adjustColliderHeight && mono.GetComponent<CapsuleCollider>() != null)
                {
                    var collider = mono.GetComponent<CapsuleCollider>();
                    collider.height *= (1f - definition.heightReduction);
                    collider.center = new Vector3(collider.center.x, collider.center.y - (originalHeight * definition.heightReduction * 0.5f), collider.center.z);
                }

                // 移動速度の調整（PlayerControllerとの連携）
                // アニメーション制御
                // カメラ位置の調整
                // ステルス状態の適用
                // サウンドエフェクト
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Started crouching");
#endif
        }

        /// <summary>
        /// 立ち上がり処理
        /// </summary>
        private void StandUp()
        {
            if (!isCrouching) return;

            // 天井チェック（立ち上がれるかどうか）
            if (!CanStandUp())
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("Cannot stand up - ceiling too low");
#endif
                return;
            }

            isCrouching = false;

            // 状態の復元
            RestoreOriginalState();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Stood up from crouch");
#endif
        }

        /// <summary>
        /// 立ち上がり可能かチェック
        /// </summary>
        private bool CanStandUp()
        {
            // 実際の実装では、頭上に障害物がないかRaycastでチェック
            // 現在は常にtrueを返す
            return true;
        }

        /// <summary>
        /// 元の状態を保存
        /// </summary>
        private void SaveOriginalState()
        {
            if (context is MonoBehaviour mono)
            {
                // コライダーの高さ保存
                if (mono.GetComponent<CapsuleCollider>() != null)
                {
                    originalHeight = mono.GetComponent<CapsuleCollider>().height;
                }

                // その他の状態保存
                // originalSpeed = playerController.moveSpeed;
                // originalCameraPosition = camera.localPosition;
            }
        }

        /// <summary>
        /// 元の状態を復元
        /// </summary>
        private void RestoreOriginalState()
        {
            if (context is MonoBehaviour mono)
            {
                // コライダーの復元
                if (definition.adjustColliderHeight && mono.GetComponent<CapsuleCollider>() != null)
                {
                    var collider = mono.GetComponent<CapsuleCollider>();
                    collider.height = originalHeight;
                    collider.center = new Vector3(collider.center.x, 0f, collider.center.z);
                }

                // その他の状態復元
                // playerController.moveSpeed = originalSpeed;
                // camera.localPosition = originalCameraPosition;
            }
        }

        /// <summary>
        /// 押し続けモードでのしゃがみ終了
        /// </summary>
        public void EndCrouch()
        {
            if (!definition.toggleMode && isCrouching)
            {
                StandUp();
            }
        }

        /// <summary>
        /// Undo操作（しゃがみ状態の強制解除）
        /// </summary>
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
        /// このコマンドがUndo可能かどうか
        /// </summary>
        public bool CanUndo => executed;

        /// <summary>
        /// 現在しゃがんでいるかどうか
        /// </summary>
        public bool IsCrouching => isCrouching;
    }
}