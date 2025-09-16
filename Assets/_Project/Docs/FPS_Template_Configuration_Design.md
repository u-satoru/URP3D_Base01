# FPS Template Configuration 詳細設計書

## アーキテクチャとデザインパターン準拠設計

### 概要

本設計書は、TODO.md Phase 3のTASK-004.2における**FPS Template Configuration**の詳細設計を定義します。ServiceLocator + Event駆動のハイブリッドアーキテクチャを核心とし、以下の5つのデザインパターンを完全に統合した実装を提供します。

#### 核心アーキテクチャパターン
- **ServiceLocator + Event駆動のハイブリッドアーキテクチャ（最重要）**: グローバルサービスへのアクセスとイベントベースの通信を組み合わせたハイブリッドアプローチ
- **イベント駆動型アーキテクチャ**: `GameEvent` を介したコンポーネント間の疎結合な連携
- **コマンドパターン**: ゲーム内のアクション（射撃、リロード、武器切替等）をオブジェクトとしてカプセル化
- **ObjectPool最適化**: 弾丸、エフェクト、UIエレメント等を効率的に再利用（95%のメモリ削減効果）
- **Scriptable Objectベースのデータ管理**: 武器データ、敵データ、レベル設定等をアセットとして管理

### 名前空間・配置規約

**名前空間**: `asterivo.Unity60.Features.Templates.FPS`
**配置**: `Assets/_Project/Features/Templates/FPS/`
**アセンブリ定義**: `Features.asmdef`配下（Core層依存許可、Feature間疎結合）

---

## 注意事項 : **コンパイルは Unity Editor 内でのみ実行可能**

---
## 1. ServiceLocator統合アーキテクチャ設計

### 1.1 FPS専用サービス階層

```csharp
namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    // FPS Template専用ServiceLocator拡張
    public static class FPSServiceLocator
    {
        // Core ServiceLocatorを基盤として活用
        public static T GetService<T>() where T : class
            => asterivo.Unity60.Core.Services.ServiceLocator.GetService<T>();

        // FPS専用サービス登録
        public static void RegisterFPSServices()
        {
            RegisterService<IWeaponManager>(new WeaponManager());
            RegisterService<ICombatManager>(new CombatManager());
            RegisterService<IFPSCameraService>(new FPSCameraService());
            RegisterService<IFPSInputService>(new FPSInputService());
            RegisterService<IFPSUIService>(new FPSUIService());
            RegisterService<IAmmoManager>(new AmmoManager());
            RegisterService<ITargetingService>(new TargetingService());
        }

        private static void RegisterService<T>(T service) where T : class
        {
            asterivo.Unity60.Core.Services.ServiceLocator.RegisterService<T>(service);
        }
    }
}
```

### 1.2 FPS核心サービスインターフェース定義

```csharp
namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    // 武器管理サービス（ServiceLocator経由アクセス）
    public interface IWeaponManager
    {
        IWeaponData CurrentWeapon { get; }
        bool CanShoot { get; }
        bool CanReload { get; }
        void SwitchWeapon(int weaponIndex);
        void StartReload();
        void Fire(Vector3 origin, Vector3 direction);
        event System.Action<IWeaponData> OnWeaponSwitched;
        event System.Action<int> OnAmmoChanged;
    }

    // 戦闘管理サービス（ServiceLocator経由アクセス）
    public interface ICombatManager
    {
        bool IsInCombat { get; }
        float Health { get; }
        float MaxHealth { get; }
        void TakeDamage(float damage, Vector3 source);
        void Heal(float amount);
        void SetCombatState(bool inCombat);
        event System.Action<float> OnHealthChanged;
        event System.Action<bool> OnCombatStateChanged;
    }

    // FPSカメラサービス（ServiceLocator経由アクセス）
    public interface IFPSCameraService
    {
        bool IsFirstPerson { get; }
        float MouseSensitivity { get; set; }
        float FieldOfView { get; set; }
        void SetFirstPersonMode(bool firstPerson);
        void ApplyCameraShake(float intensity, float duration);
        void SetAimingMode(bool aiming);
        event System.Action<bool> OnViewModeChanged;
    }
}
```

