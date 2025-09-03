

# **建築的シナジー：Unity 6におけるScriptable Objectベースのイベントとコマンドパターンの統合**

## **第1章：現代のUnity開発における基礎的パラダイム**

現代のゲーム開発、特にUnity 6のような高度に進化したエンジンにおけるアーキテクチャ設計は、単なる機能実装を超え、プロジェクトの保守性、拡張性、そしてチーム全体の生産性を決定づける重要な要素となっています。複雑化するゲームシステムを効果的に管理するため、開発コミュニティは数々の設計パラダイムを洗練させてきました。その中でも、システムの疎結合化を促進する「Scriptable Object（SO）ベースのイベント駆動アーキテクチャ」と、操作をオブジェクトとしてカプセル化する「コマンドパターン」は、それぞれが強力な解決策を提供します。本章では、これら二つの foundational なパラダイムを個別に深く掘り下げ、その理論的背景と実践的価値を確立します。これは、後の章で展開される両者の統合に関する議論の強固な土台を築くための不可欠な準備作業です。

### **1.1. 疎結合アーキテクチャ：イベントチャネルとしてのScriptable Object**

ScriptableObjectは、Unityエンジンの黎明期から存在するクラスですが、その役割は時と共に大きく進化を遂げました。当初は主に、MonoBehaviourインスタンスから独立して大量の共有データを保存するための、シリアル化可能なデータコンテナとして認識されていました 1。武器のステータス、敵のパラメータ、レベル設定など、静的なデータをアセットとしてプロジェクト内に保持する用途で広く利用されてきました 2。しかし、熟練した開発者たちは、この「アセットとして存在する」という特性が、単なるデータ保持以上のポテンシャルを秘めていることを見出しました。それが、

ScriptableObjectをシステムのコミュニケーションを仲介する「イベントチャネル」として活用するアーキテクチャです 3。

このアプローチの核心は、システム間の直接的な参照関係を排除し、代わりに中間的なScriptableObjectアセットを介して通信を行う点にあります。これにより、システムは互いの存在を直接知る必要がなくなり、高度に疎結合な（loosely coupled）状態を実現できます。この概念は、特にRyan Hipple氏によるUniteでの講演によって広く知られるようになり、多くのプロジェクトで採用される標準的なプラクティスの一つとなりました 1。

このアーキテクチャは、主に二つのコンポーネントによって構成されます。

1. GameEvent（チャネル本体）:  
   これはScriptableObjectを継承したクラスであり、イベントチャネルそのものとして機能します。このアセットは、自身を購読しているリスナーのリストを保持し（C#のeventやUnityEventとして実装されることが多い）、イベントを発行するための公開メソッド、一般的にRaise()と呼ばれるものを提供します 3。この設計の妙は、「プレイヤーが死亡した」や「コインを獲得した」といった抽象的なゲーム内イベントを、Unityエディタ上で視認・参照可能な具体的なアセットに変換する点にあります。これにより、プログラマだけでなく、ゲームデザイナーもイベントの流れを直感的に理解し、設定することが可能になります。  
2. GameEventListener（購読者）:  
   こちらはMonoBehaviourを継承したコンポーネントで、特定のGameEventアセットへの参照を保持します。このコンポーネントは、自身のライフサイクル（OnEnableとOnDisable）を利用して、対応するGameEventへの購読登録と解除を自動的に行います。最も重要な特徴は、インスペクタ上でUnityEventを公開する点です 1。これにより、デザイナーはコードを一行も書くことなく、イベントが発生した際の応答（例えば、特定のサウンドを再生する、UIを有効にする、アニメーションをトリガーするなど）をエディタ上で直接設定できます。

このアーキテクチャがもたらす利益は計り知れません。第一に、システムのモジュール性が劇的に向上します。例えば、インベントリシステムは、UIシステムやサウンドシステムへの直接参照を持つ必要がなくなります。インベントリシステムは「アイテムが追加された」というGameEventを発行するだけでよく、UIやサウンドシステムはそのイベントをリッスンして自身の更新処理を行うだけです 3。これにより、各システムは独立して開発・テスト・修正が可能となり、いわゆる「スパゲッティコード」の発生を抑制します 9。

第二に、チーム内でのコラボレーションが円滑になります。プログラマがシステムの核となるロジックを実装し、イベントチャネルを定義すれば、デザイナーはGameEventListenerを用いてゲームの具体的な振る舞いを構築できます。これにより、プログラマとデザイナーの作業が並列化され、開発サイクルが加速します。また、シーンやプレハブ内での直接参照が減るため、バージョン管理システムにおけるマージコンフリクトのリスクも大幅に低減されます 1。

最終的に、このパターンは「疎結合・高凝集（Loose Coupling, High Cohesion）」という優れたソフトウェア設計の原則を具現化します 3。各モジュールは自身の責務に集中し（高凝集）、他のモジュールとのやり取りは抽象的なイベントチャネルを介して行われる（疎結合）。これは、かつて多用されたシングルトンマネージャが抱えていた、グローバルな状態への密な依存やテストの困難さといった問題をエレガントに解決する、現代的なアプローチと言えるでしょう。

### **1.2. 具体化されたメソッド呼び出し：コマンドパターンの形式的分析**

コマンドパターンは、GoF（Gang of Four）によって定義された古典的なビヘイビアデザインパターンの一つであり、その核心は「リクエスト（操作）を、そのリクエストに関するすべての情報を含むスタンドアロンのオブジェクトとしてカプセル化すること」にあります 12。この、メソッド呼び出しや操作といった「アクション」を、変数に格納したり、リストに追加したりできる「データ」に変換するプロセスは、専門的には「具体化（Reification）」と呼ばれます 13。この「アクションのデータ化」こそが、コマンドパターンが持つ力の源泉です。

このパターンは、構造的に四つの主要な役割によって定義されます。

1. Command（コマンド）:  
   実行されるべき操作をカプセル化するインターフェースまたは抽象クラスです。一般的にExecute()というメソッドを宣言し、多くの場合、操作を取り消すためのUndo()メソッドも持ちます。MoveCommandやAttackCommandといった具体的なコマンドクラスが、このインターフェースを実装します 14。  
2. Receiver（レシーバー）:  
   コマンドが実行された際に、実際の処理を行うオブジェクトです。例えば、PlayerMovementスクリプトやHealthComponentなどがこれに該当します。Commandオブジェクトは、自身がどのReceiverに対して操作を行うべきかを知るために、その参照を保持します。  
3. Invoker（インボーカー）:  
   コマンドの実行をトリガーするオブジェクトです。InputHandlerや、コマンドのキューを管理するCommandProcessorなどがこの役割を担います。Invokerの重要な特徴は、自身が実行するコマンドが具体的に何を行うかを知らない点です。Invokerはただ、渡されたCommandオブジェクトがExecute()メソッドを持つことだけを知っており、それを呼び出すだけです 15。これにより、リクエストを発行するオブジェクトと、それを実行するオブジェクトが完全に分離されます。  
4. Client（クライアント）:  
   具体的なCommandオブジェクトのインスタンスを生成し、それを適切なReceiverと関連付ける役割を担います。そして、生成したコマンドをInvokerに渡します。

ゲーム開発において、コマンドパターンがもたらす「アクションのデータ化」は、数多くの強力な機能を実現するための基盤となります。

