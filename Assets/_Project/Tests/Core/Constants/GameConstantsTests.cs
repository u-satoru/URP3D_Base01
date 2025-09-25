using NUnit.Framework;
using UnityEngine;
using asterivo.Unity60.Core.Constants;

namespace asterivo.Unity60.Tests.Core.Constants
{
    /// <summary>
    /// GameConstants の包括的なテストクラス
    /// ゲーム定数の妥当性、一貫性、範囲制限を検証
    /// マジックナンバー排除によるコード品質向上を確認
    /// </summary>
    [TestFixture]
    public class GameConstantsTests
    {
        #region Health and Damage Constants Tests

        /// <summary>
        /// ヘルス・ダメージ関連定数の存在と妥当性テスト
        /// </summary>
        [Test]
        public void GameConstants_HealthDamageConstants_HaveValidValues()
        {
            // Act & Assert - テスト用回復量定数
            Assert.AreEqual(10, GameConstants.TEST_HEAL_SMALL, "TEST_HEAL_SMALL の値が期待値と異なる");
            Assert.AreEqual(25, GameConstants.TEST_HEAL_LARGE, "TEST_HEAL_LARGE の値が期待値と異なる");
            
            // Act & Assert - テスト用ダメージ量定数
            Assert.AreEqual(10, GameConstants.TEST_DAMAGE_SMALL, "TEST_DAMAGE_SMALL の値が期待値と異なる");
            Assert.AreEqual(25, GameConstants.TEST_DAMAGE_LARGE, "TEST_DAMAGE_LARGE の値が期待値と異なる");
        }

        /// <summary>
        /// ヘルス・ダメージ定数の論理的整合性テスト
        /// </summary>
        [Test]
        public void GameConstants_HealthDamageConstants_HaveLogicalConsistency()
        {
            // Assert - 小さい値が大きい値より小さいことを確認
            Assert.Less(GameConstants.TEST_HEAL_SMALL, GameConstants.TEST_HEAL_LARGE, 
                "TEST_HEAL_SMALL は TEST_HEAL_LARGE より小さくなければならない");
            Assert.Less(GameConstants.TEST_DAMAGE_SMALL, GameConstants.TEST_DAMAGE_LARGE, 
                "TEST_DAMAGE_SMALL は TEST_DAMAGE_LARGE より小さくなければならない");
            
            // Assert - 回復とダメージの対応値が同じであることを確認（バランス設計）
            Assert.AreEqual(GameConstants.TEST_HEAL_SMALL, GameConstants.TEST_DAMAGE_SMALL, 
                "小さい回復量とダメージ量は同じ値でなければならない");
            Assert.AreEqual(GameConstants.TEST_HEAL_LARGE, GameConstants.TEST_DAMAGE_LARGE, 
                "大きい回復量とダメージ量は同じ値でなければならない");
        }

        /// <summary>
        /// ヘルス・ダメージ定数の正の値確認テスト
        /// </summary>
        [Test]
        public void GameConstants_HealthDamageConstants_ArePositiveValues()
        {
            // Assert - 全ての値が正の数であることを確認
            Assert.Greater(GameConstants.TEST_HEAL_SMALL, 0, "TEST_HEAL_SMALL は正の値でなければならない");
            Assert.Greater(GameConstants.TEST_HEAL_LARGE, 0, "TEST_HEAL_LARGE は正の値でなければならない");
            Assert.Greater(GameConstants.TEST_DAMAGE_SMALL, 0, "TEST_DAMAGE_SMALL は正の値でなければならない");
            Assert.Greater(GameConstants.TEST_DAMAGE_LARGE, 0, "TEST_DAMAGE_LARGE は正の値でなければならない");
        }

