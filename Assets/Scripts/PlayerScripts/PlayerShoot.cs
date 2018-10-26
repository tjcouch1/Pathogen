using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(WeaponManager))]
public class PlayerShoot : NetworkBehaviour {

    [SerializeField]
    private Camera cam;
    [SerializeField]
    private LayerMask mask;
    [SerializeField] private WeaponManager weaponManager;
    private PlayerWeapon currentWeapon;
    private float sprayTime = 0;                //How long the player has been spraying for
    private float lastCallToApplySpray = 0;     //I hope this variable name is descriptive enough
    [SerializeField]                            //Because world units are so big, we need a multiplier to bring down the size
    private float sprayMultiplier = 0.1f;
	private float shootTime = 0.0f; //the amount of time in seconds until the next shot may be fired

    private void Start()
    {
        //weaponManager = GetComponent<WeaponManager>();
        if(cam == null)
        {
            Debug.LogError("PlayerShoot: No Camera referenced!");
            this.enabled = false;
        }
    }

    private void Update()
    {
        currentWeapon = weaponManager.getCurrentWeapon();

        if (PauseMenu.isOn || !GameManager.singleton.inCurrentRound)
            return;

        var scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel > 0f)
        {
            // scroll up
            weaponManager.selectPrevWeapon();
        }
        else if (scrollWheel < 0f)
        {
            // scroll down
            weaponManager.selectNextWeapon();
        }

        if (Input.GetButtonDown("Reload"))
        {
            weaponManager.reload();
            return;
        }

		//deal with when the weapon can shoot
		if (shootTime > 0)
		{
			shootTime -= Time.deltaTime;
			if (shootTime < 0f)
				shootTime = 0f;
		}

		if (shootTime <= 0 && ((!currentWeapon.automatic && Input.GetButtonDown("Fire1")) || (currentWeapon.automatic && Input.GetButton("Fire1"))))
		{
			if (currentWeapon.fireRate > 0)
				shootTime = 1f / currentWeapon.fireRate;
			Shoot();
		}
    }

    //Called on server when player hits someting
    [Command]
    void CmdOnHit(Vector3 hitPos, Vector3 normal)
    {
        RpcDoHitEffect(hitPos, normal);
    }
    
    //Update all the clients to display hit effect
    [ClientRpc]
    void RpcDoHitEffect(Vector3 pos, Vector3 normal)
    {
		//copied in Spitball.cs OnDestroy
        if(weaponManager.getCurrentWeaponGraphics() != null)
        {
            GameObject hitEffect = Instantiate(weaponManager.getCurrentWeaponGraphics().hitEffectPrefab, pos, Quaternion.LookRotation(normal));

            //Destroy the effect after it has played to clean up memory
            Destroy(hitEffect, 1f);
        }   
    }

    //Called on the server when the player shoots
    [Command]
    void CmdOnShoot()
    {
        RpcDoMuzzleFlash();
    }

    //IS called on ALL clients when we need to display a shoot effect
	/// <summary>
	/// Duplicated in InfectionTool.cs
	/// </summary>
    [ClientRpc]
    void RpcDoMuzzleFlash()
    {
        if (weaponManager.getCurrentWeaponGraphics() != null)
        {
            if(weaponManager.getCurrentWeaponGraphics().muzzleFlash != null)
            {
                weaponManager.getCurrentWeaponGraphics().muzzleFlash.Play();
            }
        }
    }

    [Client]
    private void Shoot()
    {
        if (!isLocalPlayer || weaponManager.isReloading)
        {
            return;
        }

		//let infection script handle shooting if it's one of those weapons
		InfectionTool infectionTool = GetComponent<InfectionTool>();
		if (infectionTool.isInfectEquipped())
		{
			infectionTool.Infect();
			return;
		}
		else if (infectionTool.isSpitEquipped())
		{
			infectionTool.SpitInfect();
			return;
		}

        if (!currentWeapon.infiniteAmmo && currentWeapon.bullets == 0)
        {
            //Debug.Log("OUT OF AMMO");
            return;
        }

		if (!currentWeapon.infiniteAmmo)
			currentWeapon.bullets--;
        //Debug.Log("Bullets: " + currentWeapon.bullets);

        //Call OnShoot method on the server
        CmdOnShoot();
        RaycastHit hit;
        Vector3 aim = cam.transform.forward;
        if (Physics.Raycast(cam.transform.position, ApplySpray(aim), out hit, currentWeapon.range, mask) )
        {
            //We hit something
            if(hit.collider.tag == "Player")
            {
                CmdPlayerShot(hit.collider.name, currentWeapon.damage, transform.name);
            }

            //Play OnHit effects on server
            CmdOnHit(hit.point, hit.normal);
        }

    }

    [Command]
    void CmdPlayerShot(string playerID, int damage, string sourceID)
    {
        //Debug.Log(playerID + " has been shot");

        Player player = GameManager.getPlayer(playerID);
        player.RpcTakeDamage(damage, sourceID);
    }

    private Vector3 ApplySpray(Vector3 fV)
    {
        //Calculates time since the player last tried to shoot something.
        float deltaTime = Time.time - lastCallToApplySpray;
        lastCallToApplySpray = Time.time;

        //SprayTime is a variable to keep track of how long the player has been spraying. Higher values = more inacuracy
        sprayTime -= deltaTime*currentWeapon.sprayHealRate;
        sprayTime = Mathf.Clamp(sprayTime, 0, currentWeapon.maxInacuracy);        

        //Debug.Log(sprayTime);

        //Basic variance
        fV = new Vector3(fV.x + Random.Range(-sprayTime, sprayTime) * sprayMultiplier, fV.y + Random.Range(-sprayTime, sprayTime) * sprayMultiplier, fV.z);
        fV.Normalize();
        sprayTime += currentWeapon.sprayDecayRate;
        

        return fV;
    }
}
