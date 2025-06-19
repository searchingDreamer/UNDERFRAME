using UnityEngine;

public class PhysicsItemPickup : MonoBehaviour
{
    private PlayerLook playerLook;
    private PlayerStats playerStats;
    public ItemUIController uiController;
    public Transform playerCamera;

    private float holdDistanceDontChange = 2f;
    readonly float rotationSpeed = 100f;
    readonly float forceStrenght = 300f;
    private float sensetivityLose;
    readonly float sensetivityLoseCoef = 1f;
    private Rigidbody heldRb = null;
    private GameObject heldItem = null;
    private ItemController heldItemController = null;
    private BoostItem heldBoostItem = null;
    private Vector3 holdPoint;
    private bool isRotating = false;
    private Quaternion targetRotation;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        if (playerCamera == null)
            Debug.LogError("PlayerCamera is not assigned!");
        else
            playerLook = playerCamera.GetComponent<PlayerLook>();
    }

    void Update()
    {
        if (heldItem == null)
        {
            TryPickup();
        }
        else
        {
            HandleInputForHeldItem();

            if (heldBoostItem != null && Input.GetKeyDown(KeyCode.F))
            {
                UseBoostItem();
            }
        }
    }

    void FixedUpdate()
    {
        if (heldItem != null)
        {
            HandlePhysicsForHeldItem();

            if (heldBoostItem != null)
            {
                uiController.ShowCustomName(heldBoostItem.GetDisplayName());
            }
            else
            {
                uiController.ShowItemData(heldItem);
            }
        }
    }

    void TryPickup()
    {
        Ray ray = new(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, playerStats.range))
        {
            var obj = hit.collider.gameObject;
            string objName = obj.name.ToLower();

            bool isItem = hit.collider.CompareTag("Item");
            var boostItem = obj.GetComponent<BoostItem>();

            if (isItem || boostItem != null)
            {
                if (boostItem != null)
                    uiController.ShowCustomName(boostItem.GetDisplayName());
                else
                    uiController.ShowItemData(obj);

                if (Input.GetMouseButtonDown(0))
                {
                    heldItem = obj;
                    heldRb = heldItem.GetComponent<Rigidbody>();
                    heldItemController = heldItem.GetComponent<ItemController>();
                    heldBoostItem = boostItem;

                    if (heldRb != null && isItem)
                    {
                        sensetivityLose = (heldRb.mass - playerStats.strength) * sensetivityLoseCoef;
                        playerLook.mouseSensitivity -= sensetivityLose;
                    }

                    if (heldItemController != null && isItem)
                    {
                        heldItemController.OnDestroy -= DropItem;
                        heldItemController.OnDestroy += DropItem;
                    }

                    if (heldRb != null)
                    {
                        heldRb.isKinematic = false;
                        heldRb.useGravity = false;
                        heldRb.drag = 10f;
                        heldRb.angularDrag = 10f;

                        holdDistanceDontChange = Vector3.Distance(playerCamera.position, heldItem.transform.position);
                        holdPoint = playerCamera.position + playerCamera.forward * holdDistanceDontChange;

                        targetRotation = heldRb.rotation;

                        uiController.SetHoldingItem(true);
                    }
                    else
                    {
                        heldItem = null;
                    }
                }
            }
            else
            {
                uiController.ClearItemName();
            }
        }
        else
        {
            uiController.ClearItemName();
        }
    }

    void HandleInputForHeldItem()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DropItem();
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            isRotating = true;
            if (playerLook != null)
                playerLook.enabled = false;
        }
        if (Input.GetMouseButtonUp(1))
        {
            isRotating = false;
            if (playerLook != null)
                playerLook.enabled = true;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            holdDistanceDontChange = Mathf.Clamp(holdDistanceDontChange + scroll * 2f, 1f, 5f);
        }
    }

    void HandlePhysicsForHeldItem()
    {
        holdPoint = playerCamera.position + playerCamera.forward * holdDistanceDontChange;

        if (heldRb != null)
        {
            Vector3 toHoldPoint = holdPoint - heldRb.position;
            float distance = toHoldPoint.magnitude;

            float forceMagnitude = distance * forceStrenght;
            Vector3 force = toHoldPoint.normalized * forceMagnitude;

            heldRb.AddForce(force, ForceMode.Force);

            if (isRotating)
            {
                float rotX = Input.GetAxis("Mouse X") * rotationSpeed * Time.fixedDeltaTime;
                float rotY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.fixedDeltaTime;

                Quaternion deltaX = Quaternion.AngleAxis(-rotX, playerCamera.up);
                Quaternion deltaY = Quaternion.AngleAxis(rotY, playerCamera.right);

                targetRotation *= deltaX * deltaY;
            }

            heldRb.MoveRotation(Quaternion.Slerp(heldRb.rotation, targetRotation, Time.fixedDeltaTime * 10f));
        }
    }

    void DropItem()
    {
        if (heldRb != null)
        {
            heldRb.useGravity = true;
            heldRb.drag = 0f;
            heldRb.angularDrag = 0.05f;
        }

        heldItem = null;
        heldRb = null;
        heldItemController = null;
        heldBoostItem = null;
        isRotating = false;

        if (playerLook != null)
            playerLook.enabled = true;

        uiController.SetHoldingItem(false);
        uiController.ClearItemName();

        playerLook.mouseSensitivity += sensetivityLose;
    }
    void UseBoostItem()
    {
        if (playerStats == null || heldBoostItem == null) return;

        switch (heldBoostItem.boostType)
        {
            case BoostType.Stamina:
                playerStats.maxStamina += 10; // +10
                break;
            case BoostType.Strength:
                playerStats.strength += 3; // +3
                break;
            case BoostType.Range:
                playerStats.range += 1; // +1
                break;
            case BoostType.Health:
                playerStats.maxHealth += 20; // +20
                break;
            case BoostType.Speed:
                playerStats.runSpeed += 1;
                break;
        }
        Debug.Log($"Applied boost: {heldBoostItem.boostType}");

        Destroy(heldItem);
        DropItem();
    }
}
