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
		weaponManager.PickupWeapon("SpitInfect");
		isSetup = true;
	}

	void Update() {

		if (PauseMenu.isOn)
			return;

		if (!isSetup)
			return;
	}

    private void linkInfectionTools()
    {
        if(infectionTool == null)
            infectionTool = weaponManager.getWeaponByName("Infect");
        if(spitInfectTool == null)
            spitInfectTool = weaponManager.getWeaponByName("SpitInfect");
    }

    public bool isInfectEquipped()
	{
        linkInfectionTools();
        return weaponManager.getCurrentWeapon().Equals(infectionTool);
	}

	public bool isSpitEquipped()
	{
        linkInfectionTools();
        return weaponManager.getCurrentWeapon().Equals(spitInfectTool);
	}

	//TODO: this won't be called anymore when we fix the server disable stuff. Gotta set infectionTool and spitInfectionTool to null some other way
    public void Disable()
    {
        linkInfectionTools();
        isSetup = false;
        if(weaponManager != null)
        {
            weaponManager.RemoveWeapon(infectionTool);
			weaponManager.RemoveWeapon(spitInfectTool);
            infectionTool = null;
            spitInfectTool = null;
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
        Debug.Log("Comand Spit");
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
        Debug.Log("Cmd Player Spit infected");
		Player player = GameManager.getPlayer(playerID);
		player.SetInfected(true);
		Destroy(spit);
	}
}
