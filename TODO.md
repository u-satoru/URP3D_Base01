# TODO.md - 3層アーキテクチャ移行 実行タスク管理

## 📊 移行進捗ダッシュボード

**最終更新**: 2025年9月20日 15:30
**現在フェーズ**: Phase 1 準備フェーズ
**全体進捗**: 🟦⬜⬜⬜⬜⬜⬜⬜⬜⬜ 0% (0/13タスク完了)

### 今週の目標
- [ ] Phase 1 準備フェーズの完了（6タスク）
- [ ] Phase 2 Feature層整理の開始

## 🏗️ アーキテクチャ移行の目的
現在の2層構造（`Core` ← `Feature`）から、新たに `Template` 層を追加した3層アーキテクチャ（`Core` ← `Feature` ← `Template`）へ移行し、以下を実現：
- ✨ **再利用性の最大化**: Core/Feature層の他プロジェクトでの再利用
- 🚀 **プロトタイピング速度向上**: テンプレート複製による高速開発
- 👥 **非プログラマーとの連携強化**: Template層でのデザイナー作業領域確立

---

## 🔥 本日の優先タスク（2025/09/20）

### 🎯 最優先: Template層の基盤構築
```bash
# 実行時間: 約5分
# 以下のコマンドをPowerShell 7で実行
```

#### 1. ディレクトリ構造の作成 [推定: 5分]
```powershell
# Template層のディレクトリを一括作成
$templatePath = "D:\UnityProjects\URP3D_Base01\Assets\_Project\Features\Templates"
$genres = @("Common", "Stealth", "SurvivalHorror", "Platformer", "FPS", "TPS", "Adventure", "ActionRPG")

foreach ($genre in $genres) {
    New-Item -ItemType Directory -Force -Path "$templatePath\$genre"
    New-Item -ItemType Directory -Force -Path "$templatePath\$genre\Scenes"
    New-Item -ItemType Directory -Force -Path "$templatePath\$genre\Prefabs"
    New-Item -ItemType Directory -Force -Path "$templatePath\$genre\Data"
}
```
- [ ] コマンド実行
- [ ] ディレクトリ構造確認
- [ ] Unity上での認識確認

#### 2. Assembly Definition作成 [推定: 10分]
- [ ] `Assets/_Project/Features/Templates/` に以下のファイルを作成
  - ファイル名: `asterivo.Unity60.Features.Templates.asmdef`
- [ ] Migration_TASKS.mdのJSON内容を設定
- [ ] Unityエディタでコンパイルエラーがないことを確認
- [ ] 依存関係のテスト（Template→Feature→Core）

---

## 📅 今週の実行計画

### 月曜日（09/20） - Phase 1開始
- [x] 移行計画書の作成
- [x] TASKSファイルの作成
- [x] TODO.mdの作成
- [ ] **タスク1.1**: Template層ディレクトリ作成 ⏰15:45
- [ ] **タスク1.2**: Assembly Definition設定 ⏰16:00

### 火曜日（09/21） - 名前空間移行準備
- [ ] **タスク1.3.1**: 既存スクリプトの名前空間調査
  - [ ] Core層のスクリプト数カウント
  - [ ] Feature層のスクリプト数カウント
  - [ ] 影響範囲の特定
- [ ] 名前空間移行スクリプトの作成

### 水曜日（09/22） - 名前空間移行実行
- [ ] **タスク1.3.2**: 名前空間の一括変更
  - [ ] バックアップの作成
  - [ ] 自動置換の実行
  - [ ] コンパイルエラーの解消
- [ ] **タスク1.3.3**: asmdefのrootNamespace設定

### 木曜日（09/23） - Phase 2準備
- [ ] **タスク2.1**: ジャンル固有アセットの調査開始
  - [ ] ステルス関連アセットのリスト作成
  - [ ] 移動対象の特定

### 金曜日（09/24） - Phase 2実行
- [ ] **タスク2.1**: 移動計画の完成
- [ ] レビューとフィードバック

---

## 📝 実行中タスクの詳細

### 🚧 現在作業中
**タスク**: なし
**開始時刻**: -
**作業内容**: -
**ブロッカー**: なし

### ⏸️ 一時停止中
なし

### ⚠️ ブロック中のタスク
なし

---

## ✅ 完了タスク履歴

