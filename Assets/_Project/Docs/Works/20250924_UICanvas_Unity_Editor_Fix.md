# UICanvas.prefab Unity Editor修正手順

## ⚠️ 重要な注意事項
このエラーはファイルシステム側では既に対処済みですが、Unity Editorの内部キャッシュが原因で継続して表示されています。

## 現在のファイル状態
- `UICanvas.prefab` - **存在しません**（削除済み）
- `BasicUICanvas.prefab` - ✅ 新しくクリーンな状態で作成済み
- `UICanvas_broken.prefab.backup` - 問題のある古いファイル（バックアップ）

## Unity Editor内での解決手順

### 手順1: Unity Editorの完全リスタート 🔄

1. **Unityを完全に終了**
   - File → Exit
   - タスクマネージャーで Unity.exe プロセスが残っていないか確認

2. **以下のフォルダを削除**（安全）
   ```
   D:\UnityProjects\URP3D_Base01\Library\ArtifactDB
   D:\UnityProjects\URP3D_Base01\Library\SourceAssetDB
   D:\UnityProjects\URP3D_Base01\Temp\
   ```

3. **Unity Hubから再度プロジェクトを開く**

### 手順2: Unity Editor内でのPrefab確認 🔍

1. **Project窓で確認**
   - `Assets/_Project/Features/Templates/Common/Prefabs/UI/`を開く
   - 表示されているファイルを確認

2. **もしUICanvas.prefabが見える場合**
   - 右クリック → Delete
   - または右クリック → Show in Explorer で実際のファイルを確認

3. **BasicUICanvas.prefabを使用**
   - BasicUICanvas.prefabを選択
   - F2キーを押してリネーム → `UICanvas`

### 手順3: Console エラーのクリア 🧹

1. **Consoleウィンドウ**
   - Clear ボタンをクリック
   - またはメニュー: Tools → Clear Console（Ctrl+Alt+C）

2. **エラーが消えない場合**
   - Console窓の右上の3点メニュー → Clear on Recompile をON
   - スクリプトを軽く編集して再コンパイルを強制

### 手順4: 強制的な解決方法 💪

もし上記で解決しない場合：

1. **新規Canvas作成（Unity Editor内）**
   ```
   Hierarchy → 右クリック → UI → Canvas
   ```

2. **必要な設定を追加**
   - Canvas Scaler コンポーネント追加
   - Resolution: 1920x1080
   - UI Scale Mode: Scale With Screen Size

3. **Prefab として保存**
   - Hierarchyから直接ドラッグ
   - `Assets/_Project/Features/Templates/Common/Prefabs/UI/`
   - 名前: `UICanvas` として保存（上書き確認が出たらYes）

### 手順5: プロジェクト設定の確認 ⚙️

1. **Edit → Project Settings → Editor**
   - Enter Play Mode Settings
   - Reload Domain: ON（一時的に）

2. **Assets → Reimport All**（最終手段）
   - 時間がかかりますが、全アセットを再インポート

## 確認チェックリスト ✅

- [ ] Unity Editor再起動完了
- [ ] Library/ArtifactDB 削除完了
- [ ] BasicUICanvas.prefab が表示される
- [ ] Console にエラーが表示されない
- [ ] 新しいCanvasが正常に作成できる

## それでも解決しない場合

1. **プロジェクトのクリーンクローン**
   ```bash
   git status  # 変更を確認
   git add -A  # 変更を保存
   git commit -m "WIP: UICanvas fix"
   git clean -xfd  # 未追跡ファイルを削除（注意！）
   ```

2. **Unity バージョン確認**
   - Unity 6000.0.42f1 を使用しているか確認
   - Unity Hub → Installs → 歯車アイコン → Add modules で修復

## 問題の根本原因

このエラーは以下が原因の可能性があります：
- Unity Editorの内部アセットデータベースの不整合
- Libraryフォルダのキャッシュ破損
- 削除されたファイルへの参照が残存

---

**最終更新**: 2025年9月24日 12:00
**対象Unity**: 6000.0.42f1
**問題**: UICanvas.prefab の幽霊参照エラー
