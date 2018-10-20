using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(WeaponManager))]
public class InfectionTool : NetworkBehaviour {

    [SerializeField] private Camera cam;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private PlayerWeapon infectionTool;
    [SerializeField] private LayerMask mask;
    
    private PlayerWeapon currentWeapon;

    private bool isSetup = false;

    public void Setup () {
        Debug.Log("Infection tool setup was called for " + gameObject.name);
        weaponManager.PickupWeapon(infectionTool);
        isSetup = true;
    }

    void Update () {

        if (PauseMenu.isOn)
            return;

        if (!isSetup)
            return;
        
        currentWeapon = weaponManager.getCurrentWeapon();

        if (currentWeapon.Equals(infectionTool))
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Infect();
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

    [Command]
    void CmdPlayerInfected(string playerID, string sourceID)
    {
        //Debug.Log(playerID + " has been shot");

        Player player = GameManager.getPlayer(playerID);
        player.SetInfected(true);
    }
}
