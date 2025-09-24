# Performance Monitoring Guidelines

**更新日**: 2025-09-12  
**バージョン**: 1.0  
**対象**: URP3D_Base01 Unity プロジェクト パフォーマンス監視

## 📊 概要

このドキュメントでは、プロジェクトのパフォーマンス監視と最適化に関するガイドラインを定義します。継続的な品質向上とリグレッション防止のための包括的なアプローチを提供します。

## 🎯 監視対象指標

### 1. 実行時パフォーマンス

#### フレームレート
- **ターゲット**: 60 FPS (PC), 30 FPS (Mobile)
- **測定方法**: Unity Profiler, Frame Debugger
- **閾値**:
  - ✅ Good: Target FPS の 95% 以上
  - ⚠️ Warning: Target FPS の 80-94%
  - ❌ Critical: Target FPS の 80% 未満

#### メモリ使用量
- **ターゲット**: 
  - PC: <2GB RAM
  - Mobile: <1GB RAM
- **測定項目**:
  - Heap Memory
  - Graphics Memory  
  - Audio Memory
  - Managed Memory

#### CPU使用率
- **Main Thread**: <16.67ms (60 FPS)
- **Render Thread**: <16.67ms (60 FPS)
- **Workers**: 効率的な分散処理

### 2. アーキテクチャパフォーマンス

#### GameObject.Find() 使用状況
- **ターゲット**: 主要ランタイムファイルで0件
- **監視項目**:
  - Update/FixedUpdate内での使用
  - 頻繁に呼ばれるメソッド内での使用
  - UI・サービス系での使用

#### ServiceLocator効率性
- **測定項目**:
  - GetService()呼び出し回数
  - サービス登録/解除時間
  - メモリフットプリント

#### イベントシステム性能
- **測定項目**:
  - Event発行/購読レイテンシ
  - 同時Event処理数
  - メモリリーク検出

## 🔧 監視ツールとセットアップ

### Unity内蔵ツール

#### 1. Unity Profiler
```csharp
// プロファイラーマーカーの設定例
using Unity.Profiling;

public class AudioService : MonoBehaviour
{
    private static readonly ProfilerMarker audioUpdateMarker = new ProfilerMarker("AudioService.Update");
    
    void Update()
    {
        using (audioUpdateMarker.Auto())
        {
            // Audio処理
        }
    }
}
```

#### 2. Frame Debugger
- **用途**: レンダリングパフォーマンスの詳細分析
- **監視点**: Draw Call数、Batch処理効率

#### 3. Memory Profiler Package
- **設定**: Window > Analysis > Memory Profiler
- **監視**: メモリリーク、fragmentation

### カスタム監視システム

#### 1. パフォーマンスメトリクス収集
```csharp
using UnityEngine;
using System.Collections.Generic;

namespace asterivo.Unity60.Core.Performance
{
    public class PerformanceMonitor : MonoBehaviour
    {
        [Header("Monitoring Settings")]
        [SerializeField] private bool enableMonitoring = true;
        [SerializeField] private float samplingInterval = 1f;
        
        private PerformanceData currentData;
        private List<PerformanceData> history = new List<PerformanceData>();
        
        void Start()
        {
            if (enableMonitoring)
            {
                InvokeRepeating(nameof(CollectMetrics), 0f, samplingInterval);
            }
        }
        
        void CollectMetrics()
        {
            currentData = new PerformanceData
            {
                Timestamp = System.DateTime.Now,
                FrameRate = 1f / Time.deltaTime,
                HeapMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(false),
                ReservedMemory = UnityEngine.Profiling.Profiler.GetTotalReservedMemory(false),
                GameObjectCount = FindObjectsOfType<GameObject>().Length
            };
            
            history.Add(currentData);
            
            // 履歴管理（直近100サンプルを保持）
            if (history.Count > 100)
            {
                history.RemoveAt(0);
            }
            
            CheckPerformanceThresholds();
        }
        
        void CheckPerformanceThresholds()
        {
            if (currentData.FrameRate < 50f)
            {
                Debug.LogWarning($"[PerformanceMonitor] Low FPS detected: {currentData.FrameRate:F1}");
            }
            
            if (currentData.HeapMemory > 500 * 1024 * 1024) // 500MB
            {
                Debug.LogWarning($"[PerformanceMonitor] High memory usage: {currentData.HeapMemory / (1024*1024):F1}MB");
            }
        }
    }
    
    [System.Serializable]
    public struct PerformanceData
    {
        public System.DateTime Timestamp;
        public float FrameRate;
        public long HeapMemory;
        public long ReservedMemory;
        public int GameObjectCount;
    }
}
```

