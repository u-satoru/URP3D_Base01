using UnityEngine;
using asterivo.Unity60.Core.Patterns.StateMachine;
using asterivo.Unity60.Core.StateMachine;
using asterivo.Unity60.Features.Camera.States;

namespace asterivo.Unity60.Features.Camera.StateMachine.Hierarchical
{
    /// <summary>
    /// エイムカメラ状態 - 階層化ステートマシンの実装例
    /// Precise（精密エイム）とQuick（クイックエイム）を子状態として持つ
    /// </summary>
    public class AimCameraState : HierarchicalState<CameraContext>
    {
        private const float QUICK_AIM_TIMEOUT = 2f;
        private const float PRECISION_MOVEMENT_THRESHOLD = 0.05f;

        private Vector3 aimStartPosition;
        private Quaternion aimStartRotation;

        protected override void InitializeChildStates()
        {
            // 子状態の定義
            var preciseState = new PreciseAimChildState();
            var quickState = new QuickAimChildState();

            AddChildState("Precise", preciseState);
            AddChildState("Quick", quickState);
        }

        protected override string GetDefaultChildStateKey()
        {
            return "Quick";
        }

        protected override void OnParentEnter(CameraContext context)
        {
            // エイム開始時の位置・回転を記録
            aimStartPosition = context.GetCameraPosition();
            aimStartRotation = context.GetCameraRotation();

            // エイム設定を適用
            var settings = context.AimSettings;
            context.SetCameraFOV(settings.aimFOV);

            Debug.Log("[AimCameraState] Entered aim mode");
        }

        protected override void OnParentUpdate(CameraContext context)
        {

            // エイム解除の処理
            if (context.AimReleased)
            {
                // 上位ステートマシンに前の状態への復帰を要求
                Debug.Log("[AimCameraState] Aim released, requesting return to previous state");
            }

            // ズーム処理（エイム中の微調整）
            if (Mathf.Abs(context.ZoomInput) > 0.1f)
            {
                var settings = context.AimSettings;
                float currentFOV = context.CurrentFOV;
                float zoomDelta = -context.ZoomInput * settings.zoomSpeed * Time.deltaTime;
                float newFOV = Mathf.Clamp(currentFOV + zoomDelta, settings.minFOV, settings.maxFOV);
                context.SetCameraFOV(newFOV);
            }
        }

        protected override void OnParentExit(CameraContext context)
        {

            // FOVを元に戻す
            var thirdPersonSettings = context.ThirdPersonSettings;
            context.SetCameraFOV(thirdPersonSettings.fieldOfView);

            Debug.Log("[AimCameraState] Exited aim mode");
        }

        // カメラリセット処理
        public void OnResetRequested(CameraContext context)
        {
            // エイム開始時の位置に戻す
            context.SetCameraPosition(aimStartPosition);
            context.SetCameraRotation(aimStartRotation);
            context.SetCameraFOV(context.AimSettings.aimFOV);
        }
    }

    /// <summary>
    /// 精密エイム子状態 - 高精度・低感度のエイム
    /// </summary>
    public class PreciseAimChildState : IState<CameraContext>
    {
        public string StateName => "Precise";
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

            Debug.Log("[PreciseAimChildState] Precise aim mode activated");
        }

        public void Exit(CameraContext context)
        {
            IsActive = false;
            Debug.Log("[PreciseAimChildState] Precise aim mode deactivated");
        }

        public void Update(CameraContext context)
        {
            if (!context.IsInputAllowed()) return;

            var settings = context.AimSettings;
            UpdatePreciseAim(context, settings);
        }

        public void FixedUpdate(CameraContext context)
        {
            // Physics-related aiming logic can be added here if needed
        }

        public void HandleInput(CameraContext context, object inputData)
        {
            // Precise aim specific input handling can be added here if needed
        }

        private void UpdatePreciseAim(CameraContext context, AimSettings settings)
        {
            // 精密エイムモードでは感度を大幅に下げる
            float preciseSensitivity = settings.preciseSensitivity;

            horizontalRotation += context.LookInput.x * preciseSensitivity * Time.deltaTime;
            verticalRotation -= context.LookInput.y * preciseSensitivity * Time.deltaTime;

            // 垂直回転の制限（エイム時はより狭い範囲）
            verticalRotation = Mathf.Clamp(verticalRotation, settings.minVerticalAngle, settings.maxVerticalAngle);

            // スムーズな回転適用
            Quaternion targetRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0);
            Quaternion currentRotation = context.GetCameraRotation();
            Quaternion newRotation = Quaternion.Slerp(currentRotation, targetRotation, settings.aimSmoothing * Time.deltaTime);

            context.SetCameraRotation(newRotation);
        }
    }

    /// <summary>
    /// クイックエイム子状態 - 通常感度のエイム
    /// </summary>
    public class QuickAimChildState : IState<CameraContext>
    {
        public string StateName => "Quick";
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

            Debug.Log("[QuickAimChildState] Quick aim mode activated");
        }

        public void Exit(CameraContext context)
        {
            IsActive = false;
            Debug.Log("[QuickAimChildState] Quick aim mode deactivated");
        }

        public void Update(CameraContext context)
        {
            if (!context.IsInputAllowed()) return;

            var settings = context.AimSettings;
            UpdateQuickAim(context, settings);
        }

        public void FixedUpdate(CameraContext context)
        {
            // Physics-related aiming logic can be added here if needed
        }

        public void HandleInput(CameraContext context, object inputData)
        {
            // Quick aim specific input handling can be added here if needed
        }

        private void UpdateQuickAim(CameraContext context, AimSettings settings)
        {
            // 通常エイムモードでは標準的な感度を使用
            float aimSensitivity = settings.aimSensitivity;

            horizontalRotation += context.LookInput.x * aimSensitivity * Time.deltaTime;
            verticalRotation -= context.LookInput.y * aimSensitivity * Time.deltaTime;

            // 垂直回転の制限
            verticalRotation = Mathf.Clamp(verticalRotation, settings.minVerticalAngle, settings.maxVerticalAngle);

            // ダイレクトな回転適用（クイックレスポンス）
            Quaternion rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0);
            context.SetCameraRotation(rotation);
        }
    }
}