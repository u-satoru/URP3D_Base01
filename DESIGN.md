# DESIGN.md - Unity 6 3Dゲーム基盤プロジェクト 技術設計書

## エグゼクティブサマリー

本設計書は、Unity 6ベースの究極の3Dゲーム開発基盤テンプレートの技術実装方針を定義します。3層アーキテクチャ（Core ← Feature ← Template）を中核とし、ServiceLocator + Event駆動のハイブリッドアーキテクチャを採用します。

### 核心価値の技術実現
- **Clone & Create**: 1分セットアップ（97%短縮）
- **Learn & Grow**: 学習コスト70%削減
- **Ship & Scale**: プロトタイプ→プロダクション対応

### 定量目標
- セットアップ時間: 30分→1分（97%短縮）
- 学習コスト: 40時間→12時間（70%削減）
- メモリ効率: ObjectPoolで95%削減
- 実行速度: 67%向上

## 1. アーキテクチャ設計

### 1.1 3層アーキテクチャ（最重要）

```
Assets/_Project/
├── Core/              # ゲームのOS - ジャンル非依存の普遍的な仕組み
│   ├── Services/      # ServiceLocator基盤
│   ├── Events/        # イベント駆動システム
│   ├── Commands/      # コマンドパターン+ObjectPool
│   ├── Patterns/      # デザインパターン基盤
│   └── Audio/         # オーディオシステム
├── Features/          # ゲームのアプリケーション - Core層を活用した機能部品
│   ├── Player/        # プレイヤー機能
│   ├── AI/           # AI機能
│   ├── Camera/       # カメラ制御
│   └── Combat/       # 戦闘システム
└── Features/Templates/ # ゲームのドキュメント - Feature層の組み合わせ
    ├── Stealth/       # ステルスアクション
    ├── SurvivalHorror/# サバイバルホラー
    ├── FPS/          # 一人称シューティング
    ├── TPS/          # 三人称シューティング
    ├── Platformer/   # プラットフォーマー
    └── ActionRPG/    # アクションRPG
```

### 1.2 依存関係制御
- **依存方向**: Template → Feature → Core（逆方向禁止）
- **Assembly Definition強制**: コンパイル時チェック
- **通信方式**: GameEvent経由のイベント駆動通信

### 1.3 名前空間規約
- **Root**: `asterivo.Unity60`
- **Core層**: `asterivo.Unity60.Core.*`
- **Feature層**: `asterivo.Unity60.Features.*`
- **Template層**: `asterivo.Unity60.Features.Templates.*`
- **Tests**: `asterivo.Unity60.Tests.*`

## 2. コアパターン実装設計

### 2.1 ServiceLocator + Event駆動ハイブリッド

```csharp
namespace asterivo.Unity60.Core.Services
{
    public interface IService
    {
        void Initialize();
        void Shutdown();
    }

    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, IService> _services = new();
        private static readonly object _lock = new();

        public static void Register<T>(T service) where T : IService
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (!_services.ContainsKey(type))
                {
                    _services[type] = service;
                    service.Initialize();
                }
            }
        }

        public static T Get<T>() where T : IService
        {
            lock (_lock)
            {
                if (_services.TryGetValue(typeof(T), out var service))
                    return (T)service;
                throw new InvalidOperationException($"Service {typeof(T)} not registered");
            }
        }

        public static bool TryGet<T>(out T service) where T : IService
        {
            lock (_lock)
            {
                if (_services.TryGetValue(typeof(T), out var s))
                {
                    service = (T)s;
                    return true;
                }
                service = default;
                return false;
            }
        }

        public static void Clear()
        {
            lock (_lock)
            {
                foreach (var service in _services.Values)
                    service.Shutdown();
                _services.Clear();
            }
        }
    }
}
```

### 2.2 イベント駆動アーキテクチャ

