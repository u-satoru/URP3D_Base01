using System;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Events
{
    /// <summary>
    /// UI関連イベント
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// Core層のGameEventシステム統合
    /// </summary>

    [CreateAssetMenu(fileName = "UIStateEvent", menuName = "FPS Template/Events/UI State Event")]
    public class UIStateEvent : ScriptableObject
    {
        [SerializeField] private asterivo.Unity60.Core.Events.GameEvent<UIStateData> _uiStateChangedEvent;

        /// <summary>
        /// UI状態変更イベント発行
        /// </summary>
        public void RaiseUIStateChanged(UIStateData data)
        {
            _uiStateChangedEvent?.Raise(data);
            Debug.Log($"[UIEvent] UI state changed: {data.UIType} - {(data.IsVisible ? "Show" : "Hide")}");
        }

        /// <summary>
        /// UI状態変更リスナー登録
        /// </summary>
        public void RegisterListener(Action<UIStateData> callback)
        {
            _uiStateChangedEvent?.AddListener(callback);
        }

        /// <summary>
        /// UI状態変更リスナー解除
        /// </summary>
        public void UnregisterListener(Action<UIStateData> callback)
        {
            _uiStateChangedEvent?.RemoveListener(callback);
        }
    }

    [CreateAssetMenu(fileName = "HUDUpdateEvent", menuName = "FPS Template/Events/HUD Update Event")]
    public class HUDUpdateEvent : ScriptableObject
    {
        [SerializeField] private asterivo.Unity60.Core.Events.GameEvent<HUDUpdateData> _hudUpdateEvent;

        /// <summary>
        /// HUD更新イベント発行
        /// </summary>
        public void RaiseHUDUpdate(HUDUpdateData data)
        {
            _hudUpdateEvent?.Raise(data);
            Debug.Log($"[UIEvent] HUD update: {data.ElementType} - {data.Value}");
        }

        /// <summary>
        /// HUD更新リスナー登録
        /// </summary>
        public void RegisterListener(Action<HUDUpdateData> callback)
        {
            _hudUpdateEvent?.AddListener(callback);
        }

        /// <summary>
        /// HUD更新リスナー解除
        /// </summary>
        public void UnregisterListener(Action<HUDUpdateData> callback)
        {
            _hudUpdateEvent?.RemoveListener(callback);
        }
    }

    [CreateAssetMenu(fileName = "MenuNavigationEvent", menuName = "FPS Template/Events/Menu Navigation Event")]
    public class MenuNavigationEvent : ScriptableObject
    {
        [SerializeField] private asterivo.Unity60.Core.Events.GameEvent<MenuNavigationData> _menuNavigationEvent;

        /// <summary>
        /// メニューナビゲーションイベント発行
        /// </summary>
        public void RaiseMenuNavigation(MenuNavigationData data)
        {
            _menuNavigationEvent?.Raise(data);
            Debug.Log($"[UIEvent] Menu navigation: {data.FromMenu} → {data.ToMenu}");
        }

        /// <summary>
        /// メニューナビゲーションリスナー登録
        /// </summary>
        public void RegisterListener(Action<MenuNavigationData> callback)
        {
            _menuNavigationEvent?.AddListener(callback);
        }

        /// <summary>
        /// メニューナビゲーションリスナー解除
        /// </summary>
        public void UnregisterListener(Action<MenuNavigationData> callback)
        {
            _menuNavigationEvent?.RemoveListener(callback);
        }
    }

    [CreateAssetMenu(fileName = "NotificationEvent", menuName = "FPS Template/Events/Notification Event")]
    public class NotificationEvent : ScriptableObject
    {
        [SerializeField] private asterivo.Unity60.Core.Events.GameEvent<NotificationData> _notificationEvent;

        /// <summary>
        /// 通知イベント発行
        /// </summary>
        public void RaiseNotification(NotificationData data)
        {
            _notificationEvent?.Raise(data);
            Debug.Log($"[UIEvent] Notification: {data.Type} - {data.Message}");
        }

        /// <summary>
        /// 通知リスナー登録
        /// </summary>
        public void RegisterListener(Action<NotificationData> callback)
        {
            _notificationEvent?.AddListener(callback);
        }

        /// <summary>
        /// 通知リスナー解除
        /// </summary>
        public void UnregisterListener(Action<NotificationData> callback)
        {
            _notificationEvent?.RemoveListener(callback);
        }
    }

    /// <summary>
    /// UI状態変更イベントデータ
    /// </summary>
    [System.Serializable]
    public class UIStateData
    {
        public UIType UIType;
        public bool IsVisible;
        public float TransitionDuration;
        public Vector2 Position;
        public GameObject UIElement;
        public string CustomData;

        public UIStateData(UIType uiType, bool isVisible, float transitionDuration = 0.3f,
                          Vector2 position = default, GameObject uiElement = null, string customData = "")
        {
            UIType = uiType;
            IsVisible = isVisible;
            TransitionDuration = transitionDuration;
            Position = position;
            UIElement = uiElement;
            CustomData = customData;
        }
    }

    /// <summary>
    /// HUD更新イベントデータ
    /// </summary>
    [System.Serializable]
    public class HUDUpdateData
    {
        public HUDElementType ElementType;
        public float Value;
        public float MaxValue;
        public string TextValue;
        public Color Color;
        public bool IsAnimated;
        public float AnimationDuration;

        public HUDUpdateData(HUDElementType elementType, float value, float maxValue = 100f,
                            string textValue = "", Color color = default, bool isAnimated = true,
                            float animationDuration = 0.5f)
        {
            ElementType = elementType;
            Value = value;
            MaxValue = maxValue;
            TextValue = textValue;
            Color = color == default ? Color.white : color;
            IsAnimated = isAnimated;
            AnimationDuration = animationDuration;
        }
    }

    /// <summary>
    /// メニューナビゲーションイベントデータ
    /// </summary>
    [System.Serializable]
    public class MenuNavigationData
    {
        public MenuType FromMenu;
        public MenuType ToMenu;
        public NavigationType NavigationType;
        public bool PlaySound;
        public string TransitionEffect;
        public GameObject MenuContext;

        public MenuNavigationData(MenuType fromMenu, MenuType toMenu, NavigationType navigationType = NavigationType.Push,
                                 bool playSound = true, string transitionEffect = "Fade", GameObject menuContext = null)
        {
            FromMenu = fromMenu;
            ToMenu = toMenu;
            NavigationType = navigationType;
            PlaySound = playSound;
            TransitionEffect = transitionEffect;
            MenuContext = menuContext;
        }
    }

    /// <summary>
    /// 通知イベントデータ
    /// </summary>
    [System.Serializable]
    public class NotificationData
    {
        public NotificationType Type;
        public string Message;
        public string Title;
        public float Duration;
        public Color BackgroundColor;
        public Sprite Icon;
        public bool PlaySound;
        public string SoundName;

        public NotificationData(NotificationType type, string message, string title = "",
                               float duration = 3f, Color backgroundColor = default,
                               Sprite icon = null, bool playSound = true, string soundName = "")
        {
            Type = type;
            Message = message;
            Title = title;
            Duration = duration;
            BackgroundColor = backgroundColor == default ? Color.blue : backgroundColor;
            Icon = icon;
            PlaySound = playSound;
            SoundName = soundName;
        }
    }

    /// <summary>
    /// UIタイプ
    /// </summary>
    public enum UIType
    {
        MainHUD,
        PauseMenu,
        SettingsMenu,
        InventoryMenu,
        WeaponSelectMenu,
        ScoreBoard,
        LoadingScreen,
        GameOverScreen,
        Crosshair,
        Minimap,
        ObjectivePanel,
        InteractionPrompt
    }

    /// <summary>
    /// HUD要素タイプ
    /// </summary>
    public enum HUDElementType
    {
        Health,
        Shield,
        Ammo,
        ReserveAmmo,
        WeaponName,
        Score,
        Timer,
        Objectives,
        InteractionHint,
        DamageIndicator,
        XPBar,
        LevelDisplay
    }

    /// <summary>
    /// メニュータイプ
    /// </summary>
    public enum MenuType
    {
        None,
        MainMenu,
        PauseMenu,
        SettingsMenu,
        GraphicsSettings,
        AudioSettings,
        ControlSettings,
        GameplaySettings,
        InventoryMenu,
        LoadGameMenu,
        SaveGameMenu,
        CreditsMenu
    }

    /// <summary>
    /// ナビゲーションタイプ
    /// </summary>
    public enum NavigationType
    {
        Push,      // 新しいメニューを上に重ねる
        Pop,       // 現在のメニューを閉じて前に戻る
        Replace,   // 現在のメニューを置き換える
        Modal      // モーダルダイアログとして表示
    }

    /// <summary>
    /// 通知タイプ
    /// </summary>
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error,
        Achievement,
        QuestComplete,
        ItemPickup,
        LevelUp
    }
}