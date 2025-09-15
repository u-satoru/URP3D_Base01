# ServiceLocator登録サービス一覧

このドキュメントは、プロジェクトの`ServiceLocator`を通じて利用可能なサービスの一覧とその詳細を記載します。
各サービスは、その役割に応じて「Core Services」と「Feature Services」に分類されています。

## Core Services

プロジェクト全体の基盤となるコア機能を提供するサービス群です。

---

### IAudioService

- **実装クラス:** `AudioService`
- **ファイルパス:** `Assets/_Project/Core/Audio/Services/AudioService.cs`
- **概要:** BGM、環境音、効果音など、ゲーム全体のオーディオを統合管理する主要なサービスです。各オーディオサブシステム（BGMManager, EffectManagerなど）を統括し、音量設定や再生・停止を一元的に制御します。
- **主要なメソッドとプロパティ:**
    - `PlaySound(string soundId, Vector3 position, float volume)`: 指定されたIDの効果音を再生します。
    - `PlayBGM(string bgmName, float fadeTime)`: 指定された名前のBGMを再生します。
    - `StopAllSounds()`: すべてのサウンドを停止します。
    - `SetMasterVolume(float volume)`: マスター音量を設定します。
    - `SetCategoryVolume(string category, float volume)`: カテゴリ別（BGM, Effectなど）の音量を設定します。
    - `Pause()` / `Resume()`: オーディオ全体を一時停止・再開します。

---

### IAudioUpdateService

- **実装クラス:** `AudioUpdateService`
- **ファイルパス:** `Assets/_Project/Core/Audio/Services/AudioUpdateService.cs`
- **概要:** オーディオ関連コンポーネントの更新処理を統括し、パフォーマンスを最適化するためのサービスです。`IAudioUpdatable`インターフェースを実装したオブジェクトを登録し、一元的に更新タイミングを管理します。
- **主要なメソッドとプロパティ:**
    - `RegisterUpdatable(IAudioUpdatable updatable)`: 更新対象のオブジェクトを登録します。
    - `UnregisterUpdatable(IAudioUpdatable updatable)`: 更新対象のオブジェクトを登録解除します。
    - `StartCoordinatedUpdates()` / `StopCoordinatedUpdates()`: 統括更新ループを開始・停止します。
    - `GetNearbyAudioSources(Vector3 center, float radius)`: 指定位置周辺のオーディオソースを取得します。

---

### IEffectService

- **実装クラス:** `EffectManager`
- **ファイルパス:** `Assets/_Project/Core/Audio/EffectManager.cs`
- **概要:** UI、インタラクション、戦闘、ステルスなど、様々なカテゴリの効果音（SFX）を管理・再生するサービスです。オーディオソースのプーリングにより、パフォーマンスの最適化も行います。
- **主要なメソッドとプロパティ:**
    - `PlayEffect(string effectId, Vector3 position, float volume)`: 効果音を再生します。
    - `PlayUIEffect(string effectId)`: UI効果音を再生します。
    - `PlayCombatEffect(string effectId, Vector3 position)`: 戦闘効果音を再生します。
    - `StopAllEffects()`: すべての効果音を停止します。
    - `IsPlaying(string effectId)`: 指定した効果音が再生中か確認します。

---

### ISpatialAudioService

- **実装クラス:** `SpatialAudioService`
- **ファイルパス:** `Assets/_Project/Core/Audio/Services/SpatialAudioService.cs`
- **概要:** 3D空間における音響（空間音響）を専門に扱うサービスです。音の減衰、ドップラー効果、オクルージョン（遮蔽）、リバーブなどを管理し、没入感のあるサウンドスケープを構築します。
- **主要なメソッドとプロパティ:**
    - `Play3DSound(string soundId, Vector3 position, float maxDistance, float volume)`: 3D空間でサウンドを再生します。
    - `CreateMovingSound(string soundId, Transform source, float maxDistance)`: 移動する音源を作成します。
    - `UpdateOcclusion(Vector3 listenerPosition, Vector3 sourcePosition, float occlusionLevel)`: 音の遮蔽レベルを更新します。
    - `SetReverbZone(string zoneId, float reverbLevel)`: 特定エリアのリバーブレベルを設定します。

---

### IStealthAudioService

