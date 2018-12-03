using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

[NetworkSettings(channel = 8, sendInterval = 0.1f)]
public class WeaponManager : NetworkBehaviour {

    [SerializeField] private const string remoteLayerName = "RemotePlayer";
    [SerializeField] private const string dontDrawLayerName = "DontDraw";
    [SerializeField] private WeaponGraphics localRife;
    [SerializeField] private WeaponGraphics localKnife;
    [SerializeField] private List<PlayerWeapon> weaponPrefabs;
    [SerializeField] private Transform weaponHolder;

    //Maps each weapon to its instance
    [SerializeField] private List<WeaponInstancePair> weapons;

    [SyncVar] [SerializeField] private int selectedWeaponIndex = 0;//serialized for debug reasons
    public bool isReloading = false;

    private PlayerAudio playerAudio;

    // Use this for initialization
    void Start()
    {
        playerAudio = GetComponent<PlayerAudio>();
		setupDefaultWeapons();

        if (!isLocalPlayer)
        {
            Util.SetLayerRecursively(localRife.gameObject, LayerMask.NameToLayer(dontDrawLayerName));
            Util.SetLayerRecursively(localKnife.gameObject, LayerMask.NameToLayer(dontDrawLayerName));
        }
       
	}

	/// <summary>
	/// destroys all weapons and creates default weapons
	/// Currently called in an Rpc, so all clients run this code
	/// </summary>
	public void setupDefaultWeapons()
	{
		List<WeaponInstancePair> weaponsToRemove = new List<WeaponInstancePair>(weapons);
		foreach (WeaponInstancePair pair in weaponsToRemove)
            if (pair != null && pair.weapon != null)
			    RemoveWeapon(pair.weapon);
		weapons = new List<WeaponInstancePair>();
		foreach (PlayerWeapon w in weaponPrefabs)
		{
			if (w.isDefault)
				SpawnWeapon(w, false);
		}
		//weapons[selectedWeaponIndex].weaponInstance.SetActive(true);
        CmdRequestWeaponSwitch(selectedWeaponIndex);
	}

	/// <summary>
	/// Spawns the weapon with supplied name on the current player. If select is true, selects the weapon immediately
	/// </summary>
	/// <param name="weaponName"></param>
	/// <param name="select"></param>
	[ClientRpc]
	public void RpcSpawnWeapon(string weaponName, bool select)
	{
        Debug.Log("Weapon: " + weaponName + " spawned!");
		PlayerWeapon weaponPrefab = getWeaponPrefabByName(weaponName);
		if (weaponPrefab != null)
			SpawnWeapon(weaponPrefab, select);
		else Debug.LogError("Weapon with name " + weaponName + " does not exist! RpcSpawnWeapon");
	}

	/// <summary>
	/// Spawns the playerweapon on the current player. Is select is true, selects the weapon immediately. Otherwise deactivates it.
	/// </summary>
	/// <param name="w"></param>
	/// <param name="select"></param>
	/// <returns></returns>
	//needs to run on all clients
    private void SpawnWeapon(PlayerWeapon w, bool select)
	{
		GameObject wGraphicsInstance = null;

		//create graphics instance
		if (w.weaponGFX != null)
        {
			wGraphicsInstance = Instantiate(w.weaponGFX, weaponHolder.position, weaponHolder.rotation);
			wGraphicsInstance.transform.SetParent(weaponHolder);
            if (!isLocalPlayer)
            {
                Util.SetLayerRecursively(wGraphicsInstance, LayerMask.NameToLayer(remoteLayerName));
            }
            else if (isLocalPlayer)
            {
                Util.SetLayerRecursively(wGraphicsInstance, LayerMask.NameToLayer(dontDrawLayerName));
            }
        }
        else
        {
            Debug.Log("Weapon " + w.weaponName + " has no graphics");
        }

		//create playerweapon
		WeaponInstancePair newWeapon = new WeaponInstancePair(w, wGraphicsInstance);
		weapons.Add(newWeapon);
		w.weaponSetup();

		//select the weapon right off the bat
		if (select)
			CmdRequestWeaponSwitch(weapons.IndexOf(newWeapon));
		else//don't select the weapon - make it inactive
			if (wGraphicsInstance != null)
				wGraphicsInstance.SetActive(false);
	}

