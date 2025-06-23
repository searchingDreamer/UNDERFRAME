using UnityEngine;

enum ObjectType
{
    Item,
    Boost,
    EngineComponent
}

public class Interaction : MonoBehaviour
{
    [SerializeField] PlayerLook playerLook;
    [SerializeField] PlayerStats playerStats;
    [SerializeField] Transform playerCamera;

    private float holdDistanceDontChange = 2f;
    [SerializeField] float rotationSpeed = 100f;
    [SerializeField] float forceStrenght = 300f;
    private float weightEffect = 1;
    [SerializeField] float playerStrengthCoef = 1f;

    private GameObject heldObj = null;
    private Rigidbody heldRb = null;
    private ObjectType heldObjType;

    private Vector3 holdPoint;
    private bool isRotating = false;
    private Quaternion targetRotation;

    private LayerMask itemLayer;

    void Start()
    {
        itemLayer = LayerMask.GetMask("Interactable");
        Engine.Instance.OnComponentInserting += DropItem;
    }

    void Update()
    {
        if (heldObj == null)
        {
            TryPickup();
        }
        else
        {
            HandleInputForHeldItem();
        }
    }

    void FixedUpdate()
    {
        if (heldObj != null)
        {
            HandlePhysicsForHeldItem();
        }
    }