---

## 2. Event駆動アーキテクチャ統合設計

### 2.1 FPS専用GameEventチャネル

```csharp
namespace asterivo.Unity60.Features.Templates.FPS.Events
{
    // 武器関連イベント
    [CreateAssetMenu(menuName = "Events/FPS/Weapon Event")]
    public class WeaponEvent : GameEvent<WeaponEventData> { }

    [System.Serializable]
    public struct WeaponEventData
    {
        public WeaponEventType EventType;
        public int WeaponIndex;
        public int AmmoCount;
        public Vector3 FirePosition;
        public Vector3 FireDirection;
    }

    public enum WeaponEventType
    {
        WeaponFired,
        WeaponReloaded,
        WeaponSwitched,
        AmmoPickedUp,
        WeaponPickedUp
    }

    // 戦闘関連イベント
    [CreateAssetMenu(menuName = "Events/FPS/Combat Event")]
    public class CombatEvent : GameEvent<CombatEventData> { }

    [System.Serializable]
    public struct CombatEventData
    {
        public CombatEventType EventType;
        public float DamageAmount;
        public Vector3 SourcePosition;
        public GameObject Source;
        public GameObject Target;
    }

    public enum CombatEventType
    {
        PlayerHit,
        EnemyHit,
        PlayerKilled,
        EnemyKilled,
        CombatStarted,
        CombatEnded
    }
}
```

### 2.2 ServiceLocator + Event駆動統合パターン

```csharp
namespace asterivo.Unity60.Features.Templates.FPS.Core
{
    // ServiceLocatorとEvent駆動を統合したFPS管理クラス
    public class FPSTemplateManager : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private WeaponEvent _weaponEvent;
        [SerializeField] private CombatEvent _combatEvent;
        [SerializeField] private GameEvent _fpsSetupCompleted;

        // ServiceLocator経由でサービス取得
        private IWeaponManager _weaponManager;
        private ICombatManager _combatManager;
        private IFPSCameraService _cameraService;

        void Start()
        {
            // ServiceLocator経由でサービス取得
            _weaponManager = FPSServiceLocator.GetService<IWeaponManager>();
            _combatManager = FPSServiceLocator.GetService<ICombatManager>();
            _cameraService = FPSServiceLocator.GetService<IFPSCameraService>();

            // サービスのイベントをGameEventに変換
            _weaponManager.OnWeaponSwitched += OnWeaponSwitched;
            _weaponManager.OnAmmoChanged += OnAmmoChanged;
            _combatManager.OnHealthChanged += OnHealthChanged;
            _combatManager.OnCombatStateChanged += OnCombatStateChanged;

            // 初期化完了をEvent通知
            _fpsSetupCompleted?.Raise();
        }

        private void OnWeaponSwitched(IWeaponData weapon)
        {
            _weaponEvent?.Raise(new WeaponEventData
            {
                EventType = WeaponEventType.WeaponSwitched,
                WeaponIndex = weapon.WeaponIndex
            });
        }

        // ServiceLocatorとEvent駆動の橋渡し役
        public void HandleWeaponFire(Vector3 origin, Vector3 direction)
        {
            // ServiceLocator経由でサービス実行
            _weaponManager.Fire(origin, direction);

            // 結果をEventで通知
            _weaponEvent?.Raise(new WeaponEventData
            {
                EventType = WeaponEventType.WeaponFired,
                FirePosition = origin,
                FireDirection = direction
            });
        }
    }
}
```

---

## 3. コマンドパターン統合設計

### 3.1 FPS専用コマンドシステム

