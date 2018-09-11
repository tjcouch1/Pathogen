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

    private void Update()
    {
        Camera cam = Camera.main;

        usernameText.text = "<b>" + player.username + "</b>";
        healthFill.localScale = new Vector3(1, player.getHealth(), 1);

        //Billboarding function to rotate nameplate towards player camera
        transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);
    }
}
