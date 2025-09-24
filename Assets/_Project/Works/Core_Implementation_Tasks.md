# Core システム実装タスク一覧

**プロジェクト**: URP3D_Base01  
**目的**: 「Unity 6プロジェクトのベーステンプレート」として完全に機能するCoreシステムの実装  
**作成日**: 2025年1月  
**優先度**: 🔴 **最高優先度** - システムの基盤として必須

---

## 📋 概要

現在のプロジェクトは「アーキテクチャのデモンストレーション」レベルに留まっており、実用的なテンプレートとしては不完全です。本ドキュメントは、真の「ベーステンプレート」として機能するために必要な実装タスクを定義します。

### 🎯 実装後の目標状態

- Unity_Player_Setup_Guide.mdの手順に従うだけで、プレイヤーキャラクターが**即座に動作**
- 新規プロジェクト開始時に、**追加のアセット作成が不要**
- **「Clone & Play」**を実現する完全なテンプレート

---

## 🔥 最高優先度タスク（Phase 1）

### Task 1.1: Input Action Asset の実装
**場所**: `Assets/_Project/Core/Input/`  
**ファイル**: `InputSystem_Actions.inputactions`

#### 実装内容
```
Action Maps:
└── Player
    ├── Move (Vector2) - WASD, Left Stick
    ├── Jump (Button) - Space, A Button
    ├── Sprint (Button) - Left Shift, Right Shoulder
    ├── Crouch (Button) - Left Ctrl, B Button
    └── Interact (Button) - E, X Button

UI Maps:
└── UI
    ├── Navigate (Vector2) - WASD, D-Pad
    ├── Submit (Button) - Enter, A Button
    ├── Cancel (Button) - Escape, B Button
    └── Pause (Button) - Escape, Menu Button
```

#### 設定要件
- Generate C# Class: ✅ 有効
- Class Name: `InputSystem_Actions`
- Class Namespace: `asterivo.Unity60.Core.Input`

---

### Task 1.2: 基本GameEventアセットの実装
**場所**: `Assets/_Project/Core/ScriptableObjects/Events/Core/`

#### プレイヤー制御用イベント
```
必須アセット:
├── PlayerCommandDefinitionEvent.asset (CommandDefinitionGameEvent)
├── FreezePlayerMovement.asset (GameEvent)
├── UnfreezePlayerMovement.asset (GameEvent)
├── UndoStateChanged.asset (BoolGameEvent)
└── RedoStateChanged.asset (BoolGameEvent)
```

#### ゲーム状態管理用イベント
```
システムイベント:
├── GameStateChanged.asset (GameStateEvent)
├── ScoreChanged.asset (IntGameEvent)
├── LivesChanged.asset (IntGameEvent)
├── HealthChanged.asset (FloatGameEvent)
└── LevelCompleted.asset (GameEvent)
```

#### UI制御用イベント
```
UIイベント:
├── ShowMainMenu.asset (GameEvent)
├── ShowGameHUD.asset (GameEvent)
├── ShowPauseMenu.asset (GameEvent)
├── ShowInventory.asset (GameEvent)
├── ShowSettings.asset (GameEvent)
├── HideCurrentUI.asset (GameEvent)
└── HideAllUI.asset (GameEvent)
```

---

### Task 1.3: デフォルトAnimation Controllerの実装
**場所**: `Assets/_Project/Core/Animations/Templates/`  
**ファイル**: `DefaultPlayerAnimationController.controller`

#### ステート構成
```
Layer: Base Layer
├── Entry → Idle
├── Idle (Default State)
├── Movement (1D BlendTree)
│   ├── Idle (Threshold: 0.0)
│   ├── Walk (Threshold: 0.3)
│   ├── Jog (Threshold: 0.7)
│   └── Run (Threshold: 1.0)
├── Jump
├── Fall
├── Land
└── Crouch

Layer: Upper Body (Additive)
├── Idle Upper
├── Wave
└── Point
```