```csharp
namespace asterivo.Unity60.Core.Events
{
    [CreateAssetMenu(menuName = "Game/Events/Game Event")]
    public class GameEvent : ScriptableObject
    {
        private readonly HashSet<IGameEventListener> _listeners = new();

        public void Raise()
        {
            foreach (var listener in _listeners)
                listener.OnEventRaised();
        }

        public void RegisterListener(IGameEventListener listener) =>
            _listeners.Add(listener);

        public void UnregisterListener(IGameEventListener listener) =>
            _listeners.Remove(listener);
    }

    public interface IGameEventListener
    {
        void OnEventRaised();
    }

    // 型安全なジェネリック版
    public class GameEvent<T> : ScriptableObject
    {
        private readonly HashSet<IGameEventListener<T>> _listeners = new();

        public void Raise(T data)
        {
            foreach (var listener in _listeners)
                listener.OnEventRaised(data);
        }
    }
}
```

### 2.3 コマンドパターン + ObjectPool統合

```csharp
namespace asterivo.Unity60.Core.Commands
{
    public interface ICommand
    {
        void Execute();
        bool CanExecute();
    }

    public interface IPoolableCommand : ICommand
    {
        void Reset();
    }

    public static class CommandPoolManager
    {
        private static readonly Dictionary<Type, Queue<IPoolableCommand>> _pools = new();
        private const int DefaultPoolSize = 50;

        public static T GetCommand<T>() where T : IPoolableCommand, new()
        {
            var type = typeof(T);
            if (!_pools.ContainsKey(type))
                InitializePool<T>();

            if (_pools[type].Count > 0)
            {
                var command = (T)_pools[type].Dequeue();
                command.Reset();
                return command;
            }
            return new T();
        }

        public static void ReturnCommand<T>(T command) where T : IPoolableCommand
        {
            var type = typeof(T);
            if (!_pools.ContainsKey(type))
                _pools[type] = new Queue<IPoolableCommand>();

            command.Reset();
            _pools[type].Enqueue(command);
        }

        private static void InitializePool<T>() where T : IPoolableCommand, new()
        {
            var type = typeof(T);
            _pools[type] = new Queue<IPoolableCommand>();
            for (int i = 0; i < DefaultPoolSize; i++)
                _pools[type].Enqueue(new T());
        }
    }
}
```

### 2.4 DamageCommand実装例

```csharp
namespace asterivo.Unity60.Core.Commands
{
    public class DamageCommand : IPoolableCommand, IUndoableCommand
    {
        private IHealth _target;
        private float _damage;
        private float _actualDamageDealt;

        public static DamageCommand Create(IHealth target, float damage)
        {
            var command = CommandPoolManager.GetCommand<DamageCommand>();
            command._target = target;
            command._damage = damage;
            return command;
        }

        public void Execute()
        {
            if (!CanExecute()) return;

            var previousHealth = _target.CurrentHealth;
            _target.TakeDamage(_damage);
            _actualDamageDealt = previousHealth - _target.CurrentHealth;
        }

        public bool CanExecute() => _target?.IsAlive == true && _damage > 0;

        public void Undo()
        {
            if (CanUndo())
                _target.Heal(_actualDamageDealt);
        }

        public bool CanUndo() => _target != null && _actualDamageDealt > 0;

        public void Reset()
        {
            _target = null;
            _damage = 0;
            _actualDamageDealt = 0;
        }
    }
}
```

### 2.5 階層化ステートマシン（HSM）

```csharp
namespace asterivo.Unity60.Core.Patterns.StateMachine
{
    public interface IState
    {
        void OnEnter();
        void OnUpdate();
        void OnExit();
        IState Parent { get; set; }
        List<IState> SubStates { get; }
    }

    public abstract class HierarchicalState : IState
    {
        public IState Parent { get; set; }
        public List<IState> SubStates { get; } = new();
        private IState _currentSubState;

        public virtual void OnEnter()
        {
            if (SubStates.Count > 0)
            {
                _currentSubState = SubStates[0];
                _currentSubState.OnEnter();
            }
        }

        public virtual void OnUpdate() => _currentSubState?.OnUpdate();
        public virtual void OnExit() => _currentSubState?.OnExit();

        public void ChangeSubState(IState newState)
        {
            if (!SubStates.Contains(newState)) return;

            _currentSubState?.OnExit();
            _currentSubState = newState;
            _currentSubState.OnEnter();
        }
    }

    public class StateMachine<T> where T : System.Enum
    {
        private Dictionary<T, IState> _states = new();
        private IState _currentState;
        public T CurrentStateKey { get; private set; }

        public void AddState(T key, IState state) => _states[key] = state;

        public void ChangeState(T newStateKey)
        {
            if (!_states.ContainsKey(newStateKey)) return;

            _currentState?.OnExit();
            CurrentStateKey = newStateKey;
            _currentState = _states[newStateKey];
            _currentState.OnEnter();
        }

        public void Update() => _currentState?.OnUpdate();
    }
}
```

