using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Main,
    Shop,
    Menu,
    Initializing
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private GameObject valuableItems;
    [SerializeField] float itemCollisionDelay = 1f;

    [SerializeField] Camera minimapCamera;
    [SerializeField] Camera fullmapCamera;
    [SerializeField] GameObject player;

    [SerializeField] GameObject minimapMarkersCanvas;
    [SerializeField] GameObject p_lightItemMark; // p_ - prefab
    [SerializeField] GameObject p_heavyItemMark; // p_ - prefab

    private int totalCash = 0;

    private GameState state;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);

        DontDestroyOnLoad(Instance);
    }

    // Start is called before the first frame update
    void Start()
    {
        state = GameState.Initializing;
        DontDestroyOnLoad(player);
        DontDestroyOnLoad(fullmapCamera);
        DontDestroyOnLoad(minimapMarkersCanvas);
        SceneManager.sceneLoaded += OnSceneLoaded;
        EndLevel();
    }

    // Update is called once per frame
    void Update()
    {
        
    }  

    IEnumerator ActivateCollisions(float delay)
    {
        yield return new WaitForSeconds(delay);

        for (int i = 0; i < valuableItems.transform.childCount; i++)
        {
            GameObject itemObj = valuableItems.transform.GetChild(i).gameObject;
            
            ValuableItem item = itemObj.GetComponent<ValuableItem>();
            item.ActivateCollision();
        }
    }

    // false - minimap camera, true - fullmap camera 
    public void SwitchEventCamera(bool value)
    {    
        minimapCamera.enabled = !value;
        fullmapCamera.enabled = value;


        Canvas playerCanvas = player.transform.Find("Canvas").gameObject.GetComponent<Canvas>();
        if (value)
        {
            playerCanvas.worldCamera = fullmapCamera;
            minimapMarkersCanvas.GetComponent<Canvas>().worldCamera = fullmapCamera;
        }
        else
        {
            playerCanvas.worldCamera = minimapCamera;
            minimapMarkersCanvas.GetComponent<Canvas>().worldCamera = minimapCamera;
        }
    }

    public void EndLevel()
    {
        if (state == GameState.Main)
        {
            SceneManager.LoadScene("shop");
            state = GameState.Shop;
        }
        else if (state == GameState.Shop)
        {
            SceneManager.LoadScene("underframe");
            state = GameState.Main;
        }
        else if (state == GameState.Initializing)
        {
            SceneManager.LoadScene("underframe");
            state = GameState.Main;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "underframe")
        {
            UIManager.Instance.UpdateBankBalance(Bank.GetBalance());
            valuableItems = GameObject.Find("Items").gameObject;
            UIManager.Instance.ActivateMinimap(true);
            StartCoroutine(ActivateCollisions(itemCollisionDelay));
            InitializeItemMarkers();
        }
        else if (scene.name == "shop")
        {
            totalCash += Bank.GetBalance();
            UIManager.Instance.UpdateBankBalance(totalCash);
            UIManager.Instance.ActivateMinimap(false);
        }
       
        GameObject spawnPoint = GameObject.Find("spawnPoint");
        player.transform.position = spawnPoint.transform.position;
    }

    public GameState GetState()
    {
        return state;
    }

    public bool TryBuy(BoostItem boostItem)
    {
        int price = boostItem.GetPrice();

        if(totalCash - price >= 0)
        {
            totalCash -= price;
            UIManager.Instance.UpdateBankBalance(totalCash);
            return true;
        }
        else
        {
            return false;
        }
    }

    private void InitializeItemMarkers()
    {
        for (int i = 0; i < valuableItems.transform.childCount; i++)
        {
            ValuableItem item = valuableItems.transform.GetChild(i).gameObject.GetComponent<ValuableItem>();
            ItemType type = item.GetItemType();
            GameObject marker = null;

            switch(type)
            {
                case ItemType.Light:
                    marker = Instantiate(p_lightItemMark, Vector3.zero, Quaternion.identity);
                    marker.gameObject.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(90, 0, 0));
                    break;
                case ItemType.Heavy:
                    marker = Instantiate(p_heavyItemMark, Vector3.zero, Quaternion.identity);
                    marker.gameObject.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(90, 0, 0));
                    break;
            }

            marker.transform.SetParent(minimapMarkersCanvas.transform, false);
            item.SetMinimapMarker(marker);
        }
    }
}
