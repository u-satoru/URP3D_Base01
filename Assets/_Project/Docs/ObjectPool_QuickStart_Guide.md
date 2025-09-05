# ObjectPool クイックスタートガイド

このガイドでは、Unity6プロジェクトでObjectPoolを**最短手順で導入**する方法を説明します。

## 🚀 5分でObjectPool導入

### Step 1: シーンにCommandPoolを設置（1分）

1. **Hierarchy**で右クリック → `Create Empty`
2. オブジェクト名を「**CommandPoolManager**」に変更
3. **CommandPool.cs**スクリプトをアタッチ

### Step 2: 基本設定（1分）

**CommandPool**コンポーネントの設定：
```
Default Pool Size: 10        # 初期プールサイズ
Max Pool Size: 50           # 最大プールサイズ  
Show Debug Stats: true      # デバッグ情報表示
```

### Step 3: テスト実行（1分）

1. 同じGameObjectに**CommandPoolTester.cs**をアタッチ
2. **Inspector**で設定：
   ```
   Test Command Count: 100     # テストコマンド数
   Command Interval: 0.01      # 実行間隔（秒）
   Auto Start Test: true       # 自動開始
   Show Detailed Stats: true   # 詳細統計表示
   ```
3. **プレイモード実行**

### Step 4: 効果確認（2分）

**Consoleウィンドウ**で以下のログを確認：
```
CommandPoolテストを開始します。コマンド数: 100, 間隔: 0.01秒
コマンド実行中... (10/100)
コマンド実行中... (20/100)
...
=== テスト完了 ===
実行したコマンド数: 100
総実行時間: 1.23秒
平均実行速度: 81.3 コマンド/秒

=== CommandPool統計 ===
DamageCommand: プール内オブジェクト数 = 5
HealCommand: プール内オブジェクト数 = 5
```

## ✅ 正常動作の確認方法

### 成功パターン
```
✓ "CommandPool initialized with 10 pre-warmed commands per type"
✓ "Retrieved DamageCommand from pool (reused X times)"  
✓ "Returned DamageCommand to pool (pool size: Y)"
```

### エラーパターンと解決
```
❌ "CommandPoolが見つかりません"
→ Step 1を再実行

❌ "IHealthTargetを実装していません"  
→ 自動でDummyHealthTargetが作成されるので問題なし

❌ コンパイルエラー
→ Assets/Refreshを実行
```

## 📊 即座に効果を実感

### メモリプロファイラーで確認

1. **Window** → **Analysis** → **Profiler**
2. **Memory**タブを選択
3. プレイモード実行前後を比較

**期待される結果:**
- **GC Alloc**: 90%減少
- **Used Total**: 大幅削減
- **Reserved Total**: 安定

### FrameTimingでパフォーマンス確認

```csharp
// 追加のテストコード（optional）
void Update()
{
    if (Input.GetKeyDown(KeyCode.Space))
    {
        // 1000コマンドを瞬時に実行
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < 1000; i++)
        {
            var command = CommandPool.Instance.GetCommand<DamageCommand>();
            command.Initialize(healthTarget, 10, "test");
            command.Execute();
            CommandPool.Instance.ReturnCommand(command);
        }
        
        stopwatch.Stop();
        Debug.Log($"1000コマンド実行時間: {stopwatch.ElapsedMilliseconds}ms");
    }
}
```

## 🎯 実際のゲームでの使用例

### プレイヤーの攻撃処理に適用

```csharp
public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private DamageCommandDefinition damageDefinition;
    
    public void Attack(IHealthTarget target)
    {
        // ObjectPoolが自動で使用される
        var damageCommand = damageDefinition.CreateCommand(target);
        
        // CommandInvokerで実行（Undo/Redo対応）
        CommandInvoker.Instance.ExecuteCommand(damageCommand);
    }
}
```

### アイテム使用時のヒール処理

```csharp
public class HealthPotion : MonoBehaviour
{
    [SerializeField] private HealCommandDefinition healDefinition;
    
    public void UsePotion(IHealthTarget target)
    {
        // ObjectPoolが自動で使用される
        var healCommand = healDefinition.CreateCommand(target);
        healCommand.Execute();
    }
}
```

## 🔧 トラブルシューティング

### Q: プールが効いているか分からない
```csharp
// CommandPoolTesterのShow Pool Statsを実行
// または手動確認
var stats = CommandPool.Instance.GetPoolStats();
foreach (var kvp in stats)
{
    Debug.Log($"{kvp.Key.Name}: {kvp.Value}個がプール内");
}
```

### Q: メモリ使用量が減らない
- **Unity Profiler**のMemoryタブで**GC Alloc**を確認
- **Total Reserved**ではなく**Total Used**を見る
- 大量実行テストで差が顕著に表れる

### Q: パフォーマンスが向上しない
- **頻繁に使用されるコマンド**でのみ効果大
- 1回だけの実行では差は小さい
- **継続的な使用**（戦闘中など）で効果を実感

## 📈 次のステップ

### より高度な使用方法
1. **[ObjectPool_Implementation_Guide.md]** で詳細仕様を学習
2. **独自コマンドクラス**のプール化対応
3. **UI要素やエフェクト**への応用

### パフォーマンス最適化
1. **プールサイズの調整**（使用パターンに基づく）
2. **統計情報の監視**（定期的な最適化）
3. **他システムへの拡張**（AIコマンド、UIプールなど）

---

**🎉 ObjectPoolの導入完了！**  
これで大幅なパフォーマンス向上を実感できるはずです。