    void TryPickup()
    {
        Ray ray = new(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, playerStats.range, itemLayer))
        {
            UIManager.Instance.ActivateItemInfo(false);
            UIManager.Instance.NormalCrosshair();
        }
        else
        {
            GameObject obj = hit.collider.gameObject;
            UIManager.Instance.EnlargeCrosshair();

            string itemType = hit.collider.tag;

            switch (itemType)
            {
                case "Item":
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            heldObj = obj;
                            heldRb = heldObj.GetComponent<Rigidbody>();
                            ValuableItem item = heldObj.GetComponent<ValuableItem>();

                            if (heldRb != null)
                            {
                                weightEffect = (playerStats.strength / heldRb.mass) * playerStrengthCoef;
                                if (weightEffect > 1) weightEffect = 1;
                                else if(weightEffect < 0.3)
                                {
                                    heldObj = null;
                                    heldRb = null;
                                    Debug.Log("Not enought strenght");
                                    return;
                                }

                                playerLook.mouseSensitivity *= weightEffect;
                                playerStats.speed *= weightEffect;
                                Debug.Log("weight effect: " + weightEffect);
                                Debug.Log("sensetivity: " + playerLook.mouseSensitivity);
                                Debug.Log("speed: " + playerStats.speed);

                                heldRb.isKinematic = false;
                                heldRb.useGravity = false;
                                heldRb.drag = 10f;
                                heldRb.angularDrag = 10f;

                                holdDistanceDontChange = Vector3.Distance(playerCamera.position, heldObj.transform.position);
                                holdPoint = playerCamera.position + playerCamera.forward * holdDistanceDontChange;

                                targetRotation = heldRb.rotation;
                            }
                            else
                            {
                                heldObj = null;
                                Debug.LogWarning("InvalidItem: No RigidBody");
                                return;
                            }

                            if (item != null)
                            {
                                // update interface on hit
                                item.OnHit += () => UIManager.Instance.UpdateItemInfo(obj);
                                item.OnDestroy -= DropItem;
                                item.OnDestroy -= () => UIManager.Instance.ActivateItemInfo(false, false);
                                item.OnDestroy += DropItem; // drop item when it destroys
                                item.OnDestroy += () => UIManager.Instance.ActivateItemInfo(false, false);
                            }
                            else
                            {
                                heldObj = null;
                                Debug.LogWarning("InvalidItem: No ItemController");
                                return;
                            }

                            heldObjType = ObjectType.Item;
                        }
                        
                        UIManager.Instance.ActivateItemInfo(true);
                        UIManager.Instance.UpdateItemInfo(obj);

                        break;
                    }
                case "Boost":
                    {
                        BoostItem boostItem = obj.GetComponent<BoostItem>();

                        UIManager.Instance.ActivateItemInfo(true);
                        if(GameManager.Instance.GetState() == GameState.Shop)
                        {
                            string displayInfo = boostItem.GetDisplayName();
                            displayInfo += $" ($ {boostItem.GetPrice()})";
                            UIManager.Instance.ShowCustomItemInfo(displayInfo);
                        }
                        else UIManager.Instance.ShowCustomItemInfo(boostItem.GetDisplayName());

                        if (Input.GetMouseButtonDown(0))
                        {
                            if (GameManager.Instance.GetState() == GameState.Shop)
                            {
                                if (!GameManager.Instance.TryBuy(boostItem)) return;
                            }

                            heldObj = obj;
                            heldRb = heldObj.GetComponent<Rigidbody>();


                            if (heldRb != null)
                            {
                                heldRb.isKinematic = false;
                                heldRb.useGravity = false;
                                heldRb.drag = 10f;
                                heldRb.angularDrag = 10f;

                                holdDistanceDontChange = Vector3.Distance(playerCamera.position, heldObj.transform.position);
                                holdPoint = playerCamera.position + playerCamera.forward * holdDistanceDontChange;

                                targetRotation = heldRb.rotation;
                            }
                            else
                            {
                                heldObj = null;
                                Debug.LogWarning("InvalidItem: No RigidBody");
                                return;
                            }

                            heldObjType = ObjectType.Boost;
                        }

                        break;
                    }
                case "Button":
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            GameManager.Instance.ExitLocation();
                        }
                        break;
                    }
                case "Door":
                    {
                        if(Input.GetMouseButtonDown(0))
                        {
                            obj.GetComponent<DoorController>().Open(transform.position);
                        }

                        break;
                    }
                case "EngineComponent":
                    {
                        UIManager.Instance.ActivateItemInfo(true);
                        EngineComponent component = obj.GetComponent<EngineComponent>();

                        UIManager.Instance.ShowCustomItemInfo(component.GetName());

                        if (Input.GetMouseButtonDown(0))
                        {
                            heldObj = obj;
                            heldRb = heldObj.GetComponent<Rigidbody>();

                            if (heldRb != null)
                            {
                                weightEffect = (playerStats.strength / heldRb.mass) * playerStrengthCoef;
                                if (weightEffect > 1) weightEffect = 1;
                                else if (weightEffect < 0.3)
                                {
                                    heldObj = null;
                                    heldRb = null;
                                    Debug.Log("Not enought strenght");
                                    return;
                                }

                                playerLook.mouseSensitivity *= weightEffect;
                                playerStats.speed *= weightEffect;
                                Debug.Log("weight effect: " + weightEffect);
                                Debug.Log("sensetivity: " + playerLook.mouseSensitivity);
                                Debug.Log("speed: " + playerStats.speed);

                                heldRb.isKinematic = false;
                                heldRb.useGravity = false;
                                heldRb.drag = 10f;
                                heldRb.angularDrag = 10f;

                                holdDistanceDontChange = Vector3.Distance(playerCamera.position, heldObj.transform.position);
                                holdPoint = playerCamera.position + playerCamera.forward * holdDistanceDontChange;

                                targetRotation = heldRb.rotation;
                            }
                            else
                            {
                                heldObj = null;
                                Debug.LogWarning("InvalidItem: No RigidBody");
                                return;
                            }

                            heldObjType = ObjectType.EngineComponent;
                        }

                        break; 
                    }
            }
        }
    }

    void HandleInputForHeldItem()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DropItem();
            return;
        }

        if (Input.GetKeyDown(KeyCode.F) && heldObjType == ObjectType.Boost)
        {
            UseBoostItem(heldObj.GetComponent<BoostItem>());
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
            Vector3 force = toHoldPoint.normalized * forceMagnitude * heldRb.mass;

            heldRb.AddForce(force, ForceMode.Force);

            if (isRotating)
            {
                float rotX = Input.GetAxis("Mouse X") * rotationSpeed * weightEffect * Time.fixedDeltaTime;
                float rotY = Input.GetAxis("Mouse Y") * rotationSpeed * weightEffect * Time.fixedDeltaTime;

                Quaternion deltaX = Quaternion.AngleAxis(-rotX, playerCamera.up);
                Quaternion deltaY = Quaternion.AngleAxis(rotY, playerCamera.right);

                targetRotation *= deltaX * deltaY;
            }

            Quaternion deltaRotation = Quaternion.Slerp(heldRb.rotation, targetRotation, Time.fixedDeltaTime * 10f);
            heldRb.MoveRotation(deltaRotation);
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

        if (heldObjType == ObjectType.Item ||
            heldObjType == ObjectType.EngineComponent)
        {
            if(heldObjType == ObjectType.Item)
            {
                ValuableItem item = heldObj.GetComponent<ValuableItem>();
                item.OnDestroy -= DropItem;
            }

            playerLook.mouseSensitivity /= weightEffect;
            playerStats.speed /= weightEffect;
        }

        heldObj = null;
        heldRb = null;
        isRotating = false;

        if (playerLook != null)
            playerLook.enabled = true;

        UIManager.Instance.ActivateItemInfo(false);
        UIManager.Instance.NormalCrosshair();
    }

    void UseBoostItem(BoostItem boost)
    {
        if (playerStats == null || boost == null) return;

        switch (boost.boostType)
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
                playerStats.speed += 1;
                break;
        }
        Debug.Log($"Applied boost: {boost.boostType}");

        Destroy(boost.gameObject);
    }
}
