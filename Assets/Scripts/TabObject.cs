using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//TabObject: determines what object to tab to when pressed. Does not manage tabbing
public class TabObject : MonoBehaviour
{
	//what to tab to and back tab to
	public GameObject backTabTo;
    public GameObject tabTo;
	public bool selectOnEnable = false;

	void OnEnable()
	{
		if (selectOnEnable)
			TabManager.SelectWithInput (GetComponent<Selectable> ());
	}
}