```csharp
namespace asterivo.Unity60.Features.Templates.FPS.Commands
{
    // 射撃コマンド（ObjectPool対応）
    public class FireWeaponCommand : ICommand, IResettableCommand
    {
        private Vector3 _firePosition;
        private Vector3 _fireDirection;
        private IWeaponData _weaponData;

        // ServiceLocator経由でサービス取得
        private IWeaponManager _weaponManager;
        private IAudioService _audioService; // Core層サービス

        public void Initialize(Vector3 position, Vector3 direction, IWeaponData weapon)
        {
            _firePosition = position;
            _fireDirection = direction;
            _weaponData = weapon;

            // ServiceLocator経由でサービス取得
            _weaponManager ??= FPSServiceLocator.GetService<IWeaponManager>();
            _audioService ??= ServiceLocator.GetService<IAudioService>();
        }

        public void Execute()
        {
            // ServiceLocator経由で射撃実行
            _weaponManager.Fire(_firePosition, _fireDirection);

            // AudioServiceも活用
            _audioService.PlaySFX(_weaponData.FireSound, _firePosition);

            // Projectile生成（ObjectPool活用）
            var projectile = ProjectilePool.Instance.Get();
            projectile.Initialize(_firePosition, _fireDirection, _weaponData);
        }

        public bool CanUndo => false; // 射撃は取り消し不可
        public void Undo() { }

        public void Reset()
        {
            _firePosition = Vector3.zero;
            _fireDirection = Vector3.zero;
            _weaponData = null;
        }
    }

    // リロードコマンド（Undo対応）
    public class ReloadWeaponCommand : ICommand, IResettableCommand
    {
        private int _previousAmmo;
        private IWeaponData _weaponData;
        private IWeaponManager _weaponManager;

        public void Initialize(IWeaponData weapon)
        {
            _weaponData = weapon;
            _previousAmmo = weapon.CurrentAmmo;
            _weaponManager ??= FPSServiceLocator.GetService<IWeaponManager>();
        }

        public void Execute()
        {
            _weaponManager.StartReload();
        }

        public bool CanUndo => true;
        public void Undo()
        {
            _weaponData.SetAmmo(_previousAmmo);
        }

        public void Reset()
        {
            _previousAmmo = 0;
            _weaponData = null;
        }
    }
}
```

### 3.2 コマンド実行統合システム

```csharp
namespace asterivo.Unity60.Features.Templates.FPS.Core
{
    public class FPSCommandProcessor : MonoBehaviour
    {
        // Core層CommandPoolManagerを活用
        private CommandPoolManager _commandPool;
        private IInputService _inputService; // Core層サービス

        void Start()
        {
            _commandPool = ServiceLocator.GetService<CommandPoolManager>();
            _inputService = ServiceLocator.GetService<IInputService>();

            // 入力バインディング（ServiceLocator + Event駆動）
            _inputService.OnFirePressed += HandleFireInput;
            _inputService.OnReloadPressed += HandleReloadInput;
        }

        private void HandleFireInput()
        {
            // ObjectPool経由でコマンド取得・実行
            var fireCommand = _commandPool.Get<FireWeaponCommand>();
            var weaponManager = FPSServiceLocator.GetService<IWeaponManager>();

            fireCommand.Initialize(
                transform.position,
                transform.forward,
                weaponManager.CurrentWeapon
            );

            fireCommand.Execute();
            _commandPool.Return(fireCommand);
        }
    }
}
```

---

## 4. ObjectPool最適化設計

### 4.1 FPS専用ObjectPoolシステム

