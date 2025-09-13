# ステルスゲーム用サウンドシステム

## 概要

このドキュメントでは、Unity 6のURP3D_Base01プロジェクトに実装されたステルスゲーム特化型サウンドシステムについて説明します。本システムは、音がゲームプレイの中核要素となるステルスゲームのために設計され、プロジェクトのイベント駆動アーキテクチャとコマンドパターンを活用した高度な音響処理を提供します。

## アーキテクチャ概要

### 設計原則

- **イベント駆動型**: `GameEvent`による疎結合な音響通知システム
- **コマンドパターン**: ObjectPool最適化済みの音声再生処理
- **データ駆動設計**: `ScriptableObject`による設定データ管理
- **モジュール化**: 各機能が独立したコンポーネントとして実装

### システム構成図

```
┌─────────────────────────────────────────────────────────────┐
│                  ステルスオーディオシステム                      │
├─────────────────────────────────────────────────────────────┤
│ ┌─────────────────┐  ┌─────────────────┐  ┌───────────────┐ │
│ │  プレイヤー音源   │  │   NPC聴覚センサ  │  │ 動的音響環境    │ │
│ │  PlayerAudio     │  │  NPCAuditory     │  │ DynamicAudio  │ │
│ │  System          │  │  Sensor          │  │ Environment   │ │
│ └─────────────────┘  └─────────────────┘  └───────────────┘ │
│           │                    │                    │        │
│           └────────────────────┼────────────────────┘        │
│                                │                             │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │              SpatialAudioManager                        │ │
│ │           (3D音響・距離減衰・オクルージョン)                │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                │                             │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │                コアサウンドシステム                        │ │
│ │ AudioEvent | AudioEventData | PlaySoundCommand          │ │
│ └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## コアシステム

### 1. AudioEvent (`AudioEvent.cs`)

```csharp
// 使用例
[SerializeField] private AudioEvent playerFootstepEvent;

// 位置指定での音の再生
playerFootstepEvent.RaiseAtPosition("footstep_concrete", playerPosition, 0.8f);

// プレイヤー音源としての再生
playerFootstepEvent.RaisePlayerSound("footstep_metal", 1.0f, AudioSourceType.Player);
```

**特徴:**
- `GenericGameEvent<AudioEventData>`を継承
- ステルスゲーム特化のデバッグ情報表示
- 簡易インターフェース提供

### 2. AudioEventData (`AudioEventData.cs`)

```csharp
public struct AudioEventData
{
    public string soundID;           // 音響ID
    public float volume;             // 音量
    public Vector3 worldPosition;    // 3D位置
    public AudioSourceType sourceType; // 音源種別
    public float hearingRadius;      // NPCが聞き取れる範囲
    public SurfaceMaterial surfaceType; // 表面材質
    public bool canBemasked;         // マスキング可能か
    public float priority;           // 音の優先度
}
```

**対応する表面材質:**
- `Default`, `Concrete`, `Carpet`, `Metal`, `Wood`, `Grass`, `Water`, `Gravel`

### 3. SoundDataSO (`SoundDataSO.cs`)

ScriptableObjectベースの音響設定アセット。

```csharp
[CreateAssetMenu(fileName = "New Sound Data", menuName = "asterivo.Unity60/Audio/Sound Data")]
public class SoundDataSO : ScriptableObject
{
    [SerializeField] private AudioClip[] audioClips;      // 音声クリップ配列
    [SerializeField] private float baseHearingRadius;     // 基本聴取範囲
    [SerializeField] private SurfaceAudioModifier[] surfaceModifiers; // 表面材質別調整
}
```

**設定項目:**
- 音量・ピッチのランダム変動設定
- 3D音響パラメータ（距離減衰、ロールオフモード）
- 表面材質別の音響調整

### 4. PlaySoundCommand (`PlaySoundCommand.cs`)

ObjectPool対応の音声再生コマンド。

```csharp
// コマンドプール使用例
var command = CommandPool.Instance.GetCommand<PlaySoundCommand>();
command.Initialize(audioData, soundData, audioSource, listener);
command.Execute();
```

**機能:**
- `IResettableCommand`実装によるプール再利用
- 表面材質による音響調整自動適用
- Undo機能対応

## 空間音響システム

### SpatialAudioManager (`SpatialAudioManager.cs`)

3D音響処理の中央管理システム。

**主要機能:**

#### 1. オーディオソースプール管理
```csharp
// 音源プールから取得・再生・返却の自動管理
AudioSource audioSource = SpatialAudioManager.Instance.PlaySoundAtPosition(soundData, position, volume);
```

#### 2. 距離減衰計算
```csharp
// カスタム減衰カーブによる自然な音量変化
float volume = CalculateVolumeAtDistance(distance, maxHearingRadius);
```

#### 3. オクルージョン（遮蔽）システム
- 障害物による音の遮蔽を物理演算で計算
- リアルタイム更新（デフォルト0.1秒間隔）
- 最大80%までの音量減衰

#### 4. 聴取可能性判定
```csharp
// 指定位置の音が聞こえるかの判定
bool isAudible = SpatialAudioManager.Instance.IsAudibleAtPosition(
    soundPosition, hearingRadius, listenerPosition);
