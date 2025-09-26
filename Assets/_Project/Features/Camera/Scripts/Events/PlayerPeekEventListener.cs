using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Camera.States;

namespace asterivo.Unity60.Features.Camera.Events
{
    /// <summary>
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ隕励″隕九い繧ｯ繧ｷ繝ｧ繝ｳ繧､繝吶Φ繝医ｒ蜿嶺ｿ｡縺励√き繝｡繝ｩ迥ｶ諷九ｒ譖ｴ譁ｰ縺吶ｋ繝ｪ繧ｹ繝翫・
    /// </summary>
    /// <remarks>
    /// Player螻､縺九ｉ縺ｮPeek繧､繝吶Φ繝医ｒGameEvent邨檎罰縺ｧ蜿嶺ｿ｡縺励・
    /// Camera螻､縺ｮ謖吝虚繧貞宛蠕｡縺吶ｋ縺薙→縺ｧ縲∝ｱ､髢薙・逍守ｵ仙粋繧貞ｮ溽樟縺励∪縺吶・
    /// </remarks>
    public class PlayerPeekEventListener : MonoBehaviour
    {
        [Header("Camera Components")]
        [SerializeField] private CameraStateMachine cameraStateMachine;

        [Header("Peek Settings")]
        [SerializeField] private float peekIntensity = 1.0f;
        [SerializeField] private float peekTransitionSpeed = 8f;

        // 迴ｾ蝨ｨ縺ｮ隕励″隕狗憾諷・
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
            // TODO: GameEvent繝吶・繧ｹ縺ｮ繧､繝吶Φ繝郁ｳｼ隱ｭ縺ｫ螟画峩
            // ServiceLocator/IEventManager縺ｮ螳溯｣・ｾ後↓譛牙柑蛹・
            /*
            var eventManager = ServiceLocator.TryGet<IEventManager>(out var em) ? em : null;
            eventManager?.Subscribe("PlayerPeek", OnPlayerPeekEvent);
            */
        }

        private void OnDisable()
        {
            // TODO: GameEvent繝吶・繧ｹ縺ｮ繧､繝吶Φ繝郁ｳｼ隱ｭ隗｣髯､縺ｫ螟画峩
            // ServiceLocator/IEventManager縺ｮ螳溯｣・ｾ後↓譛牙柑蛹・
            /*
            var eventManager = ServiceLocator.TryGet<IEventManager>(out var em) ? em : null;
            eventManager?.Unsubscribe("PlayerPeek", OnPlayerPeekEvent);
            */
        }

        /// <summary>
        /// Player螻､縺九ｉ縺ｮPeek繧､繝吶Φ繝医ワ繝ｳ繝峨Λ繝ｼ
        /// </summary>
        private void OnPlayerPeekEvent(object eventData)
        {
            // PlayerPeekEventData縺ｮ蠖｢蠑上〒繝・・繧ｿ繧貞女菫｡
            // 螳滄圀縺ｮPlayerPeekEventData繧ｯ繝ｩ繧ｹ縺ｯPlayer螻､縺ｧ螳夂ｾｩ縺輔ｌ縺ｦ縺・ｋ縺溘ａ縲・
            // 縺薙％縺ｧ縺ｯdynamic縺ｾ縺溘・蠑ｱ縺・梛莉倥￠縺ｧ蜃ｦ逅・
            if (eventData == null)
            {
                StopPeeking();
                return;
            }

            // 繧､繝吶Φ繝医ョ繝ｼ繧ｿ縺九ｉ隕励″隕区婿蜷代→蠑ｷ蠎ｦ繧貞叙蠕・
            // 豕ｨ: Player螻､縺ｮPlayerPeekEventData繧ｯ繝ｩ繧ｹ繧貞盾辣ｧ縺ｧ縺阪↑縺・◆繧√・
            // 繝ｪ繝輔Ξ繧ｯ繧ｷ繝ｧ繝ｳ縺ｾ縺溘・蜈ｱ譛峨う繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ繧剃ｽｿ逕ｨ縺吶ｋ蠢・ｦ√′縺ゅｊ縺ｾ縺・
            // 迴ｾ蝨ｨ縺ｯ邁｡譏灘ｮ溯｣・→縺励※繧ｹ繧ｭ繝・・縺励※縺・∪縺・
            // TODO: 繧､繝吶Φ繝医ョ繝ｼ繧ｿ縺ｮ驕ｩ蛻・↑蜃ｦ逅・婿豕輔ｒ螳溯｣・
        }

        // UpdatePeekState 繝｡繧ｽ繝・ラ縺ｯ縲￣layer螻､縺ｨ縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ邨ｱ蜷亥ｾ後↓蜀榊ｮ溯｣・ｺ亥ｮ・
        // 迴ｾ蝨ｨ縺ｯ繧､繝吶Φ繝医ョ繝ｼ繧ｿ縺ｮ蜃ｦ逅・ｒ繧ｹ繧ｭ繝・・縺励※縺・∪縺・

        private Vector3 CalculatePeekOffset()
        {
            if (cameraStateMachine?.FollowTarget == null)
                return Vector3.zero;

            float offsetAmount = peekIntensity * 1.5f; // 隕励″隕九が繝輔そ繝・ヨ驥・

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

            // 繧ｹ繝繝ｼ繧ｺ縺ｫ繧ｪ繝輔そ繝・ヨ繧帝←逕ｨ
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
            // 隕励″隕倶ｸｭ縺ｮ蝣ｴ蜷医∵ｯ弱ヵ繝ｬ繝ｼ繝繧ｫ繝｡繝ｩ菴咲ｽｮ繧呈峩譁ｰ
            if (isPeeking && cameraStateMachine != null)
            {
                ApplyCameraPeekOffset();
            }
        }

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ隕励″隕狗憾諷九ｒ蜿門ｾ・
        /// </summary>
        public bool IsPeeking => isPeeking;

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ隕励″隕区婿蜷代ｒ蜿門ｾ・
        /// </summary>
        public PeekDirection CurrentPeekDirection => currentPeekDirection;
    }
}


