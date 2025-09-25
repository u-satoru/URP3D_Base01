using UnityEngine;
using asterivo.Unity60.Core.Data;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Player
{
    /// <summary>
    /// プレイヤーのステルス行動（姿勢、騒音、視認性）を総合的に管理します。
    /// 移動モード（歩行、走行、しゃがみ等）に応じてパラメータを変化させ、ステルス状態をイベントで通知します。
    /// </summary>
    public class StealthMovementController : MonoBehaviour
    {
        [Header("Movement Modes")]
        /// <summary>
        /// 定義済みの移動モードの配列。各モードは移動速度、騒音レベル、視認性、キャラクターの高さなどを定義します。
        /// </summary>
        [Tooltip("定義済みの移動モードの配列")]
        [SerializeField] private MovementMode[] movementModes;
        /// <summary>
        /// 現在選択されている移動モードのインデックス。
        /// </summary>
        [Tooltip("現在選択されている移動モードのインデックス")]
        [SerializeField] private int currentModeIndex = 1;
        
        [Header("Components")]
        /// <summary>
        /// プレイヤーのCharacterControllerコンポーネント。
        /// </summary>
        [SerializeField] private CharacterController characterController;
        /// <summary>
        /// プレイヤーのTransformコンポーネント。
        /// </summary>
        [SerializeField] private Transform playerTransform;
        
        [Header("Current State")]
        /// <summary>
        /// 現在のプレイヤーの姿勢（例: Standing, Crouching, Prone）。
        /// </summary>
        [Tooltip("現在のプレイヤーの姿勢")]
        [SerializeField] private MovementStance currentStance = MovementStance.Standing;
        /// <summary>
        /// 現在の騒音レベル。移動速度や姿勢によって変化します。
        /// </summary>
        [Tooltip("現在の騒音レベル")]
        [SerializeField] private float currentNoiseLevel = 0.5f;
        /// <summary>
        /// 現在の視認性。姿勢、影、光源レベルによって変化します。
        /// </summary>
        [Tooltip("現在の視認性")]
        [SerializeField] private float currentVisibility = 1.0f;
        /// <summary>
        /// プレイヤーが影の中にいるかどうか。
        /// </summary>
        [Tooltip("影の中にいるかどうか")]
        [SerializeField] private bool isInShadow = false;
        
        [Header("Events")]
        /// <summary>
        /// プレイヤーの姿勢が変更された時に発行されるイベント。
        /// </summary>
        [Tooltip("姿勢が変更された時に発行されるイベント")]
        [SerializeField] private MovementStanceEvent onStanceChanged;
        /// <summary>
        /// 移動情報（騒音、視認性など）が更新された時に発行されるイベント。
        /// </summary>
        [Tooltip("移動情報（騒音、視認性など）が更新された時に発行されるイベント")]
        [SerializeField] private MovementInfoEvent onMovementInfoChanged;
        
        [Header("Settings")]
        /// <summary>
        /// 姿勢を変更する際のキャラクターの高さの遷移速度。
        /// </summary>
        [Tooltip("姿勢を変更する際のキャラクターの高さの遷移速度")]
        [SerializeField] private float stanceTransitionSpeed = 2f;
        /// <summary>
        /// 影を検出するためのレイヤーマスク。
        /// </summary>
        [Tooltip("影を検出するためのレイヤーマスク")]
        [SerializeField] private LayerMask shadowCheckLayers = -1;
        /// <summary>
        /// 影を検出する際の球体の半径。
        /// </summary>
        [Tooltip("影を検出する際の球体の半径")]
        [SerializeField] private float shadowCheckRadius = 0.5f;
        
        /// <summary>
        /// 現在のステルス移動情報。イベントで発行されます。
        /// </summary>
        private StealthMovementInfo currentMovementInfo;
        /// <summary>
        /// キャラクターの現在の高さ。
        /// </summary>
        private float currentHeight;
        /// <summary>
        /// キャラクターの目標の高さ。
        /// </summary>
        private float targetHeight;
        /// <summary>
        /// キャラクターの現在の速度。
        /// </summary>
        private Vector3 currentVelocity;
        
        /// <summary>
        /// 移動モードのパラメータを定義します。
        /// </summary>
        [System.Serializable]
        public class MovementMode
        {
            public string name = "Movement Mode";
            public MovementStance stance = MovementStance.Standing;
            public float moveSpeed = 4f;
            public float noiseLevel = 0.5f;
            public float visibilityMultiplier = 1f;
            public float characterHeight = 2f;
            public Vector3 cameraOffset = Vector3.zero;
        }
        
        /// <summary>
        /// スクリプトインスタンスがロードされたときに呼び出され、必要なコンポーネントを初期化し、移動モードを設定します。
        /// </summary>
        /// <summary>
        /// スクリプトインスタンスがロードされたときに呼び出され、必要なコンポーネントを初期化し、移動モードを設定します。
        /// </summary>
        private void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
                
            if (playerTransform == null)
                playerTransform = transform;
                
            InitializeMovementModes();
        }
        
        /// <summary>
        /// Inspectorで移動モードが設定されていない場合に、デフォルト値を設定します。
        /// </summary>
        private void InitializeMovementModes()
        {
            if (movementModes == null || movementModes.Length == 0)
            {
                movementModes = new MovementMode[]
                {
                    new MovementMode 
                    { 
                        name = "Prone", 
                        stance = MovementStance.Prone,
                        moveSpeed = 1f, 
                        noiseLevel = 0.1f, 
                        visibilityMultiplier = 0.3f,
                        characterHeight = 0.5f
                    },
                    new MovementMode 
                    { 
                        name = "Crouch", 
                        stance = MovementStance.Crouching,
                        moveSpeed = 2.5f, 
                        noiseLevel = 0.3f, 
                        visibilityMultiplier = 0.6f,
                        characterHeight = 1.2f
                    },
                    new MovementMode 
                    { 
                        name = "Walk", 
                        stance = MovementStance.Standing,
                        moveSpeed = 4f, 
                        noiseLevel = 0.5f, 
                        visibilityMultiplier = 1f,
                        characterHeight = 2f
                    },
                    new MovementMode 
                    { 
                        name = "Run", 
                        stance = MovementStance.Standing,
                        moveSpeed = 7f, 
                        noiseLevel = 1.0f, 
                        visibilityMultiplier = 1.2f,
                        characterHeight = 2f
                    }
                };
            }
            
            currentHeight = targetHeight = movementModes[currentModeIndex].characterHeight;
        }
        
        /// <summary>
        /// 最初のフレーム更新の前に呼び出され、初期の移動モードを適用します。
        /// </summary>
        private void Start()
        {
            ApplyMovementMode(currentModeIndex);
        }
        
        /// <summary>
        /// フレームごとに呼び出され、キャラクターの高さ、影の検出、移動情報を更新します。
        /// </summary>
        private void Update()
        {
            UpdateCharacterHeight();
            UpdateShadowDetection();
            UpdateMovementInfo();
        }
        
        /// <summary>
        /// しゃがみ状態と立ち状態を切り替えます。
        /// </summary>
        public void ToggleCrouch()
        {
            if (currentStance == MovementStance.Crouching)
            {
                SetStance(MovementStance.Standing);
            }
            else
            {
                SetStance(MovementStance.Crouching);
            }
        }
        
        /// <summary>
        /// 伏せ状態と立ち状態を切り替えます。
        /// </summary>
        public void ToggleProne()
        {
            if (currentStance == MovementStance.Prone)
            {
                SetStance(MovementStance.Standing);
            }
            else
            {
                SetStance(MovementStance.Prone);
            }
        }
        
        /// <summary>
        /// 指定された新しい姿勢にプレイヤーの状態を変更します。
        /// </summary>
        /// <param name="newStance">新しい姿勢。</param>
        public void SetStance(MovementStance newStance)
        {
            if (currentStance == newStance) return;
            
            int modeIndex = FindModeIndexForStance(newStance);
            if (modeIndex >= 0)
            {
                ApplyMovementMode(modeIndex);
            }
        }

        /// <summary>
        /// 走行状態を設定します。
        /// </summary>
        /// <param name="isRunning">走行状態にする場合はtrue、歩行状態に戻す場合はfalse。</param>
        public void SetRunning(bool isRunning)
        {
            if (isRunning)
            {
                // 走行モードのインデックスを検索
                for (int i = movementModes.Length - 1; i >= 0; i--)
                {
                    if (movementModes[i].moveSpeed > 6f) // 仮に走行速度を6以上と定義
                    {
                        ApplyMovementMode(i);
                        break;
                    }
                }
            }
            else
            {
                // 歩行モードに戻す
                SetStance(MovementStance.Standing);
            }
        }

        /// <summary>
        /// 指定された姿勢に対応する移動モードのインデックスを検索します。
        /// </summary>
        /// <param name="stance">検索する姿勢。</param>
        /// <returns>対応するモードのインデックス。見つからない場合は-1。</returns>
        private int FindModeIndexForStance(MovementStance stance)
        {
            for (int i = 0; i < movementModes.Length; i++)
            {
                if (movementModes[i].stance == stance)
                    return i;
            }
            return -1;
        }
        
        /// <summary>
        /// 指定されたインデックスの移動モードを適用します。
        /// </summary>
        /// <param name="modeIndex">適用する移動モードのインデックス。</param>
        private void ApplyMovementMode(int modeIndex)
        {
            if (modeIndex < 0 || modeIndex >= movementModes.Length) return;
            
            currentModeIndex = modeIndex;
            MovementMode mode = movementModes[modeIndex];
            
            currentStance = mode.stance;
            targetHeight = mode.characterHeight;
            
            onStanceChanged?.Raise(currentStance);
        }
        
        /// <summary>
        /// キャラクターの高さを目標の高さに滑らかに更新します。
        /// CharacterControllerの高さと中心を調整します。
        /// </summary>
        private void UpdateCharacterHeight()
        {
            if (Mathf.Abs(currentHeight - targetHeight) > 0.01f)
            {
                currentHeight = Mathf.Lerp(currentHeight, targetHeight, 
                    Time.deltaTime * stanceTransitionSpeed);
                    
                if (characterController != null)
                {
                    characterController.height = currentHeight;
                    Vector3 center = characterController.center;
                    center.y = currentHeight / 2f;
                    characterController.center = center;
                }
            }
        }
        
        /// <summary>
        /// プレイヤーが影の中にいるかどうかを検出します。
        /// 頭上から下向きにSphereCastを行い、ヒットがなければ影とみなします。
        /// </summary>
        private void UpdateShadowDetection()
        {
            RaycastHit hit;
            Vector3 checkPosition = transform.position + Vector3.up * 2f;
            
            // 仮の実装：頭上から下向きにレイを飛ばし、何もヒットしなければ影とみなす
            isInShadow = !Physics.SphereCast(checkPosition, shadowCheckRadius, 
                Vector3.down, out hit, 10f, shadowCheckLayers);
        }
        
        /// <summary>
        /// 現在の移動情報（騒音レベル、視認性など）を計算し、イベントを発行します。
        /// 影や光源レベルに応じて視認性を調整します。
        /// </summary>
        private void UpdateMovementInfo()
        {
            MovementMode currentMode = movementModes[currentModeIndex];
            
            float lightLevel = CalculateLightLevel();
            float finalVisibility = currentMode.visibilityMultiplier * lightLevel;
            
            if (isInShadow)
                finalVisibility *= 0.5f;
                
            currentNoiseLevel = currentMode.noiseLevel * GetVelocityMagnitude();
            currentVisibility = finalVisibility;
            
            currentMovementInfo = new StealthMovementInfo
            {
                stance = currentStance,
                velocity = GetComponent<Rigidbody>()?.linearVelocity ?? Vector3.zero,
                noiseLevel = currentNoiseLevel, // 騒音レベル
                visibilityLevel = currentVisibility, // 視認性レベル
                isInCover = isInShadow, // 隠蔽状態
                concealmentLevel = isInShadow ? ConcealmentLevel.Medium : ConcealmentLevel.None
            };
            
            onMovementInfoChanged?.Raise(currentMovementInfo);
        }
        
        /// <summary>
        /// 現在の光源レベルを計算します。
        /// プレイヤーの周囲にあるライトソースの影響を総合して光量を計算します。
        /// </summary>
        /// <returns>計算された光源レベル（0.0-1.0）。</returns>
        private float CalculateLightLevel()
        {
            float totalLightIntensity = 0.0f;
            Vector3 playerPosition = transform.position;
            
            // 周囲のライトを検出して光量を計算
            Light[] nearbyLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            
            foreach (Light light in nearbyLights)
            {
                if (light == null || !light.enabled || !light.gameObject.activeInHierarchy)
                    continue;
                    
                float distance = Vector3.Distance(playerPosition, light.transform.position);
                
                // ライトタイプに応じた影響範囲チェック
                float lightInfluence = 0.0f;
                
                switch (light.type)
                {
                    case LightType.Directional:
                        // 太陽光などの方向光は距離に関係なく影響
                        lightInfluence = light.intensity * 0.3f; // 基準値として30%
                        break;
                        
                    case LightType.Point:
                        // 点光源は距離の二乗に反比例
                        if (distance <= light.range)
                        {
                            float attenuation = 1.0f - (distance * distance) / (light.range * light.range);
                            lightInfluence = light.intensity * attenuation * 0.8f;
                        }
                        break;
                        
                    case LightType.Spot:
                        // スポットライトは方向と距離の両方を考慮
                        if (distance <= light.range)
                        {
                            Vector3 directionToPlayer = (playerPosition - light.transform.position).normalized;
                            float angle = Vector3.Angle(light.transform.forward, directionToPlayer);
                            
                            if (angle <= light.spotAngle * 0.5f)
                            {
                                float attenuation = 1.0f - (distance / light.range);
                                float spotAttenuation = 1.0f - (angle / (light.spotAngle * 0.5f));
                                lightInfluence = light.intensity * attenuation * spotAttenuation * 0.9f;
                            }
                        }
                        break;
                }
                
                totalLightIntensity += lightInfluence;
            }
            
            // 環境光の基準値を追加（完全な暗闇を避けるため）
            totalLightIntensity += RenderSettings.ambientIntensity * 0.2f;
            
            // 0.0-1.0の範囲にクランプ
            return Mathf.Clamp01(totalLightIntensity);
        }
        
        /// <summary>
        /// 現在の移動速度の大きさを取得します。
        /// CharacterControllerの速度を現在の移動モードの速度で正規化します。
        /// </summary>
        /// <returns>正規化された速度の大きさ。</returns>
        private float GetVelocityMagnitude()
        {
            if (characterController != null && movementModes[currentModeIndex].moveSpeed > 0)
                return characterController.velocity.magnitude / movementModes[currentModeIndex].moveSpeed;
            return 0f;
        }
        
        /// <summary>
        /// 現在の移動モードでの移動速度を取得します。
        /// </summary>
        public float GetCurrentMoveSpeed()
        {
            return movementModes[currentModeIndex].moveSpeed;
        }
        
        /// <summary>
        /// 現在の姿勢を取得します。
        /// </summary>
        public MovementStance GetCurrentStance() => currentStance;

        /// <summary>
        /// 現在の騒音レベルを取得します。
        /// </summary>
        public float GetNoiseLevel() => currentNoiseLevel;

        /// <summary>
        /// 現在の視認性を取得します。
        /// </summary>
        public float GetVisibility() => currentVisibility;

        /// <summary>
        /// プレイヤーが影の中にいるかどうかを取得します。
        /// </summary>
        public bool IsInShadow() => isInShadow;

        /// <summary>
        /// 現在のステルス移動情報を取得します。
        /// </summary>
        public StealthMovementInfo GetMovementInfo() => currentMovementInfo;
    }
}