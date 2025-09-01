

# **成功のためのアーキテクチャ設計：Unity 6 プロジェクト構造に関する決定版ガイド**

## **第1章 プロジェクト構成の哲学：スケーラブルな未来のための基盤**

優れたプロジェクト構成は、単なる「整理整頓」の問題ではありません。それは、ソフトウェアアーキテクチャ、チームマネジメント、そして将来の拡張性に関わる、プロジェクトの成功を左右する foundational な原則です。この章では、なぜ適切に定義されたプロジェクト構造が不可欠であるかを確立し、その基盤となる哲学を探求します。

### **「なぜ」重要なのか：混沌から明瞭性へ**

プロジェクトの初期段階では、アセットを無秩序に配置しても問題ないように思えるかもしれません。しかし、プロジェクトが成長するにつれて、この無秩序さは「技術的負債」として顕在化します。整理されていないプロジェクトは、単に見た目が悪いだけでなく、開発速度を能動的に低下させ、バグの温床となり、新しいチームメンバーの参加を困難にします 1。

一貫性のある構造は、プロジェクトの「信頼できる唯一の情報源（Source of Truth）」として機能し、暗黙的なコミュニケーションの形式となります。これにより、役割に関わらずすべてのチームメンバーがプロジェクト内を容易に移動し、各アセットの「意図された用途」を直感的に理解できるようになります 3。プロジェクトのディレクトリ構造は、単なるファイリングシステムではなく、チームのコミュニケーションパターンと開発方法論を直接反映する鏡です。構造的な規律の崩壊は、多くの場合、チームのコミュニケーションの崩壊や、統一されたアーキテクチャビジョンの欠如を示唆しています。したがって、フォルダ構造を定義し、それを徹底するプロセス自体が、チームビルディングとアーキテクチャ計画における価値ある演習となります。これは、本格的なコードが書かれる前に、ゲームのコンポーネントがどのように構成され、相互作用するかについてチームが合意することを強制するからです。

### **健全なプロジェクト構造を支える4つの柱**

成功するプロジェクト構造は、以下の4つの基本原則に基づいています。

* **一貫性（Consistency）:** 最も重要なルールです。文書化され、合意された構造は、チームの全員が遵守しなければなりません。逸脱は意図的に行われ、文書化されるべきです 3。  
* **拡張性（Scalability）:** 構造はプロジェクトと共に進化する必要があります。最初はシンプルに始め、後から完全かつ破壊的な見直しを必要とせずに、増大する複雑さに対応できなければなりません 6。  
* **発見可能性（Discoverability）:** アセットやスクリプトは簡単に見つけられるべきです。これにより、開発者の時間の浪費と認知的負荷が軽減されます 9。  
* **分離（Isolation）:** 見過ごされがちですが、極めて重要な原則です。プロジェクトのコアアセットは、サードパーティのパッケージ、実験的なコード、一時ファイルからクリーンに分離されなければなりません 3。これにより、依存関係の問題（いわゆる「依存関係地獄」）を防ぎ、パッケージの更新を簡素化します。

### **あらゆるUnityプロジェクトに共通する基本ルール**

どのような構成戦略を選択するにせよ、以下のルールはすべてのUnityプロジェクトの基盤となります。

* **すべてを文書化する:** 選択したフォルダ構造と命名規則は、スタイルガイドとして文書化する必要があります。これはチームプロジェクトにおいて交渉の余地はありません 3。  
* **名前にスペースを使用しない:** ファイル名やフォルダ名にスペースを含めてはいけません。これはコマンドラインツールやその他の開発パイプラインの一部を破壊する可能性があります。代わりにPascalCaseやsnake_caseを使用してください 2。  
* **Assetsフォルダは神聖である:** すべてのゲームコンテンツはAssetsフォルダ内に配置する必要があります。プロジェクトのルートレベルに追加のフォルダを作成することは避けてください 3。  
* **初日からバージョン管理を使用する:** Gitのようなバージョン管理システム（VCS）は任意ではなく、バックアップ、コラボレーション、変更履歴の追跡のために専門的な開発には必須です 1。

## **第2章 主要な構成戦略の比較分析**

Unityプロジェクトのフォルダ構成には、主に二つの思想的対立が存在します。この章では、これら二つの主要なアプローチを詳細に分析し、チームが自身の特定の文脈に適した戦略を選択するための、ニュアンスに富んだ視点を提供します。

### **2.1 アセットタイプ駆動アプローチ：専門家のワークフロー**

このアプローチは、すべてのファイルをその種類ごとにトップレベルのフォルダにグループ化する伝統的な方法です（例：Assets/Scripts、Assets/Models、Assets/Materials）1。これはUnity自身のテンプレートや古いチュートリアルでよく見られる構成です 3。

* **長所:**  
  * **役割中心の効率性:** 専門家にとって非常に直感的です。オーディオエンジニアはAudioフォルダ内でのみ作業し、プログラマーは主にScriptsフォルダ内に留まります 10。  
  * **単純さと低い認知的負荷:** 理解しやすく実装も容易なため、初心者、個人開発者、小規模プロジェクトに最適です 9。  
  * **一括操作:** 特定のタイプのアセットすべてに対して操作を行うタスク（例：すべてのテクスチャのインポート設定の調整）を簡素化します。  