## 3. システム実装設計

### 3.1 マルチモーダルAIセンサーシステム

```csharp
namespace asterivo.Unity60.Features.AI.Sensors
{
    public class SensorFusionSystem : MonoBehaviour
    {
        [SerializeField] private VisualSensor visualSensor;
        [SerializeField] private AuditorySensor auditorySensor;
        [SerializeField] private OlfactorySensor olfactorySensor;

        private float _alertLevel;
        public float AlertLevel => _alertLevel;

        void Update()
        {
            // センサー情報の重み付け統合
            float visualWeight = 0.5f;
            float auditoryWeight = 0.3f;
            float olfactoryWeight = 0.2f;

            _alertLevel = visualSensor.GetDetectionLevel() * visualWeight +
                         auditorySensor.GetDetectionLevel() * auditoryWeight +
                         olfactorySensor.GetDetectionLevel() * olfactoryWeight;

            UpdateAIState();
        }

        private void UpdateAIState()
        {
            AIState newState = _alertLevel switch
            {
                > 0.8f => AIState.Combat,
                > 0.5f => AIState.Alert,
                > 0.2f => AIState.Suspicious,
                _ => AIState.Patrol
            };

            var eventManager = ServiceLocator.Get<IEventManager>();
            eventManager?.RaiseEvent($"AI_StateChange_{newState}", gameObject);
        }
    }
}
```

### 3.2 視覚センサー実装

```csharp
namespace asterivo.Unity60.Features.AI.Sensors
{
    public class VisualSensor : MonoBehaviour
    {
        [SerializeField] private float viewDistance = 15f;
        [SerializeField] private float viewAngle = 45f;
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private LayerMask obstructionMask;

        private List<Transform> _visibleTargets = new();
        private float _detectionLevel;

        public float GetDetectionLevel() => _detectionLevel;
        public List<Transform> VisibleTargets => _visibleTargets;

        void Update()
        {
            _visibleTargets.Clear();
            _detectionLevel = 0f;

            Collider[] targetsInView = Physics.OverlapSphere(
                transform.position, viewDistance, targetMask);

            foreach (var target in targetsInView)
            {
                Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
                float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);

                if (angleToTarget < viewAngle / 2f)
                {
                    float distToTarget = Vector3.Distance(transform.position, target.transform.position);

                    if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, obstructionMask))
                    {
                        _visibleTargets.Add(target.transform);

                        float distanceScore = 1f - (distToTarget / viewDistance);
                        float angleScore = 1f - (angleToTarget / (viewAngle / 2f));
                        _detectionLevel = Mathf.Max(_detectionLevel, distanceScore * angleScore);
                    }
                }
            }
        }
    }
}
```

### 3.3 サバイバルホラーシステム

#### リソース管理システム

```csharp
namespace asterivo.Unity60.Features.Templates.SurvivalHorror.Systems
{
    public class ResourceManager : IService
    {
        private Dictionary<string, int> _resources = new();
        private const int MaxInventorySlots = 8;

        public void Initialize()
        {
            _resources["ammo_pistol"] = 15;
            _resources["health_herb"] = 2;
            _resources["key_items"] = 0;
        }

        public bool TryUseResource(string resourceType, int amount)
        {
            if (_resources.ContainsKey(resourceType) && _resources[resourceType] >= amount)
            {
                _resources[resourceType] -= amount;

                var eventManager = ServiceLocator.Get<IEventManager>();
                eventManager?.RaiseEvent("ResourceUsed", new ResourceData(resourceType, amount));
                return true;
            }
            return false;
        }

        public bool TryAddResource(string resourceType, int amount)
        {
            if (GetTotalItemCount() >= MaxInventorySlots)
                return false;

            if (!_resources.ContainsKey(resourceType))
                _resources[resourceType] = 0;

            _resources[resourceType] += amount;
            return true;
        }

        private int GetTotalItemCount() => _resources.Values.Sum();
        public void Shutdown() => _resources.Clear();
    }
}
```

