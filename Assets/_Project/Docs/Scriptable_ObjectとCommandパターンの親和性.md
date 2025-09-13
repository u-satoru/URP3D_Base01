

# **Unity 6における相乗的アーキテクチャ：Scriptable Object駆動イベントとCommandパターンの統合**

## **序論：スケーラブルで疎結合なゲームアーキテクチャの探求**

現代の大規模ゲーム開発における中心的な課題は、プロジェクトがスケールするにつれて増大する複雑性の管理です。システム間の直接的な依存関係は、いわゆる「スパゲッティコード」を生み出し、コードベースを脆弱で、テストが困難で、変更に弱いものにします 1。デザインパターンは、学術的な演習ではなく、これらの共通の問題に対する、実戦で証明された解決策として位置づけられています 2。

本レポートでは、Unity 6環境における2つの強力なデザインパターン、すなわち**Scriptable Object (SO) を利用したイベント駆動アーキテクチャ**と**Commandパターン**の組み合わせについて分析します。SOイベントアーキテクチャは、特にプログラマー以外のチームメンバーに力を与え、システム間の疎結合を実現する強力なメカニズムを提供します 3。一方、Commandパターンは、アクション（操作）をオブジェクトとしてカプセル化し、アンドゥ・リドゥ（Undo/Redo）やアクションキューイングといった高度な機能の実装を可能にする堅牢な手法です 5。

本レポートの主題は、これら2つのパターンの親和性が非常に高いという点にあります。適切に組み合わせることで、イベントが「意図」を通知し、コマンドが状態変更を制御されたトランザクションとして実行する、極めてモジュール性が高く、拡張可能で、保守性に優れたアーキテクチャを構築できます。この親和性は偶然の産物ではありません。両パターンは、「間接層を導入して密結合を解消する」という共通の設計思想に基づいています。SOイベントアーキテクチャでは、「イベントチャネル」というアセットが発行者と購読者の仲介役となり、互いの存在を意識させずに通信を可能にします 1。同様に、Commandパターンでは、

ICommandインターフェースを実装したオブジェクトが、操作の呼び出し元（Invoker）と実行者（Receiver）を分離します 8。どちらのケースも、「メッセージ」や「メソッド呼び出し」といった概念を、独立して受け渡しや保存が可能な有形のオブジェクトに変換する「具体化（Reification）」というアプローチを用いています。この共通の設計思想が、両者の統合を自然かつ強力なものにしているのです。

本レポートでは、まず各パターンを個別に深く掘り下げ、次に両者を統合した際の詳細な設計図を示します。さらに、実際の製品開発レベルで直面する課題や代替アーキテクチャとの比較分析を通じて、アーキテクチャ選定のための戦略的な指針を提供します。

## **第1章 Scriptable Objectイベントアーキテクチャの疎結合力**

### **1.1 中核原則：通信ハブとしてのScriptable Object**

Scriptable Object（SO）は、Unityにおけるシリアライズ可能なデータコンテナであり、シーンやGameObjectインスタンスから独立したアセットとしてプロジェクト内に存在します 3。この永続性と独立性が、SOをアーキテクチャの根幹をなす強力なツールたらしめる鍵です。

当初、SOは武器のステータスのような静的データを格納するために用いられていました。しかし、その応用範囲は大きく広がり、現在では動的なメッセージバス、すなわち「イベントチャネル」として活用されるようになりました 7。これは、2017年のUniteカンファレンスでRyan Hipple氏が提唱して以来、広く知られるようになったアーキテクチャです 12。このアプローチでは、SOがシステム間の通信を仲介するハブとして機能し、コンポーネント同士が互いを直接参照することなく連携できます。

### **1.2 実装設計図：チャネルとリスナー**

このアーキテクチャは、主に2つのコンポーネントで構成されます。

#### **1.2.1 GameEvent Scriptable Object**

イベントチャネルの本体となるSOです。通常、リスナーのリスト（System.ActionやUnityEventで実装）と、それらを呼び出すためのRaise()メソッドを持ちます 1。

C#

```csharp
// 例：引数なしの基本的なGameEvent  
using UnityEngine;  
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Void Event Channel")]  
public class VoidEventChannelSO : ScriptableObject  
{  
    public UnityAction OnEventRaised;

    public void RaiseEvent()  
    {  
        OnEventRaised?.Invoke();  
    }  
}
```

#### **1.2.2 GameEventListener MonoBehaviour**

イベントを購読し、応答するコンポーネントです。このMonoBehaviourは、特定のGameEvent SOへの参照と、イベント発生時に実行されるUnityEventを保持します。自身のOnEnableメソッドでイベントに購読し、OnDisableメソッドで購読を解除するのが一般的です 7。

C#

