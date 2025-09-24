# Unity 6 テスト実行ガイド

## 概要
Unity 6 (6000.0.42f1) でのテスト実行に関する既知の問題と解決方法をまとめたガイドです。

## 重要な制約事項

### 🔴 Unity 6 の重要な仕様変更
**Unity 6 では `-runTests` と `-quit` フラグを同時に使用できません。**

この制約により、従来のバッチモードテスト実行コマンドは動作しません。

## 正しいテスト実行方法

### ✅ 推奨方法1: Unity Editor内での実行
```
Window > General > Test Runner > Run All
```
最も確実で、問題が発生しにくい方法です。

### ✅ 推奨方法2: バッチモード（-quit なし）
```powershell
# 正しいコマンド（-quit を使用しない）
& 'C:\Program Files\Unity\Hub\Editor\6000.0.42f1\Editor\Unity.exe' `
    -projectPath . `
    -batchmode `
    -runTests `
    -testPlatform EditMode `
    -testResults 'Tests\Results\test-results.xml' `
    -logFile 'Assets\_Project\Logs\test.log'
```

### ❌ 間違った方法（動作しません）
```powershell
# このコマンドは Unity 6 では動作しません
Unity.exe -projectPath . -batchmode -quit -runTests  # -quit と -runTests の併用は不可
```

## 既知の問題と対処法

### 問題1: 長時間実行テスト
**症状**: テストが永続的に実行され続ける
**原因**: `Migration_LongRunningStabilityTest_24HourSimulation` のような長時間シミュレーションテスト
**解決策**: フィルターで除外
```powershell
-testFilter '!Migration_LongRunningStabilityTest_24HourSimulation'
```

### 問題2: 複数Unity インスタンス
**症状**: 「別のUnityが起動中」エラー
**解決策**:
```powershell
# すべてのUnityプロセスを終了
Get-Process Unity -ErrorAction SilentlyContinue | Stop-Process -Force
```

### 問題3: テスト結果XMLが生成されない
**症状**: テスト実行後もXMLファイルが作成されない
**原因**: `-quit` フラグによる早期終了
**解決策**: `-quit` フラグを削除

## 実用的なテスト実行例

### Core層のみテスト
```powershell
Unity.exe -projectPath . -batchmode -runTests `
    -testPlatform EditMode `
    -testFilter 'asterivo.Unity60.Core' `
    -testResults 'core-tests.xml'
```

### 特定のテストクラスのみ実行
```powershell
Unity.exe -projectPath . -batchmode -runTests `
    -testPlatform EditMode `
    -testFilter 'asterivo.Unity60.Tests.SimpleTest' `
    -testResults 'simple-tests.xml'
```

## テストアセンブリ定義の確認事項

EditMode テストを実行する場合、`.asmdef` ファイルに以下の設定が必要です：

```json
{
    "name": "asterivo.Unity60.Tests",
    "includePlatforms": [
        "Editor"  // この設定が必須
    ],
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ]
}
```

## フィルター指定の重要な注意点

### ⚠️ 名前空間とクラス名の正確な指定が必須
フィルターはテストクラスの**実際の完全修飾名**と一致する必要があります。

#### 例：名前空間なしのクラス
```csharp
// SimpleTest.cs - 名前空間なし
[TestFixture]
public class SimpleTest { ... }
```
**正しいフィルター**: `-testFilter 'SimpleTest'`
**間違ったフィルター**: `-testFilter 'asterivo.Unity60.Tests.SimpleTest'` ❌

#### 例：名前空間ありのクラス
```csharp
namespace asterivo.Unity60.Tests
{
    [TestFixture]
    public class CoreTests { ... }
}
```
**正しいフィルター**: `-testFilter 'asterivo.Unity60.Tests.CoreTests'`

### フィルター検証方法
1. テストクラスのソースコードで名前空間を確認
2. クラス名を正確に指定
3. XMLの `testcasecount` で実行数を確認

## トラブルシューティング

### Q: テストが見つからない（testcasecount="0"）
A: 以下を確認してください：
1. フィルターがクラスの実際の名前と一致しているか
2. 名前空間の有無を正しく反映しているか
3. アセンブリ定義の `includePlatforms` に "Editor" が含まれているか

### Q: テストが途中で止まる
A: 長時間実行テストを除外するフィルターを使用してください。

### Q: プロセスが終了しない
A: `-quit` フラグを削除し、手動でプロセスを終了してください。

## 参考資料

- [詳細な調査報告書](../Logs/batch-test-investigation-report.md)
- [Unity Test Framework 公式ドキュメント](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)

---
最終更新: 2025-09-22
Unity Version: 6000.0.42f1