#### 正気度システム

```csharp
namespace asterivo.Unity60.Features.Templates.SurvivalHorror.Atmosphere
{
    public class SanitySystem : MonoBehaviour
    {
        [SerializeField] private float maxSanity = 100f;
        [SerializeField] private float currentSanity;
        [SerializeField] private float darknessDrainRate = 2f;

        private HallucinationController _hallucinationController;

        void Start()
        {
            currentSanity = maxSanity;
            _hallucinationController = GetComponent<HallucinationController>();
        }

        void Update()
        {
            // 暗闇による正気度低下
            if (RenderSettings.ambientIntensity < 0.3f)
                DecreaseSanity(darknessDrainRate * Time.deltaTime);

            ApplySanityEffects();
        }

        void ApplySanityEffects()
        {
            float sanityPercentage = currentSanity / maxSanity;

            var (level, penalty) = sanityPercentage switch
            {
                < 0.2f => (HallucinationLevel.Severe, 0.5f),
                < 0.4f => (HallucinationLevel.Moderate, 0.8f),
                < 0.6f => (HallucinationLevel.Light, 0.9f),
                < 0.8f => (HallucinationLevel.Minimal, 1.0f),
                _ => (HallucinationLevel.None, 1.0f)
            };

            _hallucinationController?.SetLevel(level);
            ApplyControlPenalty(penalty);
        }

        private void ApplyControlPenalty(float multiplier)
        {
            ServiceLocator.Get<IInputManager>()?.SetSensitivityMultiplier(multiplier);
        }

        public void DecreaseSanity(float amount)
        {
            currentSanity = Mathf.Max(0, currentSanity - amount);

            var eventManager = ServiceLocator.Get<IEventManager>();
            eventManager?.RaiseEvent("SanityChanged", currentSanity / maxSanity);
        }
    }
}
```

#### ストーカー型AI

```csharp
namespace asterivo.Unity60.Features.Templates.SurvivalHorror.AI
{
    public class StalkerAIController : MonoBehaviour
    {
        private enum StalkerState
        {
            Dormant, Hunting, Stalking, Ambush, Attack, Retreat
        }

        private StateMachine<StalkerState> _stateMachine;
        private Transform _player;
        private NavMeshAgent _agent;
        private List<Vector3> _playerFrequentPositions = new();

        [SerializeField] private float respawnDelay = 180f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float stalkingDistance = 10f;

        void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _player = GameObject.FindWithTag("Player").transform;
            InitializeStateMachine();
            StartCoroutine(RespawnCycle());
        }

        void InitializeStateMachine()
        {
            _stateMachine = new StateMachine<StalkerState>();

            _stateMachine.AddState(StalkerState.Dormant, new DormantState(this));
            _stateMachine.AddState(StalkerState.Hunting, new HuntingState(this));
            _stateMachine.AddState(StalkerState.Stalking, new StalkingState(this));
            _stateMachine.AddState(StalkerState.Ambush, new AmbushState(this));
            _stateMachine.AddState(StalkerState.Attack, new AttackState(this));
            _stateMachine.AddState(StalkerState.Retreat, new RetreatState(this));

            _stateMachine.ChangeState(StalkerState.Dormant);
        }

        void Update()
        {
            _stateMachine.Update();
            LearnPlayerBehavior();
        }

        void LearnPlayerBehavior()
        {
            if (Vector3.Distance(transform.position, _player.position) < 20f)
            {
                _playerFrequentPositions.Add(_player.position);
                if (_playerFrequentPositions.Count > 50)
                    _playerFrequentPositions.RemoveAt(0);
            }
        }

        public Vector3 PredictPlayerPosition()
        {
            if (_playerFrequentPositions.Count > 10)
            {
                Vector3 centroid = Vector3.zero;
                for (int i = _playerFrequentPositions.Count - 10; i < _playerFrequentPositions.Count; i++)
                    centroid += _playerFrequentPositions[i];
                return centroid / 10f;
            }
            return _player.position;
        }

        IEnumerator RespawnCycle()
        {
            while (true)
            {
                yield return new WaitForSeconds(respawnDelay);

                if (_stateMachine.CurrentStateKey == StalkerState.Retreat)
                {
                    RespawnAtRandomLocation();
                    _stateMachine.ChangeState(StalkerState.Hunting);
                }
            }
        }

        void RespawnAtRandomLocation()
        {
            Vector3 randomDirection = Random.insideUnitSphere * 30f;
            randomDirection.y = 0;
            Vector3 spawnPosition = _player.position + randomDirection;

            if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, 10f, NavMesh.AllAreas))
                transform.position = hit.position;
        }
    }
}
```

