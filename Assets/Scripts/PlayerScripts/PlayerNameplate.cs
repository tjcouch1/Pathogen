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

    private void Update()
    {
        Camera cam = Camera.main;

        usernameText.text = "<b>" + player.username + "</b>";
        healthFill.localScale = new Vector3(player.getHealth(), 1, 1);

        //Billboarding function to rotate nameplate towards player camera
        transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);

        //Debug.Log("PushToTalkSprite " + (PushToTalkSprite != null) + " AudioSource " + (GetComponentInParent<AudioSource>() != null));
        if (!GetComponentInParent<Player>().isLocalPlayer)
            if (GetComponentInParent<AudioSource>() != null)
            {
                if (PushToTalkSprite.GetComponent<UIShowHide>().Hidden && GetComponentInParent<AudioSource>().isPlaying)
                    PushToTalkSprite.GetComponent<UIShowHide>().Hidden = false;
                else if (!PushToTalkSprite.GetComponent<UIShowHide>().Hidden && !GetComponentInParent<AudioSource>().isPlaying)
                    PushToTalkSprite.GetComponent<UIShowHide>().Hidden = true;
            }
            else Debug.Log("Didn't find AudioSource!");
    }
}
