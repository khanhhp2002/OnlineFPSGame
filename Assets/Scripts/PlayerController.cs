using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform viewPoint;
    public float mouseSensitivity = 1f;
    private float verticalRotStore;
    private Vector2 mouseInput;
    public bool invertLook;
    public float movSpeed = 5f, runSpeed = 8f;
    private Vector3 moveDir, movement;
    public CharacterController characterController;
    private Camera cam;
    public float jumpForce, gravityMod;
    public Transform groundCheckPoint;
    public LayerMask groundLayers;
    // Start is called before the first frame update
    void Start()
    {
        // lock cursor on the center of the screen and disable it
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        //View point direction:
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;
        verticalRotStore += mouseInput.y;
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);
        viewPoint.rotation = Quaternion.Euler((invertLook ? 1 : -1) * Mathf.Clamp(verticalRotStore, -60f, 60f), viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);

        //Body movement: Gravity + body move direction
        float yVelocity = movement.y;
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        movement = ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized * (Input.GetKey(KeyCode.LeftShift) ? runSpeed : movSpeed);
        movement.y = yVelocity;
        if (characterController.isGrounded)
        {
            movement.y = 0f;
        }
        if (Input.GetButtonDown("Jump") && Physics.Raycast(groundCheckPoint.position, Vector3.down, .25f, groundLayers)) // origin, direction, max distance, layer mask
        {
            movement.y = jumpForce;
        }

        movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;
        characterController.Move(movement * Time.deltaTime);

        // Cursor mode:
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Unlock
            Cursor.lockState = CursorLockMode.None;
        }
        else if (Cursor.lockState == CursorLockMode.None && Input.GetKeyUp(KeyCode.Tab))
        {
            //Lock
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    private void LateUpdate()
    {
        cam.transform.position = viewPoint.position;
        cam.transform.rotation = viewPoint.rotation;
    }
}
