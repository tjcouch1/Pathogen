using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DatabaseControl; // << Remember to add this reference to your scripts which use DatabaseControl
using System.Text;

public class LoginManager : MonoBehaviour {

    //All public variables bellow are assigned in the Inspector

    //These are the GameObjects which are parents of groups of UI elements. The objects are enabled and disabled to show and hide the UI elements.
    public GameObject loginParent;
    public GameObject registerParent;
    public GameObject loadingParent;        //Using LOMenuUI system
    public GameObject loginUIParent;

    //These are all the InputFields which we need in order to get the entered usernames, passwords, etc
    public InputField Login_UsernameField;
    public InputField Login_PasswordField;
    public InputField Register_UsernameField;
    public InputField Register_PasswordField;
    public InputField Register_ConfirmPasswordField;

    //These are the UI Texts which display errors
    public Text Login_ErrorText;
    public Text Register_ErrorText;

    //These store the username and password of the player when they have logged in. For temporary use.
    //Use the UAM to get the syced username and password
    private string playerUsername = "";
    private string playerPassword = "";

    private UserAccountManager UAM;

    private bool isDeployed = false;//whether the game is deployed
    private string deployVersion = "";//what version the deployed game is running

    //Called at the very start of the game
    void Start()
    {
        ResetAllUIElements();
        UAM = UserAccountManager.instance;
    }

    //Called by Button Pressed Methods to Reset UI Fields
    void ResetAllUIElements ()
    {
        //This resets all of the UI elements. It clears all the strings in the input fields and any errors being displayed
        Login_UsernameField.text = "";
        Login_PasswordField.text = "";
        Register_UsernameField.text = "";
        Register_PasswordField.text = "";
        Register_ConfirmPasswordField.text = "";
        Login_ErrorText.text = "";
        Register_ErrorText.text = "";
    }

