using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject scoreboard;
    [SerializeField] private GameObject infectedUI;
    [SerializeField] private Image weaponImage;
    [SerializeField] private RectTransform healthFill;
    [SerializeField] private Text ammoText;
    [SerializeField] private Text timerTitle;
    [SerializeField] private Text timerText;
    [SerializeField] private Text audioPositionsText;
    [SerializeField] private bool showAudioPositions = false;
    [SerializeField] private GameObject pushToTalkSprite;
    [SerializeField] private GameObject[] disableWhileInLobby;

    public Color infectedColor;
    public Color healthyColor;

    private Player player;
    private WeaponManager weaponManager;
    private NotificationsManager notificationsManager;

	private void Start()
    {
        PauseMenu.isOn = false;
        notificationsManager = NotificationsManager.instance;
        GameManager.singleton.onPlayerKilledCallbacks.Add(UIOnDeathCallback);
    }

    public void SetPlayer(Player _player)
    {
        player = _player;
        weaponManager = player.GetComponent<WeaponManager>();
    }

    public void LobbyMode(bool state)
    {
        foreach(GameObject g in disableWhileInLobby)
        {
            g.SetActive(!state);
        }
    }

		// Update is called once per frame
	void Update () {

        if(weaponManager == null){
            //Weapon manager not set up yet. Need to wait until SetPlayer is called
            return;
        }

        if(weaponManager.getCurrentWeapon() == null)
        {
            return;
        }

        //Proabably a better way of doing this, but this is the list of overrides for weapons/tools that don't have bullets
        switch (weaponManager.getCurrentWeapon().weaponName)
        {
            case "Infect":
                ammoText.text = "Infect";
                break;
            case "Knife":
                ammoText.text = "Knife";
                break;
            case "Holster":
                ammoText.text = "Holster";
                break;
            default:
                SetAmmoAmount(weaponManager.getCurrentWeapon().bullets, weaponManager.getCurrentWeapon().clips);
                break;
        }

        SetHealthAmount(player.getHealth());
        weaponImage.sprite = weaponManager.getCurrentWeapon().weaponIcon;
        SetInfected(player.GetInfectedState());
        UpdateTimer();
        UpdateAudioPositionsText();
        UpdatePushToTalkSprite();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            scoreboard.SetActive(true);
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            scoreboard.SetActive(false);
        }
	}

    private void SetInfected(bool state)
    {
        Image hf = healthFill.GetComponent<Image>();
        if (hf == null)
        {
            Debug.LogError("Can't find Image component on health bar");
            return;
        }

        if (state)
        {
            hf.color = infectedColor;
            infectedUI.SetActive(true);
        }
        else
        {
            hf.color = healthyColor;
            infectedUI.SetActive(false);
        }
    }

    public void TogglePauseMenu()
    {
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        PauseMenu.isOn = pauseMenu.activeSelf;
        if (PauseMenu.isOn)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (!PauseMenu.isOn)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    //TO-DO: Refactor this into the points system
    //Anything that the UI needs to do on death happens here
    public void UIOnDeathCallback(string playerName, string sourceName)
    {
        //Our player killed someone
        if(sourceName == player.username)
        {
            var killedPlayer = GameManager.getPlayer(playerName);
            if(killedPlayer.GetInfectedState() == player.GetInfectedState())
            {
                //Player teamkilled! Bad bad!
                notificationsManager.CreateNotification("Teamkill!", "You killed someone on your own team! -10 karma");
            }
            else
            {
                //Player was infected and killed a healthy person
                if(player.GetInfectedState() == true)
                {
                    notificationsManager.CreateNotification("Killed Healthy", "You killed a potential host. Infect healthy players, don't kill them. -5 karma");
                }
                //Player was healthy and killed an infected person
                else 
                {
                    notificationsManager.CreateNotification("Killed Infected", "You killed an infected! +10 karma");
                }
            }
        }
        //Our player got killed
        else if(playerName == player.username){
            notificationsManager.CreateNotification("You were killed by: ", sourceName);
        }
        
    }

    void SetHealthAmount(float health)
    {
        healthFill.localScale = new Vector3(1, health, 1);
    }

    void SetAmmoAmount(int bullets, int clips)
    {
        ammoText.text = bullets.ToString() + " / " + clips.ToString();
    }

    void UpdateAudioPositionsText()
    {
        if (showAudioPositions)
        {
            audioPositionsText.text = "";
            var listener = player.GetComponentInChildren<AudioListener>();
            if (listener != null)
                audioPositionsText.text += "Listener X: " + listener.transform.position.x + " Y: " + listener.transform.position.y + " Z: " + listener.transform.position.z;

            foreach (Player p in GameManager.getAllPlayers())
            {
                var proxy = p.gameObject.GetComponentInChildren<AudioSource>();
                if (proxy != null)
                {
                    if (!p.isLocalPlayer && proxy != null)
                    {
                        if (!audioPositionsText.text.Equals(""))
                            audioPositionsText.text += "\n";
                        audioPositionsText.text += p.username + " " + proxy.clip.name + " X: " + proxy.transform.position.x + " Y: " + proxy.transform.position.y + " Z: " + proxy.transform.position.z;
                    }

                    audioPositionsText.text += " Audio MaxDistance: " + proxy.maxDistance;
                }
            }
        }
    }

    void UpdatePushToTalkSprite()
    {
        //update the VOIP icon
        if (pushToTalkSprite.GetComponent<UIShowHide>().Hidden && VoiceChat.VoiceChatRecorder.Instance.IsPushingToTalk)
            pushToTalkSprite.GetComponent<UIShowHide>().Hidden = false;
        else if (!pushToTalkSprite.GetComponent<UIShowHide>().Hidden && !VoiceChat.VoiceChatRecorder.Instance.IsPushingToTalk)
            pushToTalkSprite.GetComponent<UIShowHide>().Hidden = true;
    }

    private void UpdateTimer()
    {
        int time = GameTimer.singleton.getRoundTime();
        timerTitle.text = GameTimer.singleton.getRoundTitle();
        timerText.text = ("" + Mathf.Floor(time / 60.00f).ToString("0") + ":" + Mathf.Floor(time % 60.00f).ToString("00"));
    }

    public void updateTimerColor(Color c)
    {
        timerTitle.color = c;
    }
}
