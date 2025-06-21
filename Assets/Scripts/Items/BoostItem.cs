using UnityEngine;

public enum BoostType
{
    Stamina,
    Strength,
    Range,
    Health,
    Speed,
    None
}

public class BoostItem : MonoBehaviour
{
    public BoostType boostType = BoostType.None;

    [SerializeField] int price = 500;
    public string GetDisplayName()
    {
        return boostType.ToString();
    }

    public int GetPrice()
    {
        return price;
    }
}
