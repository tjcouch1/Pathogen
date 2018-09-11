using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject scoreboard;
    [SerializeField] private RectTransform healthFill;
    [SerializeField] private Text ammoText;
    [SerializeField] private Text timerTitle;
    [SerializeField] private Text timerText;

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

    // Update is called once per frame
    void Update () {

        SetHealthAmount(player.getHealth());
        SetAmmoAmount(weaponManager.getCurrentWeapon().bullets, weaponManager.getCurrentWeapon().clips);
        UpdateTimer();

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
