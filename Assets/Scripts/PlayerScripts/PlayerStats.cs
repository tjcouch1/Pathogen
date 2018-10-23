using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour {

    public Text playerStats;

    void Start()
    {
        if(UserAccountManager.IsLoggedIn)
            UserAccountManager.instance.GetPlayerData(OnDataRecieved);
    }

    void OnDataRecieved(string data)
    {
        //Debug.Log(data);

        //Eventually this text might scroll, but this works fine for now
        string marquee = "";

        marquee += "KILLS: " + UADataTranslator.DataToKills(data).ToString() + "    ";
        marquee += "DEATHS: " + UADataTranslator.DataToDeaths(data).ToString() + "    ";
        marquee += "POINTS: " + UADataTranslator.DataToPoints(data).ToString() + "    ";

        playerStats.text = marquee;
    }
}