```

## プレイヤー音源管理

### PlayerAudioSystem (`PlayerAudioSystem.cs`)

プレイヤーが発生させる音の包括的管理システム。

**管理する音源:**
- **足音**: 移動速度・表面材質に応じた自動再生
- **インタラクション音**: オブジェクト操作時の効果音
- **呼吸音**: 緊張状態での呼吸音

**依存コンポーネント:**
- `CharacterController` (必須)
- `PlayerController` (オプション - より精密な状態管理)
- `SurfaceTypeDetector` (オプション - 高精度な表面検出)

#### 足音システム

```csharp
[Header("Movement Speed Modifiers")]
[SerializeField] private MovementAudioSettings walkingSettings = new MovementAudioSettings(1.0f, 1.0f, 5.0f);
[SerializeField] private MovementAudioSettings runningSettings = new MovementAudioSettings(1.5f, 1.2f, 8.0f);
[SerializeField] private MovementAudioSettings crouchingSettings = new MovementAudioSettings(0.3f, 0.8f, 2.0f);
```

**動作:**
1. `CharacterController`の移動を監視
2. `PlayerController`から状態取得（利用可能時）、または移動速度から状態判定（歩行・走行・しゃがみ）
3. 表面材質の検出（`SurfaceTypeDetector`使用）
4. 適切な足音の自動選択・再生

#### 表面材質検出

### SurfaceTypeDetector (`SurfaceTypeDetector.cs`)

高精度な表面材質検出システム。

**検出方法の優先順位:**
1. **マテリアルベース**: Rendererのマテリアル直接参照
2. **タグベース**: GameObjectのタグによる判定
3. **名前ベース**: オブジェクト名からの推測

```csharp
[Header("Material Mapping")]
[SerializeField] private MaterialSurfaceMapping[] materialMappings;
[SerializeField] private TagSurfaceMapping[] tagMappings;
```

**パフォーマンス最適化:**
- 0.1秒間のキャッシュシステム
- SphereCast使用による確実な検出

## NPC聴覚システム

### NPCAuditorySensor (`NPCAuditorySensor.cs`)

NPCの音声認識・反応システム。

#### 聴覚能力設定

```csharp
[Header("Hearing Capabilities")]
[SerializeField] private float baseHearingRadius = 10f;        // 基本聴取範囲
[SerializeField] private float hearingAccuity = 1f;            // 聴力鋭敏さ (0.1～3.0)
[SerializeField] private AnimationCurve hearingCurve;          // 距離減衰カーブ
```

#### 警戒レベルシステム

```csharp
public enum AlertLevel
{
    Relaxed,        // リラックス状態
    Suspicious,     // 疑わしい音を検出
    Investigating,  // 調査状態  
    Alert           // 警戒状態
}
```

**閾値設定:**
- `detectionThreshold = 0.3f`: 音を認識する最小閾値
- `investigationThreshold = 0.6f`: 調査を始める閾値  
- `alertThreshold = 0.8f`: 警戒状態に入る閾値

#### マスキング効果

```csharp
// 環境音による音のマスキング
[SerializeField] private float ambientMaskingLevel = 0.2f;
[SerializeField] private bool affectedByMasking = true;
```

**マスキング計算:**
1. 環境音レベルによる基本マスキング
2. 競合する同時音による相互マスキング
3. `canBeMasked`フラグによる制御

#### 音の記憶システム

```csharp
[Header("State Management")]  
[SerializeField] private float memoryDuration = 5f;           // 記憶保持時間
[SerializeField] private int maxSimultaneousSounds = 5;       // 最大同時追跡音数
```

**機能:**
- 検出した音の5秒間記憶
- 最大5つまでの同時音追跡
- 記憶に基づく行動パターン変更

## 動的音響環境

### DynamicAudioEnvironment (`DynamicAudioEnvironment.cs`)

時間・天候・場所による音響環境の動的変化システム。

#### 環境要素

```csharp
public enum EnvironmentType { Indoor, Outdoor, Cave, Forest, Underwater }
public enum WeatherType { Clear, Rain, Storm, Fog }  
public enum TimeOfDay { Day, Evening, Night, Dawn }
```

#### 環境プリセット

```csharp
[System.Serializable]
public struct EnvironmentPreset
{
    public EnvironmentType environmentType;
    public float ambientVolume;        // アンビエント音量
    public float reverbLevel;          // リバーブレベル  
    public float lowPassFrequency;     // ローパスフィルタ
    public float maskingMultiplier;    // マスキング倍率
}
```

#### 動的天候システム

```csharp
[Header("Dynamic Weather")]
[SerializeField] private bool enableDynamicWeather = true;
[SerializeField] private float weatherChangeInterval = 60f;    // 天候変化間隔（秒）
```

**機能:**
- 自動天候変化（60秒間隔）
- 天候に応じたアンビエントサウンド切り替え
- AudioMixerパラメータの動的調整

#### 位置ベース環境検出

```csharp
// プレイヤー位置での環境自動判定
EnvironmentType DetectEnvironmentAtPosition(Vector3 position)
{
    // 天井検出による室内/屋外判定
    // 近辺オブジェクトによる詳細環境判定
}
```

## 使用方法

### 1. 基本セットアップ

```csharp
// シーンに必須コンポーネントを配置
- SpatialAudioManager (シングルトン)
- DynamicAudioEnvironment  
- Player (CharacterController + AudioListener + PlayerAudioSystem)
- NPC (NPCAuditorySensor)
```

### 2. SoundDataSOアセット作成

```csharp
// メニューから作成: asterivo.Unity60/Audio/Sound Data
// 設定項目：
- AudioClip配列
- 音量・ピッチ変動範囲
- 3D音響パラメータ  
- 表面材質別調整値
```

### 3. AudioEventアセット作成

```csharp
// メニューから作成: asterivo.Unity60/Audio/Events/Audio Event
// PlayerAudioSystemで参照設定
```

### 4. 表面材質タグ設定

```csharp
// Unity Editorでタグ追加:
"Concrete", "Metal", "Wood", "Carpet", "Grass", "Gravel", "Water"

