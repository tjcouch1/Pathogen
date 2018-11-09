using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[NetworkSettings(channel = 4, sendInterval = 0.1f)]
[RequireComponent(typeof(WeaponManager))]
public class InfectionTool : NetworkBehaviour {

	[SerializeField] private Camera cam;
	[SerializeField] private WeaponManager weaponManager;
	[SerializeField] private GameObject spitPrefab;
	[SerializeField] private LayerMask mask;

	[SerializeField] private const string infectToolName = "Infect";
	[SerializeField] private const string spitInfectToolName = "SpitInfect";

	private bool isSetup = false;

	[Server]
	public void Setup() {
		Debug.Log("Infection tool setup was called for " + gameObject.name);
		weaponManager.RpcSpawnWeapon(infectToolName, false);
		weaponManager.RpcSpawnWeapon(spitInfectToolName, false);
		isSetup = true;
	}

    void Update() {

		if (PauseMenu.isOn)
			return;

		if (!isSetup)
			return;
	}

    public bool isInfectEquipped()
	{
        return weaponManager.getCurrentWeapon().weaponName.Equals(infectToolName);
	}

	public bool isSpitEquipped()
	{
        return weaponManager.getCurrentWeapon().weaponName.Equals(spitInfectToolName);
	}
	
	[Server]
    public void Disable()
    {
        isSetup = false;
        if(weaponManager != null)
        {
            weaponManager.RpcRemoveWeapon(infectToolName);
			weaponManager.RpcRemoveWeapon(spitInfectToolName);
		}
    }

	//precondition: infectionTool must be set up before this code runs
    [Client]
    public void Infect()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        RaycastHit hit;
		PlayerWeapon infectTool = weaponManager.getWeaponByName(infectToolName);
        Vector3 aim = cam.transform.forward;
        if (Physics.Raycast(cam.transform.position, aim, out hit, infectTool.range, mask))
        {
            //We hit something
            if (hit.collider.tag == "Player")
            {
                //If that player is already infected
                if (hit.transform.gameObject.GetComponent<Player>().GetInfectedState())
                {
                    NotificationsManager.instance.CreateNotification("Infection", "This player is already infected!");
                }
                else
                {
                    CmdPlayerInfected(hit.collider.name, transform.name);
                }

            }
        }
	}

	//precondition: spitInfectionTool must be set up before this code runs
	[Client]
	public void SpitInfect()
	{
		if (!isLocalPlayer)
		{
			return;
		}

		//make spitball
		PlayerWeapon currentWeapon = weaponManager.getCurrentWeapon();
		Vector3 aim = cam.transform.TransformDirection(new Vector3(Random.Range(-currentWeapon.maxInacuracy, currentWeapon.maxInacuracy), Random.Range(-currentWeapon.maxInacuracy, currentWeapon.maxInacuracy), currentWeapon.range));
		CmdSpit(aim);
	}

	/// <summary>
	/// make the spitball on server and send down to clients
	/// </summary>
	/// <param name="direction">Direction to send the spitball including velocity (not a unit vector)</param>
	[Command]
	void CmdSpit(Vector3 direction)
	{
		//create spit object
		GameObject spit = Instantiate(spitPrefab, cam.transform);
		Spitball spitball = spit.GetComponent<Spitball>();
		spitball.weaponManager = weaponManager;
		spitball.shooter = GetComponent<Player>();
		spitball.infectionTool = this;
		spit.GetComponent<Rigidbody>().velocity = direction;
		NetworkServer.Spawn(spit);
		Destroy(spit, 5.0f);

		//particle effects
		RpcDoMuzzleFlash();
	}

	//IS called on ALL clients when we need to display a shoot effect
	/// <summary>
	/// Duplicated from PlayerShoot.cs
	/// </summary>
	[ClientRpc]
	void RpcDoMuzzleFlash()
	{
		if (weaponManager.getCurrentWeaponGraphics() != null)
		{
			if (weaponManager.getCurrentWeaponGraphics().muzzleFlash != null)
			{
				weaponManager.getCurrentWeaponGraphics().muzzleFlash.Play();
			}
		}
	}

    //Callled when a player gets infected by touch
	[Command]
    void CmdPlayerInfected(string playerID, string sourceID)
    {
        Debug.Log("Cmd Player Touch Infected. playerID: " + playerID);
        Player player = GameManager.getPlayer(playerID);
        player.SetInfected(true);
        RpcInfectionNotification(playerID, sourceID);
	}

	[Command]
	public void CmdPlayerSpitInfected(string playerID, string sourceID, GameObject spit)
	{
		Debug.Log("Cmd Player Spit Infected. playerID: " + playerID);
		Player player = GameManager.getPlayer(playerID);
		player.SetInfected(true);
		Destroy(spit);
        RpcInfectionNotification(playerID, sourceID);
    }

    [ClientRpc]
    public void RpcInfectionNotification(string playerID, string sourceID)
    {
        Player source = GameManager.getPlayer(sourceID);
        Player target = GameManager.getPlayer(playerID);

        //If target == our player
        if (target.isLocalPlayer)
        {
            NotificationsManager.instance.CreateNotification("Infection", "You were Infected!");
        }

        //If our player is the one who did the infecting...
        if (source.isLocalPlayer)
        {
            NotificationsManager.instance.CreateNotification("Infection", "You infected a player! +5 points");
        }
    }

    [ClientRpc]
    public void RpcFailedNotificationForShooter(string shooter)
    {
        Player s = GameManager.getPlayer(shooter);
        if (s.isLocalPlayer)
        {
            NotificationsManager.instance.CreateNotification("Infection", "This player is already infected!");
        }
    }
}
