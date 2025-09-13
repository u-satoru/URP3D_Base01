# TODO - アーキテクチャ準拠性向上 

**更新日**: 2025-09-12 12:45  
**現在の準拠率**: 94.7% / 100%  
**次回目標**: 100% 完全準拠  

## 🎯 今すぐ実行すべき項目

### 🔥 最高優先度 - namespace統一 (今日中)
- [ ] **GradualUpdatePatternTest.cs** namespace修正
  - 現在: `namespace _Project.Tests.Core.Services`
  - 修正: `namespace asterivo.Unity60.Tests.Core.Services`
- [ ] **PlayerStealthController.cs** namespace修正
  - 現在: `namespace _Project.Features.Player.Scripts` 
  - 修正: `namespace asterivo.Unity60.Features.Player.Scripts`
- [ ] **SystemInitializer.cs** namespace修正
- [ ] **IServiceLocatorRegistrable.cs** namespace修正

### 📝 検証必須 (namespace修正後)
- [ ] Unity エディタでコンパイルエラー確認
- [ ] 全テスト実行（EditMode/PlayMode）
- [ ] 動作確認

## 🟡 今週中完了項目

### namespace修正 残り3ファイル
- [ ] `StealthAudioServiceTest.cs`
- [ ] `StealthAudioCoordinatorServiceLocatorTest.cs`  
- [ ] `MigrationValidatorTest.cs`

### using文修正 10ファイル
- [ ] `ServiceHelper.cs` - using _Project.Core削除
- [ ] `ProductionValidationTests.cs`
- [ ] `ServiceLocatorStressTests.cs`
- [ ] その他7ファイル

## 🔧 来週実装項目

### GameObject.Find()最適化 - UI系優先
- [ ] **HUDManager.cs**
  ```csharp
  // 現在: GameObject.Find("Canvas/HealthBar")  
  // 変更: [SerializeField] private HealthBarUI healthBar;
  ```
- [ ] **NPCVisualSensor.cs**  
  ```csharp
  // 現在: GameObject.Find("Player")
  // 変更: [SerializeField] private Transform playerTransform;
  ```

### GameObject.Find()最適化 - サービス系
- [ ] **StealthAudioService.cs**
  ```csharp
  // 現在: GameObject.Find("AudioManager")
  // 変更: ServiceLocator.GetService<IAudioService>()
  ```
- [ ] **StealthAudioCoordinator.cs**

## 📊 定期チェック項目

### 毎日
- [ ] 新規追加コードのnamespace確認
- [ ] コンパイルエラー監視

### 毎週  
- [ ] GameObject.Find()使用状況確認
- [ ] 準拠率測定・レポート更新

## 🚨 ブロッカー・注意事項

### 即座に対処必要
- **なし** (現在クリティカルなブロッカーは存在しない)

### 注意深く実行
- namespace変更時のmetaファイル整合性
- Inspector参照が切れる可能性（GameObject.Find()置き換え時）

## ✅ 完了済み項目

### アーキテクチャ分析・レポート作成
- [x] プロジェクト全体の準拠性検証
- [x] Architecture_Compliance_Verification_Report.md作成  
- [x] GameObject.Find()使い分けガイド追加
- [x] 優先度別修正計画策定

### 作業環境整備
- [x] 作業ログ作成 (20250912_1245)
- [x] TASKS.md作成
- [x] TODO.md作成（本ファイル）

## 🎯 成功の定義

### 短期目標 (今週)
- **namespace準拠率**: 93% → 100%
- **コンパイルエラー**: 0件維持
- **テスト通過率**: 100%維持

### 中期目標 (来週)  
- **GameObject.Find()使用**: 8ファイル → 4ファイル以下
- **パフォーマンス**: 初期化時間10%改善
- **全体準拠率**: 94.7% → 97%以上

### 長期目標 (継続)
- **全体準拠率**: 100%達成・維持
- **品質維持体制**: 運用開始
- **開発効率**: レビュー指摘20%削減

---

💡 **今日のアクション**: まずnamespace修正の4ファイルから開始。1ファイルずつ修正→テスト→次のファイルの安全なアプローチで進める。