// オブジェクトへのタグ割り当てまたは
// SurfaceTypeDetectorのMaterial Mapping設定
```

### 5. NPCへの聴覚センサー追加

```csharp
// NPCオブジェクトにNPCAuditorySensorコンポーネント追加
// グローバルAudioEventへの参照設定
// 注意: ReadOnly属性は使用せず、デバッグ情報は通常のSerializeFieldで表示
```

## パフォーマンス最適化

### 1. オーディオソースプール

- 最大32個の同時再生音源をプール化
- メモリアロケーション削減
- 自動返却システム

### 2. 検出頻度制御

```csharp
// NPCAuditorySensor
private const float DETECTION_CACHE_TIME = 0.1f;

// SurfaceTypeDetector  
private const float DETECTION_CACHE_TIME = 0.1f;

// DynamicAudioEnvironment
[SerializeField] private float occlusionCheckInterval = 0.1f;
```

### 3. メモリ使用量削減

- CommandPoolによるコマンドオブジェクト再利用
- 音の記憶システムの自動クリーンアップ
- 距離による早期カリング

## デバッグ機能

### 1. ビジュアルデバッグ

```csharp
// NPCAuditorySensorのOnDrawGizmosSelected:
- 聴取範囲の可視化（青色円）
- 警戒レベル別色分け表示
- 検出音源への線表示

