using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spitball : MonoBehaviour
{
	[HideInInspector] public WeaponManager weaponManager;
	[HideInInspector] public Player shooter;
	[HideInInspector] public InfectionTool infectionTool;

	//destroy if it touches something
	public void OnTriggerEnter(Collider other)
	{
        Debug.Log("Spitball_OnTriggerEnter");
		if (GameManager.singleton.isServer)
			if (other.CompareTag("Player"))
			{
                Debug.Log("Spit touched player");
				Player touchedPlayer = other.GetComponent<Player>();
				if (touchedPlayer != shooter)
					infectionTool.CmdPlayerSpitInfected(touchedPlayer.transform.name, shooter.transform.name, gameObject);
			}
			else
			{
				Destroy(this);
			}
	}

    private void OnDestroy()
    {
        Debug.Log("On Destroy - Spitball");
    }

    //not working. I think it's some concurrency issue with WeaponManager lines 65-67ish (RemoveWeapon destroying this then resetting currentweapon)
    /*void OnDestroy()
	{
		//copied from PlayerShoot.cs RpcDoHitEffect
		if (weaponManager.getCurrentWeaponGraphics() != null)
		{
			Player player = GameManager.getLocalPlayer();
			GameObject hitEffect = Instantiate(weaponManager.getCurrentWeaponGraphics().hitEffectPrefab, transform.position, Quaternion.LookRotation(player.transform.position - transform.position, Vector3.up));

			//Destroy the effect after it has played to clean up memory
			Destroy(hitEffect, 3f);
		}
	}*/
}
