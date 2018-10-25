using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(WeaponManager))]
public class InfectionTool : NetworkBehaviour {

    [SerializeField] private Camera cam;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private PlayerWeapon infectionTool;
	[SerializeField] private PlayerWeapon spitInfectTool;
	[SerializeField] private GameObject spitPrefab;
    [SerializeField] private LayerMask mask;
    
    private PlayerWeapon currentWeapon;

    private bool isSetup = false;

    public void Setup () {
        Debug.Log("Infection tool setup was called for " + gameObject.name);
		weaponManager.PickupWeapon(infectionTool);
		weaponManager.PickupWeapon(spitInfectTool);
		isSetup = true;
    }

    void Update () {

        if (PauseMenu.isOn)
            return;

        if (!isSetup)
            return;
        
        currentWeapon = weaponManager.getCurrentWeapon();

		bool hasInfect = currentWeapon.Equals(infectionTool);
		bool hasSpit = currentWeapon.Equals(spitInfectTool);
        if (hasInfect || hasSpit)
        {
            if (Input.GetButtonDown("Fire1"))
            {
				if (hasInfect)
					Infect();
				else if (hasSpit)
					SpitInfect();
            }
        }
    }

    public void Disable()
    {
        isSetup = false;
        if(weaponManager != null)
        {
            weaponManager.RemoveWeapon(infectionTool);
        }
    }

    [Client]
    private void Infect()
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
		Vector3 aim = cam.transform.TransformDirection(new Vector3(Random.Range(-currentWeapon.maxInacuracy, currentWeapon.maxInacuracy), Random.Range(-currentWeapon.maxInacuracy, currentWeapon.maxInacuracy), spitInfectTool.range));
		CmdSpit(aim);
	}

	/// <summary>
	/// make the spitball on server and send down to clients
	/// </summary>
	/// <param name="direction">Direction to send the spitball including velocity (not a unit vector)</param>
	[Command]
	void CmdSpit(Vector3 direction)
	{
		GameObject spit = Instantiate(spitPrefab);
		Spitball spitball = spit.GetComponent<Spitball>();
		spitball.weaponManager = weaponManager;
		spitball.shooter = GetComponent<Player>();
		spitball.infectionTool = this;
		spit.GetComponent<Rigidbody>().velocity = direction;
		spit.transform.rotation = Quaternion.LookRotation(Vector3.Normalize(direction), Vector3.up);

		NetworkServer.Spawn(spit);

		Destroy(spit, 5.0f);
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
