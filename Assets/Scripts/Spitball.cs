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
		if (GameManager.singleton.isServer)
			if (other.CompareTag("Player"))
			{
				Player touchedPlayer = other.GetComponent<Player>();
				if (!touchedPlayer.isLocalPlayer)
					infectionTool.CmdPlayerSpitInfected(touchedPlayer.transform.name, shooter.transform.name, gameObject);
			}
			else
			{
				Debug.Log("Spit destroyed on something other than player");
				Destroy(this);
			}
	}
}
