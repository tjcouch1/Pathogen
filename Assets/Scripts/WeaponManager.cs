using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class WeaponManager : NetworkBehaviour {

    [SerializeField] private const string remoteLayerName = "RemotePlayer";
    [SerializeField] private List<PlayerWeapon> defaultWeapons;
    [SerializeField] private Transform weaponHolder;

    //Maps each weapon to its instance
    private List<KeyValuePair<PlayerWeapon, GameObject>> weapons;

    private int selectedWeaponIndex = 0;
    public bool isReloading = false;

    // Use this for initialization
    void Start()
    {
        weapons = new List<KeyValuePair<PlayerWeapon, GameObject>>();
        foreach (PlayerWeapon w in defaultWeapons)
        {
            var instance = SpawnWeapon(w);
            weapons.Add(new KeyValuePair<PlayerWeapon, GameObject>(w, instance));
            if (instance != null)
                instance.SetActive(false);
        }
        weapons[selectedWeaponIndex].Value.SetActive(true);
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

    public void PickupWeapon(PlayerWeapon weapon)
    {
        weapons.Add(new KeyValuePair<PlayerWeapon, GameObject>(weapon, SpawnWeapon(weapon)));
        Debug.LogWarning("Pickup weapon was called for " + gameObject.name);
    }

    public void RemoveWeapon(PlayerWeapon weapon)
    {
        foreach(KeyValuePair<PlayerWeapon, GameObject> pair in weapons)
        {
            if(pair.Key == weapon)
            {
                Destroy(pair.Value);
                NetworkServer.Destroy(pair.Value);
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
        if (weapons[selectedWeaponIndex].Value != null)
            weapons[selectedWeaponIndex].Value.SetActive(false);
        if (weapons[requestedIndex].Value != null)
            weapons[requestedIndex].Value.SetActive(true);

        if (isLocalPlayer)
        {
            selectedWeaponIndex = requestedIndex;
        }
    }

    public PlayerWeapon getCurrentWeapon()
    {
        try
        {
            var weapon = weapons[selectedWeaponIndex].Key;
            return weapon;
        }catch(ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    public WeaponGraphics getCurrentWeaponGraphics()
    {
        var weaponGFX = weapons[selectedWeaponIndex].Value.GetComponent<WeaponGraphics>();
        if(weaponGFX != null)
        {
            return weaponGFX;
        }
        else
        {
            Debug.Log("Weapon " + weapons[selectedWeaponIndex].Key.weaponName + " has no graphics");
            return null;
        }
    }

    public void reload()
    {
        if (isReloading)
            return;

        var currentWeapon = weapons[selectedWeaponIndex].Key;

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
        var currentGraphics = weapons[selectedWeaponIndex].Value;
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
            Debug.Log("Weapon " + weapons[selectedWeaponIndex].Key.weaponName + " has no graphics");
        }
	}

	/// <summary>
	/// sets all weapons to max ammo on each local player
	/// </summary>
	//TODO: Austin, when you make it so the player finds weapons around, make sure to remove the weapons and load up defaultWeapons again please
	public void RefreshAllWeapons()
	{
		if (isLocalPlayer)
		{
			foreach (KeyValuePair<PlayerWeapon, GameObject> w in weapons)
			{
				w.Key.clips = w.Key.startingClips;
				w.Key.bullets = w.Key.clipSize;
			}
		}
	}

}
