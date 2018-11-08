using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PointsSystem : NetworkBehaviour {

    [SerializeField]
    private Sprite warningLogo;
    [SerializeField]
    private Color warningsColor;

    //The amount of points each player has at the start of the round
    private Dictionary<Player, int> playerPoints;

    private void Start()
    {
        Debug.Log("Points set up");
        playerPoints = new Dictionary<Player, int>();
        GameManager.singleton.onStartRoundCallbacks.Add(OnStartRoundCallback);
        GameManager.singleton.onPlayerKilledCallbacks.Insert(0, PointsOnDeathCallback);
        
    }

    public void OnStartRoundCallback()
    {
        playerPoints.Clear();

        //Save the points values that players have at the beginning of a round
        Player[] players = GameManager.getAllPlayers();
        foreach(Player p in players)
        {
            playerPoints.Add(p, p.points);
        }
    }

    public void PointsOnDeathCallback(string playerName, string sourceName)
    {
        Debug.Log("Points on Player Killed Callback called");
        var killedPlayer = GameManager.getPlayer(playerName);
        var sourcePlayer = GameManager.getPlayer(sourceName);

        if(killedPlayer == null || sourcePlayer == null)
        {
            Debug.Log("Points on death callback called without a target player");
            return;
        }

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
        CheckRDM(sourceName);
    }

    private void CheckRDM(string sourcePlayer)
    {
        var player = GameManager.getPlayer(sourcePlayer);

        if (playerPoints.ContainsKey(player))
        {
            var deltaPoints = player.points - playerPoints[player];
            if(deltaPoints < 0 && deltaPoints >= -10)
            {
                //Warn the player they are losing points
                RpcDisplayNotifications(sourcePlayer, "Warning!", "You are losing points quickly. Be careful who you shoot!");
            }
            else if(deltaPoints < -10 && deltaPoints >= -20)
            {
                //Warn the player if they kill another player they will be kicked
                RpcDisplayNotifications(sourcePlayer, "Warning!", "If you teamkill another player, you will be removed from the round!");
            }
            else if(deltaPoints < -20)
            {
                //Remove the player from the game
                RpcDisplayNotifications(sourcePlayer, "Teamkill!", "You were removed from the round for killing too many friendlies");
                player.RpcTakeDamage(100, "Anti-RDM");
            }
        }
        else
        {
            Debug.LogWarning("Points System - Player not found in dictionary!");
        }
    }

    [ClientRpc]
    private void RpcDisplayNotifications(string sourceID, string text, string desc)
    {
        var player = GameManager.getPlayer(sourceID);

        if (player.isLocalPlayer)
        {
            NotificationsManager.instance.CreateNotification(text, desc, warningLogo, warningsColor);
        }
    }
}