* Undo/Redoシステム:  
  実行されたCommandオブジェクトをスタックに保存しておくことで、Undo（元に戻す）機能はスタックから最新のコマンドを取り出してUndo()メソッドを呼び出すだけで実現できます。同様に、Redo（やり直し）機能も、Undoされたコマンドを別のスタックに保持しておくことで実装可能です。これは、パズルゲームやストラテジーゲーム、さらにはレベルエディタのようなツール開発において不可欠な機能です 15。  
* アクションのキューイング:  
  リアルタイムストラテジー（RTS）やターン制ストラテジーゲームにおいて、プレイヤーがユニットに対して複数のアクション（移動、攻撃、建設など）を連続して指示する場面は頻繁にあります。コマンドパターンを用いると、これらの指示をCommandオブジェクトのリストやキューとして保持し、順番に実行していくシステムを容易に構築できます 13。  
* 入力ハンドリングとキーリマッピング:  
  プレイヤーの入力（キーボード、マウス、ゲームパッド）と、それによって引き起こされるゲーム内のアクションを分離することができます。InputHandlerの役割は、特定の入力があった場合に、対応するCommandオブジェクトを生成してInvokerに渡すことだけに単純化されます。これにより、どの入力がどのコマンドに対応するかをデータ（例えば設定ファイル）として外部から変更できるようになり、柔軟なキーリマッピング機能が実現します 13。

これらの応用例から明らかなように、コマンドパターンは、操作の実行タイミング、順序、そして操作そのものをプログラムの流れから切り離し、より柔軟でデータ駆動な制御を可能にするための強力な設計ツールです。

これら二つのパラダイムを並べて考察すると、その根底に流れる哲学的な共通性が見えてきます。SOイベントアーキテクチャは、オブジェクト間の**コミュニケーションの繋がり**を外部化し、疎結合を実現します。一方、コマンドパターンは、**アクションやメソッド呼び出しそのもの**を外部化し、操作の実行を柔軟にします。両者はともに、ハードコーディングされた依存関係を排除し、システムをよりデータ駆動にすることを目指す、いわば同じ設計思想のコインの裏表です。

この親和性の高さは、両者の統合が単なる技術的な組み合わせに留まらないことを示唆しています。SOイベントは、疎結合なメッセージを伝達するための理想的な**輸送層（Transport Layer）**を提供し、コマンドオブジェクトは、そのメッセージの構造化され、状態を持ち、強力な**ペイロード（Payload）**となり得ます。イベント駆動システムは、非同期で疎結合なメッセージを扱うための基盤がすでに整っています。コマンドは、そのシステム上を流れる、より高度で多機能なメッセージ形式と見なすことができるのです。この自然なシナジーこそが、ユーザーが求める「親和性の高さ」の核心であり、両者を統合することが、単なる足し算ではなく、アーキテクチャ全体の表現力を飛躍的に向上させる掛け算となる理由です。

## **第2章：コマンドとイベントの統合手法**

前章で確立した理論的基盤を踏まえ、本章ではScriptableObjectベースのイベントアーキテクチャとコマンドパターンを具体的に統合するための実践的な手法を探求します。これら二つの強力なパターンを組み合わせるアプローチは一つではなく、それぞれに利点と欠点、そして最適な適用シナリオが存在します。ここでは、主要な二つの統合戦略を詳細に分析し、それぞれのアーキテクチャがプロジェクトに与える影響を比較検討することで、開発者が自身のプロジェクト要件に最適な選択を行えるような指針を提示します。

### **2.1. アプローチA：具象コマンドとしてのScriptable Object (CommandSO)**

これは、二つのパターンを統合する最も直接的で直感的な方法です。このアプローチでは、コマンドパターンにおけるCommandの役割をScriptableObject自身が担います。具体的には、コマンドの基底クラス（例：CommandSO）がScriptableObjectを継承し、個々の具体的なコマンド（例：JumpCommandSO、FireballCommandSO）がその基底クラスを継承したアセットとしてプロジェクト内に作成されます 14。

**実装例：**

C#

```csharp
// 全てのコマンドアセットの基底クラス  
public abstract class CommandSO : ScriptableObject  
{  
    public abstract void Execute();  
}

// 具体的なジャンプコマンドのアセット  
[CreateAssetMenu(fileName = "NewJumpCommand", menuName = "Commands/JumpCommand")]  
public class JumpCommandSO : CommandSO  
{  
    // このコマンドが作用する対象（Receiver）をインスペクタで設定可能  
    private PlayerJumper playerJumper;

    public override void Execute()  
    {  
        if (playerJumper!= null)  
        {  
            playerJumper.Jump();  
        }  
    }  
}
```

ワークフロー：  
このアプローチの最大の魅力は、そのワークフローが非常にデザイナーフレンドリーである点です。ゲームデザイナーは、UnityエディタのCreateAssetMenuからJumpCommandSOのようなコマンドアセットを自由に作成し、インスペクタ上でそのパラメータ（例えば、ジャンプの高さや効果音など）を調整できます。そして、作成したコマンドアセットを、MonoBehaviourの公開フィールドにドラッグ＆ドロップで割り当てることができます。例えば、ある特定のアイテム（ItemSO）が使用された時の効果として、OnUseCommandというフィールドに特定のCommandSOを割り当てる、といった使い方が可能です 20。これにより、ゲームの振る舞いをコードの変更なしに、アセットの組み合わせだけで定義・変更できるようになります 11。  
**利点：**

* **デザイナーとの親和性**: 振る舞いがアセットとして可視化されるため、非プログラマでもゲームロジックの構築に貢献しやすくなります。  
* **データ駆動設計の徹底**: ゲームのロジックがデータ（アセット）として扱われるため、柔軟な差し替えやバリエーションの作成が容易です。  
* **静的なアクションに最適**: 事前に定義された、パラメータが固定的なアクション（例：「UIウィンドウを開く」「特定のBGMを再生する」）の実装には非常に適しています。

欠点：  
このアプローチの柔軟性は、コマンドが実行時に動的なデータを必要とする場合に著しく低下します。例えば、「指定した位置に移動する」というMoveCommandを考えた場合、移動先の座標は実行の瞬間に決定されるため、アセット内に静的に保持することができません。この動的なデータをCommandSOに渡す方法は不格好になりがちで、アセットの状態管理に問題を引き起こす可能性があります。ScriptableObjectはプレイモードを終了しても値が保持されるため、ランタイムで変更されたデータが意図せず永続化してしまうという典型的な落とし穴があります 9。この問題は、あるユーザーがランタイムで特定のダメージ値を持つ  
FireBallCommandを生成したいが、SOアプローチは事前定義された固定ダメージの火の玉にしか適していない、というジレンマとして明確に示されています 23。Undo/Redoシステムのように、各コマンドの実行インスタンスが固有の状態（例：移動前の座標）を保持する必要がある場合、このアプローチはさらに不向きとなります。

### **2.2. アプローチB：イベント駆動によるコマンドのインスタンス化**

アプローチAの動的なデータ扱いの困難さを克服するため、より柔軟で厳密な関心の分離を志向するのがこのアプローチです。ここでは、ScriptableObjectベースのイベント（GameEvent）は純粋な「信号」としての役割に徹します。そして、その信号を受け取った中央集権的なサービスやマネージャ（Invokerの役割を担う）が、応答として通常のC#クラス（POCO - Plain Old C# Object）で実装されたICommandオブジェクトをその場でインスタンス化します。

**データフロー：**

