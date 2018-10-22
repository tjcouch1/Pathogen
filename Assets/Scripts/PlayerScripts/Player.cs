using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(PlayerSetup))]
public class Player : NetworkBehaviour {

    [SyncVar]
    private bool _isAlive = true;

    [SyncVar]
    private bool isInfected = false;     //Bool for storing what team Player is on. Default is human
	
    [SerializeField] private InfectionTool infectionTool;

	[SyncVar]
	private bool _isTyping = false;//when true, player is typing into text chat

	public bool isTyping
	{
		get { return _isTyping; }
		protected set { _isTyping = value; }
	}

	public bool shouldPreventInput//when true, should prevent typing
	{
		get { return PauseMenu.isOn || isTyping; }
	}

    //Getter/Setter for isAlive
    public bool isAlive
    {
        get { return _isAlive; }
        protected set { _isAlive = value; }
    }

    [SerializeField]
    private int maxHealth = 100;
    [SyncVar]
    private int currentHealth;
    [SyncVar]
    public string username = "(null)";

    public int killCount;               //Killcount local to this match
    public int deathCount;              //Death count local to this match
    public int points;                  //Points local to this match

    [SerializeField]
    private Behaviour[] disableOnDeath;
    [SerializeField]
    private GameObject[] disableGOnDeath;
    private bool[] wasEnabled;
    private bool firstSetup = true;

	[HideInInspector]
	public bl_ChatManager chatManager;

    public void Setup()
    {
        //Start all players in the lobby as soon as they join
        CmdSendPlayerToLobby();
    }

    [ClientRpc]
    public void RpcTakeDamage(int amount, string sourceID)
    {
        if (!isAlive)
        {
            return;
        }
        currentHealth -= amount;
        Debug.Log(transform.name + " took " + amount + " points of damage from " + sourceID);
        if (currentHealth <= 0)
        {
            Die(sourceID);
        }
    }

    public void SetInfected(bool value)
    {
        if (value)
        {
            isInfected = true;
            infectionTool.Setup();
            GameManager.singleton.RegisterNewInfected(this);
            Debug.Log(username + " is infected!");
        }
        else
        {
            isInfected = false;
            infectionTool.Disable();
        }
    }

    public bool GetInfectedState()
    {
        if (isInfected)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

	public override void OnStartClient()
	{
		base.OnStartClient();
		GetComponentInChildren<AudioListener>().enabled = false;
	}

	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer();
		GameObject.Find("_VoiceChat").GetComponent<VoiceChat.VoiceChatRecorder>().clientPlayer = this;
		GetComponentInChildren<AudioListener>().enabled = true;
		GetComponent<AudioSource>().enabled = false;
	}

	private void Die(string killerID)
    {
        isAlive = false;

		if (string.IsNullOrEmpty(killerID) && killerID != "Quarantine")
			try
			{
				Player sourcePlayer = GameManager.getPlayer(killerID);
				if (sourcePlayer != null)
				{
					sourcePlayer.killCount++;
					GameManager.singleton.CallOnDeathCallbacks(transform.name, sourcePlayer.username);
				}
			}catch(KeyNotFoundException e)
			{
				GameManager.singleton.CallOnDeathCallbacks(transform.name, killerID);
			}

        deathCount++;

		//set dead player's voice chat to dead
		if (!isLocalPlayer)
			GetComponent<VoiceChat.VoiceChatPlayer>().SetAlive(false);
		else chatManager.SetAlive(false);

        CmdSendPlayerToLobby(); 
        Debug.Log(transform.name + " has died. ");

    }

    [Command]
    public void CmdSendPlayerToLobby()
    {
        RpcSendPlayerToLobby();
    }

    [ClientRpc]
    private void RpcSendPlayerToLobby()
    {
        //Disable components on player
        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = false;
        }

        //Disable GameObjects on player
        for (int i = 0; i < disableGOnDeath.Length; i++)
        {
            disableGOnDeath[i].SetActive(false);
        }

        //Disable collider on player
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        //Disable Physics on player
        Rigidbody rb = GetComponent<Rigidbody>();
        if(rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        //Switch cameras
        if (isLocalPlayer)
        {
            GameManager.singleton.SetSceneCameraActive(true);
            GetComponent<PlayerSetup>().playerUIInstance.GetComponent<PlayerUI>().LobbyMode(true);
        }
    }

    //Called everytime a new round starts
    [Command]
    public void CmdRespawnPlayer()
    {
        RpcRespawn();
    }

    [ClientRpc]
    private void RpcRespawn()
    {
        isAlive = true;
        currentHealth = maxHealth;

        //Enable the GameObjects
        for (int i = 0; i < disableGOnDeath.Length; i++)
        {
            disableGOnDeath[i].SetActive(true);
        }

        //Enable the collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }

        //Enable Physics on player
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        Transform respawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;

        if (isLocalPlayer)
        {
            //Enable the components
            for (int i = 0; i < disableOnDeath.Length; i++)
            {
                disableOnDeath[i].enabled = true;
            }

            GetComponent<PlayerSetup>().playerUIInstance.GetComponent<PlayerUI>().LobbyMode(false);
            GameManager.singleton.SetSceneCameraActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

			chatManager.SetAlive(true);
		}
		else//set the voice chat player's falloff back to live falloff
			GetComponent<VoiceChat.VoiceChatPlayer>().SetAlive(true);

		Debug.Log(transform.name + " has respawned.");
    }

	[Command]
	public void CmdPlayerSetTyping(bool typing)
	{
		isTyping = typing;
	}

    public float getHealth()
    {
        return (float)currentHealth / maxHealth;
	}

	//check if players enter the quarantine zone
	[Server]
	public void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<Collider>().CompareTag("QuarantineZone"))
			RpcTakeDamage(maxHealth, "Quarantine");
	}
}
