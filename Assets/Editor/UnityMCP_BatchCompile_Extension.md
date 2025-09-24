# Unity MCP BatchCompile Extension Documentation

## Unity MCP Server 側の拡張方法

Unity MCPサーバーに`batch_compile`機能を追加するには、以下の変更が必要です：

### 1. 新しいツール定義の追加（Python版の例）

```python
# unity_mcp_server.py に追加

@server.tool()
def batch_compile(
    ctx: Context,
    project_path: str = None,
    auto_restart: bool = True,
    save_before_compile: bool = True
) -> dict:
    """
    バッチモードでUnityプロジェクトをコンパイルし、オプションで再起動します

    Args:
        ctx: MCPコンテキスト
        project_path: プロジェクトパス（省略時は現在のプロジェクト）
        auto_restart: コンパイル後に自動的にエディタを再起動するか
        save_before_compile: コンパイル前にシーン/アセットを保存するか

    Returns:
        実行結果の辞書
    """
    try:
        # Unity側のBatchCompileManagerを呼び出す
        if save_before_compile:
            # まず保存
            result = execute_unity_method(
                "BatchCompileManager.SaveAllAssetsAndScenes"
            )
            if not result.get("success"):
                return {"success": False, "error": "Failed to save assets"}

        # バッチコンパイル実行
        params = {
            "projectPath": project_path or get_current_project_path(),
            "autoRestart": auto_restart
        }

        result = execute_unity_method(
            "BatchCompileManager.SaveCompileAndRestartWithOptions",
            params
        )

        return {
            "success": True,
            "message": "Batch compile initiated",
            "data": {
                "project_path": params["projectPath"],
                "auto_restart": auto_restart,
                "log_file": f"{params['projectPath']}/Logs/BatchCompile.log"
            }
        }
    except Exception as e:
        return {"success": False, "error": str(e)}
```

### 2. 既存の`manage_editor`への統合（代替案）

```python
# manage_editor 関数の拡張

def manage_editor(ctx: Context, action: str, **kwargs) -> dict:
    # 既存のアクション...

    if action == "batch_compile":
        # バッチコンパイル処理
        project_path = kwargs.get("project_path")
        auto_restart = kwargs.get("auto_restart", True)

        # Unity側のメソッドを実行
        method_name = "BatchCompileManager.SaveCompileAndRestartWithOptions"
        params = [project_path, auto_restart]

        return execute_editor_method(method_name, params)

    # その他のアクション...
```

## 使用例

### Claude/VS Code から Unity MCPを使用

```javascript
// JavaScript/TypeScript での使用例
const unityMCP = await getMCPClient("unity-mcp");

// バッチコンパイルを実行
const result = await unityMCP.callTool("batch_compile", {
    project_path: "D:/UnityProjects/URP3D_Base01",
    auto_restart: true,
    save_before_compile: true
});

if (result.success) {
    console.log("バッチコンパイル開始:", result.data.log_file);
} else {
    console.error("エラー:", result.error);
}
```

### Python から直接使用

```python
from unity_mcp import UnityMCPClient

client = UnityMCPClient()
result = client.batch_compile(
    project_path="D:/UnityProjects/URP3D_Base01",
    auto_restart=True
)

if result["success"]:
    print(f"ログファイル: {result['data']['log_file']}")
```

## 実装のポイント

1. **非同期処理**: バッチコンパイルは時間がかかるため、非同期で実行
2. **ログ監視**: コンパイル結果のログファイルを監視して成功/失敗を判定
3. **エラーハンドリング**: コンパイルエラーの詳細を返す
4. **プログレス報告**: 可能であれば進捗状況を報告

## セキュリティ考慮事項

- プロジェクトパスの検証（不正なパスへのアクセス防止）
- Unity実行ファイルパスの検証
- ログファイルへの書き込み権限確認

## テスト方法

```bash
# Unity MCPサーバーをテストモードで起動
python unity_mcp_server.py --test

# テストコマンド実行
python -m pytest test_batch_compile.py
```