1. **発行者（Broadcaster）**: InputHandlerのようなコンポーネントが、プレイヤーのジャンプ入力を検知し、PlayerJumpEventというGameEventアセットのRaise()メソッドを呼び出します。このイベントには、必要に応じてペイロード（付加情報）を含めることができます。例えば、PlayerMoveEventであれば、移動先のVector3座標をペイロードとして渡します。  
2. **リスナー/インボーカー（Listener/Invoker）**: CommandInvokerという中央サービスが、PlayerJumpEventをリッスンしています。このサービスはシーン内に存在するシングルトンや、依存性注入（DI）フレームワークによって管理されるオブジェクトです。  
3. インスタンス化（Instantiation）: PlayerJumpEventを受け取ったCommandInvokerは、新しいコマンドオブジェクトを生成します。  
   ICommand jumpCommand = new JumpCommand(playerReference);  
   ここでJumpCommandはScriptableObjectではなく、ICommandインターフェースを実装した通常のC#クラスです。  
4. **実行（Execution）**: CommandInvokerは生成したコマンドのExecute()メソッドを呼び出します。Undo/Redoをサポートする場合、このコマンドオブジェクトをUndoスタックにプッシュします。

**利点：**

* **動的なデータへの高い柔軟性**: コマンドは実行時にインスタンス化されるため、コンストラクタを通じて必要なランタイムデータを簡単に渡すことができます。これにより、状況に応じた多様な振る舞いを実現できます。  
* **クリーンな状態管理**: コマンドは軽量で一時的な（transient）C#オブジェクトであるため、ScriptableObjectのような永続的な状態管理の問題から解放されます。各コマンドインスタンスは、実行が完了すればガベージコレクションの対象となります（Undoスタックに保持される場合を除く）。  
* **明確な関心の分離**: 「何が起こったか」（イベント）と「それをどのように実行するか」（コマンド）が明確に分離されます。これにより、アーキテクチャの見通しが良くなり、テストも容易になります。  
* **Undo/Redoへの適合性**: 各コマンドインスタンスが固有の状態（移動前の座標など）をフィールドとして保持できるため、Undo/Redoシステムの実装に非常に適しています。

**欠点：**

* **デザイナーからの可視性の低下**: コマンドの生成ロジックはCommandInvoker内のコードに集約されるため、アプローチAのようにデザイナーがインスペクタ上で直接コマンドを割り当てることはできません。デザイナーの作業は、イベントを発行する部分の設定に限られます。  
* **中央集権的なシステムの必要性**: このアプローチは、コマンドの生成と実行を管理するCommandInvokerのような、より堅牢な中央システムの存在を前提とします。アーキテクチャの初期設計がより重要になります。

### **2.3. 統合アプローチの比較分析**

これら二つのアプローチは、トレードオフの関係にあります。アプローチAはシンプルさとデザイナーのワークフローを優先し、アプローチBは柔軟性と厳密なアーキテクチャを優先します。どちらを選択するかは、プロジェクトの具体的な要件、チームの構成、そして実装しようとしている機能の性質に依存します。以下の比較表は、それぞれの特性を要約し、意思決定の助けとなることを目的としています。

**表2.1：コマンド統合手法の比較分析**

| 特徴 | アプローチA (CommandSO) | アプローチB (イベント駆動インスタンス化) |
| :---- | :---- | :---- |
| **柔軟性と動的データ** | **低い**: 静的なパラメータを持つ事前定義済みのアクションに最適。ランタイムデータの扱いは不格好で、状態管理の問題を引き起こす可能性がある 23。 | **高い**: コマンドは実行時に生成され、コンストラクタ経由で任意のランタイムデータを受け取れる。状況に応じた動的な振る舞いを容易に実現可能。 |
| **デザイナーのワークフロー** | **非常に良い**: コマンドがアセットとして可視化され、ドラッグ＆ドロップで設定可能。非プログラマでも振る舞いを直接構築できる 11。 | **普通**: デザイナーはイベントの発行側を設定するが、コマンドの生成ロジックはコード内にあり直接触れない。イベントチャネルの参照が主な作業となる。 |
| **パフォーマンスオーバーヘッド** | **やや高い**: ScriptableObjectはUnityEngine.Objectであり、軽量なC#オブジェクトと比較してメモリやインスタンス化のオーバーヘッドが大きい。 | **低い**: コマンドは軽量なPOCO（Plain Old C# Object）であり、必要な時だけ生成され、不要になれば破棄されるため、パフォーマンスへの影響は最小限。 |
| **状態管理** | **注意が必要**: ランタイムでの変更がエディタ上で永続化するリスクがある。意図しない状態の共有や、シーン遷移後のリセット処理が必要になる場合がある 9。 | **クリーン**: コマンドインスタンスは一時的であり、状態はカプセル化されている。永続化の問題はなく、状態管理がシンプル。 |
| **Undo/Redoシステムへの適合性** | **低い**: 各コマンドインスタンスが固有の状態（例：実行前の値）を保持することが困難。アセットは共有されるため、インスタンスごとの状態保存には不向き。 | **非常に良い**: 各コマンドインスタンスが自身の状態をフィールドとして保持できるため、Undo/Redoの実装に理想的。これが標準的な使い方である 15。 |
| **初期実装の容易さ** | **容易**: 基底クラスと具象クラスを作成するだけで基本的なシステムが機能する。中央集権的な管理システムがなくても始めやすい。 | **やや複雑**: コマンドをリッスンし、生成・実行を管理する中央のCommandInvokerシステムを設計・実装する必要がある。 |
| **最適なユースケース** | ・UIボタンの静的な機能 ・アイテムの固定効果 ・事前定義されたAIの振る舞い | ・プレイヤーの動的なアクション ・Undo/Redo機能 ・複雑なシーケンスやパラメータを伴うスキル |

この分析から、両アプローチは競合するものではなく、相補的なものであることがわかります。単純で静的なアクションにはアプローチAを、複雑で動的なアクションやUndo/Redoが求められるシステムにはアプローチBを選択するというハイブリッドな使い分けが、多くのプロジェクトにとって現実的な解となるでしょう。しかし、アーキテクチャの統一性を損なうことなく、両者の利点を享受する方法は存在するのでしょうか。次章では、この問いに答えるための先進的な技術を探ります。

## **第3章：ポリモーフィックシリアライゼーションによる先進的実装**

前章で提示された二つのアプローチ、CommandSOとイベント駆動インスタンス化は、それぞれデザイナーのワークフローとプログラムの柔軟性という異なる側面で優位性を持っていました。このトレードオフは、長らくUnity開発における設計上の制約と見なされてきました。しかし、Unityが導入した先進的なシリアライゼーション機能、特に属性は、この二項対立を解消し、両者の長所を兼ね備えた、より洗練されたハイブリッドアーキテクチャの構築を可能にします。本章では、このがもたらすパラダイムシフトを解き明かし、それを利用した究極的な統合設計図を提示します。

### **3.1. C#の属性である：パラダイムシフト**

この属性の重要性を理解するためには、まずUnityの従来のシリアライゼーションの制約を把握する必要があります。Unityのシリアライザは、UnityEngine.Object（MonoBehaviourやScriptableObjectなど）を継承する型については「参照」でシリアライズしますが、それ以外のカスタムクラス（POCO）については「値」でシリアライズします。これは、インスペクタ上でList<ICommand>のようなインターフェース型のリストを公開しても、そこにJumpCommandやMoveCommandといった異なる具象クラスのインスタンスを格納し、その多態性（ポリモーフィズム）を維持したままシリアライズすることができないことを意味していました 24。開発者はこの制約を回避するために、すべてを

