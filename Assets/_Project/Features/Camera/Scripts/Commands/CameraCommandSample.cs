using UnityEngine;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Features.Camera.Commands.Definitions;
using asterivo.Unity60.Camera.States;

namespace asterivo.Unity60.Features.Camera.Commands
{
    /// <summary>
    /// カメラコマンドの使用例とサンプルコード
    /// このクラスは実際のプロジェクトでの使用方法を示しています
    /// </summary>
    public class CameraCommandSample : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CameraStateMachine cameraStateMachine;
        [SerializeField] private CommandInvoker commandInvoker;

        private void Start()
        {
            // 使用例をコンソールに出力
            PrintUsageExamples();
            
            // 5秒後にサンプル実行（デモ用）
            Invoke(nameof(RunSampleCommands), 5f);
        }

        /// <summary>
        /// 使用例の説明をコンソールに出力
        /// </summary>
        private void PrintUsageExamples()
        {
            Debug.Log("=== カメラ肩越しアングル切り替えコマンド 使用例 ===");
            Debug.Log("1. 基本的な使用方法:");
            Debug.Log("   - Q キー: トグル切り替え（右肩→左肩→中央→右肩...）");
            Debug.Log("   - E キー: 右肩越しに固定");
            Debug.Log("   - R キー: 左肩越しに固定");
            Debug.Log("   - T キー: 中央に固定");
            Debug.Log("");
            Debug.Log("2. プログラムからの呼び出し例:");
            Debug.Log("   cameraCommandManager.ToggleShoulderAngle();");
            Debug.Log("   cameraCommandManager.SwitchToRightShoulder();");
            Debug.Log("");
            Debug.Log("3. カスタムオフセットでの使用例:");
            Debug.Log("   SwitchToCustomAngle(ShoulderAngle.RightShoulder, new Vector3(1.2f, 1.8f, 0f));");
        }

        /// <summary>
        /// サンプルコマンドを実行（デモ用）
        /// </summary>
        private void RunSampleCommands()
        {
            StartCoroutine(SampleCommandSequence());
        }

        /// <summary>
        /// サンプルコマンドの連続実行
        /// </summary>
        private System.Collections.IEnumerator SampleCommandSequence()
        {
            if (cameraStateMachine == null || commandInvoker == null)
            {
                Debug.LogWarning("CameraStateMachine または CommandInvoker が設定されていません");
                yield break;
            }

            Debug.Log("=== カメラコマンドサンプル実行開始 ===");
            
            // 1. 右肩越しに切り替え
            yield return new WaitForSeconds(2f);
            ExecuteRightShoulderCommand();
            
            // 2. 左肩越しに切り替え
            yield return new WaitForSeconds(2f);
            ExecuteLeftShoulderCommand();
            
            // 3. 中央に戻す
            yield return new WaitForSeconds(2f);
            ExecuteCenterCommand();
            
            // 4. カスタムオフセットでの右肩越し
            yield return new WaitForSeconds(2f);
            ExecuteCustomRightShoulderCommand();
            
            Debug.Log("=== カメラコマンドサンプル実行完了 ===");
        }

        /// <summary>
        /// 右肩越しコマンドの実行例
        /// </summary>
        public void ExecuteRightShoulderCommand()
        {
            Debug.Log("右肩越しアングルに切り替え中...");
            
            // Definition を作成
            var definition = new CameraShoulderSwitchCommandDefinition(
                CameraShoulderSwitchCommandDefinition.ShoulderAngle.RightShoulder);
            
            // 実行可能かチェック
            if (definition.CanExecute(cameraStateMachine))
            {
                // コマンドを生成して実行
                var command = definition.CreateCommand(cameraStateMachine);
                commandInvoker.ExecuteCommand(command);
            }
            else
            {
                Debug.LogWarning("右肩越しコマンドを実行できません（カメラ状態を確認）");
            }
        }