### 3.4 Cinemachine統合カメラシステム

```csharp
namespace asterivo.Unity60.Features.Camera
{
    public class CameraManager : MonoBehaviour, IService
    {
        [SerializeField] private CinemachineVirtualCamera firstPersonCamera;
        [SerializeField] private CinemachineVirtualCamera thirdPersonCamera;
        [SerializeField] private CinemachineVirtualCamera aimCamera;
        [SerializeField] private CinemachineVirtualCamera coverCamera;

        private Dictionary<CameraMode, CinemachineVirtualCamera> _cameras;
        private CameraMode _currentMode;

        public void Initialize()
        {
            _cameras = new Dictionary<CameraMode, CinemachineVirtualCamera>
            {
                { CameraMode.FirstPerson, firstPersonCamera },
                { CameraMode.ThirdPerson, thirdPersonCamera },
                { CameraMode.Aim, aimCamera },
                { CameraMode.Cover, coverCamera }
            };

            SetCameraMode(CameraMode.ThirdPerson);
        }

        public void SetCameraMode(CameraMode mode)
        {
            foreach (var cam in _cameras.Values)
                cam.Priority = 0;

            if (_cameras.ContainsKey(mode))
            {
                _cameras[mode].Priority = 10;
                _currentMode = mode;

                ServiceLocator.Get<IEventManager>()?.RaiseEvent("CameraChanged", mode);
            }
        }

        public void Shutdown() { }
    }

    public enum CameraMode
    {
        FirstPerson, ThirdPerson, Aim, Cover
    }
}
```

## 4. データアーキテクチャ

### 4.1 ScriptableObject階層設計

```
GameData/
├── Characters/
│   ├── CharacterStats.asset
│   └── PlayerConfig.asset
├── Items/
│   ├── Weapons/
│   ├── Consumables/
│   └── KeyItems/
├── Events/
│   ├── GameEvents/
│   └── PlayerEvents/
├── AI/
│   └── SensorSettings/
│       ├── VisualSensorProfile.asset
│       └── AuditorySensorProfile.asset
└── Templates/
    ├── StealthTemplate.asset
    └── SurvivalHorrorTemplate.asset
```

### 4.2 データ検証システム

```csharp
namespace asterivo.Unity60.Core.Validation
{
    public static class DataValidationSystem
    {
        public static bool ValidateProjectData()
        {
            bool isValid = true;

            var allAssets = Resources.LoadAll<ScriptableObject>("GameData");
            foreach (var asset in allAssets)
            {
                if (!ValidateAsset(asset))
                {
                    Debug.LogError($"Validation failed for {asset.name}");
                    isValid = false;
                }
            }

            return isValid && ValidateDependencies() && ValidatePerformanceMetrics();
        }

        private static bool ValidateAsset(ScriptableObject asset)
        {
            // Odin Validatorを使用した検証
            return true;
        }

        private static bool ValidateDependencies()
        {
            // 3層アーキテクチャ制約の確認
            return true;
        }

        private static bool ValidatePerformanceMetrics()
        {
            // メモリ使用量、実行速度の基準確認
            return true;
        }
    }
}
```

## 5. パフォーマンス最適化戦略

### 5.1 UniTask統合による非同期最適化

