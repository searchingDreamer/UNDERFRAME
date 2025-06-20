using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] GameObject valuableItems;
    [SerializeField] float itemCollisionDelay = 1f;

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
        StartCoroutine(ActivateCollisions(itemCollisionDelay));
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
            ValuableItem item = valuableItems.transform.GetChild(i).GetComponent<ValuableItem>();

            item.ActivateCollision();
        }
    }
}
