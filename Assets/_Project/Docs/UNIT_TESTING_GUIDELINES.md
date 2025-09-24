# ユニットテスト実装ガイドライン (Unit Testing Guidelines)

## 1. はじめに

このドキュメントは、`URP3D_Base01`プロジェクトにおけるユニットテストの実装方針、役割、および実行方法を定義します。品質の高いソフトウェアを継続的に提供するため、すべての開発者はこのガイドラインを遵守してください。

## 2. 基本原則

- **テスト可能な設計**: すべてのコードは、テストのしやすさを念頭に置いて設計されるべきです。`Core`層のインターフェース設計ガイドラインに従い、依存関係を疎に保つことが基本となります。
- **網羅性**: `Core`層のすべてのロジックと、`Feature`層の主要な機能は、ユニットテストによってカバーされている必要があります。
- **独立性**: 各テストケースは、他のテストケースから独立しており、実行順序に依存してはなりません。
- **再現性**: テストは、誰が、いつ、どこで実行しても、同じ結果を再現できなければなりません。

## 3. テストの「種類」：Edit Mode vs Play Mode

テストコードを記述する際は、その目的応じて適切な「種類」を選択する必要があります。

### 3.1. エディットモードテスト (Edit Mode Tests)

-   **役割**:
    Unityの実行環境（シーン再生）に依存しない、**純粋なC#のロジック**を検証します。プロジェクトの基盤となるロジックの安定性を保証する、最も重要なテストです。
-   **テスト対象**:
    -   **`Core`層のほぼ全てのクラス**: `ServiceLocator`, `Command`, `ObjectPool`, 計算ロジックなど。
    -   **`Feature`層の非`MonoBehaviour`クラス**: Unityのライフサイクルに依存しないビジネスロジック。
-   **実装場所**:
    `Assets/_Project/Tests/Core/Editor/`
    `Assets/_Project/Tests/Features/{FeatureName}/Editor/`
-   **必要性**: **必須**。`Core`層の新規クラスや`public`メソッドには、原則として対応するエディットモードテストを実装する必要があります。

### 3.2. プレイモードテスト (Play Mode Tests)

-   **役割**:
    実際にゲームを再生した状態で、**Unityのコンポーネントやライフサイクル（`Update`, `Coroutine`など）と連携する機能**を検証します。システム間の連携や、実際のゲームプレイに近い状況での動作を保証します。
-   **テスト対象**:
    -   **`Feature`層の`MonoBehaviour`クラス**: `PlayerController`の入力応答、`Interactor`のRaycast検知など。
    -   **システム間連携**: `GameEvent`の発行からUIの更新まで、複数のシステムをまたぐ一連のフロー。
    -   物理演算、アニメーション、時間経過を伴う処理。
-   **実装場所**:
    `Assets/_Project/Tests/Core/Runtime/`
    `Assets/_Project/Tests/Features/{FeatureName}/Runtime/`
-   **必要性**: **重要**。主要なゲームプレイ機能や、複数のシステムが連携する複雑な機能には、プレイモードテストを実装する必要があります。

## 4. テストの「実行方法」：Editor vs Batch Mode

作成されたユニットテストは、以下の2つの方法で実行可能でなければなりません。**テストコードは、どちらの実行方法でも同じ結果になるように実装する必要があります。**

### 4.1. 方法A: Unity Editor上での手動実行

-   **目的**:
    開発者が開発サイクルの中で、迅速にテストを実行し、フィードバックを得るための方法です。デバッグや、特定の機能に対するテストの実行に用います。
-   **実行手順**:
    1.  Unity Editorの上部メニューから `Window > General > Test Runner` を選択します。
    2.  `Test Runner`ウィンドウを開きます。
    3.  `PlayMode`タブまたは`EditMode`タブを選択します。
    4.  `Run All`ボタン、または個別のテストケースを選択して`Run Selected`ボタンをクリックします。
-   **要求事項**:
    すべてのテストは、開発者のローカル環境で、この方法でパスすることが必須です。

### 4.2. 方法B: バッチモードでの自動実行

-   **目的**:
    CI/CD（継続的インテグレーション）サーバーなどで、**人間の手を介さずにすべてのテストを自動実行**するための方法です。コード変更がリポジトリにプッシュされるたびに、プロジェクト全体の品質が維持されていることを保証します。
-   **実行コマンド例**:
    ```bash
    /path/to/Unity.exe -batchmode -runTests -projectPath /path/to/project -testPlatform EditMode -testResults Assets/_Project/Tests/Results/editmode-results.xml
    /path/to/Unity.exe -batchmode -runTests -projectPath /path/to/project -testPlatform PlayMode -testResults Assets/_Project/Tests/Results/playmode-results.xml
    ```
-   **要求事項**:
    すべてのテストは、バッチモード実行時に問題（無限ループ、ダイアログによる停止など）を引き起こさないように実装する必要があります。`Debug.Log`以外の、ユーザー入力を待つような処理をテストコードに含めてはなりません。

## 5. 実装のベストプラクティス

-   **AAAパターン**: テストは`Arrange`（準備）、`Act`（実行）、`Assert`（検証）の3つのステップで構成します。
    ```csharp
    [Test]
    public void Add_TwoNumbers_ReturnsCorrectSum()
    {
        // Arrange - テスト対象の準備
        var calculator = new Calculator();
        int a = 2;
        int b = 3;

        // Act - 実行
        int result = calculator.Add(a, b);

        // Assert - 検証
        Assert.AreEqual(5, result);
    }
    ```
-   **NUnit属性の活用**: `[Test]`, `[TestCase]`, `[SetUp]`, `[TearDown]` などのNUnit属性を適切に活用し、効率的で可読性の高いテストを記述します。
-   **モックとスタブ**: テスト対象が他のクラスに依存している場合、その依存クラスをモック（偽物）に置き換えることで、テスト対象のロジックのみを独立して検証します。
-   **命名規則**: テストメソッド名は、`[テスト対象メソッド]_[状態]_[期待される結果]` の形式で、そのテストが何を検証しているのかが一目で分かるように命名します。
    -   例: `Add_PositiveAndNegativeNumbers_ReturnsCorrectSum`
