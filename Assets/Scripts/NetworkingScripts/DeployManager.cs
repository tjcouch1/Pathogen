using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DatabaseControl; // << Remember to add this reference to your scripts which use DatabaseControl
using UnityEditor;

public class DeployManager : MonoBehaviour {

	public bool register = false;
    public string version = "";
	public bool deploy = true;

	public const string user = "deploy";
	public const string pass = "versionPassW0Ot";

    public delegate void OnDataRecievedCallback(string data);

	// Use this for initialization
	void Start () {
		if (register)
		{
            StartCoroutine(RegisterUser());

            Debug.Log("Sup");
		}
		else
		{
			StartCoroutine(SetData());
			Debug.Log("Sah");
		}
	}

	private static string deployString(string version, bool deploy)
	{
		string customVersion;
		if (string.IsNullOrEmpty(version))
			customVersion = Application.version;
		else customVersion = version;

		return "[VERSION]" + customVersion + "/[DEPLOY]" + deploy + "/";
	}

    IEnumerator RegisterUser()
    {
		Debug.Log("Register");
		string data = deployString(version, deploy);
        IEnumerator e = DCF.RegisterUser(user, pass, data); // << Send request to register a new user, providing submitted username and password. It also provides an initial value for the data string on the account, which is "Hello World".
        while (e.MoveNext())
        {
            yield return e.Current;
        }
        string response = e.Current as string; // << The returned string from the request

        if (response == "Success")
        {
			Debug.Log("successfully registered deploy with data: " + data);

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }
        else
        {
            if (response == "UserError")
            {
                //The username has already been taken. Player needs to choose another. Shows error message.
                Debug.Log("Error: Username Already Taken for deploy");
            }
            else
            {
                //There was another error. This error message should never appear, but is here just in case.
                Debug.Log("Error: Unknown Error. Please try again later for deploy.");
            }
        }
    }

    IEnumerator SetData()
    {
        Debug.Log("SetData");
		string data = deployString(version, deploy);
        IEnumerator e = DCF.SetUserData(user, pass, data); // << Send request to set the player's data string. Provides the username, password and new data string
        while (e.MoveNext())
        {
            yield return e.Current;
        }
        string response = e.Current as string; // << The returned string from the request

        if (response == "Success")
        {
			Debug.Log("Successfully set deploy data to: " + data);

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }
        else
        {
            //There was another error. This error message should never appear, but is here just in case.
            //In this case the data will not be changed
            Debug.LogError("UAM Error: deployManager.SetData - Unknown Error. Please try again later.");
        }
    }

    public static IEnumerator GetData(OnDataRecievedCallback onDataRecieved)
    {
        IEnumerator e = DCF.GetUserData(user, pass); // << Send request to get the player's data string. Provides the username and password
        while (e.MoveNext())
        {
            yield return e.Current;
        }
        string response = e.Current as string; // << The returned string from the request

        if (response == "Error")
        {
            //There was another error. This error message should never appear, but is here just in case.
            //In this case the old data that currently exists in the UAM will be returned
            Debug.LogError("UAM Error: GetData - Unknown Error for deploy. Please try again later.");
        }
        else
        {
            //The player's data was retrieved. Puts the player's data in the UAM
            Debug.Log("Got deploy info: " + response);
            if (onDataRecieved != null)
                onDataRecieved.Invoke(response);
        }
    }
}
