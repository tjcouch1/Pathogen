using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationsManager : MonoBehaviour {

    public GameObject notificationPrefab;
    public GameObject healthyWin;
    public GameObject infectedWin;
    public RectTransform centerScreen;
    public float notificationTimeout = 10;
    public float winNotificationTimeout = 5;

    public static NotificationsManager instance;

    private void Awake()
    {
        healthyWin.SetActive(false);
        infectedWin.SetActive(false);

        if(instance != null)
        {
            Debug.LogError("Cannot have more than one notifications manager in the scene!");
        }

        instance = this;
    }

    public void CreateNotification(string text, string desc)
    {
        var go = Instantiate(notificationPrefab, this.transform);
        NotificationExample note = go.GetComponent<NotificationExample>();
        note.titleText = text;
        note.descriptionText = desc;
        note.ShowNotification();

        Destroy(go, notificationTimeout);
    }

    /*<summary>
     * An override for CreateNotification that also takes a custom sprite and color
     *</summary>
     */
    public void CreateNotification(string text, string desc, Sprite icon, Color color)
    {
        var go = Instantiate(notificationPrefab, this.transform);
        NotificationExample note = go.GetComponent<NotificationExample>();
        note.titleText = text;
        note.descriptionText = desc;
        note.textColor = color;
        note.icon.sprite = icon;
        note.ShowNotification();

        Destroy(go, notificationTimeout);
    }

    public void DisplayWinState(bool healthyWin)
    {
        StartCoroutine(WinDisplayCoroutine(healthyWin));
    }

    private IEnumerator WinDisplayCoroutine(bool healthtyWin)
    {
        Debug.Log("Win State Notifications called");
        if (healthtyWin)
        {
            healthyWin.SetActive(true);
            yield return new WaitForSeconds(winNotificationTimeout);
            healthyWin.SetActive(false);
        }
        else
        {
            infectedWin.SetActive(true);
            yield return new WaitForSeconds(winNotificationTimeout);
            infectedWin.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            instance.CreateNotification("Test Notification", "This is a test");
        }
    }

}