ScriptableObjectとして実装する（アプローチA）か、シリアライズを諦めてランタイムで生成する（アプローチB）かの選択を迫られていたのです。

C#の属性は、この長年の課題に対する直接的な解決策です。この属性をフィールドに付与すると、UnityはそのフィールドがUnityEngine.Objectを継承していないPOCOであっても、「参照」としてシリアライズするように指示されます。これにより、インターフェースや抽象クラス型のフィールドやリストに対して、その具象型のインスタンスをインスペクタ上で（カスタムエディタやライブラリの助けを借りて）割り当て、その多態性を維持したまま保存することが可能になります 24。

**コード例：**

C#

```csharp
public interface ICommand { void Execute(); }

public class JumpCommand : ICommand  
{  
    public float jumpForce;  
    public void Execute() { /*... */ }  
}

public class MoveCommand : ICommand  
{  
    public Vector3 direction;  
    public float speed;  
    public void Execute() { /*... */ }  
}

public class CommandHolder : MonoBehaviour  
{  
    // このリストはインスペクタでJumpCommandやMoveCommandのインスタンスを  
    // 混在させて保持できる  
     
    public List<ICommand> commands;  
}
```

この機能がもたらす影響は絶大です。これは単なる技術的な改善ではなく、Unityにおけるデータと振る舞いの境界線を再定義するアーキテクチャの実現要因（Architectural Enabler）です。開発者はもはや、「MonoBehaviourかScriptableObjectか」という二元論に縛られる必要がなくなります。代わりに、軽量なC#オブジェクトをデータ駆動ワークフローの第一級市民として扱い、ScriptableObjectのオーバーヘッドや状態管理の懸念なしに、複雑で再利用可能なデータ構造を構築できるのです 25。これは、アプローチAとBの間に存在した根本的な緊張関係、すなわち「デザイナーフレンドリーなアセット」と「プログラマフレンドリーなランタイムオブジェクト」の対立を解消します。C#の属性を用いることで、コマンドの

データ定義はアセット内にシリアライズし、コマンドの**実行**は一時的なランタイムオブジェクトとして扱う、という理想的な分離が実現可能になるのです。

この変化の背後にあるロジックを追うと、より深いレベルでの変革が見えてきます。従来の制約は、Unityのインスペクタで設定可能なものはUnityEngine.Objectでなければならない、という歴史的経緯に起因していました。C#の属性はこの制約を打ち破り、POCOの多態性をインスペクタに持ち込むことを可能にしました。これにより、**データオーサリングのワークフロー**と**ランタイムのオブジェクトモデル**を分離するという、高度なアーキテクチャ設計が可能になります。デザイナーは、ScriptableObjectの状態管理の複雑さに煩わされることなく、振る舞いのための複雑な「レシピ」（コマンド定義のリスト）を構築でき、アーキテクトはより効率的でクリーンなランタイムシステムを自由に設計できるのです。

### **3.2. ハイブリッドアーキテクチャの設計図：両者の長所の融合**

C#の属性の力を借りて、我々はアプローチAとBの利点を統合した、専門家が推奨する決定版のアーキテクチャを構築することができます。このハイブリッドモデルは、柔軟性、保守性、そしてチームのコラボレーション効率を最大化するように設計されています。

**アーキテクチャの構成要素：**

1. GameEvent (ScriptableObject):  
   これは引き続き、システム間の主要な通信バックボーンとして機能します。イベントは「意図」を伝達します。例えば、OnSkillUsedイベントは、どのスキルが使用されたかを示すSkillDataアセットをペイロードとして運びます。  
2. CommandInvoker (中央サービス):  
   シーンに一つ存在するサービスで、GameEventをリッスンします。このサービスの責務は、イベントを受け取り、対応するコマンドを生成し、実行を管理することです。依存性注入（DI）フレームワークを用いて管理することが推奨されます 30。  
3. SkillData (ScriptableObject):  
   特定のスキル（例：「ファイアボール」）を定義するデータアセットです。このアセットは、アプローチAのように直接的なCommandSOへの参照を持つ代わりに、C#の属性を用いてシリアライズされたコマンド定義のリストを保持します。  
   public List<ICommandDefinition> commandDefinitions;  
4. ICommandDefinition (POCO):  
   これはC#の属性が付与された、インターフェースまたは抽象クラスです。このオブジェクトの役割は、ランタイムでICommandを生成するために必要なデータを保持することです。例えば、DamageCommandDefinitionはdamageAmountやelementTypeといったフィールドを持ちます。デザイナーはインスペクタでこれらの値を設定します。  
5. ICommand (POCO):  
   これは従来通りの、実行ロジックを持つランタイムコマンドオブジェクトです。CommandInvokerが、ICommandDefinitionのデータを用いて、実行時にこのオブジェクトをインスタンス化します。

**データフローの具体例：**

1. プレイヤーがスキルボタンを押します。InputHandlerはOnSkillUsedイベントを発行し、ペイロードとして対応するSkillData SOを渡します。  
2. CommandInvokerがこのイベントを受信します。  
3. CommandInvokerは受け取ったSkillDataアセット内のcommandDefinitionsリストを走査します。  
4. リスト内の各definitionオブジェクト（例：DamageCommandDefinition）に対して、CommandInvokerは対応するランタイムICommandをインスタンス化します。この際、definitionからデータを取り出し、コマンドのコンストラクタに渡します。  
   var command = new DamageCommand(target, definition.damageAmount, definition.elementType);  
5. CommandInvokerは生成したcommandのExecute()を呼び出し、必要に応じてUndoスタックにプッシュします。

このハイブリッドモデルがもたらす究極のバランス：  
この設計は、すべての関係者にとっての最適解を提供します。

* **デザイナーにとって**: SkillDataという直感的なScriptableObjectを操作します。インスペクタ上で「ダメージを与える」「状態異常を付与する」といったICommandDefinitionのインスタンスをリストに追加し、そのパラメータを調整するだけで、複雑なスキルを組み立てることができます。振る舞いの構成が完全にデータ駆動になります。  
* **プログラマにとって**: ランタイムでは、クリーンで一時的なICommandオブジェクトを扱います。これらのオブジェクトは必要なデータを持って生成され、状態管理が容易で、テストも簡単です。ScriptableObjectの永続化の問題を心配する必要はありません。  
* **アーキテクトにとって**: システムは高度に疎結合で、拡張性に富んでいます。新しい種類のコマンド効果を追加したい場合、新しいICommandDefinitionとICommandクラスを追加し、CommandInvokerにその生成ロジックを登録するだけで済み、既存のコードへの影響は最小限です。

このハイブリッドアーキテクチャは、C#の属性という技術的な触媒を用いて、SOイベントの疎結合性、コマンドパターンの柔軟性、そしてScriptableObjectのデータ駆動ワークフローを、矛盾なく一つのエレガントなソリューションに統合します。これは、Unity 6における現代的なゲームアーキテクチャの一つの完成形と言えるでしょう。

## **第4章：ケーススタディ：可逆的なアクションシステムの実装**

理論と設計図だけでは、アーキテクチャの真価は完全には伝わりません。本章では、前章で提示したハイブリッドアーキテクチャを実際に適用し、具体的で実践的な「可逆的（Undo/Redo可能）なアクションシステム」を構築するケーススタディを展開します。この実装を通じて、各コンポーネントがどのように連携し、データがどのように流れ、そしてシステム全体としてどのようにして柔軟性と堅牢性を両立させるのかを、コードレベルで詳細に示します。

