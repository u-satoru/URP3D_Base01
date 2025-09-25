# テストカバレッジ測定 実装ガイド

## 作成日: 2025年9月22日

### エグゼクティブサマリー
Unity 6でのテストカバレッジ測定について調査を実施し、実装可能な方法を特定しました。Unity Code Coverageパッケージ v1.2.7が利用可能で、バッチモードでの測定も可能です（Unity 6の制限事項を考慮）。

---

## 現在の環境

### インストール済みパッケージ
- **Unity Test Framework**: v1.4.6
- **Unity Code Coverage**: v1.2.7（追加済み）
- **Unity Version**: 6000.0.42f1

### Unity 6の重要な制限
⚠️ **`-quit`フラグと`-runTests`の非互換性**
- Unity 6では同時使用不可
- カバレッジ測定時も同様の制限適用

---

## 実装方法

### 方法1: Unity Editor内での実行（推奨）

#### 手順
1. **Window > Analysis > Code Coverage**を開く
2. **Enable Code Coverage**をチェック
3. **Window > General > Test Runner**でテスト実行
4. カバレッジレポートが自動生成される

#### メリット
- 最も確実で簡単
- GUIで結果を即座に確認可能
- エラーが発生しにくい

### 方法2: バッチモードでの実行（CI/CD向け）

#### EditModeテストのカバレッジ測定
```powershell
# Unity 6対応版（-quitなし）
& 'C:\Program Files\Unity\Hub\Editor\6000.0.42f1\Editor\Unity.exe' `
    -projectPath . `
    -batchmode `
    -runTests `
    -testPlatform EditMode `
    -enableCodeCoverage `
    -coverageResultsPath "Tests\Coverage\EditMode" `
    -coverageOptions "generateAdditionalMetrics;assemblyFilters:+asterivo.Unity60.Core.*,+asterivo.Unity60.Features.*" `
    -testResults "Tests\Results\test-results-coverage.xml" `
    -logFile "Assets\_Project\Logs\coverage-test.log"
```

#### PlayModeテストのカバレッジ測定
```powershell
# PlayModeテスト用（必要な場合）
& 'C:\Program Files\Unity\Hub\Editor\6000.0.42f1\Editor\Unity.exe' `
    -projectPath . `
    -batchmode `
    -runTests `
    -testPlatform PlayMode `
    -enableCodeCoverage `
    -coverageResultsPath "Tests\Coverage\PlayMode" `
    -coverageOptions "generateAdditionalMetrics;assemblyFilters:+asterivo.Unity60.Core.*,+asterivo.Unity60.Features.*" `
    -testResults "Tests\Results\playmode-results-coverage.xml" `
    -logFile "Assets\_Project\Logs\playmode-coverage.log"
```

### 方法3: 統合カバレッジレポート生成

```powershell
# 複数のテスト実行結果を統合
& 'C:\Program Files\Unity\Hub\Editor\6000.0.42f1\Editor\Unity.exe' `
    -projectPath . `
    -batchmode `
    -enableCodeCoverage `
    -coverageResultsPath "Tests\Coverage\Combined" `
    -generateCoverageReport `
    -quit  # レポート生成のみの場合は-quit使用可能
```

---

## カバレッジオプション詳細

### assemblyFilters
```
# Core層とFeature層のみ測定
assemblyFilters:+asterivo.Unity60.Core.*,+asterivo.Unity60.Features.*

# Template層を除外
assemblyFilters:+asterivo.Unity60.*,-asterivo.Unity60.Features.Templates.*
```

### pathFilters
```
# 特定パスのみ対象
pathFilters:+Assets/_Project/Core/**,+Assets/_Project/Features/**
```

### 追加メトリクス
- **generateAdditionalMetrics**: Cyclomatic ComplexityとCrap Score計算
- **generateHtmlReport**: HTML形式のレポート生成
- **generateBadgeReport**: カバレッジバッジ生成

---

## 期待される出力

### ディレクトリ構造
```
Tests/
├── Coverage/
│   ├── EditMode/
│   │   ├── Report/
│   │   │   ├── index.html
│   │   │   └── Summary.xml
│   │   └── TestCoverageResults_*.xml
│   ├── PlayMode/
│   └── Combined/
└── Results/
    └── test-results-coverage.xml
```

### レポート内容
1. **Line Coverage**: 実行された行の割合
2. **Branch Coverage**: 実行された分岐の割合
3. **Method Coverage**: 実行されたメソッドの割合
4. **Cyclomatic Complexity**: コードの複雑度
5. **Crap Score**: 変更リスクの指標

---

## 現在の制限事項と回避策

### 問題1: Unity 6の-quit制限
**回避策**:
- テスト実行時は-quitを使用しない
- レポート生成のみの場合は-quit使用可能
- プロセスは手動またはタイムアウトで終了

### 問題2: 長時間実行テスト
**回避策**:
```powershell
-testFilter "!Migration_LongRunningStabilityTest_24HourSimulation"
```

### 問題3: バッチモードでのHTML生成
**回避策**:
- Unity Editor内で生成を推奨
- または別途レポート生成コマンドを実行

---

## 推奨実装計画

### Phase 1: Editor内での手動実行（即実施可能）
1. Unity Editorを開く
2. Code Coverageウィンドウを設定
3. Test Runnerでテスト実行
4. レポート確認

### Phase 2: バッチモード自動化（将来対応）
1. 上記コマンドをスクリプト化
2. CI/CDパイプラインに統合
3. 定期実行スケジュール設定

### Phase 3: 品質ゲート設定（オプション）
1. カバレッジ閾値設定（例: 80%以上）
2. ビルド失敗条件の定義
3. レポート自動通知

---

## 現実的な提案

### 当面の対応
- **Unity Editor内での手動測定を推奨**
- バッチモードは制限が多いため、必要最小限の使用に留める
- HTMLレポートはEditor内で生成し、共有

### 測定対象
```
優先度高：
- asterivo.Unity60.Core.Services
- asterivo.Unity60.Core.Commands
- asterivo.Unity60.Core.Events

優先度中：
- asterivo.Unity60.Features.Player
- asterivo.Unity60.Features.AI

優先度低：
- Template層（設定中心のため）
- Editor拡張（ツール系）
```

---

## まとめ

Unity 6でのコードカバレッジ測定は可能ですが、以下の点に注意が必要：

1. **-quitフラグの制限により、完全自動化は困難**
2. **Unity Editor内での実行が最も確実**
3. **バッチモードは限定的に使用**
4. **現在80%のカバレッジ目標は、Editor内での測定で確認**

これらの制限を理解した上で、プロジェクトの要件に応じた測定方法を選択することを推奨します。

---

**作成者**: Claude Code Assistant
**レビュー待ち**: プロジェクトマネージャー
