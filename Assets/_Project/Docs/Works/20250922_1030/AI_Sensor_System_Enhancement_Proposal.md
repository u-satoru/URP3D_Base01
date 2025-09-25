# AIセンサーシステムの拡張設計案：嗅覚センサーの導入と動的統合

## 1. 概要

本ドキュメントは、既存のAIセンサーシステム（視覚・聴覚）を拡張し、新たに**嗅覚センサー**を導入するとともに、これら全てのセンサーを動的に有効化・無効化し、**SensorFusionSystem**を介して汎用的に利用可能にするためのシステム設計を提案するものである。

この拡張により、ゲームプレイの深化、没入感の向上、パフォーマンスの最適化、そして多様なキャラクターの容易な作成を実現することを目的とする。

## 2. 提案の核心：動的センサースイッチングシステム

PlayerやNPCが持つセンサー（視覚・聴覚・嗅覚）を、ゲームの状況に応じてリアルタイムで有効化・無効化できるようにする。`SensorFusionSystem`は、そのキャラクターにおいて現在アクティブになっているセンサーからの情報のみを自動的に統合し、最終的な警戒レベルを算出する。

### 2.1. このアプローチのメリット

*   **ゲームプレイの飛躍的な向上**:
    *   **状態異常**: 「盲目」状態のキャラクターは視覚センサーをOFFにする。
    *   **環境効果**: 濃霧の中では視覚センサーが自動で無効化される。
    *   **キャラクター成長**: スキル習得により新しいセンサーをアンロックする。
    *   **多様なNPC**: センサー構成を変えるだけで「盲目だが聴覚が鋭いモンスター」などを簡単に作成できる。
*   **パフォーマンス最適化**: 不要なセンサーの計算処理を削減できる。
*   **設計の美しさ**: `SensorFusionSystem`は「アクティブなセンサーを統合する」という単一責務に特化でき、汎用性と再利用性が高まる。

## 3. 実装設計：3層アーキテクチャへの統合

プロジェクトの3層アーキテクチャ (`Core` ← `Feature` ← `Template`) に沿って、各機能を段階的に実装する。

### 3.1. `Core`層：システムの共通規格を定義

ジャンルを問わない普遍的な「仕組み」として、センサーの基本仕様と匂いの概念を定義する。

#### **センサー共通インターフェース (`ISensor.cs`)**
全てのセンサーが実装すべき共通規格。

```csharp
public enum SensorType { Visual, Auditory, Olfactory }

public interface ISensor
{
    SensorType Type { get; }
    bool IsActive { get; }
    void Enable();
    void Disable();
    void Tick(); // センサーの更新処理 (Updateの代わり)
    float GetDetectionLevel(); // 0.0 ~ 1.0 の検知レベルを返す
}
```

#### **センサー管理司令塔 (`SensorController.cs`)**
PlayerやNPCにアタッチし、そのキャラクターが持つセンサー群を統括する。

```csharp
public class SensorController : MonoBehaviour
{
    private List<ISensor> _allSensors;

    void Awake()
    {
        // 自分にアタッチされているISensorを全て取得
        _allSensors = GetComponents<ISensor>().ToList();
    }

    public void EnableSensor(SensorType type) { /* ... */ }
    public void DisableSensor(SensorType type) { /* ... */ }

    public IEnumerable<ISensor> GetActiveSensors()
    {
        return _allSensors.Where(s => s.IsActive);
    }
}
```

#### **匂いの基本定義 (`OdorProfile.cs`, `IOdorable.cs`, `WindSystem.cs`)**
*   **`OdorProfile.cs` (ScriptableObject)**: 匂いの種類、強度、持続時間などを定義するデータアセット。
*   **`IOdorable.cs` (Interface)**: 匂いを発生させるオブジェクトが実装するインターフェース。
*   **`WindSystem.cs` (Service)**: 風向きと風速を管理するグローバルサービス。`ServiceLocator`に登録する。

### 3.2. `Feature`層：具体的な機能部品の実装

`Core`層の仕組みを利用して、具体的なセンサーや統合システムを実装する。

#### **各センサーの実装**
*   **`NPCVisualSensor.cs`, `NPCAuditorySensor.cs`, `NPCOlfactorySensor.cs`**:
    *   それぞれが `ISensor` インターフェースを実装する。
    *   `Enable()` / `Disable()` メソッドで有効/無効を切り替えるロジックを実装。
    *   `NPCOlfactorySensor`は、`WindSystem`と`OdorSource`を基に匂いを検知するロジックを持つ。

