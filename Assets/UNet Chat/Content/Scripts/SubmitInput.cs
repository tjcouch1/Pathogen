using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Add this to the input desired to make it send the text when clicked enter in
/// </summary>
public class SubmitInput : MonoBehaviour {

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
			if (eSystem.currentSelectedGameObject == gameObject)//if the player is typing, send it
			{
				GetComponentInParent<bl_ChatManager>().SendChatText(inputToSubmit);
				inputToSubmit.DeactivateInputField();
				eSystem.SetSelectedGameObject(null);

				GameManager.getLocalPlayer().isTyping = false;
			}
		}
		if (Input.GetButtonDown("Chat") && !GameManager.getLocalPlayer().shouldPreventInput)//open the chat box
		{
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
