# Template層シーン動作確認手順書
**作成日**: 2025年9月24日
**目的**: Phase 4.1で移動したアセットの動作確認とMissing Reference解消

## 事前準備

### Unity Editor起動前チェック
```
□ Gitステータス確認（git status）
□ 最新のfeature/3-layer-architecture-migrationブランチ
□ バックアップの確認
```

### Unity Editor起動
```
1. Unity Hub起動
2. プロジェクト: URP3D_Base01を選択
3. Unity Version: 6000.0.42f1 確認
4. Safe Modeダイアログが出た場合は "Ignore" を選択
```

## Missing Reference確認手順

### 手順1: プロジェクト全体スキャン

1. **Console Windowクリア**
   - Window > General > Console
   - Clear ボタンクリック
   - Clear on Play 有効化
   - Error Pause 有効化

2. **アセットデータベース更新**
   ```
   Edit > Refresh (Ctrl+R)
   Assets > Reimport All（時間がかかる場合はスキップ可）
   ```

3. **Missing Reference検索**
   - Window > Analysis > Missing References Finder（ない場合は手動確認）
   - または Project Windowで検索: "t:Missing"

### 手順2: シーン別確認

## 📁 Stealth Template

### StealthAudioTest.unity
```
場所: Assets/_Project/Features/Templates/Stealth/Scenes/Tests/
```

**開く手順**:
1. Project Window > Assets/_Project/Features/Templates/Stealth/Scenes/Tests
2. StealthAudioTest.unity をダブルクリック
3. シーンロード完了まで待機

**確認項目**:
```
□ シーンが正常にロード
□ Hierarchy内の黄色警告マークなし
□ Console Windowにエラーなし
□ NPCオブジェクト存在確認
  └□ VisualSensor Component
  └□ AuditorySensor Component
  └□ OlfactorySensor Component（あれば）
□ Player オブジェクト存在確認
  └□ PlayerController Component
  └□ StealthController Component（あれば）
□ AudioManager 存在確認
```

**実行テスト**:
1. Playボタン押下
2. 10秒間実行
3. エラー確認
4. Stopボタン押下

**Missing Reference修正（発見時）**:
```
1. 該当GameObjectを選択
2. Inspectorで黄色の"Missing"を確認
3. 正しいアセットを再割り当て:
   - Scripts: Features/AI/Sensors/から選択
   - Prefabs: Features/Templates/Common/Prefabsから選択
   - ScriptableObjects: Features/Templates/Stealth/Configurationから選択
```

---

## 📁 TPS Template

### TPSTemplateTest.unity
```
場所: Assets/_Project/Features/Templates/TPS/Scenes/Tests/
```

**確認項目**:
```
□ CinemachineVirtualCamera設定
  └□ Follow Target（Player Transform）
  └□ Look At Target（Player Transform）
□ WeaponManager 存在確認
□ PlayerHealth Component
□ InputManager 参照
```

**カメラ視点切り替えテスト**:
```
1. Play実行
2. Tab キーで視点切り替え（該当する場合）
3. マウス右クリックでエイムモード
4. カメラ遷移の滑らかさ確認
```

---

## 📁 Common Templates（共通機能）

### AudioSystemDemo.unity
```
場所: Assets/_Project/Features/Templates/Common/Scenes/Demos/
```

**確認項目**:
```
□ AudioManager
  └□ ServiceLocator登録確認
  └□ AudioSource Components
□ 3D Audio Sources
  └□ Spatial Blend = 1.0
  └□ Min/Max Distance設定
□ GameEvent References
  └□ OnPlaySound Event
  └□ OnStopSound Event
```

### BasicMovementDemo.unity
```
□ Player GameObject
  └□ CharacterController
  └□ PlayerController Script
  └□ Animator Component
□ Input System References
  └□ PlayerInput Component
  └□ Input Action Asset割り当て
```

### CombatSystemDemo.unity
```
□ CommandInvoker
  └□ DamageCommand Pool
  └□ HealCommand Pool
□ Health Components
  └□ IHealth実装確認
□ UI Health Display
  └□ GameEventListener設定
```

