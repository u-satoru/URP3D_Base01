

# **現代3D FPSゲームの要件仕様に関する包括的分析**

## **第1章：コアゲームプレイアーキテクチャ - 設計とメカニクス**

技術的基盤からソフトウェア層へと移行し、本章ではプレイヤーの瞬間的な体験を定義するコアデザインの原則を分析する。FPSのサブジャンルのスペクトラム、戦闘のペースを決定づけるTTK（Time-to-Kill）の重要性、そしてプレイヤーを長期的に惹きつけるためのシステムについて掘り下げる。

### **1.1. エンゲージメントのスペクトラム：タクティカルシューター vs. カジュアルシューター**

FPSは一つのジャンルでありながら、その内部には設計思想の異なる多様なサブジャンルが存在する。その分類軸の一つが、ゲームのペースと戦略性である。

* **カジュアル（アーケード）シューター:** 『Call of Duty』シリーズや『Apex Legends』に代表されるこのカテゴリは、スピーディーな移動、素早い戦闘、そして死亡時のペナルティが比較的低いことを特徴とする 1。プレイヤーは絶えず動き回り、頻繁に銃撃戦が発生するため、反射神経と瞬間的な判断力が求められる 2。  
* **タクティカルシューター:** 『VALORANT』や『Counter-Strike: Global Offensive (CS:GO)』がこのカテゴリの代表格である。ここでは、意図的にペースを落とし、戦略的なポジショニング、マップコントロール、そして一瞬のミスが敗北に直結する緊張感の高い銃撃戦が重視される 1。情報収集とチーム連携が勝利の鍵を握る。

これらのカテゴリ内には、さらにキャラクターの役割に基づく分類が存在する。

* **ヒーローシューター:** 『Overwatch』、『Apex Legends』、『VALORANT』などが含まれる。プレイヤーは、それぞれが固有の武器やゲーム展開を大きく左右するアビリティを持つ、ユニークな「ヒーロー」キャラクターを選択する 4。チーム全体のヒーロー構成と、アビリティを連携させるシナジーが極めて重要となる 6。  
* **クラスベース／ロードアウトシューター:** 『Call of Duty』や『Battlefield』シリーズがこれにあたる。プレイヤーは「突撃兵」や「衛生兵」といった広範な役割（クラス）を選び、多種多様な武器やパーク（特殊能力）を組み合わせて自分だけの装備セット（ロードアウト）を作成する 6。ここでは、キャラクター固有の能力よりも、純粋な銃撃スキルとロードアウトの戦略性が戦闘の主軸となる。

### **1.2. 戦闘の定義：TTK（Time-to-Kill）の決定的役割**

TTK（キルするまでの時間）は、FPSの設計において最も重要な変数と言っても過言ではない。この数値が、戦闘全体の感覚と流れを決定づけるからである。

* **低TTK:** 『Call of Duty』や『CS:GO』のようなゲームでは、TTKが約100ミリ秒から400ミリ秒程度に設定されている 8。この設計は、反射神経、有利な位置取り、そして先制攻撃の重要性を最大化する。戦闘は瞬時に決着し、一瞬の油断が死に繋がる。初心者にとっては反応する間もなく倒されるためフラストレーションが溜まりやすいが、成功した際には高いアドレナリンラッシュをもたらす 9。  
* **高TTK:** 『Apex Legends』や『Halo』シリーズでは、TTKが1秒を超えることも珍しくない 11。この設計は、敵を追い続けるトラッキングエイム、巧みな移動による回避行動、そして長引く「決闘」の中でのアビリティ使用といった、持続的なメカニクススキルを重視する。先に撃たれた側にも反撃の機会が与えられるため、不意打ちよりも純粋な撃ち合いの技術が勝敗を分ける傾向が強い 11。