- **実装クラス:** `StealthAudioService`
- **ファイルパス:** `Assets/_Project/Core/Audio/Services/StealthAudioService.cs`
- **概要:** ステルスゲームプレイに特化した音響を管理するサービスです。足音の生成、環境ノイズレベルの設定、AIの聴覚センサーへの通知など、隠密行動に関連するサウンドを制御します。
- **主要なメソッドとプロパティ:**
    - `CreateFootstep(Vector3 position, float intensity, string surfaceType)`: 足音を生成します。
    - `EmitDetectableSound(Vector3 position, float radius, float intensity, string soundType)`: AIが検知可能なサウンドを発生させます。
    - `SetAlertLevelMusic(AlertLevel level)`: AIの警戒レベルに応じたBGMを設定します。
    - `ShouldReduceNonStealthAudio()`: 非ステルス音響の音量を抑制すべきかを判定します。

---

### ICommandInvoker

- **実装クラス:** `CommandInvoker`
- **ファイルパス:** `Assets/_Project/Core/Commands/CommandInvoker.cs`
- **概要:** コマンドパターンの実行役（Invoker）です。`ICommand`インターフェースを実装したコマンドオブジェクトを受け取り、実行、Undo、Redoの履歴管理を行います。
- **主要なメソッドとプロパティ:**
    - `ExecuteCommand(ICommand command)`: コマンドを実行し、Undoスタックに追加します。
    - `Undo()`: 最後に実行したコマンドを取り消します。
    - `Redo()`: Undoしたコマンドを再度実行します。
    - `ClearHistory()`: すべてのコマンド履歴を消去します。
    - `CanUndo` / `CanRedo`: Undo/Redoが可能かどうかの状態を取得します。

---

### ICommandPoolService

- **実装クラス:** `CommandPoolService`
- **ファイルパス:** `Assets/_Project/Core/Commands/CommandPoolService.cs`
- **概要:** コマンドオブジェクトのオブジェクトプーリングを管理するサービスです。頻繁に生成・破棄されるコマンドオブジェクトを再利用することで、メモリ効率とパフォーマンスを向上させます。
- **主要なメソッドとプロパティ:**
    - `GetCommand<T>()`: プールから指定された型のコマンドを取得します。
    - `ReturnCommand<T>(T command)`: 使用済みのコマンドをプールに返却します。
    - `GetStatistics<T>()`: 特定のコマンドプールの統計情報を取得します。

---

### IEventLogger

- **実装クラス:** `EventLogger`
- **ファイルパス:** `Assets/_Project/Core/Debug/EventLogger.cs`
- **概要:** ゲーム内で発生するイベントや重要なログを一元的に記録・管理するためのデバッグ用サービスです。ログレベルのフィルタリングやCSVへのエクスポート機能を持ちます。
- **主要なメソッドとプロパティ:**
    - `Log(string message)`: 一般的なログを記録します。
    - `LogEvent(string eventName, int listenerCount, string payload)`: イベントの発生を記録します。
    - `LogError(string message)`: エラーログを記録します。
    - `GetFilteredLog(string nameFilter, LogLevel minLevel)`: ログをフィルタリングして取得します。
    - `ExportToCSV(string filePath)`: ログをCSVファイルに出力します。

---

### IGameStateManager

- **実装クラス:** `GameStateManagerService`
- **ファイルパス:** `Assets/_Project/Core/Services/Implementations/GameStateManagerService.cs`
- **概要:** ゲームの全体的な状態（MainMenu, InGame, Pausedなど）を管理するサービスです。状態の変更と、それに伴うイベントの発行を担当します。
- **主要なメソッドとプロパティ:**
    - `ChangeGameState(GameState newState)`: ゲームの状態を変更します。
    - `CurrentGameState`: 現在のゲーム状態を取得します。
    - `PreviousGameState`: 以前のゲーム状態を取得します。

---

### IPauseService

- **実装クラス:** `PauseService`
- **ファイルパス:** `Assets/_Project/Core/Services/Implementations/PauseService.cs`
- **概要:** ゲームの一時停止（ポーズ）機能を専門に扱うサービスです。ゲーム時間の停止・再開や、ポーズ状態の切り替えを管理します。
- **主要なメソッドとプロパティ:**
    - `TogglePause()`: ポーズ状態を切り替えます。
    - `SetPauseState(bool paused)`: ポーズ状態を明示的に設定します。
    - `ResumeGame()`: ゲームを再開します。
    - `IsPaused`: 現在ポーズ中かどうかを取得します。

---

### ISceneLoadingService

