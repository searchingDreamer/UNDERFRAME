using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public enum GameState
{
    Main,
    Shop,
    Menu,
    Loading
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] GameObject valuableItems;
    [SerializeField] float itemCollisionDelay = 1f;

    [SerializeField] Camera minimapCamera;
    [SerializeField] Camera fullmapCamera;
    [SerializeField] GameObject player;

    [SerializeField] GameObject minimapMarkersCanvas;
    [SerializeField] GameObject p_lightItemMark; // p_ - prefab
    [SerializeField] GameObject p_heavyItemMark; // p_ - prefab

    [SerializeField] List<GameObject> engineComponents;

    [Header ("Post-Processing")]
    [SerializeField] Volume volume;

    private int totalCash = 0;

    private GameState state;

    private int currentLevel = -1;
    private bool isLevelFinished = true;
    private bool isFirstLoad = true;

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
        state = GameState.Loading;
        DontDestroyOnLoad(player);
        DontDestroyOnLoad(fullmapCamera);
        DontDestroyOnLoad(minimapMarkersCanvas);
        foreach (var engineComponent in engineComponents)
        {
            DontDestroyOnLoad(engineComponent);
        }
        DontDestroyOnLoad(valuableItems);
        DontDestroyOnLoad(volume);
        SceneManager.sceneLoaded += OnSceneLoaded;
        Engine.Instance.OnComponentInserting += () => UIManager.Instance.ActivateMission(false);
        ExitLocation();
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

    public void ExitLocation()
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
        else if (state == GameState.Loading)
        {
            SceneManager.LoadScene("underframe");
            state = GameState.Main;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject spawnPoint = null;
        if (scene.name == "underframe")
        {
            if (isLevelFinished) ToNextLevel();
            valuableItems.SetActive(true);

            UIManager.Instance.UpdateBankBalance(0);
            UIManager.Instance.ActivateMinimap(true);
            Engine.Instance.gameObject.SetActive(true);
            engineComponents[currentLevel].SetActive(true);
            if (isFirstLoad) spawnPoint = GameObject.Find("spawnPoint");
            else spawnPoint = GameObject.Find("resumeSpawnPoint");

            UIManager.Instance.ActivateMission(true);
        }
        else if (scene.name == "shop")
        {
            totalCash += Bank.GetBalance();
            Bank.DestroyItems();
            valuableItems.SetActive(false);

            UIManager.Instance.UpdateBankBalance(totalCash);
            UIManager.Instance.ActivateMinimap(false);
            UIManager.Instance.ActivateMission(false);
            Engine.Instance.gameObject.SetActive(false);
            engineComponents[currentLevel].SetActive(false);

            if (engineComponents[currentLevel].GetComponent<EngineComponent>().isCollected)
            {
                isLevelFinished = true;
            }
            spawnPoint = GameObject.Find("spawnPoint");
        }

        player.transform.position = spawnPoint.transform.position;
    }

    public GameState GetState()
    {
        return state;
    }

    public bool TryBuy(BoostItem boostItem)
    {
        if (boostItem.isSold == true) return true;
        int price = boostItem.GetPrice();

        if(totalCash - price >= 0)
        {
            totalCash -= price;
            boostItem.isSold = true;
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

    private void LoadUnderframeScene()
    {
        // init all objects
        Transform spawnPoints = GameObject.Find("ItemSpawnPoints").transform.Find($"Level{currentLevel}");

        for(int i = 0; i < spawnPoints.childCount; i++)
        {
            ItemSpawnPoint itemSP = spawnPoints.GetChild(i).gameObject.GetComponent<ItemSpawnPoint>();
            
            itemSP.SpawnObject(valuableItems.transform);
        }

        StartCoroutine(ActivateCollisions(itemCollisionDelay));
    }

    // call when underframe scene loaded
    private void ToNextLevel()
    {
        isLevelFinished = false;
        if(currentLevel != -1)
            engineComponents[currentLevel].gameObject.SetActive(false);
        currentLevel += 1;
        engineComponents[currentLevel].gameObject.SetActive(true);
        UIManager.Instance.SetMissionText($"Find {engineComponents[currentLevel].GetComponent<EngineComponent>().GetName()} ({currentLevel + 1}/12)");
        DestroyItems();
        LoadUnderframeScene();
    }

    //private void DestoyItemsInTheBank()
    //{
    //    List<GameObject> bankItems = Bank.GetItems();

    //    for(int i = 0; i < bankItems.Count; i++)
    //    {
    //        Bank.GetItems().RemoveAt(i);
    //        Destroy(bankItems[i]);
    //    }
    //}

    private void DestroyItems()
    {
        for(int i = 0; i < valuableItems.transform.childCount; i++)
        {
            Destroy(valuableItems.transform.GetChild(i).gameObject);
        }
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }
}