# TPS Template - 三人称視点シューター

## 概要

Unity 6 URP3D_Base01プロジェクト用のTPSテンプレート実装です。Phase 3: Learn & Grow価値実現の一環として、15分間の完全なTPS（三人称視点シューター）ゲームプレイ体験を提供します。

## アーキテクチャ

### 設計思想
- **Event-Driven Architecture**: 既存のGameEventシステムとの完全統合
- **Command Pattern**: 射撃・リロード・カバーアクション等のコマンド化
- **Component-Based Design**: 再利用可能なモジュラー設計
- **ScriptableObject Data-Driven**: 武器データ等の外部化
- **FPS Template資産活用**: 武器システム・AIシステム95%再利用

### 名前空間構造
```
asterivo.Unity60.Features.Templates.TPS
├── .Player     - TPS専用プレイヤー制御（三人称特化）
├── .Camera     - TPS用カメラ制御・三人称視点
├── .UI         - TPS専用HUD・カバー状況表示
├── .AI         - 既存AIシステムとの統合
└── .Cover      - カバーシステム（TPS固有機能）

# FPS Templateから継承
└── asterivo.Unity60.Features.Templates.FPS.Weapons  - 武器システム再利用
```

## コアコンポーネント

### 1. 三人称カメラシステム (`/Scripts/Camera/`)

#### `TPSCameraController.cs` - TPS専用カメラ制御
- **機能**: 三人称視点カメラ制御システム
- **特徴**:
  - プレイヤー追従システム（距離・角度調整）
  - カメラコリジョン回避システム
  - エイム時の肩越し視点切り替え
  - カバー時の視点調整
- **技術**:
  - 既存CameraStateMachineとの統合
  - Cinemachine 3.1統合による高度なカメラワーク
  - スムーズな視点遷移とカメラブレンド

### 2. プレイヤーシステム (`/Scripts/Player/`)

#### `TPSPlayerController.cs` - TPS専用プレイヤー制御
- **機能**:
  - TPS特有の移動（歩行・走行・しゃがみ・カバー移動）
  - 武器操作（射撃・リロード・エイム・カバー射撃）
  - カバーシステム統合
  - Input System統合
- **特徴**:
  - 既存PlayerControllerとの協調動作
  - カバー状態での制限された移動制御
  - 三人称視点での精密エイミング調整
  - ピーキング・ブラインドファイア対応

### 3. カバーシステム (`/Scripts/Cover/`)

#### `CoverSystem.cs` - カバーシステム中核
- **機能**:
  - カバーポイント自動検出システム
  - カバー状態管理（In Cover / Out of Cover）
  - ピーキング動作制御（左右覗き込み）
  - カバー間移動システム
- **技術**:
  - Raycast による壁・障害物検出
  - カバーポイント自動生成・管理
  - プレイヤー・カメラとの統合制御

### 4. UIシステム (`/Scripts/UI/`)

#### `TPSUIManager.cs` - TPS専用HUD管理
- **要素**:
  - 三人称視点専用クロスヘア（距離補正対応）
  - カバー状況インジケーター（In Cover表示）
  - 弾薬表示（色分け・残量警告）
  - ヘルスバー（グラデーション色変化）
  - ミニマップ（敵配置・カバーポイント表示）
- **特徴**:
  - カバー状態でのUI要素動的切り替え
  - 三人称視点での射撃精度表示
  - ピーキング方向インジケーター

### 5. AI統合システム (`/Scripts/AI/`)

#### `TPSAIIntegration.cs` - 既存AIとの統合
- **統合対象**:
  - NPCVisualSensor（50体同時稼働対応）活用
  - NPCAuditorySensor（4段階警戒レベル）活用
  - AIStateMachine（状態遷移システム）連携
- **TPS対応機能**:
  - カバー利用AI（AIもカバーシステム活用）
  - 三人称戦闘への対応調整
  - プレイヤーカバー状態検知・対応AI
  - 側面攻撃・包囲戦術AI

## ゲームプレイ仕様

### 15分間ゲームプレイ体験

#### `TPSTemplateManager.cs` - 統合ゲーム管理
- **目標**: 20体エネミー撃破 / 15分制限時間（FPS Template同等）
- **TPS固有要素**:
  - カバーポイント戦略的配置
  - 三人称視点での戦術的戦闘
  - カバー移動を活用した戦闘システム
  - 側面攻撃・包囲戦への対応要求