```csharp
namespace asterivo.Unity60.Core.Async
{
    public static class AsyncHelper
    {
        public static async UniTask DelayedAction(float seconds, System.Action action,
            CancellationToken cancellationToken = default)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(seconds), cancellationToken: cancellationToken);
            action?.Invoke();
        }

        public static async UniTask ProcessLargeData<T>(IEnumerable<T> data,
            System.Action<T> processor, int itemsPerFrame = 10)
        {
            int processed = 0;
            foreach (var item in data)
            {
                processor(item);
                if (++processed % itemsPerFrame == 0)
                    await UniTask.Yield();
            }
        }

        public static async UniTask<T> LoadResourceAsync<T>(string path) where T : UnityEngine.Object
        {
            var request = Resources.LoadAsync<T>(path);
            await request;
            return request.asset as T;
        }
    }
}
```

### 5.2 メモリ最適化実装

```csharp
namespace asterivo.Unity60.Core.Memory
{
    public static class MemoryOptimizer
    {
        private static readonly System.Diagnostics.Stopwatch _gcStopwatch = new();

        public static void OptimizeMemory()
        {
            Resources.UnloadUnusedAssets();

            if (ShouldRunGC())
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.GC.Collect();
            }
        }

        private static bool ShouldRunGC() => _gcStopwatch.Elapsed.TotalSeconds > 30;

        public static void MonitorMemoryUsage()
        {
            long usedMemory = System.GC.GetTotalMemory(false);

            if (usedMemory > 500 * 1024 * 1024) // 500MB超過
            {
                Debug.LogWarning($"High memory usage: {usedMemory / (1024 * 1024)}MB");
                OptimizeMemory();
            }
        }
    }
}
```

## 6. テンプレート実装設計

### 6.1 ステルステンプレート

```csharp
namespace asterivo.Unity60.Features.Templates.Stealth
{
    [CreateAssetMenu(menuName = "Templates/Stealth Template")]
    public class StealthTemplateConfig : ScriptableObject
    {
        [Header("AI Settings")]
        public float visualDetectionRange = 15f;
        public float auditoryDetectionRange = 20f;
        public float alertGrowthRate = 0.5f;

        [Header("Player Settings")]
        public float sneakSpeedMultiplier = 0.5f;
        public float noiseLevelWalking = 0.5f;
        public float noiseLevelRunning = 1.0f;

        [Header("Environment")]
        public LayerMask shadowLayer;
        public float shadowConcealmentBonus = 0.5f;
    }

    public class StealthGameManager : MonoBehaviour
    {
        [SerializeField] private StealthTemplateConfig config;

        void Start()
        {
            InitializeStealthSystems();
        }

        void InitializeStealthSystems()
        {
            var allAI = FindObjectsOfType<AIController>();
            foreach (var ai in allAI)
            {
                ai.GetComponent<VisualSensor>()?.SetDetectionRange(config.visualDetectionRange);
                ai.GetComponent<AuditorySensor>()?.SetDetectionRange(config.auditoryDetectionRange);
            }
        }
    }
}
```

### 6.2 アクションRPGテンプレート

```csharp
namespace asterivo.Unity60.Features.Templates.ActionRPG
{
    [CreateAssetMenu(menuName = "Templates/ActionRPG Template")]
    public class ActionRPGTemplateConfig : ScriptableObject
    {
        [Header("Character System")]
        public CharacterClass[] availableClasses;
        public int maxLevel = 50;
        public AnimationCurve experienceCurve;

        [Header("Combat System")]
        public float baseDamage = 10f;
        public float criticalMultiplier = 2f;

        [Header("Inventory")]
        public int inventorySlots = 50;
    }

    public class CharacterStatsManager : MonoBehaviour
    {
        [SerializeField] private ActionRPGTemplateConfig config;

        private int _currentLevel = 1;
        private float _currentExp = 0;
        private Dictionary<StatType, float> _baseStats = new();

        public void AddExperience(float exp)
        {
            _currentExp += exp;
            CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            float requiredExp = config.experienceCurve.Evaluate(_currentLevel);
            if (_currentExp >= requiredExp && _currentLevel < config.maxLevel)
            {
                _currentLevel++;
                _currentExp -= requiredExp;
                ApplyLevelUpBonuses();

                ServiceLocator.Get<IEventManager>()?.RaiseEvent("LevelUp", _currentLevel);
            }
        }

        private void ApplyLevelUpBonuses()
        {
            _baseStats[StatType.Health] += 10;
            _baseStats[StatType.Mana] += 5;
            _baseStats[StatType.Strength] += 2;
        }
    }

    public enum StatType
    {
        Health, Mana, Strength, Dexterity, Intelligence, Attack, Defense
    }
}
```

