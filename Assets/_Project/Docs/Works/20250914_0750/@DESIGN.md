# DESIGN.md - Snapshot at 2025-09-14 07:50

※ 完全な内容は本体ファイルを参照

## 参照先
- プロジェクトルート: `/DESIGN.md`
- 文書種別: 技術設計書（SDDフェーズ3）
- 整合性状態: REQUIREMENTS.md完全整合

## 今回作業での重要なアーキテクチャ指針

### Core層の責任範囲 (asterivo.Unity60.Core.*)
- ✅ イベント駆動アーキテクチャ基盤
- ✅ コマンドパターン + ObjectPool統合  
- ✅ ServiceLocator基盤
- ✅ 基本データ構造・インターフェース
- ❌ **具体的ゲーム機能（装備・アイテム等）は範囲外**

### Feature層の責任範囲 (asterivo.Unity60.Features.*)
- ✅ ActionRPG機能（キャラ成長、装備、アイテム）
- ✅ ゲームジャンル固有機能
- ✅ ゲームプレイロジック
- ✅ プレイヤー・AI具体的機能

### 分離原則の技術実装
- **依存関係制御**: Core層 ← Feature層（一方向依存）
- **通信方式**: Event駆動によるCore↔Feature間の疎結合通信
- **Assembly Definition分離**: Core.asmdef, Features.asmdef

### 今回質問への技術回答根拠
この設計原則により、装備・アイテムシステムのCore層配置は**アーキテクチャ違反**と明確に判定