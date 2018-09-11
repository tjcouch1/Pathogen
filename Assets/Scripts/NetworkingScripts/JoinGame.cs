using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using System;

public class JoinGame : MonoBehaviour {

    List<GameObject> roomList = new List<GameObject>();
    private NetworkManager networkManager;
    [SerializeField] private Text statusText;
    [SerializeField] private GameObject roomListItemPrefab;
    [SerializeField] private Transform roomListItemParent;

    private void Start()
    {
        networkManager = NetworkManager.singleton;
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
            networkManager.matchMaker.CreateMatch(matchName, 10, true, "", "", "", 0, 0, networkManager.OnMatchCreate);
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
        Debug.Log("Joining " + match.name);
        networkManager.matchMaker.JoinMatch(match.networkId, "", "", "", 0, 0, networkManager.OnMatchJoined);
        StartCoroutine(WaitForJoin());
    }

    IEnumerator WaitForJoin()
    {
        ClearRoomList();
        statusText.text = "Joining game...";

        int countdown = 20;
        while (countdown > 0)
        {
            countdown--;
            yield return new WaitForSeconds(1);
        }

        //Failed to connect to game
        statusText.text = "Failed to connect to game. Connection timeout afer 20 seconds.";
        yield return new WaitForSeconds(2);

        MatchInfo matchInfo = networkManager.matchInfo;
        if (matchInfo != null)
        {
            networkManager.matchMaker.DropConnection(matchInfo.networkId, matchInfo.nodeId, 0, networkManager.OnDropConnection);
            networkManager.StopHost();
        }
        RefreshList();
    }
}
