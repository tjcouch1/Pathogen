using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PointsSystem : NetworkBehaviour {

    [SerializeField]
    private Sprite warningLogo;
    [SerializeField]
    private Color warningsColor;

    [Tooltip("Players who have more points than this value are considered trustworthy")]
    [SerializeField] private int trustyNo;
    [Tooltip("Players who have less points than this value are considered untrustworthy")]
    [SerializeField] private int neutralNo;

    //Calculate a player's trust factor at the beginning of each round
    private List<Player> trustyBois;
    private List<Player> badBois;

    //The amount of teamkills each player has - reset at the start of the round
    private Dictionary<Player, int> playerTKs;

    private void Start()
    {
        Debug.Log("Points set up");
        playerTKs = new Dictionary<Player, int>();
        trustyBois = new List<Player>();
        badBois = new List<Player>();
        GameManager.singleton.onStartRoundCallbacks.Add(OnStartRoundCallback);
        GameManager.singleton.onPlayerKilledCallbacks.Insert(0, PointsOnDeathCallback);
        
    }

    public void OnStartRoundCallback()
    {
        //Reset player trust-factor/TKs lists
        playerTKs.Clear();
        trustyBois.Clear();
        badBois.Clear();

        //Reset player TKS
        Player[] players = GameManager.getAllPlayers();
        foreach(Player p in players)
        {
            playerTKs.Add(p, 0);
            if(p.points > trustyNo)
            {
                trustyBois.Add(p);
            }
            else if(p.points < neutralNo)
            {
                badBois.Add(p);
            }
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
            playerTKs[sourcePlayer] += 1;
            CheckRDM(sourceName);
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

    private void CheckRDM(string sourcePlayer)
    {
        var player = GameManager.getPlayer(sourcePlayer);

        if (playerTKs.ContainsKey(player))
        {
            //Player gets one chance if they are untrustworthy
            if(playerTKs[player] == 1 && badBois.Contains(player))
            {
                CmdRemovePlayer(sourcePlayer);
            }
            //Player gets 2 chances if they are neutral
            else if(playerTKs[player] == 2 && !trustyBois.Contains(player))
            {
                CmdRemovePlayer(sourcePlayer);
            }
            //Player gets 3 chances if they are trustworthy
            else if(playerTKs[player] == 3 && trustyBois.Contains(player))
            {
                CmdRemovePlayer(sourcePlayer);
            }
        }
        else
        {
            Debug.LogWarning("Points System - Player not found in dictionary!");
        }
    }

    [Command]
    private void CmdRemovePlayer(string sourcePlayer)
    {
        var player = GameManager.getPlayer(sourcePlayer);

        //Remove the player from the game
        RpcDisplayNotifications(sourcePlayer, "Teamkill!", "You were removed from the round for killing too many friendlies");
        player.RpcTakeDamage(100, "Anti-RDM");
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