#### **匂い発生源 (`OdorSource.cs`)**
*   `IOdorable`を実装したコンポーネント。GameObjectにアタッチし、`OdorProfile`をセットすることで匂いを発生させる。

#### **センサー統合システム (`SensorFusionSystem.cs`)**
`SensorController`と連携し、動的にセンサー情報を統合する。

*   **`SensorFusionProfile.cs` (ScriptableObject)**:
    *   各センサーの検知レベルに対する重み付け（重要度）を定義するデータアセット。
    ```csharp
    [CreateAssetMenu(menuName = "AI/Sensor Fusion Profile")]
    public class SensorFusionProfile : ScriptableObject
    {
        public float visualWeight = 0.5f;
        public float auditoryWeight = 0.3f;
        public float olfactoryWeight = 0.2f;
    }
    ```

*   **`SensorFusionSystem.cs` (Component)**:
    *   `SensorController`に問い合わせ、**現在アクティブなセンサー**のリストのみを取得して処理する。
    ```csharp
    public class SensorFusionSystem : MonoBehaviour
    {
        [SerializeField] private SensorFusionProfile profile;
        private SensorController _sensorController;

        void Awake()
        {
            _sensorController = GetComponent<SensorController>();
        }

        void Update()
        {
            float totalAlertLevel = 0f;
            
            // SensorControllerからアクティブなセンサーだけを取得
            foreach (var sensor in _sensorController.GetActiveSensors())
            {
                float level = sensor.GetDetectionLevel();
                totalAlertLevel += level * GetWeightForSensor(sensor.Type);
            }

            // 統合された警戒レベルをAIのステートマシン等に渡す
            UpdateAIState(totalAlertLevel);
        }

        private float GetWeightForSensor(SensorType type) { /* ... */ }
        private void UpdateAIState(float level) { /* ... */ }
    }
    ```

### 3.3. `Template`層：ゲームプレイへの適用例

この動的な仕組みを使い、ゲームデザイナーがジャンルに特化した体験を構築する。

*   **`Stealth`テンプレート**:
    *   アイテム「スモークボム」の効果範囲内のキャラクターに対し、`sensorController.DisableSensor(SensorType.Visual)`を一定時間呼び出す。
*   **`SurvivalHorror`テンプレート**:
    *   特定の敵NPCのプレハブには、デフォルトで`NPCVisualSensor`を無効化した`SensorController`を設定し、「盲目のモンスター」を簡単に作成する。
    *   プレイヤーが負傷状態になると、`OdorSource`コンポーネントが「血」の匂いを強く発するように動的に変更する。

## 4. 結論

嗅覚センサーの導入と、それに伴う動的なセンサー統合システムの構築は、このプロジェクトのアーキテクチャのポテンシャルを最大限に引き出す、論理的かつ強力な機能拡張である。これにより、より動的で、予測不能で、奥深い「創発的（Emergent）」なゲームプレイを生み出すための強力な基盤が完成する。この設計案に沿った実装を強く推奨する。

---

## 5. 設計補足と拡張案

### 5.1. コンポーネントベース設計による汎用性と工数削減
「NPCは複数存在するが、個体ごとに`MonoBehaviour`コンポーネントを実装するのか？」という疑問に対し、本設計はUnityのプレハブシステムを活用することで解決する。

*   **実装は一度きり**: `SensorController.cs`等の機能コンポーネントのコーディングは一度だけ行う。
*   **設定で汎用化**: NPCの種類ごとにプレハブを作成し、インスペクター上でセンサーのパラメータや種類を設定することで、コーディングなしに多様なAIを作成可能。
*   **個体差の表現**: シーンに配置したプレハブインスタンスのパラメータを個別に上書きすることで、ユニークなキャラクターを容易に作成できる。

このアプローチにより、工数を増加させることなく、**「一度実装すれば、設定だけで無限にバリエーションを生み出せる」**という効率的な開発スタイルを実現する。

### 5.2. さらなる拡張案：感度（Sensitivity）レベルの導入
現在の設計（有効/無効のON/OFF）に加え、各センサーに**「感度 (Sensitivity)」**というアナログな調整機能を追加することで、システムの柔軟性と表現力を飛躍的に高めることができる。