```csharp
// 例：VoidEventChannelSOを購読するリスナー  
using UnityEngine;  
using UnityEngine.Events;

public class VoidEventListener : MonoBehaviour  
{  
    private VoidEventChannelSO eventChannel;  
    private UnityEvent onEventRaised;

    private void OnEnable()  
    {  
        if (eventChannel!= null)  
        {  
            eventChannel.OnEventRaised += Respond;  
        }  
    }

    private void OnDisable()  
    {  
        if (eventChannel!= null)  
        {  
            eventChannel.OnEventRaised -= Respond;  
        }  
    }

    private void Respond()  
    {  
        onEventRaised.Invoke();  
    }  
}
```

このシステムは、例えばプレイヤーキャラクターがPlayerDiedEvent SOのRaiseEvent()を呼び出すと、同じアセットを購読しているUIマネージャー、オーディオマネージャー、敵AIなどが、プレイヤーオブジェクトへの直接参照なしに、それぞれの応答（ゲームオーバー画面の表示、専用サウンドの再生など）を実行することを可能にします 12。

### **1.3 アーキテクチャ上の利点**

このアーキテクチャは、いくつかの重要な利点をもたらします。

* **極めて高い疎結合性**: システムは直接的な依存関係なしに通信できるため、モジュール性と再利用性が向上します 1。例えば、インベントリシステムはUIシステムの存在を知る必要がなく、ただ  
  OnItemAddedイベントを発行するだけです 3。  
* **デザイナーと開発者のワークフロー改善**: デザイナーがInspector上でGameEventアセットをリスナーに割り当てることで、プログラマーの介入なしにゲームロジックを構築できます。これにより、ラピッドプロトタイピングとイテレーションが加速します 4。  
* **シーンからの独立とテスト容易性**: SOはプロジェクトアセットであるため、シーンをまたいだ通信を容易にします。また、モックイベントを発行するだけで個々のシステムを分離してテストすることが容易になります 3。

### **1.4 固有の課題と注意点**

強力な利点の一方で、いくつかの課題も存在します。

* **可読性とデバッグの困難さ**: 直接参照がないため、ロジックの流れを追跡することが非常に困難になる場合があります。IDEの「参照を検索」機能は、イベントリスナーを見つけるのには役立ちません 16。これを解決するには、カスタムツールの開発が不可欠です。  
* **アセットの増殖**: 複雑なゲームでは、数百ものGameEvent SOアセットが作成され、プロジェクトの整理が課題となる可能性があります 16。  
* **エディタとビルド版の挙動の違い**: ランタイム中にSOのデータを変更した場合、その変更はエディタ上ではプレイセッションが終了するまで維持されますが、スタンドアロンビルドではアプリケーションを再起動するとリセットされます。これは、このパターンに不慣れな開発者にとって混乱やバグの原因となりがちです 11。

このアーキテクチャの最も深遠な影響は、技術的な側面（疎結合）以上に、チームのワークフローと構造そのものを変革する点にあります。このパターンは、事実上「契約ベース」の開発環境を構築します。プログラマーは利用可能なイベント（PlayerJumpedEventなど）と応答（リスナーコンポーネントのメソッド）を定義し、これが一種の公開API、すなわち「契約」となります。デザイナーやレベルビルダーは、Unityエディタ内でこれらの契約を消費し、イベントと応答をリンクさせることで、コードを記述することなく複雑なインタラクションを構築できます 4。これにより、プログラマーが新しいコアシステムを構築している間に、デザイナーが既存のイベント契約を利用してシネマティックシーケンスを作成するといった、ワークフローの並列化が実現します。これはゲーム開発における主要なボトルネックの一つである「ゲームプレイロジックの接続におけるプログラマーへの依存」を解消し、チーム全体の開発速度と自律性を向上させるのです。

## **第2章 Commandパターン：アクションをオブジェクトとしてカプセル化する**

### **2.1 中核原則：メソッド呼び出しの具体化**

Commandパターンの核心は、リクエスト（要求）を、そのリクエストに関するすべての情報を含むスタンドアロンオブジェクトに変換することです 8。これはしばしば「メソッド呼び出しの具体化（Reified Method Call）」と表現されます 9。このパターンは、主に4つの役割から構成されます。

* **Command**: Execute()メソッド（多くの場合Undo()メソッドも）を持つインターフェースまたは抽象クラス 5。  
* **Concrete Command**: 特定のアクションをカプセル化し、実行者（Receiver）と必要なパラメータへの参照を保持する実装クラス 6。  
* **Receiver**: 実際の処理を実行するオブジェクト（例：PlayerMotorクラス） 9。  
* **Invoker**: コマンドを起動するオブジェクト（例：InputHandler）。アンドゥ・リドゥのためにコマンドの履歴を保持する場合もあります 5。

### **2.2 ゲーム開発における典型的なユースケース**

Commandパターンは、ゲーム開発の様々な場面でその価値を発揮します。

