using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class WeaponManager : NetworkBehaviour {

    [SerializeField] private const string remoteLayerName = "RemotePlayer";
    [SerializeField] public List<PlayerWeapon> weapons;
    [SerializeField] private Transform weaponHolder;

    private PlayerWeapon currentWeapon;
    private WeaponGraphics currentGraphics;
    private GameObject weaponInstance;
    private int selectedWeaponIndex = 0;
    public bool isReloading = false;
    public bool isEquipped = false;

	// Use this for initialization
	void Start ()
    {
        currentWeapon = weapons[selectedWeaponIndex];
        weaponInstance = Instantiate(currentWeapon.weaponGFX, weaponHolder.position, weaponHolder.rotation);
        weaponInstance.transform.SetParent(weaponHolder);
        currentGraphics = weaponInstance.GetComponent<WeaponGraphics>();
        if (!isLocalPlayer)
        {
            Util.SetLayerRecursively(weaponInstance, LayerMask.NameToLayer(remoteLayerName));
        }
    }
	
    public void EquipWeapon()
    {
        weaponInstance.SetActive(true);
        isEquipped = true;

    }

    public void DequipWeapon() {
        weaponInstance.SetActive(false);
        isEquipped = false;
    }

    private void SwitchWeapon()
    {
        Destroy(weaponInstance);
        currentWeapon = weapons[selectedWeaponIndex];
        weaponInstance = Instantiate(currentWeapon.weaponGFX, weaponHolder.position, weaponHolder.rotation);
        weaponInstance.transform.SetParent(weaponHolder);
        currentGraphics = weaponInstance.GetComponent<WeaponGraphics>();
        if (isLocalPlayer)
        {
            Util.SetLayerRecursively(weaponInstance, LayerMask.NameToLayer(remoteLayerName));
        }
    }

    public void selectNextWeapon()
    {
        if (selectedWeaponIndex + 1 <= weapons.Capacity)
        {
            selectedWeaponIndex++;
        }
        else
        {
            selectedWeaponIndex = 0;
        }
        SwitchWeapon();
    }

    public void selectPrevWeapon()
    {
        if (selectedWeaponIndex - 1 <= 0)
        {
            selectedWeaponIndex = weapons.Capacity;
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
