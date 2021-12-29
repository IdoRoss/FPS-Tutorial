using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    //-----Variables-----//
    // Camera vars
    public float mouseSensitivity = 1f;
    private float verticalRotStore;
    public bool invertLook;
    public Transform viewPoint;
    private Camera cam;
    private Vector2 mouseInput;

    // Movement vars
    public float moveSpeed = 5f, runSpeed = 8f;
    private float activeMoveSpeed;
    private Vector3 moveDir, movement;

    public float jumpForce = 12f;
    public float gravityMod = 2.5f;
    public Transform groundCheckPoint;
    public LayerMask groundLayer;
    private bool isGrounded;

    // Player vars
    public GameObject playerModel;
    public CharacterController charCon;
    public int maxHealth = 100;
    private int currentHealth;

    // Weapon vars
    public GameObject bulletImpact;
    private float shotCounter;

    public float maxHeat = 10f, coolRate = 4f, overHeatCoolRate = 5f;
    private float heatCounter;
    private bool overHeated;

    public Gun[] allGuns;
    private int selectedGun;

    public float muzzleDisplayTime = 0.01f;
    private float muzzleCounter;

    public GameObject playerHitImpact;

    public Animator anim;
    public Transform modelGunPoint, gunHolder;

    //-----Start is called before the first frame update-----//
    void Start()
    {
        currentHealth = maxHealth;
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;
        
        selectedGun = 0;
        //SwitchGun();
        photonView.RPC("SetGun", RpcTarget.All, selectedGun);
        /*????????
        Transform newTrans = SpawnManager.instance.GetSpawnPoint();
        transform.position = newTrans.position;
        transform.rotation = newTrans.rotation;
        */
        if(photonView.IsMine)
        {
            UIController.instance.weaponTempSlider.maxValue = maxHeat;
            UIController.instance.healthSlider.maxValue = maxHealth;
            UIController.instance.healthSlider.value = currentHealth;
            playerModel.SetActive(false);
        }
        else
        {
            gunHolder.parent = modelGunPoint;
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
        }
    }

    //-----Update is called once per frame-----//
    void Update()
    {
        if (photonView.IsMine)
        {
            // Handle mouse look
            mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);
            verticalRotStore += mouseInput.y;
            verticalRotStore = Mathf.Clamp(verticalRotStore, -60f, 60f);
            if (invertLook)
            {
                verticalRotStore = -verticalRotStore;
            }
            viewPoint.rotation = Quaternion.Euler(-verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);

            // Handle movement
            moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            if (Input.GetKey(KeyCode.LeftShift))
            {
                activeMoveSpeed = runSpeed;
            }
            else
            {
                activeMoveSpeed = moveSpeed;
            }

            float yVal = movement.y;
            movement = ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized * activeMoveSpeed;
            movement.y = yVal;
            if (charCon.isGrounded)
            {
                movement.y = 0f;
            }
            isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, 0.25f, groundLayer);
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                movement.y = jumpForce;
            }
            movement.y += Physics.gravity.y * gravityMod * Time.deltaTime;
            charCon.Move(movement * Time.deltaTime);

            // Handle shooting

            if (allGuns[selectedGun].muzzleFlash.activeInHierarchy)
            {
                muzzleCounter -= Time.deltaTime;
                if (muzzleCounter <= 0)
                {
                    allGuns[selectedGun].muzzleFlash.SetActive(false);
                }
            }
            if (!overHeated)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Shoot();
                }
                if (Input.GetMouseButton(0) && allGuns[selectedGun].isAutomatic)
                {
                    shotCounter -= Time.deltaTime;
                    if (shotCounter <= 0)
                    {
                        Shoot();
                    }
                }
                heatCounter -= coolRate * Time.deltaTime;
            }
            else
            {
                heatCounter -= overHeatCoolRate * Time.deltaTime;
                if (heatCounter <= 0)
                {
                    overHeated = false;
                    UIController.instance.overheatedMessage.gameObject.SetActive(false);
                }
            }
            if (heatCounter < 0)
            {
                heatCounter = 0;
            }
            UIController.instance.weaponTempSlider.value = heatCounter;

            // Handle gun switching
            if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
            {
                selectedGun++;
                if (selectedGun >= allGuns.Length)
                {
                    selectedGun = 0;
                }
                //SwitchGun();
                photonView.RPC("SetGun", RpcTarget.All, selectedGun);
            }
            else if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
            {
                selectedGun--;
                if (selectedGun < 0)
                {
                    selectedGun = allGuns.Length - 1;
                }
                //SwitchGun();
                photonView.RPC("SetGun", RpcTarget.All, selectedGun);
            }

            for (int i = 1; i <= allGuns.Length; i++)
            {
                if (Input.GetKeyDown(i.ToString()))
                {
                    selectedGun = i - 1;
                    //SwitchGun();
                    photonView.RPC("SetGun", RpcTarget.All, selectedGun);
                }
            }

            anim.SetBool("grounded", isGrounded);
            anim.SetFloat("speed", moveDir.magnitude);

            // Freeing the mouse
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else if (Cursor.lockState == CursorLockMode.None && Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    //-----Shooting the gun-----//
    private void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            //Debug.Log("Player hit" + hit.collider.gameObject.name);
            if (hit.collider.gameObject.tag == "Player")
            {
                //Debug.Log("Hit " + hit.collider.gameObject.GetPhotonView().Owner.NickName);
                PhotonNetwork.Instantiate(playerHitImpact.name, hit.point, Quaternion.identity);
                hit.collider.gameObject.GetPhotonView().RPC("DealDamage",RpcTarget.All, photonView.Owner.NickName, allGuns[selectedGun].damage);
            }
            else
            {
                GameObject bulletImpactObject = Instantiate(bulletImpact, hit.point + hit.normal * 0.002f, Quaternion.LookRotation(hit.normal, Vector3.up));
                Destroy(bulletImpactObject, 4f);
            }
        }
        shotCounter = allGuns[selectedGun].timeBetweenShots;

        heatCounter += allGuns[selectedGun].heatPerShot;
        if (heatCounter >= maxHeat)
        {
            heatCounter = maxHeat;
            overHeated = true;
            UIController.instance.overheatedMessage.gameObject.SetActive(true);
        }
        allGuns[selectedGun].muzzleFlash.SetActive(true);
        muzzleCounter = muzzleDisplayTime;
    }
    [PunRPC]
    public void DealDamage(string damager, int damageAmount)
    {
        TakeDamage(damager, damageAmount);
    }
    public void TakeDamage(string damager, int damageAmount)
    {
        if(photonView.IsMine)
        {
            currentHealth -= damageAmount;
            
            if (currentHealth <= 0)
            {
                PlayerSpawner.instance.Die(damager);
            }

            UIController.instance.healthSlider.value = currentHealth;
        }
    }
    //-----LateUpdate is called once per frame after all regular Update  calls-----//
    private void LateUpdate()
    {
        if(photonView.IsMine)
        {
            cam.transform.position = viewPoint.position;
            cam.transform.rotation = viewPoint.rotation;
        }
    }
    //-----Switch Guns-----//
    void SwitchGun()
    {
        foreach (Gun gun in allGuns)
        {
            gun.gameObject.SetActive(false);
        }
        allGuns[selectedGun].gameObject.SetActive(true);
        allGuns[selectedGun].muzzleFlash.SetActive(false);
    }

    [PunRPC]
    public void SetGun(int gunToSwitchto)
    {
        if(gunToSwitchto < allGuns.Length)
        {
            selectedGun = gunToSwitchto;
            SwitchGun();
        }
    }
}