* **短所:**  
  * **低い拡張性:** プロジェクトが成長するにつれて、これらのフォルダは肥大化し、ナビゲーションが困難になります。単一の機能（例：プレイヤーキャラクター）に関連するすべてのアセットを見つけるには、複数の巨大なディレクトリを横断して検索する必要があります 11。  
  * **機能ベースの作業を阻害:** この構造は、特定のゲーム機能に取り組む開発者にとって摩擦を生じさせます。なぜなら、彼らは常にフォルダ間（Scripts/Player、Models/Player、Audio/Player/Footsteps）を飛び回る必要があるからです 13。  
  * **アーキテクチャの隠蔽:** このフォルダ構造は「これはUnityプロジェクトです」と語るだけで、「これは一人称視点のパズルゲームです」とは語りません。ゲームの実際のアーキテクチャやドメインロジックについては何も明らかにしません 15。

### **2.2 機能駆動（ドメイン駆動）アプローチ：設計者の選択**

このアプローチは、特定のゲーム機能やドメインに関連するすべてのファイルをトップレベルのフォルダにグループ化します（例：Assets/Player、Assets/Enemies/Goblin、Assets/UI/MainMenu）13。各機能フォルダ内では、しばしばアセットタイプに基づいたサブフォルダが使用されます。

* **長所:**  
  * **高い凝集度、低い結合度:** 関連するアセットを物理的に近くに保ちます。これは優れたソフトウェア設計の核となる原則です。Player機能に取り組む際、必要なものはすべて一箇所にまとまっています 15。  
  * **アーキテクチャの明瞭性:** フォルダ構造がゲームの設計とアーキテクチャを反映するため、プロジェクトの全体構造が自己文書化されます 15。  
  * **モジュール性と再利用性:** 機能を独立して開発できます。適切に構造化されたWeaponsフォルダは、潜在的に抽出して別のプロジェクトで再利用することが可能です 16。  
  * **最新ツールとの親和性:** 手動でのフォルダナビゲーションへの依存度が低くなります。Unityの検索・フィルタリングツールを使えば、場所に関わらず特定のタイプのアセット（例：「t:script」）を簡単に見つけることができます 13。  
* **短所:**  
  * **共有アセットのジレンマ:** 最大の課題は、複数の機能間で共有されるアセットをどこに配置するかという問題です。これには専用のSharedまたはCommonフォルダが必要となり、そのフォルダ自体の慎重な管理が求められます 10。  
  * **重複の可能性:** 慎重な管理がなければ、共有アセット（汎用的なボタンのテクスチャなど）を複数の機能フォルダ内に重複して配置してしまう誘惑に駆られる可能性があります。  
  * **初期設定の複雑さ:** より単純なアセットタイプ駆動アプローチよりも、事前のアーキテクチャ計画がより多く必要とされます。

アセットタイプ駆動と機能駆動の組織化の選択は、チームの開発方法論（例：専門部署によるウォーターフォール型 対 機能横断型チームによるアジャイル型）を代理的に示すものと言えます。アセットタイプ構造は、異なる専門分野（アート、コード、オーディオ）の作業を物理的に分離し 10、これはアーティストがプログラマーにアセットを引き渡すような伝統的な部署ベースのワークフローを反映しています。一方、機能駆動構造は、単一の機能に必要なすべての分野のアセットを同じ場所に配置し 15、これはアーティスト、プログラマー、デザイナーからなる小グループが完全な機能の提供に責任を持つ、現代的なアジャイルの機能横断型チーム構造を反映しています。したがって、プロジェクトのフォルダアーキテクチャは、チームが選択した開発プロセスと整合し、それをサポートする意識的な決定であるべきです。不一致は、隠れた非効率性やフラストレーションにつながる可能性があります。

### **2.3 ハイブリッドアーキテクチャと「意図された用途」の原則**

ほとんどのプロフェッショナルなプロジェクトにおいて、純粋なアプローチは最適ではありません。ベストプラクティスは、トップレベルに機能を配置し、その内部でアセットタイプのフォルダを使用するハイブリッドモデルです。

ここで、Unity自身の専門家が提唱する「意図された用途（Intended Use）」の原則を導入します 4。フォルダ構造の主要な目標は、アセットが「どのように」「いつ」使用されるかを伝えることであるべきです。これは、アセットバンドル化やメモリ管理に関する重要な決定に直接情報を提供し、構成を単なる見た目の選択からパフォーマンス戦略へと昇華させます。

例えば、キャラクターのモデル、テクスチャ、アニメーションをAssets/Characters/Player/に配置することは、これらのアセットがプレイヤーがアクティブな時に一緒にロードされることを明確に伝えます。これは、それらがModels、Textures、Animationsフォルダに散在しているよりも、メモリ最適化にとって遥かに強力なシグナルとなります。

| 戦略 | 最適な対象 | 拡張性 | ナビゲーションの容易さ（役割別） | アーキテクチャの明瞭性 | 主な長所 | 主な短所 |
| :---- | :---- | :---- | :---- | :---- | :---- | :---- |
| **アセットタイプ駆動** | 個人開発者、プロトタイプ、小規模プロジェクト | 低い | 高い（専門家向け） | 低い | シンプルで直感的 | 機能関連のアセットが散在する |
| **機能駆動** | 中〜大規模チーム、長期プロジェクト | 高い | 中程度（機能開発者向け） | 高い | 高い凝集度、モジュール性 | 共有アセットの管理が課題 |
| **ハイブリッドモデル** | ほとんどのプロフェッショナルプロジェクト | 非常に高い | 高い（全体） | 非常に高い | 両者の利点を両立 | 初期計画が必要 |