        /// <summary>
        /// 左肩越しコマンドの実行例
        /// </summary>
        public void ExecuteLeftShoulderCommand()
        {
            Debug.Log("左肩越しアングルに切り替え中...");
            
            var definition = new CameraShoulderSwitchCommandDefinition(
                CameraShoulderSwitchCommandDefinition.ShoulderAngle.LeftShoulder);
            
            if (definition.CanExecute(cameraStateMachine))
            {
                var command = definition.CreateCommand(cameraStateMachine);
                commandInvoker.ExecuteCommand(command);
            }
        }

        /// <summary>
        /// 中央アングルコマンドの実行例
        /// </summary>
        public void ExecuteCenterCommand()
        {
            Debug.Log("中央アングルに切り替え中...");
            
            var definition = new CameraShoulderSwitchCommandDefinition(
                CameraShoulderSwitchCommandDefinition.ShoulderAngle.Center);
            
            if (definition.CanExecute(cameraStateMachine))
            {
                var command = definition.CreateCommand(cameraStateMachine);
                commandInvoker.ExecuteCommand(command);
            }
        }

        /// <summary>
        /// カスタムオフセット付き右肩越しコマンドの実行例
        /// </summary>
        public void ExecuteCustomRightShoulderCommand()
        {
            Debug.Log("カスタムオフセットで右肩越しアングルに切り替え中...");
            
            // カスタムオフセットを設定
            var definition = new CameraShoulderSwitchCommandDefinition(
                CameraShoulderSwitchCommandDefinition.ShoulderAngle.RightShoulder);
            
            definition.rightShoulderOffset = new Vector3(1.2f, 1.8f, 0.2f); // より外側で高い位置
            definition.transitionSpeed = 15f; // より高速な遷移
            
            if (definition.CanExecute(cameraStateMachine))
            {
                var command = definition.CreateCommand(cameraStateMachine);
                commandInvoker.ExecuteCommand(command);
            }
        }

        /// <summary>
        /// トグルコマンドの実行例
        /// </summary>
        public void ExecuteToggleCommand()
        {
            Debug.Log("肩越しアングルをトグル切り替え中...");
            
            var definition = new CameraShoulderSwitchCommandDefinition(
                CameraShoulderSwitchCommandDefinition.ShoulderAngle.Toggle);
            
            if (definition.CanExecute(cameraStateMachine))
            {
                var command = definition.CreateCommand(cameraStateMachine);
                commandInvoker.ExecuteCommand(command);
            }
        }

        /// <summary>
        /// UIボタンから呼び出し可能な公開メソッド群
        /// </summary>
        #region UI Button Methods

        [System.Obsolete("Use ExecuteRightShoulderCommand() instead")]
        public void OnRightShoulderButtonClicked()
        {
            ExecuteRightShoulderCommand();
        }

        [System.Obsolete("Use ExecuteLeftShoulderCommand() instead")]
        public void OnLeftShoulderButtonClicked()
        {
            ExecuteLeftShoulderCommand();
        }

        [System.Obsolete("Use ExecuteCenterCommand() instead")]
        public void OnCenterButtonClicked()
        {
            ExecuteCenterCommand();
        }

        [System.Obsolete("Use ExecuteToggleCommand() instead")]
        public void OnToggleButtonClicked()
        {
            ExecuteToggleCommand();
        }

        #endregion

        /// <summary>
        /// エディタでのテスト用コンテキストメニュー
        /// </summary>
        #region Context Menu Methods

        [ContextMenu("Sample: Right Shoulder")]
        private void TestRightShoulder()
        {
            ExecuteRightShoulderCommand();
        }

        [ContextMenu("Sample: Left Shoulder")]
        private void TestLeftShoulder()
        {
            ExecuteLeftShoulderCommand();
        }

        [ContextMenu("Sample: Center")]
        private void TestCenter()
        {
            ExecuteCenterCommand();
        }

        [ContextMenu("Sample: Toggle")]
        private void TestToggle()
        {
            ExecuteToggleCommand();
        }

        [ContextMenu("Sample: Custom Right")]
        private void TestCustomRight()
        {
            ExecuteCustomRightShoulderCommand();
        }

        #endregion
    }
}