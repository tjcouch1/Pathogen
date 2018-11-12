﻿using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HostGame : MonoBehaviour {

    [SerializeField]
    private uint roomSize = 10;
    [SerializeField] private uint maxRoomSize = 10;
    private NetworkManager networkManager;
    private string roomName;
    [SerializeField] private Text errorText;
    [SerializeField] private GameObject hostName;
    [SerializeField] private GameObject hostNum;

    private void Start()
    {
		Random.InitState((int)System.DateTime.Now.Ticks); 
		networkManager = NetworkManager.singleton;
        if(networkManager.matchMaker == null)
        {
            networkManager.StartMatchMaker();
        }

        hostName.GetComponent<InputField>().text = UserAccountManager.playerUsername + "'s Room";
        hostNum.GetComponent<InputField>().text = roomSize.ToString();
    }

    public void SetRoomName(string name)
    {
        roomName = name;
    }

    public void SetRoomSize(string size)
    {
        if(!uint.TryParse(size, out roomSize))
        {
            Debug.LogError("Could not parse room size input!");
        }
       
    }

    public void CreateRoom()
    {
        if(roomName != "" && roomName != null)
        {
            if(roomSize <= maxRoomSize && roomSize > 1)
            {
                Debug.Log("Creating Room: " + roomName + " for " + roomSize + " players.");

                //create room
                networkManager.matchMaker.CreateMatch(roomName, roomSize, true, "", "", "", 0, 0, networkManager.OnMatchCreate);
				networkManager.matchSize = roomSize;
            }
            else
            {
                Debug.LogError("Invalid room size! Please use room size 2 - " + maxRoomSize);
                errorText.text = "Invalid room size! Please use room size 2 - " + maxRoomSize;
            }      
        }
        else
        {
            Debug.LogError("Invalid room name! Room name must not equal NULL");
            errorText.text = "Invalid room name! Room name must not equal NULL";
        }
    }
}
