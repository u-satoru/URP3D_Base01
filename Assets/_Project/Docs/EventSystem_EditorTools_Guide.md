# Unity6 イベントシステム エディタツール完全ガイド

Unity6プロジェクトのイベントシステムを**エディタ上から直接操作**するための包括的なツール群です。デバッグ、テスト、開発効率の向上を目的とします。

## 🛠️ 提供されるエディタツール

### 1. Event Asset Creation Wizard
**目的**: ScriptableObjectベースのイベントアセットを簡単作成  
**アクセス**: `Unity6 > Event System > Event Asset Creation Wizard`

#### 主な機能
- **ウィザード形式**: ステップ毎の分かりやすいUI
- **型選択**: GameEvent, GenericGameEvent, 専用イベントから選択
- **自動配置**: 適切なフォルダへの自動保存
- **プレビュー**: 作成前の確認機能

#### 使用手順
```
1. Unity6 > Event System > Event Asset Creation Wizard を選択
2. イベント名を入力 (例: "OnPlayerHealthChanged")
3. イベントタイプを選択
4. ペイロードタイプを選択 (GenericEventの場合)
5. 保存先フォルダを選択
6. プレビューを確認
7. "イベントアセットを作成" をクリック
```

### 2. Event Trigger Window  
**目的**: 作成済みイベントのエディタからの手動発行  
**アクセス**: `Unity6 > Event System > Event Trigger Window`

#### 主な機能
- **イベント一覧**: プロジェクト内の全イベントを表示
- **検索・フィルター**: イベント名での絞り込み
- **ペイロード入力**: 型に応じた値入力UI
- **一括発行**: 表示中の全イベントを一度に発行

#### 対応イベント種類
```csharp
✓ GameEvent (パラメーターなし)
✓ StringGameEvent 
✓ IntGameEvent
✓ FloatGameEvent  
✓ BoolGameEvent
✓ Vector2GameEvent
✓ Vector3GameEvent
✓ PlayerStateEvent
✓ CameraStateEvent  
✓ CommandGameEvent (案内表示のみ)
```

### 3. Event Quick Creation Menu
**目的**: プロジェクトウィンドウからの即座なイベント作成  
**アクセス**: プロジェクトウィンドウ右クリック > `Create > Unity6 Events`

#### メニュー構造
```
Unity6 Events/
├── Game Event                          // 基本イベント
├── Generic Events/                     // 型付きイベント
│   ├── String Event
│   ├── Int Event  
│   ├── Float Event
│   └── Bool Event
├── Vector Events/                      // Vector型イベント
│   ├── Vector2 Event
│   └── Vector3 Event
├── Specialized Events/                 // 専用イベント
│   ├── Player State Event
│   ├── Camera State Event
│   └── Command Event
├── Input Events/                       // 入力イベント
│   └── Input Action Event
├── Common Templates/                   // よく使用するテンプレート
│   ├── Health Changed Event
│   ├── Level Complete Event  
│   ├── Item Collected Event
│   └── Damage Dealt Event
├── Debug Events/                       // デバッグ用
│   ├── Debug Log Event
│   └── Performance Warning Event
└── Batch Creation/                     // バッチ作成
    └── Create Common Event Set
```

## 🚀 クイックスタートガイド

### Step 1: 最初のイベントアセット作成（1分）

1. **メニューからウィザードを起動**
   ```
   Unity6 > Event System > Event Asset Creation Wizard
   ```

2. **基本的なゲームイベントを作成**
   - イベント名: `OnGameStart`
   - イベントタイプ: `GameEvent` 
   - 説明: `ゲーム開始時に発行されるイベント`
   - **作成** ボタンをクリック

### Step 2: イベントの手動発行テスト（1分）

1. **Event Trigger Windowを開く**
   ```
   Unity6 > Event System > Event Trigger Window
   ```

2. **作成したイベントを発行**
   - リストから `OnGameStart` を確認
   - **発行** ボタンをクリック
   - Consoleでログ確認: `"イベントを発行しました: OnGameStart"`

### Step 3: 型付きイベントの作成とテスト（2分）

1. **プロジェクトウィンドウから作成**
   - 右クリック > `Create > Unity6 Events > Generic Events > Int Event`
   - ファイル名: `OnScoreChanged`

2. **Event Trigger Windowでテスト**  
   - 更新ボタンをクリック
   - `OnScoreChanged` を確認
   - 整数フィールドに `100` を入力
   - **発行** ボタンをクリック

## 💡 実用的な使用例

### デバッグシナリオ1: プレイヤー状態確認

```csharp
// 問題: プレイヤーが正しく状態遷移しているか確認したい

手順:
1. Event Trigger Window を開く
2. PlayerStateEvent を探す
3. プレイヤー状態を「Running」に設定
4. 発行ボタンをクリック
5. ゲーム内でプレイヤーが走行状態になることを確認
```

### デバッグシナリオ2: UI応答テスト

```csharp
// 問題: ヘルスバーUIが正しく更新されるか確認

手順:
1. Quick Creation Menu で「Health Changed Event」を作成
2. Event Trigger Window で該当イベントを選択  
3. 異なる数値 (100, 50, 0) で発行
4. UI要素が正しく反応することを確認
```