	[ClientRpc]
	public void RpcRemoveWeapon(string weaponName)
	{
		PlayerWeapon weaponPrefab = getWeaponByName(weaponName);
		if (weaponPrefab != null)
			RemoveWeapon(weaponPrefab);
		//else Debug.LogError("Weapon with name " + weaponName + " does not exist! RpcRemoveWeapon");
	}

	//needs to run on all clients
    private void RemoveWeapon(PlayerWeapon weapon)
    {
        foreach(WeaponInstancePair pair in weapons)
        {
            if(pair.weapon == weapon)
			{
				//TODO: if player is holding weapon to remove, set his weapon to default
				//currently returns null when client infects server for win (maybe more cases) probably because it calls RemoveWeapon multiple times before selectedWeaponIndex has a chance to network update. Therefore it destroys something then tries to rely on it for next time
				//Debug.Log(getCurrentWeapon().weaponName + " " + weapon.weaponName);
				//if (getCurrentWeapon().weaponName.Equals(weapon.weaponName))
				{
					//Debug.Log("Switching");
					CmdRequestWeaponSwitch(0);
				}
				Destroy(pair.weaponInstance);
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
        RpcSwitchWeapon(selectedWeaponIndex, newWeaponIndex);
		selectedWeaponIndex = newWeaponIndex;
	} 

    [ClientRpc]
    public void RpcSwitchWeapon(int deactivateIndex, int requestedIndex)
    {
		if (weapons.Count > deactivateIndex)
			if (weapons[deactivateIndex].weaponInstance != null)
			{
				weapons[deactivateIndex].weaponInstance.SetActive(false);
			}
		
		if (weapons[requestedIndex].weaponInstance != null)
		{
			weapons[requestedIndex].weaponInstance.SetActive(true);
		}

        if (isLocalPlayer)
        {
            SwitchLocalWeapon(deactivateIndex, requestedIndex);
        }

        playerAudio.PlaySwap();
    }

    //AGH. BAD CODE! but its fine for now shhh don't look at it :)
    private void SwitchLocalWeapon(int deactivateIndex, int requestedIndex)
    {
        //Deactivate local guy
        if (weapons[deactivateIndex].weapon.weaponName == "Rifle")
        {
            localRife.gameObject.SetActive(false);
        }
        else if (weapons[deactivateIndex].weapon.weaponName == "Knife")
        {
            localKnife.gameObject.SetActive(false);
        }

        //Activate local guy
        if (weapons[requestedIndex].weapon.weaponName == "Rifle")
        {
            localRife.gameObject.SetActive(true);
        }
        else if (weapons[requestedIndex].weapon.weaponName == "Knife")
        {
            localKnife.gameObject.SetActive(true);
        }
    }

    public PlayerWeapon getCurrentWeapon()
    {
        try
        {
            var weapon = weapons[selectedWeaponIndex].weapon;
            return weapon;
        }catch(ArgumentOutOfRangeException)
        {
            Debug.LogError("Get current weapon returned null");
            return null;
        }
    }

    public PlayerWeapon getWeaponByName(string Name)
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i].weapon.weaponName == Name)
            {
                return weapons[i].weapon;
            }
        }
        Debug.Log("Player " + GetComponent<Player>().username + " does not have the weapon: " + Name);
        return null;
	}

	public PlayerWeapon getWeaponPrefabByName(string Name)
	{
		for (int i = 0; i < weaponPrefabs.Count; i++)
		{
			if (weaponPrefabs[i].weaponName == Name)
			{
				return weaponPrefabs[i];
			}
		}
		Debug.Log("This player does not have the weapon: " + Name);
		return null;
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

    public void DoMuzzleFlash()
    {
        var gfx = getCurrentWeaponGraphics();
        if (gfx != null)
        {
            if (gfx.muzzleFlash != null)
            {
                gfx.muzzleFlash.Play();
            }
        }

        //Play local weapon flash
        if (isLocalPlayer)
        {
            if(weapons[selectedWeaponIndex].weapon.weaponName == "Rifle")
            {
                localRife.muzzleFlash.Play();
            }

            //Would put knife particle effect here if it had one
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
                playerAudio.PlayReload();
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
