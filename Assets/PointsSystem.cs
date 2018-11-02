using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsSystem : MonoBehaviour {

    private void Start()
    {
        Debug.Log("Points set up");
        GameManager.singleton.onPlayerKilledCallbacks.Insert(0, PointsOnDeathCallback);
    }

    public void PointsOnDeathCallback(string playerName, string sourceName)
    {
        Debug.Log("Points on Player Killed Callback called");
        var killedPlayer = GameManager.getPlayer(playerName);
        var sourcePlayer = GameManager.getPlayer(sourceName);

        if (killedPlayer.GetInfectedState() == sourcePlayer.GetInfectedState())
        {
            //Player teamkilled! Bad bad!
            sourcePlayer.points -= 10;
        }
        else
        {
            //Player was infected and killed a healthy person
            if (sourcePlayer.GetInfectedState() == true)
            {
                sourcePlayer.points -= 5;
            }
            //Player was healthy and killed an infected person
            else
            {
                sourcePlayer.points += 10;
            }
        }
    }
}
