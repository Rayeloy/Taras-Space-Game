using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum NotificationButton
{
    None,
    rightMenu_LeftArrow,
    leftMenu_Lab,
    leftMenu_Map,
    leftMenu_FoodTable,
    leftMenu_RightArrow,
    lab_Chemistry,
    lab_MejoraPersonajes,
    lab_Workshop
}

public enum MenuState
{
    None,
    MenuRight,
    MenuLeft,
    Lab,
    Lab_Chemistry,
    Lab_MejoraPersonajes,
    Lab_Workshop,
    Map,
    FoodTable,
    Shop,
    Merchant,
    Options,
    Machine
}

public class NotificationsManagerScript : MonoBehaviour
{
    public static NotificationsManagerScript instance;
    public MenuNotification[] notifications;
    public UIAnimationBaseSettingData animationBaseSettings;

    public bool MenuHasNotification(MenuState menu)
    {
        bool result = false;
        for (int i = 0; i < notifications.Length && !result; i++)
        {
            if (notifications[i].menu == menu && notifications[i].active) result = true;
        }
        return result;
    }

    public bool AnyButtonDependecyOn(MenuNotification notification)
    {
        for (int i = 0; i < notification.buttonDependencies.Length; i++)
        {
            if (IsNotificationActive(notification.buttonDependencies[i])) return true;
        }
        return false;
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene current, Scene next)
    {
        Setup();
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }
    }

    private void Start()
    {
        Setup();
    }

    void Setup()
    {
        //Debug.Log("Start NotificationsManager setup");
        for (int i = 0; i < notifications.Length; i++)
        {
            notifications[i].animation.baseSettings = animationBaseSettings;

            notifications[i].SwitchNotification(false, true, true, false);
        }
        for (int i = 0; i < notifications.Length; i++)
        {
            if (notifications[i].GetSavedActiveState())
            {
                notifications[i].SwitchNotification(true, true, true);
            }
        }
    }

    public void ResetNotifications()
    {
        for (int i = 0; i < notifications.Length; i++)
        {
            notifications[i].Reset();
        }
    }

    public void SwitchNotification(NotificationButton button, bool active, bool dontSave = false)
    {
        for (int i = 0; i < notifications.Length; i++)
        {
            if (notifications[i].button == button)
            {
                notifications[i].SwitchNotification(active, dontSave);
                return;
            }
        }
    }

    public bool IsNotificationActive(NotificationButton button)
    {
        for (int i = 0; i < notifications.Length; i++)
        {
            if (notifications[i].button == button && notifications[i].active) return true;
        }
        return false;
    }

    public void OnNotificationSwitched(MenuNotification notif, bool active)
    {
        for (int i = 0; i < notifications.Length; i++)
        {
            if (notif.button == notifications[i].button) continue;
            if (notifications[i].HasButtonDependency(notif.button))
            {
                if (active)
                {
                    //Debug.Log("the button " + notif.button + " has switched to " + active + " so we turn the button " + notifications[i].button + " to " + active + " too");
                    notifications[i].SwitchNotification(active);
                }
                else if (!AnyButtonDependecyOn(notif))
                {
                    //Debug.Log("the button " + notif.button + " has switched to " + active + " so we turn the button " + notifications[i].button + " to " + active + " too");
                    notifications[i].SwitchNotification(active);
                }

            }
        }
    }

    public void SwitchLabTablesNotifications(bool active)
    {
        for (int i = 0; i < notifications.Length; i++)
        {
            if (notifications[i].button == NotificationButton.lab_Chemistry || notifications[i].button == NotificationButton.lab_MejoraPersonajes ||
                 notifications[i].button == NotificationButton.lab_Workshop)
            {
                if (!active)
                {
                    notifications[i].SwitchNotification(active, true);
                }
                else if (notifications[i].GetSavedActiveState())
                {
                    notifications[i].SwitchNotification(active, true);
                }
            }
        }
    }


}

[System.Serializable]
public class MenuNotification
{
    public UIAnimation animation;
    public NotificationButton[] buttonDependencies;
    public MenuState menu = MenuState.None;
    public bool active = false;
    public NotificationButton button = NotificationButton.None;

    public bool savingNeeded
    {
        get
        {
            return (buttonDependencies == null || buttonDependencies.Length == 0);
        }
    }

    public MenuNotification(UIAnimation _animation, NotificationButton _button, NotificationButton[] _buttonDependencies, MenuState _menu, bool _active)
    {
        animation = _animation;
        button = _button;
        buttonDependencies = _buttonDependencies;
        menu = _menu;
        active = _active;
    }

    public void SwitchNotification(bool _active, bool dontSave = false, bool forced = false, bool propagation = true)
    {
        //Debug.Log("SwitchNotification " + _active + "; dontSave = " + dontSave + "; notificationButton = " + button);
        if ((forced && _active) || (_active && !active))
        {
            active = true;
            animation.transf.gameObject.SetActive(true);
            UIAnimationsManager.instance.StartAnimation(animation);
            if (!dontSave && savingNeeded) PlayerPrefs.SetInt("Notification_" + button, 1);
            if (propagation) NotificationsManagerScript.instance.OnNotificationSwitched(this, active);
        }
        else if ((forced && !_active) || (!_active && active))
        {
            active = false;
            UIAnimationsManager.instance.StopUIAnimation(animation);
            animation.transf.gameObject.SetActive(false);
            //Debug.Log("dontSave = " + dontSave + "; savingNeeded = " + savingNeeded);
            if (!dontSave && savingNeeded)
            {
                //Debug.Log("SwitchNotification off with saving enable for " + button);
                PlayerPrefs.SetInt("Notification_" + button, 0);
            }
            if (propagation) NotificationsManagerScript.instance.OnNotificationSwitched(this, active);
        }
    }

    public bool GetSavedActiveState()
    {
        bool result = false;
        if (savingNeeded)
        {
            //Debug.Log("GetSavedActiveState -> true ; button = " + button);
            result = PlayerPrefs.GetInt("Notification_" + button, 0) == 1;
        }
        return result;
    }

    public bool HasButtonDependency(NotificationButton button)
    {
        for (int i = 0; i < buttonDependencies.Length; i++)
        {
            if (buttonDependencies[i] == button) return true;
        }
        return false;
    }

    public void Reset()
    {
        PlayerPrefs.SetInt("Notification_" + button, 0);
    }
}
