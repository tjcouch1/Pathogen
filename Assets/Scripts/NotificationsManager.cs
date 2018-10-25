using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationsManager : MonoBehaviour {

    public GameObject notificationPrefab;
    public float notificationTimeout;

    public static NotificationsManager instance;

    private void Awake()
    {
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

}
