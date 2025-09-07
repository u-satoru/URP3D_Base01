# Unity エディタでプレイヤーキャラクターを動かすための詳細手順

**対象プロジェクト**: URP3D_Base01 (Unity 6 + Universal Render Pipeline)  
**アーキテクチャ**: イベント駆動 + コマンドパターン  
**最終更新**: 2025年1月

---

## 📋 概要

このドキュメントは、URP3D_Base01プロジェクトにおけるプレイヤーキャラクターのセットアップ手順を詳細に解説します。このプロジェクトは通常のUnityプロジェクトとは異なり、**イベント駆動アーキテクチャ + コマンドパターン**を採用しているため、特殊な設定手順が必要です。

### 必要な知識レベル
- Unity基本操作
- コンポーネントシステムの理解
- ScriptableObjectの基本概念

---

## 🎯 手順1: 基本的なプレイヤーオブジェクトの作成

### 1-1. プレイヤーGameObjectの作成
```
1. Hierarchy右クリック → Create Empty
2. 名前を "Player" に変更
3. Transform Position を (0, 0, 0) に設定
```

### 1-2. 基本コンポーネントの追加
Player GameObjectに以下のコンポーネントを追加：

| コンポーネント | 設定内容 | 用途 |
|---------------|----------|------|
| Capsule Collider | Height: 2.0, Radius: 0.5 | 当たり判定 |
| Rigidbody | Mass: 1.0, Use Gravity: ✓ | 物理演算 |
| Capsule (子オブジェクト) | 視覚的な仮モデル | 見た目確認用 |

### 1-3. 基本的なヒエラルキー構成
```
Player
├── PlayerModel (Capsule等)
└── CameraTarget (空のGameObject)
```

---

## ⚙️ 手順2: プレイヤー専用コンポーネントの設定

### 2-1. PlayerController の追加

**場所**: `Assets/_Project/Features/Player/Scripts/PlayerController.cs`

```
1. Player GameObject を選択
2. Inspector → Add Component 
3. "PlayerController" を検索して追加
```

**主な機能**:
- 入力システムとの連携
- コマンド定義の発行
- 移動凍結システム

### 2-2. HealthComponent の追加

**場所**: `Assets/_Project/Features/Player/Scripts/HealthComponent.cs`

```
1. 同じPlayer GameObject に追加
2. Inspector設定:
   - Max Health: 100
   - Current Health: 100 (自動設定)
   - Is Invulnerable: false
```

**主な機能**:
- 体力管理システム
- ダメージ・回復処理
- DOTweenアニメーション効果

### 2-3. PlayerInput の確認

PlayerControllerコンポーネントで`[RequireComponent(typeof(PlayerInput))]`により自動追加されます。

```
PlayerInput設定確認:
- Actions: InputSystem_Actions.inputactions を割り当て
- Default Map: Player
- Behavior: Send Messages または Invoke Unity Events
```

---

## 🎮 手順3: 入力システムの設定

### 3-1. Input Action Asset の確認

**場所**: `Assets/_Project/Core/Input/InputSystem_Actions.inputactions`

**必要なアクション**:
| アクション名 | 型 | 入力デバイス | 用途 |
|-------------|----|-----------|----|
| Move | Vector2 | WASD, 左スティック | 移動 |
| Jump | Button | Spacebar, Aボタン | ジャンプ |
| Sprint | Button | Left Shift, 右肩ボタン | スプリント |

### 3-2. Input Action Asset の作成（未作成の場合）

```
1. Project右クリック → Create → Input Actions
2. 名前: "InputSystem_Actions"
3. 場所: Assets/_Project/Core/Input/
4. 上記のアクションを設定
5. Generate C# Class にチェックを入れて Apply
```

---

## 📡 手順4: イベントシステムの設定

### 4-1. 必要な ScriptableObject の作成

このプロジェクトのイベントシステムには、以下のScriptableObjectアセットが必要です：

#### CommandDefinitionGameEvent の作成
```
1. Project右クリック → Create → asterivo.Unity60/Events/Command Definition Game Event
2. ファイル名: "PlayerCommandDefinitionEvent"
3. 保存場所: Assets/_Project/Core/ScriptableObjects/Events/Core/
```

#### 移動制御用GameEvent の作成
```
以下2つのGameEventを作成:
1. Create → asterivo.Unity60/Events/Game Event
   - 名前: "FreezePlayerMovement"
2. Create → asterivo.Unity60/Events/Game Event  
   - 名前: "UnfreezePlayerMovement"
保存場所: Assets/_Project/Core/ScriptableObjects/Events/Core/
```

### 4-2. PlayerController の詳細設定

PlayerController Inspector での設定：

| フィールド | 設定値 | 説明 |
|-----------|--------|------|
| Command Definition Event | PlayerCommandDefinitionEvent | コマンド発行用イベント |
| Freeze Movement Listener | FreezePlayerMovement | 移動凍結イベント |
| Unfreeze Movement Listener | UnfreezePlayerMovement | 移動解除イベント |