### **4.1. CommandInvokerと履歴スタックの設計**

システムの心臓部となるのがCommandInvokerです。このクラスは、コマンドの実行、そしてその履歴管理を一手に担います。Undo/Redo機能を実現するため、内部に二つのスタックを保持します 15。

**CommandInvoker.cs の実装:**

C#

```csharp
using System.Collections.Generic;  
using UnityEngine;

public class CommandInvoker : MonoBehaviour  
{  
    private readonly Stack<ICommand> _undoStack = new Stack<ICommand>();  
    private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();

    // 他のシステム（例：InputHandler）からコマンドを受け取り実行する  
    public void ExecuteCommand(ICommand command)  
    {  
        command.Execute();  
        _undoStack.Push(command);  
          
        // 新しいコマンドが実行されたら、それまでのRedo履歴は無効になる  
        _redoStack.Clear();   
          
        // UIなどに変更を通知するためのイベントを発行する（詳細は次節）  
        BroadcastHistoryChanges();  
    }

    // Undo処理  
    public void Undo()  
    {  
        if (_undoStack.Count > 0)  
        {  
            ICommand command = _undoStack.Pop();  
            command.Undo();  
            _redoStack.Push(command);  
            BroadcastHistoryChanges();  
        }  
    }

    // Redo処理  
    public void Redo()  
    {  
        if (_redoStack.Count > 0)  
        {  
            ICommand command = _redoStack.Pop();  
            // Redoは通常、Executeと同じロジックを再実行する  
            command.Execute();   
            _undoStack.Push(command);  
            BroadcastHistoryChanges();  
        }  
    }

    private void BroadcastHistoryChanges()  
    {  
        // OnUndoStackChanged.Raise(_undoStack.Count > 0);  
        // OnRedoStackChanged.Raise(_redoStack.Count > 0);  
        Debug.Log($"Undo: {_undoStack.Count}, Redo: {_redoStack.Count}");  
    }  
}
```

このCommandInvokerは、コマンドのライフサイクルを完全に管理します。ExecuteCommandメソッドは、コマンドを実行した後にそれを_undoStackに積み、同時に_redoStackをクリアします。これは、ユーザーが新しい操作を行った場合、それ以前の「やり直し」のタイムラインは意味をなさなくなるため、標準的な挙動です 32。

UndoとRedoメソッドは、それぞれスタック間でコマンドを移動させながら、対応するメソッドを呼び出します。

### **4.2. イベントチャネルを介したコマンド状態のブロードキャスト**

優れたアーキテクチャの鍵は、疎結合を維持することです。CommandInvokerがUIのボタンやテキストを直接参照してしまうと、UIの変更がCommandInvokerに影響を与える密結合な関係が生まれてしまいます。これを避けるため、CommandInvokerは自身の状態変化をGameEventチャネルを通じてブロードキャストするべきです 33。

**拡張されたCommandInvoker.cs:**

C#

```csharp
//... 既存のコード...  
using UnityEngine.Events;

// パラメータ付きイベントのためのシンプルなGameEventの例

public class BoolEventChannelSO : ScriptableObject  
{  
    public UnityAction<bool> OnEventRaised;  
    public void Raise(bool value) => OnEventRaised?.Invoke(value);  
}

public class CommandInvoker : MonoBehaviour  
{  
    //... 既存のスタック...

     
    private BoolEventChannelSO onUndoStateChanged;  
     
    private BoolEventChannelSO onRedoStateChanged;  
      
    //... ExecuteCommand, Undo, Redo...

    private void BroadcastHistoryChanges()  
    {  
        onUndoStateChanged?.Raise(_undoStack.Count > 0);  
        onRedoStateChanged?.Raise(_redoStack.Count > 0);  
    }  
}
```

**UI側のリスナー UIManager.cs:**

C#

```csharp
using UnityEngine;  
using UnityEngine.UI;

public class UIManager : MonoBehaviour  
{  
    private Button undoButton;  
    private Button redoButton;

    // これらのメソッドは、GameEventListenerコンポーネントを介して  
    // onUndoStateChangedとonRedoStateChangedイベントに接続される  
    public void UpdateUndoButtonState(bool isEnabled)  
    {  
        undoButton.interactable = isEnabled;  
    }

    public void UpdateRedoButtonState(bool isEnabled)  
    {  
        redoButton.interactable = isEnabled;  
    }  
}
```

この設計により、CommandInvokerはUIの存在を一切知る必要がなくなります。Invokerはただ「Undoスタックの状態が変わった」という事実を、onUndoStateChangedチャネルを通じてブロードキャストするだけです。一方、UIManagerはそのイベントをリッスンし、自身の責務であるボタンの状態更新を行います。これにより、システムはモジュール化され、テストや変更が容易になります。例えば、将来的にUndo/Redoの状態をログに出力する新しいシステムを追加する場合でも、CommandInvokerのコードを変更することなく、同じイベントをリッスンするだけで実現できます。

### **4.3. 応用：データ駆動型のスキル・アイテムシステム**

それでは、この基盤の上に、ハイブリッドアーキテクチャの真骨頂であるデータ駆動型のシステムを構築します。

**ステップ1：コマンド定義と実行クラスの作成**

C#

```csharp
// ICommandDefinition.cs (マーカーインターフェース)  
public interface ICommandDefinition { }

// ICommand.cs (ランタイムコマンドのインターフェース)  
public interface ICommand { void Execute(); void Undo(); }

// 体力回復コマンドの定義と実行クラス

public class HealCommandDefinition : ICommandDefinition  
{  
    public int healAmount;  
}

public class HealCommand : ICommand  
{  
    private readonly HealthComponent _target;  
    private readonly int _healAmount;

    public HealCommand(HealthComponent target, int healAmount)  
    {  
        _target = target;  
        _healAmount = healAmount;  
    }

    public void Execute() => _target.Heal(_healAmount);  
    public void Undo() => _target.TakeDamage(_healAmount); // Undoはダメージ  
}

// ダメージコマンドの定義と実行クラス

public class DamageCommandDefinition : ICommandDefinition  
{  
    public int damageAmount;  
    public string elementType;  
}

public class DamageCommand : ICommand  
{  
    private readonly HealthComponent _target;  
    private readonly int _damageAmount;

    public DamageCommand(HealthComponent target, int damageAmount)  
    {  
        _target = target;  
        _damageAmount = damageAmount;  
    }

    public void Execute() => _target.TakeDamage(_damageAmount);  
    public void Undo() => _target.Heal(_damageAmount); // Undoは回復  
}
```

**ステップ2：アイテムデータSOの作成**

C#

```csharp
// ItemData.cs  
using System.Collections.Generic;  
using UnityEngine;

public class ItemData : ScriptableObject  
{  
    public string itemName;  
    public string description;

    // デザイナーはインスペクタでこのリストにHealCommandDefinitionなどを追加できる  
     
    public List<ICommandDefinition> commandDefinitions = new List<ICommandDefinition>();  
}

**ステップ3：CommandInvokerの拡張（コマンドファクトリ機能）**

CommandInvokerは、ICommandDefinitionからICommandを生成する役割も担います。

C#

```csharp
public class CommandInvoker : MonoBehaviour  
{  
    //... 既存のコード...

    // この例ではプレイヤーを対象とする  
    private HealthComponent playerHealth;