TTKの選択は、単なる数値設定ではなく、ゲーム全体の設計思想を方向付ける「キーストーン」である。低TTKは、先に敵を発見し、先に撃つことが絶対的なアドバンテージとなるため、情報収集、ポジショニング、マップ知識の価値を極限まで高める。その結果、『CS:GO』や『VALORANT』に見られるように、マップデザインは予測可能な射線、戦術的なチョークポイント、明確な交戦エリアを持つ構造になる。対照的に、高TTKは戦闘が長引くため、プレイヤーの移動スキル、エイムの持続力、戦闘中の状況判断が重要となる。そのため、『Apex Legends』や『Halo』のマップは、豊富な遮蔽物、高低差、そして回避行動のための広いスペースが確保される傾向にある。TTKの選択は、そのゲームが「戦略的なチェス」になるか、「高速なダンス」になるかを根本的に決定づけるのである。

### **1.3. コンテンツとモード戦略：リプレイ性の高い体験の構築**

現代の成功しているFPSフランチャイズは、単一のゲームモードに依存せず、多様なプレイヤー層に対応するために複数の体験を提供するプラットフォームとして機能している 15。

* **現代の三本柱:**  
  1. **キャンペーン／シングルプレイ／Co-op:** 物語主導の体験を提供するモード 5。『Call of Duty』シリーズで人気の「ゾンビモード」もこのカテゴリに含まれる 5。  
  2. **クラシックマルチプレイヤー:** チームデスマッチ、ドミネーション、爆破ミッションなど、チームベースの目標達成型モード。これがコアな競技体験の中核を成す 16。  
  3. **バトルロイヤル (BR):** 広大なマップで最後の1人または1チームになるまで戦う大規模モード。『Apex Legends』、『Call of Duty: Warzone』、『PUBG: BATTLEGROUNDS』など、今やジャンルの定番となっている 16。  
* **新興モード:** 上記以外にも、装備を収集・強化しながら戦う「ルートシューター」（例：『Destiny 2』） 18 や、マップから貴重品を確保して脱出を目指す「脱出シューター」（例：『Hunt: Showdown』、『Delta Force』リブート作） 5 といった新たなサブジャンルも人気を博している。

### **1.4. プレイヤーの成長と定着：長期エンゲージメントのためのシステム**

プレイヤーを長期間ゲームに惹きつけ続けるためには、巧妙に設計された進行（プログレッション）システムが不可欠である。これらのシステムは、プレイヤーが「生産的である」「強力になった」「常に新たな挑戦がある」と感じられるように設計されなければならない 19。

* **メカニズム:**  
  * **アンロック:** 新しい武器、キャラクター、アビリティ、マップ、そして外見を飾るコスメティックアイテム（スキン）などをプレイ時間や達成度に応じて解放していく 19。  
  * **ランキングシステム:** 自分のスキルレベルを可視化し、より高いランクを目指すという明確な目標を提供する競争的なラダーシステム（詳細は第3章で後述）。  
  * **フィードバックループ:** プレイヤーが費やした時間に対して、システムは明確なフィードバックを返す必要がある 19。目標を達成し、メカニクスを習得することが、意味のある成長（新しいアンロックやランクアップ）に繋がるというサイクルが重要である 21。

現代のゲーム市場は、プレイヤーの可処分時間と継続的な収益を巡る熾烈な競争の場である 22。単一のモードしか提供しないタイトルは、より多様な体験を提供する競合にプレイヤーを奪われるリスクが高い。そのため、『Call of Duty』のような巨大フランチャイズは、単一のゲームではなく、複数のゲームモード（マルチプレイヤー、Warzone、ゾンビ）を一つのIP傘下に持つエコシステムへと進化している。これにより、物語を求めるプレイヤー、競技性を求めるプレイヤー、大規模なソーシャル体験を求めるプレイヤーといった異なるセグメントを同一のフランチャイズ内に留め、エンゲージメントと収益機会を最大化している 5。