```csharp
namespace asterivo.Unity60.Features.Templates.FPS.ObjectPools
{
    // 弾丸ObjectPool（95%メモリ削減効果活用）
    public class ProjectilePool : MonoBehaviour
    {
        public static ProjectilePool Instance { get; private set; }

        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private int _poolSize = 100;

        private Queue<Projectile> _projectilePool = new Queue<Projectile>();
        private int _activeCount = 0;

        void Awake()
        {
            Instance = this;
            InitializePool();
        }

        private void InitializePool()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                var projectile = Instantiate(_projectilePrefab).GetComponent<Projectile>();
                projectile.gameObject.SetActive(false);
                projectile.OnProjectileDestroyed += ReturnToPool;
                _projectilePool.Enqueue(projectile);
            }
        }

        public Projectile Get()
        {
            if (_projectilePool.Count > 0)
            {
                var projectile = _projectilePool.Dequeue();
                projectile.gameObject.SetActive(true);
                _activeCount++;
                return projectile;
            }

            // プール不足時の動的生成
            return CreateNewProjectile();
        }

        public void ReturnToPool(Projectile projectile)
        {
            projectile.Reset();
            projectile.gameObject.SetActive(false);
            _projectilePool.Enqueue(projectile);
            _activeCount--;
        }
    }

    // UI要素ObjectPool
    public class FPSUIElementPool : MonoBehaviour
    {
        // ダメージ表示、弾数表示等のUI要素をプール化
        private Dictionary<System.Type, Queue<Component>> _uiPools = new();

        public T GetUIElement<T>() where T : Component
        {
            var type = typeof(T);
            if (_uiPools.ContainsKey(type) && _uiPools[type].Count > 0)
            {
                return (T)_uiPools[type].Dequeue();
            }

            // ServiceLocator経由でUIサービス取得
            var uiService = FPSServiceLocator.GetService<IFPSUIService>();
            return uiService.CreateUIElement<T>();
        }
    }
}
```

---

## 5. ScriptableObject データ管理設計

### 5.1 武器データアセット

```csharp
namespace asterivo.Unity60.Features.Templates.FPS.Data
{
    [CreateAssetMenu(menuName = "FPS Template/Weapon Data")]
    public class WeaponData : ScriptableObject, IWeaponData
    {
        [Header("基本設定")]
        public string WeaponName;
        public WeaponType Type;
        public int WeaponIndex;

        [Header("弾薬設定")]
        public int MaxAmmo = 30;
        public int ReserveAmmo = 90;
        public float ReloadTime = 2.0f;

        [Header("射撃設定")]
        public float Damage = 25f;
        public float FireRate = 600f; // RPM
        public float Range = 100f;
        public float Accuracy = 0.95f;

        [Header("Audio")]
        public AudioClip FireSound;
        public AudioClip ReloadSound;
        public AudioClip EmptySound;

        [Header("視覚効果")]
        public GameObject MuzzleFlashPrefab;
        public GameObject ImpactEffectPrefab;

        // ServiceLocator経由でサービス統合
        public void FireWeapon(Vector3 origin, Vector3 direction)
        {
            var audioService = ServiceLocator.GetService<IAudioService>();
            audioService.PlaySFX(FireSound, origin);

            var effectsService = FPSServiceLocator.GetService<IEffectsService>();
            effectsService.PlayMuzzleFlash(MuzzleFlashPrefab, origin);
        }
    }

    [CreateAssetMenu(menuName = "FPS Template/FPS Configuration")]
    public class FPSConfiguration : ScriptableObject
    {
        [Header("プレイヤー設定")]
        public float MovementSpeed = 5f;
        public float SprintSpeedMultiplier = 1.5f;
        public float JumpHeight = 1.2f;

        [Header("カメラ設定")]
        public float MouseSensitivity = 2f;
        public float FieldOfView = 75f;
        public float AimFOV = 45f;

        [Header("武器設定")]
        public WeaponData[] AvailableWeapons;
        public int StartingWeaponIndex = 0;

        [Header("UI設定")]
        public bool ShowCrosshair = true;
        public bool ShowAmmoCounter = true;
        public bool ShowHealthBar = true;

        // ServiceLocator統合で設定適用
        public void ApplyConfiguration()
        {
            var cameraService = FPSServiceLocator.GetService<IFPSCameraService>();
            cameraService.MouseSensitivity = MouseSensitivity;
            cameraService.FieldOfView = FieldOfView;

            var weaponManager = FPSServiceLocator.GetService<IWeaponManager>();
            weaponManager.InitializeWeapons(AvailableWeapons);
            weaponManager.SwitchWeapon(StartingWeaponIndex);
        }
    }
}
```

