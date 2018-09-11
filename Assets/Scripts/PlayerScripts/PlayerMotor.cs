using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour {

    public bool invertMouse = false;
    public bool GunOut
    {
        get { return animator.GetBool("GunOut"); }
        set { animator.SetBool("GunOut", value); }
    }

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private Animator animator;

    private Vector3 velocity;
    private Vector3 rotation;
    private float tiltX = 0f;
    private float currentCamRotX = 0;
    private Rigidbody rb;
    private bool canJump = true;

    [SerializeField]
    private float cameraRotationLimit = 85f;
    [SerializeField]
    private float runningThreshold = 2.0f;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (animator == null)
        {
            Debug.LogError("NO ANIMATOR ATTACHED TO PLAYER MOTOR!");
        }
    }

    //Updates the value of velocity from the playerController
    public void move(Vector3 v)
    {
        velocity = v;
        if(v.magnitude > 0.1)
        {
            animator.SetBool("isMoving", true);
            if (v.magnitude >= runningThreshold)
            {
                animator.SetBool("isRunning", true);
            }
            else
            {
                animator.SetBool("isRunning", false);
            }
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }

    //Pass in rotation about the Y and then rotation about the X
    public void rotate(Vector3 r, float t)
    {
        rotation = r;
        tiltX = -t;
        if (invertMouse)
        {
            tiltX *= -1;
        }
    }

    public void Jump(float force)
    {
        if (canJump)
        {
            rb.AddForce(Vector3.up * force);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        canJump = true;
        animator.SetBool("jumping", false);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.contacts.Length == 0)
        {
            canJump = false;
            animator.SetBool("jumping", true);
        }
    }

    //Runs on every physics iteration
    private void FixedUpdate()
    {
        perfromMovement();
        performRotation();
    }

    //Perfrom movement based on velocity vector
    private void perfromMovement()
    {
        if(velocity != Vector3.zero)
        {
            rb.MovePosition(transform.position + velocity * Time.fixedDeltaTime);
        }

    }

    private void performRotation()
    {
        //Converts our vector3 to a quaternion that can be used by the rigidbody
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
        if(cam != null)
        {
            currentCamRotX += tiltX;
            currentCamRotX = Mathf.Clamp(currentCamRotX, -cameraRotationLimit, cameraRotationLimit);
            cam.transform.localEulerAngles = new Vector3(currentCamRotX, 0f, 0f);
        }
    }
}
