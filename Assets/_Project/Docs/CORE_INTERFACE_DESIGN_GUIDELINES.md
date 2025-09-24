# Core層インターフェース設計ガイドライン

## 1. はじめに

このドキュメントは、`URP3D_Base01`プロジェクトの`Core`層におけるインターフェース設計のベストプラクティスを定義するものです。

`Core`層のインターフェースは、プロジェクト全体の再利用性、拡張性、保守性を決定づける最も重要な要素です。ここに定義された原則を遵守することは、アーキテクチャの健全性を維持するために不可欠です。

## 2. 基本原則：Core層の役割

`Core`層は、ゲームのジャンルや見た目に依存しない、**普遍的で抽象的な「仕組み」と「概念」**を提供する責務を負います。

-   **OK**: `IHealth`（HPを持つもの、という概念）, `ICommand`（コマンド、という仕組み）
-   **NG**: `IPlayer`（プレイヤー、という具体的な役割）, `IOrc`（オーク、という具体的な種族）

`Core`層は、特定のゲームに登場する具体的な固有名詞を知ってはいけません。

## 3. 設計のベストプラクティス

### 3.1. 責務を一つに絞る（単一責任の原則 / インターフェース分離の原則）

**「巨大で万能なインターフェース」は作らないでください。** 責務ごとに小さく、具体的なインターフェースに分割します。

-   **Bad Practice:**
    ```csharp
    // あらゆるゲーム管理ができてしまう、巨大なインターフェース
    public interface IGameManager
    {
        void PlaySound(string soundName);
        void PauseGame();
        void AddScore(int amount);
        void MovePlayer(Vector3 direction);
    }
    ```
    このインターフェースは、サウンド、ゲーム状態、スコア、プレイヤー操作など、多数の責務が混在しており、肥大化しやすく、テストも困難です。

-   **Best Practice:**
    ```csharp
    // 責務ごとにインターフェースを分割する
    public interface IAudioManager { void PlaySound(string soundName); }
    public interface IPauseService { void PauseGame(); void ResumeGame(); }
    public interface IScoreService { void AddScore(int amount); }
    public interface IMovable { void Move(Vector3 direction); }
    ```
    `Feature`層のクラスは、本当に必要な機能（インターフェース）だけに依存できます。これにより、依存関係が最小限に抑えられ、コンポーネントの独立性が高まります。

### 3.2. 「何ができるか(What)」を定義し、「どうやるか(How)」は隠蔽する

インターフェースは、外部に公開する**「契約」**です。その機能が**どのように実装されているか**（`How`）を一切感じさせてはいけません。

-   **例: `IAudioManager`**
    -   **What（契約）**: `PlaySound("Explosion")` というメソッドを提供する。
    -   **How（実装の詳細）**: その内部でUnityの`AudioSource`を使っているのか、サードパーティのライブラリを使っているのか、オブジェクトプールを利用しているのか、といった仕組みは完全に隠蔽します。

この原則により、将来`AudioManager`の実装をまるごと入れ替えても、`IAudioManager`を利用している`Feature`層のコードは一切変更する必要がなくなります。

### 3.3. `Feature`層の具体的な「名詞」を使わない（最重要）

**`Core`層のインターフェースは、`Player`や`Enemy`、`Orc`、`Spaceship`といった、特定のゲームに登場する具体的な固有名詞を知ってはいけません。** 代わりに、それらが持つ**本質的な「役割」や「能力」**をインターフェースとして定義します。

-   **Bad Practice:**
    ```csharp
    // "Player" という Feature 層の概念に依存してしまっている
    public interface IHealthManager
    {
        void DamagePlayer(float amount);
        void HealPlayer(float amount);
    }
    ```
    これでは、敵やオブジェクトのHPを管理するためにこのインターフェースを再利用できません。

-   **Best Practice:**
    ```csharp
    // 「HPを持つもの」という普遍的な概念を定義する
    public interface IHealth
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        bool IsAlive { get; }

        void TakeDamage(float amount);
        void Heal(float amount);
    }
    ```
    この`IHealth`インターフェースは、**「HPを持つもの」という抽象的な概念**を定義しています。これを`Feature`層の`Player`クラス、`Enemy`クラス、`DestructibleBarrel`クラスに実装（implement）することで、どんなオブジェクトでも同じようにHPを扱えるようになります。

### 3.4. プリミティブ型と`Core`層のデータ構造のみを引数と戻り値に使う

上記3.3のルールを徹底するための具体的な方法です。インターフェースのメソッドが受け取る引数や、返す値の型は、以下のものに限定します。

-   `int`, `float`, `string`, `bool` などの**プリミティブ型**
-   `Vector3`, `Quaternion` などの**Unityの基本的な構造体**
-   **`Core`層自身で定義された、他のインターフェースやデータクラス**

**絶対に`Feature`層で定義されたクラス（例: `PlayerController`）を引数に取ってはいけません。** それを許した瞬間に、`Core`が`Feature`に依存してしまい、アーキテクチャの根幹である一方向の依存関係が崩壊します。

## 4. 実践例：ダメージ処理の抽象化プロセス

1.  **やりたいこと**: 「プレイヤーが敵を攻撃してダメージを与える」
2.  **悪い抽象化**: `Core`層に`IAttackSystem`を作り、`Attack(Player attacker, Enemy target)`のようなメソッドを定義する。
    -   **問題点**: `Player`と`Enemy`という`Feature`層の具体的なクラスに依存しているため、再利用性がなく、依存関係ルールに違反している。
3.  **良い抽象化**:
    -   **Step 1: 本質的な概念を抽出する**
        -   攻撃する側 → **「攻撃力を持つもの」 (Attacker)**
        -   攻撃される側 → **「HPを持つもの」 (Health)**
    -   **Step 2: `Core`層で普遍的なインターフェースを定義する**
        ```csharp
        // Core/Interfaces/IAttacker.cs
        public interface IAttacker
        {
            float AttackPower { get; }
        }

        // Core/Interfaces/IHealth.cs
        public interface IHealth
        {
            void TakeDamage(float amount);
            // ...
        }
        ```
    -   **Step 3: `Core`層のシステム（例: Command）は、これらの抽象インターフェースだけを使って処理を記述する**
        ```csharp
        // Core/Commands/DamageCommand.cs
        public class DamageCommand : ICommand
        {
            private IAttacker _attacker;
            private IHealth _target;

            public DamageCommand(IAttacker attacker, IHealth target)
            {
                _attacker = attacker;
                _target = target;
            }

            public void Execute()
            {
                float damage = _attacker.AttackPower;
                _target.TakeDamage(damage);
            }
        }
        ```
        この`DamageCommand`は、誰が誰を攻撃しているのか全く知りません。ただ「`IAttacker`が`IHealth`を持つものにダメージを与える」という事実だけを処理します。
    -   **Step 4: `Feature`層で、具体的なクラスにこれらのインターフェースを実装する**
        ```csharp
        // Features/Player/Scripts/Player.cs
        public class Player : MonoBehaviour, IAttacker, IHealth { /* ... */ }

        // Features/AI/Scripts/Enemy.cs
        public class Enemy : MonoBehaviour, IAttacker, IHealth { /* ... */ }
        ```

この設計により、`DamageCommand`は「プレイヤーが敵を攻撃する」ことも、「敵がプレイヤーを攻撃する」ことも、「敵が別の敵を攻撃する」ことさえも、**全く同じコードで**処理できるようになります。これが`Core`層におけるインターフェース設計の目指すゴールです。
