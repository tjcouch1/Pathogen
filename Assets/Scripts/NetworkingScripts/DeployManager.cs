using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DatabaseControl; // << Remember to add this reference to your scripts which use DatabaseControl
using UnityEditor;
using UnityEngine.UI;

public class DeployManager : MonoBehaviour {

    [SerializeField] private GameObject versionInput;
    [SerializeField] private GameObject versionInputPlaceholder;
    [SerializeField] private GameObject deployToggle;
    [SerializeField] private GameObject updateButton;
    

	public const string user = "deploy";
	public const string pass = "versionPassW0Ot";

    public delegate void OnDataRecievedCallback(string data);

	// Use this for initialization
	void Start () {
        //lock the ui elements until the server data is loaded
        versionInput.GetComponent<InputField>().enabled = false;
        versionInputPlaceholder.GetComponent<Text>().text = Application.version;
        deployToggle.GetComponent<Toggle>().enabled = false;

        DoGetDataUpdateUI();
	}

    //set the values for the ui to the server values
    private void updateUI(string data)
    {
        string deployVersion = UADataTranslator.DataToVersion(data);
        bool isDeployed = UADataTranslator.DataToDeploy(data);

        versionInput.GetComponent<InputField>().enabled = true;
        deployToggle.GetComponent<Toggle>().enabled = true;

        if (deployVersion != Application.version)
            versionInput.GetComponent<InputField>().text = deployVersion;
        else versionInput.GetComponent<InputField>().text = "";

        deployToggle.GetComponent<Toggle>().isOn = isDeployed;
    }

	private static string deployString(string version, bool deploy)
	{
		string customVersion;
		if (string.IsNullOrEmpty(version))
			customVersion = Application.version;
		else customVersion = version;

		return "[VERSION]" + customVersion + "/[DEPLOY]" + deploy + "/";
	}

    public void DoRegisterUser()
    {
        StartCoroutine(RegisterUser());
    }

    IEnumerator RegisterUser()
    {
		Debug.Log("Register");
		string data = deployString(versionInput.GetComponent<InputField>().text, deployToggle.GetComponent<Toggle>().isOn);
        IEnumerator e = DCF.RegisterUser(user, pass, data); // << Send request to register a new user, providing submitted username and password. It also provides an initial value for the data string on the account, which is "Hello World".
        while (e.MoveNext())
        {
            yield return e.Current;
        }
        string response = e.Current as string; // << The returned string from the request

        if (response == "Success")
        {
			Debug.Log("successfully registered deploy with data: " + data);
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

    public void DoSetData()
    {
        StartCoroutine(SetData());
    }

    IEnumerator SetData()
    {
		string data = deployString(versionInput.GetComponent<InputField>().text, deployToggle.GetComponent<Toggle>().isOn);
        IEnumerator e = DCF.SetUserData(user, pass, data); // << Send request to set the player's data string. Provides the username, password and new data string
        while (e.MoveNext())
        {
            yield return e.Current;
        }
        string response = e.Current as string; // << The returned string from the request

        if (response == "Success")
        {
			Debug.Log("Successfully set deploy data to: " + data);
        }
        else
        {
            //There was another error. This error message should never appear, but is here just in case.
            //In this case the data will not be changed
            Debug.LogError("UAM Error: deployManager.SetData - Unknown Error. Please try again later.");
        }
    }

    public void DoGetDataUpdateUI()
    {
        StartCoroutine(GetData(updateUI));
    }

    public static IEnumerator GetData(OnDataRecievedCallback onDataReceived)
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
            if (onDataReceived != null)
                onDataReceived.Invoke(response);
        }
    }
}
