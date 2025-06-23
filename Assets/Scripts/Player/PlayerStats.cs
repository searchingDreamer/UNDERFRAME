using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Player characteristics")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float currentStamina;

    [Header("Pickup properties")]
    public float strength = 50f;
    public float range = 3f;

    [Header("Movement Speeds")]
    public float maxStamina = 40f;
    public float speed = 5f;

    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }
    public bool ApplyStatChange(string statName, float value)
    {
        switch (statName)
        {
            case "strength": strength = value; return true;
            case "range": range = value; return true;
            case "maxhealth": maxHealth = value; return true;
            case "maxstamina": maxStamina = value; return true;
            case "runspeed": speed = value; return true;
            case "stamina": currentStamina = value; return true;

            default: return false;
        }
    }
}
