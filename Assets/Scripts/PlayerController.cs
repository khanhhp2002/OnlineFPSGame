using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    public float jumpForce, gravityMod = 2.5f;

    public Transform groundCheckPoint;
    public LayerMask groundLayers;

    public GameObject bulletImpact;
    private float shootCounter;
    public float muzzleDisplayTime;
    private float muzzleCounter;

    public float maxHeat = 10f, coolRate = 4f, overHeatCoolRate = 5f;
    private float heatCounter;
    private bool overHeat;

    public Gun[] allGuns;
    private int selectedGun;

    public AudioSource audioSourse;

    // Start is called before the first frame update
    void Start()
    {
        // lock cursor on the center of the screen and disable it
        Cursor.lockState = CursorLockMode.Locked;

        cam = Camera.main;
        UIController.instance.weaponHeatSlider.maxValue = maxHeat;

        // Spawn player:
        Transform playerTransform = SpawnManager.instance.GetSpawnPoint();
        transform.position = playerTransform.position;
        transform.rotation = playerTransform.rotation;
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
        movement = ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized * (Input.GetMouseButton(1) ? runSpeed : movSpeed);
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

        // shooting:
        if (allGuns[selectedGun].muzzleFlash.activeInHierarchy)
        {
            muzzleCounter -= Time.deltaTime;
            if (muzzleCounter <= 0)
            {
                allGuns[selectedGun].muzzleFlash.SetActive(false);
            }
        }

        if (!overHeat)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }

            if (Input.GetMouseButton(0) && allGuns[selectedGun].isAutomatic)
            {
                shootCounter -= Time.deltaTime;
                if (shootCounter <= 0)
                {
                    Shoot();
                }
            }
            if (heatCounter <= 0)
            {
                heatCounter = 0f;
            }
            else
            {
                heatCounter -= coolRate * Time.deltaTime;
            }
        }
        else
        {
            heatCounter -= overHeatCoolRate * Time.deltaTime;
            if (heatCounter <= 0)
            {
                heatCounter = 0f;
                overHeat = false;
                UIController.instance.overHeatMessage.gameObject.SetActive(false);
            }
        }
        UIController.instance.weaponHeatSlider.value = heatCounter;

        // changing weapon:
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            if (selectedGun == allGuns.Length - 1)
            {
                selectedGun = 0;
            }
            else
            {
                selectedGun++;
            }
            SwitchWeapon();
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            if (selectedGun == 0)
            {
                selectedGun = allGuns.Length - 1;
            }
            else
            {
                selectedGun--;
            }
            SwitchWeapon();
        }

        for (int i = 0; i < allGuns.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                selectedGun = i;
                SwitchWeapon();
            }
        }
    }
    private void LateUpdate()
    {
        cam.transform.position = viewPoint.position;
        cam.transform.rotation = viewPoint.rotation;
    }

    private void Shoot()
    {
        Sound();
        Ray ray = cam.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("Hitting: " + hit.collider.gameObject.name);
            Destroy(Instantiate(bulletImpact, hit.point + (hit.normal * 0.002f), Quaternion.LookRotation(hit.normal, Vector3.up)), 10f);
        }
        shootCounter = allGuns[selectedGun].timeBetweenShots;
        heatCounter += allGuns[selectedGun].heatPerShot;
        if (heatCounter >= maxHeat)
        {
            heatCounter = maxHeat;
            overHeat = true;

            UIController.instance.overHeatMessage.gameObject.SetActive(true);
        }
        allGuns[selectedGun].muzzleFlash.SetActive(true);
        muzzleCounter = muzzleDisplayTime;
    }

    private void Sound()
    {
        audioSourse.Play();
    }

    void SwitchWeapon()
    {
        foreach (Gun gun in allGuns)
        {
            gun.gameObject.SetActive(false);
            gun.muzzleFlash.SetActive(false);
        }

        allGuns[selectedGun].gameObject.SetActive(true);
    }
}