#### パラメータ定義
```
Parameters:
├── MoveSpeed (Float) - 移動速度制御
├── MoveX (Float) - 2D BlendTree用 X軸
├── MoveZ (Float) - 2D BlendTree用 Z軸
├── IsGrounded (Bool) - 接地状態
├── IsJumping (Bool) - ジャンプ中フラグ
├── IsCrouching (Bool) - しゃがみ中フラグ
├── VerticalVelocity (Float) - 縦方向速度
├── JumpTrigger (Trigger) - ジャンプトリガー
└── LandTrigger (Trigger) - 着地トリガー
```

---

## 🟡 高優先度タスク（Phase 2）

### Task 2.1: Command Definition クラスの完全実装
**場所**: `Assets/_Project/Core/Commands/Definitions/`

#### 実装すべきCommand Definition
```csharp
基本移動系:
├── MoveCommandDefinition.cs
├── JumpCommandDefinition.cs  
├── SprintCommandDefinition.cs
└── CrouchCommandDefinition.cs

戦闘系:
├── AttackCommandDefinition.cs
├── DefendCommandDefinition.cs
├── DamageCommandDefinition.cs
└── HealCommandDefinition.cs

インタラクション系:
├── InteractCommandDefinition.cs
├── PickupCommandDefinition.cs
├── UseItemCommandDefinition.cs
└── OpenInventoryCommandDefinition.cs

システム系:
├── SaveGameCommandDefinition.cs
├── LoadGameCommandDefinition.cs
├── PauseGameCommandDefinition.cs
└── QuitGameCommandDefinition.cs
```

### Task 2.2: デフォルトGameDataアセットの実装
**場所**: `Assets/_Project/Core/ScriptableObjects/Data/`  
**ファイル**: `DefaultGameData.asset`

#### 含むべきデフォルト値
```
Player Settings:
├── Default Health: 100
├── Default Speed: 5.0
├── Jump Force: 10.0
└── Sprint Multiplier: 2.0

Game Settings:  
├── Default Lives: 3
├── Starting Score: 0
├── Max Score: 999999
└── Game Version: "1.0.0"

Physics Settings:
├── Gravity Scale: 1.0
├── Ground Layer: "Ground"
├── Player Layer: "Player"
└── Enemy Layer: "Enemy"
```

### Task 2.3: プリファブテンプレートの実装
**場所**: `Assets/_Project/Core/Prefabs/Templates/`

#### 必須プリファブ
```
システムプリファブ:
├── DefaultPlayer.prefab - 完全設定済みプレイヤー
├── GameManager.prefab - 完全設定済みゲームマネージャー
├── CommandSystem.prefab - 完全設定済みコマンドシステム
├── UICanvas.prefab - 基本UI構成
└── AudioManager.prefab - サウンドシステム

環境プリファブ:
├── DefaultGround.prefab - 基本的な地面
├── DefaultCamera.prefab - 基本カメラ設定
├── DefaultLighting.prefab - 基本ライティング設定
└── SpawnPoint.prefab - スポーンポイント
```

---

## 🟢 中優先度タスク（Phase 3）

### Task 3.1: エディタメニューの改善
**場所**: `Assets/_Project/Core/Editor/`

#### 実装すべきメニュー
```csharp
Unity Menu Items:
├── "asterivo.Unity60/Quick Setup/Create Default Scene" - 基本シーン自動作成
├── "asterivo.Unity60/Quick Setup/Setup Player Character" - プレイヤー自動配置
├── "asterivo.Unity60/Quick Setup/Create Game Manager" - GameManager自動配置  
├── "asterivo.Unity60/Quick Setup/Validate Project Setup" - 設定検証
└── "asterivo.Unity60/Quick Setup/Fix Common Issues" - 一般的な問題の自動修正
```

### Task 3.2: デフォルトシーンテンプレートの実装
**場所**: `Assets/_Project/Scenes/Templates/`

#### シーンファイル
```
Template Scenes:
├── MainMenuTemplate.unity - メインメニューテンプレート
├── GameplayTemplate.unity - ゲームプレイテンプレート  
├── LoadingTemplate.unity - ローディング画面テンプレート
└── SettingsTemplate.unity - 設定画面テンプレート
```

