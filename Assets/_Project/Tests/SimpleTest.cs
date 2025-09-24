using NUnit.Framework;
using UnityEngine;

/// <summary>
/// 最小限のテストファイル - XML出力検証用
/// Unity Test Runnerの動作確認とXMLファイル生成テスト
/// </summary>
[TestFixture]
public class SimpleTest
{
    [Test]
    public void SimpleTest_ShouldPass()
    {
        // Arrange & Act & Assert
        Assert.IsTrue(true, "This simple test should always pass");
        Debug.Log("[SimpleTest] Basic test execution confirmed");
    }

    [Test]
    public void SimpleTest_Unity_ShouldHaveCorrectVersion()
    {
        // Arrange & Act
        string unityVersion = Application.unityVersion;
        
        // Assert
        Assert.IsNotNull(unityVersion, "Unity version should not be null");
        Assert.IsTrue(unityVersion.Contains("6000"), "Unity version should be 6000.x");
        Debug.Log($"[SimpleTest] Unity version: {unityVersion}");
    }

    [Test]
    public void SimpleTest_Math_ShouldCalculateCorrectly()
    {
        // Arrange
        int a = 2;
        int b = 3;
        int expected = 5;
        
        // Act
        int result = a + b;
        
        // Assert
        Assert.AreEqual(expected, result, "Basic math should work correctly");
        Debug.Log($"[SimpleTest] Math test: {a} + {b} = {result}");
    }
}