- **実装クラス:** `SceneLoadingService`
- **ファイルパス:** `Assets/_Project/Core/Services/Implementations/SceneLoadingService.cs`
- **概要:** シーンの読み込みを管理するサービスです。特に、最低ロード時間を保証する機能などを持ち、スムーズなシーン遷移を実現します。
- **主要なメソッドとプロパティ:**
    - `LoadGameplaySceneWithMinTime()`: ゲームプレイシーンを最低ロード時間付きで読み込みます。
    - `LoadSceneWithMinTime(string sceneName)`: 指定されたシーンを最低ロード時間付きで読み込みます。

---

### IScoreService

- **実装クラス:** `ScoreService`
- **ファイルパス:** `Assets/_Project/Core/Services/Implementations/ScoreService.cs`
- **概要:** プレイヤーのスコアとライフ（残機）を管理するサービスです。スコアの加算、ライフの増減、ゲームオーバーの判定などを行います。
- **主要なメソッドとプロパティ:**
    - `AddScore(int points)`: スコアを加算します。
    - `LoseLife()`: ライフを1つ減らします。
    - `AddLife()`: ライフを1つ増やします。
    - `CurrentScore` / `CurrentLives`: 現在のスコアとライフを取得します。

---

### SystemInitializer

- **実装クラス:** `SystemInitializer`
- **ファイルパス:** `Assets/_Project/Core/SystemInitializer.cs`
- **概要:** `IInitializable`インターフェースを実装した各種システムを、指定された優先度順に初期化する役割を持つサービスです。ゲーム起動時のシステムのセットアップフローを管理します。
- **主要なメソッドとプロパティ:**
    - `InitializeAllSystems()`: 登録されているすべてのシステムを初期化します。
    - `IsSystemInitialized<T>()`: 指定されたシステムが初期化済みか確認します。
    - `AreAllSystemsInitialized()`: すべてのシステムが初期化済みか確認します。

---

### CinemachineIntegration

- **実装クラス:** `CinemachineIntegration`
- **ファイルパス:** `Assets/_Project/Features/Camera/Scripts/CinemachineIntegration.cs`
- **概要:** Cinemachineカメラシステムを統合管理するサービスです。複数の仮想カメラの状態（追従、エイム、カットシーンなど）をイベント駆動で切り替え、スムーズなカメラワークを実現します。
- **主要なメソッドとプロパティ:**
    - `SwitchToCamera(CameraState targetState)`: 指定されたカメラ状態に切り替えます。
    - `SetPlayerTarget(Transform target)`: カメラが追従・注視するプレイヤーのTransformを設定します。
    - `SetFieldOfView(float fov)`: 現在アクティブなカメラの視野角を設定します。
    - `GetCurrentCameraState()`: 現在のカメラ状態を取得します。

## Feature Services

特定のゲームジャンルの機能を実現するために提供されるサービス群です。

---

### ActionRPGTemplateManager

- **実装クラス:** `ActionRPGTemplateManager`
- **ファイルパス:** `Assets/_Project/Features/Templates/ActionRPG/Scripts/ActionRPGTemplateManager.cs`
- **概要:** アクションRPGテンプレート全体の管理を行うマネージャークラスです。キャラクター成長、装備、戦闘、インベントリなど、ARPGに関連する各サブシステムを統合します。
- **主要なメソッドとプロパティ:**
    - `StartActionRPGTemplate()` / `StopActionRPGTemplate()`: テンプレートの有効/無効を切り替えます。
    - `CharacterProgression`: `CharacterProgressionManager`へのアクセスを提供します。
    - `Equipment`: `EquipmentManager`へのアクセスを提供します。
    - `Combat`: `CombatManager`へのアクセスを提供します。
    - `Inventory`: `InventoryManager`へのアクセスを提供します。

---

### CharacterProgressionManager

- **実装クラス:** `CharacterProgressionManager`
- **ファイルパス:** `Assets/_Project/Features/Templates/ActionRPG/Scripts/Character/CharacterProgressionManager.cs`
- **概要:** ARPGにおけるキャラクターの成長（レベル、経験値、スキル、ステータス）を管理します。
- **主要なメソッドとプロパティ:**
    - `AddExperience(int experienceAmount)`: 経験値を加算し、レベルアップを処理します。
    - `IncreaseAttribute(CharacterAttribute attribute, int points)`: 属性値を上昇させます。
    - `UpgradeSkill(string skillName, List<SkillTreeNode> skillTree)`: スキルをアップグレードします。

---

### EquipmentManager

