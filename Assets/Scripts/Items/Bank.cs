using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bank : MonoBehaviour
{
    static List<GameObject> items = new List<GameObject>();
    static int balance = 0;

    private void Awake()
    {
        balance = 0;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Item"))
        {
            ValuableItem itemController = other.gameObject.GetComponent<ValuableItem>();
            itemController.OnHit += RecalculateBalance;
            balance += itemController.GetCost();
            UIManager.Instance.UpdateBankBalance(balance);
            items.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Item"))
        {
            ValuableItem itemController = other.gameObject.GetComponent<ValuableItem>();
            itemController.OnHit -= RecalculateBalance;
            balance -= itemController.GetCost();
            UIManager.Instance.UpdateBankBalance(balance);
            items.Remove(other.gameObject);
        }
    }

    private void RecalculateBalance()
    {
        balance = 0;
        foreach (var item in items)
        {
            balance += item.GetComponent<ValuableItem>().GetCost();
        }

        UIManager.Instance.UpdateBankBalance(balance);
    }

    static public int GetBalance()
    {
        return balance;
    }

    static public List<GameObject> GetItems()
    {
        return items;
    }
}