    //what a guy https://forum.unity.com/threads/hash-function-for-game.452779/
    public static string sha256(string str)
    {
        System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed();
        System.Text.StringBuilder hash = new System.Text.StringBuilder();
        byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(str), 0, Encoding.UTF8.GetByteCount(str));
        foreach (byte bit in crypto)
        {
            hash.Append(bit.ToString("x2"));
        }
        return hash.ToString().ToLower();
    }

    //check version number and whether game is deployed
    private void setGotDeployed(string data)
    {
        deployVersion = UADataTranslator.DataToVersion(data);
        isDeployed = UADataTranslator.DataToDeploy(data);

        StartCoroutine(LoginUser());
    }

    IEnumerator LoginUser()
    {
        if (isDeployed && Application.version.Equals(deployVersion))
        {
            IEnumerator e = DCF.Login(playerUsername, playerPassword); // << Send request to login, providing username and password
            while (e.MoveNext())
            {
                yield return e.Current;
            }
            string response = e.Current as string; // << The returned string from the request

            if (response == "Success")
            {
                //Username and Password were correct. Stop showing 'Loading...' and transition scenes
                ResetAllUIElements();
                //loadingParent.gameObject.SetActive(false);
                UAM.Login(playerUsername, playerPassword);
            }
            else
            {
                //Something went wrong logging in. Stop showing 'Loading...' and go back to LoginUI
                loadingParent.GetComponent<LO_LoadScene>().HideScreen();
                loginUIParent.SetActive(true);
                loginParent.gameObject.SetActive(true);
                if (response == "UserError")
                {
                    //The Username was wrong so display relevent error message
                    Login_ErrorText.text = "Error: Username not Found";
                }
                else
                {
                    if (response == "PassError")
                    {
                        //The Password was wrong so display relevent error message
                        Login_ErrorText.text = "Error: Password Incorrect";
                    }
                    else
                    {
                        //There was another error. This error message should never appear, but is here just in case.
                        Login_ErrorText.text = "Error: Unknown Error. Please try again later.";
                    }
                }
            }
        }
        else//game isn't deployed
        {
            loadingParent.GetComponent<LO_LoadScene>().HideScreen();
            loginUIParent.SetActive(true);
            loginParent.gameObject.SetActive(true);

            if (!isDeployed)
                Login_ErrorText.text = "Error: Servers are closed. Try again later.";
            else if (!Application.version.Equals(deployVersion))
                Login_ErrorText.text = "Error: Your version is out of date. Please update to " + deployVersion + "!";
        }
    }

    //Called by Button Pressed Methods. These use DatabaseControl namespace to communicate with server.
    void TryLoginUser ()
    {
        isDeployed = false;
        deployVersion = "";

        //check if game is deployed
        StartCoroutine(DeployManager.GetData(setGotDeployed));
    }

    IEnumerator RegisterUser()
    {
        IEnumerator e = DCF.RegisterUser(playerUsername, playerPassword, "[KILLS]0/[DEATHS]0/[POINTS]0/"); // << Send request to register a new user, providing submitted username and password. It also provides an initial value for the data string on the account, which is "Hello World".
        while (e.MoveNext())
        {
            yield return e.Current;
        }
        string response = e.Current as string; // << The returned string from the request

        if (response == "Success")
        {
            //Username and Password were valid. Account has been created. Stop showing 'Loading...' and show the loggedIn UI and set text to display the username.
            ResetAllUIElements();
            //loadingParent.gameObject.SetActive(false);
            UAM.Login(playerUsername, playerPassword);
        } else
        {
            //Something went wrong logging in. Stop showing 'Loading...' and go back to RegisterUI
            loadingParent.GetComponent<LO_LoadScene>().HideScreen();
            loginUIParent.SetActive(true);
            registerParent.gameObject.SetActive(true);
            if (response == "UserError")
            {
                //The username has already been taken. Player needs to choose another. Shows error message.
                Register_ErrorText.text = "Error: Username Already Taken";
            } else
            {
                //There was another error. This error message should never appear, but is here just in case.
                Login_ErrorText.text = "Error: Unknown Error. Please try again later.";
            }
        }
    }

    //UI Button Pressed Methods
    public void Login_LoginButtonPressed ()
    {
        //Called when player presses button to Login

        //Get the username and password the player entered
        playerUsername = Login_UsernameField.text;
        int passLength = Login_PasswordField.text.Length;
        playerPassword = sha256(Login_PasswordField.text);

        //make sure username isn't reserved
        if (!playerUsername.Equals("deploy"))
        {
            //Check the lengths of the username and password. (If they are wrong, we might as well show an error now instead of waiting for the request to the server)
            if (playerUsername.Length > 3)
            {
                if (passLength > 5)
                {
                    //Username and password seem reasonable. Change UI to 'Loading...'. Start the Coroutine which tries to log the player in.
                    loginParent.gameObject.SetActive(false);
                    loginUIParent.gameObject.SetActive(false);
                    loadingParent.GetComponent<LO_LoadScene>().LoadLooader();
                    TryLoginUser();
                }
                else
                {
                    //Password too short so it must be wrong
                    Login_ErrorText.text = "Error: Password Incorrect";
                }
            } else
            {
                //Username too short so it must be wrong
                Login_ErrorText.text = "Error: Username too short";
            }
        }
        else
        {
            //using a reserved name. Make it quiet, though, so people don't know it's reserved
            Login_ErrorText.text = "Error: Password Incorrect";
        }
    }
    public void Login_RegisterButtonPressed ()
    {
        //Called when the player hits register on the Login UI, so switches to the Register UI
        ResetAllUIElements();
        loginParent.gameObject.SetActive(false);
        registerParent.gameObject.SetActive(true);
    }

    public void Register_RegisterButtonPressed ()
    {
        //Called when the player presses the button to register

        //Get the username and password and repeated password the player entered
        playerUsername = Register_UsernameField.text;
        int passLength = Register_PasswordField.text.Length;
        playerPassword = sha256(Register_PasswordField.text);
        string confirmedPassword = sha256(Register_ConfirmPasswordField.text);

        //Make sure username and password are long enough
        if (playerUsername.Length > 3)
        {
            if (passLength > 5)
            {
                //Check the two passwords entered match
                if (playerPassword == confirmedPassword)
                {
                    //Username and passwords seem reasonable. Switch to 'Loading...' and start the coroutine to try and register an account on the server
                    registerParent.gameObject.SetActive(false);
                    loginUIParent.gameObject.SetActive(false);
                    loadingParent.GetComponent<LO_LoadScene>().LoadLooader();
                    StartCoroutine(RegisterUser());
                }
                else
                {
                    //Passwords don't match, show error
                    Register_ErrorText.text = "Error: Password's don't Match";
                }
            }
            else
            {
                //Password too short so show error
                Register_ErrorText.text = "Error: Password too Short";
            }
        }
        else
        {
            //Username too short so show error
            Register_ErrorText.text = "Error: Username too Short";
        }
    }
    public void Register_BackButtonPressed ()
    {
        //Called when the player presses the 'Back' button on the register UI. Switches back to the Login UI
        ResetAllUIElements();
        loginParent.gameObject.SetActive(true);
        registerParent.gameObject.SetActive(false);
    }
    
}
