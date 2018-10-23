using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuarantineZoneTrigger : MonoBehaviour {

	// Use this for initialization
	void Start () {
		gameObject.SetActive(false);
	}
	
	//check if players enter the quarantine zone
	public void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<Collider>().CompareTag("Player"))
		{
			Player touchedPlayer = other.GetComponent<Player>();
			if (touchedPlayer.isLocalPlayer)
				touchedPlayer.CmdTakeDamage(touchedPlayer.maxHealth, "Quarantine");
		}
	}
}
