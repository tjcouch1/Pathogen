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
	private Player localPlayer;
	private string submissionText;

	// Use this for initialization
	void Start () {
		inputToSubmit = GetComponent<InputField>();
		eSystem = EventSystem.current;
		localPlayer = GameManager.getLocalPlayer();
		inputToSubmit.onEndEdit.AddListener(delegate { EndEdit(inputToSubmit); });
	}
	
	// Update is called once per frame
	void Update ()
	{

		if (Input.GetButtonDown("Submit"))
		{
			if (eSystem.currentSelectedGameObject == gameObject)//if the player is typing, send it
			{
				inputToSubmit.text = submissionText;
				GetComponentInParent<bl_ChatManager>().SendChatText(inputToSubmit);
				eSystem.SetSelectedGameObject(null);
			}
		}
		if (Input.GetButtonDown("Chat") && !localPlayer.shouldPreventInput)//open the chat box
		{
			localPlayer.CmdPlayerSetTyping(true);

			if (eSystem != null)
			{
				//select it
				inputToSubmit.ActivateInputField();
				eSystem.SetSelectedGameObject(gameObject, new BaseEventData(eSystem));

				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}
	}

	void EndEdit(InputField input)
	{
		submissionText = input.text;
		input.text = "";
		input.DeactivateInputField();
		localPlayer.CmdPlayerSetTyping(false);

		if (GameManager.singleton.inCurrentRound)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		else
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}
}
