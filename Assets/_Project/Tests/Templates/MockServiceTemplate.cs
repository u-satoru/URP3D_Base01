using UnityEngine;
using asterivo.Unity60.Core.Audio.Interfaces;

namespace asterivo.Unity60.Tests.Templates
{
    /// <summary>
    /// モックサービステンプレート
    /// テスト用のモックオブジェクト作成テンプレート
    /// 
    /// 使用方法:
    /// 1. このファイルをコピー
    /// 2. クラス名と実装するインターフェースを変更
    /// 3. 必要なモック動作を実装
    /// 4. テストで必要な検証ポイントを追加
    /// </summary>
    public class MockServiceTemplate : IAudioService
    {
        #region Mock State Management

        /// <summary>
        /// モックの呼び出し回数を記録
        /// </summary>
        public int CallCount { get; private set; }

        /// <summary>
        /// 最後に呼び出されたメソッド名
        /// </summary>
        public string LastCalledMethod { get; private set; }

        /// <summary>
        /// 最後に渡された引数
        /// </summary>
        public object[] LastArguments { get; private set; }

        /// <summary>
        /// モックの戻り値を設定
        /// </summary>
        public object MockReturnValue { get; set; }

        /// <summary>
        /// 例外をスローするかどうか
        /// </summary>
        public bool ShouldThrowException { get; set; }

        /// <summary>
        /// スローする例外
        /// </summary>
        public System.Exception ExceptionToThrow { get; set; }

        /// <summary>
        /// 遅延実行のシミュレーション（秒）
        /// </summary>
        public float SimulatedDelay { get; set; }

        #endregion

        #region IAudioService Implementation

        public void PlaySound(string soundId, Vector3 position = default, float volume = 1f)
        {
            RecordMethodCall(nameof(PlaySound), soundId, position, volume);
            
            if (ShouldThrowException)
            {
                throw ExceptionToThrow ?? new System.Exception("Mock exception");
            }

            SimulateDelay();
        }

        public void StopSound(string soundId)
        {
            RecordMethodCall(nameof(StopSound), soundId);
            
            if (ShouldThrowException)
            {
                throw ExceptionToThrow ?? new System.Exception("Mock exception");
            }

            SimulateDelay();
        }

        public void StopAllSounds()
        {
            RecordMethodCall(nameof(StopAllSounds));
            
            if (ShouldThrowException)
            {
                throw ExceptionToThrow ?? new System.Exception("Mock exception");
            }

            SimulateDelay();
        }

        public float GetMasterVolume()
        {
            RecordMethodCall(nameof(GetMasterVolume));
            
            if (ShouldThrowException)
            {
                throw ExceptionToThrow ?? new System.Exception("Mock exception");
            }

            SimulateDelay();
            return MockReturnValue is float floatValue ? floatValue : 1f;
        }

        public void SetMasterVolume(float volume)
        {
            RecordMethodCall(nameof(SetMasterVolume), volume);
            
            if (ShouldThrowException)
            {
                throw ExceptionToThrow ?? new System.Exception("Mock exception");
            }

            SimulateDelay();
        }

        public float GetBGMVolume()
        {
            RecordMethodCall(nameof(GetBGMVolume));
            
            if (ShouldThrowException)
            {
                throw ExceptionToThrow ?? new System.Exception("Mock exception");
            }

            SimulateDelay();
            return MockReturnValue is float floatValue ? floatValue : 1f;
        }

        public float GetAmbientVolume()
        {
            RecordMethodCall(nameof(GetAmbientVolume));
            
            if (ShouldThrowException)
            {
                throw ExceptionToThrow ?? new System.Exception("Mock exception");
            }

            SimulateDelay();
            return MockReturnValue is float floatValue ? floatValue : 1f;
        }

        public float GetEffectVolume()
        {
            RecordMethodCall(nameof(GetEffectVolume));
            
            if (ShouldThrowException)
            {
                throw ExceptionToThrow ?? new System.Exception("Mock exception");
            }

            SimulateDelay();
            return MockReturnValue is float floatValue ? floatValue : 1f;
        }

        public void SetCategoryVolume(string category, float volume)
        {
            RecordMethodCall(nameof(SetCategoryVolume), category, volume);
            
            if (ShouldThrowException)
            {
                throw ExceptionToThrow ?? new System.Exception("Mock exception");
            }

            SimulateDelay();
        }

        public bool IsPlaying(string soundId)
        {
            RecordMethodCall(nameof(IsPlaying), soundId);
            
            if (ShouldThrowException)
            {
                throw ExceptionToThrow ?? new System.Exception("Mock exception");
            }

            SimulateDelay();
            
            // モックの戻り値を返す
            return MockReturnValue is bool boolValue ? boolValue : false;
        }

