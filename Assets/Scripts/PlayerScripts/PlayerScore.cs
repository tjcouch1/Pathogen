using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerScore : MonoBehaviour {

    Player player;
    [SerializeField]
    private float syncRate = 5;
    private int lastKills = 0;
    private int lastDeaths = 0;
    private int lastPoints = 0;

	// Use this for initialization
	void Start () {
        player = GetComponent<Player>();
        StartCoroutine(SyncLoop());
	}

    //Method used when restarting a match so that the lastKills and lastDeaths for each player should be 0
    public void resetScore()
    {
        lastKills = 0;
        lastDeaths = 0;
        lastPoints = 0;
    }

    IEnumerator SyncLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(syncRate);
            SyncScore();
        }

    }

    //Function call to sync the local match's score with the server DB. Should be called when player exits a match
    public void SyncScore()
    {
        if (UserAccountManager.IsLoggedIn)
        {
            UserAccountManager.instance.GetPlayerData(OnDataRecieved);
        }

    }

    void OnDestroy()
    {
        if(player != null)
            SyncScore();
    }

    void OnDataRecieved(string data)
    {

        int deltaKills = player.killCount - lastKills;
        int deltaDeath = player.deathCount - lastDeaths;
        int deltaPoints = player.points - lastPoints;

        //Don't sync if the player hasn't made any progress
        if (deltaKills == 0 && deltaDeath == 0 && deltaPoints == 0)
        {
            return;
        }

        int kills = UADataTranslator.DataToKills(data);
        int deaths = UADataTranslator.DataToDeaths(data);
        int points = UADataTranslator.DataToPoints(data);

        kills += deltaKills;
        deaths += deltaDeath;
        points += deltaPoints;

        string newData = UADataTranslator.FormatDataToString(kills, deaths, points);

        Debug.Log("SYNCING: " + newData + " for " + player.username);

        lastKills = player.killCount;
        lastDeaths = player.deathCount;
        lastPoints = player.points;

        UserAccountManager.instance.SetPlayerData(newData);

    }
}
