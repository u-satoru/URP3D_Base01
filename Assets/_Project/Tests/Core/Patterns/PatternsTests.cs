using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using asterivo.Unity60.Core.Patterns.ObjectPool;
using asterivo.Unity60.Core.Patterns.Registry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace asterivo.Unity60.Tests.Core.Patterns
{
    /// <summary>
    /// Patterns配下の実装に対する包括的なテストクラス
    /// ObjectPool、Registry パターンの動作を検証
    /// Factory + Strategy + Registry パターンの組み合わせをテスト
    /// </summary>
    [TestFixture]
    public class PatternsTests
    {
        // テスト用のダミークラス
        private class TestItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsActive { get; set; }
            
            public TestItem(int id = 0, string name = "Default")
            {
                Id = id;
                Name = name;
                IsActive = true;
            }
            
            public void Reset()
            {
                IsActive = false;
            }
            
            public void Activate()
            {
                IsActive = true;
            }
        }
        
        private class AnotherTestItem
        {
            public float Value { get; set; }
            public AnotherTestItem(float value = 0f) { Value = value; }
        }

        #region ObjectPool Tests

        private GenericObjectPool<TestItem> objectPool;
        private int factoryCallCount;
        private int getCallbackCount;
        private int returnCallbackCount;

        [SetUp]
        public void SetUp()
        {
            // テストカウンターをリセット
            factoryCallCount = 0;
            getCallbackCount = 0;
            returnCallbackCount = 0;
            
            // ObjectPoolのセットアップ
            objectPool = new GenericObjectPool<TestItem>(
                factory: () => {
                    factoryCallCount++;
                    return new TestItem(factoryCallCount, $"Item_{factoryCallCount}");
                },
                onGet: (item) => {
                    getCallbackCount++;
                    item.Activate();
                },
                onReturn: (item) => {
                    returnCallbackCount++;
                    item.Reset();
                },
                maxSize: 5
            );
        }

        [TearDown]
        public void TearDown()
        {
            objectPool?.Clear();
            objectPool = null;
        }

        /// <summary>
        /// ObjectPoolの基本的な取得動作テスト
        /// </summary>
        [Test]
        public void ObjectPool_Get_CreatesAndReturnsObject()
        {
            // Act
            var item = objectPool.Get();
            
            // Assert
            Assert.IsNotNull(item, "プールからオブジェクトが取得できない");
            Assert.AreEqual(1, item.Id, "ファクトリで作成されたオブジェクトのIDが正しくない");
            Assert.AreEqual("Item_1", item.Name, "ファクトリで作成されたオブジェクトの名前が正しくない");
            Assert.IsTrue(item.IsActive, "onGetコールバックでオブジェクトが有効化されていない");
            Assert.AreEqual(1, factoryCallCount, "ファクトリが1回呼ばれるべき");
            Assert.AreEqual(1, getCallbackCount, "onGetコールバックが1回呼ばれるべき");
        }

        /// <summary>
        /// ObjectPoolの返却動作テスト
        /// </summary>
        [Test]
        public void ObjectPool_Return_StoresObjectInPool()
        {
            // Arrange
            var item = objectPool.Get();
            
            // Act
            objectPool.Return(item);
            
            // Assert
            Assert.IsFalse(item.IsActive, "onReturnコールバックでオブジェクトがリセットされていない");
            Assert.AreEqual(1, returnCallbackCount, "onReturnコールバックが1回呼ばれるべき");
            
            var statistics = objectPool.GetStatistics();
            Assert.AreEqual(1, statistics.TotalCreated, "総作成数が正しくない");
            Assert.AreEqual(1, statistics.TotalReturned, "総返却数が正しくない");
            Assert.AreEqual(1, statistics.CurrentInPool, "プール内現在数が正しくない");
        }

        /// <summary>
        /// ObjectPoolの再利用動作テスト
        /// </summary>
        [Test]
        public void ObjectPool_Reuse_ReturnsExistingObject()
        {
            // Arrange - オブジェクトを取得して返却
            var item1 = objectPool.Get();
            objectPool.Return(item1);
            
            // Act - 再度取得
            var item2 = objectPool.Get();
            
            // Assert - 同じオブジェクトが返却されることを確認
            Assert.AreSame(item1, item2, "返却されたオブジェクトが再利用されていない");
            Assert.AreEqual(1, factoryCallCount, "ファクトリが再利用時に余分に呼ばれている");
            Assert.AreEqual(2, getCallbackCount, "onGetコールバックが2回呼ばれるべき");
            
            var statistics = objectPool.GetStatistics();
            Assert.AreEqual(1, statistics.TotalReused, "再利用回数が正しくない");
            Assert.AreEqual(1.0f, statistics.ReuseRatio, 0.01f, "再利用率が正しくない");
        }

        /// <summary>
        /// ObjectPoolの事前ウォームアップテスト
        /// </summary>
        [Test]
        public void ObjectPool_Prewarm_CreatesObjectsInAdvance()
        {
            // Act
            objectPool.Prewarm(3);
            
            // Assert
            Assert.AreEqual(3, factoryCallCount, "プリウォームで3個のオブジェクトが作成されるべき");
            
            var statistics = objectPool.GetStatistics();
            Assert.AreEqual(3, statistics.TotalCreated, "プリウォーム後の総作成数が正しくない");
            Assert.AreEqual(3, statistics.CurrentInPool, "プリウォーム後のプール内現在数が正しくない");
            
            // プリウォームされたオブジェクトを取得
            var item1 = objectPool.Get();
            var item2 = objectPool.Get();
            var item3 = objectPool.Get();
            
            Assert.AreEqual(3, factoryCallCount, "プリウォーム後の取得でファクトリが余分に呼ばれていない");
            Assert.IsNotNull(item1, "プリウォームされたオブジェクト1が取得できない");
            Assert.IsNotNull(item2, "プリウォームされたオブジェクト2が取得できない");
            Assert.IsNotNull(item3, "プリウォームされたオブジェクト3が取得できない");
        }

        /// <summary>
        /// ObjectPoolの最大サイズ制限テスト
        /// </summary>
        [Test]
        public void ObjectPool_MaxSize_LimitsPoolCapacity()
        {
            // Arrange - 最大サイズ（5）を超える数のオブジェクトを作成して返却
            var items = new List<TestItem>();
            for (int i = 0; i < 7; i++)
            {
                items.Add(objectPool.Get());
            }
            
            // Act - 全オブジェクトを返却
            foreach (var item in items)
            {
                objectPool.Return(item);
            }
            
            // Assert - プール内のオブジェクト数が最大サイズで制限されている
            var statistics = objectPool.GetStatistics();
            Assert.AreEqual(5, statistics.CurrentInPool, "プール内現在数が最大サイズで制限されていない");
            Assert.AreEqual(5, statistics.TotalReturned, "プールの容量制限により返却数が制限されるべき");
            Assert.AreEqual(7, statistics.TotalCreated, "総作成数は制限されない");
        }

        /// <summary>
        /// ObjectPoolのクリア動作テスト
        /// </summary>
        [Test]
        public void ObjectPool_Clear_RemovesAllObjects()
        {
            // Arrange
            objectPool.Prewarm(3);
            Assert.AreEqual(3, objectPool.GetStatistics().CurrentInPool, "事前条件：プールに3個のオブジェクトがあるべき");
            
            // Act
            objectPool.Clear();
            
            // Assert
            var statistics = objectPool.GetStatistics();
            Assert.AreEqual(0, statistics.CurrentInPool, "クリア後のプール内現在数が0でない");
            
            // クリア後の取得で新しいオブジェクトが作成されることを確認
            var item = objectPool.Get();
            Assert.IsNotNull(item, "クリア後にオブジェクトが取得できない");
            Assert.AreEqual(4, factoryCallCount, "クリア後の取得で新しいオブジェクトが作成されるべき");
        }

        /// <summary>
        /// ObjectPoolのnullオブジェクト返却ハンドリングテスト
        /// </summary>
        [Test]
        public void ObjectPool_ReturnNull_HandlesGracefully()
        {
            // Arrange
            var initialStatistics = objectPool.GetStatistics();
            
            // Act & Assert - nullを返却してもエラーが発生しない
            Assert.DoesNotThrow(() => objectPool.Return(null), "null返却で例外が発生");
            
            var statistics = objectPool.GetStatistics();
            Assert.AreEqual(initialStatistics.TotalReturned, statistics.TotalReturned, "null返却で返却数が変化している");
            Assert.AreEqual(0, returnCallbackCount, "null返却でコールバックが呼ばれている");
        }

        /// <summary>
        /// ObjectPoolの統計情報精度テスト
        /// </summary>
        [Test]
        public void ObjectPool_Statistics_AccurateTracking()
        {
            // Arrange & Act - 複数のオペレーションを実行
            objectPool.Prewarm(2);                    // 2個作成
            var item1 = objectPool.Get();              // 1個再利用
            var item2 = objectPool.Get();              // 1個再利用
            var item3 = objectPool.Get();              // 1個新規作成
            
            objectPool.Return(item1);                  // 1個返却
            objectPool.Return(item2);                  // 1個返却
            // item3は返却しない
            
            // Assert
            var statistics = objectPool.GetStatistics();
            Assert.AreEqual(3, statistics.TotalCreated, "総作成数が正しくない");
            Assert.AreEqual(2, statistics.TotalReused, "総再利用数が正しくない");
            Assert.AreEqual(2, statistics.TotalReturned, "総返却数が正しくない");
            Assert.AreEqual(2, statistics.CurrentInPool, "現在プール内数が正しくない");
            
            float expectedReuseRatio = 2f / 3f; // 2回再利用 / 3個作成
            Assert.AreEqual(expectedReuseRatio, statistics.ReuseRatio, 0.01f, "再利用率が正しくない");
        }

        /// <summary>
        /// ObjectPoolの引数なしファクトリでの例外テスト
        /// </summary>
        [Test]
        public void ObjectPool_NullFactory_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new GenericObjectPool<TestItem>(null), 
                "nullファクトリで例外が発生するべき");
        }

        #endregion

        #region TypeRegistry Tests

        private TypeRegistry<string> stringRegistry;
        private TypeRegistry<TestItem> itemRegistry;

        [SetUp]
        public void SetUpRegistry()
        {
            stringRegistry = new TypeRegistry<string>();
            itemRegistry = new TypeRegistry<TestItem>(allowOverwrite: false);
        }

        [TearDown]
        public void TearDownRegistry()
        {
            stringRegistry?.Clear();
            itemRegistry?.Clear();
        }

        /// <summary>
        /// TypeRegistryの基本的な登録・取得動作テスト
        /// </summary>
        [Test]
        public void TypeRegistry_RegisterAndGet_WorksCorrectly()
        {
            // Arrange
            var testValue = "TestString";
            
            // Act
            stringRegistry.Register<string>(testValue);
            var retrieved = stringRegistry.Get<string>();
            
            // Assert
            Assert.AreEqual(testValue, retrieved, "登録された値が正しく取得できない");
            Assert.AreEqual(1, stringRegistry.Count, "登録後のカウントが正しくない");
            Assert.IsTrue(stringRegistry.IsRegistered<string>(), "型が登録されていると判定されない");
        }

        /// <summary>
        /// TypeRegistryの型指定登録・取得動作テスト
        /// </summary>
        [Test]
        public void TypeRegistry_RegisterByType_WorksCorrectly()
        {
            // Arrange
            var testItem = new TestItem(42, "TypeTest");
            
            // Act
            itemRegistry.Register(typeof(TestItem), testItem);
            var retrieved = itemRegistry.Get(typeof(TestItem));
            
            // Assert
            Assert.AreSame(testItem, retrieved, "型指定で登録された値が正しく取得できない");
            Assert.IsTrue(itemRegistry.IsRegistered(typeof(TestItem)), "型が登録されていると判定されない");
        }

        /// <summary>
        /// TypeRegistryの複数型登録テスト
        /// </summary>
        [Test]
        public void TypeRegistry_MultipleTypes_RegisterIndependently()
        {
            // Arrange
            var stringValue = "StringValue";
            var intValue = "IntValue";
            var floatValue = "FloatValue";
            
            // Act
            stringRegistry.Register<string>(stringValue);
            stringRegistry.Register<int>(intValue);
            stringRegistry.Register<float>(floatValue);
            
            // Assert
            Assert.AreEqual(3, stringRegistry.Count, "複数型登録後のカウントが正しくない");
            Assert.AreEqual(stringValue, stringRegistry.Get<string>(), "string型の値が正しくない");
            Assert.AreEqual(intValue, stringRegistry.Get<int>(), "int型の値が正しくない");
            Assert.AreEqual(floatValue, stringRegistry.Get<float>(), "float型の値が正しくない");
            
            Assert.IsTrue(stringRegistry.IsRegistered<string>(), "string型が登録されていない");
            Assert.IsTrue(stringRegistry.IsRegistered<int>(), "int型が登録されていない");
            Assert.IsTrue(stringRegistry.IsRegistered<float>(), "float型が登録されていない");
        }

        /// <summary>
        /// TypeRegistryの上書き許可・禁止テスト
        /// </summary>
        [Test]
        public void TypeRegistry_Overwrite_RespectsSetting()
        {
            // Arrange - 上書き許可レジストリ（stringRegistry）
            stringRegistry.Register<string>("Original");
            
            // Act & Assert - 上書き許可の場合
            Assert.DoesNotThrow(() => stringRegistry.Register<string>("Overwritten"), 
                "上書き許可レジストリで上書きできない");
            Assert.AreEqual("Overwritten", stringRegistry.Get<string>(), "上書きされた値が取得できない");
            
            // Arrange - 上書き禁止レジストリ（itemRegistry）
            var originalItem = new TestItem(1, "Original");
            itemRegistry.Register<TestItem>(originalItem);
            
            // Act & Assert - 上書き禁止の場合
            var newItem = new TestItem(2, "New");
            Assert.Throws<InvalidOperationException>(() => itemRegistry.Register<TestItem>(newItem), 
                "上書き禁止レジストリで上書きが許可されている");
            Assert.AreSame(originalItem, itemRegistry.Get<TestItem>(), "上書き禁止時に元の値が変更されている");
        }

        /// <summary>
        /// TypeRegistryの登録解除テスト
        /// </summary>
        [Test]
        public void TypeRegistry_Unregister_RemovesRegistration()
        {
            // Arrange
            stringRegistry.Register<string>("ToBeRemoved");
            Assert.IsTrue(stringRegistry.IsRegistered<string>(), "事前条件：型が登録されているべき");
            
            // Act
            bool result = stringRegistry.Unregister(typeof(string));
            
            // Assert
            Assert.IsTrue(result, "登録解除が成功を返すべき");
            Assert.IsFalse(stringRegistry.IsRegistered<string>(), "登録解除後に型が登録されていない状態になるべき");
            Assert.AreEqual(0, stringRegistry.Count, "登録解除後のカウントが0になるべき");
            
            // 存在しない型の登録解除
            bool falseResult = stringRegistry.Unregister(typeof(int));
            Assert.IsFalse(falseResult, "存在しない型の登録解除は失敗を返すべき");
        }

        /// <summary>
        /// TypeRegistryの安全な取得（TryGet）テスト
        /// </summary>
        [Test]
        public void TypeRegistry_TryGet_SafeRetrieval()
        {
            // Arrange
            var testValue = "SafeGetTest";
            stringRegistry.Register<string>(testValue);
            
            // Act & Assert - 登録済み型の安全取得
            bool success = stringRegistry.TryGet<string>(out string retrieved);
            Assert.IsTrue(success, "登録済み型の安全取得が成功を返すべき");
            Assert.AreEqual(testValue, retrieved, "安全取得で正しい値が取得できない");
            
            // Act & Assert - 未登録型の安全取得
            bool failure = stringRegistry.TryGet<int>(out string notFound);
            Assert.IsFalse(failure, "未登録型の安全取得は失敗を返すべき");
            Assert.AreEqual(default(string), notFound, "失敗時にデフォルト値が返されるべき");
            
            // 型指定版のテスト
            bool typeSuccess = stringRegistry.TryGet(typeof(string), out string typeRetrieved);
            Assert.IsTrue(typeSuccess, "型指定での安全取得が成功を返すべき");
            Assert.AreEqual(testValue, typeRetrieved, "型指定での安全取得で正しい値が取得できない");
        }

        /// <summary>
        /// TypeRegistryの未登録型アクセス例外テスト
        /// </summary>
        [Test]
        public void TypeRegistry_GetUnregistered_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => stringRegistry.Get<string>(), 
                "未登録型のGet呼び出しで例外が発生するべき");
            
            Assert.Throws<KeyNotFoundException>(() => stringRegistry.Get(typeof(int)), 
                "型指定での未登録型Get呼び出しで例外が発生するべき");
        }

        /// <summary>
        /// TypeRegistryのコレクション取得テスト
        /// </summary>
        [Test]
        public void TypeRegistry_GetKeysAndValues_ReturnsCollections()
        {
            // Arrange
            stringRegistry.Register<string>("StringValue");
            stringRegistry.Register<int>("IntValue");
            stringRegistry.Register<float>("FloatValue");
            
            // Act
            var keys = stringRegistry.GetKeys().ToList();
            var values = stringRegistry.GetValues().ToList();
            
            // Assert
            Assert.AreEqual(3, keys.Count, "キーコレクションの数が正しくない");
            Assert.AreEqual(3, values.Count, "値コレクションの数が正しくない");
            
            Assert.Contains(typeof(string), keys, "string型がキーコレクションに含まれていない");
            Assert.Contains(typeof(int), keys, "int型がキーコレクションに含まれていない");
            Assert.Contains(typeof(float), keys, "float型がキーコレクションに含まれていない");
            
            Assert.Contains("StringValue", values, "StringValueが値コレクションに含まれていない");
            Assert.Contains("IntValue", values, "IntValueが値コレクションに含まれていない");
            Assert.Contains("FloatValue", values, "FloatValueが値コレクションに含まれていない");
        }

        /// <summary>
        /// TypeRegistryのクリア動作テスト
        /// </summary>
        [Test]
        public void TypeRegistry_Clear_RemovesAllRegistrations()
        {
            // Arrange
            stringRegistry.Register<string>("Value1");
            stringRegistry.Register<int>("Value2");
            Assert.AreEqual(2, stringRegistry.Count, "事前条件：2つの型が登録されているべき");
            
            // Act
            stringRegistry.Clear();
            
            // Assert
            Assert.AreEqual(0, stringRegistry.Count, "クリア後のカウントが0でない");
            Assert.IsFalse(stringRegistry.IsRegistered<string>(), "クリア後にstring型が登録されている");
            Assert.IsFalse(stringRegistry.IsRegistered<int>(), "クリア後にint型が登録されている");
            
            var keys = stringRegistry.GetKeys().ToList();
            var values = stringRegistry.GetValues().ToList();
            Assert.AreEqual(0, keys.Count, "クリア後のキーコレクションが空でない");
            Assert.AreEqual(0, values.Count, "クリア後の値コレクションが空でない");
        }

        /// <summary>
        /// TypeRegistryのnull型処理テスト
        /// </summary>
        [Test]
        public void TypeRegistry_NullType_HandlesGracefully()
        {
            // Act & Assert - null型での各操作が適切にハンドリングされることを確認
            Assert.Throws<ArgumentNullException>(() => stringRegistry.Register(null, "Value"), 
                "null型の登録で例外が発生するべき");
            
            Assert.Throws<ArgumentNullException>(() => stringRegistry.Get(null), 
                "null型の取得で例外が発生するべき");
            
            Assert.IsFalse(stringRegistry.TryGet(null, out string value), 
                "null型の安全取得は失敗を返すべき");
            
            Assert.IsFalse(stringRegistry.IsRegistered(null), 
                "null型は登録されていない判定になるべき");
            
            Assert.IsFalse(stringRegistry.Unregister(null), 
                "null型の登録解除は失敗を返すべき");
        }

        #endregion

        #region Integration Tests

        /// <summary>
        /// ObjectPoolとTypeRegistryの統合テスト
        /// ファクトリ + レジストリパターンの組み合わせ
        /// </summary>
        [Test]
        public void Integration_PoolWithRegistry_WorksTogether()
        {
            // Arrange - レジストリにプールを登録
            var poolRegistry = new TypeRegistry<IObjectPool<TestItem>>();
            
            var testItemPool = new GenericObjectPool<TestItem>(
                () => new TestItem(UnityEngine.Random.Range(1, 1000), "PooledItem"),
                maxSize: 3
            );
            
            poolRegistry.Register<TestItem>(testItemPool);
            
            // Act - レジストリからプールを取得してオブジェクトを操作
            var retrievedPool = poolRegistry.Get<TestItem>();
            var item1 = retrievedPool.Get();
            var item2 = retrievedPool.Get();
            
            retrievedPool.Return(item1);
            retrievedPool.Return(item2);
            
            var reusedItem = retrievedPool.Get();
            
            // Assert
            Assert.IsNotNull(retrievedPool, "レジストリからプールが取得できない");
            Assert.AreSame(testItemPool, retrievedPool, "レジストリから同じプールインスタンスが取得できない");
            Assert.IsNotNull(reusedItem, "プールから再利用アイテムが取得できない");
            
            var statistics = retrievedPool.GetStatistics();
            Assert.GreaterOrEqual(statistics.TotalReused, 1, "統合環境で再利用が発生していない");
        }

        /// <summary>
        /// 複数プール管理の統合テスト
        /// </summary>
        [Test]
        public void Integration_MultiplePoolsManagement_WorksCorrectly()
        {
            // Arrange - 複数種類のプールをレジストリで管理
            var poolRegistry = new TypeRegistry<object>();
            
            var testItemPool = new GenericObjectPool<TestItem>(
                () => new TestItem(),
                maxSize: 10
            );
            
            var anotherItemPool = new GenericObjectPool<AnotherTestItem>(
                () => new AnotherTestItem(),
                maxSize: 5
            );
            
            // Act - プールを型別に登録
            poolRegistry.Register<TestItem>(testItemPool);
            poolRegistry.Register<AnotherTestItem>(anotherItemPool);
            
            // プールから様々なオブジェクトを取得
            var testPool = poolRegistry.Get<TestItem>() as GenericObjectPool<TestItem>;
            var anotherPool = poolRegistry.Get<AnotherTestItem>() as GenericObjectPool<AnotherTestItem>;
            
            var testItem = testPool.Get();
            var anotherItem = anotherPool.Get();
            
            // Assert
            Assert.IsNotNull(testPool, "TestItemプールが取得できない");
            Assert.IsNotNull(anotherPool, "AnotherTestItemプールが取得できない");
            Assert.IsNotNull(testItem, "TestItemが取得できない");
            Assert.IsNotNull(anotherItem, "AnotherTestItemが取得できない");
            
            Assert.IsInstanceOf<TestItem>(testItem, "TestItemの型が正しくない");
            Assert.IsInstanceOf<AnotherTestItem>(anotherItem, "AnotherTestItemの型が正しくない");
            
            Assert.AreEqual(2, poolRegistry.Count, "レジストリに2つのプールが登録されているべき");
        }

        /// <summary>
        /// パフォーマンスとメモリ効率の統合テスト
        /// </summary>
        [Test]
        public void Integration_PerformanceAndMemoryEfficiency_Validated()
        {
            // Arrange
            var pool = new GenericObjectPool<TestItem>(
                () => new TestItem(),
                maxSize: 100
            );
            
            // Act - 大量のオブジェクト操作をシミュレート
            var items = new List<TestItem>();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // 取得フェーズ
            for (int i = 0; i < 50; i++)
            {
                items.Add(pool.Get());
            }
            
            // 返却フェーズ
            foreach (var item in items)
            {
                pool.Return(item);
            }
            
            // 再利用フェーズ
            for (int i = 0; i < 25; i++)
            {
                var reusedItem = pool.Get();
                Assert.IsNotNull(reusedItem, "再利用アイテムが取得できない");
            }
            
            stopwatch.Stop();
            
            // Assert - パフォーマンス検証
            Assert.Less(stopwatch.ElapsedMilliseconds, 100, "大量操作が100ms以内で完了するべき");
            
            var statistics = pool.GetStatistics();
            Assert.AreEqual(50, statistics.TotalCreated, "総作成数が正しくない");
            Assert.AreEqual(25, statistics.TotalReused, "再利用数が正しくない");
            Assert.Greater(statistics.ReuseRatio, 0.4f, "再利用率が40%以上であるべき");
            
            Debug.Log($"統合テスト完了: 実行時間={stopwatch.ElapsedMilliseconds}ms, " +
                     $"再利用率={statistics.ReuseRatio:P1}, " +
                     $"総作成数={statistics.TotalCreated}, " +
                     $"再利用数={statistics.TotalReused}");
        }

        #endregion
    }
}