---

## 🎯 手順5: コマンド実行システムの設定

### 5-1. CommandInvoker の設定

```
1. 新しい空のGameObject作成 → 名前: "CommandSystem"  
2. CommandInvoker コンポーネントを追加
3. 位置: シーンのルートレベル（DontDestroyOnLoad推奨）
```

**CommandInvoker Inspector設定**:

| フィールド | 設定値 | 説明 |
|-----------|--------|------|
| Command Definition Received | PlayerCommandDefinitionEvent | 同じアセット |
| Player Health Component | Player の HealthComponent | コマンド実行対象 |
| Max History Size | 100 | Undo履歴の最大数 |
| Enable Undo | ✓ | Undo機能有効化 |
| Enable Redo | ✓ | Redo機能有効化 |

### 5-2. 状態変更イベントの作成

```
以下のBoolEventChannelSO を作成:
1. Create → asterivo.Unity60/Events/Bool Event Channel
   - 名前: "UndoStateChanged"
2. Create → asterivo.Unity60/Events/Bool Event Channel
   - 名前: "RedoStateChanged"
保存場所: Assets/_Project/Core/ScriptableObjects/Events/Core/
```

**CommandInvoker の状態イベント設定**:
- On Undo State Changed: UndoStateChanged
- On Redo State Changed: RedoStateChanged

---

## 🎨 手順6: アニメーションシステムの設定

### 6-1. 標準的なAnimationController使用

#### Animation Controller作成
```
1. Project右クリック → Create → Animator Controller
2. 名前: "PlayerAnimationController"  
3. 場所: Assets/_Project/Features/Player/Animations/
```

#### 基本的なステート構成
```
States:
├── Idle (待機)
├── Walk (歩行)
├── Run (走行) 
├── Jump (ジャンプ)
├── Fall (落下)
├── Land (着地)
└── Crouch (しゃがみ)
```

#### パラメータ設定
```
Parameters:
- MoveSpeed (Float): 移動速度 0.0-1.0
- IsGrounded (Bool): 接地状態
- IsJumping (Bool): ジャンプ中  
- IsCrouching (Bool): しゃがみ中
- JumpTrigger (Trigger): ジャンプトリガー
```

#### Animatorコンポーネント追加
```
1. Player GameObject に Animator コンポーネント追加
2. Controller に PlayerAnimationController を割り当て
```

### 6-2. PlayerMovementAnimator の追加（DOTween効果用）

```
1. Player GameObject に PlayerMovementAnimator コンポーネント追加
2. DOTween Pro設定:
   - Jump Height: 2.0
   - Jump Duration: 0.5  
   - Damage Shake Intensity: 0.3
```

### 6-3. アニメーション統合コード例

```csharp
// PlayerController.cs での実装例
[Header("Animation")]
[SerializeField] private Animator animator;

private void OnMove(InputAction.CallbackContext context)
{
    var moveInput = context.ReadValue<Vector2>();
    
    // 1. アニメーション更新
    animator.SetFloat("MoveSpeed", moveInput.magnitude);
    animator.SetBool("IsMoving", moveInput.magnitude > 0.1f);
    animator.SetBool("IsRunning", IsSprintPressed && moveInput.magnitude > 0.1f);
    
    // 2. コマンド発行（既存システム）
    var definition = new MoveCommandDefinition(moveInput);
    onCommandDefinitionIssued?.Raise(definition);
}
```

---

## 📊 手順7: GameManager システムの設定

### 7-1. GameManager の配置

```
1. 空のGameObject作成 → 名前: "GameManager"
2. GameManager コンポーネントを追加
3. 位置: シーンのルートレベル
```

### 7-2. GameManager の設定

**主要フィールド設定**:

| カテゴリ | フィールド | 設定値 |
|---------|-----------|--------|
| Command System | Command Definition Received | PlayerCommandDefinitionEvent |
| Command System | Command Invoker | CommandSystem GameObject |
| Game Data | Game Data | 新規GameDataアセット作成 |
| Scene Management | Main Menu Scene Name | "MainMenu" |
| Scene Management | Gameplay Scene Name | "SampleScene" |

### 7-3. 必要なイベントアセットの追加作成

```
GameState用イベント作成:
1. Create → asterivo.Unity60/Events/Game State Event
   - 名前: "GameStateChanged"
   
スコア・ライフ用イベント作成:  
2. Create → asterivo.Unity60/Events/Int Game Event
   - 名前: "ScoreChanged"
   - 名前: "LivesChanged"

保存場所: Assets/_Project/Core/ScriptableObjects/Events/Core/
```

---

## 🔧 手順8: デバッグとテスト設定

### 8-1. エディタでの動作確認

**基本動作テスト**:
```
1. Play ボタンを押してプレイモード開始
2. WASD で移動確認
3. Spacebar でジャンプ確認  
4. Left Shift でスプリント確認
5. Console でエラーがないことを確認
```

