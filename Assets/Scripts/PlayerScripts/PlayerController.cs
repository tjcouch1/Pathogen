using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour {

    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 200f;
    [SerializeField] private float mouseSensitivity = 3;
    private PlayerMotor motor;
    private WeaponManager weaponManger;

    private void Start()
    {
        motor = GetComponent<PlayerMotor>();
        weaponManger = GetComponent<WeaponManager>();
    }

    private void Update()
    {
        if (GetComponent<Player>().shouldPreventInput)
        {
            motor.move(Vector3.zero);
            motor.rotate(Vector3.zero, 0);
            return;
        }

        //Calc movement velocity
        float X_mov = Input.GetAxis("Horizontal");
        float Z_mov = Input.GetAxis("Vertical");

        //Jumping
        if (Input.GetButtonDown("Jump"))
        {
            motor.Jump(jumpForce);
        }

        Vector3 movHorizontal = transform.right * X_mov;
        Vector3 movVertical = transform.forward * Z_mov;

        Vector3 velocity = (movHorizontal + movVertical) * speed;

        //Apply movement
        motor.move(velocity);

        //Calculate rotation (turning around Y)
        float Y_rot = Input.GetAxisRaw("Mouse X");
        Vector3 rotation = new Vector3(0f, Y_rot, 0f) * mouseSensitivity;

        //Calculate rotation (tilt around X)
        float X_rot = Input.GetAxisRaw("Mouse Y");
        float tilt_X = X_rot * mouseSensitivity;

        //Apply rotation
        motor.rotate(rotation, tilt_X);
    }
}