- **実装クラス:** `EquipmentManager`
- **ファイルパス:** `Assets/_Project/Features/Templates/ActionRPG/Scripts/Equipment/EquipmentManager.cs`
- **概要:** ARPGの装備システムを管理します。アイテムの装備・解除、および装備品によるステータスボーナスの計算を行います。
- **主要なメソッドとプロパティ:**
    - `EquipItem(ItemData itemData)`: アイテムを装備します。
    - `UnequipItem(EquipmentSlot slot)`: 指定したスロットの装備を解除します。
    - `CalculateEquipmentStats()`: 装備品全体のステータスボーナスを計算します。

---

### InventoryManager

- **実装クラス:** `InventoryManager`
- **ファイルパス:** `Assets/_Project/Features/Templates/ActionRPG/Scripts/Equipment/InventoryManager.cs`
- **概要:** ARPGのインベントリ（所持品）を管理します。アイテムの追加、削除、スタック、ソートなどを行います。
- **主要なメソッドとプロパティ:**
    - `AddItem(ItemData itemData, int quantity)`: インベントリにアイテムを追加します。
    - `RemoveItem(ItemData itemData, int quantity)`: インベントリからアイテムを削除します。
    - `HasItem(ItemData itemData, int requiredQuantity)`: 指定したアイテムを必要数所持しているか確認します。

---

### CombatManager

- **実装クラス:** `CombatManager`
- **ファイルパス:** `Assets/_Project/Features/Templates/ActionRPG/Scripts/Combat/CombatManager.cs`
- **概要:** ARPGの戦闘システムを管理します。ダメージ計算、戦闘状態の管理、スキル使用などを担当します。
- **主要なメソッドとプロパティ:**
    - `DealDamage(GameObject target, int baseDamage, DamageType damageType, bool isCritical)`: ターゲットにダメージを与えます。
    - `StartCombat(GameObject target)` / `EndCombat()`: 戦闘状態を開始・終了します。
    - `UseSkill(string skillName, GameObject target)`: スキルを使用します。

---

### Health

- **実装クラス:** `Health`
- **ファイルパス:** `Assets/_Project/Features/Templates/ActionRPG/Scripts/Combat/Health.cs`
- **概要:** プレイヤーや敵キャラクターのHP（体力）とMP（マナ）を管理します。ダメージ処理、回復、死亡判定などを行います。
- **主要なメソッドとプロパティ:**
    - `TakeDamage(int amount, bool ignoreInvulnerability)`: ダメージを受けます。
    - `Heal(int amount, bool showMessage)`: HPを回復します。
    - `ConsumeMana(int amount)`: MPを消費します。
    - `Revive(int healthAmount, int manaAmount)`: 死亡状態から復活します。

---

### StatusEffectManager

- **実装クラス:** `StatusEffectManager`
- **ファイルパス:** `Assets/_Project/Features/Templates/ActionRPG/Scripts/Combat/StatusEffectManager.cs`
- **概要:** バフやデバフなどのステータス効果を管理します。効果の適用、持続時間、スタックなどを処理します。
- **主要なメソッドとプロパティ:**
    - `ApplyStatusEffect(StatusEffectData effectData, GameObject source)`: ステータス効果を適用します。
    - `RemoveStatusEffect(string effectName)`: 指定した名前のステータス効果を解除します。
    - `HasEffect(string effectName)`: 特定の効果が付与されているか確認します。

---

### AdventureTemplateManager

- **実装クラス:** `AdventureTemplateManager`
- **ファイルパス:** `Assets/_Project/Features/Templates/Adventure/Scripts/AdventureTemplateManager.cs`
- **概要:** アドベンチャーゲームテンプレート全体の管理を行います。対話、クエスト、インベントリ、インタラクションシステムを統合します。
- **主要なメソッドとプロパティ:**
    - `ActivateTemplate()` / `DeactivateTemplate()`: テンプレートの有効/無効を切り替えます。
    - `UpdateStoryProgress(string newPhase)`: 物語の進行状況を更新します。
    - `DialogueManager`, `QuestManager`, `InventoryManager`, `InteractionManager`: 各サブシステムへのアクセスを提供します。

---

### AdventureInteractionSystem

- **実装クラス:** `AdventureInteractionSystem`
- **ファイルパス:** `Assets/_Project/Features/Templates/Adventure/Scripts/Interaction/AdventureInteractionSystem.cs`
- **概要:** アドベンチャーゲームにおけるプレイヤーとオブジェクトのインタラクションを処理します。
- **主要なメソッドとプロパティ:**
    - `StartInteraction()` / `EndInteraction()`: インタラクションを開始・終了します。
    - `SetInteractionRange(float range)`: インタラクション可能な距離を設定します。
    - `HasInteractableInRange()`: 範囲内にインタラクション可能なオブジェクトがあるか確認します。