* **アンドゥ・リドゥシステム**: 最も古典的な例です。実行されたコマンドをスタックに保存することで、最後に追加されたコマンドを取り出してそのUndo()メソッドを呼び出すだけで、アンドゥ操作を簡単に実装できます 5。  
* **アクションキューイング**: リアルタイムストラテジー（RTS）やターン制ゲームにおいて、プレイヤーやAIのアクションをコマンドオブジェクトのリストとしてキューに追加し、順次実行させることができます 5。  
* **入力処理**: 入力デバイスとプレイヤーのアクションを分離します。キーが押されるとJumpCommandが生成され、プレイヤーキャラクターによって実行されます。これにより、キーコンフィグ機能や複数デバイスのサポートが容易になります 24。  
* **ネットワークとリプレイ**: コマンドオブジェクトはシリアライズしてネットワーク経由で送信できるため、全クライアントで決定論的なシミュレーションを保証できます。また、コマンドの履歴を保存・再生することでリプレイシステムを構築できます 9。  
* **AIシステム**: AIの意思決定コードがMoveToPositionCommandやAttackTargetCommandといったコマンドオブジェクトを生成し、それをアクタのコマンドキューに渡すことで、AIの「頭脳」と「身体」を分離できます 9。

### **2.3 アーキテクチャ上の利点**

* **InvokerとReceiverの分離**: InputHandlerはPlayerMotorの存在を知る必要がなく、ICommandインターフェースのみに依存します 8。  
* **単一責任の原則とオープン・クローズドの原則**: 各コマンドクラスは単一の責任を持ちます。既存のInvokerやReceiverのコードを変更することなく、新しいコマンドを追加できます 8。  
* **アクションの第一級オブジェクト化**: アクションがオブジェクトになることで、保存、受け渡し、操作、そして後からの実行が可能となり、非常に高い柔軟性が得られます 9。

### **2.4 実装コストと考慮事項**

* **コードの冗長性**: 主な欠点はクラスの増殖です。個別のアクションごとに具象コマンドクラスを作成する必要があり、多くの定型的なコード（ボイラープレート）を生み出す可能性があります 28。  
* **パフォーマンスオーバーヘッド**: アクションが発生するたびに新しいクラスオブジェクトをインスタンス化することは、特に頻繁に（例えば毎フレーム）発生するアクションの場合、ガベージコレクション（GC）の負荷を高める可能性があります 29。

Commandパターンは、単なる疎結合化以上の価値をもたらします。それは、ゲームロジックに「トランザクション」の概念を導入することです。通常のメソッド呼び出しはオブジェクトの状態を直接変更しますが、処理の途中で失敗した場合、状態が不正なまま残される危険性があります。一方、コマンドオブジェクトは、完結した原子的な操作をカプセル化します。Execute()メソッドがトランザクション全体を表し、Undo()メソッドの存在がこのトランザクションを「可逆的」なものにします。これにより、単純なアクションが、安全で、可逆的で、監査可能な「作業単位」へと昇華されます。この厳密さこそが、レベルエディタや複雑なストラテジーゲームのように、決定の取り消しが中核的なメカニクスとなる堅牢なシステムを可能にするのです。

## **第3章 統合：ハイブリッド・イベント・コマンドアーキテクチャ**

### **3.1 概念的基盤：イベントとコマンドの区別**

クリーンなアーキテクチャを構築するためには、イベントとコマンドの違いを明確に理解することが最も重要です。

* **イベントは通知（過去形）**: イベントは、何かが「既に起こった」ことを示します。これは事実の表明です。例：PlayerDied（プレイヤーが死亡した）、ItemCollected（アイテムが収集された）。一つのイベントに対して、複数のシステムが反応することも、全く反応しないこともあります 30。  
* **コマンドは要求（命令形）**: コマンドは、何かを「実行せよ」という指示です。明確な意図を持ち、通常はただ一つのシステムによって処理されることが期待されます。例：MovePlayer（プレイヤーを動かせ）、FireWeapon（武器を発射せよ） 31。

この区別に基づくと、堅牢な処理フローは **入力 → コマンド → 状態変化 → イベント** となります。例えば、「W」キーが押される（入力）とMoveForwardCommandが生成され（コマンド）、それがプレイヤーの位置を変更し（状態変化）、その結果としてオーディオシステムが足音を再生するために購読しているPlayerMovedEventが発行される（イベント）、という流れです。

### **3.2 アーキテクチャ設計図1：イベントをトリガーとしたコマンドのインスタンス化**

これは最も柔軟で一般的な統合パターンです。UIや入力ハンドラなどのシステムが、汎用的なSOイベントを発行します。このイベントを、「コマンドファクトリ」や「コントローラー」といった専用のリスナーが購読します。このリスナーの唯一の責任は、イベント（とそのペイロード）を現在のゲームコンテキストで解釈し、適切なCommandオブジェクトをインスタンス化して、CommandInvokerやキューにディスパッチすることです。

**処理フローの例:**

