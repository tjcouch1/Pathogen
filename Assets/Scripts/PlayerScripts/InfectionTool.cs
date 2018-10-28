using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(WeaponManager))]
public class InfectionTool : NetworkBehaviour {

	[SerializeField] private Camera cam;
	[SerializeField] private WeaponManager weaponManager;
	[SerializeField] private GameObject spitPrefab;
	[SerializeField] private LayerMask mask;

    private PlayerWeapon infectionTool;
    private PlayerWeapon spitInfectTool;

    private bool isSetup = false;

	public void Setup() {
		Debug.Log("Infection tool setup was called for " + gameObject.name);
		weaponManager.PickupWeapon("Infect");
        infectionTool = weaponManager.getWeaponByName("Infect");
		weaponManager.PickupWeapon("SpitInfect");
        spitInfectTool = weaponManager.getWeaponByName("SpitInfect");
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
		return weaponManager.getCurrentWeapon().Equals(infectionTool);
	}

	public bool isSpitEquipped()
	{
		return weaponManager.getCurrentWeapon().Equals(spitInfectTool);
	}

    public void Disable()
    {
        isSetup = false;
        if(weaponManager != null)
        {
            weaponManager.RemoveWeapon(infectionTool);
			weaponManager.RemoveWeapon(spitInfectTool);
		}
    }

    [Client]
    public void Infect()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        RaycastHit hit;
        Vector3 aim = cam.transform.forward;
        if (Physics.Raycast(cam.transform.position, aim, out hit, infectionTool.range, mask))
        {
            //We hit something
            if (hit.collider.tag == "Player")
            {
                CmdPlayerInfected(hit.collider.name, transform.name);
                NotificationsManager.instance.CreateNotification("Infection", "You infected a player by touch! +5 points");
            }
        }
	}

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

	[Command]
    void CmdPlayerInfected(string playerID, string sourceID)
    {
        //Debug.Log(playerID + " has been shot");

        Player player = GameManager.getPlayer(playerID);
        player.SetInfected(true);
	}

	[Command]
	public void CmdPlayerSpitInfected(string playerID, string sourceID, GameObject spit)
	{
		Player player = GameManager.getPlayer(playerID);
		player.SetInfected(true);
		Destroy(spit);
	}
}
