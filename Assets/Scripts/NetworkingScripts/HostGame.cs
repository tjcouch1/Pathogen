using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HostGame : MonoBehaviour {

    [SerializeField]
    private uint roomSize = 10;
    private NetworkManager networkManager;
    private string roomName;
    private bool isClicked = false;
    [SerializeField] private Text errorText;

    private void Start()
    {
		Random.InitState((int)System.DateTime.Now.Ticks);
        isClicked = false;
		networkManager = NetworkManager.singleton;
        if(networkManager.matchMaker == null)
        {
            networkManager.StartMatchMaker();
        }
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
        if (isClicked)
            return;

        if(roomName != "" && roomName != null)
        {
            if(roomSize <= 20 && roomSize > 1)
            {
                isClicked = true;
                Debug.Log("Creating Room: " + roomName + " for " + roomSize + " players.");

                //create room
                networkManager.matchMaker.CreateMatch(roomName, roomSize, true, "", "", "", 0, 0, networkManager.OnMatchCreate);
				networkManager.matchSize = roomSize;
            }
            else
            {
                Debug.LogError("Invalid room size! Please use room size 2-20");
                errorText.text = "Invalid room size! Please use room size 2 - 20";
            }      
        }
        else
        {
            Debug.LogError("Invalid room name! Room name must not equal NULL");
            errorText.text = "Invalid room name! Room name must not equal NULL";
        }
    }
}