        /// <summary>
        /// テスト用定数の実用的範囲テスト
        /// </summary>
        [Test]
        public void GameConstants_HealthDamageConstants_AreInPracticalRange()
        {
            // Assert - テスト用定数が実用的範囲内（1-1000）であることを確認
            Assert.That(GameConstants.TEST_HEAL_SMALL, Is.InRange(1, 1000), 
                "TEST_HEAL_SMALL は実用的な範囲（1-1000）内でなければならない");
            Assert.That(GameConstants.TEST_HEAL_LARGE, Is.InRange(1, 1000), 
                "TEST_HEAL_LARGE は実用的な範囲（1-1000）内でなければならない");
            Assert.That(GameConstants.TEST_DAMAGE_SMALL, Is.InRange(1, 1000), 
                "TEST_DAMAGE_SMALL は実用的な範囲（1-1000）内でなければならない");
            Assert.That(GameConstants.TEST_DAMAGE_LARGE, Is.InRange(1, 1000), 
                "TEST_DAMAGE_LARGE は実用的な範囲（1-1000）内でなければならない");
        }

        #endregion

        #region Time Constants Tests

        /// <summary>
        /// 時間関連定数の存在と妥当性テスト
        /// </summary>
        [Test]
        public void GameConstants_TimeConstants_HaveValidValues()
        {
            // Act & Assert - 時間関連定数
            Assert.AreEqual(1.0f, GameConstants.MIN_LOADING_TIME, 0.001f, "MIN_LOADING_TIME の値が期待値と異なる");
            Assert.AreEqual(0.3f, GameConstants.UI_FADE_DURATION, 0.001f, "UI_FADE_DURATION の値が期待値と異なる");
            Assert.AreEqual(0.5f, GameConstants.AUDIO_FADE_DURATION, 0.001f, "AUDIO_FADE_DURATION の値が期待値と異なる");
        }

        /// <summary>
        /// 時間定数の論理的整合性テスト
        /// </summary>
        [Test]
        public void GameConstants_TimeConstants_HaveLogicalConsistency()
        {
            // Assert - UIフェードがオーディオフェードより速いことを確認（UX設計）
            Assert.Less(GameConstants.UI_FADE_DURATION, GameConstants.AUDIO_FADE_DURATION, 
                "UI フェードはオーディオフェードより短時間でなければならない");
            
            // Assert - 最小ローディング時間が他のフェード時間より長いことを確認
            Assert.Greater(GameConstants.MIN_LOADING_TIME, GameConstants.UI_FADE_DURATION, 
                "最小ローディング時間は UI フェード時間より長くなければならない");
            Assert.Greater(GameConstants.MIN_LOADING_TIME, GameConstants.AUDIO_FADE_DURATION, 
                "最小ローディング時間はオーディオフェード時間より長くなければならない");
        }

        /// <summary>
        /// 時間定数の正の値確認テスト
        /// </summary>
        [Test]
        public void GameConstants_TimeConstants_ArePositiveValues()
        {
            // Assert - 全ての時間定数が正の値であることを確認
            Assert.Greater(GameConstants.MIN_LOADING_TIME, 0f, "MIN_LOADING_TIME は正の値でなければならない");
            Assert.Greater(GameConstants.UI_FADE_DURATION, 0f, "UI_FADE_DURATION は正の値でなければならない");
            Assert.Greater(GameConstants.AUDIO_FADE_DURATION, 0f, "AUDIO_FADE_DURATION は正の値でなければならない");
        }

        /// <summary>
        /// 時間定数の実用的範囲テスト
        /// </summary>
        [Test]
        public void GameConstants_TimeConstants_AreInReasonableRange()
        {
            // Assert - 時間定数が実用的範囲内（0.1秒-10秒）であることを確認
            Assert.That(GameConstants.MIN_LOADING_TIME, Is.InRange(0.1f, 10f), 
                "MIN_LOADING_TIME は実用的な範囲（0.1-10秒）内でなければならない");
            Assert.That(GameConstants.UI_FADE_DURATION, Is.InRange(0.1f, 2f), 
                "UI_FADE_DURATION は実用的な範囲（0.1-2秒）内でなければならない");
            Assert.That(GameConstants.AUDIO_FADE_DURATION, Is.InRange(0.1f, 2f), 
                "AUDIO_FADE_DURATION は実用的な範囲（0.1-2秒）内でなければならない");
        }

        #endregion

        #region UI Constants Tests

        /// <summary>
        /// UI関連定数の存在と妥当性テスト
        /// </summary>
        [Test]
        public void GameConstants_UIConstants_HaveValidValues()
        {
            // Act & Assert - UI関連定数
            Assert.AreEqual(1.0f, GameConstants.DEFAULT_UI_SCALE, 0.001f, "DEFAULT_UI_SCALE の値が期待値と異なる");
            Assert.AreEqual(1.1f, GameConstants.UI_HOVER_SCALE, 0.001f, "UI_HOVER_SCALE の値が期待値と異なる");
        }

