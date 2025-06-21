using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] GameObject valuableItems;
    [SerializeField] float itemCollisionDelay = 1f;

    [SerializeField] Camera minimapCamera;

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
            GameObject itemObj = valuableItems.transform.GetChild(i).gameObject;
            
            ValuableItem item = itemObj.GetComponent<ValuableItem>();
            item.ActivateCollision();

            GameObject minimapCanvas = itemObj.transform.Find("Canvas").gameObject;
            Canvas canvas = minimapCanvas.GetComponent<Canvas>();
            canvas.worldCamera = minimapCamera;
        }
    }
}
