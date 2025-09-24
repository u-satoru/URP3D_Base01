using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// 縺励ｃ縺後∩・医け繝ｩ繧ｦ繝・ｼ峨さ繝槭Φ繝峨・螳夂ｾｩ縲・    /// 繝励Ξ繧､繝､繝ｼ縺ｾ縺溘・AI縺ｮ縺励ｃ縺後∩繧｢繧ｯ繧ｷ繝ｧ繝ｳ繧偵き繝励そ繝ｫ蛹悶＠縺ｾ縺吶・    /// 
    /// 荳ｻ縺ｪ讖溯・・・    /// - 縺励ｃ縺後∩迥ｶ諷九・髢句ｧ九→邨ゆｺ・    /// - 遘ｻ蜍暮溷ｺｦ縺ｮ螟画峩縺ｨ繧ｹ繝・Ν繧ｹ蜉ｹ譫・    /// - 繧ｳ繝ｪ繧ｸ繝ｧ繝ｳ繧ｵ繧､繧ｺ縺ｮ隱ｿ謨ｴ
    /// - 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ縺ｨ繧ｫ繝｡繝ｩ縺ｮ蛻ｶ蠕｡
    /// </summary>
    [System.Serializable]
    public class CrouchCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 縺励ｃ縺後∩縺ｮ遞ｮ鬘槭ｒ螳夂ｾｩ縺吶ｋ蛻玲嫌蝙・        /// </summary>
        public enum CrouchType
        {
            Normal,     // 騾壼ｸｸ縺ｮ縺励ｃ縺後∩
            Sneak,      // 繧ｹ繝・Ν繧ｹ驥崎ｦ悶・縺励ｃ縺後∩
            Cover,      // 驕ｮ阡ｽ迚ｩ蛻ｩ逕ｨ縺ｮ縺励ｃ縺後∩
            Slide       // 繧ｹ繝ｩ繧､繝・ぅ繝ｳ繧ｰ
        }

        [Header("Crouch Parameters")]
        public CrouchType crouchType = CrouchType.Normal;
        public bool toggleMode = true; // true: 繝医げ繝ｫ蠖｢蠑・ false: 謚ｼ縺礼ｶ壹￠繧句ｽ｢蠑・        public float speedMultiplier = 0.5f;
        public float heightReduction = 0.5f;

        [Header("Stealth Effects")]
        public float noiseReduction = 0.7f; // 髻ｳ縺ｮ蜑頑ｸ帷紫
        public float visibilityReduction = 0.3f; // 隕冶ｪ肴ｧ縺ｮ蜑頑ｸ帷紫
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
        /// 繝・ヵ繧ｩ繝ｫ繝医さ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public CrouchCommandDefinition()
        {
        }

        /// <summary>
        /// 繝代Λ繝｡繝ｼ繧ｿ莉倥″繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public CrouchCommandDefinition(CrouchType type, bool isToggle, float speedMult = 0.5f)
        {
            crouchType = type;
            toggleMode = isToggle;
            speedMultiplier = speedMult;
        }

        /// <summary>
        /// 縺励ｃ縺後∩繧ｳ繝槭Φ繝峨′螳溯｡悟庄閭ｽ縺九←縺・°繧貞愛螳壹＠縺ｾ縺・        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 蝓ｺ譛ｬ逧・↑螳溯｡悟庄閭ｽ諤ｧ繝√ぉ繝・け
            if (speedMultiplier < 0f || heightReduction < 0f || heightReduction > 1f) 
                return false;
            
            if (transitionDuration < 0f) 
                return false;

            // 繧ｳ繝ｳ繝・く繧ｹ繝医′縺ゅｋ蝣ｴ蜷医・霑ｽ蜉繝√ぉ繝・け
            if (context != null)
            {
                // 迴ｾ蝨ｨ縺ｮ蝨ｰ蠖｢繝√ぉ繝・け・域･譁憺擇縺ｧ縺ｯ荳榊庄遲会ｼ・                // 螟ｩ莠輔・鬮倥＆繝√ぉ繝・け・育ｫ九■荳翫′繧後↑縺・ｴ謇縺ｧ縺ｮ蛻ｶ髯撰ｼ・                // 迥ｶ諷狗焚蟶ｸ繝√ぉ繝・け・郁ｶｳ縺ｮ雋蛯ｷ遲会ｼ・                // 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ迥ｶ諷九メ繧ｧ繝・け・医ず繝｣繝ｳ繝嶺ｸｭ縺ｯ荳榊庄遲会ｼ・            }

            return true;
        }

        /// <summary>
        /// 縺励ｃ縺後∩繧ｳ繝槭Φ繝峨ｒ菴懈・縺励∪縺・        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new CrouchCommand(this, context);
        }
    }

    /// <summary>
    /// CrouchCommandDefinition縺ｫ蟇ｾ蠢懊☆繧句ｮ滄圀縺ｮ繧ｳ繝槭Φ繝牙ｮ溯｣・    /// </summary>
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
        /// 縺励ｃ縺後∩繧ｳ繝槭Φ繝峨・螳溯｡・        /// </summary>
        public void Execute()
        {
            if (executed) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.crouchType} crouch: toggle={definition.toggleMode}");
