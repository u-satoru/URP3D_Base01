# アーキテクチャ準拠性改善TODOリスト

**作成日**: 2025-09-12  
**基準レポート**: Architecture_Compliance_Verification_Report.md  
**現在の準拠率**: 94.7%  
**目標準拠率**: 100%  

## 🔥 高優先度（即時対応推奨）

### 1. namespace migration完了
**推定作業時間**: 1-2時間  
**リスク**: 低（コンパイル時エラーで早期発見可能）  
**影響**: 禁止事項違反の完全解決  

#### 1.1 namespace定義の修正（7ファイル）
- [ ] `Assets/_Project/Tests/Core/Services/GradualUpdatePatternTest.cs`
  - 現在: `namespace _Project.Tests.Core.Services`
  - 修正要: `namespace asterivo.Unity60.Tests.Core.Services`

- [ ] `Assets/_Project/Features/Player/Scripts/PlayerStealthController.cs`
  - 現在: `namespace _Project.Features.Player.Scripts`
  - 修正要: `namespace asterivo.Unity60.Features.Player.Scripts`

- [ ] `Assets/_Project/Tests/Core/Services/StealthAudioServiceTest.cs`
  - 修正要: `namespace asterivo.Unity60.Tests.Core.Services`

- [ ] `Assets/_Project/Tests/Core/Services/StealthAudioCoordinatorServiceLocatorTest.cs`
  - 修正要: `namespace asterivo.Unity60.Tests.Core.Services`

- [ ] `Assets/_Project/Tests/Core/Services/MigrationValidatorTest.cs`
  - 修正要: `namespace asterivo.Unity60.Tests.Core.Services`

- [ ] `Assets/_Project/Core/SystemInitializer.cs`
  - 修正要: `namespace asterivo.Unity60.Core`

- [ ] `Assets/_Project/Core/Lifecycle/IServiceLocatorRegistrable.cs`
  - 修正要: `namespace asterivo.Unity60.Core.Lifecycle`

#### 1.2 using文の修正（10ファイル）
- [ ] `Assets/_Project/Core/Helpers/ServiceHelper.cs`
- [ ] `Assets/_Project/Tests/Runtime/ProductionValidationTests.cs`
- [ ] `Assets/_Project/Features/Player/Scripts/PlayerStealthController.cs`
- [ ] `Assets/_Project/Tests/Performance/ServiceLocatorStressTests.cs`
- [ ] その他6ファイル（一括検索・置換で対応）

#### 実行手順
1. **一括検索・置換実行**
   ```bash
   # namespace定義の置換
   find . -name "*.cs" -exec sed -i 's/namespace _Project\./namespace asterivo.Unity60./g' {} \;
   
   # using文の置換
   find . -name "*.cs" -exec sed -i 's/using _Project\./using asterivo.Unity60./g' {} \;
   ```

2. **コンパイル確認**
   - Unity Editor でコンパイルエラーがないことを確認
   - Test Runner でテストが正常に実行されることを確認

3. **metaファイル整合性確認**
   - 参照が破損していないことを確認
   - アセンブリ定義ファイルとの整合性確認

## 🟡 中優先度（計画的対応推奨）

### 2. GameObject.Find()の最適化
**推定作業時間**: 2-4時間  
**リスク**: 中（実行時パフォーマンスに影響）  
**影響**: パフォーマンス向上、アンチパターン解消  

#### 2.1 直接参照推奨ファイル（UI・固定オブジェクト系）

- [ ] **HUDManager.cs**
  ```csharp
  // ❌ Before: GameObject.Find("Canvas/HealthBar")
  // ✅ After: [SerializeField] private HealthBarUI healthBar;
  ```
  - 理由: UI要素は通常シーン固定配置
  - 手法: Inspector経由での直接参照

- [ ] **NPCVisualSensor.cs**
  ```csharp
  // ❌ Before: GameObject.Find("Player")
  // ✅ After: [SerializeField] private Transform playerTransform;
  ```
  - 理由: Playerは通常1体でシーン固定
  - 手法: Inspector経由での直接参照

