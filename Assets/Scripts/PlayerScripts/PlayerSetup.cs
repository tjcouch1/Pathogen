using UnityEngine;
using UnityEngine.Networking;

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
            playerUIInstance = Instantiate(playerUIprefab);
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

			chatManager = FindObjectOfType<bl_ChatManager>();

            GetComponent<Player>().Setup();
            Debug.Log("Local Player Set Up!");
        }
	}

	public void SetUpChatUI(string username)
	{
		Player player = GetComponent<Player>();
		Debug.Log("Setup ChatUI! Name:" + username);
		GameObject textChatCanvas = chatManager.gameObject.transform.parent.gameObject;
		chatManager.gameObject.transform.SetParent(playerUIInstance.transform, true);
		Object.Destroy(textChatCanvas);
		chatManager.SetPlayerName(username, true);
		isChatUISetUp = true;
	}

	[Command]
    void CmdSetUsername(string playerID, string username)
    {
        Player player = GameManager.getPlayer(playerID);
        if(player != null)
        {
            Debug.Log(username + " has joined the game!");
            player.username = username;
			RpcSetupChatUI(playerID, username);
        }
    }

	[ClientRpc]
	void RpcSetupChatUI(string playerID, string username)
	{
		if (!isChatUISetUp && transform.name == playerID)
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
