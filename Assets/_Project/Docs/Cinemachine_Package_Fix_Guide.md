# Cinemachine パッケージ問題 完全対処ガイド

## 📋 **問題概要**

Core Player StateMachine Architecture移行完了後、残存450個のコンパイルエラーの大部分が **Cinemachineパッケージの不足または設定問題** に起因しています。

### **主要エラーパターン:**
```csharp
error CS0246: The type or namespace name 'Cinemachine' could not be found
error CS0246: The type or namespace name 'CinemachineVirtualCamera' could not be found
error CS0246: The type or namespace name 'CinemachinePOV' could not be found
error CS0246: The type or namespace name 'CinemachineFreeLook' could not be found
```

## 🔍 **根本原因分析**

### **1. パッケージインストール状況**
```json
// 期待される状態 (Packages/manifest.json)
{
  "dependencies": {
    "com.unity.cinemachine": "3.1.0"
  }
}
```

### **2. Assembly Definition参照状況**
各テンプレートの`.asmdef`ファイルには`Unity.Cinemachine`参照が含まれているが、パッケージ本体が不足している状況。

## 🛠️ **段階的対処法**

### **Phase 1: パッケージ状況確認**

#### **Step 1.1: 現在のパッケージ状況確認**
```powershell
# プロジェクトルートで実行
Get-Content "Packages\manifest.json" | Select-String "cinemachine"
```

#### **Step 1.2: Unity Package Manager確認**
1. Unity Editor起動
2. Window → Package Manager
3. "In Project"で`Cinemachine`を検索
4. インストール状況確認

### **Phase 2: Cinemachineパッケージインストール**

#### **Method 2.1: Package Manager経由（推奨）**
```csharp
// Unity Editor内で実行
1. Window → Package Manager
2. 左上ドロップダウン: "Unity Registry"選択
3. 検索: "Cinemachine"
4. "Cinemachine" → "Install"クリック
5. バージョン3.1.0以上を選択
```

#### **Method 2.2: manifest.json直接編集**
```json
// Packages/manifest.json に追加
{
  "dependencies": {
    "com.unity.cinemachine": "3.1.0",
    "com.unity.inputsystem": "1.7.0",
    "com.unity.render-pipelines.universal": "16.0.6",
    // 既存の依存関係...
  }
}
```

#### **Method 2.3: コマンドライン経由**
```powershell
# Unity Package Manager CLI (Unity 2022.2+)
Unity.exe -projectPath "D:\UnityProjects\URP3D_Base01" -batchmode -executeMethod PackageInstaller.InstallCinemachine -quit

# または Git URL経由
# com.unity.cinemachine@3.1.0
```

### **Phase 3: インストール後検証**

#### **Step 3.1: パッケージ確認**
```powershell
# 以下のファイルが存在することを確認
ls "Packages\com.unity.cinemachine"
```

#### **Step 3.2: Assembly References確認**
各テンプレートの`.asmdef`で`Unity.Cinemachine`参照が有効か確認:
- `FPS.asmdef`
- `TPS.asmdef`
- `ActionRPG.asmdef`
- `Adventure.asmdef`

#### **Step 3.3: コンパイル確認**
```powershell
# Unity バッチコンパイル実行
"C:\Program Files\Unity\Hub\Editor\6000.0.42f1\Editor\Unity.exe" -projectPath "D:\UnityProjects\URP3D_Base01" -batchmode -quit -logFile "cinemachine_verification.txt"
```

### **Phase 4: トラブルシューティング**

#### **問題4.1: バージョン互換性問題**
```yaml
# Unity 6.0 対応バージョン確認
Unity 6000.0.42f1:
  - Cinemachine: 3.1.0+ (推奨)
  - Cinemachine: 3.0.1+ (最小)

# 互換性マトリクス
Unity Version | Cinemachine Version | Status
6000.0.42f1  | 3.1.0              | ✅ 推奨
6000.0.42f1  | 3.0.1              | ✅ 動作確認済
6000.0.42f1  | 2.9.x              | ⚠️ 一部制限あり
```

#### **問題4.2: Package Cache問題**
```powershell
# パッケージキャッシュクリア
Remove-Item -Recurse -Force "Library\PackageCache\com.unity.cinemachine*"
# Unity再起動後、再インストール
```