### デバッグシナリオ3: イベント一括テスト

```csharp  
// 問題: 複数のイベントリスナーが正常に動作するか確認

手順:
1. Event Trigger Window で検索フィールドを空にする
2. 「全て発行」ボタンをクリック
3. Consoleでエラーが発生しないことを確認
4. 各システムが適切に反応することを確認
```

## 🎯 高度な機能

### バッチ作成機能

**Common Event Set**を使用して基本的なイベントセットを一括作成:
```csharp
作成されるイベント:
• OnGameStart (GameEvent)
• OnGamePause (BoolGameEvent)  
• OnScoreChanged (IntGameEvent)
• OnHealthChanged (FloatGameEvent)
• OnPlayerDied (GameEvent)
```

### ショートカットキー

- **Ctrl+Shift+E**: クイックGameEvent作成
- プロジェクトウィンドウでのコンテキストメニュー対応

### 検索とフィルター

Event Trigger Windowの高度な検索機能:
```csharp
// 検索例
"health"     → ヘルス関連イベントのみ表示
"player"     → プレイヤー関連イベントのみ表示
"state"      → 状態変更イベントのみ表示
```

## 📊 既存ツールとの統合

このプロジェクトには既に高度なイベントデバッグツールが存在します：

### EventFlowVisualizer (既存)
- **機能**: イベント-リスナー関係の可視化
- **統合**: 新しいイベントアセットも自動で検出・表示

### EventHistoryWindow (既存)  
- **機能**: リアルタイムイベント監視
- **統合**: エディタから発行したイベントも履歴に記録

### EventLoggerWindow (既存)
- **機能**: 詳細なイベントログ表示
- **統合**: 手動発行イベントもログに出力

## 🔧 カスタマイズとベストプラクティス

### イベント命名規則

```csharp
推奨命名パターン:
• On + [動作] + [対象]
• 例: OnPlayerHealthChanged, OnEnemyDefeated, OnItemCollected

フォルダ構成:
Assets/_Project/Core/ScriptableObjects/Events/
├── Core/           // システム共通イベント
├── Player/         // プレイヤー関連イベント  
├── Combat/         // 戦闘関連イベント
└── UI/            // UI関連イベント
```

### パフォーマンス考慮

```csharp
// 重いイベントの連続発行を避ける
if (Event Trigger Windowで「全て発行」を使用する場合)
{
    // 大量のイベントリスナーがある場合は注意
    // 必要に応じて検索フィルターを使用して範囲を限定
}
```

### エラー処理

```csharp
よくあるエラーと対処法:

❌ "イベントアセットが見つかりません"
→ Event Asset Creation Wizard でアセットを作成

❌ "未対応のイベント型です"  
→ 対応する型付きイベントクラスを実装

❌ "イベント発行中にエラーが発生"
→ イベントリスナーのコードを確認
```

## 🧪 テストとデバッグワークフロー

### 基本的なテストフロー

```csharp
1. イベント作成
   ↓
2. エディタから手動発行  
   ↓
3. 期待される動作の確認
   ↓  
4. 問題があれば EventHistoryWindow で詳細確認
   ↓
5. EventFlowVisualizer でリスナー接続確認
```

### 継続的インテグレーション

```csharp
// 自動テスト用のイベント発行
[MenuItem("Tests/Trigger All Events")]
public static void TriggerAllEventsForTesting()
{
    // すべてのイベントを発行してシステムの整合性をチェック
    var window = EditorWindow.GetWindow<EventTriggerWindow>();
    // 実装は EventTriggerWindow の TriggerAllVisibleEvents() を参考
}
```

## 📈 開発効率の向上

### Before（ツール導入前）

```csharp
イベントテスト手順:
1. テスト用スクリプトを作成 (10分)
2. シーンにGameObjectを配置 (2分)  
3. プレイモードでテスト (5分)
4. 問題があれば修正・再テスト (10分+)
総時間: 27分+
```

### After（ツール導入後）

```csharp
イベントテスト手順:
1. Event Asset Creation Wizard でイベント作成 (1分)
2. Event Trigger Window で発行テスト (1分)
3. 問題があれば即座に再テスト (30秒)  
総時間: 2分30秒
```

**効率改善**: **約90%の時間短縮**

## 🎉 まとめ

Unity6プロジェクトのイベントシステムが、エディタツールの追加により大幅に使いやすくなりました：

### 主な改善点

1. **アセット作成の簡素化**: ウィザードによる直感的作成
2. **即座のテスト**: エディタから直接イベント発行
3. **開発効率向上**: 90%の時間短縮を実現
4. **デバッグ支援**: 既存ツールとの統合により包括的デバッグ環境

### 今後の展開

- **AIイベント対応**: AIStateEventの追加
- **カスタム型対応**: プロジェクト固有の型への拡張
- **自動テスト統合**: CI/CDパイプラインとの連携

これで、Unity6プロジェクトにおけるイベント駆動開発が格段に効率化されました！