// SurfaceTypeDetectorのOnDrawGizmos:  
- 検出範囲の可視化
- 現在の表面材質テキスト表示
```

### 2. ランタイムデバッグ情報

```csharp
[Header("Debug Info (Runtime)")]
[SerializeField, ReadOnly] private int activeSoundsCount;        // アクティブ音数
[SerializeField, ReadOnly] private string currentAlertState;     // 現在警戒状態
[SerializeField, ReadOnly] private float loudestSoundLevel;      // 最大音レベル
```

### 3. コンソールログ

```csharp
#if UNITY_EDITOR || DEVELOPMENT_BUILD
Debug.Log($"<color=green>[PlayerAudio]</color> Footstep on {currentSurface} " +
         $"(Volume: {footstepData.volume:F2}, Hearing: {footstepData.hearingRadius:F1}m)");
#endif
```

## 拡張性

### 1. 新しい表面材質の追加

```csharp
// SurfaceMaterial enumに新要素追加
// SoundDataSOの表面材質別設定に対応追加
// SurfaceTypeDetectorの判定ロジック追加
```

### 2. 新しい音源タイプの追加

```csharp
// AudioSourceType enumに新要素追加  
// NPCAuditorySensorの優先度計算ロジック調整
// 対応するSoundDataSOアセット作成
```

### 3. カスタム警戒システム

```csharp
// NPCAuditorySensorを継承
// OnAlertLevelChanged()をオーバーライド
// カスタム反応ロジックを実装
```

## トラブルシューティング

### よくある問題

1. **足音が再生されない**
   - CharacterControllerのisGroundedを確認
   - minimumMovementSpeedの設定値確認
   - footstepSounds配列に適切なSoundDataSO設定
   - PlayerControllerコンポーネントの存在確認（オプション）

2. **NPCが音に反応しない**  
   - globalAudioEventの参照設定確認
   - detectionThresholdの値確認
   - 聴取範囲と音源の距離確認
   - NPCAuditorySensorのデバッグ情報で状態確認

3. **表面材質が正しく検出されない**
   - オブジェクトのタグ設定確認（Concrete, Metal, Wood等）
   - groundLayerMaskの設定確認
   - SurfaceTypeDetectorのmaterialMappings設定

### パフォーマンス問題

1. **フレームレート低下**
   - maxConcurrentSoundsを32以下に設定
   - occlusionCheckIntervalを0.1秒以上に設定
   - 不要なDebugログを無効化

2. **メモリ使用量増加**
   - memoryDurationを5秒以下に設定
   - maxSimultaneousSoundsを10以下に設定
   - 使用していないAudioClipのアンロード

## 今後の拡張予定

- **PlayerMovement連携強化**: PlayerControllerとのより緊密な状態連携
- **音響分析ツール**: リアルタイム音響状況の可視化
- **プロシージャル音生成**: 表面材質に基づく動的音生成
- **AI反応システム**: 聴覚センサーとAI行動のより高度な結合
- **エディターツール**: 音響デバッグ用カスタムエディターウィンドウ

---

**作成日**: 2025年9月6日  
**バージョン**: 1.0  
**対応Unity版本**: Unity 6000.0.42f1  
**プロジェクト**: URP3D_Base01