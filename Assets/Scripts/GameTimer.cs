using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameTimer : NetworkBehaviour {

    [SyncVar] private int roundTime;    //Round time in seconds
    [SyncVar] private string title;
    [SyncVar] public bool timerIsRunning = false;
    private HashSet<timerEvent> events = new HashSet<timerEvent>();
    public static GameTimer singleton;

    private void Awake()
    {
        if (singleton != null)
        {
            Debug.LogError("More than one GameTimer present in the scene!");
        }
        else
        {

            singleton = this;
        }
    }

    public int getRoundTime()
    {
        return roundTime;
    }

    public string getRoundTitle()
    {
        return title;
    }

    public void setRoundTitle(string _title)
    {
        title = _title;
    }

    //Only the server should be able to start and stop the timer
    public void StartTimer(int _roundTime)
    {
        if (!isServer)
        {
            return;
        }
        roundTime = _roundTime;
        StartCoroutine(roundTimer());
    }

    //Only the server should be able to start and stop the timer
    public void StopTimer()
    {
        if (!isServer)
        {
            return;
        }
        StopCoroutine(roundTimer());
        timerIsRunning = false;
    }

    public void addTimerEvent(timerEvent newEvent)
    {
        events.Add(newEvent);
    }

    public void clearTimerEvents()
    {
        events.Clear();
    }

    //Only the server has authority to count down the timer
    [Server]
    IEnumerator roundTimer()
    {
        while (true)
        {
            timerIsRunning = true;
            foreach (timerEvent timerEvent in events)
            {
                if (timerEvent != null)
                {
                    if (roundTime == timerEvent.time)
                    {
                        timerEvent.eventCallbackFunction();
                        Debug.Log("Event " + timerEvent + " was called.");
                    }
                }
            }
      
            yield return new WaitForSeconds(1);
            roundTime--;
        }
    }

}
