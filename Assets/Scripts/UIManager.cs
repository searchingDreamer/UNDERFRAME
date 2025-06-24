using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] GameObject UI;

    [Header("Item")]
    [SerializeField] TextMeshProUGUI itemInfo;
    [SerializeField] TextMeshProUGUI itemInfoShadow;
    [SerializeField] float textFadeSpeed = 5f;
    private enum TextFadingState { Trasparent, Filled, Transparenting, Filling}
    private TextFadingState textFadingState = TextFadingState.Trasparent;

    [Header("Player")]
    [SerializeField] PlayerStats playerStats;
    [SerializeField] TextMeshProUGUI playerHealth;
    [SerializeField] TextMeshProUGUI playerStamina;

    [Header("Bank")]
    [SerializeField] TextMeshProUGUI bankBalance;
    [SerializeField] TextMeshProUGUI bankBalanceShadow;

    //[Header("Shop")]
    //[SerializeField] TextMeshProUGUI price;
    //[SerializeField] TextMeshProUGUI priceShadow;

    [Header("Crosshair")]
    [SerializeField] UnityEngine.UI.Image crosshair;
    [SerializeField] float normalCrosshairSize = 100f;
    [SerializeField] float enlargedCrosshairSize = 300f;
    [SerializeField] float crosshairLerpSpeed = 8f;

    [Header("Minimap")]
    [SerializeField] GameObject minimap;
    [SerializeField] GameObject fullmap;

    [Header("Mission")]
    [SerializeField] TextMeshProUGUI missionText;
    [SerializeField] TextMeshProUGUI missionShadow;

    private enum CrosshairStates { Normal, Enlarged, Normalizing, Enlarging };
    private CrosshairStates crosshairState = CrosshairStates.Normal;

    private Coroutine crosshairResize;
    private Coroutine itemInfoFade;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);

        DontDestroyOnLoad(Instance);
    }

    private void Start()
    {
        Color c1 = itemInfo.color; Color c2 = itemInfoShadow.color;
        c1.a = c2.a = 0;
        itemInfo.color = c1; itemInfoShadow.color = c2;

        DontDestroyOnLoad(UI);

        SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) => { GameManager.Instance.SwitchEventCamera(false); };
    }

    private void Update()
    {
        UpdatePlayerStats();
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            ChangeMinimap(true);
        }
        else if(Input.GetKeyUp(KeyCode.Tab))
        {
            ChangeMinimap(false);
        }
    }

    public void ActivateItemInfo(bool state, bool smooth = true)
    {
        if(!smooth)
        {
            itemInfo.gameObject.SetActive(state);
            return;
        }

        if(state)
        {
            if (textFadingState == TextFadingState.Filled ||
                textFadingState == TextFadingState.Filling)
            {
                return;
            }
            else
            {
                if (itemInfoFade != null) StopCoroutine(itemInfoFade);
                textFadingState = TextFadingState.Filling;
                itemInfoFade = StartCoroutine(SmoothShowItemInfo());
            }
        }
        else
        {
            if (textFadingState == TextFadingState.Trasparent ||
                textFadingState == TextFadingState.Transparenting)
            {
                return;
            }
            else
            {
                if (itemInfoFade != null) StopCoroutine(itemInfoFade);
                textFadingState = TextFadingState.Transparenting;
                itemInfoFade = StartCoroutine(SmoothHideItemInfo());
            }
        }
    }

    public void UpdateItemInfo(GameObject item)
    {
        ValuableItem itemController = item.GetComponent<ValuableItem>();
        string itemCost = $"$ {itemController.GetCost()}";
        itemInfo.text = itemCost;
        itemInfoShadow.text = itemCost;
    }

    public void ShowCustomItemInfo(string text)
    {
        itemInfo.text = text;
        itemInfoShadow.text = text;
    }

    private void UpdatePlayerStats()
    {
        if (playerStats == null) return;

        playerHealth.text = $"{Mathf.FloorToInt(playerStats.currentHealth)} / {playerStats.maxHealth}";
        playerStamina.text = $"{Mathf.FloorToInt(playerStats.currentStamina)} / {playerStats.maxStamina}";
    }

    public void UpdateBankBalance(int balance)
    {
        bankBalance.text = $"$ {balance}";
        bankBalanceShadow.text = $"$ {balance}";
    }

    public void NormalCrosshair()
    {
        if (crosshairState == CrosshairStates.Normal ||
            crosshairState == CrosshairStates.Normalizing)
        {
            return;
        }
        else
        {
            if (crosshairResize != null) StopCoroutine(crosshairResize);
            crosshairState = CrosshairStates.Normalizing;
            crosshairResize = StartCoroutine(SmoothNormalCrosshair());
        }
    }

    public void EnlargeCrosshair()
    {
        if (crosshairState == CrosshairStates.Enlarged ||
            crosshairState == CrosshairStates.Enlarging)
        {
            return;
        }
        else
        {
            if (crosshairResize != null) StopCoroutine(crosshairResize);
            crosshairState = CrosshairStates.Enlarging;
            crosshairResize = StartCoroutine(SmoothEnlargeCrosshair());
        }
    }

    private IEnumerator SmoothNormalCrosshair()
    {
        float currentSize = crosshair.rectTransform.sizeDelta.x;
        float t = 0;

        while(currentSize > normalCrosshairSize + Mathf.Epsilon)
        {
            t += Time.deltaTime * crosshairLerpSpeed;
            currentSize = Mathf.Lerp(currentSize, normalCrosshairSize, t);
            crosshair.rectTransform.sizeDelta = new Vector2(currentSize, currentSize);
            yield return null;
        }

        crosshairState = CrosshairStates.Normal;
    }

    private IEnumerator SmoothEnlargeCrosshair()
    {
        float currentSize = crosshair.rectTransform.sizeDelta.x;
        float t = 0;

        while (currentSize < enlargedCrosshairSize - Mathf.Epsilon)
        {
            t += Time.deltaTime * crosshairLerpSpeed;
            currentSize = Mathf.Lerp(currentSize, enlargedCrosshairSize, t);
            crosshair.rectTransform.sizeDelta = new Vector2(currentSize, currentSize);
            yield return null;
        }

        crosshairState = CrosshairStates.Enlarged;
    }
    private IEnumerator SmoothShowItemInfo()
    {
        itemInfo.gameObject.SetActive(true);

        float currentA = itemInfo.color.a;
        Color currentColor1 = itemInfo.color;
        Color currentColor2 = itemInfoShadow.color;
        float t = 0;

        while (currentA < 1f - Mathf.Epsilon)
        {
            t += Time.deltaTime * textFadeSpeed;
            currentColor1.a = currentColor2.a = Mathf.Lerp(currentA, 1f, t);
            itemInfo.color = currentColor1;
            itemInfoShadow.color = currentColor2;
            yield return null;
        }

        textFadingState = TextFadingState.Filled;
    }

    private IEnumerator SmoothHideItemInfo()
    {
        float currentA = itemInfo.color.a;
        Color currentColor1 = itemInfo.color;
        Color currentColor2 = itemInfoShadow.color;
        float t = 0;

        while (currentA > Mathf.Epsilon)
        {
            t += Time.deltaTime * textFadeSpeed;
            currentColor1.a = currentColor2.a = Mathf.Lerp(currentA, 0f, t);
            itemInfo.color = currentColor1;
            itemInfoShadow.color = currentColor2;
            yield return null;
        }

        textFadingState = TextFadingState.Trasparent;
        
        itemInfo.gameObject.SetActive(false);
    }

    public void ActivateMinimap(bool state)
    {
        minimap.SetActive(state);
    }

    // false - change to minimap, true - to fullmap
    private void ChangeMinimap(bool value)
    {
        GameManager.Instance.SwitchEventCamera(value);

        if (value)
        {
            minimap.SetActive(false);
            fullmap.SetActive(true);
        }
        else
        {
            minimap.SetActive(true);
            fullmap.SetActive(false);
        }
    }

    public void ActivateMission(bool state)
    {
        missionText.gameObject.SetActive(state);
    }

    public void SetMissionText(string text)
    {
        missionText.text = text;
        missionShadow.text = text;
    }
}