#endif

            // 繝医げ繝ｫ繝｢繝ｼ繝峨・蝣ｴ蜷医・迥ｶ諷九ｒ蛻・ｊ譖ｿ縺・            if (definition.toggleMode)
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
                // 謚ｼ縺礼ｶ壹￠繝｢繝ｼ繝峨・蝣ｴ蜷医・蟶ｸ縺ｫ縺励ｃ縺後∩髢句ｧ・                StartCrouch();
            }

            executed = true;
        }

        /// <summary>
        /// 縺励ｃ縺後∩迥ｶ諷九・髢句ｧ・        /// </summary>
        private void StartCrouch()
        {
            if (isCrouching) return;

            // 螳溯｡悟燕縺ｮ迥ｶ諷九ｒ菫晏ｭ假ｼ・ndo逕ｨ・・            SaveOriginalState();

            isCrouching = true;

            // 螳滄圀縺ｮ縺励ｃ縺後∩蜃ｦ逅・ｒ縺薙％縺ｫ螳溯｣・            if (context is MonoBehaviour mono)
            {
                // 繧ｳ繝ｩ繧､繝繝ｼ縺ｮ鬮倥＆隱ｿ謨ｴ
                if (definition.adjustColliderHeight && mono.GetComponent<CapsuleCollider>() != null)
                {
                    var collider = mono.GetComponent<CapsuleCollider>();
                    collider.height *= (1f - definition.heightReduction);
                    collider.center = new Vector3(collider.center.x, collider.center.y - (originalHeight * definition.heightReduction * 0.5f), collider.center.z);
                }

                // 遘ｻ蜍暮溷ｺｦ縺ｮ隱ｿ謨ｴ・・layerController縺ｨ縺ｮ騾｣謳ｺ・・                // 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ蛻ｶ蠕｡
                // 繧ｫ繝｡繝ｩ菴咲ｽｮ縺ｮ隱ｿ謨ｴ
                // 繧ｹ繝・Ν繧ｹ迥ｶ諷九・驕ｩ逕ｨ
                // 繧ｵ繧ｦ繝ｳ繝峨お繝輔ぉ繧ｯ繝・            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Started crouching");
#endif
        }

        /// <summary>
        /// 遶九■荳翫′繧雁・逅・        /// </summary>
        private void StandUp()
        {
            if (!isCrouching) return;

            // 螟ｩ莠輔メ繧ｧ繝・け・育ｫ九■荳翫′繧後ｋ縺九←縺・°・・            if (!CanStandUp())
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("Cannot stand up - ceiling too low");
#endif
                return;
            }

            isCrouching = false;

            // 迥ｶ諷九・蠕ｩ蜈・            RestoreOriginalState();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Stood up from crouch");
#endif
        }

        /// <summary>
        /// 遶九■荳翫′繧雁庄閭ｽ縺九メ繧ｧ繝・け
        /// </summary>
        private bool CanStandUp()
        {
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ縲・ｭ荳翫↓髫懷ｮｳ迚ｩ縺後↑縺・°Raycast縺ｧ繝√ぉ繝・け
            // 迴ｾ蝨ｨ縺ｯ蟶ｸ縺ｫtrue繧定ｿ斐☆
            return true;
        }

        /// <summary>
        /// 蜈・・迥ｶ諷九ｒ菫晏ｭ・        /// </summary>
        private void SaveOriginalState()
        {
            if (context is MonoBehaviour mono)
            {
                // 繧ｳ繝ｩ繧､繝繝ｼ縺ｮ鬮倥＆菫晏ｭ・                if (mono.GetComponent<CapsuleCollider>() != null)
                {
                    originalHeight = mono.GetComponent<CapsuleCollider>().height;
                }

                // 縺昴・莉悶・迥ｶ諷倶ｿ晏ｭ・                // originalSpeed = playerController.moveSpeed;
                // originalCameraPosition = camera.localPosition;
            }
        }

        /// <summary>
        /// 蜈・・迥ｶ諷九ｒ蠕ｩ蜈・        /// </summary>
        private void RestoreOriginalState()
        {
            if (context is MonoBehaviour mono)
            {
                // 繧ｳ繝ｩ繧､繝繝ｼ縺ｮ蠕ｩ蜈・                if (definition.adjustColliderHeight && mono.GetComponent<CapsuleCollider>() != null)
                {
                    var collider = mono.GetComponent<CapsuleCollider>();
                    collider.height = originalHeight;
                    collider.center = new Vector3(collider.center.x, 0f, collider.center.z);
                }

                // 縺昴・莉悶・迥ｶ諷句ｾｩ蜈・                // playerController.moveSpeed = originalSpeed;
                // camera.localPosition = originalCameraPosition;
            }
        }

        /// <summary>
        /// 謚ｼ縺礼ｶ壹￠繝｢繝ｼ繝峨〒縺ｮ縺励ｃ縺後∩邨ゆｺ・        /// </summary>
        public void EndCrouch()
        {
            if (!definition.toggleMode && isCrouching)
            {
                StandUp();
            }
        }

        /// <summary>
        /// Undo謫堺ｽ懶ｼ医＠繧・′縺ｿ迥ｶ諷九・蠑ｷ蛻ｶ隗｣髯､・・        /// </summary>
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
        /// 縺薙・繧ｳ繝槭Φ繝峨′Undo蜿ｯ閭ｽ縺九←縺・°
        /// </summary>
        public bool CanUndo => executed;

        /// <summary>
        /// 迴ｾ蝨ｨ縺励ｃ縺後ｓ縺ｧ縺・ｋ縺九←縺・°
        /// </summary>
        public bool IsCrouching => isCrouching;
    }
}