        /// <summary>
        /// UI定数の論理的整合性テスト
        /// </summary>
        [Test]
        public void GameConstants_UIConstants_HaveLogicalConsistency()
        {
            // Assert - ホバー時スケールがデフォルトより大きいことを確認
            Assert.Greater(GameConstants.UI_HOVER_SCALE, GameConstants.DEFAULT_UI_SCALE, 
                "ホバー時スケールはデフォルトスケールより大きくなければならない");
            
            // Assert - スケールの差が妥当な範囲内であることを確認
            float scaleDifference = GameConstants.UI_HOVER_SCALE - GameConstants.DEFAULT_UI_SCALE;
            Assert.That(scaleDifference, Is.InRange(0.05f, 0.5f), 
                "スケール差は妥当な範囲（0.05-0.5）内でなければならない");
        }

        /// <summary>
        /// UI定数の正の値確認テスト
        /// </summary>
        [Test]
        public void GameConstants_UIConstants_ArePositiveValues()
        {
            // Assert - 全てのUIスケール定数が正の値であることを確認
            Assert.Greater(GameConstants.DEFAULT_UI_SCALE, 0f, "DEFAULT_UI_SCALE は正の値でなければならない");
            Assert.Greater(GameConstants.UI_HOVER_SCALE, 0f, "UI_HOVER_SCALE は正の値でなければならない");
        }

        /// <summary>
        /// UI定数の実用的範囲テスト
        /// </summary>
        [Test]
        public void GameConstants_UIConstants_AreInPracticalRange()
        {
            // Assert - UIスケールが実用的範囲内（0.1-5.0）であることを確認
            Assert.That(GameConstants.DEFAULT_UI_SCALE, Is.InRange(0.1f, 5.0f), 
                "DEFAULT_UI_SCALE は実用的な範囲（0.1-5.0）内でなければならない");
            Assert.That(GameConstants.UI_HOVER_SCALE, Is.InRange(0.1f, 5.0f), 
                "UI_HOVER_SCALE は実用的な範囲（0.1-5.0）内でなければならない");
        }

        #endregion

        #region Performance Constants Tests

        /// <summary>
        /// パフォーマンス関連定数の存在と妥当性テスト
        /// </summary>
        [Test]
        public void GameConstants_PerformanceConstants_HaveValidValues()
        {
            // Act & Assert - パフォーマンス関連定数
            Assert.AreEqual(50, GameConstants.OBJECT_POOL_INITIAL_SIZE, "OBJECT_POOL_INITIAL_SIZE の値が期待値と異なる");
            Assert.AreEqual(500, GameConstants.OBJECT_POOL_MAX_SIZE, "OBJECT_POOL_MAX_SIZE の値が期待値と異なる");
        }

        /// <summary>
        /// パフォーマンス定数の論理的整合性テスト
        /// </summary>
        [Test]
        public void GameConstants_PerformanceConstants_HaveLogicalConsistency()
        {
            // Assert - 最大サイズが初期サイズより大きいことを確認
            Assert.Greater(GameConstants.OBJECT_POOL_MAX_SIZE, GameConstants.OBJECT_POOL_INITIAL_SIZE, 
                "オブジェクトプール最大サイズは初期サイズより大きくなければならない");
            
            // Assert - 適切な比率であることを確認（最大サイズは初期サイズの2倍以上）
            Assert.GreaterOrEqual(GameConstants.OBJECT_POOL_MAX_SIZE, GameConstants.OBJECT_POOL_INITIAL_SIZE * 2, 
                "オブジェクトプール最大サイズは初期サイズの2倍以上でなければならない");
        }

        /// <summary>
        /// パフォーマンス定数の正の値確認テスト
        /// </summary>
        [Test]
        public void GameConstants_PerformanceConstants_ArePositiveValues()
        {
            // Assert - 全てのパフォーマンス定数が正の値であることを確認
            Assert.Greater(GameConstants.OBJECT_POOL_INITIAL_SIZE, 0, "OBJECT_POOL_INITIAL_SIZE は正の値でなければならない");
            Assert.Greater(GameConstants.OBJECT_POOL_MAX_SIZE, 0, "OBJECT_POOL_MAX_SIZE は正の値でなければならない");
        }

