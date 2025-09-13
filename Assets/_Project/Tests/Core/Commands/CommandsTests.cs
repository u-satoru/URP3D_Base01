using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Tests.Core.Commands
{
    /// <summary>
    /// Commands配下の実装に対する包括的なテストクラス
    /// DamageCommand、HealCommand、およびICommandインターフェースの動作を検証
    /// </summary>
    [TestFixture]
    public class CommandsTests
    {
        private MockHealthTarget mockHealthTarget;
        
        /// <summary>
        /// IHealthTargetのモック実装
        /// テスト用のヘルスターゲット
        /// </summary>
        private class MockHealthTarget : IHealthTarget
        {
            public int CurrentHealth { get; private set; }
            public int MaxHealth { get; private set; }
            
            // テスト検証用のフラグ
            public int LastHealAmount { get; private set; }
            public int LastDamageAmount { get; private set; }
            public string LastDamageElementType { get; private set; }
            public int HealCallCount { get; private set; }
            public int DamageCallCount { get; private set; }
            
            public MockHealthTarget(int maxHealth = 100, int currentHealth = -1)
            {
                MaxHealth = maxHealth;
                CurrentHealth = currentHealth == -1 ? maxHealth : currentHealth;
                ResetCallCounts();
            }
            
            public void Heal(int amount)
            {
                LastHealAmount = amount;
                HealCallCount++;
                CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
            }
            
            public void TakeDamage(int amount)
            {
                LastDamageAmount = amount;
                LastDamageElementType = "physical";
                DamageCallCount++;
                CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
            }
            
            public void TakeDamage(int amount, string elementType)
            {
                LastDamageAmount = amount;
                LastDamageElementType = elementType;
                DamageCallCount++;
                CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
            }
            
            public void ResetCallCounts()
            {
                LastHealAmount = 0;
                LastDamageAmount = 0;
                LastDamageElementType = null;
                HealCallCount = 0;
                DamageCallCount = 0;
            }
            
            public void SetHealth(int health)
            {
                CurrentHealth = Mathf.Clamp(health, 0, MaxHealth);
            }
        }

        [SetUp]
        public void SetUp()
        {
            mockHealthTarget = new MockHealthTarget(100, 50);
        }

        [TearDown]
        public void TearDown()
        {
            mockHealthTarget = null;
        }

        #region DamageCommand Tests

        /// <summary>
        /// DamageCommandの基本的な実行動作テスト
        /// </summary>
        [Test]
        public void DamageCommand_Execute_AppliesCorrectDamageAmount()
        {
            // Arrange
            const int damageAmount = 20;
            var damageCommand = new DamageCommand(mockHealthTarget, damageAmount);
            var initialHealth = mockHealthTarget.CurrentHealth;
            
            // Act
            damageCommand.Execute();
            
            // Assert
            Assert.AreEqual(damageAmount, mockHealthTarget.LastDamageAmount, "ダメージ量が正しく設定されていない");
            Assert.AreEqual(1, mockHealthTarget.DamageCallCount, "TakeDamageが1回呼ばれるべき");
            Assert.AreEqual(initialHealth - damageAmount, mockHealthTarget.CurrentHealth, "ヘルスが正しく減少していない");
        }

        /// <summary>
        /// DamageCommandの属性ダメージテスト
        /// </summary>
        [Test]
        public void DamageCommand_ExecuteWithElementType_AppliesCorrectElementalDamage()
        {
            // Arrange
            const int damageAmount = 15;
            const string elementType = "fire";
            var damageCommand = new DamageCommand(mockHealthTarget, damageAmount, elementType);
            
            // Act
            damageCommand.Execute();
            
            // Assert
            Assert.AreEqual(damageAmount, mockHealthTarget.LastDamageAmount, "ダメージ量が正しく設定されていない");
            Assert.AreEqual(elementType, mockHealthTarget.LastDamageElementType, "属性タイプが正しく設定されていない");
            Assert.AreEqual(1, mockHealthTarget.DamageCallCount, "TakeDamageが1回呼ばれるべき");
        }

        /// <summary>
        /// DamageCommandのUndo機能テスト
        /// </summary>
        [Test]
        public void DamageCommand_Undo_RestoresHealthCorrectly()
        {
            // Arrange
            const int damageAmount = 25;
            var damageCommand = new DamageCommand(mockHealthTarget, damageAmount);
            var initialHealth = mockHealthTarget.CurrentHealth;
            
            // Act
            damageCommand.Execute();
            mockHealthTarget.ResetCallCounts(); // Execute後のカウントをリセット
            damageCommand.Undo();
            
            // Assert
            Assert.AreEqual(damageAmount, mockHealthTarget.LastHealAmount, "Undo時の回復量が実行時のダメージ量と一致していない");
            Assert.AreEqual(1, mockHealthTarget.HealCallCount, "Undo時にHealが1回呼ばれるべき");
            Assert.AreEqual(initialHealth, mockHealthTarget.CurrentHealth, "Undo後にヘルスが初期値に戻っていない");
        }

        /// <summary>
        /// DamageCommandがUndo可能であることを確認
        /// </summary>
        [Test]
        public void DamageCommand_CanUndo_ReturnsTrue()
        {
            // Arrange
            var damageCommand = new DamageCommand(mockHealthTarget, 10);
            
            // Assert
            Assert.IsTrue(damageCommand.CanUndo, "DamageCommandはUndo可能でなければならない");
        }

        /// <summary>
        /// DamageCommandのReset機能テスト
        /// </summary>
        [Test]
        public void DamageCommand_Reset_ClearsAllParameters()
        {
            // Arrange
            var damageCommand = new DamageCommand(mockHealthTarget, 30, "ice") as IResettableCommand;
            
            // Act
            damageCommand.Reset();
            damageCommand.Execute(); // Reset後はExecuteしても何も起こらない
            
            // Assert
            Assert.AreEqual(0, mockHealthTarget.DamageCallCount, "Reset後はDamageが呼ばれないべき");
        }

        /// <summary>
        /// DamageCommandの型安全な初期化テスト
        /// </summary>
        [Test]
        public void DamageCommand_TypeSafeInitialize_SetsParametersCorrectly()
        {
            // Arrange
            var damageCommand = new DamageCommand() as IResettableCommand;
            const int damageAmount = 40;
            const string elementType = "thunder";
            
            // Act
            ((DamageCommand)damageCommand).Initialize(mockHealthTarget, damageAmount, elementType);
            damageCommand.Execute();
            
            // Assert
            Assert.AreEqual(damageAmount, mockHealthTarget.LastDamageAmount, "初期化後のダメージ量が正しくない");
            Assert.AreEqual(elementType, mockHealthTarget.LastDamageElementType, "初期化後の属性タイプが正しくない");
        }

        /// <summary>
        /// DamageCommandの配列による初期化テスト
        /// </summary>
        [Test]
        public void DamageCommand_ArrayInitialize_SetsParametersCorrectly()
        {
            // Arrange
            var damageCommand = new DamageCommand() as IResettableCommand;
            const int damageAmount = 35;
            const string elementType = "physical";
            
            // Act
            damageCommand.Initialize(mockHealthTarget, damageAmount, elementType);
            damageCommand.Execute();
            
            // Assert
            Assert.AreEqual(damageAmount, mockHealthTarget.LastDamageAmount, "配列初期化後のダメージ量が正しくない");
            Assert.AreEqual(elementType, mockHealthTarget.LastDamageElementType, "配列初期化後の属性タイプが正しくない");
        }

        /// <summary>
        /// DamageCommandの不正な初期化パラメーターに対するエラーハンドリングテスト
        /// </summary>
        [Test]
        public void DamageCommand_InvalidInitializeParameters_HandlesGracefully()
        {
            // Arrange
            var damageCommand = new DamageCommand() as IResettableCommand;
            
            // Act - パラメーター不足での初期化試行
            damageCommand.Initialize(mockHealthTarget);
            damageCommand.Execute();
            Assert.AreEqual(0, mockHealthTarget.DamageCallCount, "不正な初期化後はダメージが適用されないべき");
            
            // Act - 無効なターゲットでの初期化試行
            damageCommand.Initialize("invalid_target", 10);
            damageCommand.Execute();
            Assert.AreEqual(0, mockHealthTarget.DamageCallCount, "無効ターゲット初期化後はダメージが適用されないべき");
            
            // Act - 無効なダメージ量での初期化試行
            damageCommand.Initialize(mockHealthTarget, "invalid_damage");
            damageCommand.Execute();
            Assert.AreEqual(0, mockHealthTarget.DamageCallCount, "無効ダメージ量初期化後はダメージが適用されないべき");
        }

        /// <summary>
        /// DamageCommandのnullターゲットに対するエラーハンドリングテスト
        /// </summary>
        [Test]
        public void DamageCommand_NullTarget_HandlesGracefully()
        {
            // Arrange
            var damageCommand = new DamageCommand(null, 10);
            
            // Act & Assert - nullターゲットでも例外が発生しないことを確認
            Assert.DoesNotThrow(() => damageCommand.Execute(), "nullターゲットでExecute時に例外が発生");
            Assert.DoesNotThrow(() => damageCommand.Undo(), "nullターゲットでUndo時に例外が発生");
        }

        #endregion

        #region HealCommand Tests

        /// <summary>
        /// HealCommandの基本的な実行動作テスト
        /// </summary>
        [Test]
        public void HealCommand_Execute_AppliesCorrectHealAmount()
        {
            // Arrange
            const int healAmount = 30;
            var healCommand = new HealCommand(mockHealthTarget, healAmount);
            var initialHealth = mockHealthTarget.CurrentHealth;
            
            // Act
            healCommand.Execute();
            
            // Assert
            Assert.AreEqual(healAmount, mockHealthTarget.LastHealAmount, "回復量が正しく設定されていない");
            Assert.AreEqual(1, mockHealthTarget.HealCallCount, "Healが1回呼ばれるべき");
            Assert.AreEqual(initialHealth + healAmount, mockHealthTarget.CurrentHealth, "ヘルスが正しく回復していない");
        }

        /// <summary>
        /// HealCommandのUndo機能テスト
        /// </summary>
        [Test]
        public void HealCommand_Undo_DealsCorrectDamage()
        {
            // Arrange
            const int healAmount = 20;
            var healCommand = new HealCommand(mockHealthTarget, healAmount);
            var initialHealth = mockHealthTarget.CurrentHealth;
            
            // Act
            healCommand.Execute();
            mockHealthTarget.ResetCallCounts(); // Execute後のカウントをリセット
            healCommand.Undo();
            
            // Assert
            Assert.AreEqual(healAmount, mockHealthTarget.LastDamageAmount, "Undo時のダメージ量が実行時の回復量と一致していない");
            Assert.AreEqual("healing_undo", mockHealthTarget.LastDamageElementType, "Undo時の属性タイプが正しくない");
            Assert.AreEqual(1, mockHealthTarget.DamageCallCount, "Undo時にTakeDamageが1回呼ばれるべき");
            Assert.AreEqual(initialHealth, mockHealthTarget.CurrentHealth, "Undo後にヘルスが初期値に戻っていない");
        }

        /// <summary>
        /// HealCommandがUndo可能であることを確認
        /// </summary>
        [Test]
        public void HealCommand_CanUndo_ReturnsTrue()
        {
            // Arrange
            var healCommand = new HealCommand(mockHealthTarget, 15);
            
            // Assert
            Assert.IsTrue(healCommand.CanUndo, "HealCommandはUndo可能でなければならない");
        }

        /// <summary>
        /// HealCommandのReset機能テスト
        /// </summary>
        [Test]
        public void HealCommand_Reset_ClearsAllParameters()
        {
            // Arrange
            var healCommand = new HealCommand(mockHealthTarget, 25) as IResettableCommand;
            
            // Act
            healCommand.Reset();
            healCommand.Execute(); // Reset後はExecuteしても何も起こらない
            
            // Assert
            Assert.AreEqual(0, mockHealthTarget.HealCallCount, "Reset後はHealが呼ばれないべき");
        }

        /// <summary>
        /// HealCommandの型安全な初期化テスト
        /// </summary>
        [Test]
        public void HealCommand_TypeSafeInitialize_SetsParametersCorrectly()
        {
            // Arrange
            var healCommand = new HealCommand() as IResettableCommand;
            const int healAmount = 45;
            
            // Act
            ((HealCommand)healCommand).Initialize(mockHealthTarget, healAmount);
            healCommand.Execute();
            
            // Assert
            Assert.AreEqual(healAmount, mockHealthTarget.LastHealAmount, "初期化後の回復量が正しくない");
            Assert.AreEqual(1, mockHealthTarget.HealCallCount, "初期化後にHealが1回呼ばれるべき");
        }

        /// <summary>
        /// HealCommandの配列による初期化テスト
        /// </summary>
        [Test]
        public void HealCommand_ArrayInitialize_SetsParametersCorrectly()
        {
            // Arrange
            var healCommand = new HealCommand() as IResettableCommand;
            const int healAmount = 50;
            
            // Act
            healCommand.Initialize(mockHealthTarget, healAmount);
            healCommand.Execute();
            
            // Assert
            Assert.AreEqual(healAmount, mockHealthTarget.LastHealAmount, "配列初期化後の回復量が正しくない");
            Assert.AreEqual(1, mockHealthTarget.HealCallCount, "配列初期化後にHealが1回呼ばれるべき");
        }

        /// <summary>
        /// HealCommandの不正な初期化パラメーターに対するエラーハンドリングテスト
        /// </summary>
        [Test]
        public void HealCommand_InvalidInitializeParameters_HandlesGracefully()
        {
            // Arrange
            var healCommand = new HealCommand() as IResettableCommand;
            
            // Act - パラメーター不足での初期化試行
            healCommand.Initialize(mockHealthTarget);
            healCommand.Execute();
            Assert.AreEqual(0, mockHealthTarget.HealCallCount, "不正な初期化後は回復が適用されないべき");
            
            // Act - 無効なターゲットでの初期化試行
            healCommand.Initialize("invalid_target", 10);
            healCommand.Execute();
            Assert.AreEqual(0, mockHealthTarget.HealCallCount, "無効ターゲット初期化後は回復が適用されないべき");
            
            // Act - 無効な回復量での初期化試行
            healCommand.Initialize(mockHealthTarget, "invalid_heal");
            healCommand.Execute();
            Assert.AreEqual(0, mockHealthTarget.HealCallCount, "無効回復量初期化後は回復が適用されないべき");
        }

        /// <summary>
        /// HealCommandのnullターゲットに対するエラーハンドリングテスト
        /// </summary>
        [Test]
        public void HealCommand_NullTarget_HandlesGracefully()
        {
            // Arrange
            var healCommand = new HealCommand(null, 10);
            
            // Act & Assert - nullターゲットでも例外が発生しないことを確認
            Assert.DoesNotThrow(() => healCommand.Execute(), "nullターゲットでExecute時に例外が発生");
            Assert.DoesNotThrow(() => healCommand.Undo(), "nullターゲットでUndo時に例外が発生");
        }

        #endregion

        #region Command Sequence Tests

        /// <summary>
        /// DamageCommandとHealCommandの組み合わせテスト
        /// </summary>
        [Test]
        public void Commands_DamageAndHealSequence_WorksCorrectly()
        {
            // Arrange
            const int initialHealth = 50;
            mockHealthTarget.SetHealth(initialHealth);
            var damageCommand = new DamageCommand(mockHealthTarget, 20);
            var healCommand = new HealCommand(mockHealthTarget, 15);
            
            // Act
            damageCommand.Execute(); // 50 - 20 = 30
            healCommand.Execute();   // 30 + 15 = 45
            
            // Assert
            Assert.AreEqual(45, mockHealthTarget.CurrentHealth, "ダメージと回復の組み合わせが正しく動作していない");
            Assert.AreEqual(1, mockHealthTarget.DamageCallCount, "ダメージが1回適用されるべき");
            Assert.AreEqual(1, mockHealthTarget.HealCallCount, "回復が1回適用されるべき");
        }

        /// <summary>
        /// コマンドのUndo順序テスト
        /// </summary>
        [Test]
        public void Commands_UndoSequence_RestoresCorrectState()
        {
            // Arrange
            const int initialHealth = 60;
            mockHealthTarget.SetHealth(initialHealth);
            var damageCommand = new DamageCommand(mockHealthTarget, 25);
            var healCommand = new HealCommand(mockHealthTarget, 20);
            
            // Act - Execute sequence
            damageCommand.Execute(); // 60 - 25 = 35
            healCommand.Execute();   // 35 + 20 = 55
            
            // Act - Undo in reverse order
            healCommand.Undo();      // 55 - 20 = 35 (healing undone)
            damageCommand.Undo();    // 35 + 25 = 60 (damage undone)
            
            // Assert
            Assert.AreEqual(initialHealth, mockHealthTarget.CurrentHealth, "Undo順序で初期状態に戻っていない");
        }

        /// <summary>
        /// プール化されたコマンドの再利用テスト
        /// </summary>
        [Test]
        public void Commands_PooledReuse_WorksCorrectly()
        {
            // Arrange
            var damageCommand = new DamageCommand() as IResettableCommand;
            var secondTarget = new MockHealthTarget(100, 80);
            
            // Act - 最初の使用
            damageCommand.Initialize(mockHealthTarget, 30);
            damageCommand.Execute();
            
            // Act - リセットして再利用
            damageCommand.Reset();
            damageCommand.Initialize(secondTarget, 40);
            damageCommand.Execute();
            
            // Assert
            Assert.AreEqual(20, mockHealthTarget.CurrentHealth, "最初のターゲットのヘルスが正しくない");
            Assert.AreEqual(40, secondTarget.CurrentHealth, "2番目のターゲットのヘルスが正しくない");
        }

        #endregion
    }
}