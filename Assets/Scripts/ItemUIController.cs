using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemUIController : MonoBehaviour
{
    public TextMeshProUGUI[] itemTexts; 
    public CanvasGroup itemCanvasGroup;
    public Image crosshairImage;

    public float normalCrosshairSize = 100f;
    public float enlargedCrosshairSize = 300f;
    public float crosshairLerpSpeed = 8f;
    public float textFadeSpeed = 5f;

    private bool isHoldingItem = false;
    private float targetTextAlpha = 0f;

    void Start()
    {
        ClearText();
        SetCrosshairSize(normalCrosshairSize);

        if (itemCanvasGroup != null)
            itemCanvasGroup.alpha = 1f;
    }

    void Update()
    {
        float targetSize = isHoldingItem ? enlargedCrosshairSize : normalCrosshairSize;
        float currentSize = crosshairImage.rectTransform.sizeDelta.x;
        float newSize = Mathf.Lerp(currentSize, targetSize, Time.deltaTime * crosshairLerpSpeed);
        crosshairImage.rectTransform.sizeDelta = new Vector2(newSize, newSize);

        foreach (var text in itemTexts)
        {
            if (text == null) continue;

            Color c = text.color;
            c.a = Mathf.Lerp(c.a, targetTextAlpha, Time.deltaTime * textFadeSpeed);
            text.color = c;
        }
    }

    public void ShowItemData(GameObject item)
    {
        if (item == null) return;
        if (!item.TryGetComponent<ItemController>(out var itemData)) return;

        string itemCost = $"$ {itemData.GetCost()}";

        foreach (var text in itemTexts)
        {
            if (text != null)
                text.text = itemCost;
        }

        targetTextAlpha = 1f;
    }

    public void ClearItemName()
    {
        targetTextAlpha = 0f;
    }

    public void SetHoldingItem(bool state)
    {
        isHoldingItem = state;
    }

    private void ClearText()
    {
        foreach (var text in itemTexts)
        {
            if (text != null)
                text.text = "";
        }
    }

    private void SetCrosshairSize(float size)
    {
        crosshairImage.rectTransform.sizeDelta = new Vector2(size, size);
    }
    public void ShowCustomName(string name)
    {
        foreach (var text in itemTexts) text.text = name;
        targetTextAlpha = 1f;
    }
}