## **第2章：感覚的体験 - アート、オーディオ、インターフェースデザイン**

本章では、ゲームのプレゼンテーションを構成する重要な要素を探求する。現代のレンダリング技術、戦略的なアートディレクション、戦術的なオーディオデザイン、そしてミニマルなユーザーインターフェースがどのように組み合わさり、没入感が高いだけでなく、ゲームプレイの明瞭性と競技上の公平性を高める体験を生み出すかを分析する。

### **2.1. 視覚的忠実度：物理ベースレンダリング (PBR)**

* **業界標準:** PBR（Physically Based Rendering）は、光が現実世界の物質とどのように相互作用するかをシミュレートするレンダリング手法であり、リアルタイムのゲームエンジンで効率的に動作しながら、フォトリアルなビジュアルを実現するための業界標準となっている 23。  
* **コアワークフロー:** アーティストは、アルベド（ベースカラー）、メタリック、ラフネス、ノーマルマップといった一連のテクスチャマップを作成し、マテリアルに物理的な特性を与える。ゲームエンジンのシェーダーはこれらのマップを利用して、様々な照明条件下でマテリアルがどのように見えるべきかを物理法則に基づいて計算する 24。このワークフローは物理学に基づきながらも、アーティストが最終的な見た目をコントロールする余地を残している 23。

### **2.2. 戦略としてのアートディレクション：リアリズム vs. スタイライズ**

アートスタイルの選択は、単なる美的嗜好の問題ではなく、ゲームのコアな目標を反映した戦略的な決定である。

* **フォトリアリズム (例: 『Call of Duty』, 『Battlefield』):** 現実を模倣した、没入感の高い映画的なビジュアルを目指すアプローチ。グラフィック負荷は高くなる傾向があるが、プレイヤーに強烈な臨場感と重厚な雰囲気を提供する。サウンドデザインもこれに追随し、本物の銃声や環境音の再現を目指すことが多い 28。  
* **スタイライズ (例: 『VALORANT』, 『Apex Legends』):** クリーンな線、鮮やかな色彩、簡略化されたテクスチャを特徴とする、よりイラスト的、しばしば「カートゥーン調」の美的感覚を採用する 29。これは意図的な戦略的選択であり、いくつかの重要な利点をもたらす。  
  * **ゲームプレイの明瞭性:** キャラクターやアビリティは、明確なシルエットと配色でデザインされ、乱戦の中でも瞬時に識別できるように作られている。これは特に『VALORANT』のデザイン哲学の中核をなす要素である 29。  
  * **パフォーマンスとアクセシビリティ:** 複雑さを抑えたアートスタイルは、より広範なスペックのPCで良好に動作するため、潜在的なプレイヤーベースを拡大することができる 30。  
  * **時代を超えた魅力:** スタイライズされたアートは、技術の進歩によってすぐに古く見えてしまうフォトリアリズムへの挑戦よりも、時間が経っても魅力が色褪せにくい 30。

この「明瞭性 vs. 没入感」という方程式において、アートスタイルは直接的な変数となる。競技用FPSの核となる要件は、プレイヤーが脅威を明確に識別し、迅速に反応できること、すなわち「ゲームプレイの明瞭性」である。フォトリアルなアートスタイルは、複雑なライティングや詳細なテクスチャ、迷彩効果などによってキャラクターモデルを背景に溶け込ませることがあり、これは「没入感」を高める一方で明瞭性を低下させる可能性がある。『VALORANT』のようなスタイライズされたアートは、キャラクターを際立たせる明るい輪郭（フレネル効果）やシンプルな環境、識別しやすいVFXを用いることで、意図的にリアリズムを犠牲にして明瞭性を確保している 30。

### **2.3. 没入型サウンドスケープ：3Dポジショナルオーディオの戦術的利点**

