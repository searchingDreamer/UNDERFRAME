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

    public string GetDisplayName()
    {
        return boostType.ToString();
    }
}