## 7. セットアップウィザード実装

```csharp
namespace asterivo.Unity60.Core.Setup
{
    public class InteractiveSetupWizard : EditorWindow
    {
        private enum SetupPhase
        {
            Welcome, GenreSelection, FeatureConfiguration, Validation, Complete
        }

        private SetupPhase _currentPhase;
        private GenreType _selectedGenre;
        private Dictionary<string, bool> _selectedFeatures;

        [MenuItem("Tools/Setup Wizard")]
        public static void ShowWizard()
        {
            var window = GetWindow<InteractiveSetupWizard>("Setup Wizard");
            window.minSize = new Vector2(600, 400);
        }

        void OnGUI()
        {
            DrawHeader();

            switch (_currentPhase)
            {
                case SetupPhase.Welcome:
                    DrawWelcomePhase();
                    break;
                case SetupPhase.GenreSelection:
                    DrawGenreSelectionPhase();
                    break;
                case SetupPhase.FeatureConfiguration:
                    DrawFeatureConfigurationPhase();
                    break;
                case SetupPhase.Validation:
                    DrawValidationPhase();
                    break;
                case SetupPhase.Complete:
                    DrawCompletePhase();
                    break;
            }

            DrawNavigationButtons();
        }

        void DrawGenreSelectionPhase()
        {
            EditorGUILayout.LabelField("Select Game Genre", EditorStyles.boldLabel);
            _selectedGenre = (GenreType)EditorGUILayout.EnumPopup("Genre:", _selectedGenre);
            EditorGUILayout.HelpBox(GetGenreDescription(_selectedGenre), MessageType.Info);
        }

        async void ApplyConfiguration()
        {
            EditorUtility.DisplayProgressBar("Setup", "Applying configuration...", 0);

            await ImportRequiredPackages();
            EditorUtility.DisplayProgressBar("Setup", "Importing packages...", 0.3f);

            ApplyGenreTemplate(_selectedGenre);
            EditorUtility.DisplayProgressBar("Setup", "Applying template...", 0.6f);

            ConfigureFeatures(_selectedFeatures);
            EditorUtility.DisplayProgressBar("Setup", "Configuring features...", 0.9f);

            ValidateSetup();
            EditorUtility.ClearProgressBar();
            _currentPhase = SetupPhase.Complete;
        }

        async UniTask ImportRequiredPackages()
        {
            // Cinemachine, Input System等のパッケージ自動インポート
            await UniTask.Delay(1000);
        }
    }
}
```

## 8. 学習システム設計

```csharp
namespace asterivo.Unity60.Features.Tutorial
{
    public class LearningProgressTracker : MonoBehaviour
    {
        [System.Serializable]
        public class LearningStage
        {
            public string stageName;
            public string description;
            public float estimatedHours;
            public bool isCompleted;
        }

        [SerializeField] private List<LearningStage> learningStages = new()
        {
            new LearningStage { stageName = "Stage 1: 基礎概念", estimatedHours = 2f },
            new LearningStage { stageName = "Stage 2: Core層活用", estimatedHours = 3f },
            new LearningStage { stageName = "Stage 3: Feature層実装", estimatedHours = 3f },
            new LearningStage { stageName = "Stage 4: Template層カスタマイズ", estimatedHours = 2f },
            new LearningStage { stageName = "Stage 5: プロダクション準備", estimatedHours = 2f }
        };

        public float GetTotalProgress()
        {
            int completedStages = learningStages.Count(s => s.isCompleted);
            return (float)completedStages / learningStages.Count * 100f;
        }

        public float GetEstimatedRemainingTime()
        {
            return learningStages.Where(s => !s.isCompleted).Sum(s => s.estimatedHours);
        }

        public void CompleteStage(int stageIndex)
        {
            if (stageIndex >= 0 && stageIndex < learningStages.Count)
            {
                learningStages[stageIndex].isCompleted = true;
                SaveProgress();

                ServiceLocator.Get<IEventManager>()?.RaiseEvent("LearningStageCompleted", stageIndex);
            }
        }

        private void SaveProgress()
        {
            PlayerPrefs.SetFloat("LearningProgress", GetTotalProgress());
            PlayerPrefs.Save();
        }
    }
}
```