* **ステレオを超えて:** 現代のFPSには、プレイヤーが音（足音、銃声、アビリティの発動音）の発生源を3D空間内で正確に特定できる高度な3Dオーディオソリューションが求められる。これには上下方向の定位も含まれ、単なる没入感の向上だけでなく、極めて重要な戦術的情報源となる 32。  
* **コンソールの革新 (PS5のTempest 3D AudioTech):** PlayStation 5は、Tempestエンジンと呼ばれる専用のハードウェアチップを搭載し、数百もの3Dオーディオソースを同時に処理することが可能である 33。このシステムは、頭部伝達関数（HRTF）を用いて、個人が音の方向をどのように知覚するかをシミュレートし、一般的なヘッドフォンで非常に正確な立体音響を再現する 33。この機能は、開発者にとってプレイヤーの状況認識能力を高めるための強力なツールとなっている 37。

視覚情報がますます複雑化し、混沌とする中で、3Dポジショナルオーディオは戦術情報を得るための主要なチャネルへと進化した。高速なFPSではプレイヤーの視線は前方に集中しがちだが、脅威は360度、さらには上下からもやってくる。3Dオーディオは、足音やリロード音といった音のキューを通じて、全方位の敵の位置情報を常に提供する 37。したがって、PS5のTempestエンジンのような高忠実度の3Dオーディオシステムは、単なる「没入機能」ではなく、高レベルの競技プレイに必須の、レーダーのような感覚的ツールと位置づけられる。

### **2.4. プレイヤーインターフェース (UI/HUD)：情報の最大化と妨害の最小化**

* **「見えない」理想:** FPSのHUD（ヘッズアップディスプレイ）の目標は、体力、弾薬、目標状況、ミニマップといった重要な情報を、画面を乱雑にすることなく、プレイヤーの注意を逸らさずに一目で提供することである 38。優れたFPSのUIは「ほとんど見えない」と表現される。  
* **基本原則:**  
  * **明瞭性:** 情報は瞬時に読み取れる必要がある。  
  * **ミニマリズム:** ゲームプレイのその瞬間に不可欠な情報のみを表示する。  
  * **応答性:** UIは機敏に反応し、プレイヤーのアクションに対して明確なフィードバックを返す。  
  * **文脈認識:** インベントリ画面やスコアボードなど、必須でない情報は必要な時にのみ表示されるべきである。

#### **引用文献**

