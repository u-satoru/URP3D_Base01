# TODO.md - Phase 1 実装タスク管理

## 文書管理情報

- **ドキュメント種別**: 進行中タスク管理
- **生成元**: TASKS.md Phase 1（並行実行可能タスク）
- **対象読者**: 開発者、実装担当者
- **更新日**: 2025年9月
- **ステータス**: Phase 1 実行中

## 🎯 Phase 1 目標

**最短5-7日でステルスゲーム中核機能完成**

- ✅ **並行実行**: TASK-001 + TASK-002 を同時進行
- 🎮 **成果物**: ステルスゲーム基本動作の実現
- 📊 **成功指標**: Alpha Release レベル到達

---

## 🚀 TASK-001: NPCVisualSensor System 完全実装

**優先度**: ⚡ Critical | **工数**: 3-4日 | **状況**: ❌ 未着手

### 📋 実装チェックリスト

#### Phase 1.1: 基盤システム構築（Day 1）
- [ ] **TASK-001.1**: NPCVisualSensor.cs 基底クラス実装
  - [ ] MonoBehaviour基底クラス作成
  - [ ] 継続的視界スキャンシステム（10-20Hz可変頻度）
  - [ ] Update()での効率的スキャン処理
  - [ ] ICoroutine による分散処理実装
  - [ ] 基本的なデバッグログ出力

- [ ] **TASK-001.2**: VisualDetectionModule 実装
  - [ ] 多重判定システム（距離・角度・遮蔽・光量）
  - [ ] VisibilityCalculator との統合
  - [ ] 閾値ベース判定ロジック
  - [ ] 検出スコア計算アルゴリズム

#### Phase 1.2: 警戒・記憶システム（Day 2）
- [ ] **TASK-001.3**: AlertSystemModule 実装
  - [ ] 4段階警戒レベル（Relaxed → Suspicious → Investigating → Alert）
  - [ ] 警戒レベル自動遷移制御
  - [ ] AI State Machine との連動
  - [ ] 警戒レベル変更イベント発行

- [ ] **TASK-001.4**: MemoryModule 実装
  - [ ] 短期記憶（5秒）→長期記憶（30秒）階層管理
  - [ ] 位置履歴管理システム
  - [ ] 目標の予測位置計算
  - [ ] 記憶データ構造定義

#### Phase 1.3: 追跡・設定システム（Day 3）
- [ ] **TASK-001.5**: TargetTrackingModule 実装
  - [ ] 複数目標同時追跡（最大5目標）
  - [ ] 優先度管理システム
  - [ ] DetectedTarget データ構造実装
  - [ ] 目標リスト管理機能

- [ ] **TASK-001.6**: Configuration System 実装
  - [ ] VisualSensorSettings (ScriptableObject)
  - [ ] DetectionConfiguration (ScriptableObject)
  - [ ] Inspector UI カスタマイズ
  - [ ] デフォルト設定値定義

#### Phase 1.4: 最適化・統合（Day 4）
- [ ] **TASK-001.7**: Performance Optimization 実装
  - [ ] LOD対応による動的最適化
  - [ ] フレーム分散処理システム
  - [ ] 早期カリング機能実装
  - [ ] メモリプール活用

- [ ] **TASK-001.8**: Event Integration 実装
  - [ ] onTargetSpotted, onTargetLost イベント
  - [ ] onAlertLevelChanged, onSuspiciousActivity イベント
  - [ ] Event-Driven Architecture との統合
  - [ ] イベント発行タイミング最適化

- [ ] **TASK-001.9**: Debug Tools 実装
  - [ ] Scene View Gizmos 描画
  - [ ] カスタムInspector ウィンドウ
  - [ ] リアルタイムデバッグ情報表示
  - [ ] デバッグ設定UI

- [ ] **TASK-001.10**: Testing & Validation
  - [ ] パフォーマンステスト（0.1ms/frame以下）
  - [ ] 50体NPC同時稼働テスト
  - [ ] メモリ使用量検証（5KB/NPC以下）
  - [ ] 統合テストシナリオ実行

### ✅ 完了条件
- [ ] 視覚検出と警戒レベル遷移が正常動作
- [ ] パフォーマンス要件達成（1フレーム0.1ms以下）
- [ ] 50体NPC同時稼働で正常動作
- [ ] AIStateMachine との完全統合

---

## 🎮 TASK-002: PlayerStateMachine 復元・完全実装

**優先度**: ⚡ Critical | **工数**: 2-3日 | **状況**: ❌ 未着手

### 📋 実装チェックリスト