        /// <summary>
        /// パフォーマンス定数の実用的範囲テスト
        /// </summary>
        [Test]
        public void GameConstants_PerformanceConstants_AreInPracticalRange()
        {
            // Assert - オブジェクトプールサイズが実用的範囲内であることを確認
            Assert.That(GameConstants.OBJECT_POOL_INITIAL_SIZE, Is.InRange(1, 1000), 
                "OBJECT_POOL_INITIAL_SIZE は実用的な範囲（1-1000）内でなければならない");
            Assert.That(GameConstants.OBJECT_POOL_MAX_SIZE, Is.InRange(10, 10000), 
                "OBJECT_POOL_MAX_SIZE は実用的な範囲（10-10000）内でなければならない");
        }

        /// <summary>
        /// オブジェクトプールサイズの効率性テスト
        /// </summary>
        [Test]
        public void GameConstants_PerformanceConstants_AreEfficient()
        {
            // Assert - 初期サイズが小さすぎないことを確認（頻繁な拡張を防ぐ）
            Assert.GreaterOrEqual(GameConstants.OBJECT_POOL_INITIAL_SIZE, 10, 
                "オブジェクトプール初期サイズは最低10以上が推奨される");
            
            // Assert - 最大サイズが大きすぎないことを確認（メモリ効率）
            Assert.LessOrEqual(GameConstants.OBJECT_POOL_MAX_SIZE, 1000, 
                "オブジェクトプール最大サイズは1000以下が推奨される");
        }

        #endregion

        #region Cross-Category Consistency Tests

        /// <summary>
        /// 定数カテゴリ間の一貫性テスト
        /// </summary>
        [Test]
        public void GameConstants_CrossCategory_MaintainConsistency()
        {
            // Assert - テスト値の整合性（小さな回復時間とUIフェード時間の関係）
            // 小さなテスト値は、短いUIアニメーション時間内で完了すべき
            float testProcessingTime = 0.1f; // 仮想的なテスト処理時間
            Assert.LessOrEqual(testProcessingTime, GameConstants.UI_FADE_DURATION, 
                "テスト処理時間は UI フェード時間以下でなければならない");
            
            // Assert - パフォーマンスとUI設定の整合性
            // 初期プールサイズが小さい値なら、UI応答も迅速でなければならない
            if (GameConstants.OBJECT_POOL_INITIAL_SIZE <= 100)
            {
                Assert.LessOrEqual(GameConstants.UI_FADE_DURATION, 1.0f, 
                    "小さなプール設定時は UI フェードも短時間でなければならない");
            }
        }

        /// <summary>
        /// 定数の命名規則一貫性テスト
        /// </summary>
        [Test]
        public void GameConstants_NamingConventions_AreConsistent()
        {
            // このテストは定数の命名規則を確認
            // リフレクションを使用して全ての定数フィールドを取得し命名規則を確認
            var constantsType = typeof(GameConstants);
            var fields = constantsType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            
            foreach (var field in fields)
            {
                if (field.IsLiteral) // const フィールドのみ
                {
                    var name = field.Name;
                    
                    // Assert - 全て大文字とアンダースコアの命名規則
                    Assert.IsTrue(name.ToUpperInvariant() == name, 
                        $"定数 {name} は全て大文字でなければならない");
                    
                    // Assert - 意味のある名前の長さ（短すぎない）
                    Assert.GreaterOrEqual(name.Length, 3, 
                        $"定数 {name} は3文字以上の意味のある名前でなければならない");
                    
                    // Assert - 適切な接頭辞を持つ
                    bool hasValidPrefix = name.StartsWith("TEST_") || 
                                         name.StartsWith("MIN_") || 
                                         name.StartsWith("DEFAULT_") || 
                                         name.StartsWith("UI_") || 
                                         name.StartsWith("AUDIO_") || 
                                         name.StartsWith("OBJECT_");
                    
                    Assert.IsTrue(hasValidPrefix, 
                        $"定数 {name} は適切な接頭辞（TEST_, MIN_, DEFAULT_, UI_, AUDIO_, OBJECT_）を持つべき");
                }
            }
        }

