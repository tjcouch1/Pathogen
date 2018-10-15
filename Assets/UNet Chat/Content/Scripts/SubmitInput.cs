using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Add this to the input desired to make it send the text when clicked enter in
/// </summary>
public class SubmitInput : MonoBehaviour, IDeselectHandler, ISubmitHandler {

	private InputField inputToSubmit;
	private static EventSystem eSystem;

	// Use this for initialization
	void Start () {
		inputToSubmit = GetComponent<InputField>();
		eSystem = EventSystem.current;
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetButtonDown("Submit"))
		{
			if (inputToSubmit.isFocused)//if the player is typing, send it
			{
				Debug.Log("Woo " + inputToSubmit.text);
				GetComponentInParent<bl_ChatManager>().SendChatText(inputToSubmit);
				eSystem.SetSelectedGameObject(null);
			}
			else//if the player isn't typing, select it
			{
				inputToSubmit.interactable = true;
				GameManager.getLocalPlayer().isTyping = true;

				if (eSystem != null)
				{
					//select it
					inputToSubmit.ActivateInputField();
					eSystem.SetSelectedGameObject(gameObject, new BaseEventData(eSystem));
				}
			}
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		Debug.Log("wowee");
		inputToSubmit.interactable = false;
		GameManager.getLocalPlayer().isTyping = false;
	}

	public void OnSubmit(BaseEventData eventData)
	{
		Debug.Log("Woo " + inputToSubmit.text);
		GetComponentInParent<bl_ChatManager>().SendChatText(inputToSubmit);
		eSystem.SetSelectedGameObject(null);
	}
}
