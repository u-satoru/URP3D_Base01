# Audio系Singleton段階的無効化テスト実行手順書

**作成日**: 2025年9月12日  
**対象**: Unity開発者  
**難易度**: 初級〜中級  
**所要時間**: 約10分

---

## 📋 **概要**

このドキュメントでは、Migration Reportで完了した「Audio系Singletonの段階的無効化実行」機能を、Unity Editor上で実際にテストする詳細手順を説明します。

### **テストの目的**
- AudioManager.csとSpatialAudioManager.csのSingleton段階的無効化が正常動作することを確認
- 開発環境→ステージング環境→本番環境の段階的ロールアウトをシミュレーション
- FeatureFlagsによる制御可能な移行プロセスの動作検証

---

## 🚀 **事前準備**

### **必要な環境**
- ✅ Unity Editor 6000.0.42f1 以上
- ✅ URP3D_Base01プロジェクトが開けること
- ✅ コンパイルエラーがないこと

### **確認事項**
```bash
# プロジェクトのパスが正しいことを確認
D:\UnityProjects\URP3D_Base01
```

---

## 📝 **詳細実行手順**

### **Step 1: Unityプロジェクトを開く**

1. **Unity Hubを起動**
2. **"URP3D_Base01"プロジェクトをクリック**
3. Unity Editorが開くまで待機（約1-2分）
4. **Consoleウィンドウでエラーがないことを確認**
   - `Window` → `General` → `Console` でConsole表示
   - 赤いエラーメッセージがないことを確認

![Unity起動確認](./images/unity-startup-check.png)

---

### **Step 2: 新しいシーンの準備**

1. **新しいシーンを作成**
   - `File` → `New Scene` 
   - `Basic (URP)` を選択
   - `Create` をクリック

2. **シーンを保存**
   - `Ctrl + S` を押下
   - `Assets/_Project/Scenes/` に移動
   - ファイル名: `AudioSingletonTestScene` と入力
   - `Save` をクリック

![新しいシーン作成](./images/new-scene-creation.png)

---

### **Step 3: テストオブジェクトの作成**

1. **空のGameObjectを作成**
   - Hierarchyウィンドウで右クリック
   - `Create Empty` を選択

2. **オブジェクト名を変更**
   - 作成されたGameObjectを選択
   - Inspectorで名前を `AudioSingletonTester` に変更
   - `Enter` キーで確定

![GameObject作成](./images/gameobject-creation.png)

---

### **Step 4: テストスクリプトのアタッチ**

1. **AudioSingletonTesterオブジェクトを選択**
2. **Inspectorで"Add Component"をクリック**
3. **検索ボックスに"AudioSingleton"と入力**
4. **"AudioSingletonGradualDisablingScript"を選択**

![スクリプトアタッチ](./images/script-attach.png)

### **⚠️ スクリプトが見つからない場合**

スクリプトが表示されない場合の対処法：

1. **Project ウィンドウで確認**
   - `Assets/_Project/Tests/Core/Services/AudioSingletonGradualDisablingScript.cs` が存在するか確認

2. **再コンパイル実行**
   - `Assets` → `Reimport All` を実行
   - 完了まで数分待機

3. **Console でエラー確認**
   - コンパイルエラーがある場合は修正が必要

---

### **Step 5: テスト設定の確認**

AudioSingletonGradualDisablingScriptがアタッチされると、Inspectorに以下の項目が表示されます：

#### **Test Configuration**
- ✅ **Enable Detailed Logging**: `True` （推奨）
- ⚠️ **Execute On Start**: `False` （手動実行推奨）

#### **Test Results - Read Only**  
- 📝 **Test Results**: （テスト実行後に結果が表示される領域）

#### **Current FeatureFlags Status**
- 📊 現在のFeatureFlagsの状態がリアルタイム表示される

![Inspector設定画面](./images/inspector-settings.png)

---

### **Step 6: テスト実行**

#### **方法1: Context Menuから実行（推奨）**

1. **AudioSingletonGradualDisablingScriptのコンポーネント名を右クリック**
2. **"Execute Gradual Disabling Test"を選択**

![Context Menu実行](./images/context-menu-execution.png)

#### **方法2: Play Mode中に実行**

1. **Execute On Start を `True` に設定**
2. **Play ボタン（▶️）をクリック**
3. **Playモードに入ると自動実行される**

---

### **Step 7: テスト結果の確認**

テスト実行後、Inspector の **Test Results** 欄に以下のような結果が表示されます：

```
=== Audio System Singleton Gradual Disabling Test ===

--- Phase 1: Development Environment ---
EnableDay1TestWarnings executed
DisableLegacySingletons: False
EnableMigrationWarnings: True
✅ Expected behavior: AudioManager.Instance = VALID

--- Phase 2: Staging Environment ---
Staging environment settings applied
DisableLegacySingletons: False
UseServiceLocator: True
EnableMigrationWarnings: True
✅ Expected behavior: SpatialAudioManager.Instance = VALID

--- Phase 3: Production Environment ---
EnableDay4SingletonDisabling executed
DisableLegacySingletons: True
UseServiceLocator: True
UseNewAudioService: True
UseNewSpatialService: True
✅ Expected behavior: AudioManager.Instance = NULL
✅ Expected behavior: SpatialAudioManager.Instance = NULL

--- Phase 4: Emergency Rollback ---
Emergency rollback executed
DisableLegacySingletons: False
UseServiceLocator: False
✅ Expected behavior: AudioManager.Instance = VALID

=== Test Completed Successfully ===
```

![テスト結果表示](./images/test-results-display.png)

