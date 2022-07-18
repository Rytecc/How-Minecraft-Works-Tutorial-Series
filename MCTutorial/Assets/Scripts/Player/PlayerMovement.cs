using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform PlayerCamera;
    [SerializeField] private float Sensitivity;
    [SerializeField] private float MovementSpeed;
    [SerializeField] private float JumpForce;
    [SerializeField] private float Gravity = 9.81f;

    private CharacterController PlayerController;
    private float GravityForce;

    // Start is called before the first frame update
    void Start()
    {
        PlayerController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        GetInput(out Vector3 MoveInput, out Vector2 MouseInput);

        MovePlayer(MoveInput);
        MovePlayerCamera(MouseInput);
    }

    void GetInput(out Vector3 MoveInput, out Vector2 MouseInput)
    {
        MoveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        MouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    void MovePlayer(Vector3 MoveInput)
    {
        if(PlayerController.isGrounded) 
        {
            GravityForce = -2f;

            if(Input.GetKeyDown(KeyCode.Space))
            {
                GravityForce = JumpForce;
            }
        } 
        else 
        {
            GravityForce -= Gravity * -2f * Time.deltaTime;
        }

        Vector3 moveVector = transform.TransformDirection(MoveInput);
        PlayerController.Move(moveVector * MovementSpeed * Time.deltaTime);
        PlayerController.Move(new Vector3(0f, GravityForce, 0f) * Time.deltaTime);
    }

    void MovePlayerCamera(Vector2 MouseInput)
    {
        transform.Rotate(0f, MouseInput.x * Sensitivity, 0f);
        PlayerCamera.Rotate(-MouseInput.y * Sensitivity, 0f, 0f, Space.Self);
    }
}