*Table 2.1: 構成戦略の比較*

## **第3章 プロジェクト構造の実践的ブループリント**

この章では、理論から実践へと移行し、具体的ですぐに利用可能なフォルダ構造のテンプレートを提供します。これらのブループリントは、プロジェクトの規模やチームのニーズに応じて選択・調整することができます。

### **3.1 個人開発・プロトタイプ向けブループリント（アセットタイプ駆動）**

この構造は、スピードとシンプルさを最適化した、クリーンで最小限の構成です。ゲームジャム、プロトタイプ、あるいは大規模なアーキテクチャ計画が過剰となる小規模な個人プロジェクトに最適です。

Assets/  
├── _Project/  
│   ├── Art/  
│   │   ├── Materials/  
│   │   ├── Models/  
│   │   └── Textures/  
│   ├── Audio/  
│   │   ├── Music/  
│   │   └── SFX/  
│   ├── Prefabs/  
│   ├── Scenes/  
│   └── Scripts/  
├── _ThirdParty/  
│   └──/  
└──

この構造は、プロジェクトファイル（_Project）とサードパーティのアセット（_ThirdParty）を最初から分離するという重要な原則を取り入れています 11。先頭のアンダースコア

_ は、これらの重要なフォルダがプロジェクトビューの常に上部に表示されるようにするための工夫です。

### **3.2 スケーラブルなチーム向けブループリント（ハイブリッド機能駆動）**

**これは、ほとんどのプロフェッショナルな3Dゲームプロジェクトに対する主要な推奨事項として提示されます。** チームでの開発と長期的なメンテナンスを念頭に設計された、堅牢でスケーラブルなハイブリッドモデルです。

Assets/  
├── _Project/  
│   ├── Core/  
│   │   ├── GameLoop/ (GameManager, SceneLoader, etc.)  
│   │   ├── Services/ (AudioSystem, InputSystem, etc.)  
│   │   └── Shared/ (Common scripts, base classes, shared materials)  
│   ├── Features/  
│   │   ├── Player/  
│   │   │   ├── Art/ (Models, Textures, Materials)  
│   │   │   ├── Prefabs/  
│   │   │   └── Scripts/ (Controller, Stats, Abilities)  
│   │   ├── Enemies/  
│   │   │   ├── Goblin/  
│   │   │   │   ├── Art/  
│   │   │   │   ├── Prefabs/  
│   │   │   │   └── Scripts/ (AI, Combat)  
│   │   │   └──...  
│   │   └── UI/  
│   │       ├── MainMenu/  
│   │       └── HUD/  
│   ├── Scenes/  
│   │   ├── Levels/  
│   │   └── System/ (Bootstrapper scene, etc.)  
│   └── _Sandbox/  
│       └──/ (For safe experimentation)  
├── _ThirdParty/  
│   └──/  
└──

このブループリントは、11、11、16 から得られた知見に基づいています。コアシステムを個別の機能から分離し、共有アセットのための明確な場所を提供し、さらにメインプロジェクトを汚染することなく安全な実験を可能にする

_Sandbox エリアを含んでいます 2。

_Sandbox/[Username] フォルダの導入は、単なる整理整頓のためだけではありません。これはバージョン管理のコンフリクトを直接的に削減し、安全な実験を奨励する極めて重要なワークフローツールです。チーム環境では、開発者はしばしば一時的なシーンやテスト用のプレハブを作成する必要があります 3。指定されたエリアがなければ、これらの一時的なアセットはしばしばコアプロジェクトのフォルダに紛れ込み、プロジェクトを汚染します。さらに重要なのは、実験的または壊れたアセットが誤ってメインのバージョン管理ブランチにコミットされ、チームの他のメンバーに問題を引き起こす可能性があることです。ユーザー固有のサンドボックスフォルダを提供し、これを最終ビルドやバージョン管理のコミットから（

.gitignore を介して）容易に除外できるようにすることで、このアーキテクチャは生産用アセットの完全性を保護しつつ、実験を積極的に奨励します。これは、一般的なワークフローの問題に対する、予防的かつアーキテクチャ的な解決策です。

### **3.3 大規模プロダクションのための高度な考慮事項**

* **アセンブリ定義（.asmdef）:** 大規模プロジェクトにおいて、コードを分割するためにアセンブリ定義ファイルを使用することの重要性は計り知れません。これにより、スクリプトのコンパイル時間が劇的に短縮され、アーキテクチャ上の境界が強制されます 6。スクリプトのフォルダ構造は、アセンブリ構造と名前空間を反映するべきです 5。  
* **シーン管理:** モノリシックな（一枚岩の）シーンは避けるべきです。大規模なレベルは、複数の小さなシーンに分割し、加算的にロードするべきです。これにより、アーティストとデザイナーの並行作業が可能になり、バージョン管理におけるマージコンフリクトが減少します 3。  
* **AssetBundlesとAddressables:** 非常に大規模なプロジェクトでは、アセット（特にモデルやテクスチャのような重いもの）は、別々のUnityプロジェクトで管理し、AssetBundleとしてビルドするか、Addressable Asset Systemを介して管理することがあります。これにより、メインの開発プロジェクトを軽量で応答性の高い状態に保つことができます 4。

## **第4章 Unityの特殊フォルダ詳解**