---

### **Step 8: 詳細ログの確認**

**Enable Detailed Logging** が `True` の場合、より詳細な情報がConsole に出力されます：

1. **Consoleウィンドウを開く** (`Window` → `General` → `Console`)
2. **"[AudioSingletonTest]"でフィルタリング**
3. **各フェーズの詳細な実行ログを確認**

![Console詳細ログ](./images/console-detailed-logs.png)

---

## 🔍 **テスト結果の読み方**

### **✅ 成功パターン**

```
✅ Expected behavior: AudioManager.Instance = NULL
```
- **意味**: DisableLegacySingletons=true時に正しくnullが返された
- **状態**: 正常動作

```
✅ Expected behavior: AudioManager.Instance = VALID  
```
- **意味**: DisableLegacySingletons=false時に正しくインスタンスが返された
- **状態**: 正常動作

### **❌ 異常パターン**

```
❌ UNEXPECTED: AudioManager.Instance should be null but got valid instance
```
- **意味**: 無効化されているはずなのにインスタンスが返された
- **対処**: FeatureFlags設定を確認

```  
❌ AudioManager.Instance access failed: System.Exception
```
- **意味**: アクセス中に例外が発生
- **対処**: エラー詳細をConsoleで確認

---

## 🛠️ **トラブルシューティング**

### **問題1: スクリプトが見つからない**

**症状**: Add Component時にAudioSingletonGradualDisablingScriptが見つからない

**解決方法**:
1. Project ウィンドウでスクリプトファイルの存在確認
2. `Assets` → `Reimport All` 実行
3. Console でコンパイルエラー確認・修正

### **問題2: テスト結果が表示されない**

**症状**: Execute Gradual Disabling Test実行後も Test Results が空白

**解決方法**:
1. **Enable Detailed Logging** を `True` に設定
2. Console ウィンドウで "[AudioSingletonTest]" ログを確認
3. エラーメッセージがある場合は内容を確認

### **問題3: 全てのテストが失敗する**

**症状**: 全てのフェーズで ❌ UNEXPECTED が表示される

**解決方法**:
1. **Current FeatureFlags Status** を確認
2. 他のスクリプトでFeatureFlagsが変更されていないか確認
3. `Reset FeatureFlags to Default` を実行

---

## 📊 **追加機能の使用方法**

### **FeatureFlagsのリアルタイム監視**

Inspector の **Current FeatureFlags Status** セクションで、現在のフラグ状態をリアルタイム確認できます：

- `Current Disable Legacy Singletons`: 現在の無効化状態
- `Current Enable Migration Warnings`: 現在の警告有効状態  
- `Current Use Service Locator`: 現在のServiceLocator使用状態

### **手動操作用Context Menu**

右クリックメニューから以下の操作が可能：

1. **"Execute Gradual Disabling Test"**: フル段階的テスト実行
2. **"Reset FeatureFlags to Default"**: フラグをデフォルト状態にリセット
3. **"Get Current Status"**: 現在のフラグ状態を Test Results に表示

---

## 📋 **テスト完了チェックリスト**

テスト実行後、以下の項目を確認してください：

### **Phase 1 (Development)**
- [ ] DisableLegacySingletons = False
- [ ] EnableMigrationWarnings = True  
- [ ] AudioManager.Instance = VALID
- [ ] SpatialAudioManager.Instance = VALID

### **Phase 2 (Staging)**  
- [ ] DisableLegacySingletons = False
- [ ] UseServiceLocator = True
- [ ] AudioManager.Instance = VALID
- [ ] SpatialAudioManager.Instance = VALID

### **Phase 3 (Production)**
- [ ] DisableLegacySingletons = True
- [ ] UseServiceLocator = True
- [ ] UseNewAudioService = True
- [ ] UseNewSpatialService = True
- [ ] AudioManager.Instance = NULL
- [ ] SpatialAudioManager.Instance = NULL

### **Phase 4 (Emergency Rollback)**
- [ ] DisableLegacySingletons = False
- [ ] UseServiceLocator = False  
- [ ] AudioManager.Instance = VALID
- [ ] SpatialAudioManager.Instance = VALID

---

## 🎯 **期待される結果**

このテストが正常完了すれば、以下が確認できます：

1. **✅ Singleton Pattern Migration が100%完了**していること
2. **✅ 段階的ロールアウト機能**が正常動作すること  
3. **✅ 緊急ロールバック機能**が正常動作すること
4. **✅ FeatureFlagsによる制御**が適切に機能すること

---

## 📚 **関連ドキュメント**

- **Migration Report**: `Assets/_Project/Docs/Singleton_Pattern_Migration_Report.md`
- **FeatureFlags 仕様**: `Assets/_Project/Core/FeatureFlags.cs`  
- **テストスクリプト**: `Assets/_Project/Tests/Core/Services/AudioSingletonGradualDisablingScript.cs`

---

## ❓ **よくある質問**

### **Q: テストは毎回実行する必要がありますか？**
**A**: いいえ。Migration完了確認や機能変更時の検証時のみ実行してください。

### **Q: 本番環境でこのテストを実行しても大丈夫ですか？**
**A**: このテストはFeatureFlagsを変更するため、本番環境での実行は推奨しません。開発・テスト環境でのみ使用してください。

### **Q: テスト実行後、FeatureFlagsは元に戻りますか？**
**A**: いいえ。テスト実行後は変更された状態のままです。必要に応じて "Reset FeatureFlags to Default" を実行してください。

---

**📧 サポート**: 問題が発生した場合は、Console のエラーログと共に開発チームに相談してください。