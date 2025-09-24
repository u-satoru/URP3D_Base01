# Unity 6（6000.0.42f1）詳細セットアップ手順書

## 📋 目次
1. [プロジェクト作成](#1-プロジェクト作成)
2. [必須パッケージのインストール](#2-必須パッケージのインストール)
3. [プロジェクト設定](#3-プロジェクト設定)
4. [フォルダ構造の作成](#4-フォルダ構造の作成)
5. [コアコードの実装](#5-コアコードの実装)
6. [Assembly Definitionの設定](#6-assembly-definitionの設定)
7. [ScriptableObjectアセットの作成](#7-scriptableobjectアセットの作成)
8. [基本コンポーネントのセットアップ](#8-基本コンポーネントのセットアップ)
9. [動作確認チェックリスト](#-動作確認チェックリスト)
10. [高度なセットアップ（オプション）](#-高度なセットアップオプション)

---

## 1. プロジェクト作成

### Unity Hub設定
1. Unity Hub 3.7.0以降を使用
2. Unity 6000.0.42f1がインストール済みであることを確認

### 新規プロジェクト
```
Unity Hub → New project
→ Editor Version: 6000.0.42f1
→ Template: Universal 3D
→ Project name: Your_Project_Name
→ Create project
```

---

## 2. 必須パッケージのインストール

### Package Manager設定
**Window → Package Manager**

### 必須パッケージ
```json
{
  "dependencies": {
    "com.unity.inputsystem": "1.7.0",
    "com.unity.cinemachine": "3.1.0",
    "com.unity.render-pipelines.universal": "17.0.4",
    "com.unity.textmeshpro": "3.2.0-pre.10"
  }
}
```

### Input System & Cinemachineインストール
```
Package Manager → Unity Registry → Input System
→ Install → プロジェクト再起動（プロンプトで「Yes」）

Package Manager → Unity Registry → Cinemachine
→ Install
```

---

## 3. プロジェクト設定

### Player設定（Edit → Project Settings → Player）
```
Other Settings:
├── Rendering
│   ├── Color Space: Linear
│   └── Auto Graphics API: ✓
├── Configuration
│   ├── Scripting Backend: IL2CPP
│   ├── Api Compatibility Level: .NET Standard 2.1
│   ├── Active Input Handling: Input System Package (New)
│   └── Incremental GC: ✓
```

### Quality設定
```
Quality → URP-HighQuality
├── Render Pipeline Asset: UniversalRP-HighQuality
├── Shadows: Very High Resolution
└── V Sync Count: Every V Blank
```

### Input System Package設定
```
Input System Package:
├── Create settings asset
├── Update Mode: Dynamic Update
└── Background Behavior: Reset And Disable
```

---

## 4. フォルダ構造の作成

### Unity6_Fixedプロジェクト構造
```
Assets/
└── Unity6_Fixed/
    ├── Assembly_Definitions/
    │   ├── Unity6.Core.asmdef
    │   ├── Unity6.Player.asmdef
    │   ├── Unity6.Camera.asmdef
    │   ├── Unity6.Systems.asmdef
    │   └── Unity6.Optimization.asmdef
    ├── Core/
    │   ├── Data/
    │   │   └── GameData.cs
    │   ├── Events/
    │   │   ├── GameEvent.cs
    │   │   ├── GenericGameEvent.cs
    │   │   ├── GameEventListener.cs
    │   │   ├── IGameEventListener.cs
    │   │   ├── GameDataEvent.cs
    │   │   ├── PlayerStateEvent.cs
    │   │   ├── Vector2GameEvent.cs
    │   │   ├── Vector2GameEventListener.cs
    │   │   └── CameraStateEvent.cs
    │   └── Player/
    │       └── PlayerState.cs
    ├── Player/
    │   ├── PlayerController.cs
    │   ├── PlayerStateMachine.cs
    │   └── States/
    │       └── BasePlayerState.cs
    ├── Camera/
    │   └── CinemachineIntegration.cs
    └── Systems/
        └── GameManager.cs
```

### フォルダ作成コマンド
```bash
cd Assets
mkdir Unity6_Fixed
cd Unity6_Fixed
mkdir -p Assembly_Definitions
mkdir -p Core/Data Core/Events Core/Player
mkdir -p Player/States
mkdir -p Camera
mkdir -p Systems
```

---

## 5. コアコードの実装

### Unity6_Fixedプロジェクトからコピーするファイル

#### Core/Events/
- `GameEvent.cs` - 基本イベントチャネル
- `GenericGameEvent.cs` - ジェネリック型付きイベント
- `GameEventListener.cs` - イベントリスナー基底クラス
- `IGameEventListener.cs` - リスナーインターフェース
- `PlayerStateEvent.cs` - プレイヤー状態イベント
- `GameDataEvent.cs` - データイベント定義
- `Vector2GameEvent.cs` - Vector2パラメータ付きイベント（カメラルック入力用）
- `Vector2GameEventListener.cs` - Vector2イベントリスナー
- `CameraStateEvent.cs` - カメラ状態変更イベント

#### Core/Player/
- `PlayerState.cs` - PlayerStateとGameStateのenum定義

#### Core/Data/
- `GameData.cs` - GameDataとPlayerDataPayloadクラス

#### Player/
- `PlayerController.cs` - プレイヤーコントローラー
- `PlayerStateMachine.cs` - ステートマシン
- `States/BasePlayerState.cs` - ステート基底クラス

#### Camera/
- `CinemachineIntegration.cs` - Cinemachine 3.1統合カメラシステム

#### Systems/
- `GameManager.cs` - ゲームマネージャー

---

## 6. Assembly Definitionの設定

### Unity6.Core.asmdef
```json
{
    "name": "Unity6.Core",
    "rootNamespace": "Unity6.Core",
    "references": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

### Unity6.Player.asmdef
```json
{
    "name": "Unity6.Player",
    "rootNamespace": "Unity6.Player",
    "references": [
        "Unity6.Core",
        "Unity6.Camera",
        "Unity.InputSystem"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [
        {
            "name": "com.unity.inputsystem",
            "expression": "1.7.0",
            "define": "NEW_INPUT_SYSTEM"
        }
    ],
    "noEngineReferences": false
}
```

### Unity6.Camera.asmdef
```json
{
    "name": "Unity6.Camera",
    "rootNamespace": "Unity6.Camera",
    "references": [
        "Unity6.Core",
        "Unity6.Player",
        "Unity.Cinemachine"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [
        {
            "name": "com.unity.cinemachine",
            "expression": "3.1.0",
            "define": "CINEMACHINE_3_1"
        }
    ],
    "noEngineReferences": false
}
```

### Unity6.Systems.asmdef
```json
{
    "name": "Unity6.Systems",
    "rootNamespace": "Unity6.Systems",
    "references": [
        "Unity6.Core"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

---

## 7. ScriptableObjectアセットの作成

### イベントアセット作成手順
1. **基本イベント**
   ```
   Project → Create → Unity6/Events/
   → Game Event → "OnPlayerJump"
   → Game Event → "OnPlayerDeath"
   → Game Event → "OnPlayerLanded"
   ```

2. **型付きイベント**
   ```
   → Float Event → "OnHealthChanged"
   → Float Event → "OnStaminaChanged"
   → Bool Event → "OnGroundedChanged"
   → Vector3 Event → "OnPositionChanged"
   → Vector2 Game Event → "OnLookInput"
   → Vector2 Game Event → "OnCameraRotation"
   ```

3. **状態イベント**
   ```
   → Player State Event → "OnStateChangeRequest"
   → Player State Event → "OnStateChanged"
   → Game State Event → "OnGameStateChanged"
   → Camera State Event → "OnCameraStateChanged"
   ```

4. **カメラシステムイベント**
   ```
   → Camera State Event → "OnCameraFollow"
   → Camera State Event → "OnCameraAim"
   → Camera State Event → "OnCameraCombat"
   → Game Event → "OnAimStarted"
   → Game Event → "OnAimEnded"
   ```

4. **データイベント**
   ```
   → Game Data Event → "OnGameDataUpdated"
   → Player Data Event → "OnPlayerDataUpdated"
   ```

---

## 8. 基本コンポーネントのセットアップ

### Playerオブジェクト設定
```
Hierarchy → Create Empty → "Player"
Add Components:
├── Character Controller
├── Player Input (Input System)
├── PlayerController
└── PlayerStateMachine
```

### PlayerController設定
Inspector設定:
- **Static Event Listeners**
  - 各種GameEventListenerコンポーネントを追加
  - damageReceivedListener
  - damageAmountListener
  - forceStateChangeListener
  
- **State Change Events (Output)**
  - onStateChangeRequest → PlayerStateEventアセット
  - onGroundedChanged → BoolGameEventアセット
  - onHealthChanged → FloatGameEventアセット
  - onStaminaChanged → FloatGameEventアセット

- **Camera Events (Output)**
  - lookInputEvent → Vector2GameEventアセット
  - cameraStateChangeEvent → CameraStateEventアセット

### CinemachineIntegration設定
```
CinemachineIntegrationコンポーネント:
├── Camera System Configuration
│   ├── Camera Configs List
│   │   ├── Follow Camera (Priority: 10)
│   │   ├── Aim Camera (Priority: 15)
│   │   ├── Combat Camera (Priority: 12)
│   │   └── Cinematic Camera (Priority: 20)
│   └── Default Camera State: Follow
│
├── Event Channels
│   ├── cameraStateChangeEvent → CameraStateEventアセット
│   ├── lookInputEvent → Vector2GameEventアセット
│   ├── aimStartedEvent → GameEventアセット
│   └── aimEndedEvent → GameEventアセット
│
└── Look Settings
    ├── Look Sensitivity: 2.0
    ├── Vertical Look Limit: 80
    └── Invert Y Axis: false
```

### Cinemachine Virtual Cameras設定
```
各カメラ状態用のVirtual Cameraを作成:
├── FollowCamera (CinemachineVirtualCamera)
│   ├── Priority: 10
│   ├── Follow: Player Transform
│   ├── LookAt: Player Transform
│   └── Body: 3rd Person Follow
│
├── AimCamera (CinemachineVirtualCamera)
│   ├── Priority: 15 (非アクティブ時は0)
│   ├── Follow: Player Transform
│   ├── LookAt: Player Transform
│   └── Body: Composer
│
└── CombatCamera (CinemachineVirtualCamera)
    ├── Priority: 12 (非アクティブ時は0)
    ├── Follow: Player Transform
    ├── LookAt: Player Transform
    └── Body: Transposer
```

### GameManager設定
```
Hierarchy → Create Empty → "GameManager"
Add Component: GameManager

Inspector設定:
├── Event Channels - Input
│   ├── onStartGameCommand → GameEventアセット
│   ├── onPauseGameCommand → GameEventアセット
│   └── onResumeGameCommand → GameEventアセット
│
├── Event Channels - Output
│   ├── gameStateChangedEvent → GameStateEventアセット
│   └── gameDataUpdatedEvent → GameDataEventアセット
│
└── Event Listeners
    ├── startGameListener → GameEventListenerコンポーネント
    ├── pauseGameListener → GameEventListenerコンポーネント
    └── resumeGameListener → GameEventListenerコンポーネント
```

### PlayerStateMachine設定
```
Inspector設定:
├── State Configuration
│   ├── currentState: Idle
│   └── previousState: Idle
│
└── Event Channels
    ├── stateChangeRequestEvent → PlayerStateEventアセット
    └── stateChangedEvent → PlayerStateEventアセット
```

---

## 🎯 動作確認チェックリスト

### コンパイル確認
- [ ] プロジェクトがエラーなくコンパイル
- [ ] Assembly Definitionが正しく認識
- [ ] 名前空間が正しく解決

### イベントシステム確認
- [ ] ScriptableObjectアセットが作成済み
- [ ] イベントリスナーが正しく接続
- [ ] OnEnable/OnDisableでの登録/解除が動作

### Input System確認
- [ ] PlayerInputコンポーネントが設定済み
- [ ] Input Actionsアセットが接続済み
- [ ] 移動・ジャンプ入力が反応

### Cinemachineシステム確認
- [ ] Cinemachine Brain がMain Cameraに追加済み
- [ ] Virtual Cameraが各状態用に作成済み
- [ ] CinemachineIntegrationコンポーネントが設定済み
- [ ] カメラ状態変更が正しく動作
- [ ] ルック入力によるカメラ回転が動作
- [ ] カメラブレンドが滑らかに実行

---

## 🚀 高度なセットアップ（オプション）

### Cinemachine Impulse System設定
```
Cinemachine Impulse Source:
├── 爆発エフェクト用
├── 着地時の振動
└── ダメージ時のシェイク
```

### カメラブレンド設定
```
Cinemachine Brain → Custom Blends:
├── Follow → Aim: Cut (0.0s)
├── Aim → Follow: EaseInOut (0.5s)
├── Follow → Combat: EaseInOut (1.0s)
└── Combat → Follow: EaseInOut (0.8s)
```

### 最適化設定
```
Unity6.Optimization.asmdef対応:
├── オブジェクトプール
├── イベントバッファリング
└── LODシステム統合
```

### ゲーム動作確認
- [ ] Playモードでエラーなし
- [ ] プレイヤーが移動可能
- [ ] 状態遷移が正常動作
- [ ] イベントが正しく発火

---

## ⚠️ トラブルシューティング

### Input System未反応
```
Edit → Project Settings → Player
→ Active Input Handling: Input System Package (New)
→ プロジェクト再起動
```

### コンパイルエラー
```
1. Assembly Definition参照を確認
2. 名前空間を確認（Unity6.Core, Unity6.Player, Unity6.Systems）
3. パッケージ依存関係を確認
```

### NullReferenceException
```
1. Inspector上でScriptableObjectアセットが設定されているか確認
2. GetComponent呼び出しの前にnullチェック
3. イベントリスナーコンポーネントが追加されているか確認
```

---

## 📚 参考資料

- [Unity 6 Documentation](https://docs.unity3d.com/6000.0/Documentation/Manual/)
- [Cinemachine Documentation](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/index.html)
- [ScriptableObject Event System](https://unity.com/how-to/architect-game-code-scriptable-objects)
- [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)
- [Event-Driven Architecture](https://unity.com/how-to/create-modular-game-architecture-scriptable-objects)

---

## 📋 実装リファレンス

### イベント駆動アーキテクチャの特徴
- **疎結合**: ScriptableObjectイベントによるコンポーネント間通信
- **型安全性**: ジェネリック型によるコンパイル時エラー検出
- **パフォーマンス**: HashSetベースの高速リスナー管理
- **デバッグ性**: エディタ向けデバッグ機能とログ出力

### Unity 6 (6000.0.42f1) 対応機能
- **新Input System**: 完全統合とイベント駆動制御
- **Assembly Definitions**: モジュラー構造による高速コンパイル
- **Cinemachine 3.1**: 最新機能を活用したカメラシステム
- **URP最適化**: Universal Render Pipelineとの完全互換性

### 実装済みコンポーネント
```
Unity6_Fixed/
├── コアシステム
│   ├── イベントシステム (9ファイル)
│   ├── データ定義 (1ファイル)
│   └── プレイヤー状態管理 (1ファイル)
├── プレイヤーシステム
│   ├── PlayerController (895行)
│   ├── ステートマシン (3ファイル)
│   └── Input System統合
├── カメラシステム
│   ├── CinemachineIntegration (450行)
│   ├── Vector2入力制御
│   └── 状態ベース切り替え
└── ゲームシステム
    └── GameManager (統合管理)
```

---

## 🚀 次のステップ

1. **プレイヤー機能の拡張**
   - アニメーション統合
   - 戦闘システム実装
   - インタラクションシステム

2. **ゲームシステムの構築**
   - セーブ/ロード機能
   - インベントリシステム
   - クエストシステム

3. **最適化**
   - オブジェクトプーリング
   - LODシステム
   - Addressables統合
