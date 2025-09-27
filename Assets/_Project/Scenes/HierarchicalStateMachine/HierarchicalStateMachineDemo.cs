using UnityEngine;
using asterivo.Unity60.Features.Player.StateMachine;
using asterivo.Unity60.Features.AI.StateMachine;
using asterivo.Unity60.Features.Camera.StateMachine;
using asterivo.Unity60.Core.Patterns.StateMachine.Editor;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Scenes.HierarchicalStateMachine
{
    /// <summary>
    /// 階層化ステートマシンのデモンストレーション用コントローラー
    /// Player、AI、Cameraの階層化ステートマシンを統合テスト
    /// </summary>
    public class HierarchicalStateMachineDemo : MonoBehaviour
    {
        [TabGroup("Demo Control", "References")]
        [Required]
        [LabelText("Player Context")]
        [SerializeField] private PlayerContext playerContext;

        [TabGroup("Demo Control", "References")]
        [Required]
        [LabelText("Hierarchical AI")]
        [SerializeField] private HierarchicalAIStateMachine hierarchicalAI;

        [TabGroup("Demo Control", "References")]
        [Required]
        [LabelText("Hierarchical Camera")]
        [SerializeField] private HierarchicalCameraStateMachine hierarchicalCamera;

        [TabGroup("Demo Control", "Demo Settings")]
        [LabelText("Auto Demo Mode")]
        [SerializeField] private bool autoDemoMode = true;

        [TabGroup("Demo Control", "Demo Settings")]
        [PropertyRange(1f, 10f)]
        [LabelText("Demo Interval")]
        [SuffixLabel("s", overlay: true)]
        [SerializeField] private float demoInterval = 3f;

        [TabGroup("Demo Control", "Status")]
        [ReadOnly]
        [LabelText("Current Demo Phase")]
        [SerializeField] private string currentDemoPhase;

        [TabGroup("Demo Control", "Status")]
        [ReadOnly]
        [LabelText("Demo Timer")]
        [SerializeField] private float demoTimer;

        private int currentPhaseIndex = 0;
        private readonly string[] demoPhases = {
            "Player Grounded States",
            "Player Airborne States",
            "AI Patrol States",
            "AI Alert States",
            "Camera Third Person States",
            "Camera Aim States"
        };

        private void Start()
        {
            InitializeDemo();
        }

        private void Update()
        {
            if (autoDemoMode)
            {
                UpdateAutoDemo();
            }

            UpdateDemoStatus();
        }

        private void InitializeDemo()
        {
            currentDemoPhase = demoPhases[0];
            demoTimer = 0f;

            Debug.Log("[HierarchicalStateMachineDemo] Demo initialized");
        }

        private void UpdateAutoDemo()
        {
            demoTimer += Time.deltaTime;

            if (demoTimer >= demoInterval)
            {
                NextDemoPhase();
                demoTimer = 0f;
            }
        }

        private void UpdateDemoStatus()
        {
            currentDemoPhase = demoPhases[currentPhaseIndex];
        }

        private void NextDemoPhase()
        {
            currentPhaseIndex = (currentPhaseIndex + 1) % demoPhases.Length;
            ExecuteCurrentPhase();
        }

        private void ExecuteCurrentPhase()
        {
            string phase = demoPhases[currentPhaseIndex];

            switch (phase)
            {
                case "Player Grounded States":
                    DemoPlayerGroundedStates();
                    break;
                case "Player Airborne States":
                    DemoPlayerAirborneStates();
                    break;
                case "AI Patrol States":
                    DemoAIPatrolStates();
                    break;
                case "AI Alert States":
                    DemoAIAlertStates();
                    break;
                case "Camera Third Person States":
                    DemoCameraThirdPersonStates();
                    break;
                case "Camera Aim States":
                    DemoCameraAimStates();
                    break;
            }

            Debug.Log($"[HierarchicalStateMachineDemo] Executing phase: {phase}");
        }

        #region Demo Phase Implementations

        private void DemoPlayerGroundedStates()
        {
            if (playerContext == null) return;

            // Player Grounded状態の子状態をデモンストレーション
            // TODO: PlayerContextでの階層化ステート操作
            Debug.Log("[Demo] Player Grounded States - cycling through Idle, Walk, Run, Crouch");
        }

        private void DemoPlayerAirborneStates()
        {
            if (playerContext == null) return;

            // Player Airborne状態の子状態をデモンストレーション
            // TODO: PlayerContextでの階層化ステート操作
            Debug.Log("[Demo] Player Airborne States - cycling through Jump, Fall, Glide");
        }

        private void DemoAIPatrolStates()
        {
            if (hierarchicalAI == null) return;

            // AI Patrol状態の子状態をデモンストレーション
            hierarchicalAI.TransitionToState("Patrol");
            Debug.Log("[Demo] AI Patrol States - cycling through Walking, Waiting");
        }

        private void DemoAIAlertStates()
        {
            if (hierarchicalAI == null) return;

            // AI Alert状態の子状態をデモンストレーション
            hierarchicalAI.TransitionToState("Alert");

            // 仮想的なターゲットを設定してCombat/Searching状態をトリガー
            var demoTarget = UnityEngine.Camera.main?.transform;
            if (demoTarget != null)
            {
                hierarchicalAI.OnSightTarget(demoTarget);
            }

            Debug.Log("[Demo] AI Alert States - cycling through Combat, Searching");
        }

        private void DemoCameraThirdPersonStates()
        {
            if (hierarchicalCamera == null) return;

            // Camera ThirdPerson状態の子状態をデモンストレーション
            hierarchicalCamera.TransitionToState("ThirdPerson");
            Debug.Log("[Demo] Camera Third Person States - cycling through Follow, FreeLook");
        }

        private void DemoCameraAimStates()
        {
            if (hierarchicalCamera == null) return;

            // Camera Aim状態の子状態をデモンストレーション
            hierarchicalCamera.TransitionToState("Aim");
            Debug.Log("[Demo] Camera Aim States - cycling through Quick, Precise");
        }

        #endregion

        #region Manual Demo Controls

        [TabGroup("Demo Control", "Manual Controls")]
        [Button("Next Phase")]
        private void ManualNextPhase()
        {
            NextDemoPhase();
            demoTimer = 0f;
        }

        [TabGroup("Demo Control", "Manual Controls")]
        [Button("Previous Phase")]
        private void ManualPreviousPhase()
        {
            currentPhaseIndex = (currentPhaseIndex - 1 + demoPhases.Length) % demoPhases.Length;
            ExecuteCurrentPhase();
            demoTimer = 0f;
        }

        [TabGroup("Demo Control", "Manual Controls")]
        [Button("Reset Demo")]
        private void ResetDemo()
        {
            currentPhaseIndex = 0;
            demoTimer = 0f;
            ExecuteCurrentPhase();
        }

        #endregion

        #region Debug Controls

        [TabGroup("Demo Control", "Debug")]
        [Button("Open State Hierarchy Debugger")]
        private void OpenStateDebugger()
        {
#if UNITY_EDITOR
            StateHierarchyDebugWindow.ShowWindow();
#endif
        }

        [TabGroup("Demo Control", "Debug")]
        [Button("Log All State Info")]
        private void LogAllStateInfo()
        {
            if (hierarchicalAI != null)
            {
                Debug.Log($"[AI] Context: {(hierarchicalAI.Context != null ? "Active" : "Null")}");
            }

            if (hierarchicalCamera != null)
            {
                Debug.Log($"[Camera] Context: {(hierarchicalCamera.Context != null ? "Active" : "Null")}");
            }

            if (playerContext != null)
            {
                Debug.Log($"[Player] Position: {playerContext.Position}");
            }
        }

        #endregion

        #region Scene Setup Helpers

        [TabGroup("Demo Control", "Scene Setup")]
        [Button("Auto Setup Scene")]
        private void AutoSetupScene()
        {
            // 自動的にシーン内のコンポーネントを検索・設定
            if (playerContext == null)
            {
                // PlayerContext is not a MonoBehaviour, so we need to create or get it differently
                // For demo purposes, we'll create a simple context
                playerContext = new PlayerContext();
            }

            if (hierarchicalAI == null)
            {
                hierarchicalAI = FindFirstObjectByType<HierarchicalAIStateMachine>();
            }

            if (hierarchicalCamera == null)
            {
                hierarchicalCamera = FindFirstObjectByType<HierarchicalCameraStateMachine>();
            }

            Debug.Log($"[HierarchicalStateMachineDemo] Auto setup complete - " +
                     $"Player: {playerContext != null}, " +
                     $"AI: {hierarchicalAI != null}, " +
                     $"Camera: {hierarchicalCamera != null}");
        }

        [TabGroup("Demo Control", "Scene Setup")]
        [Button("Validate Setup")]
        private void ValidateSetup()
        {
            bool allValid = true;

            if (playerContext == null)
            {
                Debug.LogError("[Demo] PlayerContext not assigned!");
                allValid = false;
            }

            if (hierarchicalAI == null)
            {
                Debug.LogError("[Demo] HierarchicalAIStateMachine not assigned!");
                allValid = false;
            }

            if (hierarchicalCamera == null)
            {
                Debug.LogError("[Demo] HierarchicalCameraStateMachine not assigned!");
                allValid = false;
            }

            if (allValid)
            {
                Debug.Log("[Demo] All components properly assigned!");
            }
        }

        #endregion

        private void OnGUI()
        {
            if (!Application.isPlaying) return;

            // 簡易なGUIでのデモ情報表示
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            var boldStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            GUILayout.Label("Hierarchical State Machine Demo", boldStyle);
            GUILayout.Space(10);

            GUILayout.Label($"Phase: {currentDemoPhase}");
            GUILayout.Label($"Timer: {demoTimer:F1}s / {demoInterval:F1}s");
            GUILayout.Label($"Auto Mode: {(autoDemoMode ? "ON" : "OFF")}");

            GUILayout.Space(10);

            if (GUILayout.Button("Next Phase"))
            {
                ManualNextPhase();
            }

            if (GUILayout.Button("Toggle Auto Mode"))
            {
                autoDemoMode = !autoDemoMode;
            }

            GUILayout.EndArea();
        }
    }
}
