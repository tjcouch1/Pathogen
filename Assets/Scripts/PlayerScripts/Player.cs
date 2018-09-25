using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(PlayerSetup))]
public class Player : NetworkBehaviour {

    [SyncVar]
    private bool _isAlive = true;

    [SyncVar]
    public bool isInfected = false;     //Bool for storing what team Player is on. Default is human

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

    public void Setup()
    {
        //CmdBroadcastNewPlayerSetup();
        CmdSendPlayerToLobby();
    }

    //Tell the server that a new player has spawned
    [Command]
    private void CmdBroadcastNewPlayerSetup()
    {
        RpcSetupPlayerOnAllClients();
    }

    //Update all the clients with the new GameObjects data
    [ClientRpc]
    private void RpcSetupPlayerOnAllClients()
    {
        if (firstSetup)
        {
            //Player setup logic
            wasEnabled = new bool[disableOnDeath.Length];
            for (int i = 0; i < wasEnabled.Length; i++)
            {
                wasEnabled[i] = disableOnDeath[i].enabled;
            }
            firstSetup = false;
        }

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

	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer();
		GameObject.Find("_VoiceChat").GetComponent<VoiceChat.VoiceChatRecorder>().clientPlayer = this;
    }

    private void Die(string killerID)
    {
        isAlive = false;

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

    [Command]
    public void CmdRespawnPlayer()
    {
        RpcRespawn();
    }

    [ClientRpc]
    private void RpcRespawn()
    {
        resetDefaults();
        //Enable the GameObjects
        for (int i = 0; i < disableGOnDeath.Length; i++)
        {
            disableGOnDeath[i].SetActive(true);
        }

        Transform respawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;

        if (isLocalPlayer)
        {
            GetComponent<PlayerSetup>().playerUIInstance.GetComponent<PlayerUI>().LobbyMode(false);
            GameManager.singleton.SetSceneCameraActive(false);
        }
        Debug.Log(transform.name + " has respawned.");
    }

    private void resetDefaults()
    {
        isAlive = true;
        currentHealth = maxHealth;

        //Enable the collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }

        if (isLocalPlayer)
        {
            //Enable the components
            for (int i = 0; i < disableOnDeath.Length; i++)
            {
                disableOnDeath[i].enabled = true;
            }   

            //Enable Physics on player
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.detectCollisions = true;
            }

            //Disable scene camera for local player
            GameManager.singleton.SetSceneCameraActive(false);
        }
    }

    public float getHealth()
    {
        return (float)currentHealth / maxHealth;
    }
}
