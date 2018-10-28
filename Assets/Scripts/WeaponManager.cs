using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class WeaponManager : NetworkBehaviour {

    [SerializeField] private const string remoteLayerName = "RemotePlayer";
    [SerializeField] private List<PlayerWeapon> weaponPrefabs;
    [SerializeField] private Transform weaponHolder;

    //Maps each weapon to its instance
    private List<WeaponInstancePair> weapons;

    [SyncVar] private int selectedWeaponIndex = 0;//serialized for debug reasons
    public bool isReloading = false;

    // Use this for initialization
    void Start()
    {
        weapons = new List<WeaponInstancePair>();
        foreach (PlayerWeapon w in weaponPrefabs)
        {
            if (w.isDefault)
            {
                var instance = SpawnWeapon(w);
                weapons.Add(new WeaponInstancePair(w, instance));
                if (instance != null)
                    instance.SetActive(false);
            }
        }
        weapons[selectedWeaponIndex].weaponInstance.SetActive(true);
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
            NetworkServer.SpawnWithClientAuthority(w_instance, gameObject);
            return w_instance;
        }
        else
        {
            Debug.Log("Weapon " + w.weaponName + " has no graphics");
            return null;
        }
    }

    public PlayerWeapon PickupWeapon(string WeaponName)
    {
        Debug.LogWarning("Pickup weapon was called for " + WeaponName);
        if (isLocalPlayer)
        {
            for(int i = 0; i < weaponPrefabs.Count; i++)
            {
                if(weaponPrefabs[i].weaponName == WeaponName)
                {
                    weapons.Add(new WeaponInstancePair(weaponPrefabs[i], SpawnWeapon(weaponPrefabs[i])));
                    CmdPickupWeapon(i);
                    return weapons[i].weapon;
                }
            }
            Debug.LogWarning("Weapon " + WeaponName + " is not a valid weapon prefab!");
            return null;
        }
        Debug.LogWarning("Pickup Weapon called on non-local player!");
        return null;
    }

    [Command]
    void CmdPickupWeapon(int index)
    {
        RpcPickupWeapon(index);
    }

    [ClientRpc]
    void RpcPickupWeapon(int index)
    {
        weapons.Add(new WeaponInstancePair(weaponPrefabs[index], SpawnWeapon(weaponPrefabs[index])));
    }

    public void RemoveWeapon(PlayerWeapon weapon)
    {
        foreach(WeaponInstancePair pair in weapons)
        {
            if(pair.weapon == weapon)
			{
				//TODO: fix this so it doesn't reset alive players' weapons to 0 every time
				//if (getCurrentWeapon() == weapon)
				//{
					selectedWeaponIndex = 0;
					CmdRequestWeaponSwitch(0);
				//}
				Destroy(pair.weaponInstance);
                NetworkServer.Destroy(pair.weaponInstance);
				weapons.Remove(pair);
                break;
            }
        }
    }

    public void selectNextWeapon()
    {
        if (selectedWeaponIndex + 1 >= weapons.Count)
        {
            CmdRequestWeaponSwitch(0);
        }
        else
        {
            CmdRequestWeaponSwitch(selectedWeaponIndex + 1);
        }
    }

    public void selectPrevWeapon()
    {
        if (selectedWeaponIndex - 1 < 0)
        {
            CmdRequestWeaponSwitch(weapons.Count - 1);
        }
        else
        {
            CmdRequestWeaponSwitch(selectedWeaponIndex-1);
        }        
    }

    [Command]
    public void CmdRequestWeaponSwitch(int newWeaponIndex)
    {
        RpcSwitchWeapon(newWeaponIndex);
    } 

    [ClientRpc]
    public void RpcSwitchWeapon(int requestedIndex)
    {
        if (weapons[selectedWeaponIndex].weaponInstance != null)
            weapons[selectedWeaponIndex].weaponInstance.SetActive(false);

        if (weapons[requestedIndex].weaponInstance != null)
            weapons[requestedIndex].weaponInstance.SetActive(true);

        if (isLocalPlayer)
        {
            CmdupdateSelectedWeaponIndex(requestedIndex);
        }
    }

    [Command]
    private void CmdupdateSelectedWeaponIndex(int requestedIndex)
    {
        selectedWeaponIndex = requestedIndex;
    }

    public PlayerWeapon getCurrentWeapon()
    {
        try
        {
            var weapon = weapons[selectedWeaponIndex].weapon;
            return weapon;
        }catch(ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    public WeaponGraphics getCurrentWeaponGraphics()
    {
        var weaponGFX = weapons[selectedWeaponIndex].weaponInstance.GetComponent<WeaponGraphics>();
        if(weaponGFX != null)
        {
            return weaponGFX;
        }
        else
        {
            Debug.Log("Weapon " + weapons[selectedWeaponIndex].weapon.weaponName + " has no graphics");
            return null;
        }
    }

    public void reload()
    {
        if (isReloading)
            return;

        var currentWeapon = weapons[selectedWeaponIndex].weapon;

        if(currentWeapon.bullets == currentWeapon.clipSize)
        {
            Debug.Log("Clip Already full. Not reloading");
            return;
        }

        isReloading = true;

        StartCoroutine(reloadCoroutine(currentWeapon));
    }

    private IEnumerator reloadCoroutine(PlayerWeapon currentWeapon)
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
        var currentGraphics = weapons[selectedWeaponIndex].weaponInstance;
        if(currentGraphics != null)
        {
            try
            {
                Animator anim = currentGraphics.GetComponent<Animator>();
                anim.SetTrigger("Reload");
            }
            catch (MissingReferenceException)
            {
                Debug.Log("The weapon instance of " + currentGraphics.name + " does not have an animator component attached");
            }
        }
        else
        {
            Debug.Log("Weapon " + weapons[selectedWeaponIndex].weapon.weaponName + " has no graphics");
        }
	}

	/// <summary>
	/// sets all weapons to max ammo on each local player
	/// </summary>
	public void RefreshAllWeapons()
	{
		if (isLocalPlayer)
		{
            weapons = new List<WeaponInstancePair>();
            foreach (PlayerWeapon w in weaponPrefabs)
            {
                if (w.isDefault)
                {
                    var instance = SpawnWeapon(w);
                    weapons.Add(new WeaponInstancePair(w, instance));
                    if (instance != null)
                        instance.SetActive(false);
                }
            }
        }
	}

}

[System.Serializable]
public class WeaponInstancePair
{
    public PlayerWeapon weapon;
    public GameObject weaponInstance;

    public WeaponInstancePair(PlayerWeapon w, GameObject instance)
    {
        weapon = w;
        weaponInstance = instance;
    }
}
