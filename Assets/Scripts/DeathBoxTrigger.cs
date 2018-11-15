using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBoxTrigger : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}

	//check if players enter the death zone
	public void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<Collider>().CompareTag("Player"))
		{
			Player touchedPlayer = other.GetComponent<Player>();
			if (touchedPlayer.isLocalPlayer)
				touchedPlayer.CmdTakeDamage(touchedPlayer.maxHealth, "Falling out of the map");
		}
	}
}
