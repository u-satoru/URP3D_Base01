using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// バッチモードコンパイルを管理するエディタ拡張
    /// Unity MCPサーバーから呼び出し可能
    /// </summary>
    public class BatchCompileManager
    {
        private static string GetUnityExecutablePath()
        {
            return EditorApplication.applicationPath;
        }

        private static string GetProjectPath()
        {
            return Path.GetDirectoryName(Application.dataPath);
        }

        /// <summary>
        /// 保存、終了、バッチコンパイル、再起動を実行
        /// </summary>
        [MenuItem("Tools/Batch Compile/Save Compile and Restart")]
        public static void SaveCompileAndRestart()
        {
            SaveCompileAndRestartWithOptions(GetProjectPath(), true);
        }

        /// <summary>
        /// Unity MCPから直接呼び出すためのエントリーポイント
        /// プロジェクトパスを文字列として受け取る簡易版
        /// </summary>
        public static void BatchCompileProject()
        {
            // デフォルト設定で実行
            SaveCompileAndRestartWithOptions(null, true);
        }

        /// <summary>
        /// Unity MCPから呼び出すためのエントリーポイント（プロジェクトパス指定版）
        /// コマンドライン引数から取得: -customProjectPath "path"
        /// </summary>
        public static void BatchCompileProjectWithPath()
        {
            string projectPath = GetCommandLineArg("-customProjectPath");
            bool autoRestart = GetCommandLineArg("-autoRestart") != "false";
            
            SaveCompileAndRestartWithOptions(projectPath, autoRestart);
        }

        private static string GetCommandLineArg(string name)
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == name && i + 1 < args.Length)
                {
                    return args[i + 1];
                }
            }
            return null;
        }

        /// <summary>
        /// Unity MCPから呼び出し可能なメソッド
        /// </summary>
        public static void SaveCompileAndRestartWithOptions(string projectPath = null, bool autoRestart = true)
        {
            if (string.IsNullOrEmpty(projectPath))
            {
                projectPath = GetProjectPath();
            }

            // 設定ファイルに情報を保存（バッチモード完了後の再起動用）
            string configPath = Path.Combine(projectPath, "Temp", "BatchCompileConfig.json");
            var config = new BatchCompileConfig
            {
                ProjectPath = projectPath,
                AutoRestart = autoRestart,
                UnityPath = GetUnityExecutablePath()
            };
            
            Directory.CreateDirectory(Path.GetDirectoryName(configPath));
            File.WriteAllText(configPath, JsonUtility.ToJson(config, true));

            // シーンとアセットを保存
            SaveAllAssetsAndScenes();

            // バッチコンパイルを起動する外部プロセスを準備
            string unityExe = GetUnityExecutablePath();
            string logFile = Path.Combine(projectPath, "Logs", "BatchCompile.log");
            
            // バッチモードコンパイルのコマンドライン引数
            string batchArgs = $"-quit -batchmode -projectPath \"{projectPath}\" -logFile \"{logFile}\" -executeMethod BatchCompileManager.OnBatchCompileComplete";

            // 外部プロセスとしてバッチコンパイルを起動
            var startInfo = new ProcessStartInfo
            {
                FileName = unityExe,
                Arguments = batchArgs,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            UnityEngine.Debug.Log($"[BatchCompileManager] バッチモードコンパイルを開始します: {projectPath}");
            
            // Unity Editorを終了（バッチプロセスは別プロセスで実行される）
            EditorApplication.delayCall += () =>
            {
                Process.Start(startInfo);
                EditorApplication.Exit(0);
            };
        }

        /// <summary>
        /// バッチコンパイル完了時に呼ばれるコールバック
        /// </summary>
        public static void OnBatchCompileComplete()
        {
            string projectPath = GetProjectPath();
            string configPath = Path.Combine(projectPath, "Temp", "BatchCompileConfig.json");

            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                var config = JsonUtility.FromJson<BatchCompileConfig>(json);

                if (config.AutoRestart)
                {
                    // Unity Editorを再起動
                    Process.Start(config.UnityPath, $"-projectPath \"{config.ProjectPath}\"");
                }

                // 設定ファイルを削除
                File.Delete(configPath);
            }

            UnityEngine.Debug.Log("[BatchCompileManager] バッチコンパイル完了");
        }

        /// <summary>
        /// すべてのシーンとアセットを保存
        /// </summary>
        public static void SaveAllAssetsAndScenes()
        {
            // 開いているシーンをすべて保存
            if (!EditorSceneManager.SaveOpenScenes())
            {
                UnityEngine.Debug.LogWarning("[BatchCompileManager] シーンの保存に失敗しました");
            }

            // アセットの変更も保存
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            UnityEngine.Debug.Log("[BatchCompileManager] すべてのシーンとアセットを保存しました");
        }

        /// <summary>
        /// コンパイルエラーがあるかチェック
        /// </summary>
        public static bool HasCompilationErrors()
        {
            return UnityEditor.Compilation.CompilationPipeline
                .GetAssemblies()
                .Any(a => a.compiledAssemblyReferences.Length == 0);
        }

        [System.Serializable]
        private class BatchCompileConfig
        {
            public string ProjectPath;
            public string UnityPath;
            public bool AutoRestart;
        }
    }
}