このセクションは、Unityが予約しているフォルダ名を解き明かし、明確な使用法と禁止事項を提供する、決定版のリファレンスガイドとして機能します。これらのフォルダのルールは厳格であり、誤った使用は予期せぬ問題を引き起こす可能性があります。

特殊フォルダのコンパイル順序への影響（Editor、Plugins、Standard Assetsなど）は、主にレガシーシステムの名残です 21。これらのフォルダは、スクリプトのコンパイルを4つのフェーズに分ける暗黙的で複雑なシステムを形成していました。しかし、このシステムは、アセンブリ定義ファイルによって提供される、より明示的で強力な制御に大部分が取って代わられています。アセンブリ定義は、依存関係とコンパイルを明示的に制御し、管理可能な個別のアセンブリを作成する、はるかに堅牢で透明性の高いシステムです 6。したがって、開発者は依然として特殊フォルダのルール（特に

Editor）を理解する必要がありますが、現代のスクリプトアーキテクチャ管理のベストプラクティスは、アセンブリ定義の体系的な使用に依存しており、古いコンパイル順序のルールは新しいコードにとっては二次的な関心事となっています。

| フォルダ名 | 目的 | 配置ルール | 主なベストプラクティス / 警告 |
| :---- | :---- | :---- | :---- |
| **Editor** | Unityエディタを拡張するスクリプトを格納。ビルドからは除外される。 | Assetsフォルダ内のどこにでも複数配置可能。場所によってコンパイル順序が影響を受ける。 | ランタイムコードからエディタ専用APIを呼び出す場合は、#if UNITY_EDITORディレクティブを使用する 23。 |
| **Editor Default Resources** | EditorGUIUtility.Loadでロードされるエディタスクリプト用アセットを格納。 | Assetsフォルダの直下に1つだけ配置可能。 | ランタイムからはアクセス不可。エディタ拡張のUI用画像などに使用 23。 |
| **Gizmos** | Gizmos.DrawIconでシーンビューに表示されるアイコン画像を格納。 | Assetsフォルダの直下に1つだけ配置可能。 | デバッグやレベルデザインの視覚的補助として活用 23。 |
| **Plugins** | ネイティブ（C/C++など）のプラグイン（DLL）を格納。 | Assetsフォルダの直下に1つだけ配置することが強く推奨される。 | プラットフォームごとにインポート設定を正しく構成する必要がある 23。 |
| **Resources** | Resources.Load APIを介して動的にアセットをロードするためのフォルダ。 | Assetsフォルダ内のどこにでも複数配置可能。 | **【警告】本番プロジェクトでの使用は強く非推奨。** ビルドサイズの肥大化、起動時間の増加、メモリ管理の困難化を招く 6。 |
| **StreamingAssets** | ビルド後にターゲットデバイス上で生のファイルとしてアクセスする必要があるアセットを格納。 | Assetsフォルダの直下に1つだけ配置可能。 | 多くのプラットフォームで読み取り専用。Android/WebGLではUnityWebRequest経由でのアクセスが必須 25。 |

*Table 4.1: Unity特殊フォルダ クイックリファレンス*

### **4.1 Resourcesフォルダ：使用を避けるべき理由**

Resourcesフォルダは、その手軽さから多くのチュートリアルや小規模プロジェクトで利用されてきました。しかし、プロフェッショナルな開発や大規模プロジェクトにおいては、その使用は深刻なパフォーマンス問題を引き起こすため、原則として避けるべきです 6。

* **問題点:**  
  1. **ビルドサイズの肥大化:** Resourcesフォルダ内のすべてのアセットは、実際にシーンで使用されているかどうかにかかわらず、無条件にビルドに含まれます。これにより、未使用のアセットがビルドサイズを不必要に増加させます 27。  
  2. **起動時間の増加とメモリの非効率な使用:** アプリケーションの起動時に、UnityはResourcesフォルダ内のすべてのアセットのインデックスを作成します。フォルダが巨大になると、このプロセスが起動時間を著しく遅延させます。また、Resources.Loadでロードされたアセットのメモリ管理は手動で行う必要があり、参照が複雑になるとメモリリークの原因となりやすいです 27。  
  3. **依存関係の隠蔽:** Resourcesフォルダは、アセット間の依存関係を隠蔽します。あるプレハブがどのテクスチャやマテリアルに依存しているかが不明確になり、プロジェクトが複雑化するにつれて管理が困難になります。  
* 代替手段：Addressable Asset System  
  現代のUnity開発におけるResourcesフォルダの代替手段は、**Addressable Asset System（アドレッサブル・アセット・システム）**です 27。Addressablesは、アセットを「アドレス」によって非同期にロードするための強力なフレームワークです。これにより、以下のような利点がもたらされます。  
  * **オンデマンドのコンテンツ配信:** 必要な時に必要なアセットだけをロード（およびアンロード）できるため、メモリ使用量を最適化できます。  
  * **コンテンツの更新:** ゲーム本体を再ビルドすることなく、リモートサーバーからアセットを更新できます。これはライブサービス型のゲームにとって不可欠です。  
  * **依存関係の自動管理:** Addressablesはアセット間の依存関係を自動的に追跡し、必要なすべてのアセットを効率的にロードします。

結論として、迅速なプロトタイピングを除き、新規プロジェクトではResourcesフォルダの使用を避け、最初からAddressable Asset Systemの導入を検討することが強く推奨されます。

### **4.2 StreamingAssetsフォルダ：正しい使い方**

