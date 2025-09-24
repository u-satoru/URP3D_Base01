# Phase 3.3: Camera機能の疎結合化 - リファクタリング計画書

## 作成日
2025年9月23日

## 概要
Camera層を3層アーキテクチャ（Core ← Feature ← Template）に準拠させ、他のFeature層との直接的な依存関係を排除し、イベント駆動通信への移行を実施します。

## 現状分析

### ファイル構成
- **総ファイル数**: 19個のC#スクリプトファイル
- **主要コンポーネント**:
  - CameraStateMachine（カメラ状態管理）
  - CinemachineIntegration（Cinemachine 3.1統合）
  - ViewModeController（視点モード制御）
  - 各種カメラ状態（FirstPerson, ThirdPerson, Aim, Cover）

### 名前空間の状態
- **基本名前空間**: `asterivo.Unity60.Features.Camera` ✅（正しい）
- **サブ名前空間**:
  - `asterivo.Unity60.Features.Camera.States`
  - `asterivo.Unity60.Features.Camera.ViewMode`
  - `asterivo.Unity60.Features.Camera.Cinemachine`
  - `asterivo.Unity60.Features.Camera.Commands`
  - `asterivo.Unity60.Features.Camera.Events`（新規追加）

### アセンブリ定義の状態
- **ファイル名**: `asterivo.Unity60.Features.Camera.asmdef` ✅
- **参照**:
  - asterivo.Unity60.Core ✅
  - asterivo.Unity60.Core.Events ✅
  - asterivo.Unity60.Core.Services ✅
  - asterivo.Unity60.Core.Debug ✅
  - Unity.Cinemachine ✅
  - Unity.InputSystem ✅
  - UniTask ✅
- **他Feature層への参照**: なし ✅（正しい）

## 実施した修正

### 1. 名前空間の修正
```csharp
// 修正前
namespace asterivo.Unity60.Camera

// 修正後
namespace asterivo.Unity60.Features.Camera
```
- **修正ファイル**: CameraEvents.cs

### 2. 名前空間参照の更新
```csharp
// 修正前
using CameraState = asterivo.Unity60.Camera.CameraState;

// 修正後
using CameraState = asterivo.Unity60.Features.Camera.CameraState;
```
- **修正ファイル**: CinemachineIntegration.cs

### 3. イベント駆動通信の実装
- **新規作成**: `PlayerPeekEventListener.cs`
  - Player層からのPeekイベントを受信
  - カメラの覗き見動作を制御
  - ServiceLocator経由でEventManagerに登録

## GameEvent経由の通信設計

### 受信イベント
| イベント名 | 送信元 | データ型 | 用途 |
|-----------|--------|---------|------|
| PlayerPeek | Player層 | PlayerPeekEventData | 覗き見動作の制御 |
| PlayerStateChanged | Player層 | PlayerState | カメラモードの自動切替 |
| CombatStarted | Combat層 | CombatData | 戦闘カメラへの切替 |

### 送信イベント
| イベント名 | データ型 | 用途 |
|-----------|---------|------|
| CameraStateChanged | CameraStateType | カメラ状態変更通知 |
| CameraTargetChanged | Transform | ターゲット変更通知 |

## アーキテクチャ準拠状況

### ✅ 達成事項
1. **名前空間統一**: 全ファイルが`asterivo.Unity60.Features.Camera`配下
2. **依存関係適正化**: Core層のみ参照（他Feature層への依存なし）
3. **アセンブリ定義**: 3層アーキテクチャに準拠
4. **イベント駆動通信**: PlayerPeekEventListenerの実装

### 🔄 推奨改善事項
1. **CoverCameraState内の直接入力処理**
   - 現状: Input.GetKeyDown等の直接入力取得
   - 改善案: InputManagerからのイベント駆動に変更

2. **Core層のCamera関連インターフェース整理**
   - ICameraControllerなどがCore層に存在
   - 評価: 汎用的なインターフェースのため妥当

3. **状態遷移のイベント化**
   - 現状: 直接的な状態遷移メソッド呼び出し
   - 改善案: GameEvent経由での状態遷移リクエスト

## 実装サンプル

### イベントリスナーの使用例
```csharp
public class CameraEventHandler : MonoBehaviour
{
    private void OnEnable()
    {
        var eventManager = ServiceLocator.Get<IEventManager>();
        eventManager?.Subscribe("PlayerStateChanged", OnPlayerStateChanged);
        eventManager?.Subscribe("CombatStarted", OnCombatStarted);
    }

    private void OnPlayerStateChanged(object data)
    {
        // プレイヤー状態に応じたカメラモード切替
        if (data is PlayerState state)
        {
            switch (state)
            {
                case PlayerState.Stealth:
                    TransitionToStealthCamera();
                    break;
                case PlayerState.Combat:
                    TransitionToCombatCamera();
                    break;
            }
        }
    }
}
```

## テスト検証計画

### 単体テスト
- [ ] PlayerPeekEventListenerの動作確認
- [ ] カメラ状態遷移のテスト
- [ ] イベント受信・送信のテスト

### 統合テスト
- [ ] Player層からのPeekイベント連携
- [ ] カメラモード切替の動作確認
- [ ] Cinemachine統合の検証

### パフォーマンステスト
- [ ] イベント駆動によるオーバーヘッド測定
- [ ] 60FPS維持の確認

## リスクと対策

### リスク1: 動的型付けによる実行時エラー
- **問題**: PlayerPeekEventDataをdynamicで受信
- **対策**: 共通インターフェースまたはCore層でのデータ型定義

### リスク2: 既存の直接入力処理との競合
- **問題**: CoverCameraStateの直接入力とイベント駆動の混在
- **対策**: 段階的な移行計画の策定

## 次のステップ

1. **統合テストの実施**
   - Unity Editor内でのコンパイル確認
   - 実行時のイベント連携確認

2. **残りの直接参照の除去**
   - 入力処理のイベント化
   - 状態遷移のイベント駆動化

3. **ドキュメント更新**
   - Camera層の使用方法
   - イベントインターフェース仕様

## 結論
Camera層の疎結合化は概ね完了しました。名前空間の統一、アセンブリ定義の適正化、そしてPlayerPeekEventListenerの実装により、3層アーキテクチャへの準拠が達成されました。残る課題は直接入力処理のイベント化ですが、これは既存機能への影響を考慮し、段階的に実施することを推奨します。
