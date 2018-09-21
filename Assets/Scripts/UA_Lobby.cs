using UnityEngine;
using UnityEngine.UI;

public class UA_Lobby : MonoBehaviour {

    public Text usernameText;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (UserAccountManager.IsLoggedIn)
        {
            usernameText.text = "Welcome, " + UserAccountManager.playerUsername + "! ";
        }
    }

    public void LogOutButtonPressed()
    {
        if(UserAccountManager.IsLoggedIn)
            UserAccountManager.instance.Logout();
    }
}
