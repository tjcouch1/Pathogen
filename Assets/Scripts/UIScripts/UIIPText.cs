using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UIIPText : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent<Text>().text += NetworkManager.singleton.networkAddress;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