### 2025年9月20日
- [x] 3層アーキテクチャ移行計画書（Migration_Plan.md）作成
- [x] 移行タスク管理シート（Migration_TASKS.md）作成
- [x] TODO.md作成

---

## 📋 バックログ（優先度順）

### High Priority（今週中）
1. [ ] Phase 1: Template層の基盤構築
2. [ ] Phase 1: 名前空間の移行
3. [ ] Phase 2: アセット調査

### Medium Priority（来週）
1. [ ] Phase 3: Stealthテンプレート構築
2. [ ] Phase 3: 動作検証
3. [ ] ドキュメント更新

### Low Priority（再来週以降）
1. [ ] 他のジャンルテンプレート構築
2. [ ] パフォーマンステスト
3. [ ] 最終レビュー

---

## 🔍 名前空間移行チェックリスト

### 事前準備
- [ ] プロジェクト全体のバックアップ
- [ ] Gitブランチの作成（`feature/3-layer-architecture`）
- [ ] 現在の名前空間使用状況の調査

### Core層
- [ ] `_Project.Core` → `asterivo.Unity60.Core` への変更
- [ ] サブ名前空間の確認と変更
  - [ ] `.Services`
  - [ ] `.Events`
  - [ ] `.Commands`
  - [ ] `.Patterns`

### Feature層
- [ ] `_Project.Features` → `asterivo.Unity60.Features` への変更
- [ ] サブ名前空間の確認と変更
  - [ ] `.Player`
  - [ ] `.AI`
  - [ ] `.Camera`
  - [ ] `.Combat`

### Template層（新規）
- [ ] `asterivo.Unity60.Features.Templates` の設定
- [ ] ジャンル別サブ名前空間の設定
  - [ ] `.Stealth`
  - [ ] `.SurvivalHorror`
  - [ ] `.Platformer`
  - [ ] `.FPS`
  - [ ] `.TPS`
  - [ ] `.Adventure`
  - [ ] `.ActionRPG`

---

## 📊 メトリクス・KPI

| 指標 | 目標 | 現在値 | 状態 |
|------|------|--------|------|
| **Phase 1完了率** | 100% | 0% | 🔴 |
| **コンパイルエラー** | 0 | - | ⏸️ |
| **テストカバレッジ** | 80% | - | ⏸️ |
| **移行済みスクリプト** | 100% | 0% | 🔴 |

---

## 🚨 リスク管理

### 識別済みリスク
| リスク | 影響 | 対策 | 状態 |
|--------|------|------|------|
| 名前空間変更による参照切れ | 🔴高 | 段階的移行・自動テスト | 監視中 |
| Assembly定義の循環参照 | 🟡中 | 依存関係の事前確認 | 未発生 |
| 既存機能への影響 | 🔴高 | 包括的テスト実施 | 準備中 |

### 緊急時対応
```bash
# ロールバック手順
git stash
git checkout main
# または
git reset --hard HEAD~1
```

---

## 📝 作業メモ・注意事項

### ⚠️ 重要な注意点
1. **Unity Editorを開いたまま作業する**
   - コンパイルエラーを即座に検知
   - Assembly Definitionの影響を確認

2. **こまめなコミット**
   - 各タスク完了ごとにGitコミット
   - 問題発生時の切り分けを容易に

3. **テストの実行**
   - 名前空間変更後は必ずPlayModeテスト実行
   - ユニットテストの全実行

### 💡 Tips
- PowerShell 7を使用してディレクトリ作成を自動化
- Visual StudioやRiderの一括置換機能を活用
- Unity Test Runnerで継続的にテスト実行

---

## 🔗 関連ドキュメント

- [移行計画書](Assets/_Project/Docs/Works/20250920_1500/3-Layer_Architecture_Migration_Plan.md)
- [タスク管理シート](Assets/_Project/Docs/Works/20250920_1500/3-Layer_Architecture_Migration_TASKS.md)
- [SPEC.md](SPEC.md)
- [REQUIREMENTS.md](REQUIREMENTS.md)
- [DESIGN.md](DESIGN.md)

---

## 📞 エスカレーション

### 技術的な問題
- Assembly Definition関連 → Unity Forum確認
- 名前空間競合 → チームレビュー実施

### スケジュール調整
- Phase 1遅延 → Phase 2開始を調整
- リソース不足 → タスク優先度の再評価

---

**次回更新予定**: 2025年9月20日 17:00（タスク1.1, 1.2完了後）