#### 2. GameObject.Find() 検出システム
```csharp
#if UNITY_EDITOR && PERFORMANCE_MONITORING
using UnityEngine;
using System.Reflection;

namespace asterivo.Unity60.Core.Performance
{
    public static class GameObjectFindDetector
    {
        [RuntimeInitializeOnLoadMethod]
        static void Initialize()
        {
            // GameObject.Find系メソッドの呼び出しを監視
            var gameObjectType = typeof(GameObject);
            var findMethod = gameObjectType.GetMethod("Find", BindingFlags.Public | BindingFlags.Static);
            
            // Method hooking implementation (実装は複雑なため概念的な例)
            Debug.LogWarning("[PerformanceMonitor] GameObject.Find() detector initialized");
        }
    }
}
#endif
```

## 📈 パフォーマンステスト実装

### 1. 自動化テストスイート

#### フレームレートテスト
```csharp
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace asterivo.Unity60.Tests.Performance
{
    public class FrameRateTests
    {
        [UnityTest]
        public IEnumerator GameplayScene_ShouldMaintain60FPS()
        {
            // テストシーン読み込み
            yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("GameplayTest");
            
            yield return new WaitForSeconds(2f); // ウォームアップ
            
            float totalFrameTime = 0f;
            int frameCount = 0;
            float testDuration = 10f;
            float startTime = Time.time;
            
            while (Time.time - startTime < testDuration)
            {
                totalFrameTime += Time.deltaTime;
                frameCount++;
                yield return null;
            }
            
            float averageFPS = frameCount / totalFrameTime;
            
            Assert.GreaterOrEqual(averageFPS, 57f, "Average FPS should be at least 57 (95% of 60 FPS target)");
        }
    }
}
```

#### メモリリークテスト
```csharp
[UnityTest]
public IEnumerator MemoryUsage_ShouldNotIncrease_DuringGameplayLoop()
{
    long initialMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(false);
    
    // ゲームプレイループを模擬実行
    for (int i = 0; i < 100; i++)
    {
        // プレイヤーアクション実行
        // エネミー生成・削除
        // UI更新
        yield return new WaitForSeconds(0.1f);
    }
    
    // ガベージコレクション強制実行
    System.GC.Collect();
    yield return new WaitForSeconds(1f);
    
    long finalMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(false);
    long memoryIncrease = finalMemory - initialMemory;
    
    Assert.Less(memoryIncrease, 50 * 1024 * 1024, "Memory increase should be less than 50MB");
}
```

### 2. ベンチマークシステム

#### パフォーマンスベースライン設定
```csharp
[System.Serializable]
public class PerformanceBaseline
{
    public string SceneName;
    public float TargetFPS;
    public long MaxMemoryMB;
    public float MaxLoadTime;
    public int MaxDrawCalls;
    public int MaxGameObjects;
}

// ベースライン設定例
public static readonly PerformanceBaseline[] Baselines = {
    new PerformanceBaseline {
        SceneName = "GameplayMain",
        TargetFPS = 60f,
        MaxMemoryMB = 500,
        MaxLoadTime = 3f,
        MaxDrawCalls = 200,
        MaxGameObjects = 1000
    }
};
```