## 9. テスト戦略

### 9.1 ユニットテスト

```csharp
namespace asterivo.Unity60.Tests.Unit
{
    [TestFixture]
    public class ServiceLocatorTests
    {
        [SetUp]
        public void Setup() => ServiceLocator.Clear();

        [Test]
        public void Register_Service_Should_Succeed()
        {
            var mockService = new MockAudioManager();
            ServiceLocator.Register<IAudioManager>(mockService);

            Assert.IsNotNull(ServiceLocator.Get<IAudioManager>());
            Assert.IsTrue(mockService.IsInitialized);
        }

        [Test]
        public void Get_Unregistered_Service_Should_Throw()
        {
            Assert.Throws<InvalidOperationException>(() =>
                ServiceLocator.Get<IGameManager>());
        }
    }

    [TestFixture]
    public class CommandPoolTests
    {
        [Test]
        public void Pool_Should_Reuse_Commands()
        {
            var command1 = CommandPoolManager.GetCommand<TestCommand>();
            CommandPoolManager.ReturnCommand(command1);
            var command2 = CommandPoolManager.GetCommand<TestCommand>();

            Assert.AreSame(command1, command2);
        }
    }
}
```

### 9.2 統合テスト

```csharp
namespace asterivo.Unity60.Tests.Integration
{
    public class AISystemIntegrationTests : MonoBehaviour
    {
        [UnityTest]
        public IEnumerator Visual_Sensor_Should_Detect_Player()
        {
            var aiObject = GameObject.Instantiate(Resources.Load<GameObject>("AI/TestNPC"));
            var playerObject = GameObject.Instantiate(Resources.Load<GameObject>("Player/TestPlayer"));

            playerObject.transform.position = aiObject.transform.position + Vector3.forward * 5f;

            yield return new WaitForSeconds(1f);

            var visualSensor = aiObject.GetComponent<VisualSensor>();
            Assert.IsTrue(visualSensor.VisibleTargets.Contains(playerObject.transform));
            Assert.Greater(visualSensor.GetDetectionLevel(), 0.5f);

            GameObject.Destroy(aiObject);
            GameObject.Destroy(playerObject);
        }
    }
}
```

## 10. プロジェクト成功指標

### 10.1 定量的指標達成計画

| 指標 | 現在値 | 目標値 | 実装方法 |
|------|--------|--------|----------|
| セットアップ時間 | 30分 | 1分 | Setup Wizard自動化 |
| 学習コスト | 40時間 | 12時間 | 5段階学習システム |
| メモリ効率 | 基準値 | 95%削減 | ObjectPool統合 |
| 実行速度 | 基準値 | 67%向上 | Command最適化 |
| 開発速度 | 基準値 | 150%向上 | Template提供 |

### 10.2 段階的実装ロードマップ

#### Phase 1（1-2週目）: Core基盤実装
- ServiceLocator実装
- Event System構築
- Command + ObjectPool統合

#### Phase 2（3-4週目）: Feature層実装
- Player/AI/Camera機能
- センサーシステム
- インタラクション

#### Phase 3（5-6週目）: Template層実装
- 7ジャンルテンプレート
- Setup Wizard
- 学習システム

#### Phase 4（7-8週目）: 最適化・品質保証
- パフォーマンス最適化
- テスト実装
- ドキュメント整備

## まとめ

本設計書は、Unity 6ベースの究極の3Dゲーム開発基盤テンプレートの技術実装方針を定義しました。3層アーキテクチャを中核とし、ServiceLocator + Event駆動のハイブリッドアーキテクチャにより、以下を実現します：

- **97%のセットアップ時間短縮**（30分→1分）
- **70%の学習コスト削減**（40時間→12時間）
- **95%のメモリ使用量削減**（ObjectPool統合）
- **67%の実行速度改善**（Command最適化）
- **150%の開発速度向上**（テンプレート活用）

これらの目標達成により、「Clone & Create」「Learn & Grow」「Ship & Scale」の核心価値を実現し、Unity開発者にとって究極のテンプレートを提供します。