        public void Pause()
        {
            RecordMethodCall(nameof(Pause));
            
            if (ShouldThrowException)
            {
                throw ExceptionToThrow ?? new System.Exception("Mock exception");
            }

            SimulateDelay();
        }

        public void Resume()
        {
            RecordMethodCall(nameof(Resume));
            
            if (ShouldThrowException)
            {
                throw ExceptionToThrow ?? new System.Exception("Mock exception");
            }

            SimulateDelay();
        }

        #endregion

        #region Mock Helper Methods

        /// <summary>
        /// メソッド呼び出しを記録
        /// </summary>
        private void RecordMethodCall(string methodName, params object[] arguments)
        {
            CallCount++;
            LastCalledMethod = methodName;
            LastArguments = arguments;
            
            // デバッグ用ログ（必要に応じて）
            Debug.Log($"[MockService] {methodName} called with {arguments?.Length ?? 0} arguments");
        }

        /// <summary>
        /// 遅延をシミュレート
        /// </summary>
        private void SimulateDelay()
        {
            if (SimulatedDelay > 0)
            {
                // 実際のアプリケーションでは async/await を使用
                // テストでは同期的に処理することが多い
                System.Threading.Thread.Sleep((int)(SimulatedDelay * 1000));
            }
        }

        /// <summary>
        /// モック状態のリセット
        /// </summary>
        public void Reset()
        {
            CallCount = 0;
            LastCalledMethod = null;
            LastArguments = null;
            MockReturnValue = null;
            ShouldThrowException = false;
            ExceptionToThrow = null;
            SimulatedDelay = 0f;
        }

        /// <summary>
        /// 特定のメソッドが呼び出されたかを確認
        /// </summary>
        public bool WasMethodCalled(string methodName)
        {
            return LastCalledMethod == methodName;
        }

        /// <summary>
        /// 特定の引数でメソッドが呼び出されたかを確認
        /// </summary>
        public bool WasCalledWith(string methodName, params object[] expectedArguments)
        {
            if (LastCalledMethod != methodName)
                return false;

            if (LastArguments == null && expectedArguments == null)
                return true;

            if (LastArguments == null || expectedArguments == null)
                return false;

            if (LastArguments.Length != expectedArguments.Length)
                return false;

            for (int i = 0; i < LastArguments.Length; i++)
            {
                if (!object.Equals(LastArguments[i], expectedArguments[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 指定した回数メソッドが呼び出されたかを確認
        /// </summary>
        public bool WasCalledTimes(int expectedCallCount)
        {
            return CallCount == expectedCallCount;
        }

        #endregion

        #region Builder Pattern for Mock Setup

        /// <summary>
        /// モックビルダー（流暢なインターフェース）
        /// </summary>
        public class Builder
        {
            private readonly MockServiceTemplate mock;

            public Builder()
            {
                mock = new MockServiceTemplate();
            }

            public Builder WithReturnValue(object returnValue)
            {
                mock.MockReturnValue = returnValue;
                return this;
            }

            public Builder WithException(System.Exception exception)
            {
                mock.ShouldThrowException = true;
                mock.ExceptionToThrow = exception;
                return this;
            }

            public Builder WithDelay(float delay)
            {
                mock.SimulatedDelay = delay;
                return this;
            }

            public MockServiceTemplate Build()
            {
                return mock;
            }
        }

        /// <summary>
        /// ビルダーインスタンスの取得
        /// </summary>
        public static Builder Create()
        {
            return new Builder();
        }

        #endregion
    }

    #region Mock Usage Examples

    /// <summary>
    /// モック使用例（コメントアウト状態）
    /// </summary>
    /*
    public class MockUsageExamples
    {
        public void BasicMockUsage()
        {
            // 基本的な使用方法
            var mockService = new MockServiceTemplate();
            mockService.MockReturnValue = true;
            
            // テスト実行
            var result = mockService.IsPlaying("test-sound");
            
            // 検証
            Assert.IsTrue(mockService.WasMethodCalled("IsPlaying"));
            Assert.IsTrue(mockService.WasCalledWith("IsPlaying", "test-sound"));
            Assert.AreEqual(1, mockService.CallCount);
        }

        public void BuilderPatternUsage()
        {
            // ビルダーパターンの使用
            var mockService = MockServiceTemplate.Create()
                .WithReturnValue(true)
                .WithDelay(0.1f)
                .Build();
                
            // テスト実行...
        }
    }
    */

    #endregion
}