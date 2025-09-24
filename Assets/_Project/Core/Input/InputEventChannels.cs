using UnityEngine;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Core.Input
{
    /// <summary>
    /// 入力関連のGameEventを一元管理するScriptableObject
    /// InputServiceや他のリスナーが参照しやすくするための中央管理システム
    /// 
    /// 使用方法:
    /// 1. Create Asset Menu から作成
    /// 2. 各GameEventアセットを個別に作成してこのオブジェクトに割り当て
    /// 3. InputServiceのInspectorでこのアセットを参照
    /// 4. 他のコンポーネントからも同じアセットを参照してイベントをリッスン
    /// </summary>
    [CreateAssetMenu(fileName = "InputEventChannels", menuName = "asterivo.Unity60/Input/Input Event Channels")]
    public class InputEventChannels : ScriptableObject
    {
        [Header("Movement Input Events")]
        [Tooltip("プレイヤーの移動入力イベント (WASD, 左スティック等)")]
        public GameEvent<Vector2> OnMoveInput;

        [Header("Action Input Events")]
        [Tooltip("ジャンプ入力が押された時のイベント")]
        public GameEvent OnJumpInputPressed;

        [Tooltip("インタラクション入力が押された時のイベント (E, F等)")]
        public GameEvent OnInteractInputPressed;

        [Tooltip("攻撃/射撃入力が押された時のイベント")]
        public GameEvent OnAttackInputPressed;

        [Header("Secondary Action Events")]
        [Tooltip("リロード入力が押された時のイベント")]
        public GameEvent OnReloadInputPressed;

        [Tooltip("しゃがみ入力の状態変更イベント")]
        public GameEvent<bool> OnCrouchInputChanged;

        [Tooltip("走行入力の状態変更イベント")]
        public GameEvent<bool> OnRunInputChanged;

        [Header("Camera Input Events")]
        [Tooltip("カメラのルック入力イベント (マウス移動, 右スティック等)")]
        public GameEvent<Vector2> OnLookInput;

        [Tooltip("視点切り替え入力が押された時のイベント")]
        public GameEvent OnToggleViewPressed;

        [Header("UI Input Events")]
        [Tooltip("メニュー開閉入力が押された時のイベント (ESC, Start等)")]
        public GameEvent OnMenuInputPressed;

        [Tooltip("インベントリ開閉入力が押された時のイベント")]
        public GameEvent OnInventoryInputPressed;

        #if UNITY_EDITOR
        [Header("Editor Debug")]
        [Tooltip("エディタでの動作確認用")]
        [SerializeField] private bool showDebugInfo = false;

        /// <summary>
        /// エディタでの動作確認用: 各イベントの登録状況を表示
        /// </summary>
        [ContextMenu("Debug: Show Event Status")]
        private void ShowEventStatus()
        {
            if (!showDebugInfo) return;

            Debug.Log("=== Input Event Channels Status ===");
            Debug.Log($"OnMoveInput: {(OnMoveInput != null ? $"Assigned ({OnMoveInput.GetListenerCount()} listeners)" : "Not Assigned")}", OnMoveInput);
            Debug.Log($"OnJumpInputPressed: {(OnJumpInputPressed != null ? $"Assigned ({OnJumpInputPressed.GetListenerCount()} listeners)" : "Not Assigned")}", OnJumpInputPressed);
            Debug.Log($"OnInteractInputPressed: {(OnInteractInputPressed != null ? $"Assigned ({OnInteractInputPressed.GetListenerCount()} listeners)" : "Not Assigned")}", OnInteractInputPressed);
            Debug.Log($"OnAttackInputPressed: {(OnAttackInputPressed != null ? $"Assigned ({OnAttackInputPressed.GetListenerCount()} listeners)" : "Not Assigned")}", OnAttackInputPressed);
            Debug.Log($"OnLookInput: {(OnLookInput != null ? $"Assigned ({OnLookInput.GetListenerCount()} listeners)" : "Not Assigned")}", OnLookInput);
        }

        /// <summary>
        /// エディタでの動作確認用: 移動入力イベントをテスト発行
        /// </summary>
        [ContextMenu("Debug: Test Move Input")]
        private void TestMoveInput()
        {
            if (OnMoveInput != null)
            {
                OnMoveInput.Raise(Vector2.one);
                Debug.Log("Test Move Input raised with Vector2.one", this);
            }
            else
            {
                Debug.LogWarning("OnMoveInput is not assigned", this);
            }
        }

        /// <summary>
        /// エディタでの動作確認用: ジャンプ入力イベントをテスト発行
        /// </summary>
        [ContextMenu("Debug: Test Jump Input")]
        private void TestJumpInput()
        {
            if (OnJumpInputPressed != null)
            {
                OnJumpInputPressed.Raise();
                Debug.Log("Test Jump Input raised", this);
            }
            else
            {
                Debug.LogWarning("OnJumpInputPressed is not assigned", this);
            }
        }
        #endif

        /// <summary>
        /// すべての必須イベントが割り当てられているかチェック
        /// </summary>
        /// <returns>true: 全て割り当て済み, false: 未割り当てあり</returns>
        public bool ValidateEventChannels()
        {
            bool isValid = true;

            if (OnMoveInput == null)
            {
                Debug.LogWarning("OnMoveInput is not assigned", this);
                isValid = false;
            }

            if (OnJumpInputPressed == null)
            {
                Debug.LogWarning("OnJumpInputPressed is not assigned", this);
                isValid = false;
            }

            if (OnInteractInputPressed == null)
            {
                Debug.LogWarning("OnInteractInputPressed is not assigned", this);
                isValid = false;
            }

            if (OnAttackInputPressed == null)
            {
                Debug.LogWarning("OnAttackInputPressed is not assigned", this);
                isValid = false;
            }

            if (OnLookInput == null)
            {
                Debug.LogWarning("OnLookInput is not assigned", this);
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// 入力イベントの総リスナー数を取得 (デバッグ用)
        /// </summary>
        /// <returns>総リスナー数</returns>
        public int GetTotalListenerCount()
        {
            int total = 0;
            
            if (OnMoveInput != null) total += OnMoveInput.GetListenerCount();
            if (OnJumpInputPressed != null) total += OnJumpInputPressed.GetListenerCount();
            if (OnInteractInputPressed != null) total += OnInteractInputPressed.GetListenerCount();
            if (OnAttackInputPressed != null) total += OnAttackInputPressed.GetListenerCount();
            if (OnLookInput != null) total += OnLookInput.GetListenerCount();
            if (OnReloadInputPressed != null) total += OnReloadInputPressed.GetListenerCount();
            if (OnCrouchInputChanged != null) total += OnCrouchInputChanged.GetListenerCount();
            if (OnRunInputChanged != null) total += OnRunInputChanged.GetListenerCount();
            if (OnToggleViewPressed != null) total += OnToggleViewPressed.GetListenerCount();
            if (OnMenuInputPressed != null) total += OnMenuInputPressed.GetListenerCount();
            if (OnInventoryInputPressed != null) total += OnInventoryInputPressed.GetListenerCount();

            return total;
        }
    }
}