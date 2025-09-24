using UnityEngine;
using UnityEditor;
using asterivo.Unity60.Core.ScriptableObjects.Data;

namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// GameDataSettingsアセットを作成するためのエディタ拡張
    /// </summary>
    public static class GameDataAssetCreator
    {
        [MenuItem("Assets/Create/Project/Core/GameData Settings Asset")]
        private static void CreateGameDataSettings()
        {
            // アセットを作成
            GameDataSettings asset = ScriptableObject.CreateInstance<GameDataSettings>();
            
            // 選択されたフォルダまたはデフォルトパスを取得
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
            {
                path = "Assets/_Project/Core/ScriptableObjects/Data";
            }
            else if (System.IO.Path.HasExtension(path))
            {
                path = path.Replace(System.IO.Path.GetFileName(path), "");
            }
            
            // ディレクトリが存在しない場合は作成
            if (!AssetDatabase.IsValidFolder(path))
            {
                System.IO.Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
            
            // ユニークなアセット名を生成
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(path + "/DefaultGameData.asset");
            
            // アセットを作成して保存
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            
            // 作成されたアセットを選択
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            
            UnityEngine.Debug.Log($"GameData Settings asset created at: {assetPath}");
        }
        
        [MenuItem("Tools/Project/Create Default GameData Asset")]
        private static void CreateDefaultGameDataAsset()
        {
            // 既存のアセットをチェック
            string[] guids = AssetDatabase.FindAssets("t:GameDataSettings");
            if (guids.Length > 0)
            {
                string existingPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                UnityEngine.Debug.Log($"GameData Settings asset already exists at: {existingPath}");
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameDataSettings>(existingPath);
                return;
            }
            
            // 新しいアセットを作成
            GameDataSettings asset = ScriptableObject.CreateInstance<GameDataSettings>();
            
            // デフォルト値を設定（既にクラス内で設定済みだが念のため）
            asset.name = "DefaultGameData";
            
            // 保存先ディレクトリを確保
            string directory = "Assets/_Project/Core/ScriptableObjects/Data";
            if (!AssetDatabase.IsValidFolder(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }
            
            // アセットを作成
            string assetPath = $"{directory}/DefaultGameData.asset";
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            
            // 作成されたアセットを選択
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            
            UnityEngine.Debug.Log($"Default GameData asset created at: {assetPath}");
        }
    }
}