StreamingAssetsフォルダは、ビルドプロセスで圧縮・統合されず、ターゲットデバイスのファイルシステムに「そのまま」コピーされるファイルを配置するための特殊なフォルダです 25。

* **主な用途:**  
  * 動画ファイルや音声ファイル（プラットフォームネイティブのAPIで再生する場合）  
  * 設定ファイル（例：JSON、XML）  
  * 外部のデータベースファイル（例：SQLite）  
  * ユーザーによるMOD（改造）を可能にするためのファイル  
* **注意点:**  
  * **パスの違い:** Application.streamingAssetsPathプロパティは、プラットフォームによって異なるパスを返します。特にAndroidでは、ファイルは圧縮されたAPKファイル内に存在するため、UnityWebRequestクラスを使用しないとアクセスできません 28。  
  * **読み取り専用:** ほとんどのプラットフォーム（特にモバイル）では、このフォルダは実行時に書き込み不可能です。ユーザーデータを保存する必要がある場合は、Application.persistentDataPathを使用してください 28。  
  * **スクリプトとDLL:** このフォルダ内のスクリプトやDLLファイルはコンパイルされません 28。

StreamingAssetsは特定のシナリオで非常に有用ですが、そのプラットフォーム依存の挙動と制限を十分に理解した上で使用する必要があります。

## **第5章 命名規則と標準の確立**

一貫した命名規則は、コードの可読性を高め、チームメンバー間の誤解を減らし、プロジェクト全体のメンテナンス性を向上させるための最も費用対効果の高い方法の一つです。この章では、プロジェクトのすべての要素に対する規範的なスタイルガイドを提供します。

### **5.1 ディレクトリとアセットの命名規則**

* **フォルダ:** PascalCaseを使用します（例：PlayerScripts, MainMenu）。これにより、単語の区切りが明確になります 3。  
* **主要なアセット:** シーン、プレハブ、スクリプタブルオブジェクトなどの主要なアセットにはPascalCaseを使用します（例：Level01, PlayerCharacter, GameSettings）。  
* **接頭辞・接尾辞の活用:** アセットの種類を名前で明確にすることをお勧めします。特に、Unityエディタのアイコンだけでは区別しにくいアセットに有効です 1。  
  * **マテリアル:** M_PlayerSkin  
  * **テクスチャ:** T_Brick_N（Nは法線マップを示す）、T_Wood_D（DはDiffuse/Albedoを示す）  
  * **アニメーションクリップ:** A_PlayerJump  
  * **オーディオクリップ:** AC_PlayerFootstep  
  * **物理マテリアル:** PM_Ice  
* **LODメッシュ:** Unityがインポート時にLODグループを自動的に設定できるように、メッシュファイルには特定の命名規則を適用します。基本名の末尾に_LOD0, _LOD1, _LOD2...と続けます。LOD0が最も詳細なモデルです 32。

### **5.2 C#スクリプティング・スタイルガイド**

このガイドは、Microsoftの公式C#ガイドラインを基に、Unityの文脈に合わせて調整したものです 31。

* **クラス、構造体、列挙型、インターフェース:** PascalCaseを使用します。インターフェースには接頭辞Iを付けます（例：class PlayerController, interface IDamageable）。  
* **public/protectedなフィールドとプロパティ:** PascalCaseを使用します（例：public float MaxHealth;）。これは、Unityのインスペクタでの表示との一貫性を保つためです。  
* **privateなフィールド:** camelCaseを使用し、接頭辞を付けます。最も一般的な接頭辞はアンダースコア (_myVariable) または m_ (m_myVariable) です。どちらを選択しても構いませんが、プロジェクト全体で一貫性を保つことが重要です 31。  
* **メソッド:** PascalCaseを使用します（例：void CalculateDamage()）。  
* **ローカル変数とメソッドの引数:** camelCaseを使用します（例：float damageAmount）。  
* **定数 (const, static readonly):** 接頭辞k_とPascalCaseを推奨します（例：private const int k_MaxPlayerCount = 4;）。これにより、定数であることが一目でわかります 31。  
* **真偽値 (bool):** 変数名が質問文として読めるように、動詞で始めるべきです（例：bool isDead, bool hasKey）。これにより、コードの意図がより明確になります 31。  
* **名前空間:** スクリプトのフォルダ構造は、名前空間と一致させるべきです。これにより、コードの論理的なグループ化が促進され、サードパーティ製アセットとのクラス名の衝突を回避できます 3。例えば、  
  _Project/Features/Player/Scripts/内のスクリプトはMyGame.Features.Playerのような名前空間に属します。

## **第6章 プロフェッショナルな開発エコシステムとの統合**

プロジェクトのフォルダ構造は、それ自体で完結するものではありません。バージョン管理システムやサードパーティ製アセットといった、外部のツールや依存関係とどのように相互作用するかを考慮して設計する必要があります。

### **6.1 バージョン管理戦略（Git）**

* **.gitignoreファイル:** 堅牢なUnity専用の.gitignoreテンプレートを使用することが不可欠です。Library、Logs、Temp、UserSettingsといったフォルダは、ローカルマシン固有の、あるいは自動生成されるデータを含んでいるため、バージョン管理から除外しなければなりません。これらをコミットすると、リポジトリが肥大化し、チームメンバー間で不要なコンフリクトが発生します。  
* **Git LFS（Large File Storage）:** 標準のGitは、テクスチャ、モデル、オーディオファイルなどの大きなバイナリファイルを扱うのに非効率です。これらのファイルを追跡するためにGit LFSを使用することが必須です。LFSを使わないと、リポジトリがすぐに使用不可能なほど巨大になります 1。プロジェクト設定時には、追跡すべきファイルタイプ（例：  
  .png, .fbx, .wav）を指定する.gitattributesファイルを設定する必要があります。  
