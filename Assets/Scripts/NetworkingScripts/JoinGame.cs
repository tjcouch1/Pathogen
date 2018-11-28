﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using System;

public class JoinGame : MonoBehaviour {

    List<GameObject> roomList = new List<GameObject>();
    private VCNetworkManager networkManager;
    [SerializeField] private Text statusText;
    [SerializeField] private GameObject roomListItemPrefab;
    [SerializeField] private Transform roomListItemParent;
    [SerializeField] private InputField joinPrivateInput;
    private bool quickPlaying = false;

    private void Start()
    {
        networkManager = (VCNetworkManager) NetworkManager.singleton;
        if(networkManager.matchMaker == null)
        {
            networkManager.StartMatchMaker();
        }

        RefreshList();
    }

    public void RefreshList()
    {
        ClearRoomList();
        if(networkManager.matchMaker == null)
        {
            networkManager.StartMatchMaker();
        }

        networkManager.matchMaker.ListMatches(0, 20, "", false, 0, 0, OnMatchList);
        statusText.text = "Loading...";
    }

    public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
    {
        statusText.text = "";

        if(matches == null)
        {
            statusText.text = "Could not retrieve room list";
            return;
        }
    
        foreach(MatchInfoSnapshot match in matches)
        {
            GameObject roomListItemGO = Instantiate(roomListItemPrefab);
            roomListItemGO.transform.SetParent(roomListItemParent);

            //Have a component sit on the gameObject that will take care of setting up the name/amount of users
            //as well as a callback function that will join the game
            RoomListItem roomListItem = roomListItemGO.GetComponent<RoomListItem>();
            if(roomListItem != null)
            {
                roomListItem.Setup(match, JoinRoom);
            }
            
            roomList.Add(roomListItemGO);

        }

        if(roomList.Count == 0)
        {
            statusText.text = "No rooms available :( ";
        }

    }

    public void QuickPlay()
    {
        if (!quickPlaying)
        {
            quickPlaying = true;
            //Try to join an existing game
            try
            {
                RoomListItem _rli = roomList[0].GetComponent<RoomListItem>();
                if (_rli != null)
                {
                    _rli.JoinRoom();
                }
                Debug.LogError("A room was found in the list, but a connection could not be made.");
            }
            //If none available, try to start a new match
            catch (ArgumentOutOfRangeException)
            {
                Debug.Log("No Rooms Available :(");
                string matchName = UserAccountManager.playerUsername + "'s Room";
                uint players = 10;

                //public room
                networkManager.matchMaker.CreateMatch(matchName, players, true, "", "", "", 0, 0, networkManager.OnMatchCreate);
                networkManager.matchSize = players;

                //private room
                //NetworkManager.singleton.maxConnections = (int) players - 1;
                //networkManager.networkPort = serverPort;
                //networkManager.isPrivate = true;
                //networkManager.StartHost();
                //networkManager.matchSize = players;
            }
        }
    }

    void ClearRoomList()
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            Destroy(roomList[i]);
        }

        roomList.Clear();
    }

    public void JoinRoom(MatchInfoSnapshot match)
    {
        cancelJoin(false);
        Debug.Log("Joining " + match.name);
        networkManager.matchMaker.JoinMatch(match.networkId, "", "", "", 0, 0, networkManager.OnMatchJoined);
        StartCoroutine(WaitForJoin());
    }

    //join game off matchmaking
    public void JoinPrivateRoom()
    {
        cancelJoin(false);

        string serverIP = joinPrivateInput.text;
        if (string.IsNullOrEmpty(serverIP))
            serverIP = "localhost";

        Debug.Log("Joining private " + serverIP);

        //join private room - big thanks to l3fty at https://forum.unity.com/threads/lan-with-unet.346182/
        //networkManager.networkPort = serverPort;
        networkManager.networkAddress = serverIP;
        networkManager.isPrivate = true;
        networkManager.StartClient();
        //networkManager.matchMaker.JoinMatch(match.networkId, "", "", "", 0, 0, networkManager.OnMatchJoined);
        //StartCoroutine(WaitForJoin());
    }

    IEnumerator WaitForJoin()
    {
        ClearRoomList();
        statusText.text = "Joining game...";

        int countdown = 10;
        while (countdown > 0)
        {
            countdown--;
            yield return new WaitForSeconds(1);
        }

        //Failed to connect to game
        statusText.text = "Failed to connect to game. Connection timeout afer 10 seconds.";
        yield return new WaitForSeconds(2);

        cancelJoin(true);
    }

    void cancelJoin(bool refreshList)
    {
        Debug.Log("Canceling join");
        MatchInfo matchInfo = networkManager.matchInfo;
        if (matchInfo != null)
        {
            networkManager.matchMaker.DropConnection(matchInfo.networkId, matchInfo.nodeId, 0, networkManager.OnDropConnection);
            networkManager.StopHost();
        }
        if (refreshList)
            RefreshList();
    }
}
