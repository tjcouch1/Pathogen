using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject scoreboard;
    [SerializeField] private GameObject infectedUI;
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

    private void Start()
    {
        PauseMenu.isOn = false;
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

        SetHealthAmount(player.getHealth());
        SetAmmoAmount(weaponManager.getCurrentWeapon().bullets, weaponManager.getCurrentWeapon().clips);
        SetInfected(player.isInfected);
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
