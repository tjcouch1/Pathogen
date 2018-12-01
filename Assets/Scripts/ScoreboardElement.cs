using UnityEngine;
using UnityEngine.UI;

public class ScoreboardElement : MonoBehaviour
{
    [SerializeField]
    Text usernameText;

    [SerializeField]
    Text karmaText;

    [SerializeField]
    Text killsText;

    [SerializeField]
    Text deathsText;

    public void Setup(string username, int karma, int kills, int deaths)
    {
        usernameText.text = " " + username + " ";
        karmaText.text = " Trust Factor: " + karma + " ";
        killsText.text = " Kills: " + kills + " ";
        deathsText.text = " Deaths: " + deaths + " ";
    }

}