#### **メリット**
*   **リッチなゲームプレイ**: 「鷹の目」スキルで視覚感度を一時的に200%にしたり、「閃光弾」で10%に低下させたりといった、ダイナミックなバフ/デバフを実装可能。
*   **AIの行動深化**: AIが自身の警戒レベルに応じて、センサー感度を動的に変化させ、よりリアルな捜索行動を行える。
*   **動的な難易度調整**: ゲームの難易度設定に応じて、敵全体のセンサー感度に補正をかけることが容易になる。

#### **実装案**
1.  **`Core`層: `ISensor`インターフェースの拡張**
    *   感度を操作するためのプロパティを追加する。
    ```csharp
    public interface ISensor
    {
        // ... 既存のメンバー ...

        /// <summary>
        /// センサーの現在の感度。1.0が標準。
        /// </summary>
        float Sensitivity { get; set; }

        /// <summary>
        /// 感度をデフォルト値にリセットする。
        /// </summary>
        void ResetSensitivity();
    }
    ```

2.  **`Feature`層: 各センサーへの感度ロジックの実装**
    *   各センサーの検知ロジック（例：検知範囲、検知スコア）の計算に`Sensitivity`の値を乗算する。
    ```csharp
    // NPCVisualSensor.cs での実装イメージ
    public float EffectiveViewDistance => baseViewDistance * Sensitivity;
    ```

3.  **`Template`層: ゲームデザイナーによる感度の活用**
    *   スキル、アイテム、環境効果のロジックから、キャラクターが持つ各センサーの`Sensitivity`値をリアルタイムに変更する。
    *   キャラクターのステータスアセットに「視覚感度の基本値」などを追加し、種族ごとの基本性能を設定する。

---
## 6. プレイヤーへの応用：汎用パーセプションシステムとしての展望

本センサーシステムはAI専用の機能ではなく、Playerキャラクターにも全く同じように適用可能であり、むしろ適用することでゲームプレイを飛躍的に豊かにする。

### 6.1. Playerへの適用可能性の根拠

*   **汎用的なコンポーネント設計**: `SensorController`や各種センサーコンポーネントは、特定のキャラクターに依存しない独立した部品であるため、PlayerのGameObjectにもNPCと同様にアタッチできる。
*   **対称的なゲームデザイン**: NPCがプレイヤーを検知するのと同じルールセットをプレイヤーも利用することで、プレイヤーがNPCや環境を「検知」する、公平で直感的なゲームメカニクスを構築できる。

### 6.2. 具体的な活用例

Playerに本システムを搭載することで、以下のような多様なゲームメカニクスが実現可能になる。

#### **視覚センサー (`VisualSensor`) の活用**
*   **インタラクション検知**: 視界内のインタラクション可能なオブジェクト（アイテム、ドア等）を検知し、UIガイドを表示する。
*   **敵のハイライト**: 視界内の敵を検知し、ミニマップに表示したり、壁越しにシルエットを表示する「探偵モード」のようなスキルを実装する。
*   **情報収集**: 特定のオブジェクトに視線を合わせることで、情報をスキャンしデータベースに記録する。

#### **聴覚センサー (`AuditorySensor`) の活用**
*   **サウンドの可視化**: 敵の足音や銃声などを検知し、その方向や大きさをUI（サウンドレーダー）として画面に表示する。これはアクセシビリティ向上にも繋がる。
*   **ステルス補助**: 敵の足音を頼りに、壁越しにその存在や巡回ルートを把握する。
*   **危険察知**: 視界外からの敵の接近を音で検知し、プレイヤーに警告する。

#### **嗅覚センサー (`OlfactorySensor`) の活用**
*   **追跡能力**: 特定のNPCやモンスターが残した匂いの痕跡を検知し、追跡する。
*   **探索補助**: 特定のアイテムや危険物（ガス漏れ等）の匂いを検知し、その方向を探る。
*   **特殊能力**: プレイヤーが人間以外のキャラクター（例：狼男）の場合、嗅覚を主要な索敵・探索手段として活用する。

### 6.3. プレイヤーにおけるSensorFusionSystemの役割

NPCが「警戒レベル」の算出に用いる`SensorFusionSystem`を、Playerの場合は**「情報統合UIマネージャー」**として活用する。各センサーから得た情報を統合し、HUD上のミニマップや各種インジケーターに分かりやすく表示する責務を担わせることができる。

### 6.4. 結論
この統合センサーシステムをPlayerにも適用することは、システムのポテンシャルを最大限に引き出し、NPCとの対話だけでなく、プレイヤーの探索や戦闘といったあらゆるゲームプレイの中核をなす、真に汎用的な基盤へと昇華させるための、自然かつ論理的なステップである。