1. UIボタンのOnClick()がPrimaryActionButtonClickedEvent（SO）を発行する。  
2. PlayerInputControllerがこのイベントを購読している。  
3. コントローラーはプレイヤーの状態を確認する。もしプレイヤーが武器を構えていればnew FireWeaponCommand(player, target)を生成し、ドアの近くにいればnew OpenDoorCommand(door)を生成する。  
4. 新しく生成されたコマンドはCommandManager.ExecuteCommand(command)に渡される。

このパターンの利点は、ユーザーの意図を具体的なアクションに変換するロジックを一元管理できる点にあります。非常に柔軟でコンテキストに応じた対応が可能であり、コマンドのロジックをプログラマーが管理しやすいC#コード内に留めることができます。

### **3.3 アーキテクチャ設計図2：コマンドとしてのScriptable Object**

より直接的でデータ駆動なアプローチです。このパターンでは、Commandクラス自体がScriptableObjectを継承し、コマンドを再利用可能なプロジェクトアセットとして扱います。

実装:  
public abstract void Execute();メソッドを持つ抽象クラスCommandSO : ScriptableObjectを作成します。JumpCommandSOのような具象コマンドはこれを継承し、[CreateAssetMenu]属性を用いてプロジェクト内でアセットとして作成します 27。  
**処理フローの例:**

1. PlayerController MonoBehaviourがpublic CommandSO jumpAbility;というフィールドを持つ。  
2. デザイナーはInspector上でPlayerJumpCommand.assetをこのスロットにドラッグ＆ドロップする。  
3. ジャンプボタンが押されると、コードは単純にjumpAbility.Execute()を呼び出す。

このアプローチは、デザイナーにとって非常に使いやすいという大きな利点があります。アビリティ、アクション、ビヘイビアなどを、参照するSOアセットを変更するだけでデザイン時に交換できます 27。設定可能な武器アビリティやダイアログアクションのようなシステムに最適です 34。

これら2つの設計図の選択は、単なる技術的な実装の詳細ではなく、「アーキテクチャの制御と柔軟性をどこに配置するか」という戦略的な決定です。設計図1（イベント→コマンドファクトリ）は**プログラマー中心**であり、どのコマンドを実行するかのロジックはC#コード内に留まります。これにより、複雑な状態依存ロジックに対して最大の制御力を持ちますが、変更にはプログラマーの作業が必要です。一方、設計図2（SO as Command）は**デザイナー中心**であり、ロジックはSOアセット自体にカプセル化され、どのロジックを実行するかの選択はInspectorに直接公開されます。これにより、デザイナーはコード変更なしに振る舞いを再構成できますが、単純なアセットの交換では表現しにくい動的な判断には不向きです。成熟したアーキテクチャでは、両者をハイブリッドで採用することが多いでしょう。例えば、ボタンに割り当てる魔法の種類のような単純で交換可能なアクションには設計図2を、ゲームの状態を広範に問い合わせてからコマンドを発行する必要がある複雑な入力処理には設計図1を使用するなど、プログラム的なパワーとデザイン時の柔軟性の間で意図的なトレードオフを行うことが求められます。

## **第4章 高度なトピックと製品レベルでの適用**

### **4.1 パフォーマンスと最適化**

すべてのアクションに対してnew MyCommand()を実行することは、GCスパイクを引き起こす可能性があります。これは、特に毎フレーム処理される移動入力のような高頻度のコマンドで問題となります 29。

* **緩和戦略1：コマンドプーリング**: オブジェクトプールパターンを導入します。newの代わりにプールからコマンドオブジェクトを要求し、実行後にプールに返却します。これにより、コマンドオブジェクトのGC負荷をなくすことができます。  
* **緩和戦略2：アロケーションフリーなコマンド（Struct）**: 究極のパフォーマンスを求める場合、コマンドをclassではなくstructとして実装し、値渡しすることでヒープアロケーションを完全に回避できます。これはパフォーマンスが重要な箇所に適した高度なテクニックですが、特にUndoのための状態管理が複雑になります 29。  
* **UnityEvent vs. C# Action**: デザイナーフレンドリーなUnityEventは、ネイティブのC#デリゲート（System.Action）よりもパフォーマンスオーバーヘッドが大きい点に注意が必要です。毎フレーム何度も発行されるようなパフォーマンスが重要なイベントには、SO内で純粋なC# Actionを使用することが望ましいです 35。

### **4.2 大規模プロジェクトにおける複雑性の管理**

多数のSOアセットを管理するためには、具体的な戦略が必要です。

* **組織戦略**:  
  * **フォルダ構造**: Assets/Events/Player/、Assets/Events/UI/のように機能ごとに整理する。  
  * **命名規則**: EVT_Player_Died.asset、CMD_Player_Jump.assetのように接頭辞を用いて種類を明確にする。  
* **カスタムツール**: 大規模プロジェクトでは、カスタムツールの開発は必須です。  
  * **イベントビジュアライザー**: イベントSOを選択すると、プロジェクト内のすべての発行者とリスナーをグラフ表示するカスタムエディタウィンドウ。これにより、可読性の問題を解決します 20。  
  * **カスタムインスペクター**: GameEventのインスペクターを拡張し、現在のランタイムリスナーのリストを表示させ、クリックするとヒエラルキー上のGameObjectをハイライトする機能を追加します 7。