* **.metaファイルの取り扱い:** 対応するアセットと共に.metaファイルが常にコミットされることを保証することが極めて重要です。アセットの移動や名前の変更は、必ずUnityエディタ*内*で行う必要があります。これにより、アセットのユニークIDを含む.metaファイルが正しく更新・移動されます。エディタ外でファイルを操作すると、シーンやプレハブ内のそのアセットへの参照がすべて壊れてしまいます 3。

### **6.2 サードパーティ製依存関係の管理**

* **分離戦略:** ブループリントで示した通り、Asset Storeのパッケージや外部プラグインはすべて、_ThirdPartyやPluginsのような単一のトップレベルフォルダに配置する戦略を徹底します 3。これにより、  
  Assetsフォルダのルートが散らかるのを防ぎ、更新や削除が容易になります。  
* **パッケージの更新:** Asset Storeからアセットを更新すると、デフォルトのルート位置にインポートしようとすることがあります。この場合の正しい手順は、まずデフォルトの場所にインポートし、次にそれを指定の_ThirdPartyフォルダに移動して古いバージョンを上書きすることです 7。ただし、一部の複雑なプラグインは移動すると破損する可能性があるため、ルートに留めておく必要がある場合もありますが、これは例外とすべきです 7。  
* **Unity Package Manager (UPM):** Assetsフォルダに配置されるレガシーなアセットパッケージと、Packages/manifest.jsonファイルを介して管理される現代的なUPMパッケージを区別することが重要です。UPMは、Unity自身のモジュールや、増加しつつあるサードパーティ製ツールを配布・利用するための推奨される方法です 37。Unity 6では、UPMのインターフェースと機能が継続的に改善されています 39。

## **第7章 結論と最終勧告**

本レポートでは、Unity 6における3Dゲームプロジェクトのディレクトリ構成に関するベストプラクティスを、その哲学的基盤から実践的なブループリント、そしてプロフェッショナルな開発エコシステムとの統合に至るまで、包括的に探求してきました。

### **主要原則の要約**

成功するプロジェクトアーキテクチャは、以下の核心的なアイデアに基づいています。

1. **計画から始める:** プロジェクト開始時に、拡張性とチームのワークフローを考慮した構造を計画する。  
2. **一貫性を最優先する:** 選択した構造と命名規則を文書化し、チーム全員で厳格に遵守する。  
3. **ワークフローに合った構造を選ぶ:** ほとんどのプロフェッショナルなチームにとって、**ハイブリッドな機能駆動モデル**が最適なバランスを提供する。  
4. **特殊フォルダを理解する:** Unityの予約済みフォルダの役割と制限、特にResourcesフォルダの危険性を理解し、適切に扱う。  
5. **命名規則を徹底する:** アセットからコードの変数に至るまで、一貫した命名規則を適用し、可読性と保守性を最大化する。  
6. **エコシステムと統合する:** バージョン管理（Git LFSを含む）とサードパーティ製アセットの管理を、プロジェクト構造の一部として設計する。

### **今後の展望**

プロジェクト構造は、一度決めたら変更できない硬直したものではありません。むしろ、プロジェクトの進化に合わせて定期的に見直され、洗練されるべき「生きたドキュメント」です 7。ここでの目標は、初日から完璧で不変の構造を作り上げることではなく、プロジェクトの変化するニーズに適応できる、強力でスケーラブルな基盤を確立することです。

### **最終勧告**

結論として、Unity 6で新規に本格的な3Dゲームプロジェクトを開始するすべての開発者およびチームに対し、本レポートの第3.2章で提示した**「スケーラブルなチーム向けブループリント（ハイブリッド機能駆動）」**をゴールドスタンダードとして採用することを強く推奨します。この構造は、本レポートで詳述した拡張性、アーキテクチャの明瞭性、そしてチームコラボレーションの原則を最も効果的に具現化するものであり、プロジェクトを成功に導くための堅牢な礎となるでしょう。

#### **引用文献**