        /// <summary>
        /// 定数値の型安全性テスト
        /// </summary>
        [Test]
        public void GameConstants_TypeSafety_IsCorrect()
        {
            // Assert - int定数が期待する型であることを確認
            Assert.IsInstanceOf<int>(GameConstants.TEST_HEAL_SMALL, "TEST_HEAL_SMALL は int 型でなければならない");
            Assert.IsInstanceOf<int>(GameConstants.TEST_HEAL_LARGE, "TEST_HEAL_LARGE は int 型でなければならない");
            Assert.IsInstanceOf<int>(GameConstants.TEST_DAMAGE_SMALL, "TEST_DAMAGE_SMALL は int 型でなければならない");
            Assert.IsInstanceOf<int>(GameConstants.TEST_DAMAGE_LARGE, "TEST_DAMAGE_LARGE は int 型でなければならない");
            Assert.IsInstanceOf<int>(GameConstants.OBJECT_POOL_INITIAL_SIZE, "OBJECT_POOL_INITIAL_SIZE は int 型でなければならない");
            Assert.IsInstanceOf<int>(GameConstants.OBJECT_POOL_MAX_SIZE, "OBJECT_POOL_MAX_SIZE は int 型でなければならない");
            
            // Assert - float定数が期待する型であることを確認
            Assert.IsInstanceOf<float>(GameConstants.MIN_LOADING_TIME, "MIN_LOADING_TIME は float 型でなければならない");
            Assert.IsInstanceOf<float>(GameConstants.UI_FADE_DURATION, "UI_FADE_DURATION は float 型でなければならない");
            Assert.IsInstanceOf<float>(GameConstants.AUDIO_FADE_DURATION, "AUDIO_FADE_DURATION は float 型でなければならない");
            Assert.IsInstanceOf<float>(GameConstants.DEFAULT_UI_SCALE, "DEFAULT_UI_SCALE は float 型でなければならない");
            Assert.IsInstanceOf<float>(GameConstants.UI_HOVER_SCALE, "UI_HOVER_SCALE は float 型でなければならない");
        }

        #endregion

        #region Usage Context Tests

        /// <summary>
        /// CommandInvokerEditor での使用想定テスト
        /// </summary>
        [Test]
        public void GameConstants_CommandInvokerEditor_UsageCompatibility()
        {
            // Assert - CommandInvokerEditor で使用される定数が適切な値であることを確認
            Assert.That(GameConstants.TEST_HEAL_SMALL, Is.InRange(5, 50), 
                "TEST_HEAL_SMALL は CommandInvokerEditor での使用に適した範囲（5-50）でなければならない");
            Assert.That(GameConstants.TEST_HEAL_LARGE, Is.InRange(20, 100), 
                "TEST_HEAL_LARGE は CommandInvokerEditor での使用に適した範囲（20-100）でなければならない");
            
            // Assert - テスト値の実用性確認
            Assert.That(GameConstants.TEST_DAMAGE_SMALL / (float)GameConstants.TEST_HEAL_SMALL, Is.EqualTo(1.0f).Within(0.01f), 
                "ダメージと回復のバランスが1:1でなければならない");
        }

        /// <summary>
        /// プール最適化での使用想定テスト
        /// </summary>
        [Test]
        public void GameConstants_ObjectPool_OptimalConfiguration()
        {
            // Assert - プール設定の最適性確認
            float poolGrowthRatio = (float)GameConstants.OBJECT_POOL_MAX_SIZE / GameConstants.OBJECT_POOL_INITIAL_SIZE;
            Assert.That(poolGrowthRatio, Is.InRange(5.0f, 20.0f), 
                "プールの成長率は5-20倍の範囲が最適");
            
            // Assert - メモリ効率性確認
            int totalPoolMemory = GameConstants.OBJECT_POOL_MAX_SIZE; // 仮想的なメモリ使用量
            Assert.LessOrEqual(totalPoolMemory, 1000, 
                "プール全体のメモリ使用量は合理的範囲内でなければならない");
        }

        #endregion
    }
}
