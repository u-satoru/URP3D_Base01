# UICanvas.prefab エラー解決手順

## 問題の概要
`UICanvas.prefab`が存在しないスクリプト参照を含んでおり、保存時にエラーが発生しています。

## 現在の状況
- ❌ `UICanvas.prefab` - 削除済み（問題があったため）
- ✅ `UICanvas_broken.prefab.backup` - バックアップファイル（問題あり）
- ✅ `BasicUICanvas.prefab` - 新規作成したクリーンなPrefab

## Unity Editorでの解決手順

### 方法1: Unity Editorのキャッシュクリア（推奨）

1. **Unity Editorを完全に終了**
   - File → Exit でUnityを閉じる

2. **Libraryフォルダをクリア**（オプション）
   ```
   D:\UnityProjects\URP3D_Base01\Library\
   ```
   - `ShaderCache`フォルダを削除
   - `StateCache`フォルダを削除

3. **Unity Editorを再起動**
   - プロジェクトを開き直す

4. **新しいPrefabを使用**
   - `BasicUICanvas.prefab`を右クリック
   - Rename → `UICanvas` に変更

### 方法2: 手動でPrefabを再作成

1. **Unity Editor内で新規Canvas作成**
   - Hierarchy → 右クリック → UI → Canvas

2. **設定を調整**
   - Canvas Scaler追加
   - Screen Size: 1920x1080
   - Scale Mode: Scale With Screen Size

3. **Prefab化**
   - HierarchyからProjectビューにドラッグ
   - `Assets/_Project/Features/Templates/Common/Prefabs/UI/UICanvas.prefab`として保存

### 方法3: BasicUICanvasをリネーム

1. **Project窓で確認**
   - `Assets/_Project/Features/Templates/Common/Prefabs/UI/`を開く

2. **BasicUICanvas.prefabを選択**
   - F2キーまたは右クリック → Rename
   - `UICanvas`に名前変更

3. **meta ファイル自動更新**
   - Unityが自動的にmetaファイルを更新

## トラブルシューティング

### まだエラーが出る場合

1. **Console窓をクリア**
   - Console窓 → Clear ボタン
   - またはCtrl+Alt+C（作成したショートカット）

2. **プロジェクト全体を再インポート**
   - Assets → Reimport All（時間がかかります）

3. **問題のあるシーンを確認**
   - どのシーンでUICanvas.prefabを使用しているか確認
   - シーン内のMissing Prefab参照を修正

## 確認項目

- [ ] BasicUICanvas.prefabが正常に開ける
- [ ] エラーメッセージが表示されない
- [ ] Prefabの保存が可能
- [ ] シーンで正常に使用できる

## 今後の対応

1. 問題が解決したら`UICanvas_broken.prefab.backup`を削除
2. 全シーンでUICanvas参照を更新
3. Gitにコミット

---

**作成日**: 2025年9月24日
**問題**: UICanvas.prefab保存エラー
**解決策**: クリーンなPrefab再作成