---

## 6. 統合アーキテクチャ実装例

### 6.1 FPSプレイヤーコントローラー

```csharp
namespace asterivo.Unity60.Features.Templates.FPS.Player
{
    public class FPSPlayerController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private FPSConfiguration _config;

        // ServiceLocator経由で取得するサービス
        private IInputService _inputService;
        private IWeaponManager _weaponManager;
        private ICombatManager _combatManager;
        private IFPSCameraService _cameraService;
        private CommandPoolManager _commandPool;

        // Event駆動通信
        [Header("Events")]
        [SerializeField] private WeaponEvent _weaponEvent;
        [SerializeField] private CombatEvent _combatEvent;

        void Start()
        {
            InitializeServices();
            ApplyConfiguration();
            SetupEventBindings();
        }

        private void InitializeServices()
        {
            // Core層サービス
            _inputService = ServiceLocator.GetService<IInputService>();
            _commandPool = ServiceLocator.GetService<CommandPoolManager>();

            // FPS層サービス
            _weaponManager = FPSServiceLocator.GetService<IWeaponManager>();
            _combatManager = FPSServiceLocator.GetService<ICombatManager>();
            _cameraService = FPSServiceLocator.GetService<IFPSCameraService>();
        }

        private void ApplyConfiguration()
        {
            _config?.ApplyConfiguration();
        }

        private void SetupEventBindings()
        {
            // 入力イベント → コマンド実行
            _inputService.OnFirePressed += HandleFireInput;
            _inputService.OnReloadPressed += HandleReloadInput;
            _inputService.OnWeaponSwitchPressed += HandleWeaponSwitchInput;

            // サービスイベント → GameEvent変換
            _weaponManager.OnWeaponSwitched += (weapon) =>
            {
                _weaponEvent?.Raise(new WeaponEventData
                {
                    EventType = WeaponEventType.WeaponSwitched,
                    WeaponIndex = weapon.WeaponIndex
                });
            };

            _combatManager.OnHealthChanged += (health) =>
            {
                _combatEvent?.Raise(new CombatEventData
                {
                    EventType = CombatEventType.PlayerHit,
                    DamageAmount = _combatManager.MaxHealth - health
                });
            };
        }

        private void HandleFireInput()
        {
            if (!_weaponManager.CanShoot) return;

            // ObjectPool経由でコマンド取得・実行
            var fireCommand = _commandPool.Get<FireWeaponCommand>();
            fireCommand.Initialize(
                _cameraService.CameraTransform.position,
                _cameraService.CameraTransform.forward,
                _weaponManager.CurrentWeapon
            );

            fireCommand.Execute();
            _commandPool.Return(fireCommand);
        }

        private void HandleReloadInput()
        {
            if (!_weaponManager.CanReload) return;

            var reloadCommand = _commandPool.Get<ReloadWeaponCommand>();
            reloadCommand.Initialize(_weaponManager.CurrentWeapon);
            reloadCommand.Execute();
            _commandPool.Return(reloadCommand);
        }
    }
}
```

---

## 7. パフォーマンス最適化考慮事項

### 7.1 ServiceLocator最適化