## 📋 継続的監視プロセス

### 1. 日次チェック

#### 自動化スクリプト
```bash
# パフォーマンステスト実行
Unity.exe -projectPath . -batchmode -runTests \
-testResults "PerformanceTestResults.xml" \
-testCategory "Performance" -quit

# 結果分析とレポート生成  
python analyze_performance_results.py
```

#### チェック項目
- [ ] 全パフォーマンステストが合格
- [ ] メモリ使用量が閾値以内
- [ ] 新しいGameObject.Find()使用がない
- [ ] フレームレートが目標値を維持

### 2. 週次レビュー

#### パフォーマンストレンド分析
- フレームレート変化の分析
- メモリ使用量推移の確認
- ボトルネック箇所の特定
- 最適化効果の測定

#### レビュー会議議題
1. パフォーマンス指標の週次レポート
2. 新たなボトルネックの特定
3. 最適化優先度の決定
4. 次週の改善計画策定

### 3. 月次総合評価

#### Monthly Performance Report
- 全体パフォーマンストレンドの分析
- 最適化効果の定量評価
- 新たなパフォーマンス目標の設定
- チーム全体でのベストプラクティス共有

## ⚠️ パフォーマンス警告システム

### 1. リアルタイム警告

#### 閾値設定
```csharp
public static class PerformanceAlerts
{
    public const float CRITICAL_FPS_THRESHOLD = 45f;
    public const long CRITICAL_MEMORY_THRESHOLD = 800 * 1024 * 1024; // 800MB
    public const float WARNING_FRAME_TIME = 20f; // ms
    
    public static void CheckAndAlert()
    {
        if (1f / Time.deltaTime < CRITICAL_FPS_THRESHOLD)
        {
            Debug.LogError("[PERFORMANCE] Critical FPS drop detected!");
        }
        
        if (UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(false) > CRITICAL_MEMORY_THRESHOLD)
        {
            Debug.LogError("[PERFORMANCE] Critical memory usage detected!");
        }
    }
}
```

### 2. CI/CD統合

#### パフォーマンスゲート
```yaml
# GitHub Actions example
- name: Performance Test Gate
  run: |
    if [ $PERFORMANCE_TEST_RESULT != "PASS" ]; then
      echo "Performance tests failed. Blocking deployment."
      exit 1
    fi
```

## 🎯 最適化優先度マトリクス

| 影響度 | 実装難易度 | 優先度 | 例 |
|--------|-----------|--------|-----|
| 高 | 低 | 🔥 最高 | GameObject.Find()削除 |
| 高 | 高 | ⚡ 高 | レンダリング最適化 |
| 中 | 低 | ✅ 中 | コルーチン最適化 |
| 低 | 高 | 🔽 低 | 高度なメモリ最適化 |

## 📚 パフォーマンス改善テクニック

### 1. GameObject.Find() 最適化
- SerializeField による直接参照
- ServiceLocator パターン使用
- オブジェクトプール活用

### 2. メモリ最適化
- オブジェクトプールによるGC負荷削減
- 文字列連結の最適化
- テクスチャ圧縮設定

### 3. レンダリング最適化
- バッチング効率化
- LODシステム導入
- カリング設定最適化

## 🔧 Tools & Resources

### 監視ツール一覧
- **Unity Profiler**: CPU/GPU/Memory分析
- **Memory Profiler**: メモリリーク検出
- **Frame Debugger**: レンダリング分析
- **Custom PerformanceMonitor**: 継続監視

### 外部ツール
- **Intel VTune**: 詳細CPU分析
- **RenderDoc**: グラフィックス詳細分析
- **Unity Cloud Build**: 継続的パフォーマンステスト

---

**このガイドラインに従うことで、プロジェクトのパフォーマンス品質を継続的に監視・改善し、ユーザー体験の向上を実現できます。**