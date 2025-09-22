# 設計提案書：群衆AI（Swarm AI）のアーキテクチャ統合方針

- **日付**: 2025年9月22日
- **作成者**: Gemini (AI Assistant)
- **ステータス**: 提案

## 1. 議題

Unity 6の強力な機能（DOTS, NavMesh Crowd Simulationなど）を、本プロジェクトの3層アーキテクチャにどのように統合し、群衆AI（Swarm AI）を実装すべきか。特に、これらの機能を`Core`層に直接組み込むことの是非を問う。

## 2. 結論

**Unity 6の具体的な機能（DOTS, NavMeshなど）を、直接`Core`層に組み込むべきではない。**

代わりに、**「`Core`層で抽象的なインターフェースを定義し、具体的な実装は`Feature`層で行う」**というアプローチを、本プロジェクトにおける公式なベストプラクティスとして採用する。

## 3. 設計思想と根拠

この結論は、本プロジェクトの根幹をなす以下のアーキテクチャ原則に基づいている。

-   **責務の分離**: `Core`層は普遍的な「概念」を、`Feature`層は具体的な「実装」を担当する。
-   **依存関係の一方向性**: `Template` → `Feature` → `Core` の依存関係を厳守する。
-   **再利用性と拡張性**: 特定の技術や実装に依存しない、移植性の高い`Core`層を維持する。

### 3.1. `Core`層に直接実装した場合の問題点

1.  **責務違反**: `Core`層が「NavMeshでの移動」といった具体的な実装方法を知ってしまい、普遍性を失う。
2.  **依存関係の汚染**: `Core`層がUnityの重量級パッケージ（例: `com.unity.ai.navigation`）に依存すると、`Core`層を利用する全ての`Feature`が不必要にその影響を受ける。
3.  **拡張性の喪失**: 将来、より優れた技術（例: DOTSベースの新しいAIシステム）が登場した際に、`Core`層からの改修が必要となり、移行コストが著しく増大する。

## 4. 推奨されるアーキテクチャと実装フロー

### Step 1: `Core`層で「契約」としてのインターフェースを定義する

`Core`層は、群衆AIが**「何をするものか(What)」**だけを、特定の技術用語を使わずに定義する。

**ファイル:** `Assets/_Project/Core/AI/Swarm/ISwarmManager.cs` (新規作成)
```csharp
using UnityEngine;

namespace asterivo.Unity60.Core.AI
{
    /// <summary>
    /// 群れを構成する個々のエージェントが持つべき、最低限の移動能力を定義するインターフェース。
    /// </summary>
    public interface ISwarmAgent
    {
        void SetDestination(Vector3 destination);
        void Stop();
        GameObject GetGameObject(); // 他のシステムがGameObjectとして参照する必要がある場合
    }

    /// <summary>
    /// 群れ全体を管理するマネージャーの能力を定義するインターフェース。
    /// </summary>
    public interface ISwarmManager
    {
        void AddAgent(ISwarmAgent agent);
        void RemoveAgent(ISwarmAgent agent);
        void SetGroupDestination(Vector3 destination, float formationRadius);
        void ClearAgents();
    }
}
```
**ポイント**:
- このインターフェースは、`NavMesh`も`DOTS`も一切知らない。
- 「群れにはメンバーがいて、目的地を設定できる」という純粋な**契約**のみを定義している。

### Step 2: `Feature`層で、Unityの具体的機能を使って「実装」する

`Feature`層は、`Core`層の「契約」を、Unityの具体的な「道具」（`NavMesh`, `DOTS`など）を使って実現する。

#### 実装例A: NavMeshベースの実装

**ファイル:** `Assets/_Project/Features/SwarmAI/Scripts/NavMeshSwarmManager.cs` (新規作成)
```csharp
using UnityEngine;
using UnityEngine.AI; // NavMeshへの具体的な依存はここで行う
using asterivo.Unity60.Core.AI; // Core層のインターフェースを利用

namespace asterivo.Unity60.Features.SwarmAI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshSwarmAgent : MonoBehaviour, ISwarmAgent
    {
        private NavMeshAgent _agent;
        
        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        public void SetDestination(Vector3 destination)
        {
            if (_agent.isOnNavMesh)
            {
                _agent.SetDestination(destination);
            }
        }
        // ... ISwarmAgentの他のメソッドを実装 ...
    }

    public class NavMeshSwarmManager : MonoBehaviour, ISwarmManager
    {
        private List<ISwarmAgent> _agents = new List<ISwarmAgent>();
        
        public void SetGroupDestination(Vector3 destination, float formationRadius)
        {
            foreach (var agent in _agents)
            {
                // NavMeshとフォーメーション計算を組み合わせた具体的なロジックをここに実装
                Vector3 pointInFormation = destination + Random.insideUnitSphere * formationRadius;
                NavMesh.SamplePosition(pointInFormation, out NavMeshHit hit, formationRadius, 1);
                agent.SetDestination(hit.position);
            }
        }
        // ... ISwarmManagerの他のメソッドを実装 ...
    }
}
```

#### 実装例B: DOTSベースの実装（将来の選択肢）

同様に、DOTSのECSやJob Systemを使って`ISwarmManager`と`ISwarmAgent`を実装した`DOTSSwarmManager`を`Feature`層に作成することも可能。

### Step 3: `Template`層で「設定」して利用する

-   `NavMeshSwarmManager`コンポーネントをシーン内のGameObjectにアタッチする。
-   `NavMeshSwarmAgent`コンポーネントを持つピクミンのプレハブを作成する。
-   移動速度やフォーメーションの半径といったパラメータをインスペクタ上で調整する。

## 5. 期待される効果

-   **`Core`層の純粋性の維持**: `Core`層は特定の技術から隔離され、プロジェクトの普遍的な基盤としての役割を維持する。
-   **技術選択の柔軟性**: `NavMesh`ベースの実装から`DOTS`ベースの実装への移行が、`Core`層や他の`Feature`に影響を与えることなく、`SwarmAI` Feature内の改修だけで可能になる。
-   **テスト容易性**: `Core`層のインターフェースをモックすることで、`Feature`層のロジックを独立してテストできる。

この方針に従うことで、プロジェクトは特定のUnity機能に縛られることなく、将来の技術進化にも柔軟に対応できる、持続可能で堅牢なアーキテクチャを維持できます。
