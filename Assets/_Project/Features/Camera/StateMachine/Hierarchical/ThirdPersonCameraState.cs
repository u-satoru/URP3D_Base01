using UnityEngine;
using asterivo.Unity60.Core.Patterns.StateMachine;
using asterivo.Unity60.Core.StateMachine;
using asterivo.Unity60.Features.Camera.ViewMode;

namespace asterivo.Unity60.Features.Camera.StateMachine.Hierarchical
{
    /// <summary>
    /// 三人称カメラ状態 - 階層化ステートマシンの実装例
    /// Follow（追従）とFreeLook（フリールック）を子状態として持つ
    /// </summary>
    public class ThirdPersonCameraState : HierarchicalState<CameraContext>
    {
        private const float FREE_LOOK_TIMEOUT = 3f;
        private const float MOVEMENT_THRESHOLD = 0.1f;

        protected override void InitializeChildStates()
        {
            // 子状態の定義
            var followState = new FollowChildState();
            var freeLookState = new FreeLookChildState();

            AddChildState("Follow", followState);
            AddChildState("FreeLook", freeLookState);
        }

        protected override string GetDefaultChildStateKey()
        {
            return "Follow";
        }

        protected override void OnParentEnter(CameraContext context)
        {

            // 三人称カメラの基本設定
            var settings = context.ThirdPersonSettings;
            context.SetCameraFOV(settings.fieldOfView);
            context.SetTargetDistance(settings.distance);

            Debug.Log("[ThirdPersonCameraState] Entered third person camera mode");
        }

        protected override void OnParentUpdate(CameraContext context)
        {

            // ズーム入力の処理
            if (Mathf.Abs(context.ZoomInput) > 0.1f)
            {
                float zoomDelta = -context.ZoomInput * context.ThirdPersonSettings.zoomSpeed * Time.deltaTime;
                context.AdjustDistance(zoomDelta);
            }

            // カメラモード切り替えの処理
            if (context.CameraModeTogglePressed)
            {
                // 上位ステートマシンに一人称への切り替えを要求
                Debug.Log("[ThirdPersonCameraState] Requesting transition to first person");
            }
        }

        protected override void OnParentExit(CameraContext context)
        {
            Debug.Log("[ThirdPersonCameraState] Exited third person camera mode");
        }

        // カメラリセット処理
        public void OnResetRequested(CameraContext context)
        {
            // デフォルト位置にリセット
            if (context.FollowTarget != null)
            {
                var settings = context.ThirdPersonSettings;
                Vector3 targetPos = context.GetTargetPosition();
                Vector3 offset = -context.GetTargetForward() * settings.distance + Vector3.up * settings.height;

                context.SetCameraPosition(targetPos + offset);
                context.SetCameraRotation(Quaternion.LookRotation(targetPos - context.GetCameraPosition()));
                context.SetTargetDistance(settings.distance);
            }
        }
    }

    /// <summary>
    /// 追従子状態 - ターゲットを自動追従
    /// </summary>
    public class FollowChildState : IState<CameraContext>
    {
        public string StateName => "Follow";
        public bool IsActive { get; private set; }

        public void Enter(CameraContext context)
        {
            IsActive = true;
            Debug.Log("[FollowChildState] Camera following target automatically");
        }

        public void Exit(CameraContext context)
        {
            IsActive = false;
            Debug.Log("[FollowChildState] Stopped automatic following");
        }

        public void Update(CameraContext context)
        {
            if (context.FollowTarget == null) return;

            var settings = context.ThirdPersonSettings;
            UpdateCameraPosition(context, settings);
        }

        public void FixedUpdate(CameraContext context)
        {
            // Physics-related camera logic can be added here if needed
        }

        public void HandleInput(CameraContext context, object inputData)
        {
            // Follow-specific input handling can be added here if needed
        }

        private void UpdateCameraPosition(CameraContext context, ThirdPersonSettings settings)
        {
            Vector3 targetPos = context.GetTargetPosition();
            Vector3 targetForward = context.GetTargetForward();

            // 理想的なカメラ位置を計算
            Vector3 idealPosition = targetPos - targetForward * context.TargetDistance + Vector3.up * settings.height;

            // 衝突チェック
            if (context.CheckCameraCollision(idealPosition, out Vector3 adjustedPosition))
            {
                idealPosition = adjustedPosition;
            }

            // スムーズに移動
            Vector3 currentPos = context.GetCameraPosition();
            Vector3 newPosition = Vector3.Lerp(currentPos, idealPosition, settings.followSpeed * Time.deltaTime);
            context.SetCameraPosition(newPosition);

            // カメラをターゲットに向ける
            Vector3 lookDirection = (targetPos - newPosition).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            Quaternion newRotation = Quaternion.Slerp(context.GetCameraRotation(), targetRotation, settings.rotationSpeed * Time.deltaTime);
            context.SetCameraRotation(newRotation);
        }
    }

    /// <summary>
    /// フリールック子状態 - 手動カメラ操作
    /// </summary>
    public class FreeLookChildState : IState<CameraContext>
    {
        public string StateName => "FreeLook";
        public bool IsActive { get; private set; }

        private float verticalRotation = 0f;
        private float horizontalRotation = 0f;

        public void Enter(CameraContext context)
        {
            IsActive = true;

            // 現在の回転を基準値として設定
            Vector3 currentEuler = context.GetCameraRotation().eulerAngles;
            horizontalRotation = currentEuler.y;
            verticalRotation = currentEuler.x;

            // 垂直回転を-180~180度の範囲に正規化
            if (verticalRotation > 180f) verticalRotation -= 360f;

            Debug.Log("[FreeLookChildState] Free look camera control enabled");
        }

        public void Exit(CameraContext context)
        {
            IsActive = false;
            Debug.Log("[FreeLookChildState] Free look camera control disabled");
        }

        public void Update(CameraContext context)
        {
            if (context.FollowTarget == null || !context.IsInputAllowed()) return;

            var settings = context.ThirdPersonSettings;
            UpdateCameraWithInput(context, settings);
        }

        public void FixedUpdate(CameraContext context)
        {
            // Physics-related camera logic can be added here if needed
        }

        public void HandleInput(CameraContext context, object inputData)
        {
            // Free look specific input handling can be added here if needed
        }

        private void UpdateCameraWithInput(CameraContext context, ThirdPersonSettings settings)
        {
            // 入力に基づく回転計算
            float mouseSensitivity = context.GetMouseSensitivity();
            horizontalRotation += context.LookInput.x * settings.rotationSpeed * mouseSensitivity * Time.deltaTime;
            verticalRotation -= context.LookInput.y * settings.rotationSpeed * mouseSensitivity * Time.deltaTime;

            // 垂直回転の制限
            verticalRotation = Mathf.Clamp(verticalRotation, settings.minVerticalAngle, settings.maxVerticalAngle);

            // 新しい回転を適用
            Quaternion rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0);
            context.SetCameraRotation(rotation);

            // ターゲット周りの位置を計算
            Vector3 targetPos = context.GetTargetPosition();
            Vector3 direction = rotation * Vector3.back;
            Vector3 idealPosition = targetPos + direction * context.TargetDistance + Vector3.up * settings.height;

            // 衝突チェック
            if (context.CheckCameraCollision(idealPosition, out Vector3 adjustedPosition))
            {
                idealPosition = adjustedPosition;
            }

            context.SetCameraPosition(idealPosition);
        }
    }
}
