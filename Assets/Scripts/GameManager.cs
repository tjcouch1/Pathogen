using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {

    public static GameManager singleton;
    [SerializeField] private GameObject sceneCamera;

    public delegate void OnPlayerKilledCallback(string player, string source);
    public OnPlayerKilledCallback onPlayerKilledCallback;

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
    [SyncVar] private bool inCurrentRound = false;

    private void Awake()
    {
        if (singleton != null)
        {
            Debug.LogError("More than one GameManager present in the scene!");
        }
        else
        {
            singleton = this;
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

    //Called when a new player joins, or whenever we are finished in the lobby
    [Command]
    public void CmdStartRound()
    {

        //We only want to start a round if enough players are connected, and we are not currently in a round already
        if(getAllPlayers().Length >= requiredPlayers && inCurrentRound == false)
        {
            if (GameTimer.singleton.timerIsRunning)
            {
                Debug.LogError("GameTimer is running! Cannot start new round");
                return;
            }

            //Choose one player at random to be infected
            Player[] players = getAllPlayers();
            var rand = Random.Range(0, players.Length);
            players[rand].isInfected = true;

            //Setup timer events
            singleton.initRoundEvents();
            GameTimer.singleton.StartTimer(roundTime);
            inCurrentRound = true;
            roundNumber++;
            GameTimer.singleton.setRoundTitle( "Round " + roundNumber); 
            Debug.Log("Round started!");
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
        bool winCase = true;
        if (winCase)
        {
            //We do not go into overtime
            RpcUpdatePlayersTimerUI(Color.blue);
            StartLobby();
            inCurrentRound = false;

            //Reset all players to not infected
            var players = getAllPlayers();
            foreach(Player p in players)
            {
                p.isInfected = false;
            }
        }
        else
        {
            GameTimer.singleton.setRoundTitle("OVERTIME");
        }
    }

    //Should only be called in between rounds, or if we don't have enough players
    public void StartLobby()
    {
        //Whenever getting ready to start a new GameTimer.singleton, we should make sure to stop old ones that may be running
        GameTimer.singleton.StopTimer();
        singleton.initLobbyEvents();
        GameTimer.singleton.setRoundTitle("Waiting for new players");
        GameTimer.singleton.StartTimer(lobbyTime);
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

        GameManager.singleton.CmdStartRound();
    }

    public static void UnRegisterPlayer(string playerID)
    {
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
