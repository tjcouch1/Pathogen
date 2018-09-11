using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DatabaseControl;
using UnityEngine.SceneManagement;

public class UserAccountManager : MonoBehaviour {

    public static UserAccountManager instance;

    public string loggedInScene = "Lobby";
    public string loggedOutScene = "Login";

    private LO_LoadScene looaderManager;

    //These store the username and password of the player when they have logged in
    public static string playerUsername { get; protected set; }
    public static string playerPassword = "";
    public static string playerData { get; protected set; }

    public static bool IsLoggedIn { get; protected set; }

    public delegate void OnDataRecievedCallback(string data);

    //Sets up simple singleton pattern
    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);
        looaderManager = GameObject.FindGameObjectWithTag("looader").GetComponent<LO_LoadScene>();
    }

    public void Logout()
    {
        playerUsername = "";
        playerPassword = "";

        IsLoggedIn = false;

        Debug.Log("User logged out");

        Application.Quit();
    }

    public void Login(string username, string password)
    {
        playerUsername = username;
        playerPassword = password;

        IsLoggedIn = true;

        Debug.Log("User Logged in as: " + playerUsername);

        if (looaderManager != null)
        {
            looaderManager.ChangeToScene(loggedInScene);
        }
        else
        {
            SceneManager.LoadScene(loggedInScene);
        }
    }

    public void SetPlayerData(string data)
    {
        //starts coroutine to set the players data string on the server DB
        StartCoroutine(SetData(data));
    }

    public void GetPlayerData(OnDataRecievedCallback onDataRecieved)
    {
        StartCoroutine(GetData(onDataRecieved));
    }

    IEnumerator GetData(OnDataRecievedCallback onDataRecieved)
    {
        IEnumerator e = DCF.GetUserData(playerUsername, playerPassword); // << Send request to get the player's data string. Provides the username and password
        while (e.MoveNext())
        {
            yield return e.Current;
        }
        string response = e.Current as string; // << The returned string from the request

        if (response == "Error")
        {
            //There was another error. This error message should never appear, but is here just in case.
            //In this case the old data that currently exists in the UAM will be returned
            Debug.LogError("UAM Error: GetData - Unknown Error. Please try again later.");
        }
        else
        {
            //The player's data was retrieved. Puts the player's data in the UAM
            playerData = response;
            if(onDataRecieved != null)
                onDataRecieved.Invoke(response);
        }
    }

    IEnumerator SetData(string data)
    {
        IEnumerator e = DCF.SetUserData(playerUsername, playerPassword, data); // << Send request to set the player's data string. Provides the username, password and new data string
        while (e.MoveNext())
        {
            yield return e.Current;
        }
        string response = e.Current as string; // << The returned string from the request

        if (response == "Success")
        {
            //The data string was set correctly. Update playerData variable
            playerData = data;
        }
        else
        {
            //There was another error. This error message should never appear, but is here just in case.
            //In this case the data will not be changed
            Debug.LogError("UAM Error: SetData - Unknown Error. Please try again later.");
        }
    }


}