#### 各シーンの構成
```
GameplayTemplate.unity:
├── GameManager (GameManager component)
├── CommandSystem (CommandInvoker component)
├── Player (完全設定済み)
├── Main Camera (Cinemachine設定済み)
├── UI Canvas (基本UI完備)
├── Environment/Ground
├── Lighting
└── Audio Manager
```

### Task 3.3: ドキュメント自動生成システム
**場所**: `Assets/_Project/Core/Editor/Documentation/`

#### 実装機能
```csharp
Auto Documentation:
├── GenerateEventFlowDiagram.cs - イベントフロー図生成
├── GenerateCommandListDoc.cs - コマンド一覧生成
├── GenerateComponentDependencyGraph.cs - 依存関係図生成
└── GenerateSetupValidationReport.cs - セットアップ検証レポート
```

---

## 🔵 低優先度タスク（Phase 4）

### Task 4.1: サンプルコンテンツの実装
**場所**: `Assets/_Project/Samples/`

#### サンプル内容
```
Sample Content:
├── BasicMovementDemo/ - 基本移動デモシーン
├── CombatSystemDemo/ - 戦闘システムデモ
├── UISystemDemo/ - UI制御デモ
├── AudioSystemDemo/ - サウンドシステムデモ
└── EventSystemDemo/ - イベント連携デモ
```

### Task 4.2: Unit Tests の実装
**場所**: `Assets/_Project/Tests/`

#### テストカテゴリ
```csharp
Test Categories:
├── Core/Commands/ - コマンドシステムテスト
├── Core/Events/ - イベントシステムテスト
├── Features/Player/ - プレイヤー機能テスト
├── Integration/ - 統合テスト
└── Performance/ - パフォーマンステスト
```

### Task 4.3: CI/CD パイプライン設定
**場所**: `.github/workflows/`

#### パイプライン構成
```yaml
Workflows:
├── build-and-test.yml - ビルド & テスト自動化
├── code-quality.yml - コード品質チェック  
├── documentation-update.yml - ドキュメント自動更新
└── release-package.yml - リリースパッケージ作成
```

---

## 📊 実装スケジュール

### Phase 1（1週間）- 基盤システム
- [ ] Input Action Asset 作成
- [ ] 必須GameEventアセット作成  
- [ ] デフォルトAnimation Controller作成
- [ ] PlayerControllerのBlendTree実装

### Phase 2（1週間）- コア機能拡張
- [ ] Command Definition完全実装
- [ ] DefaultGameDataアセット作成
- [ ] プリファブテンプレート作成

### Phase 3（1週間）- 開発者体験改善
- [ ] エディタメニュー実装
- [ ] シーンテンプレート作成
- [ ] ドキュメント自動生成

### Phase 4（1週間）- 品質向上
- [ ] サンプルコンテンツ作成
- [ ] Unit Tests実装
- [ ] CI/CD設定

---

## 🎯 完了条件（Definition of Done）

### 最小実行可能製品（MVP）
- [ ] Unity_Player_Setup_Guide.mdの手順のみでプレイヤーが動作
- [ ] 追加のScriptableObjectアセット作成が不要
- [ ] エラーなしでビルド可能

### 完全なテンプレート
- [ ] "File → New Scene from Template" で即座に動作可能
- [ ] 基本的なゲーム機能がすべて動作
- [ ] プロジェクト検証ツールで100%パス

### プロダクション対応
- [ ] Unit Tests のカバレッジ80%以上
- [ ] パフォーマンス要件を満たす
- [ ] 完全なドキュメント整備

---

## 🚨 注意事項

### 既存ファイルへの影響
- **既存のソースコードは変更しない**
- **新規ファイル作成のみで対応**
- **後方互換性を維持**

### アーキテクチャ準拠
- **イベント駆動アーキテクチャに準拠**
- **コマンドパターンを維持**
- **ScriptableObject設計哲学に従う**

### 品質基準
- **すべてのアセットにXMLドキュメント**
- **Odin Inspector対応**  
- **パフォーマンス最適化済み**

---

**このタスクリストの完了により、URP3D_Base01は真の「Unity 6プロジェクトベーステンプレート」として機能します。**