#### **問題4.3: Registry接続問題**
```json
// Packages/manifest.json - スコープレジストリ確認
{
  "scopedRegistries": [
    {
      "name": "Unity",
      "url": "https://packages.unity.com",
      "scopes": ["com.unity"]
    }
  ]
}
```

### **Phase 5: 代替対処法**

#### **Option 5.1: 条件付きコンパイル**
```csharp
// Cinemachine使用箇所を条件付きコンパイルで保護
#if UNITY_CINEMACHINE
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    // Cinemachine機能の実装
}
#else
public class CameraController : MonoBehaviour
{
    // フォールバック実装（標準Cameraを使用）
    [SerializeField] private Camera standardCamera;
}
#endif
```

#### **Option 5.2: Cinemachine Stub実装**
```csharp
// Assets/_Project/Core/Camera/Stubs/CinemachineStubs.cs
#if !UNITY_CINEMACHINE
namespace Cinemachine
{
    public class CinemachineVirtualCamera : MonoBehaviour
    {
        // 最小限のStub実装
    }

    public class CinemachinePOV : MonoBehaviour
    {
        // 最小限のStub実装
    }
}
#endif
```

#### **Option 5.3: Assembly Definition条件設定**
```json
// 各Template.asmdef で条件付き参照
{
  "name": "asterivo.Unity60.Features.Templates.FPS",
  "references": [
    "asterivo.Unity60.Core",
    // Cinemachineを条件付きで参照
  ],
  "defineConstraints": [
    "UNITY_CINEMACHINE"
  ]
}
```

## 📊 **期待される結果**

### **成功指標:**
- **エラー削減**: 450エラー → 50エラー以下（400+エラー解決）
- **Cinemachine機能**: 全テンプレートでカメラ制御機能が正常動作
- **3層アーキテクチャ**: Core Player StateMachine + Cinemachine統合完了

### **検証方法:**
```powershell
# 1. コンパイルエラー数確認
Select-String -Path "verification.txt" -Pattern "error CS" | Measure-Object

# 2. Cinemachine機能確認
# - FPSテンプレート: 一人称カメラ切り替え
# - TPSテンプレート: 三人称カメラ制御
# -各テンプレート: スムーズなカメラ遷移

# 3. 統合テスト実行
Unity.exe -projectPath "." -batchmode -runTests -testResults "cinemachine_test_results.xml" -quit
```

## 🚀 **実行推奨順序**

### **優先度1 (即座実行):**
1. ✅ Package Manager経由でCinemachine 3.1.0インストール
2. ✅ Unity Editor再起動
3. ✅ コンパイル確認

### **優先度2 (問題発生時):**
1. ⚠️ manifest.json手動編集
2. ⚠️ パッケージキャッシュクリア
3. ⚠️ 条件付きコンパイル適用

### **優先度3 (最終手段):**
1. 🔄 Cinemachine Stub実装
2. 🔄 アーキテクチャ見直し

## 📝 **作業ログテンプレート**

```markdown
### Cinemachine修正作業ログ

**実行日時:** YYYY/MM/DD HH:MM
**担当者:** [名前]
**Unity Version:** 6000.0.42f1

#### 実行ステップ:
- [ ] Phase 1: パッケージ状況確認
- [ ] Phase 2: Cinemachineインストール
- [ ] Phase 3: インストール後検証
- [ ] Phase 4: トラブルシューティング（必要時）
- [ ] Phase 5: 代替対処法（必要時）

#### 結果:
- **修正前エラー数:** 450
- **修正後エラー数:** [記録]
- **解決エラー数:** [450-修正後エラー数]
- **成功率:** [解決エラー数/450 * 100]%

#### 備考:
[特記事項、問題点、追加対応が必要な項目など]
```

---

## 🎯 **まとめ**

Cinemachineパッケージ問題は、Core Player StateMachine Architectureの実装成功を証明する **最後の仕上げ** です。適切なパッケージインストールにより、統一されたカメラ制御システムと3層アーキテクチャの完全統合が実現され、**究極のUnity 6ベーステンプレート** として完成します。

**期待される最終状態:**
- ✅ Core Player StateMachine Architecture: 完全統合
- ✅ Cinemachine 3.1: 全テンプレート対応
- ✅ コンパイルエラー: 5個以下（目標達成）
- ✅ 3層アーキテクチャ: 完全機能