### **4.3 デバッグとトレーサビリティ**

イベントが発行されると、リスナーのスタックトレースはUnityEvent.Invoke()から始まるため、イベントを最初に発行したコンテキストが失われるという問題があります。

* **解決策1：ペイロードの拡充**: イベントを発行するオブジェクトが自身（this）をパラメータとして渡すなど、デバッグ情報を含むようにイベントを設計します。  
* **解決策2：カスタムロギング**: すべての主要なイベントを購読し、その発行と処理をログに記録する中央集権的なDebugManagerを作成します。これにより、ゲームのフローを時系列で明確に追跡できます。

このような高度に疎結合なアーキテクチャを採用するという決定は、暗黙のうちにカスタムツールへの投資を決定することと同義です。このアーキテクチャの主な弱点である可読性やデバッグの困難さは、さらなるコードパターンによってではなく、より優れたツールによって解決されます 16。経験の浅いチームは、このアーキテクチャの利点に惹かれて採用し、プロジェクトがスケールするにつれてその欠点に苦しむかもしれません。しかし、経験豊富なチームは、これらの欠点が疎結合に内在するものであることを理解しています。彼らは、この強力なアーキテクチャのコストが、初期設定だけでなく、疎結合によって失われた可視性を回復するためのツールを継続的に構築・維持することによって支払われるべきだと知っています。したがって、このアーキテクチャを含むプロジェクト計画には、「イベントデバッガ」や「コマンド履歴ビューア」の開発タスクも含まれなければなりません。このツール開発の予算を怠ることが、このようなアーキテクチャが製品開発で失敗し、「管理不能な依存地獄」となる主要な理由の一つです 20。アーキテクチャとその支援ツールは、不可分一体のパッケージなのです。

## **第5章 比較分析と戦略的推奨事項**

### **5.1 代替アーキテクチャ**

* **静的イベントバス／マネージャー**: 静的なシングルトンクラスがすべてのイベントを管理する一般的な代替案です 7。  
  * *長所*: 実装が単純で、アセットが散らからない。  
  * *短所*: デザイナーフレンドリーではなく、巨大なゴッドオブジェクトになりがち。オープン・クローズドの原則に違反する。  
* **依存性注入（DI）フレームワーク（例：Zenject/Extenject）**: Inspectorでの手動接続を避け、依存性を自動的に注入します 20。  
  * *長所*: 疎結合に非常に強力で、テスト容易性（モック化）を促進する。  
  * *短所*: 学習コストが高く、動作が「魔法のように」感じられデバッグが困難な場合がある。サードパーティへの依存が発生する。  
* **リアクティブプログラミング（UniRx）**: オブザーバブルストリームを用いて、時間経過に伴うイベントや状態変化を扱います 17。  
  * *長所*: 複雑な非同期イベントシーケンスやUI更新に対してエレガントな解決策を提供する。  
  * *短所*: プログラミングパラダイムが大きく異なり（関数型リアクティブプログラミング）、学習コストが高い。

### **5.2 アーキテクチャパターンのトレードオフマトリクス**

以下の表は、開発者が情報に基づいた意思決定を行うための一助となるよう、各アーキテクチャを主要な評価基準で比較したものです。

| 評価基準 | SOイベント + Command | 静的イベントバス | 依存性注入 (Zenject) | リアクティブ (UniRx) |
| :---- | :---- | :---- | :---- | :---- |
| **疎結合性** | 非常に良い | 良い | 非常に良い | 非常に良い |
| **パフォーマンス** | 良い（プーリング併用時） | 非常に良い | とても良い | 良い |
| **デザイナー親和性** | 非常に良い | 悪い | 悪い | 悪い |
| **コードの複雑性** | 中（定型コード） | 低 | 高（初期設定） | 高（パラダイム） |
| **テスト容易性** | とても良い | 普通 | 非常に良い | 良い |
| **スケーラビリティ** | 良い（ツール必須） | 悪い（モノリス化リスク） | 非常に良い | 非常に良い |
| **可読性** | 悪い（ツール必須） | 良い（中央集権） | 普通（魔法的な注入） | 普通（ストリームの複雑性） |

### **5.3 意思決定フレームワーク：このハイブリッドアーキテクチャを使用すべき時**

* **推奨されるケース**:  
  * プログラマー以外のチームメンバーがロジック作成に深く関わる必要があるプロジェクト（例：複雑なRPG、ナラティブゲーム、システム駆動型ゲーム）。  
  * アンドゥ・リドゥ、アクションキューイング、リプレイなどの高度な機能を必要とするゲーム。  
  * 厳格な組織的規約を維持する規律と、カスタムツールに投資するリソースを持つチーム。  
