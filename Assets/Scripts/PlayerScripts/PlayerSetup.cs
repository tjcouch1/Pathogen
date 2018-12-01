using UnityEngine;
using UnityEngine.Networking;

[NetworkSettings(channel = 6, sendInterval = 0.1f)]
[RequireComponent(typeof(Player))]
public class PlayerSetup : NetworkBehaviour {

    [SerializeField]
    private Behaviour[] componentsToDisable;
    [SerializeField]
    private string remoteLayerName = "RemotePlayer";
    [SerializeField]
    private string dontDrawLayerName = "DontDraw";
    [SerializeField]
    private GameObject playerGraphics;
    [SerializeField]
    private GameObject playerUIprefab;
    [HideInInspector] public GameObject playerUIInstance;

	private bl_ChatManager chatManager;
	private bool isChatUISetUp = false;

	void Start()
	{
		if (!isLocalPlayer)
        {
            DisableComponents();
            AssignRemoteLayer();
        }
        else
        {
            //Disable Player graphics for local player
            Util.SetLayerRecursively(playerGraphics, LayerMask.NameToLayer(dontDrawLayerName));

            //Create player UI
            playerUIInstance = FindObjectOfType<PlayerUI>().gameObject;
            playerUIInstance.name = playerUIprefab.name;

            //Configure Player UI
            PlayerUI ui = playerUIInstance.GetComponent<PlayerUI>();
            if (ui == null)
                Debug.LogError("No PlayerUI component on PlayerUI prefab");
            ui.SetPlayer(GetComponent<Player>());

			string _username = "(null)";
            if (UserAccountManager.IsLoggedIn)
                _username = UserAccountManager.playerUsername;
            else
                _username = transform.name;

            CmdSetUsername(transform.name, _username);

			chatManager = playerUIInstance.GetComponentInChildren<bl_ChatManager>();
			GetComponent<Player>().chatManager = chatManager;

            GetComponent<Player>().Setup();
            Debug.Log("Local Player Set Up!");
        }
	}

	public void SetUpChatUI(string username)
	{
		//Player player = GetComponent<Player>();
		Debug.Log("Setup ChatUI! Name:" + username);
		chatManager.SetPlayerName(username, true);
		chatManager.SetAlive(false);
		isChatUISetUp = true;

		//Set the Chat UI 
		chatManager.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 80);

        //Setup Notifications UI
        playerUIInstance.GetComponent<PlayerUI>().EnableUIOnDeathCallback();
        playerUIInstance.GetComponentInChildren<KillFeed>().SetupKillFeed();
	}

	[Command]
    void CmdSetUsername(string playerID, string username)
    {
        Player player = GameManager.getPlayer(playerID);
        if(player != null)
        {
            player.username = username;
			RpcSetupChatUI(playerID, username);
        }
        RpcNewPlayerNotification(playerID);
    }

    [ClientRpc]
    void RpcNewPlayerNotification(string playerID)
    {
        Player _player = GameManager.getPlayer(playerID);
        Debug.Log(_player.username + " has joined the game!");
        if (!_player.isLocalPlayer)
        {
            NotificationsManager.instance.CreateNotification(_player.username, "Has joined the game!");
        }
    }

	[ClientRpc]
	void RpcSetupChatUI(string playerID, string username)
	{
		if (GetComponent<Player>().isLocalPlayer && !isChatUISetUp && transform.name == playerID)
			SetUpChatUI(username);
	}

    public override void OnStartClient()
    {
        base.OnStartClient();
        string netID = GetComponent<NetworkIdentity>().netId.ToString();
        Player _player = GetComponent<Player>();

        GameManager.RegisterPlayer(netID, _player);
    }

    private void AssignRemoteLayer()
    {
        Util.SetLayerRecursively(gameObject, LayerMask.NameToLayer(remoteLayerName));
    }

    private void DisableComponents()
    {
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }
    }

    private void OnDisable()
    {
        Destroy(playerUIInstance);

        if (isLocalPlayer)
            GameManager.singleton.SetSceneCameraActive(true);

        GameManager.UnRegisterPlayer(transform.name);
    }
}
