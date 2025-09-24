# Phase 3.2: AI機能の疎結合化 リファクタリング計画書

## 作成日
2025年9月23日

## 概要
AI層を3層アーキテクチャに準拠させ、他のFeature層との疎結合化を実現するためのリファクタリング計画。

## 完了済みタスク

### ✅ タスク 3.2.1: AI関連アセットの現状確認とマッピング
- **完了日時**: 2025/09/23
- **実施内容**:
  - AI層のファイル構造を完全把握
  - 27個のAI関連ファイルを特定
  - 構造: Features/AI配下に統一されている

### ✅ タスク 3.2.2: AI層の名前空間統一
- **完了日時**: 2025/09/23
- **実施内容**:
  - 名前空間は既に`asterivo.Unity60.Features.AI.*`に統一されている
  - アセンブリ定義ファイルの修正:
    - `asterivo.Unity60.AI.asmdef` → `asterivo.Unity60.Features.AI.asmdef`に名前変更

### ✅ タスク 3.2.3: 外部依存関係の洗い出し
- **完了日時**: 2025/09/23
- **実施内容**:
  - Stealth層への不適切な依存を発見
  - 具体的な問題箇所:
    - `asterivo.Unity60.Stealth.Detection`への参照（3ファイル）
    - `DetectionConfiguration`クラスへの依存

## 発見された問題と対策

### 1. Stealth層への依存関係
**問題**:
- アセンブリ定義に `asterivo.Unity60.Stealth` への参照があった
- 以下のファイルでStealth層のクラスを使用:
  - `NPCVisualSensor.cs`
  - `VisualDetectionModule.cs`
  - `VisualSensorSettings.cs`

**対策**:
- アセンブリ定義からStealth層への参照を削除（✅完了）
- AI層独自のDetectionConfigurationを作成（予定）
- GameEventを使った疎結合な実装に変更（予定）

### 2. DetectionConfigurationの配置問題
**問題**:
- AI機能に必要な`DetectionConfiguration`がStealth層に配置されている
- これは3層アーキテクチャ違反（Feature層同士の依存）

**対策**:
- AI層に独自の`AIDetectionConfiguration`を作成
- Stealth層への依存を完全に排除

## リファクタリング実行計画

### Phase A: AI層独自のDetectionConfiguration作成
1. **AIDetectionConfiguration.cs** の作成
   - AI層独自の検出パラメータ定義
   - ScriptableObjectとして実装
   - パス: `Assets/_Project/Features/AI/Configuration/`

2. **既存コードの修正**
   - `NPCVisualSensor.cs`の修正
   - `VisualDetectionModule.cs`の修正
   - `VisualSensorSettings.cs`の修正

### Phase B: GameEvent定義の作成
1. **AIDetectionEvent.asset** の作成
   - AI検出イベントを他システムに通知
   - データ: DetectionLevel, TargetPosition, AlertType

2. **AIStateChangeEvent.asset** の作成
   - AI状態変更を他システムに通知
   - データ: OldState, NewState, Reason

3. **AITargetLostEvent.asset** の作成
   - ターゲット喪失を通知
   - データ: LastKnownPosition, TimeLost

### Phase C: 他システムとの連携改善
1. **PlayerシステムとのGameEvent連携**
   - Player検出時のイベント発行
   - Player状態変更の受信

2. **AudioシステムとのGameEvent連携**
   - 音響検出イベントの受信
   - AI行動音の発行

## 実装優先順位

1. **P0 - 最優先**: AIDetectionConfiguration作成とStealth依存の削除
2. **P1 - 高優先**: GameEvent定義の作成
3. **P2 - 中優先**: 既存コードのリファクタリング
4. **P3 - 低優先**: テストコードの更新

## 成功基準

### 必須要件
- [ ] AI層がStealth層を含む他のFeature層を直接参照していない
- [ ] すべての層間通信がGameEvent経由
- [ ] 名前空間が完全に統一されている
- [ ] アセンブリ定義が3層アーキテクチャに準拠

### 品質基準
- [ ] コンパイルエラーゼロ
- [ ] 既存機能の動作保証
- [ ] パフォーマンス劣化なし
- [ ] AIの検出機能が正常動作

## 想定作業時間
- AIDetectionConfiguration作成: 30分
- コード修正: 1.5時間
- テスト・検証: 30分
- **合計**: 約2.5時間

## リスクと対策

### リスク1: DetectionConfiguration移行による互換性問題
**対策**: 既存のパラメータ値を維持し、段階的に移行

### リスク2: 複数システムでの重複定義
**対策**: Core層への将来的な統合を検討

### リスク3: GameEvent経由による検出遅延
**対策**: クリティカルな検出処理は同期的に実行

## 次のステップ

1. AIDetectionConfiguration.csの作成
2. Stealth層参照の削除とコード修正
3. GameEvent定義の作成
4. 動作確認とテスト実行
5. Phase 3.3（Camera機能の疎結合化）へ移行

## AI層の構造（リファクタリング後）

```
Assets/_Project/Features/AI/
├── Configuration/          # AI設定データ（新規作成）
│   └── AIDetectionConfiguration.cs
├── Events/                 # AIイベント定義（新規作成）
│   ├── AIDetectionEventData.cs
│   └── AIStateChangeEventData.cs
├── Scripts/
│   ├── States/            # AI状態管理
│   └── NPCMultiSensorDetector.cs
├── Visual/                # 視覚センサー
│   ├── NPCVisualSensor.cs
│   └── VisualDetectionModule.cs
├── Audio/                 # 聴覚センサー
│   └── NPCAuditorySensor.cs
└── StateMachine/          # 階層化ステートマシン
    └── HierarchicalAIStateMachine.cs
```

## 参照ドキュメント
- `Assets/_Project/Docs/Works/20250922_1015/3-Layer-Architecture-Migration-Detailed-Tasks.md`
- `Assets/_Project/Docs/Works/20250923_Phase3_Player_Refactoring_Plan.md`
- `CLAUDE.md`
- `TODO.md`