* **注意が必要なケース**:  
  * アーキテクチャのオーバーヘッドが不要な小規模で単純なプロジェクトやプロトタイプ 2。  
  * あらゆるアロケーションや間接参照が問題となる、パフォーマンスが極めて重要なゲーム（ただし緩和策は存在する）。  
  * デザインパターンに不慣れなチームや、必要なツールと規約に投資する意思のないチーム。

## **結論：強力だが要求の厳しいシナジー**

本レポートの最終的な結論として、Scriptable Objectベースのイベント駆動アーキテクチャとCommandパターンの親和性は非常に高く、単に互換性があるだけでなく、相互補完的な関係にあると断言できます。イベントが「何が起こったか」を伝える神経系として機能し、コマンドがアクションを制御された拡張可能な方法で実行する筋肉系として機能します。

このアーキテクチャがもたらす最大のトレードオフは、比類なき柔軟性とデザイナーへの権限委譲を、複雑性の増大と、カスタムツールおよび厳格なチーム規律の絶対的な必要性と引き換えに得ることです。

このアーキテクチャの導入を検討するチームへの実践的な推奨事項は以下の通りです。

1. **イベントとコマンドの明確な区別**: コードベース内で、イベント（過去の事実の通知）とコマンド（未来のアクションの要求）の役割を厳密に区別する。  
2. **適切な統合モデルの選択**: プログラム的な制御が必要か、データ駆動の制御が必要かに応じて、適切な統合モデル（イベントからファクトリへ、またはSO自体をコマンドとして扱う）を選択する。  
3. **早期のツールへの投資**: コアシステムを構築した直後のタスクとして、デバッガやビジュアライザーの構築に着手する。  
4. **パフォーマンスへの配慮**: コードをプロファイリングし、高頻度のアクションにはコマンドプーリングを実装する。  
5. **厳格な命名・組織規約の徹底**: プロジェクト初日から厳格な規約を施行する。これは将来の保守性を大きく左右する。

#### **引用文献**