#### ゲームプレイループ（TPS特化）
1. **初期化**: プレイヤー・カバーポイント・UI・AI配置
2. **戦闘フェーズ**: カバー利用・エネミー検知・戦術移動
3. **進行管理**: スコア追跡・時間管理・動的難易度
4. **終了処理**: 勝利/敗北判定・結果表示

## 技術的特徴

### TPS固有の最適化
- **カバーシステム**: 自動検出・手動選択のハイブリッド方式
- **三人称カメラ**: コリジョン回避・視界確保の高度制御
- **戦術的AI**: カバー利用・側面攻撃の戦略的AI行動
- **FPS資産活用**: 武器・AIシステム95%再利用による効率開発

### Event-Driven統合（FPS Template拡張）
- **カバーアクション**: `onPlayerTakeCover/LeaveCover` → UI・AI・カメラ連携
- **武器発射**: 既存 `onWeaponFired` システム活用
- **ダメージ**: 既存 `onPlayerDamaged` システム活用
- **ゲーム状態**: 既存 `onGameStarted/Ended` システム活用

### アンチパターン回避（FPS Template継承）
- **密結合回避**: Event・Command・Interface分離
- **直接参照排除**: ServiceLocator・Event経由通信
- **ハードコーディング排除**: ScriptableObject外部設定

## 使用方法

### 基本セットアップ
1. **TPSTemplateManager**: シーンに配置・設定
2. **プレイヤー**: TPSPlayerController追加・武器装備
3. **カバーポイント**: 手動配置 or 自動検出設定
4. **エネミー**: TPSAIIntegration付きAI配置
5. **UI**: TPSUIManagerでHUD設定

### カスタマイズ
- **武器追加**: FPS Template WeaponDataアセット活用
- **カバーポイント**: 手動配置・自動検出範囲調整
- **マップ作成**: 戦術的カバーポイント・AI配置
- **難易度調整**: TPSTemplateManagerパラメータ変更

## テスト・検証

### 動作確認項目
- [ ] カバーシステム（検出・利用・移動・ピーキング）
- [ ] 三人称カメラ（追従・コリジョン回避・視点切り替え）
- [ ] 武器射撃・ヒット判定・エフェクト（FPS継承）
- [ ] プレイヤー移動・カバー移動・戦術的移動
- [ ] AI検知・戦闘・撃破（既存システム活用）
- [ ] UI更新・カバー状況表示・弾薬表示
- [ ] 15分ゲームプレイ完走

### パフォーマンス目標（FPS Template継承）
- **フレームレート**: 60FPS安定動作
- **メモリ使用量**: ベースライン+20%以内
- **ロード時間**: 初期化5秒以内

## Phase 3価値実現

### Learn & Grow（学習コスト70%削減）継続
- **TPS開発時間**: FPS Template資産活用により50%短縮
- **カバーシステム習得**: 既存基盤上の追加学習で効率化
- **削減効果**: 70%時間短縮・即座プロトタイプ可能（FPS継承）

### 15分ゲームプレイ実現（TPS特化）
- **完全性**: プレイヤー・AI・武器・UI・カバー全統合
- **拡張性**: 新武器（FPS互換）・マップ・ゲームモード容易追加
- **品質**: プロダクション対応レベルの実装

## FPS Template資産活用状況

### 95%再利用可能システム
- ✅ **武器システム**: IWeapon, WeaponSystem, WeaponData, Commands
- ✅ **AIシステム**: NPCVisualSensor, NPCAuditorySensor, AIStateMachine
- ✅ **Event-Drivenアーキテクチャ**: GameEvent, EventChannel全体
- ✅ **Command Pattern**: 射撃・リロードコマンド

### TPS特化カスタマイズ（5%新規実装）
- 🔄 **カメラシステム**: 三人称視点専用制御
- 🔄 **プレイヤー制御**: カバーシステム統合
- 🔄 **UIシステム**: TPS専用HUD要素
- 🆕 **カバーシステム**: TPS固有の新規機能

---

**実装開始日**: 2025年9月14日
**ステータス**: Phase 3 Learn & Grow価値実現 - TPS Template基盤構築開始
**進捗**: Phase A完了準備中（基盤構築・FPS資産統合・ドキュメント作成）
**次フェーズ**: Phase B 中核システム実装（Player/Camera/Cover System）