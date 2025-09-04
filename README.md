# URP3D_Base01

これはUnity 6で作成された3Dゲームプロジェクトのベーステンプレートです。Universal Render Pipeline (URP) を使用しています。

## 概要

このプロジェクトは、イベント駆動型アーキテクチャとコマンドパターンを組み合わせた、拡張性の高い3Dゲームの基盤を提供します。Scriptable Objectを活用し、データとロジックの分離を促進することで、効率的な開発とメンテナンスを目的としています。

## 主な機能

- **イベント駆動型アーキテクチャ**: `GameEvent` を介したコンポーネント間の疎結合な連携。
- **コマンドパターン**: ゲーム内のアクション（例：ダメージ、回復）をオブジェクトとしてカプセル化し、再利用と管理を容易にします。
- **Scriptable Objectベースのデータ管理**: キャラクターのステータスやアイテム情報などをアセットとして管理。
- **基本的なプレイヤー機能**: 移動やインタラクションの基盤。
- **エディタ拡張**: コマンドの発行やイベントの流れを視覚化するカスタムウィンドウ。

## 技術仕様

- **Unity Version**: `6000.0.42f1`
- **Render Pipeline**: Universal Render Pipeline (URP)
- **Scripting Backend**: Mono
- **API Compatibility Level**: .NET Standard 2.1

## セットアップ手順

1.  このリポジトリをクローンします。
2.  Unity Editor `6000.0.42f1` 以降でプロジェクトを開きます。
3.  必要なパッケージが自動的にインポートされます。
4.  `Assets/_Project/Scenes/System/` 内のシーンから開発を開始してください。

## ディレクトリ構成

-   `Assets/_Project/Core`: ゲームのコアロジック（イベント、コマンド、データ構造など）。
-   `Assets/_Project/Features`: 各機能（プレイヤー、AI、カメラなど）の実装。
-   `Assets/_Project/Scenes`: ゲームシーン。
-   `Assets/_Project/Docs`: プロジェクト関連のドキュメント。

## 主要なパッケージ

-   `com.unity.render-pipelines.universal`: Universal Render Pipeline
-   `com.unity.inputsystem`: 新しい入力システム
-   `com.unity.cinemachine`: 高度なカメラ制御
-   `com.unity.ai.navigation`: ナビゲーションと経路探索
-   `com.unity.test-framework`: ユニットテストとプレイモードテスト