1. 人気FPSゲームおすすめ・比較COD・BF・OW・APEX・ヴァロラントなどの特徴は！？ | STADIUM, 9月 16, 2025にアクセス、 [https://gaming-stadium.com/fpscod%E3%83%BBbf%E3%83%BBow%E3%83%BBcsgo/](https://gaming-stadium.com/fpscod%E3%83%BBbf%E3%83%BBow%E3%83%BBcsgo/)  
2. I feel that games like Apex and COD have a much higher pace compared to games like CS:GO and Valorant. I personally don't like constantly running around and sliding across the map. Maybe I'm old and like the more laid back feel of other games. What are your thoughts? : r/gaming - Reddit, 9月 16, 2025にアクセス、 [https://www.reddit.com/r/gaming/comments/z1pw8h/i_feel_that_games_like_apex_and_cod_have_a_much/](https://www.reddit.com/r/gaming/comments/z1pw8h/i_feel_that_games_like_apex_and_cod_have_a_much/)  
3. Master Your Strategy: The Ultimate Guide to Tactical Shooter Games - Status Insights, 9月 16, 2025にアクセス、 [https://statustest.amherst.edu/tactical-shooter-games](https://statustest.amherst.edu/tactical-shooter-games)  
4. Shooter game - Wikipedia, 9月 16, 2025にアクセス、 [https://en.wikipedia.org/wiki/Shooter_game](https://en.wikipedia.org/wiki/Shooter_game)  
5. 【PC・Steam】FPS/TPSおすすめ・新作ゲーム｜2025年最新版, 9月 16, 2025にアクセス、 [https://gamewith.jp/pc/article/show/2153](https://gamewith.jp/pc/article/show/2153)  
6. Is there a difference between hero shooters and class based shooters? - Reddit, 9月 16, 2025にアクセス、 [https://www.reddit.com/r/truegaming/comments/6cpszh/is_there_a_difference_between_hero_shooters_and/](https://www.reddit.com/r/truegaming/comments/6cpszh/is_there_a_difference_between_hero_shooters_and/)  
7. What the strange evolution of the hero shooter tells us about the genre's future | PC Gamer, 9月 16, 2025にアクセス、 [https://www.pcgamer.com/what-the-strange-evolution-of-the-hero-shooter-tells-us-about-the-genres-future/](https://www.pcgamer.com/what-the-strange-evolution-of-the-hero-shooter-tells-us-about-the-genres-future/)  
8. 5v5 and TTK: An Analysis - General Discussion - Overwatch Forums, 9月 16, 2025にアクセス、 [https://us.forums.blizzard.com/en/overwatch/t/5v5-and-ttk-an-analysis/810528](https://us.forums.blizzard.com/en/overwatch/t/5v5-and-ttk-an-analysis/810528)  
9. Could a higher Time To Kill (TTK) be a thing ? - General Chat - Splash Damage Forums, 9月 16, 2025にアクセス、 [https://forums.splashdamage.com/t/could-a-higher-time-to-kill-ttk-be-a-thing/207556](https://forums.splashdamage.com/t/could-a-higher-time-to-kill-ttk-be-a-thing/207556)  
10. What is your preference for 'time to kill' in FPS/TPS multiplayer games? : r/truegaming - Reddit, 9月 16, 2025にアクセス、 [https://www.reddit.com/r/truegaming/comments/evho1d/what_is_your_preference_for_time_to_kill_in/](https://www.reddit.com/r/truegaming/comments/evho1d/what_is_your_preference_for_time_to_kill_in/)  
11. Picking the perfect FPS for you - Red Bull, 9月 16, 2025にアクセス、 [https://www.redbull.com/ca-en/finding-the-perfect-fps](https://www.redbull.com/ca-en/finding-the-perfect-fps)  
12. Apex/Warzone/Halo have higher TTK than PS2, it's holding the game back to new players!, 9月 16, 2025にアクセス、 [https://forums.daybreakgames.com/ps2/index.php?threads/apex-warzone-halo-have-higher-ttk-than-ps2-its-holding-the-game-back-to-new-players.259055/](https://forums.daybreakgames.com/ps2/index.php?threads/apex-warzone-halo-have-higher-ttk-than-ps2-its-holding-the-game-back-to-new-players.259055/)  
13. What are the biggest differences between Warzone and Apex? : r/apexuniversity - Reddit, 9月 16, 2025にアクセス、 [https://www.reddit.com/r/apexuniversity/comments/1lek6oh/what_are_the_biggest_differences_between_warzone/](https://www.reddit.com/r/apexuniversity/comments/1lek6oh/what_are_the_biggest_differences_between_warzone/)  
14. Difference between Apex and Warzone? : r/apexuniversity - Reddit, 9月 16, 2025にアクセス、 [https://www.reddit.com/r/apexuniversity/comments/18miczm/difference_between_apex_and_warzone/](https://www.reddit.com/r/apexuniversity/comments/18miczm/difference_between_apex_and_warzone/)  
15. FPS/TPSの魅力と特徴とは？ゲームジャンル解説 - App-BEST, 9月 16, 2025にアクセス、 [https://app-best.jp/articles/game-description-fps-tps/](https://app-best.jp/articles/game-description-fps-tps/)  
16. 【PC・PS・Xbox・Switch】FPSタイトル おすすめ10選 | 有名 人気シューター - Red Bull, 9月 16, 2025にアクセス、 [https://www.redbull.com/jp-ja/beste-multiplayer-online-shooter](https://www.redbull.com/jp-ja/beste-multiplayer-online-shooter)  
17. FPSゲームのお勧め10選！特徴や魅力、TPSとの違いも解説 - 専門学校デジタルアーツ東京, 9月 16, 2025にアクセス、 [https://www.dat.ac.jp/course/game/game-column/game-school/game-fps/](https://www.dat.ac.jp/course/game/game-column/game-school/game-fps/)  
18. PS4とPS5で楽しめるおすすめのFPSゲーム - おすすめガイド | PlayStation (日本), 9月 16, 2025にアクセス、 [https://www.playstation.com/ja-jp/editorial/best-fps-on-ps5-ps4/](https://www.playstation.com/ja-jp/editorial/best-fps-on-ps5-ps4/)  
19. Game Progression and Progression Systems - Game Design Skills, 9月 16, 2025にアクセス、 [https://gamedesignskills.com/game-design/game-progression/](https://gamedesignskills.com/game-design/game-progression/)  
20. From Zero to Hero: Visualizing Player Progression within UI/UX - GDC Vault, 9月 16, 2025にアクセス、 [https://gdcvault.com/play/1025824/From-Zero-to-Hero-Visualizing](https://gdcvault.com/play/1025824/From-Zero-to-Hero-Visualizing)  
21. Introductory guide to game progression and progression systems with examples from my work on WoW and Ori 2 : r/gamedesign - Reddit, 9月 16, 2025にアクセス、 [https://www.reddit.com/r/gamedesign/comments/1eh2n7f/introductory_guide_to_game_progression_and/](https://www.reddit.com/r/gamedesign/comments/1eh2n7f/introductory_guide_to_game_progression_and/)  
22. Skill-based matchmaking isn't based on skill, and Battlefield 6 needs to be different, 9月 16, 2025にアクセス、 [https://www.dbltap.com/features/skill-based-matchmaking-battlefield-6-needs-to-be-different](https://www.dbltap.com/features/skill-based-matchmaking-battlefield-6-needs-to-be-different)  
23. Beyond Extent Academy - Intro to pbr metallic workflow, 9月 16, 2025にアクセス、 [https://www.beyondextent.com/course/pbr-theory](https://www.beyondextent.com/course/pbr-theory)  
24. CRYENGINE | Art Asset Pipeline: PBR Texture Mapping 1/2 - Achieving Photorealism, 9月 16, 2025にアクセス、 [https://www.cryengine.com/tutorials/view/game-level-design/materials/article/art-asset-pipeline-pbr-texture-mapping-1-2-achieving-photorealism-1](https://www.cryengine.com/tutorials/view/game-level-design/materials/article/art-asset-pipeline-pbr-texture-mapping-1-2-achieving-photorealism-1)  
25. Physically Based Rendering (PBR) Explained In-Depth | Epic Developer Community, 9月 16, 2025にアクセス、 [https://dev.epicgames.com/community/learning/tutorials/Yx3q/unreal-engine-physically-based-rendering-pbr-explained-in-depth](https://dev.epicgames.com/community/learning/tutorials/Yx3q/unreal-engine-physically-based-rendering-pbr-explained-in-depth)  
26. Game Asset Workflow - All Parts (now Free!) - YouTube, 9月 16, 2025にアクセス、 [https://www.youtube.com/watch?v=4-N-0sAMg4U](https://www.youtube.com/watch?v=4-N-0sAMg4U)  
27. PBR Tutorial Series - Community & Industry Discussion - Unreal Engine Forums, 9月 16, 2025にアクセス、 [https://forums.unrealengine.com/t/pbr-tutorial-series/15702](https://forums.unrealengine.com/t/pbr-tutorial-series/15702)  
28. Creating Unforgettable Sounds for Call of Duty® | AUDIOLAND - YouTube, 9月 16, 2025にアクセス、 [https://www.youtube.com/watch?v=FoXGIqW2nyo](https://www.youtube.com/watch?v=FoXGIqW2nyo)  
29. What Valve can learn from Riot Part 1: Visual Style : r/GlobalOffensive - Reddit, 9月 16, 2025にアクセス、 [https://www.reddit.com/r/GlobalOffensive/comments/11jf946/what_valve_can_learn_from_riot_part_1_visual_style/](https://www.reddit.com/r/GlobalOffensive/comments/11jf946/what_valve_can_learn_from_riot_part_1_visual_style/)  
30. Valorant's art director reveals the one utterly modern question Riot Games had to ask itself, 9月 16, 2025にアクセス、 [https://www.inverse.com/gaming/valorant-art-style-interview-moby-francke](https://www.inverse.com/gaming/valorant-art-style-interview-moby-francke)  
31. VALORANT Shaders and Gameplay Clarity - Riot Games Technology, 9月 16, 2025にアクセス、 [https://technology.riotgames.com/news/valorant-shaders-and-gameplay-clarity](https://technology.riotgames.com/news/valorant-shaders-and-gameplay-clarity)  
32. Playstation 5 System Specs Deep dive tech talk with Mark Cerny - YouTube, 9月 16, 2025にアクセス、 [https://www.youtube.com/watch?v=KasVMOMWM-4](https://www.youtube.com/watch?v=KasVMOMWM-4)  
33. PS5 3D audio: what is it? How do you get it? - What Hi-Fi?, 9月 16, 2025にアクセス、 [https://www.whathifi.com/features/ps5-3d-audio-what-is-it-how-do-you-get-it](https://www.whathifi.com/features/ps5-3d-audio-what-is-it-how-do-you-get-it)  
34. Abbey Road's Mirek Stiles Explores The Tempest 3D Audio Tech for Sony's PS5, 9月 16, 2025にアクセス、 [https://www.abbeyroad.com/news/abbey-roads-mirek-stiles-explores-the-tempest-3d-audio-tech-for-sonys-ps5-2780](https://www.abbeyroad.com/news/abbey-roads-mirek-stiles-explores-the-tempest-3d-audio-tech-for-sonys-ps5-2780)  
35. PlayStation 5, Tempest 3D AudioTech, & HRTF (Sony PS5 Immersive Audio Explained) | Simon Hutchinson - YouTube, 9月 16, 2025にアクセス、 [https://www.youtube.com/watch?v=RxleUItROFg](https://www.youtube.com/watch?v=RxleUItROFg)  
36. Experience PS5's Tempest 3D AudioTech with compatible headsets at launch, TV Virtual Surround Sound coming after launch - PlayStation.Blog, 9月 16, 2025にアクセス、 [https://blog.playstation.com/2020/10/06/experience-ps5s-tempest-3d-audiotech-with-compatible-headsets-at-launch-tv-virtual-surround-sound-coming-after-launch/](https://blog.playstation.com/2020/10/06/experience-ps5s-tempest-3d-audiotech-with-compatible-headsets-at-launch-tv-virtual-surround-sound-coming-after-launch/)  
37. Devs speak: How PS5 console's ultra-high speed SSD and Tempest 3D AudioTech engine will enhance the future of gaming - PlayStation.Blog, 9月 16, 2025にアクセス、 [https://blog.playstation.com/2020/09/01/devs-speak-how-ps5-consoles-ultra-high-speed-ssd-and-tempest-3d-audiotech-engine-will-enhance-the-future-of-gaming/](https://blog.playstation.com/2020/09/01/devs-speak-how-ps5-consoles-ultra-high-speed-ssd-and-tempest-3d-audiotech-engine-will-enhance-the-future-of-gaming/)  
38. Game UI design: the mechanics of fun experiences - Justinmind, 9月 16, 2025にアクセス、 [https://www.justinmind.com/ui-design/game](https://www.justinmind.com/ui-design/game)