### 8-2. デバッグツールの活用

**エディタメニューからアクセス**:
```
Window → asterivo.Unity60 → 以下のツールを開く:
- Event Flow Visualizer: イベントフロー確認
- Event Logger: リアルタイムイベントログ
- Command Invoker Debugger: コマンド実行履歴
```

### 8-3. Inspector でのリアルタイム監視

**監視すべき値**:
- PlayerController: Movement Frozen, Sprint Pressed
- HealthComponent: Current Health, Is Invulnerable
- CommandInvoker: Undo Stack Count, Redo Stack Count

---

## ⚡ トラブルシューティング

### 🔴 プレイヤーが動かない場合

**チェックリスト**:
```
□ PlayerInput の Actions アセットが正しく設定されている
□ CommandDefinitionGameEvent が作成・割り当てされている  
□ CommandInvoker が正しく設定されている
□ Rigidbody の Freeze Position がチェックされていない
□ Console でエラーメッセージがないか確認
```

**解決手順**:
1. Event Logger でコマンド受信を確認
2. Event Flow Visualizer でイベントフロー確認
3. CommandInvoker の Player Health Component 設定確認

### 🔴 ジャンプしない場合

**チェックリスト**:
```
□ Rigidbody の Use Gravity がチェックされている
□ Ground Layer の設定が正しい
□ Collider が Ground に正しく接地している
□ JumpCommand の実装が正しい
```

### 🔴 アニメーションが動作しない場合

**チェックリスト**:
```
□ Animator Controller が正しく割り当てられている
□ Animation Parameters が正しく設定されている
□ PlayerController でアニメーション更新コードが実装されている
□ 3Dモデルに Animator コンポーネントがある
```

### 🔴 コマンドが実行されない場合

**診断手順**:
1. Event Flow Visualizer でイベントフロー確認
2. CommandInvoker の設定内容確認
3. Event Logger でコマンド定義受信ログ確認
4. Console でエラーメッセージ確認

---

## 🎮 動作確認用のテストシーケンス

### 最終確認手順

**Phase 1: 基本機能テスト**
```
1. 基本移動: WASD で前後左右移動確認
2. ジャンプ: Spacebar でジャンプ動作確認
3. スプリント: Left Shift 長押しで高速移動確認
4. アニメーション: 各動作でアニメーションが正しく動作するか
```

**Phase 2: システム統合テスト**  
```
5. 体力システム: Inspector で体力の増減確認
6. コマンド履歴: CommandInvoker で Undo/Redo 動作確認
7. イベントフロー: Event Flow Visualizer で正常なデータフロー確認
8. デバッグ情報: Event Logger で適切なログ出力確認
```

**Phase 3: パフォーマンステスト**
```
9. フレームレート: Stats で FPS が安定しているか確認
10. メモリ使用量: Profiler でメモリリーク確認  
11. ガベージコレクション: GC Alloc が適切な範囲か確認
```

---

## 📚 関連ドキュメント

### プロジェクト固有の設計文書
- `CLAUDE.md`: プロジェクトの全体アーキテクチャ
- `Architecture_and_DesignPatterns.md`: 設計パターンの詳細
- `SDD_Markdown作成実践ガイド.md`: 開発フロー

### Unity公式ドキュメント
- Input System パッケージ
- Animation Controller
- ScriptableObject システム

---

## 🏗️ 推奨されるプロジェクト構成

### 最終的なヒエラルキー構成
```
Scene: "SampleScene"
├── GameManager
├── CommandSystem (CommandInvoker)
├── Player
│   ├── PlayerModel (Capsule + Animator)
│   └── CameraTarget
├── Main Camera
└── Environment
    └── Ground (Plane)
```

### アセット構成
```
Assets/_Project/
├── Core/ScriptableObjects/Events/Core/
│   ├── PlayerCommandDefinitionEvent.asset
│   ├── FreezePlayerMovement.asset
│   ├── UnfreezePlayerMovement.asset
│   ├── UndoStateChanged.asset
│   └── RedoStateChanged.asset
└── Features/Player/Animations/
    └── PlayerAnimationController.controller
```

---

## 💡 追加機能の拡張ポイント

### 2Dゲームへの流用
このアーキテクチャは2Dゲームにも完全に流用可能です：
- Vector3 → Vector2 への変更
- 3D Physics → 2D Physics への変更  
- Camera設定の2D用調整

### カスタムコマンドの追加
新しい player アクションは以下の手順で追加：
1. ICommandDefinition の実装クラス作成
2. ICommand の実装クラス作成
3. Input Action Asset にアクション追加
4. PlayerController にハンドラー追加

### ネットワーク対応
コマンドパターンの採用により、ネットワーク同期が容易：
- コマンドのシリアライゼーション
- 遅延実行・予測実行の実装
- クライアント・サーバー間の同期

---

**このドキュメントの手順に従うことで、プロジェクトのアーキテクチャを活用した拡張性の高いプレイヤーシステムを構築できます。**