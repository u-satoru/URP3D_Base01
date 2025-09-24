# Code Review Checklist

**更新日**: 2025-09-12  
**バージョン**: 1.0  
**対象**: URP3D_Base01 Unity プロジェクト全コードレビュー

## 📋 概要

このチェックリストは、プロジェクトのアーキテクチャ準拠性と品質を維持するため、すべてのコードレビューで使用する必須確認項目を定義します。

## ✅ 必須確認項目

### 🏷️ 1. Namespace規約

- [ ] `asterivo.Unity60.*` 形式を使用している
- [ ] `_Project.*` を新規使用していない  
- [ ] using文が正しいnamespaceを参照している
- [ ] namespace階層が適切（Core/Features/Tests）

**例:**
```csharp
// ✅ 正しい
namespace asterivo.Unity60.Core.Audio;
using asterivo.Unity60.Core.Events;

// ❌ 間違い
namespace _Project.Core.Audio;
using _Project.Core.Events;
```

### 📁 2. ディレクトリ配置

- [ ] Core機能が `Assets/_Project/Core/` に配置
- [ ] 機能実装が `Assets/_Project/Features/` に配置
- [ ] テストコードが `Assets/_Project/Tests/` に配置
- [ ] ドキュメントが `Assets/_Project/Docs/` に配置
- [ ] ファイルが適切なサブディレクトリに配置

### ⚡ 3. パフォーマンス最適化

- [ ] `GameObject.Find()` の新規追加がないか
- [ ] 頻繁な実行処理（Update/FixedUpdate）での最適化
- [ ] 適切な代替パターンの適用
  - UI・固定オブジェクト → SerializeField参照
  - サービス・マネージャー → ServiceLocator
- [ ] オブジェクトプール利用の検討

### 🏗️ 4. アーキテクチャ準拠

- [ ] Core層からFeatures層への参照なし
- [ ] ServiceLocatorの適切な使用
- [ ] イベント駆動型アーキテクチャの活用
- [ ] 疎結合・高凝集の維持
- [ ] 単一責任原則の遵守

### 🧪 5. テスト品質

- [ ] 新機能にユニットテスト追加
- [ ] プレイモードテストの必要性検討
- [ ] テストカバレッジの適切性
- [ ] テストが `Assets/_Project/Tests/` に配置

### 📝 6. コード品質

- [ ] XMLドキュメントコメントの記述
- [ ] 適切な命名規則の使用
- [ ] マジックナンバーの削除
- [ ] エラーハンドリングの実装
- [ ] ログ出力の適切性（EventLogger使用）

### 🔧 7. Unity固有事項

- [ ] MonoBehaviourのライフサイクル適切使用
- [ ] SerializeFieldの適切な使用
- [ ] null参照チェックの実装
- [ ] コルーチンの適切な管理
- [ ] リソースリークの防止

### 📊 8. 設定・データ管理

- [ ] ScriptableObjectの適切な使用
- [ ] Inspector設定の適切性
- [ ] デフォルト値の設定
- [ ] エディタ拡張の必要性検討

## ❌ 主要な違反パターン

### Namespace違反
```csharp
// ❌ 禁止パターン
namespace _Project.Features.Player
{
    using _Project.Core.Services;
}
```

### 不適切なGameObject.Find()
```csharp
// ❌ パフォーマンス問題
void Update()
{
    var player = GameObject.FindWithTag("Player"); // 毎フレーム実行
}
```

### 不適切な層間参照
```csharp
// ❌ Core→Features 参照禁止
namespace asterivo.Unity60.Core.Audio
{
    using asterivo.Unity60.Features.Player; // 禁止
}
```

### ハードコーディング
```csharp
// ❌ マジックナンバー
transform.position += Vector3.up * 5.2f;

// ✅ 設定可能な値
[SerializeField] private float jumpHeight = 5.2f;
transform.position += Vector3.up * jumpHeight;
```

## 🔍 レビュー手順

### 1. 自動チェック実行
Unity Editor で `Tools > Architecture > Compliance Checker` を実行

### 2. 手動レビュー
このチェックリストの各項目を確認

### 3. テスト実行
- ユニットテスト実行
- プレイモードテスト実行  
- コンパイル確認

### 4. 総合評価
- [ ] すべての必須項目をクリア
- [ ] パフォーマンスに問題なし
- [ ] アーキテクチャに違反なし
- [ ] テストが適切に実装

## 📖 参考資料

### 詳細ガイドライン
- [Architecture Guidelines](Architecture_Guidelines.md) - 完全なアーキテクチャガイド
- [Architecture_Compliance_TODO.md](Architecture_Compliance_TODO.md) - 具体的な改善項目

### 修正方法
- **Namespace修正**: [Architecture Guidelines - Namespace規約](Architecture_Guidelines.md#🏷️-namespace規約)
- **GameObject.Find()最適化**: [Architecture Guidelines - GameObject.Find()代替パターン集](Architecture_Guidelines.md#🔍-gameobjectfind-代替パターン集)
- **ServiceLocator使用**: [Architecture Guidelines - ServiceLocator使用ガイド](Architecture_Guidelines.md#📚-servicelocator使用ガイド)

## 🎯 レビュー品質指標

### 合格基準
- **必須項目**: 100%クリア
- **コンパイルエラー**: 0件
- **アーキテクチャ違反**: 0件
- **テストカバレッジ**: 新機能の80%以上

### チーム指標
- **レビュー指摘事項**: 継続的減少
- **再レビュー率**: 20%以下維持
- **リリース後バグ**: アーキテクチャ起因0件

## 📝 レビューテンプレート

コードレビュー時に以下のテンプレートを使用：

```markdown
## Code Review Checklist

### Architecture Compliance
- [ ] Namespace規約遵守
- [ ] Directory配置適切
- [ ] Layer依存関係適切

### Performance
- [ ] GameObject.Find()最適化
- [ ] 適切な代替パターン使用

### Code Quality  
- [ ] テスト実装
- [ ] ドキュメント記述
- [ ] エラーハンドリング

### 総合評価
- [ ] ✅ 承認 / ❌ 要修正

### コメント
[具体的な指摘事項や改善提案]
```

---

**このチェックリストを活用することで、プロジェクト品質の継続的な向上と開発者スキルの標準化を実現します。**