#### Phase 2.1: 基盤復元（Day 1）
- [ ] **TASK-002.1**: PlayerStateMachine.cs 復元
  - [ ] 空ファイル状態からの完全実装
  - [ ] Dictionary<PlayerStateType, IPlayerState> 高速管理
  - [ ] 既存 IPlayerState インターフェースとの統合
  - [ ] 基本的な状態遷移フレームワーク

- [ ] **TASK-002.2**: State Management System 実装
  - [ ] BasePlayerState 基底クラスとの統合
  - [ ] 状態遷移ルールの定義と実装
  - [ ] Enter/Update/Exit ライフサイクル管理
  - [ ] 状態遷移ログ出力

#### Phase 2.2: 状態統合（Day 2）
- [ ] **TASK-002.3**: 基盤実装済み状態の統合
  - [ ] IdleState 統合・動作確認
  - [ ] WalkingState 統合・動作確認
  - [ ] RunningState 統合・動作確認
  - [ ] CrouchingState 統合・動作確認
  - [ ] ProneState 統合・動作確認
  - [ ] JumpingState 統合・動作確認
  - [ ] RollingState 統合・動作確認
  - [ ] CoverState 統合・動作確認

#### Phase 2.3: システム統合（Day 3）
- [ ] **TASK-002.4**: System Integration 実装
  - [ ] Camera System との状態同期
  - [ ] Audio System との状態連動
  - [ ] Event-Driven Architecture との統合
  - [ ] 状態変更イベント発行

- [ ] **TASK-002.5**: Input Integration 実装
  - [ ] Input System との統合
  - [ ] 状態別入力処理の実装
  - [ ] 状態遷移トリガーの実装
  - [ ] 入力バッファリング対応

- [ ] **TASK-002.6**: Animation Integration 実装
  - [ ] Animator Controller との統合
  - [ ] 状態別アニメーション制御
  - [ ] アニメーションイベントとの連動
  - [ ] アニメーション同期処理

- [ ] **TASK-002.7**: Physics Integration 実装
  - [ ] 物理演算との状態同期
  - [ ] コライダー制御（Crouch, Prone時）
  - [ ] 重力・移動速度制御
  - [ ] 物理フィードバック処理

### ✅ 完了条件
- [ ] プレイヤー状態遷移が正常動作
- [ ] 各状態の物理・音響特性が実装
- [ ] 他システムとの状態連動が動作

---

## 📊 Phase 1 進捗管理

### Daily Stand-up チェック項目
- [ ] **Day 1**: TASK-001.1, TASK-001.2 + TASK-002.1 完了
- [ ] **Day 2**: TASK-001.3, TASK-001.4 + TASK-002.2 完了
- [ ] **Day 3**: TASK-001.5, TASK-001.6 + TASK-002.3 完了
- [ ] **Day 4**: TASK-001.7, TASK-001.8 完了
- [ ] **Day 5**: TASK-001.9, TASK-001.10 + TASK-002.4～2.7 完了

### 🚨 ブロッカー管理
- [ ] **技術的ブロッカー**: なし
- [ ] **依存関係ブロッカー**: なし
- [ ] **リソースブロッカー**: なし

### 🎯 マイルストーン確認
- [ ] **Alpha Release 条件**:
  - [ ] NPCがプレイヤーを視覚的に検出できる
  - [ ] プレイヤーが基本8状態で動作する
  - [ ] 警戒レベルに応じてAI反応が変化する
  - [ ] パフォーマンス要件を満たす
  - [ ] 基本的なステルスゲームプレイが成立する

---

## 🛠️ 開発環境・ツール

### 必要ツール
- [ ] Unity 6000.0.42f1
- [ ] Visual Studio 2022 / VSCode
- [ ] Git バージョン管理
- [ ] Unity Profiler（パフォーマンステスト用）

### デバッグ設定
- [ ] Debug.Log有効化
- [ ] Scene View でのGizmos表示
- [ ] プロファイラー統計取得
- [ ] フレームレート監視

### テストシナリオ
- [ ] 1体NPCでの基本検出テスト
- [ ] 10体NPCでの同時動作テスト
- [ ] 50体NPCでのストレステスト
- [ ] プレイヤー全状態遷移テスト

---

## 🔄 Phase 1 完了後のネクストアクション

### Phase 2 準備
- [ ] TASK-003 (Setup Wizard) の詳細設計
- [ ] Template Systems の現状調査
- [ ] 環境診断要件の整理

### アルファ版デモ準備
- [ ] デモシーン作成
- [ ] 基本操作説明書
- [ ] パフォーマンス測定レポート

---

**🎯 Phase 1 Success Criteria**: 並行実行により最短5日でステルスゲーム基本機能が完全動作し、Alpha Release品質に到達すること。

*このTODO.mdはTASKS.md Phase 1の実装管理用です。Daily stand-upでの進捗確認と課題管理に使用してください。*