```csharp
namespace asterivo.Unity60.Features.Templates.FPS.Performance
{
    // FPS専用高速サービス取得キャッシュ
    public static class FPSServiceCache
    {
        private static IWeaponManager _weaponManager;
        private static ICombatManager _combatManager;
        private static IFPSCameraService _cameraService;

        // 初回アクセス時のみServiceLocator使用、以降はキャッシュ
        public static IWeaponManager WeaponManager =>
            _weaponManager ??= FPSServiceLocator.GetService<IWeaponManager>();

        public static ICombatManager CombatManager =>
            _combatManager ??= FPSServiceLocator.GetService<ICombatManager>();

        public static IFPSCameraService CameraService =>
            _cameraService ??= FPSServiceLocator.GetService<IFPSCameraService>();

        public static void ClearCache()
        {
            _weaponManager = null;
            _combatManager = null;
            _cameraService = null;
        }
    }
}
```

### 7.2 Event駆動最適化

- **Event Batching**: 1フレーム内の複数射撃イベントを統合
- **Priority Queue**: 重要度に応じたイベント処理順序制御
- **Event Pooling**: EventDataのObjectPool活用

---

## 8. 実装ガイドライン

### 8.1 初期化順序

1. **Core層サービス初期化** (ServiceLocator基盤)
2. **FPS専用サービス登録** (FPSService.RegisterFPSServices())
3. **ScriptableObject設定読み込み** (FPSConfiguration)
4. **Event Channel設定** (GameEvent購読設定)
5. **ObjectPool初期化** (ProjectilePool等)
6. **入力バインディング** (InputService → Command)

### 8.2 デバッグ・検証要件

```csharp
namespace asterivo.Unity60.Features.Templates.FPS.Debug
{
    public class FPSTemplateDebugger : MonoBehaviour
    {
        private void OnGUI()
        {
            GUILayout.Label("=== FPS Template Debug ===");

            // ServiceLocator状態表示
            var weaponManager = FPSServiceCache.WeaponManager;
            GUILayout.Label($"Current Weapon: {weaponManager?.CurrentWeapon?.WeaponName}");
            GUILayout.Label($"Ammo: {weaponManager?.CurrentWeapon?.CurrentAmmo}");

            // ObjectPool状態表示
            GUILayout.Label($"Projectile Pool Active: {ProjectilePool.Instance.ActiveCount}");

            // Event統計表示
            GUILayout.Label($"Weapon Events Fired: {WeaponEventStatistics.TotalEvents}");
        }
    }
}
```

### 8.3 成功指標

- **ServiceLocator活用率**: 全サービスアクセスの95%以上がServiceLocator経由
- **Event駆動疎結合度**: 直接参照を95%削減
- **ObjectPool効率**: 95%メモリ削減効果の維持
- **パフォーマンス**: 60FPS安定動作、射撃レスポンス16ms以内
- **コマンドパターン**: 全アクションの80%がCommand経由実行

---

## 9. まとめ

本FPS Template Configurationは、**ServiceLocator + Event駆動のハイブリッドアーキテクチャ**を核心として、以下の価値を実現します：

### 9.1 アーキテクチャ価値
- **疎結合設計**: Event駆動通信により、コンポーネント間の依存関係を最小化
- **サービス指向**: ServiceLocatorによる一元的なサービス管理
- **コマンド管理**: 全アクションのカプセル化とUndo対応
- **リソース最適化**: ObjectPoolによる95%メモリ削減効果

### 9.2 FPS価値
- **レスポンシブ射撃**: 16ms以内の射撃レスポンス
- **スムーズカメラ**: 一人称視点の滑らかな操作感
- **戦闘システム**: ダメージ・体力・武器の統合管理
- **カスタマイズ性**: ScriptableObjectによる柔軟な設定変更

### 9.3 開発価値
- **学習効率**: アーキテクチャパターンの実践的学習
- **拡張性**: 新要素追加時の影響範囲最小化
- **保守性**: Event駆動とServiceLocatorによる保守容易性
- **品質保証**: 統一されたアーキテクチャによる一貫した品質

この設計により、**TODO.md Phase 3のLearn & Grow価値実現**（学習コスト70%削減、15分ゲームプレイ実現）に確実に貢献します。