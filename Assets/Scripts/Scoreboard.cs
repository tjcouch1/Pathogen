using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scoreboard : MonoBehaviour {

    [SerializeField]
    GameObject ScoreboardElementPrefab;

    [SerializeField]
    Transform playersList;

    private void OnEnable()
    {
        //Get an array of players
        Player[] players = GameManager.getAllPlayers();

        //Loop through and set up a list item for each one
        foreach (Player player in players)
        {
            GameObject itemGO = Instantiate(ScoreboardElementPrefab, playersList);
            ScoreboardElement element = itemGO.GetComponent<ScoreboardElement>();
            if (element != null)
            {
                //Setting UI elements equal to relevant data handled in scoreboard element class
                element.Setup(player.username, player.points, player.killCount, player.deathCount);
            }
        }
        
    }

    private void OnDisable()
    {
        //Clean up our list of items
        foreach (Transform child in playersList)
        {
            Destroy(child.gameObject);
        }
    }
}
