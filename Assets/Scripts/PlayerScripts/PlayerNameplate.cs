using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameplate : MonoBehaviour {

    [SerializeField]
    private Text usernameText;

    [SerializeField]
    private RectTransform healthFill;

    [SerializeField]
    private Player player;

    [SerializeField]
    private GameObject PushToTalkSprite;
	private UIShowHide PushToTalkSH;

	[SerializeField]
	private GameObject TypingSprite;
	private UIShowHide TypingSH;

	private void Start()
	{
		PushToTalkSH = PushToTalkSprite.GetComponent<UIShowHide>();
		TypingSH = TypingSprite.GetComponent<UIShowHide>();
	}

	private void Update()
    {
        Camera cam = Camera.main;

        usernameText.text = "<b>" + player.username + "</b>";
        healthFill.localScale = new Vector3(player.getHealth(), 1, 1);

        //Billboarding function to rotate nameplate towards player camera
        transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);

		//show or hide various images
		if (!GetComponentInParent<Player>().isLocalPlayer)
		{
			//show or hide voice chat image
			if (GetComponentInParent<AudioSource>() != null)
			{
				if (PushToTalkSH.Hidden && GetComponentInParent<AudioSource>().isPlaying)
					PushToTalkSH.Hidden = false;
				else if (!PushToTalkSH.Hidden && !GetComponentInParent<AudioSource>().isPlaying)
					PushToTalkSH.Hidden = true;
			}
			else Debug.Log("Didn't find AudioSource!");

			//show or hide text chat message image
			if (TypingSH.Hidden && player.isTyping)
			{
				Player localPlayer = GameManager.getLocalPlayer();
				//if you're dead or the typing person is alive, show it
				if (!localPlayer.isAlive || player.isAlive)
					TypingSH.Hidden = false;
			}
			else if (!TypingSH.Hidden && !player.isTyping)
				TypingSH.Hidden = true;
		}
    }
}