1. Unityプロジェクト整理のコツとベストプラクティス：効率的な開発を実現しよう - ONES.com, 9月 1, 2025にアクセス、 [https://ones.com/ja/blog/unity-project-organization-tips/](https://ones.com/ja/blog/unity-project-organization-tips/)  
2. Best Ways to Keep Unity Project Organized: Unity3d Best Practices - Juego Studio, 9月 1, 2025にアクセス、 [https://www.juegostudio.com/blog/7-ways-to-keep-unity-project-organized-unity3d-best-practices](https://www.juegostudio.com/blog/7-ways-to-keep-unity-project-organized-unity3d-best-practices)  
3. Unityプロジェクトを整理するためのベストプラクティス, 9月 1, 2025にアクセス、 [https://unity.com/ja/how-to/organizing-your-project](https://unity.com/ja/how-to/organizing-your-project)  
4. Why folder structures matter - Unity, 9月 1, 2025にアクセス、 [https://unity.com/blog/engine-platform/why-folder-structures-matter](https://unity.com/blog/engine-platform/why-folder-structures-matter)  
5. Best practices for organizing your Unity project, 9月 1, 2025にアクセス、 [https://unity.com/how-to/organizing-your-project](https://unity.com/how-to/organizing-your-project)  
6. 【Unity】業務で行うUnityProjectのベーシックな知見をまとめてみました - 株式会社ロジカルビート, 9月 1, 2025にアクセス、 [https://logicalbeat.jp/blog/15424/](https://logicalbeat.jp/blog/15424/)  
7. A guide to folder structures for Unity 6 projects - Anchorpoint, 9月 1, 2025にアクセス、 [https://www.anchorpoint.app/blog/unity-folder-structure](https://www.anchorpoint.app/blog/unity-folder-structure)  
8. How to architect code as your project scales | Avoiding technical debt - Unity, 9月 1, 2025にアクセス、 [https://unity.com/how-to/how-architect-code-your-project-scales](https://unity.com/how-to/how-architect-code-your-project-scales)  
9. Project Structure : r/unity - Reddit, 9月 1, 2025にアクセス、 [https://www.reddit.com/r/unity/comments/192qk76/project_structure/](https://www.reddit.com/r/unity/comments/192qk76/project_structure/)  
10. Unity Folder Structure - YouTube, 9月 1, 2025にアクセス、 [https://www.youtube.com/watch?v=Qf6VHfOUkSQ](https://www.youtube.com/watch?v=Qf6VHfOUkSQ)  
11. Unityのフォルダ構成ベストプラクティス - ぷろみん, 9月 1, 2025にアクセス、 [https://torini.hateblo.jp/entry/2017/06/15/Unity%E3%81%AE%E3%83%95%E3%82%A9%E3%83%AB%E3%83%80%E6%A7%8B%E6%88%90%E3%83%99%E3%82%B9%E3%83%88%E3%83%97%E3%83%A9%E3%82%AF%E3%83%86%E3%82%A3%E3%82%B9](https://torini.hateblo.jp/entry/2017/06/15/Unity%E3%81%AE%E3%83%95%E3%82%A9%E3%83%AB%E3%83%80%E6%A7%8B%E6%88%90%E3%83%99%E3%82%B9%E3%83%88%E3%83%97%E3%83%A9%E3%82%AF%E3%83%86%E3%82%A3%E3%82%B9)  
12. バージョン管理システムのベストプラクティス - Unity, 9月 1, 2025にアクセス、 [https://unity.com/ja/how-to/version-control-systems](https://unity.com/ja/how-to/version-control-systems)  
13. How to structure your Unity project (best practice tips) - Game Dev Beginner, 9月 1, 2025にアクセス、 [https://gamedevbeginner.com/how-to-structure-your-unity-project-best-practice-tips/](https://gamedevbeginner.com/how-to-structure-your-unity-project-best-practice-tips/)  
14. Is there a standard/recommended directory structure for Unity projects? - Stack Overflow, 9月 1, 2025にアクセス、 [https://stackoverflow.com/questions/44363854/is-there-a-standard-recommended-directory-structure-for-unity-projects](https://stackoverflow.com/questions/44363854/is-there-a-standard-recommended-directory-structure-for-unity-projects)  
15. Assets folder organization by feature or content? : r/Unity3D - Reddit, 9月 1, 2025にアクセス、 [https://www.reddit.com/r/Unity3D/comments/xmdt7e/assets_folder_organization_by_feature_or_content/](https://www.reddit.com/r/Unity3D/comments/xmdt7e/assets_folder_organization_by_feature_or_content/)  
16. How do you organize your "Assets" folder in Unity? : r/Unity3D - Reddit, 9月 1, 2025にアクセス、 [https://www.reddit.com/r/Unity3D/comments/15070po/how_do_you_organize_your_assets_folder_in_unity/](https://www.reddit.com/r/Unity3D/comments/15070po/how_do_you_organize_your_assets_folder_in_unity/)  
17. Best/optimal project folder structure for mid to large sized projects? : r/Unity3D - Reddit, 9月 1, 2025にアクセス、 [https://www.reddit.com/r/Unity3D/comments/1elhbeq/bestoptimal_project_folder_structure_for_mid_to/](https://www.reddit.com/r/Unity3D/comments/1elhbeq/bestoptimal_project_folder_structure_for_mid_to/)  
18. フォルダー構成が重要な理由 - Unity, 9月 1, 2025にアクセス、 [https://unity.com/ja/blog/engine-platform/why-folder-structures-matter](https://unity.com/ja/blog/engine-platform/why-folder-structures-matter)  
19. 【Unity】Unity のフォルダ分けについて考察してみる - Qiita, 9月 1, 2025にアクセス、 [https://qiita.com/uni928/items/2399ef2594b4e214a73d](https://qiita.com/uni928/items/2399ef2594b4e214a73d)  
20. Version control & project organization best practices - Unity - YouTube, 9月 1, 2025にアクセス、 [https://www.youtube.com/watch?v=GEOqwtzmeP0](https://www.youtube.com/watch?v=GEOqwtzmeP0)  
21. Special folders and script compilation order - Unity User Manual 2021.3 (LTS), 9月 1, 2025にアクセス、 [https://docs.unity.cn/Manual/ScriptCompileOrderFolders.html](https://docs.unity.cn/Manual/ScriptCompileOrderFolders.html)  
22. [Unity] Plugin folder structure in Unity 5 - Forums - NoesisGUI, 9月 1, 2025にアクセス、 [https://www.noesisengine.com/forums/viewtopic.php?t=763](https://www.noesisengine.com/forums/viewtopic.php?t=763)  
23. Special folder names - Unity - Manual, 9月 1, 2025にアクセス、 [https://docs.unity3d.com/2017.2/Documentation/Manual/SpecialFolders.html](https://docs.unity3d.com/2017.2/Documentation/Manual/SpecialFolders.html)  
24. Why do editor scripts need to be placed in editor folder - Stack Overflow, 9月 1, 2025にアクセス、 [https://stackoverflow.com/questions/68700715/why-do-editor-scripts-need-to-be-placed-in-editor-folder](https://stackoverflow.com/questions/68700715/why-do-editor-scripts-need-to-be-placed-in-editor-folder)  
25. A Brief Anatomy of A Unity Project Folder | by Jonathan Jenkins - Medium, 9月 1, 2025にアクセス、 [https://medium.com/@jsj5909/a-brief-anatomy-of-a-unity-project-folder-563bd3f4ad40](https://medium.com/@jsj5909/a-brief-anatomy-of-a-unity-project-folder-563bd3f4ad40)  
26. Special folder names - Unity - Manual, 9月 1, 2025にアクセス、 [https://docs.unity3d.com/2018.2/Documentation/Manual/SpecialFolders.html](https://docs.unity3d.com/2018.2/Documentation/Manual/SpecialFolders.html)  
27. The Resources folder - Unity - Manual, 9月 1, 2025にアクセス、 [https://docs.unity3d.com/2022.3/Documentation/Manual/UnderstandingPerformanceResourcesFolder.html](https://docs.unity3d.com/2022.3/Documentation/Manual/UnderstandingPerformanceResourcesFolder.html)  
28. Streaming Assets - Unity - Manual, 9月 1, 2025にアクセス、 [https://docs.unity3d.com/2020.1/Documentation/Manual/StreamingAssets.html](https://docs.unity3d.com/2020.1/Documentation/Manual/StreamingAssets.html)  
29. Using the Resources folder in 2022 : r/Unity3D - Reddit, 9月 1, 2025にアクセス、 [https://www.reddit.com/r/Unity3D/comments/veevhm/using_the_resources_folder_in_2022/](https://www.reddit.com/r/Unity3D/comments/veevhm/using_the_resources_folder_in_2022/)  
30. Streaming Assets - Unity User Manual 2021.3 (LTS), 9月 1, 2025にアクセス、 [https://docs.unity.cn/es/2021.1/Manual/StreamingAssets.html](https://docs.unity.cn/es/2021.1/Manual/StreamingAssets.html)  
31. Naming Rules & Style Conventions for C# Scripting Code | Unity, 9月 1, 2025にアクセス、 [https://unity.com/how-to/naming-and-code-style-tips-c-scripting-unity](https://unity.com/how-to/naming-and-code-style-tips-c-scripting-unity)  
32. Level of Detail (LOD) - Unity - Manual, 9月 1, 2025にアクセス、 [https://docs.unity3d.com/2017.2/Documentation/Manual/LevelOfDetail.html](https://docs.unity3d.com/2017.2/Documentation/Manual/LevelOfDetail.html)  
33. Mastering Code Clarity: A Unity-Centric Guide to Effective Naming | by Juris - Medium, 9月 1, 2025にアクセス、 [https://medium.com/@juris.savos/mastering-code-clarity-a-unity-centric-guide-to-effective-naming-e5ca9094be5c](https://medium.com/@juris.savos/mastering-code-clarity-a-unity-centric-guide-to-effective-naming-e5ca9094be5c)  
34. C# at Google Style Guide | styleguide, 9月 1, 2025にアクセス、 [https://google.github.io/styleguide/csharp-style.html](https://google.github.io/styleguide/csharp-style.html)  
35. What is a good naming convention for Unity? [closed] - Stack Overflow, 9月 1, 2025にアクセス、 [https://stackoverflow.com/questions/24586319/what-is-a-good-naming-convention-for-unity](https://stackoverflow.com/questions/24586319/what-is-a-good-naming-convention-for-unity)  
36. Guide: Using GitHub and Unity (From a Game Dev) - Reddit, 9月 1, 2025にアクセス、 [https://www.reddit.com/r/unity/comments/1adjewj/guide_using_github_and_unity_from_a_game_dev/](https://www.reddit.com/r/unity/comments/1adjewj/guide_using_github_and_unity_from_a_game_dev/)  
37. Unity Package Manager, 9月 1, 2025にアクセス、 [https://docs.unity.cn/Packages/com.unity.package-manager-ui@1.8/manual/index.html](https://docs.unity.cn/Packages/com.unity.package-manager-ui@1.8/manual/index.html)  
38. The State of the Unity Package Ecosystem | randomPoison - GitHub Pages, 9月 1, 2025にアクセス、 [https://randompoison.github.io/posts/the-state-of-unity-packages/](https://randompoison.github.io/posts/the-state-of-unity-packages/)  
39. Manual: New in Unity 6.0, 9月 1, 2025にアクセス、 [https://docs.unity3d.com/6000.1/Documentation/Manual/WhatsNewUnity6.html](https://docs.unity3d.com/6000.1/Documentation/Manual/WhatsNewUnity6.html)  
40. The Package Manager window - Unity - Manual, 9月 1, 2025にアクセス、 [https://docs.unity3d.com/6000.2/Documentation/Manual/upm-ui.html](https://docs.unity3d.com/6000.2/Documentation/Manual/upm-ui.html)