#### 2.2 ServiceLocator推奨ファイル（サービス・マネージャー系）

- [ ] **StealthAudioService.cs**
  ```csharp
  // ❌ Before: GameObject.Find("AudioManager")
  // ✅ After: ServiceLocator.GetService<IAudioService>()
  ```
  - 理由: アプリケーション全体サービス
  - 手法: ServiceLocatorパターン

- [ ] **StealthAudioCoordinator.cs**
  ```csharp
  // ❌ Before: 複数音響サービス検索
  // ✅ After: ServiceLocator.GetService<ISpatialAudioService>()
  ```
  - 理由: 複数サービス協調が必要
  - 手法: ServiceLocatorパターン

#### 2.3 その他4ファイル
- [ ] 残り4ファイルの調査・分析
- [ ] 各ファイルに適した最適化手法の決定
- [ ] 実装・テスト実行

#### パフォーマンス測定
- [ ] 最適化前後のパフォーマンステスト実施
- [ ] フレームレート・メモリ使用量の比較
- [ ] 結果の文書化

## 🟢 低優先度（継続監視）

### 3. 品質維持体制の強化

#### 3.1 CI/CD拡張
- [ ] **namespace規約チェック**
  - 新規コミット時の自動検証
  - _Project.* 使用の検出・警告
  - プルリクエスト時のブロック設定

- [ ] **GameObject.Find()検出**
  - 静的解析による使用箇所の検出
  - 新規追加の警告システム
  - 代替手法の推奨メッセージ

#### 3.2 開発者向けドキュメント整備
- [ ] **アーキテクチャガイドライン更新**
  - namespace規約の詳細化
  - GameObject.Find()代替パターン集
  - ServiceLocator使用ガイド

- [ ] **コードレビューチェックリスト**
  - 必須確認項目の明文化
  - 違反パターンの例示
  - 修正方法の参照リンク

#### 3.3 監視・継続改善
- [ ] **定期的なアーキテクチャ準拠性チェック**
  - 月次での準拠性レポート生成
  - 新たな問題パターンの検出
  - 改善提案の継続的な更新

- [ ] **パフォーマンス監視**
  - 最適化効果の継続測定
  - リグレッションの早期検出
  - 新たなボトルネックの特定

## 📊 進捗管理

### 完了判定基準
- [ ] **高優先度完了**: コンパイルエラー0件、namespace違反0件
- [ ] **中優先度完了**: GameObject.Find()使用0件、パフォーマンステスト合格
- [ ] **低優先度完了**: CI/CD拡張完了、ドキュメント更新完了

### 成功指標
- **準拠率**: 94.7% → 100%
- **コンパイル時間**: 現状維持
- **実行時パフォーマンス**: GameObject.Find()最適化による改善
- **開発者体験**: ガイドライン整備による向上

### 想定スケジュール
| 優先度 | 期間 | 担当者 | 備考 |
|--------|------|--------|------|
| 高 | 今週中 | 開発者 | 即座に着手 |
| 中 | 来週中 | 開発者 | 設計検討含む |
| 低 | 継続 | チーム全体 | 体制・プロセス改善 |

## 🔧 実行時の注意点

### リスク軽減策
1. **バックアップ**: 作業前にブランチ作成
2. **段階的実行**: ファイル単位での確認・テスト
3. **テスト実行**: 各段階でのユニットテスト・プレイモードテスト
4. **チームレビュー**: 重要な変更はピアレビュー実施

### ロールバック計画
- 各優先度レベルでのチェックポイント設定
- 問題発生時の即座な復旧手順
- 影響範囲の特定・限定化

---

**最終更新**: このTODOリストに基づき、100%アーキテクチャ準拠達成への明確な道筋が提示されました。優先度に従って実行することで、プロジェクト品質の大幅な向上が期待できます。