1. Decoupled Unity Events with Scriptable Objects: A Clean Architecture - Wayline, 8月 30, 2025にアクセス、 [https://www.wayline.io/blog/decoupled-unity-events-scriptable-objects](https://www.wayline.io/blog/decoupled-unity-events-scriptable-objects)  
2. Level up your code with game programming patterns - Unity, 8月 30, 2025にアクセス、 [https://unity.com/blog/games/level-up-your-code-with-game-programming-patterns](https://unity.com/blog/games/level-up-your-code-with-game-programming-patterns)  
3. Architect your code for efficient changes and debugging with ScriptableObjects | Unity, 8月 30, 2025にアクセス、 [https://unity.com/how-to/architect-game-code-scriptable-objects](https://unity.com/how-to/architect-game-code-scriptable-objects)  
4. 6 ways ScriptableObjects can benefit your team and your code - Unity, 8月 30, 2025にアクセス、 [https://unity.com/blog/engine-platform/6-ways-scriptableobjects-can-benefit-your-team-and-your-code](https://unity.com/blog/engine-platform/6-ways-scriptableobjects-can-benefit-your-team-and-your-code)  
5. Use the command pattern for flexible and extensible game systems - Unity Learn, 8月 30, 2025にアクセス、 [https://learn.unity.com/tutorial/use-the-command-pattern-for-flexible-and-extensible-game-systems?uv=6&projectId=67bc8deaedbc2a23a7389cab](https://learn.unity.com/tutorial/use-the-command-pattern-for-flexible-and-extensible-game-systems?uv=6&projectId=67bc8deaedbc2a23a7389cab)  
6. Use the command pattern for flexible and extensible game systems - Unity Learn, 8月 30, 2025にアクセス、 [https://learn.unity.com/tutorial/use-the-command-pattern-for-flexible-and-extensible-game-systems](https://learn.unity.com/tutorial/use-the-command-pattern-for-flexible-and-extensible-game-systems)  
7. Use ScriptableObjects as event channels in game code - Unity, 8月 30, 2025にアクセス、 [https://unity.com/how-to/scriptableobjects-event-channels-game-code](https://unity.com/how-to/scriptableobjects-event-channels-game-code)  
8. Command - Refactoring.Guru, 8月 30, 2025にアクセス、 [https://refactoring.guru/design-patterns/command](https://refactoring.guru/design-patterns/command)  
9. Command · Design Patterns Revisited - Game Programming Patterns, 8月 30, 2025にアクセス、 [https://gameprogrammingpatterns.com/command.html](https://gameprogrammingpatterns.com/command.html)  
10. Scripting API: ScriptableObject - Unity - Manual, 8月 30, 2025にアクセス、 [https://docs.unity3d.com/ScriptReference/ScriptableObject.html](https://docs.unity3d.com/ScriptReference/ScriptableObject.html)  
11. Unity Architecture: Scriptable Object Pattern | by Simon Nordon | Medium, 8月 30, 2025にアクセス、 [https://medium.com/@simon.nordon/unity-architecture-scriptable-object-pattern-0a6c25b2d741](https://medium.com/@simon.nordon/unity-architecture-scriptable-object-pattern-0a6c25b2d741)  
12. Using Scriptable Object Event System architecture - Ask - GameDev.tv, 8月 30, 2025にアクセス、 [https://community.gamedev.tv/t/using-scriptable-object-event-system-architecture/240572](https://community.gamedev.tv/t/using-scriptable-object-event-system-architecture/240572)  
13. Unite Austin 2017 - Game Architecture with Scriptable Objects - YouTube, 8月 30, 2025にアクセス、 [https://www.youtube.com/watch?v=raQ3iHhE_Kk](https://www.youtube.com/watch?v=raQ3iHhE_Kk)  
14. DanielEverland/ScriptableObject-Architecture: Makes using Scriptable Objects as a fundamental part of your architecture in Unity super easy - GitHub, 8月 30, 2025にアクセス、 [https://github.com/DanielEverland/ScriptableObject-Architecture](https://github.com/DanielEverland/ScriptableObject-Architecture)  
15. Using Scriptable Objects for Events | by Natalia DaLomba - Dev Genius, 8月 30, 2025にアクセス、 [https://blog.devgenius.io/using-scriptable-objects-for-events-1e035ae60ee4](https://blog.devgenius.io/using-scriptable-objects-for-events-1e035ae60ee4)  
16. Game Architecture in Unity using Scriptable Objects. : r/Unity3D - Reddit, 8月 30, 2025にアクセス、 [https://www.reddit.com/r/Unity3D/comments/1h8ar7p/game_architecture_in_unity_using_scriptable/](https://www.reddit.com/r/Unity3D/comments/1h8ar7p/game_architecture_in_unity_using_scriptable/)  
17. Event Driven Programming In Unity : r/Unity3D - Reddit, 8月 30, 2025にアクセス、 [https://www.reddit.com/r/Unity3D/comments/67d6jo/event_driven_programming_in_unity/](https://www.reddit.com/r/Unity3D/comments/67d6jo/event_driven_programming_in_unity/)  
18. I don't get the "ScriptableObjects are magical for decoupling" concept. Help? - Reddit, 8月 30, 2025にアクセス、 [https://www.reddit.com/r/Unity3D/comments/1c2j25t/i_dont_get_the_scriptableobjects_are_magical_for/](https://www.reddit.com/r/Unity3D/comments/1c2j25t/i_dont_get_the_scriptableobjects_are_magical_for/)  
19. Event Manager script or Scriptable objects events : r/unity - Reddit, 8月 30, 2025にアクセス、 [https://www.reddit.com/r/unity/comments/16bjm98/event_manager_script_or_scriptable_objects_events/](https://www.reddit.com/r/unity/comments/16bjm98/event_manager_script_or_scriptable_objects_events/)  
20. Singleton vs Dependency Injection vs Service Locator vs Scriptable Objects : r/Unity3D, 8月 30, 2025にアクセス、 [https://www.reddit.com/r/Unity3D/comments/13ooktu/singleton_vs_dependency_injection_vs_service/](https://www.reddit.com/r/Unity3D/comments/13ooktu/singleton_vs_dependency_injection_vs_service/)  
21. Be CAREFUL with Scriptable Objects! - Unity Code Monkey, 8月 30, 2025にアクセス、 [https://unitycodemonkey.com/video_comments.php?v=5a-ztc5gcFw](https://unitycodemonkey.com/video_comments.php?v=5a-ztc5gcFw)  
22. Command Pattern in Unity - YouTube, 8月 30, 2025にアクセス、 [https://m.youtube.com/watch?v=f7X9gdUmhMY&pp=0gcJCa0JAYcqIYzv](https://m.youtube.com/watch?v=f7X9gdUmhMY&pp=0gcJCa0JAYcqIYzv)  
23. Implementation of RTS style command pattern : r/Unity3D - Reddit, 8月 30, 2025にアクセス、 [https://www.reddit.com/r/Unity3D/comments/1150npj/implementation_of_rts_style_command_pattern/](https://www.reddit.com/r/Unity3D/comments/1150npj/implementation_of_rts_style_command_pattern/)  
24. Programming Patterns for Games: Command - Evaluating Unity's Input Handling Options, 8月 30, 2025にアクセス、 [https://dev.to/zigzagoon1/programming-patterns-for-games-command-evaluating-unitys-input-handling-options-1dee](https://dev.to/zigzagoon1/programming-patterns-for-games-command-evaluating-unitys-input-handling-options-1dee)  
25. Game programming patterns in Unity with C# - Command Pattern - Habrador, 8月 30, 2025にアクセス、 [https://www.habrador.com/tutorials/programming-patterns/1-command-pattern/](https://www.habrador.com/tutorials/programming-patterns/1-command-pattern/)  
26. Using the Command design pattern for game AI : r/gamedev - Reddit, 8月 30, 2025にアクセス、 [https://www.reddit.com/r/gamedev/comments/9jry3o/using_the_command_design_pattern_for_game_ai/](https://www.reddit.com/r/gamedev/comments/9jry3o/using_the_command_design_pattern_for_game_ai/)  
27. The Command pattern with Scriptable Objects - Bronson Zgeb, 8月 30, 2025にアクセス、 [https://bronsonzgeb.com/index.php/2021/09/25/the-command-pattern-with-scriptable-objects/](https://bronsonzgeb.com/index.php/2021/09/25/the-command-pattern-with-scriptable-objects/)  
28. Design Patterns: The Command Pattern | by Wen Junhua - Medium, 8月 30, 2025にアクセス、 [https://medium.com/@wenjh1998/design-patterns-the-command-pattern-4d0642e1e5ed](https://medium.com/@wenjh1998/design-patterns-the-command-pattern-4d0642e1e5ed)  
29. Command Pattern Allocation Free in C# Unity / Godot (without memory allocations), 8月 30, 2025にアクセス、 [https://medium.com/@swiftroll3d/command-pattern-allocation-free-in-c-unity-godot-without-memory-allocations-1b60cfcbd4e2](https://medium.com/@swiftroll3d/command-pattern-allocation-free-in-c-unity-godot-without-memory-allocations-1b60cfcbd4e2)  
30. Event Queue - Game Programming Patterns, 8月 30, 2025にアクセス、 [https://gameprogrammingpatterns.com/event-queue.html](https://gameprogrammingpatterns.com/event-queue.html)  
31. Why are commands and events separately represented? - Stack Overflow, 8月 30, 2025にアクセス、 [https://stackoverflow.com/questions/4962755/why-are-commands-and-events-separately-represented](https://stackoverflow.com/questions/4962755/why-are-commands-and-events-separately-represented)  
32. Event-Driven Architecture: Know your commands from your events - Master Serverless, 8月 30, 2025にアクセス、 [https://newsletter.theburningmonk.com/posts/event-driven-architecture-know-your-commands-from-your-events](https://newsletter.theburningmonk.com/posts/event-driven-architecture-know-your-commands-from-your-events)  
33. I'm not sure that i'm following all the best practices, but Scriptable Objects is my love now : r/Unity3D - Reddit, 8月 30, 2025にアクセス、 [https://www.reddit.com/r/Unity3D/comments/1lsbczg/im_not_sure_that_im_following_all_the_best/](https://www.reddit.com/r/Unity3D/comments/1lsbczg/im_not_sure_that_im_following_all_the_best/)  
34. Whats the best solution for triggering scripts from dialogue or "events" in a generic way?, 8月 30, 2025にアクセス、 [https://www.reddit.com/r/Unity3D/comments/1he9hya/whats_the_best_solution_for_triggering_scripts/](https://www.reddit.com/r/Unity3D/comments/1he9hya/whats_the_best_solution_for_triggering_scripts/)  
35. UnityEvent performance overhead, a cause for concern? : r/Unity3D - Reddit, 8月 30, 2025にアクセス、 [https://www.reddit.com/r/Unity3D/comments/154ndpn/unityevent_performance_overhead_a_cause_for/](https://www.reddit.com/r/Unity3D/comments/154ndpn/unityevent_performance_overhead_a_cause_for/)  
36. I'm encountering some people that trying ScriptableObject Architecture. I want to create a single link that I can throw to save them. Any feedback, opinion or PR will be appreciated. : r/Unity3D - Reddit, 8月 30, 2025にアクセス、 [https://www.reddit.com/r/Unity3D/comments/wwbdmf/im_encountering_some_people_that_trying/](https://www.reddit.com/r/Unity3D/comments/wwbdmf/im_encountering_some_people_that_trying/)  
37. Create modular and maintainable code with the observer pattern - Unity Learn, 8月 30, 2025にアクセス、 [https://learn.unity.com/tutorial/create-modular-and-maintainable-code-with-the-observer-pattern?uv=6&projectId=67bc8deaedbc2a23a7389cab](https://learn.unity.com/tutorial/create-modular-and-maintainable-code-with-the-observer-pattern?uv=6&projectId=67bc8deaedbc2a23a7389cab)  
38. Event Systems in Unity : Two Ways - The Bear Blog, 8月 30, 2025にアクセス、 [https://blog.thebear.dev/how-to-event-systems-in-unity](https://blog.thebear.dev/how-to-event-systems-in-unity)  
39. Awesome Unity: Dependency Injection through ScriptableObjects | by Daniel Tan - Medium, 8月 30, 2025にアクセス、 [https://medium.com/glassblade/awesome-unity-dependency-injection-through-scriptableobjects-d58e0fc8f87c](https://medium.com/glassblade/awesome-unity-dependency-injection-through-scriptableobjects-d58e0fc8f87c)  
40. kadinche/Kassets: Unity's Scriptable Object Architecture. - GitHub, 8月 30, 2025にアクセス、 [https://github.com/kadinche/Kassets](https://github.com/kadinche/Kassets)