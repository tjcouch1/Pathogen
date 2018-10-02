using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class WeaponManager : NetworkBehaviour {

    [SerializeField] private const string remoteLayerName = "RemotePlayer";
    [SerializeField] private List<PlayerWeapon> defaultWeapons;
    [SerializeField] private Transform weaponHolder;

    //Maps each weapon to its instance
    private List<KeyValuePair<PlayerWeapon, GameObject>> weapons;

    private int selectedWeaponIndex = 0;
    public bool isReloading = false;
    public bool isEquipped = false;

	// Use this for initialization
	void Start ()
    {
        weapons = new List<KeyValuePair<PlayerWeapon, GameObject>>();
        foreach (PlayerWeapon w in defaultWeapons)
        {
            var instance = SpawnWeapon(w);
            weapons.Add(new KeyValuePair<PlayerWeapon, GameObject>(w, instance));
        }
    }

    private GameObject SpawnWeapon(PlayerWeapon w)
    {
        if (w.weaponGFX != null)
        {
            var w_instance = Instantiate(w.weaponGFX, weaponHolder.position, weaponHolder.rotation);
            w_instance.transform.SetParent(weaponHolder);
            if (!isLocalPlayer)
            {
                Util.SetLayerRecursively(w_instance, LayerMask.NameToLayer(remoteLayerName));
            }
            NetworkServer.Spawn(w_instance);
            return w_instance;
        }
        else
        {
            Debug.Log("Weapon " + w.weaponName + " has no graphics");
            return null;
        }
    }
	
    public void PickupWeapon(PlayerWeapon weapon)
    {
        weapons.Add(weapon, SpawnWeapon(weapon));
    }

    private void SwitchWeapon()
    {
        
    }

    public void selectNextWeapon()
    {
        try
        {



        }catch(KeyNotFoundException k)
        {

        }
    }

    public void selectPrevWeapon()
    {
        if (selectedWeaponIndex - 1 < 0)
        {
            selectedWeaponIndex = weapons.Capacity - 1;
        }
        else
        {
            selectedWeaponIndex--;
        }
        SwitchWeapon();
    }

    public PlayerWeapon getCurrentWeapon()
    {
        return currentWeapon;
    }

    public WeaponGraphics getCurrentWeaponGraphics()
    {
        return currentGraphics;
    }

    public void reload()
    {
        if (isReloading)
            return;

        if(currentWeapon.bullets == currentWeapon.clipSize)
        {
            Debug.Log("Clip Already full. Not reloading");
            return;
        }

        isReloading = true;

        StartCoroutine(reloadCoroutine());
    }

    private IEnumerator reloadCoroutine()
    {
        if (currentWeapon.clips > 0)
        {
            Debug.Log("Reloading...");
            CmdOnReload();
            yield return new WaitForSeconds(currentWeapon.reloadTime);
            currentWeapon.bullets = currentWeapon.clipSize;
            currentWeapon.clips--;
        }
        else
        {
            Debug.Log("No Magazines left");
        }
        isReloading = false;
    }

    [Command]
    void CmdOnReload()
    {
        RpcOnReload();
    }

    [ClientRpc]
    void RpcOnReload()
    {
        if(currentGraphics != null)
        {
            Animator anim = currentGraphics.GetComponent<Animator>();
            anim.SetTrigger("Reload");
        }
    }
}