    // ItemDataを受け取るイベントリスナーメソッド  
    public void OnItemUsed(ItemData itemData)  
    {  
        foreach (var definition in itemData.commandDefinitions)  
        {  
            ICommand command = CreateCommandFromDefinition(definition);  
            if (command!= null)  
            {  
                ExecuteCommand(command);  
            }  
        }  
    }

    private ICommand CreateCommandFromDefinition(ICommandDefinition definition)  
    {  
        switch (definition)  
        {  
            case HealCommandDefinition healDef:  
                return new HealCommand(playerHealth, healDef.healAmount);  
            case DamageCommandDefinition damageDef:  
                // この例ではプレイヤーが自身にダメージを与えるアイテム  
                return new DamageCommand(playerHealth, damageDef.damageAmount);  
            //... 他のコマンド定義に対するケースを追加...  
            default:  
                Debug.LogWarning($"No command mapping for {definition.GetType()}");  
                return null;  
        }  
    }  
}
```

**実行フローの全体像：**

1. **デザイナーの作業**: UnityエディタでItemDataアセット「すごい回復薬」を作成します。インスペクタでcommandDefinitionsリストにHealCommandDefinitionを追加し、healAmountを50に設定します。  
2. **プレイヤーの操作**: プレイヤーがインベントリUIで「すごい回復薬」を使用します。UIスクリプトはOnItemUsedEvent（ItemDataをペイロードとして持つGameEvent）を発行します。  
3. **イベントの伝播**: CommandInvokerがアタッチされたGameEventListenerがOnItemUsedEventをリッスンしており、CommandInvokerのOnItemUsed(ItemData itemData)メソッドを呼び出します。  
4. **コマンドの生成と実行**: CommandInvokerは受け取ったItemDataのcommandDefinitionsを走査し、HealCommandDefinitionを見つけます。CreateCommandFromDefinitionメソッドがnew HealCommand(playerHealth, 50)を生成し、ExecuteCommandがそれを実行し、Undoスタックに積みます。  
5. **結果**: プレイヤーの体力が50回復します。  
6. **Undo操作**: プレイヤーがUndoボタンを押すと、UIがCommandInvokerのUndo()を呼び出します。InvokerはスタックからHealCommandを取り出し、そのUndo()メソッド（この場合はTakeDamage(50)）を実行します。結果、プレイヤーの体力は元に戻ります。

このケーススタディは、ハイブリッドアーキテクチャが、デザイナーの直感的なデータ編集能力と、プログラマが求めるクリーンで拡張可能なランタイムロジックを、いかにして両立させるかを示しています。システムは完全にモジュール化され、データ駆動であり、かつ強力なUndo/Redo機能まで備えています。これは、複雑なゲームシステムを構築するための堅牢かつエレガントなソリューションです。

## **第5章：最終分析：アーキテクチャの親和性、落とし穴、および推奨事項**

これまでの章で、ScriptableObjectベースのイベントアーキテクチャとコマンドパターンの基礎理論、統合手法、そして先進的な実装例を詳細に検討してきました。本章では、これらの分析を総合し、Unity 6プロジェクトにおける両者の組み合わせに関する最終的な結論を導き出します。アーキテクチャとしての「親和性」を最終的に評価し、導入に際して注意すべき潜在的な課題とその緩和策を提示し、そして将来のプロジェクトに向けた戦略的な推奨事項を提言します。

### **5.1. 親和性に関する最終評価：極めて相乗効果の高いペアリング**

**結論として、ScriptableObjectベースのイベント駆動アーキテクチャとコマンドパターンの親和性は、単に「高い」という言葉では不十分であり、「卓越して相乗効果が高い」と評価するのが最も適切です。** これらは単に互換性があるだけでなく、互いの長所を増幅し、短所を補い合う、相互補強的な関係にあります。

この強力なシナジーの根源は、両者が共有する根本的な設計思想、すなわち「関心の分離」と「振る舞いのデータ化」にあります 1。

* **SOイベントは理想的な「通信インフラ」を提供します。** システム間の直接的な依存関係を断ち切り、疎結合なメッセージングのための堅牢な基盤を築きます。これにより、コマンドを発行する側（InvokerやClient）と、コマンドの実行結果に関心を持つ他のシステム（UI、サウンド、VFXなど）とをエレガントに分離できます。  
* **コマンドは理想的な「メッセージペイロード」を提供します。** SOイベントが運ぶメッセージは、単なる通知（「何かが起こった」）に留まりません。コマンドオブジェクトをペイロードとすることで、そのメッセージは「実行可能な操作」「状態を持つアクション」「取り消し可能な処理」へと昇華します。これにより、アーキテクチャは単純な通知システムから、高度で可逆的なアクション管理システムへと進化します。

つまり、SOイベントがシステムの**「神経網」**を形成し、コマンドがその神経網を流れるリッチで構造化された**「神経信号」**となるのです。この組み合わせにより、開発者はモジュール性が高く、テストが容易で、デザイナーにも理解しやすい、スケーラブルなアーキテクチャを構築することが可能になります。

### **5.2. 潜在的な課題と緩和策**

この強力なアーキテクチャも万能ではなく、導入と運用にはいくつかの注意点が存在します。これらの課題を事前に認識し、適切な対策を講じることが、プロジェクトを成功に導く鍵となります。

* 課題1：SOにおけるランタイムデータの管理  
  ScriptableObjectの値をランタイムで変更すると、その変更はエディタのプレイモード終了後も保持される、という挙動は古典的な落とし穴です 9。これにより、次回のプレイ時に予期せぬ状態からゲームが始まり、デバッグが困難なバグの原因となることがあります。  
  * **緩和策**:  
    1. **ハイブリッドモデルの採用**: 第3章で提唱した、C#の属性を用いるハイブリッドアーキテクチャは、この問題を大部分回避します。コマンドの実行状態は一時的なPOCOに保持されるため、SOの状態をランタイムで変更する必要性が大幅に減少します。  
    2. **厳格な初期化処理**: グローバルな設定値など、どうしてもSOでランタイムデータを管理する必要がある場合は、ゲーム開始時にそれらの値を初期状態にリセットする堅牢な初期化システムを必ず実装します。例えば、起動時にマスターデータから値をコピーする、などの対策が考えられます。  
* 課題2：デバッグの複雑性  
  イベント駆動システムは、コンポーネント間の直接的な結合をなくす代償として、処理の流れが追跡しにくくなる傾向があります。あるイベントがどこで発行され、どのリスナーがどのように応答したのかを、従来のコールスタックのように直線的に追うことが困難になります。  
  * **緩和策**:  
    1. **カスタムエディタツールの開発**: デバッグを容易にするための専用ツールへの投資は、極めて高いリターンをもたらします。例えば、GameEventアセットのカスタムインスペクタを実装し、現在そのイベントをリッスンしている全てのGameEventListenerをリスト表示し、クリックでシーン内のオブジェクトをハイライトできるようにします 3。  
    2. **イベントロガーの実装**: 全てのイベント発行をコンソールや専用のデバッグウィンドウに記録する中央ロギングシステムを導入します。これにより、イベントの発生順序やペイロードの内容を時系列で確認でき、問題の特定が容易になります。  
    3. **インスペクタからのイベント発行**: GameEventのインスペクタに「Raise」ボタンを追加し、エディタ上から手動でイベントを発行できるようにします。これにより、特定のイベントに対するシステムの応答を、ゲームを特定の状態まで進めることなく、単体でテストできます。  
* 課題3：過剰な断片化（「アセットスープ」現象）  
  SOの利便性に頼りすぎるあまり、あらゆる細かいデータやイベントに対して個別のSOアセットを作成してしまうと、プロジェクトフォルダが数百、数千の小さなアセットで溢れかえり、管理が困難になる「アセットスープ」と呼ばれる状態に陥る可能性があります。  
  * **緩和策**:  
    1. **明確な命名規則とフォルダ構造**: プロジェクトの初期段階で、アセットの命名規則と階層的なフォルダ構造を厳格に定めます。例えば、「Events/Player/OnHealthChanged.asset」や「Commands/Definitions/Items/Heal.asset」のように、役割とコンテキストが明確にわかるように整理します。  
    2. **イベントの粒度の見極め**: 全てのメソッド呼び出しをイベントに置き換えるのはやりすぎです。イベントは、システムの境界を越えるような、意味のある重要な通知に限定して使用します。モジュール内部の密な連携は、直接のメソッド呼び出しやC#のeventで十分な場合も多いです。  
    3. **データコンテナSOの活用**: 関連する複数の値を個別のSOにするのではなく、一つの「設定用SO」（例：PlayerSettingsSO）にまとめることで、アセットの数を適切に管理します。

### **5.3. Unity 6プロジェクトへの戦略的推奨事項**

以上の分析を踏まえ、Unity 6以降のプロジェクトでこのアーキテクチャを導入する際の戦略的な指針を以下に示します。

* ハイブリッドモデルを標準設計として採用する:  
  新規プロジェクトにおいて、このアーキテクチャの導入を検討する場合、第3章で詳述したC#の属性を活用するハイブリッドモデルを第一の選択肢とすべきです。これは、デザイナーの生産性とエンジニアリングの堅牢性という二つの要求を最も高いレベルで満たす、現時点での最適解です。  
* シンプルなCommandSO（アプローチA）の限定的な利用:  
  ハイブリッドモデルが標準であっても、CommandSOが有用な場面は存在します。パラメータを必要としない、完全に静的なアクション（例：特定のUIパネルを開く、ゲームをポーズする、シーンをロードするなど）については、このシンプルなアプローチを用いることで、実装の手間を省き、アセットの意図をより明確にすることができます。  
* 中央集権的なCommandInvokerを確立する:  
  複数のシステムがそれぞれ独立してコマンドを実行するような分散型の設計は避けるべきです。PlayerCommandInvokerやAICommandInvokerのように、コンテキストに応じて少数の中央CommandInvokerを設けることで、デバッグ、ロギング、履歴管理のための重要な**チョークポイント（関所）**が生まれます。このInvokerインスタンスを各システムに提供する方法としては、シングルトンパターンよりも、依存性注入（DI）フレームワークを用いることが、テスト容易性と疎結合の観点から強く推奨されます 30。  
* テスト容易性を前提に設計する:  
  このアーキテクチャは、本質的に高いテスト容易性を備えています。この利点を最大限に活かすべきです。  
  * 個々のICommandクラスは、依存関係が少ないため、単体テストが非常に容易です。  
  * CommandInvokerは、モックのコマンドオブジェクトを渡すことで、その履歴管理ロジックを単体でテストできます。  
  * 各ゲームシステムは、テストコードから直接GameEventを発行することで、その応答を検証できます。  
    プロジェクトの初期からテストを記述する文化を醸成することで、アーキテクチャの堅牢性を長期的に維持することができます。

総括すると、ScriptableObjectベースのイベントアーキテクチャとコマンドパターンを統合することは、単に技術的に「可能」であるだけでなく、現代の複雑なゲーム開発が直面する多くの課題に対する、非常に洗練された「戦略的解答」です。その親和性は極めて高く、正しく実装されたとき、それはプロジェクトに比類なき柔軟性、拡張性、そして保守性をもたらす強力な基盤となるでしょう。

#### **引用文献**

1. Architect your code for efficient changes and debugging with ScriptableObjects | Unity, 9月 3, 2025にアクセス、 [https://unity.com/how-to/architect-game-code-scriptable-objects](https://unity.com/how-to/architect-game-code-scriptable-objects)  
2. Separate Game Data and Logic with ScriptableObjects - Unity, 9月 3, 2025にアクセス、 [https://unity.com/how-to/separate-game-data-logic-scriptable-objects](https://unity.com/how-to/separate-game-data-logic-scriptable-objects)  
3. Use ScriptableObjects as event channels in game code - Unity, 9月 3, 2025にアクセス、 [https://unity.com/how-to/scriptableobjects-event-channels-game-code](https://unity.com/how-to/scriptableobjects-event-channels-game-code)  
4. Using Scriptable Object Event System architecture - Ask - GameDev.tv, 9月 3, 2025にアクセス、 [https://community.gamedev.tv/t/using-scriptable-object-event-system-architecture/240572](https://community.gamedev.tv/t/using-scriptable-object-event-system-architecture/240572)  
5. ScriptableObject Events In Unity (C# Tutorial) | Unity Scriptable Objects - YouTube, 9月 3, 2025にアクセス、 [https://www.youtube.com/watch?v=gXD2z_kkAXs](https://www.youtube.com/watch?v=gXD2z_kkAXs)  
6. Exploring Event-driven Programming in Unity: ScriptableObjects vs. UnityEvents - Medium, 9月 3, 2025にアクセス、 [https://medium.com/@sonusprocks/exploring-event-driven-programming-in-unity-scriptableobjects-vs-unityevents-90cec78d5de8](https://medium.com/@sonusprocks/exploring-event-driven-programming-in-unity-scriptableobjects-vs-unityevents-90cec78d5de8)  
7. Scriptable Objects Event - Medium, 9月 3, 2025にアクセス、 [https://medium.com/@kadircalliogluu/scriptable-objects-event-c71616bbfd75](https://medium.com/@kadircalliogluu/scriptable-objects-event-c71616bbfd75)  
8. ScriptableObject Game Events. Creating an Event System in Unity | by James Lafritz | Dev Genius, 9月 3, 2025にアクセス、 [https://blog.devgenius.io/scriptableobject-game-events-1f3401bbde72](https://blog.devgenius.io/scriptableobject-game-events-1f3401bbde72)  
9. Unity Architecture: Scriptable Object Pattern | by Simon Nordon | Medium, 9月 3, 2025にアクセス、 [https://medium.com/@simon.nordon/unity-architecture-scriptable-object-pattern-0a6c25b2d741](https://medium.com/@simon.nordon/unity-architecture-scriptable-object-pattern-0a6c25b2d741)  
10. Create modular game architecture in Unity with ScriptableObjects, 9月 3, 2025にアクセス、 [https://unity.com/resources/create-modular-game-architecture-with-scriptable-objects-ebook](https://unity.com/resources/create-modular-game-architecture-with-scriptable-objects-ebook)  
11. 6 ways ScriptableObjects can benefit your team and your code - Unity, 9月 3, 2025にアクセス、 [https://unity.com/blog/engine-platform/6-ways-scriptableobjects-can-benefit-your-team-and-your-code](https://unity.com/blog/engine-platform/6-ways-scriptableobjects-can-benefit-your-team-and-your-code)  
12. Command - Refactoring.Guru, 9月 3, 2025にアクセス、 [https://refactoring.guru/design-patterns/command](https://refactoring.guru/design-patterns/command)  
13. Command · Design Patterns Revisited - Game Programming Patterns, 9月 3, 2025にアクセス、 [https://gameprogrammingpatterns.com/command.html](https://gameprogrammingpatterns.com/command.html)  
14. The Command pattern with Scriptable Objects – Bronson Zgeb, 9月 3, 2025にアクセス、 [https://bronsonzgeb.com/index.php/2021/09/25/the-command-pattern-with-scriptable-objects/](https://bronsonzgeb.com/index.php/2021/09/25/the-command-pattern-with-scriptable-objects/)  
15. Use the command pattern for flexible and extensible game systems - Unity Learn, 9月 3, 2025にアクセス、 [https://learn.unity.com/tutorial/use-the-command-pattern-for-flexible-and-extensible-game-systems?uv=6&projectId=67bc8deaedbc2a23a7389cab](https://learn.unity.com/tutorial/use-the-command-pattern-for-flexible-and-extensible-game-systems?uv=6&projectId=67bc8deaedbc2a23a7389cab)  
16. Command Pattern in Unity - Undo functionality in 60 sec - YouTube, 9月 3, 2025にアクセス、 [https://m.youtube.com/shorts/6fiewFJQeVA](https://m.youtube.com/shorts/6fiewFJQeVA)  
17. Undo and Redo with the Command Pattern - C# and Unity - YouTube, 9月 3, 2025にアクセス、 [https://www.youtube.com/watch?v=LRZ1cuXiXTI](https://www.youtube.com/watch?v=LRZ1cuXiXTI)  
18. How do I go about creating an Undo system in a grid based game? : r/gamedev - Reddit, 9月 3, 2025にアクセス、 [https://www.reddit.com/r/gamedev/comments/1gwdrb3/how_do_i_go_about_creating_an_undo_system_in_a/](https://www.reddit.com/r/gamedev/comments/1gwdrb3/how_do_i_go_about_creating_an_undo_system_in_a/)  
19. Undo & Redo with the Command Pattern in Unity & C# | by Micha Davis | Nerd For Tech, 9月 3, 2025にアクセス、 [https://medium.com/nerd-for-tech/undo-redo-with-the-command-pattern-in-unity-c-d3b63beab7a4](https://medium.com/nerd-for-tech/undo-redo-with-the-command-pattern-in-unity-c-d3b63beab7a4)  
20. Event in Scriptable Objects : r/Unity3D - Reddit, 9月 3, 2025にアクセス、 [https://www.reddit.com/r/Unity3D/comments/sg07od/event_in_scriptable_objects/](https://www.reddit.com/r/Unity3D/comments/sg07od/event_in_scriptable_objects/)  
21. Use ScriptableObjects as Delegate Objects - Unity, 9月 3, 2025にアクセス、 [https://unity.com/how-to/scriptableobjects-delegate-objects](https://unity.com/how-to/scriptableobjects-delegate-objects)  
22. Game Architecture in Unity using Scriptable Objects. : r/Unity3D - Reddit, 9月 3, 2025にアクセス、 [https://www.reddit.com/r/Unity3D/comments/1h8ar7p/game_architecture_in_unity_using_scriptable/](https://www.reddit.com/r/Unity3D/comments/1h8ar7p/game_architecture_in_unity_using_scriptable/)  
23. Command Pattern : Runtime Commands or Scriptable Commands ..., 9月 3, 2025にアクセス、 [https://www.reddit.com/r/Unity3D/comments/x8nwuw/command_pattern_runtime_commands_or_scriptable/](https://www.reddit.com/r/Unity3D/comments/x8nwuw/command_pattern_runtime_commands_or_scriptable/)  
24. Scripting API: SerializeReference - Unity - Manual, 9月 3, 2025にアクセス、 [https://docs.unity3d.com/2020.1/Documentation/ScriptReference/SerializeReference.html](https://docs.unity3d.com/2020.1/Documentation/ScriptReference/SerializeReference.html)  
25. SerializeReference in Unity - by Aleksander Trępała - Medium, 9月 3, 2025にアクセス、 [https://medium.com/@trepala.aleksander/serializereference-in-unity-b4ee10274f48](https://medium.com/@trepala.aleksander/serializereference-in-unity-b4ee10274f48)  
26. Unity's lack of Interface serialization is a serious flaw. How do work it out ? : r/Unity3D, 9月 3, 2025にアクセス、 [https://www.reddit.com/r/Unity3D/comments/1csgpsr/unitys_lack_of_interface_serialization_is_a/](https://www.reddit.com/r/Unity3D/comments/1csgpsr/unitys_lack_of_interface_serialization_is_a/)  
27. Scripting API: SerializeReference - Unity - Manual, 9月 3, 2025にアクセス、 [https://docs.unity3d.com/ScriptReference/SerializeReference.html](https://docs.unity3d.com/ScriptReference/SerializeReference.html)  
28. Serialized Pipeline with SerializeReference - Flying Rat Tech Blog, 9月 3, 2025にアクセス、 [https://tech.flying-rat.studio/post/serialized-pipeline-unity.html](https://tech.flying-rat.studio/post/serialized-pipeline-unity.html)  
29. Serialize Reference, Serialize Field, Serializable and Scene Serialization in Unity | Unity Tutorial - YouTube, 9月 3, 2025にアクセス、 [https://www.youtube.com/watch?v=dt4bgxctVj0](https://www.youtube.com/watch?v=dt4bgxctVj0)  
30. Finally, a Unity Dependency Injection Framework That Just Works - YouTube, 9月 3, 2025にアクセス、 [https://www.youtube.com/watch?v=6bJmEnpxVoI](https://www.youtube.com/watch?v=6bJmEnpxVoI)  
31. Adic - Lightweight dependency injection container for Unity - GitHub, 9月 3, 2025にアクセス、 [https://github.com/intentor/adic](https://github.com/intentor/adic)  
32. Command Pattern in Unity, Part 3: Undo/Redo Functionality - YouTube, 9月 3, 2025にアクセス、 [https://www.youtube.com/watch?v=I1BocNFIkwI](https://www.youtube.com/watch?v=I1BocNFIkwI)  
33. Create modular and maintainable code with the observer pattern - Unity Learn, 9月 3, 2025にアクセス、 [https://learn.unity.com/tutorial/65de086fedbc2a06ac2aca58?uv=2022.3&projectId=65de084fedbc2a0699d68bfb](https://learn.unity.com/tutorial/65de086fedbc2a06ac2aca58?uv=2022.3&projectId=65de084fedbc2a0699d68bfb)  
34. Dependency Injection Using Unity - Resolve Dependency Of Dependencies - C# Corner, 9月 3, 2025にアクセス、 [https://www.c-sharpcorner.com/article/dependency-injection-using-unity-resolve-dependency-of-dependencies/](https://www.c-sharpcorner.com/article/dependency-injection-using-unity-resolve-dependency-of-dependencies/)  
35. Dependency injection - Unity documentation, 9月 3, 2025にアクセス、 [https://docs.unity.com/ugs/en-us/manual/cloud-code/manual/modules/how-to-guides/initialize-modules/dependency-injection](https://docs.unity.com/ugs/en-us/manual/cloud-code/manual/modules/how-to-guides/initialize-modules/dependency-injection)  
36. Initializing the Command Invoker in Command Pattern - Stack Overflow, 9月 3, 2025にアクセス、 [https://stackoverflow.com/questions/16815074/initializing-the-command-invoker-in-command-pattern](https://stackoverflow.com/questions/16815074/initializing-the-command-invoker-in-command-pattern)