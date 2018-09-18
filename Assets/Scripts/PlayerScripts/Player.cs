using System.Collections;
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
        get { return _isAlive;  }
        protected set { _isAlive = value;  }
    }

    [SerializeField]
    private int maxHealth = 100;
    [SerializeField]
    private int respawnTimer = 3;
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

    //Called by player setup script
    public void Setup()
    {
        if (isLocalPlayer) {
            //Switch cameras
            GameManager.singleton.SetSceneCameraActive(false);
            GetComponent<PlayerSetup>().playerUIInstance.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
        }
        CmdBroadcastNewPlayerSetup();
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

        resetDefaults();
    }

    private void Update()
    {
        if (!isLocalPlayer)
            return;

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

    private void Die(string killerID)
    {
        isAlive = false;

        Player sourcePlayer = GameManager.getPlayer(killerID);
        if(sourcePlayer != null)
        {
            sourcePlayer.killCount++;
            GameManager.singleton.CallOnDeathCallbacks(transform.name, sourcePlayer.username);
        }

        deathCount++;

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

        //Switch cameras
        if (isLocalPlayer)
        {
            GameManager.singleton.SetSceneCameraActive(true);
            GetComponent<PlayerSetup>().playerUIInstance.SetActive(false);
        }

        Debug.Log(transform.name + " has died. ");

        //Call Respawn Method
        //StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTimer);

        Transform respawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;

        Setup();

        Debug.Log(transform.name + " has respawned.");
    }

    public void resetDefaults()
    {
        isAlive = true;
        currentHealth = maxHealth;

        //Enable the components
        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = wasEnabled[i];
        }

        //Enable the GameObjects
        for (int i = 0; i < disableGOnDeath.Length; i++)
        {
            disableGOnDeath[i].SetActive(true); 
        }

        //Enable the collider
        Collider col = GetComponent<Collider>();
        if(col != null)
        {
            col.enabled = true;
        }

    }

    public float getHealth()
    {
        return (float)currentHealth / maxHealth;
    }
}