### UISystemDemo.unity
```
□ Canvas設定
  └□ Canvas Scaler
  └□ Graphic Raycaster
□ EventSystem
□ UI Elements
  └□ Button OnClick Events
  └□ Slider Value Changed Events
```

### EventSystemDemo.unity
```
□ GameEventAssets フォルダ参照
□ EventListener Components
□ Event発行元オブジェクト
```

---

## 📁 Adventure Template

### TestAdventureProject.unity
```
場所: Assets/_Project/Features/Templates/Adventure/Scenes/Tests/
```

**確認項目**:
```
□ DialogueSystem
□ QuestManager
□ InteractionSystem
  └□ IInteractable実装
```

---

## Missing Reference一括修正スクリプト

必要に応じて以下のEditor Scriptを作成・実行:

```csharp
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MissingReferencesFixer : EditorWindow
{
    [MenuItem("Tools/Fix Missing References")]
    public static void ShowWindow()
    {
        GetWindow<MissingReferencesFixer>("Fix Missing Refs");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Scan Current Scene"))
        {
            ScanScene();
        }

        if (GUILayout.Button("Auto Fix Common Issues"))
        {
            AutoFixCommonIssues();
        }
    }

    void ScanScene()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int missingCount = 0;

        foreach (var obj in allObjects)
        {
            Component[] components = obj.GetComponents<Component>();
            foreach (var comp in components)
            {
                if (comp == null)
                {
                    Debug.LogWarning($"Missing Component on: {obj.name}", obj);
                    missingCount++;
                }
            }
        }

        Debug.Log($"Found {missingCount} missing references");
    }

    void AutoFixCommonIssues()
    {
        // ServiceLocator再参照
        var audioManager = GameObject.Find("AudioManager");
        if (audioManager != null)
        {
            // Fix audio manager references
        }

        // GameEvent再参照
        var events = Resources.LoadAll<ScriptableObject>("Events");
        // Re-assign events

        AssetDatabase.SaveAssets();
        Debug.Log("Auto-fix completed");
    }
}
```

---

## テスト結果記録フォーマット

### シーンごとの記録
```
シーン名: [_______________]
テスト日時: [2025/09/24 __:__]
テスト実施者: [_______________]

チェック項目:
□ シーンロード: OK / NG
□ Missing References: [__]件
□ コンパイルエラー: [__]件
□ ランタイムエラー: [__]件
□ 警告: [__]件

パフォーマンス:
- FPS: [___] fps
- メモリ: [___] MB
- ドローコール: [___]

問題詳細:
[________________________________]
[________________________________]

修正内容:
[________________________________]
[________________________________]
```

---

## トラブルシューティング

### よくある問題と解決策

**1. "Missing (Mono Script)" エラー**
```
原因: .metaファイルのGUID不一致
解決:
1. 該当スクリプトを再インポート
2. Library/ScriptAssembliesを削除して再ビルド
```

**2. "The referenced script on this Behaviour is missing!"**
```
原因: スクリプトファイル移動による参照切れ
解決:
1. 正しいスクリプトを再割り当て
2. プレハブの場合はPrefab > Reimport
```

**3. EventListener参照切れ**
```
原因: GameEventアセット移動
解決:
1. Features/Templates/[Genre]/Events/から正しいEventを選択
2. GameEventListenerコンポーネントで再設定
```

**4. Prefab Missing**
```
原因: Prefab移動によるシーン内参照切れ
解決:
1. Hierarchy内のオブジェクトを選択
2. Prefab > Unpack Completely
3. 新しいPrefabパスから再度Prefab化
```

---

## 完了確認

### 最終チェックリスト
```
□ 全14シーンのテスト完了
□ Missing Reference 0件達成
□ コンパイルエラー 0件
□ 主要機能の動作確認
□ テスト結果の記録完了
□ 修正内容のコミット準備
```

### 次のアクション
1. テスト結果をPhase4実施報告書に記載
2. 修正が必要な場合はGitにコミット
3. Phase 4.3（パフォーマンステスト）準備

---

**注意事項**:
- Unity Editorの自動リフレッシュを有効にしておく
- 大きな変更後は必ずFile > Save Projectを実行
- テスト中は定期的にプロジェクトを保存する