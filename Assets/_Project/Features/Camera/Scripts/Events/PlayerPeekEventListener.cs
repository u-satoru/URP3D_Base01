using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Camera.States;

namespace asterivo.Unity60.Features.Camera.Events
{
    /// <summary>
    /// プレイヤーの覗き見アクションイベントを受信し、カメラ状態を更新するリスナー
    /// </summary>
    /// <remarks>
    /// Player層からのPeekイベントをGameEvent経由で受信し、
    /// Camera層の挙動を制御することで、層間の疎結合を実現します。
    /// </remarks>
    public class PlayerPeekEventListener : MonoBehaviour
    {
        [Header("Camera Components")]
        [SerializeField] private CameraStateMachine cameraStateMachine;

        [Header("Peek Settings")]
        [SerializeField] private float peekIntensity = 1.0f;
        [SerializeField] private float peekTransitionSpeed = 8f;

        // 現在の覗き見状態
        private bool isPeeking = false;
        private PeekDirection currentPeekDirection = PeekDirection.None;
        private Vector3 peekOffset = Vector3.zero;

        public enum PeekDirection
        {
            None,
            Left,
            Right,
            Over,
            Under
        }

        private void OnEnable()
        {
            // TODO: GameEventベースのイベント購読に変更
            // ServiceLocator/IEventManagerの実装後に有効化
            /*
            var eventManager = ServiceLocator.TryGet<IEventManager>(out var em) ? em : null;
            eventManager?.Subscribe("PlayerPeek", OnPlayerPeekEvent);
            */
        }

        private void OnDisable()
        {
            // TODO: GameEventベースのイベント購読解除に変更
            // ServiceLocator/IEventManagerの実装後に有効化
            /*
            var eventManager = ServiceLocator.TryGet<IEventManager>(out var em) ? em : null;
            eventManager?.Unsubscribe("PlayerPeek", OnPlayerPeekEvent);
            */
        }

        /// <summary>
        /// Player層からのPeekイベントハンドラー
        /// </summary>
        private void OnPlayerPeekEvent(object eventData)
        {
            // PlayerPeekEventDataの形式でデータを受信
            // 実際のPlayerPeekEventDataクラスはPlayer層で定義されているため、
            // ここではdynamicまたは弱い型付けで処理
            if (eventData == null)
            {
                StopPeeking();
                return;
            }

            // イベントデータから覗き見方向と強度を取得
            // 注: Player層のPlayerPeekEventDataクラスを参照できないため、
            // リフレクションまたは共有インターフェースを使用する必要があります
            // 現在は簡易実装としてスキップしています
            // TODO: イベントデータの適切な処理方法を実装
        }

        // UpdatePeekState メソッドは、Player層とのインターフェース統合後に再実装予定
        // 現在はイベントデータの処理をスキップしています

        private Vector3 CalculatePeekOffset()
        {
            if (cameraStateMachine?.FollowTarget == null)
                return Vector3.zero;

            float offsetAmount = peekIntensity * 1.5f; // 覗き見オフセット量

            return currentPeekDirection switch
            {
                PeekDirection.Left => -cameraStateMachine.FollowTarget.right * offsetAmount,
                PeekDirection.Right => cameraStateMachine.FollowTarget.right * offsetAmount,
                PeekDirection.Over => Vector3.up * offsetAmount,
                PeekDirection.Under => Vector3.down * offsetAmount * 0.5f,
                _ => Vector3.zero
            };
        }

        private void ApplyCameraPeekOffset()
        {
            if (cameraStateMachine?.CameraRig == null)
                return;

            // スムーズにオフセットを適用
            Vector3 targetPosition = cameraStateMachine.FollowTarget.position + peekOffset;
            cameraStateMachine.CameraRig.position = Vector3.Lerp(
                cameraStateMachine.CameraRig.position,
                targetPosition,
                Time.deltaTime * peekTransitionSpeed
            );
        }

        private void StopPeeking()
        {
            isPeeking = false;
            currentPeekDirection = PeekDirection.None;
            peekOffset = Vector3.zero;
        }

        private void LateUpdate()
        {
            // 覗き見中の場合、毎フレームカメラ位置を更新
            if (isPeeking && cameraStateMachine != null)
            {
                ApplyCameraPeekOffset();
            }
        }

        /// <summary>
        /// 現在の覗き見状態を取得
        /// </summary>
        public bool IsPeeking => isPeeking;

        /// <summary>
        /// 現在の覗き見方向を取得
        /// </summary>
        public PeekDirection CurrentPeekDirection => currentPeekDirection;
    }
}