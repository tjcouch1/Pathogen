﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {

    public static GameManager singleton;
    [SerializeField] private GameObject sceneCamera;

    public delegate void OnPlayerKilledCallback(string player, string source);
    public List<OnPlayerKilledCallback> onPlayerKilledCallbacks;

    //Required number of players for a game to start
    [SerializeField] private int requiredPlayers = 1;
    [SerializeField] private int roundTime = 600;
    [SerializeField] private int suddenDeathTime = 180;
    [SerializeField] private int lobbyTime = 180;
    private int roundNumber = 0;

    //List of infected and healthy players still in game
    private List<Player> healthyPlayers;
    private List<Player> infectedPlayers;

    //Timer events
    private timerEvent suddenDeathEvent;
    private timerEvent roundEnd;
    private timerEvent roundStartAlert;
    private timerEvent lobbyEnd;

    //OnSpawn, need to check if inCurrentRound to determine whether or not to spawn player
    [SyncVar] public bool inCurrentRound = false;

    private void Awake()
    {
        if (singleton != null)
        {
            Debug.LogError("More than one GameManager present in the scene!");
        }
        else
        {
            singleton = this;

            //Initialize Lists
            onPlayerKilledCallbacks = new List<OnPlayerKilledCallback>();
            healthyPlayers = new List<Player>();
            infectedPlayers = new List<Player>();
            onPlayerKilledCallbacks.Add(OnPlayerKilled);
        }
    }

    public void CallOnDeathCallbacks(string player, string source)
    {
        foreach(OnPlayerKilledCallback c in onPlayerKilledCallbacks)
        {
            c.Invoke(player, source);
        }
    }

    private void OnPlayerKilled(string player, string source)
    {
        Player p = getPlayer(player);
        if(p != null)
        {
            if (p.GetInfected())
            {
                infectedPlayers.Remove(p);
                p.SetInfected(false);
            }
            else if (!p.GetInfected())
            {
                healthyPlayers.Remove(p);
            }
        }
        else
        {
            Debug.LogError("Could not find " + player + " in player dictionary");
        }
    }

    public void RegisterNewInfected(Player infPlayer)
    {
        if (healthyPlayers.Contains(infPlayer))
        {
            healthyPlayers.Remove(infPlayer);
        }

        if (!infectedPlayers.Contains(infPlayer))
        {
            infectedPlayers.Add(infPlayer);
        }
        else
        {
            Debug.Log("Player " + infPlayer.name + " is already infected!");
        }
    }

    public void SetSceneCameraActive(bool isActive)
    {
        if (sceneCamera == null)
        {
            return;
        }
        sceneCamera.SetActive(isActive);
    }

    #region timerEvents

    private void initRoundEvents()
    {
        Debug.Log("Initializing round events");
        GameTimer.singleton.clearTimerEvents();

        GameTimer.singleton.addTimerEvent(new timerEvent(BeginSuddenDeath, suddenDeathTime));
        GameTimer.singleton.addTimerEvent(new timerEvent(EndRound, 0));
    }

    private void initLobbyEvents()
    {
        Debug.Log("Initializing lobby events");
        GameTimer.singleton.clearTimerEvents();

        GameTimer.singleton.addTimerEvent(new timerEvent(LobbyPreEnd, 10));
        GameTimer.singleton.addTimerEvent(new timerEvent(EndLobby, 0));
    }

    IEnumerator checkWinCondition()
    {
        while(inCurrentRound)
        {
            //Debug.Log("Checking for win condition");
            if (checkForWin())
            {
                EndRound();
            }
            yield return new WaitForSeconds(1);
        }
        
    }

    //Called when a new player joins, or whenever we are finished in the lobby
    [Command]
    public void CmdStartRound()
    {
        //We only want to start a round if enough players are connected, and we are not currently in a round already
        if(getAllPlayers().Length >= requiredPlayers && inCurrentRound == false)
        {
            if (GameTimer.singleton.timerIsRunning)
            {
                return;
            }
            Debug.Log("Round starting...");

            //Setup all the players for new round
            Player[] players = getAllPlayers();
            foreach(Player p in players)
            {
                p.CmdRespawnPlayer();
                p.SetInfected(false);
            }

            //Add all players to the list of healthy
            healthyPlayers.AddRange(players);

            //Choose one player at random to be infected
            var rand = Random.Range(0, players.Length);
            players[rand].SetInfected(true);
            RegisterNewInfected(players[rand]);
            
            //Setup timer events
            singleton.initRoundEvents();
            inCurrentRound = true;
            roundNumber++;
            GameTimer.singleton.setRoundTitle( "Round " + roundNumber);
            GameTimer.singleton.StartTimer(roundTime);
            Debug.Log("Round started!");

            //Start Coroutine that checks for a win condition
            StartCoroutine(checkWinCondition());
        }
        else if(inCurrentRound)
        {
            Debug.Log("In a current round already!");
        }
        else
        {
            Debug.Log("Not enough players connected!");
            //Go back to lobby to wait for players
            StartLobby();
        }
       
    }  

    //Happens when roundGameTimer.singleton == 180
    public void BeginSuddenDeath()
    {
        Debug.Log("BEGINNING SUDDEN DEATH CODE");
        //TO-DO: Implement sudden death
        RpcUpdatePlayersTimerUI(Color.red);
    }

    [ClientRpc]
    void RpcUpdatePlayersTimerUI(Color c)
    {
        PlayerUI[] playerUIs = FindObjectsOfType<PlayerUI>();
        foreach(PlayerUI ui in playerUIs)
        {
            ui.updateTimerColor(c);
        }
    }

    //Happens when roundGameTimer.singleton == 0, or should be called when a win case is met
    public void EndRound()
    {
        GameTimer.singleton.StopTimer();

        //TO-DO: Implement checking for win Case
        bool winCase = checkForWin();
        if (winCase)
        {
            StopCoroutine(checkWinCondition());

            //We do not go into overtime
            RpcUpdatePlayersTimerUI(Color.blue);
            inCurrentRound = false;

            //Reset all players to not infected
            var players = getAllPlayers();
            foreach(Player p in players)
            {
                p.SetInfected(false);
            }

            //Clear out our lists
            healthyPlayers.Clear();
            infectedPlayers.Clear();

            //Go to lobby
            StartLobby();
        }
        else
        {
            GameTimer.singleton.setRoundTitle("OVERTIME");
        }
    }

    private bool checkForWin()
    {
        if(healthyPlayers.Count == 0)
        {
            Debug.Log("Infected Win!");
            return true;
        }
        else if(infectedPlayers.Count == 0)
        {
            Debug.Log("Healthy Win!");
            return true;
        }
        else
        {
            //No win condition is reached
            return false;
        }
    }

    //Should only be called in between rounds, or if we don't have enough players
    public void StartLobby()
    {
        //Whenever getting ready to start a new GameTimer.singleton, we should make sure to stop old ones that may be running
        GameTimer.singleton.StopTimer();

        Debug.Log("Starting lobby...");
        Player[] players = getAllPlayers();
        foreach (Player p in players)
        {
            p.CmdSendPlayerToLobby();
        }

        singleton.initLobbyEvents();
        GameTimer.singleton.setRoundTitle("Waiting for new players");
        GameTimer.singleton.StartTimer(lobbyTime);
        Debug.Log("Lobby Set Up!");
    }

    //Anything that needs to be done just before the lobby ends
    public void LobbyPreEnd()
    {
        GameTimer.singleton.setRoundTitle("Next round starts in...");
    }

    //Will be called when the lobbyTimer == 0
    public void EndLobby()
    {
        GameTimer.singleton.StopTimer();
        CmdStartRound();           //Attempt to start round, if fails lobby will restart
    }

    #endregion

    #region PlayerDictionary
    private const string PLAYER_ID_PREFIX = "Player ";
    private static Dictionary<string, Player> playerDictionary = new Dictionary<string, Player>();

    public static void RegisterPlayer(string netID, Player _player)
    {
        string playerID = PLAYER_ID_PREFIX + netID;
        playerDictionary.Add(playerID, _player);
        _player.transform.name = playerID;
		//Potentially hook up the player to the voice proxy here

		GameManager.singleton.CmdStartRound();
    }

    public static void UnRegisterPlayer(string playerID)
    {
        var p = playerDictionary[playerID];
        if (p.GetInfected())
        {
            GameManager.singleton.infectedPlayers.Remove(p);
        }
        else
        {
            GameManager.singleton.healthyPlayers.Remove(p);
        }
        playerDictionary.Remove(playerID);
    }

    public static Player getPlayer(string playerID)
    {
        return playerDictionary[playerID];
    }

    public static Player[] getAllPlayers()
    {
        return playerDictionary.Values.ToArray();
    }

    
#endregion

}
