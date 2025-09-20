# 3層アーキテクチャ移行 TASKS - 進捗管理シート

## 文書情報
- **作成日時**: 2025年9月20日 15:00
- **種別**: 3層アーキテクチャ移行タスク管理
- **基準文書**: 3-Layer_Architecture_Migration_Plan.md v1.0
- **ステータス凡例**: ⏹️未着手 🚧進行中 ✅完了 ⚠️ブロック中 🔍レビュー中

## 🎯 移行目標
現在の2層構造（`Core` ← `Feature`）から、新たに `Template` 層を追加した3層アーキテクチャ（`Core` ← `Feature` ← `Template`）へ移行し、以下を実現：
- ✨ **再利用性の最大化**
- 🚀 **プロトタイピング速度の向上**
- 👥 **非プログラマーとの連携強化**

## 📊 移行進捗サマリー

| フェーズ | 進捗率 | 状態 | 完了タスク | 残タスク | 期限 |
|---------|--------|------|------------|----------|------|
| **Phase 1: 準備フェーズ** | 0% | ⏹️未着手 | 0/6 | 6 | 2025/09/22 |
| **Phase 2: Feature層整理** | 0% | ⏹️未着手 | 0/2 | 2 | 2025/09/25 |
| **Phase 3: Template層構築** | 0% | ⏹️未着手 | 0/5 | 5 | 2025/09/30 |
| **全体進捗** | **0%** | **⏹️未着手** | **0/13** | **13** | - |

---

## 📋 Phase 1: 準備フェーズ（優先度: Critical）

### タスク1.1: Template層のディレクトリ構造を作成する
- **状態**: ⏹️未着手
- **担当**: -
- **開始日**: -
- **完了予定**: 2025/09/20
- **進捗**: 0%

#### 実行コマンド
```bash
mkdir Assets/_Project/Features/Templates
mkdir Assets/_Project/Features/Templates/Common
mkdir Assets/_Project/Features/Templates/Stealth
mkdir Assets/_Project/Features/Templates/Platformer
mkdir Assets/_Project/Features/Templates/FPS
mkdir Assets/_Project/Features/Templates/TPS
mkdir Assets/_Project/Features/Templates/SurvivalHorror
mkdir Assets/_Project/Features/Templates/Adventure
mkdir Assets/_Project/Features/Templates/ActionRPG
```

#### チェックリスト
- [ ] Templates ディレクトリ作成
- [ ] Common ディレクトリ作成
- [ ] 各ジャンル用ディレクトリ作成（7種類）
- [ ] ディレクトリ構造の確認

---

### タスク1.2: Template層用のAssembly Definitionを作成する
- **状態**: ⏹️未着手
- **担当**: -
- **開始日**: -
- **完了予定**: 2025/09/20
- **進捗**: 0%

