using UnityEngine;
using TMPro;

public class PlayerStatusUIController : MonoBehaviour
{
    public PlayerStats playerStats;

    [Header("UI Elements")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI staminaText;

    void Update()
    {
        if (playerStats == null) return;

        healthText.text = $"{Mathf.FloorToInt(playerStats.currentHealth)} / {playerStats.maxHealth}";
        staminaText.text = $"{Mathf.FloorToInt(playerStats.currentStamina)} / {playerStats.maxStamina}";
    }
}
