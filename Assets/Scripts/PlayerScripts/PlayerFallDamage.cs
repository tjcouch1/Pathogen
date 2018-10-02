using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerFallDamage : MonoBehaviour {

    private bool isGrounded = true;
    private float oldTrans;
    private Player player;

    [SerializeField] private float fallThreshold = 2.0f;
    [SerializeField] private float damagePerUnitFall = 5.0f;

    private void Start()
    {
        player = gameObject.GetComponent<Player>();
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.contacts.Length == 0)
        {
            isGrounded = false;
            oldTrans = gameObject.transform.localPosition.y;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!isGrounded)
        {
            isGrounded = true;
            checkFallDamage();
        }
    }

    private void checkFallDamage()
    {
        float deltaY = Mathf.Abs(oldTrans - gameObject.transform.position.y);
        //Debug.Log("Player Fell " + deltaY);
        if (deltaY >= fallThreshold)
        {
            float damage = deltaY * damagePerUnitFall;
            player.RpcTakeDamage((int)damage, "Fall Damage");
        }
    }
}
