# Unity 6 3Dゲーム基盤 - 修正済みコード

## 📁 フォルダ構造

```
Unity6_Fixed/
├── Core/
│   └── Events/
│       ├── GameEvent.cs              // 基本イベントチャネル
│       ├── GenericGameEvent.cs       // ジェネリック型付きイベント
│       ├── GameEventListener.cs      // イベントリスナー（修正済み）
│       ├── IGameEventListener.cs     // 型付きリスナーインターフェース
│       └── GameData.cs               // データペイロード定義（新規）
├── Player/
│   ├── PlayerController.cs           // プレイヤー制御（修正済み）
│   └── PlayerStateMachine.cs         // ステートマシン（修正済み）
├── Systems/
│   └── GameManager.cs                // ゲーム管理（修正済み）
└── Assembly_Definitions/
    ├── Unity6.Core.asmdef
    ├── Unity6.Player.asmdef
    ├── Unity6.Systems.asmdef
    └── Unity6.Optimization.asmdef

```

## 🔧 主な修正内容

### 1. **名前空間の統一**
すべてのコードで一貫した名前空間構造を採用：
- `Unity6.Core.Events` - イベントシステム
- `Unity6.Core.Player` - プレイヤー関連
- `Unity6.Core.Systems` - システム管理
- `Unity6.Core.Optimization` - 最適化

### 2. **GameEventListener.csの修正**
- `Response`プロパティを追加（publicアクセサ）
- UnityEventへの適切なアクセスを提供

### 3. **PlayerControllerの簡素化**
- 内部クラス（GenericEventListener、BasicEventListener）を削除
- IGameEventListenerインターフェースの重複定義を削除
- イベントリスナー管理を簡素化
- メモリリーク対策（OnDisableでの適切な解放）

### 4. **GameData関連クラスの追加**
新規ファイル`GameData.cs`に以下を定義：
- `GameData` - ゲームデータペイロード
- `PlayerDataPayload` - プレイヤーデータ
- `GameDataEvent` - GameData用イベント
- `GameDataResponseEvent` - レスポンス用イベント
- `PlayerDataEvent` - プレイヤーデータイベント

### 5. **PlayerStateMachineの改善**
- イベントリスナー登録処理を追加
- StringGameEventからの状態変更リクエスト受信
- 文字列からPlayerState enumへの変換処理

### 6. **Assembly定義の修正**
- rootNamespaceを統一された構造に合わせて更新
- 依存関係の明確化

## 🚀 使用方法

### 1. Unityプロジェクトへの導入
1. Unity 6（6000.0.42f1）で新規プロジェクトを作成
2. `Unity6_Fixed`フォルダの内容を`Assets/_Project/Scripts/`にコピー
3. Assembly定義ファイルを適切な場所に配置

### 2. 必要なパッケージのインストール
Package Managerから以下をインストール：
```json
{
  "dependencies": {
    "com.unity.inputsystem": "1.7.0+",
    "com.unity.cinemachine": "3.1",
    "com.unity.addressables": "2.0+",
    "com.unity.ui": "2.0+"
  }
}
```

### 3. ScriptableObjectイベントの作成
1. Project windowで右クリック
2. Create > Unity6 > Events から各種イベントを作成
3. PlayerControllerやStateMachineのInspectorでイベントを設定

## ✅ 修正により解決された問題

1. **名前空間の不一致** - 統一された構造に修正
2. **依存関係の問題** - 重複定義を削除し、適切な参照に修正
3. **StateMachine連携** - イベント経由での状態変更を実装
4. **GameManager関連** - 必要なイベント定義を追加
5. **メモリリーク** - 適切なリスナー解放処理を実装
6. **循環参照** - コンポーネント間の直接参照を排除

## 📝 注意事項

- すべてのコンポーネントは疎結合を維持
- ScriptableObjectイベントを介した通信
- Inspector上でイベントの接続が必要
- Unity 6の新機能（インクリメンタルGC、SRP Batcher等）に最適化

## 🔍 次のステップ

1. PlayerControllerとPlayerStateMachineの実装を完成させる
2. UIManagerやAudioManagerなどのシステムを追加
3. 最適化システム（ObjectPool、PerformanceManager）を実装
4. 実際のゲームロジックを実装