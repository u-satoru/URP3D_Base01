# Architecture Policies (2025-09-11)

本ドキュメントは、Coreの運用ポリシーと実装変更点を短くまとめたものです。

## サービス取得ポリシー
- 優先手段: `ServiceLocator`
- フォールバック: Editor/Development ビルド限定で `FindFirstObjectByType`
- 実装: `Assets/_Project/Core/Helpers/ServiceHelper.cs`

## 初期化ライフサイクル標準化
- 登録対象は `IServiceLocatorRegistrable` を実装し `Priority` を付与
- `SystemInitializer` が起動時に Priority 順で Register、破棄時に逆順で Unregister
- 実装: `Assets/_Project/Core/Lifecycle/IServiceLocatorRegistrable.cs`, `Assets/_Project/Core/SystemInitializer.cs`

## アセンブリ依存方向
- 原則: `Core ← Features` の単方向
- Core から Features への参照を禁止（Editor 検証を追加）
- 実装: `Assets/_Project/Core/Editor/AsmdefDependencyValidator.cs`

## Features.backup の隔離
- `autoReferenced=false` に設定し、自動参照を停止
- パス: `Assets/_Project/Features/Features.backup/**.asmdef`

## FeatureFlags 運用
- 破損キー修正: `MigrateStealthAudioCoordinator`
- 起動時検証: `ValidateConfiguration()` + 非破壊の `EnforceConsistency(false)` を実行
- 追加API: `EnforceConsistency(bool autoFix)`（安全側に自動補正可能）
- 実装: `Assets/_Project/Core/FeatureFlags.cs`

## GameManager の責務分割に向けたIF化
- 導入IF: `IGameStateManager`, `ISceneLoadingService`, `IScoreService`, `IPauseService`
- 現状: GameManager が各IFを実装（非破壊）。段階的にクラス分割可能
- パス: `Assets/_Project/Core/Services/Interfaces/*.cs`

## 運用チェック
- エディタメニューで軽量スモークチェックと依存検証を実行
  - Tests: `Tools/Tests/Run Core Smoke Checks`
  - Asmdef: `Tools/Architecture/Validate Asmdef Dependencies`


