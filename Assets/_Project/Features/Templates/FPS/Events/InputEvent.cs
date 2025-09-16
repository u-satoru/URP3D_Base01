using System;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Events
{
    /// <summary>
    /// 入力関連イベント
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// Core層のGameEventシステム統合
    /// </summary>

    [CreateAssetMenu(fileName = "InputStateEvent", menuName = "FPS Template/Events/Input State Event")]
    public class InputStateEvent : ScriptableObject
    {
        [SerializeField] private asterivo.Unity60.Core.Events.GameEvent<InputStateData> _inputStateChangedEvent;

        /// <summary>
        /// 入力状態変更イベント発行
        /// </summary>
        public void RaiseInputStateChanged(InputStateData data)
        {
            _inputStateChangedEvent?.Raise(data);
            Debug.Log($"[InputEvent] Input state changed: {data.InputType} - {data.IsEnabled}");
        }

        /// <summary>
        /// 入力状態変更リスナー登録
        /// </summary>
        public void RegisterListener(Action<InputStateData> callback)
        {
            _inputStateChangedEvent?.AddListener(callback);
        }

        /// <summary>
        /// 入力状態変更リスナー解除
        /// </summary>
        public void UnregisterListener(Action<InputStateData> callback)
        {
            _inputStateChangedEvent?.RemoveListener(callback);
        }
    }

    [CreateAssetMenu(fileName = "ControllerChangedEvent", menuName = "FPS Template/Events/Controller Changed Event")]
    public class ControllerChangedEvent : ScriptableObject
    {
        [SerializeField] private asterivo.Unity60.Core.Events.GameEvent<ControllerChangeData> _controllerChangedEvent;

        /// <summary>
        /// コントローラー変更イベント発行
        /// </summary>
        public void RaiseControllerChanged(ControllerChangeData data)
        {
            _controllerChangedEvent?.Raise(data);
            Debug.Log($"[InputEvent] Controller changed: {data.PreviousDevice} → {data.NewDevice}");
        }

        /// <summary>
        /// コントローラー変更リスナー登録
        /// </summary>
        public void RegisterListener(Action<ControllerChangeData> callback)
        {
            _controllerChangedEvent?.AddListener(callback);
        }

        /// <summary>
        /// コントローラー変更リスナー解除
        /// </summary>
        public void UnregisterListener(Action<ControllerChangeData> callback)
        {
            _controllerChangedEvent?.RemoveListener(callback);
        }
    }

    [CreateAssetMenu(fileName = "PlayerActionEvent", menuName = "FPS Template/Events/Player Action Event")]
    public class PlayerActionEvent : ScriptableObject
    {
        [SerializeField] private asterivo.Unity60.Core.Events.GameEvent<PlayerActionData> _playerActionEvent;

        /// <summary>
        /// プレイヤーアクションイベント発行
        /// </summary>
        public void RaisePlayerAction(PlayerActionData data)
        {
            _playerActionEvent?.Raise(data);
            Debug.Log($"[InputEvent] Player action: {data.ActionType} at {data.Position}");
        }

        /// <summary>
        /// プレイヤーアクションリスナー登録
        /// </summary>
        public void RegisterListener(Action<PlayerActionData> callback)
        {
            _playerActionEvent?.AddListener(callback);
        }

        /// <summary>
        /// プレイヤーアクションリスナー解除
        /// </summary>
        public void UnregisterListener(Action<PlayerActionData> callback)
        {
            _playerActionEvent?.RemoveListener(callback);
        }
    }

    /// <summary>
    /// 入力状態変更イベントデータ
    /// </summary>
    [System.Serializable]
    public class InputStateData
    {
        public InputType InputType;
        public bool IsEnabled;
        public float Sensitivity;
        public InputDeviceType DeviceType;
        public string DeviceName;
        public GameObject InputSource;

        public InputStateData(InputType inputType, bool isEnabled, float sensitivity = 1f,
                             InputDeviceType deviceType = InputDeviceType.KeyboardMouse,
                             string deviceName = "", GameObject inputSource = null)
        {
            InputType = inputType;
            IsEnabled = isEnabled;
            Sensitivity = sensitivity;
            DeviceType = deviceType;
            DeviceName = deviceName;
            InputSource = inputSource;
        }
    }

    /// <summary>
    /// コントローラー変更イベントデータ
    /// </summary>
    [System.Serializable]
    public class ControllerChangeData
    {
        public InputDeviceType PreviousDevice;
        public InputDeviceType NewDevice;
        public string PreviousDeviceName;
        public string NewDeviceName;
        public bool IsAutoSwitched;
        public GameObject Player;

        public ControllerChangeData(InputDeviceType previousDevice, InputDeviceType newDevice,
                                   string previousDeviceName = "", string newDeviceName = "",
                                   bool isAutoSwitched = false, GameObject player = null)
        {
            PreviousDevice = previousDevice;
            NewDevice = newDevice;
            PreviousDeviceName = previousDeviceName;
            NewDeviceName = newDeviceName;
            IsAutoSwitched = isAutoSwitched;
            Player = player;
        }
    }

    /// <summary>
    /// プレイヤーアクションイベントデータ
    /// </summary>
    [System.Serializable]
    public class PlayerActionData
    {
        public PlayerActionType ActionType;
        public Vector3 Position;
        public Vector3 Direction;
        public float Intensity;
        public GameObject Player;
        public GameObject Target;
        public bool IsPressed;
        public float Duration;

        public PlayerActionData(PlayerActionType actionType, Vector3 position, Vector3 direction = default,
                               float intensity = 1f, GameObject player = null, GameObject target = null,
                               bool isPressed = true, float duration = 0f)
        {
            ActionType = actionType;
            Position = position;
            Direction = direction;
            Intensity = intensity;
            Player = player;
            Target = target;
            IsPressed = isPressed;
            Duration = duration;
        }
    }

    /// <summary>
    /// 入力タイプ
    /// </summary>
    public enum InputType
    {
        Movement,
        Look,
        Fire,
        Reload,
        Jump,
        Crouch,
        Sprint,
        Aim,
        Interaction,
        Pause,
        WeaponSwitch
    }

    /// <summary>
    /// プレイヤーアクションタイプ
    /// </summary>
    public enum PlayerActionType
    {
        Move,
        Jump,
        Crouch,
        Sprint,
        Fire,
        Reload,
        Aim,
        Look,
        Interact,
        Pause,
        WeaponNext,
        WeaponPrevious
    }
}