# Phase 3.1: Player機能の疎結合化 リファクタリング計画書

## 作成日
2025年9月23日

## 概要
Player層を3層アーキテクチャに準拠させ、他のFeature層との疎結合化を実現するためのリファクタリング計画。

## 完了済みタスク

### ✅ タスク 3.1.1: Player関連アセットの現状確認とマッピング
- **完了日時**: 2025/09/23
- **実施内容**:
  - Player層のファイル構造を完全把握
  - 58個のPlayer関連ファイルを特定
  - 構造: Features/Player配下とTemplates配下に分散

### ✅ タスク 3.1.2: Player層の名前空間統一
- **完了日時**: 2025/09/23
- **実施内容**:
  1. 不統一な名前空間を修正:
     - `asterivo.Unity60.Player` → `asterivo.Unity60.Features.Player`
     - `asterivo.Unity60.Features.Player.Scripts` → `asterivo.Unity60.Features.Player`
  2. アセンブリ定義ファイルの修正:
     - `asterivo.Unity60.Player.asmdef` → `asterivo.Unity60.Features.Player.asmdef`に名前変更
     - rootNamespaceも統一

### ✅ タスク 3.1.3: 外部依存関係の洗い出しとリファクタリング計画
- **完了日時**: 2025/09/23
- **実施内容**:
  - Camera層への直接参照を削除（アセンブリ定義から削除済み）
  - 他のFeature層への直接参照がないことを確認
  - GameEvent経由での通信準備の確認

## 発見された問題と対策

### 1. Camera層への依存関係
**問題**:
- アセンブリ定義に `asterivo.Unity60.Camera` への参照があった
- `PeekCommand.cs` にカメラ調整の実装が必要

**対策**:
- アセンブリ定義から Camera層への参照を削除（✅完了）
- GameEventを使った疎結合な実装に変更（予定）

### 2. 名前空間の不統一
**問題**:
- 一部ファイルで `asterivo.Unity60.Player` を使用
- アセンブリ定義の名前も不統一

**対策**:
- すべて `asterivo.Unity60.Features.Player` に統一（✅完了）

## リファクタリング実行計画

### Phase A: GameEvent定義の作成
1. **PlayerPeekEvent.asset** の作成
   - 覗き見動作をCameraシステムに通知
   - データ: PeekDirection, PeekIntensity

2. **PlayerStateChangeEvent.asset** の作成
   - プレイヤー状態変更を他システムに通知
   - データ: OldState, NewState

3. **PlayerMovementEvent.asset** の作成
   - プレイヤー移動を他システムに通知
   - データ: Position, Velocity, IsGrounded

### Phase B: PeekCommandのリファクタリング
```csharp
// 現在のコード（直接的なカメラ制御）
private void AdjustCameraForPeek()
{
    // 実際の実装では、CameraStateMachineやCinemachineとの連携が必要
}

// 改善後（GameEvent経由）
private void AdjustCameraForPeek()
{
    var peekData = new PeekEventData
    {
        Direction = _definition.peekDirection,
        Intensity = _definition.peekIntensity,
        PlayerPosition = _stateMachine.transform.position
    };

    playerPeekEvent?.Raise(peekData);
}
```

### Phase C: PlayerController統合
1. GameEventListenerの追加
2. ServiceLocator経由でのサービス取得
3. 直接参照の排除

## 実装優先順位

1. **P0 - 最優先**: GameEvent定義の作成
2. **P1 - 高優先**: PeekCommandのリファクタリング
3. **P2 - 中優先**: 他のコマンドクラスの確認と修正
4. **P3 - 低優先**: テストコードの更新

## 成功基準

### 必須要件
- [ ] Player層が他のFeature層を直接参照していない
- [ ] すべての層間通信がGameEvent経由
- [ ] 名前空間が完全に統一されている
- [ ] アセンブリ定義が3層アーキテクチャに準拠

### 品質基準
- [ ] コンパイルエラーゼロ
- [ ] 既存機能の動作保証
- [ ] パフォーマンス劣化なし
- [ ] テストカバレッジ維持

## 想定作業時間
- GameEvent定義作成: 30分
- コード修正: 1時間
- テスト・検証: 30分
- **合計**: 約2時間

## リスクと対策

### リスク1: GameEvent経由による遅延
**対策**: 重要な処理は同期的に実行し、非重要な処理のみ非同期化

### リスク2: 既存機能への影響
**対策**: feature/3-layer-architecture-migrationブランチでの作業継続

## 次のステップ

1. GameEvent定義ファイルの作成
2. PeekCommandのリファクタリング実施
3. 動作確認とテスト実行
4. Phase 3.2（AI機能の疎結合化）へ移行

## 参照ドキュメント
- `Assets/_Project/Docs/Works/20250922_1015/3-Layer-Architecture-Migration-Detailed-Tasks.md`
- `CLAUDE.md`
- `TODO.md`