---

### InteractionManager

- **実装クラス:** `InteractionManager`
- **ファイルパス:** `Assets/_Project/Features/Templates/Adventure/Scripts/Interaction/InteractionManager.cs`
- **概要:** アドベンチャーゲームのインタラクション可能なオブジェクトを検出し、管理します。
- **主要なメソッドとプロパティ:**
    - `TryInteract()`: 現在のターゲットとインタラクションを試みます。
    - `GetNearestInteractable(Vector3 position)`: 指定位置から最も近いインタラクション可能オブジェクトを取得します。
    - `CurrentTarget`: 現在のインタラクションターゲットを取得します。

---

### AdventureInventoryManager

- **実装クラス:** `AdventureInventoryManager`
- **ファイルパス:** `Assets/_Project/Features/Templates/Adventure/Scripts/Inventory/AdventureInventoryManager.cs`
- **概要:** アドベンチャーゲームに特化したインベントリを管理します。クエストアイテムや重要アイテムの管理機能が含まれます。
- **主要なメソッドとプロパティ:**
    - `AddItem(AdventureItemData itemData, int quantity)`: アイテムを追加します。
    - `UseItem(AdventureItemData itemData)`: アイテムを使用します。
    - `HasItem(AdventureItemData itemData, int requiredQuantity)`: アイテムを所持しているか確認します。

---

### AdventurePlayerController

- **実装クラス:** `AdventurePlayerController`
- **ファイルパス:** `Assets/_Project/Features/Templates/Adventure/Scripts/Player/AdventurePlayerController.cs`
- **概要:** アドベンチャーゲーム用のプレイヤーコントローラーです。移動、ジャンプ、インタラクション入力などを処理します。
- **主要なメソッドとプロパティ:**
    - `OnMove(InputAction.CallbackContext context)`: 移動入力を受け取ります。
    - `OnInteract(InputAction.CallbackContext context)`: インタラクション入力を受け取ります。
    - `TryPickupItem(AdventureItemData item, int quantity)`: アイテムの取得を試みます。

---

### QuestManager

- **実装クラス:** `QuestManager`
- **ファイルパス:** `Assets/_Project/Features/Templates/Adventure/Scripts/Quest/QuestManager.cs`
- **概要:** アドベンチャーゲームのクエストを管理します。クエストの受注、進捗追跡、完了、報酬処理などを行います。
- **主要なメソッドとプロパティ:**
    - `StartQuest(QuestData questData)`: 新しいクエストを開始します。
    - `UpdateObjectiveProgress(QuestData questData, string objectiveId, int progress)`: クエスト目標の進捗を更新します。
    - `CompleteQuest(QuestData questData)`: クエストを完了します。
    - `IsQuestActive(QuestData questData)`: クエストが進行中か確認します。

---

### StrategyBuildingManager

- **実装クラス:** `StrategyBuildingManager`
- **ファイルパス:** `Assets/_Project/Features/Templates/Strategy/Scripts/Buildings/StrategyBuildingManager.cs`
- **概要:** ストラテジーゲームの建設システムを管理します。建物の配置、建設、アップグレードなどを担当します。
- **主要なメソッドとプロパティ:**
    - `StartBuildingPlacement(BuildingType buildingType)`: 建物の配置モードを開始します。
    - `PlaceBuilding(BuildingType buildingType, Vector3 position)`: 指定位置に建物を配置します。
    - `DestroyBuilding(StrategyBuilding building)`: 建物を破壊します。

---

### StrategyResourceManager

- **実装クラス:** `StrategyResourceManager`
- **ファイルパス:** `Assets/_Project/Features/Templates/Strategy/Scripts/Resources/StrategyResourceManager.cs`
- **概要:** ストラテジーゲームのリソース（木材、石材、金など）を管理します。リソースの収集、消費、貯蔵などを処理します。
- **主要なメソッドとプロパティ:**
    - `AddResource(ResourceType type, int amount)`: リソースを追加します。
    - `ConsumeResource(ResourceType type, int amount)`: リソースを消費します。
    - `CanAfford(ResourceType type, int amount)`: 指定量のリソースを消費可能か確認します。
    - `GetResource(ResourceType type)`: 現在のリソース量を取得します。