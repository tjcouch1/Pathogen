using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(WeaponManager))]
public class InfectionTool : NetworkBehaviour {

    [SerializeField] private Camera cam;
    [SerializeField] private PlayerWeapon infectionTool;
    [SerializeField] private LayerMask mask;

    private WeaponManager weaponManager;
    private PlayerWeapon currentWeapon;
    

    void Start () {
        weaponManager = GetComponent<WeaponManager>();
        weaponManager.PickupWeapon(infectionTool);
    }

    void Update () {

        if (PauseMenu.isOn)
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

    private void OnDisable()
    {
        if(weaponManager != null)
        {
            weaponManager.ResetWeaponsToDefaults();
        }
    }

    [Client]
    private void Infect()
    {
        if (!isLocalPlayer || !weaponManager.isEquipped)
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
        player.RpcGetInfected();
    }
}