#### asterivo.Unity60.Features.Templates.asmdef の内容
```json
{
    "name": "asterivo.Unity60.Features.Templates",
    "rootNamespace": "asterivo.Unity60.Features.Templates",
    "references": [
        "asterivo.Unity60.Core",
        "asterivo.Unity60.Features"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

#### チェックリスト
- [ ] asmdefファイル作成
- [ ] 依存関係の設定確認
- [ ] Template層からFeature層への参照可能確認
- [ ] Feature層からTemplate層への参照不可確認
- [ ] コンパイルエラーがないことを確認

---

### タスク1.3: 名前空間の移行
- **状態**: ⏹️未着手
- **担当**: -
- **開始日**: -
- **完了予定**: 2025/09/21
- **進捗**: 0%

#### 名前空間移行マッピング

| 層 | 旧名前空間 | 新名前空間 |
|----|------------|------------|
| Core層 | `_Project.Core.*` | `asterivo.Unity60.Core.*` |
| Feature層 | `_Project.Features.*` | `asterivo.Unity60.Features.*` |
| Template層 | - | `asterivo.Unity60.Features.Templates.*` |

#### サブタスク
- [ ] **1.3.1**: 全C#スクリプトの名前空間宣言を変更
  - [ ] Core層のスクリプト（約__個）
  - [ ] Feature層のスクリプト（約__個）
- [ ] **1.3.2**: using ディレクティブの更新
  - [ ] 影響を受けるスクリプトのリストアップ
  - [ ] 自動置換スクリプトの作成
  - [ ] 実行と確認
- [ ] **1.3.3**: asmdefファイルのrootNamespace設定
  - [ ] Core層のasmdef更新
  - [ ] Features層のasmdef更新
  - [ ] Templates層のasmdef設定

#### 完了条件
- [ ] `namespace _Project` が一掃されている
- [ ] コンパイルエラーゼロ
- [ ] 全てのテストが通過

---

## 📋 Phase 2: Feature層の整理（優先度: High）

### タスク2.1: ジャンル固有のアセットを特定する
- **状態**: ⏹️未着手
- **担当**: -
- **開始日**: -
- **完了予定**: 2025/09/24
- **進捗**: 0%

#### 調査対象アセット

| アセット種別 | 現在のパス | 移動先 | ジャンル |
|-------------|-----------|---------|----------|
| シーン | `Assets/_Project/Scenes/` | `Templates/[Genre]/Scenes/` | - |
| プレハブ | `Assets/_Project/Features/*/Prefabs/` | `Templates/[Genre]/Prefabs/` | - |
| ScriptableObject | `Assets/_Project/Features/*/Data/` | `Templates/[Genre]/Data/` | - |

#### チェックリスト
- [ ] ステルスゲーム固有アセットのリストアップ
  - [ ] StealthDemo.unity
  - [ ] 巡回NPC関連プレハブ
  - [ ] ステルス用パラメータSO
- [ ] サバイバルホラー固有アセットのリストアップ
- [ ] プラットフォーマー固有アセットのリストアップ
- [ ] FPS/TPS固有アセットのリストアップ
- [ ] 移動対象アセットリストのレビュー

---

## 📋 Phase 3: Template層の構築（優先度: Medium）

### タスク3.1: 最初のテンプレートとして「Stealth」を構築する
- **状態**: ⏹️未着手
- **担当**: -
- **開始日**: -
- **完了予定**: 2025/09/27
- **進捗**: 0%

#### 移動対象アセット

| 移動元 | 移動先 |
|--------|--------|
| `Assets/_Project/Scenes/StealthDemo.unity` | `Assets/_Project/Features/Templates/Stealth/Scenes/StealthDemo.unity` |
| `Assets/_Project/Features/AI/Prefabs/Guard_NPC_Patrol.prefab` | `Assets/_Project/Features/Templates/Stealth/Prefabs/Guard_NPC_Patrol.prefab` |
| `Assets/_Project/Features/Weapons/Data/SilencedPistol.asset` | `Assets/_Project/Features/Templates/Stealth/Data/SilencedPistol.asset` |

#### チェックリスト
- [ ] ディレクトリ構造の作成
  - [ ] `Templates/Stealth/Scenes/`
  - [ ] `Templates/Stealth/Prefabs/`
  - [ ] `Templates/Stealth/Data/`
- [ ] アセットの移動
- [ ] 参照の更新
- [ ] プレハブの参照整合性確認
- [ ] シーンの動作確認

---

### タスク3.2: テンプレートの動作を検証する
- **状態**: ⏹️未着手
- **担当**: -
- **開始日**: -
- **完了予定**: 2025/09/28
- **進捗**: 0%

#### 検証項目

| 検証項目 | 期待値 | 結果 | 備考 |
|----------|--------|------|------|
| シーン起動 | エラーなし | - | - |
| NPC巡回動作 | 正常動作 | - | - |
| プレイヤー検知 | 正常動作 | - | - |
| UI表示 | 正常表示 | - | - |
| パフォーマンス | 60FPS維持 | - | - |

#### チェックリスト
- [ ] StealthDemo.unity の再生
- [ ] 全機能の動作確認
- [ ] プレハブ参照切れの修正
- [ ] パフォーマンステスト
- [ ] 問題点のドキュメント化

---

## 🔄 依存関係マトリクス

| タスク | 依存するタスク | ブロックするタスク |
|--------|---------------|-------------------|
| 1.1 | なし | 1.2, 3.1 |
| 1.2 | 1.1 | 1.3 |
| 1.3 | 1.2 | 2.1, 3.1 |
| 2.1 | 1.3 | 3.1 |
| 3.1 | 1.1, 2.1 | 3.2 |
| 3.2 | 3.1 | なし |

---

## 📝 作業ログ

| 日時 | タスク | 作業内容 | 作業者 | 備考 |
|------|--------|----------|--------|------|
| 2025/09/20 15:00 | - | 3層アーキテクチャ移行TASKS作成 | Claude | 初版作成 |

---

## ⚠️ リスクと課題

| リスク/課題 | 影響度 | 発生確率 | 対策 | 状態 |
|------------|--------|----------|------|------|
| 参照切れによるコンパイルエラー | 高 | 中 | 段階的移行とテスト | ⏹️ |
| 既存機能への影響 | 高 | 低 | バックアップとロールバック計画 | ⏹️ |
| 作業時間の超過 | 中 | 中 | バッファ時間の確保 | ⏹️ |

---

## ✅ 完了条件

### Phase 1 完了条件
- [ ] 全ディレクトリが作成されている
- [ ] Assembly Definitionが正しく設定されている
- [ ] 名前空間移行が完了している
- [ ] コンパイルエラーがゼロ

### Phase 2 完了条件
- [ ] Template層に移動すべきアセットのリストが完成
- [ ] 移動計画がレビュー・承認済み

### Phase 3 完了条件
- [ ] ステルステンプレートが完全動作
- [ ] 他の開発者が利用可能な状態
- [ ] ドキュメントが整備済み

### 移行完了条件
- [ ] 3層アーキテクチャが確立されている
- [ ] 依存関係が正しく制御されている
- [ ] 全テストが通過している
- [ ] パフォーマンスが維持されている
- [ ] ドキュメントが更新済み