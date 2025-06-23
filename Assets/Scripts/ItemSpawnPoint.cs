using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawnPoint : MonoBehaviour
{
    [SerializeField] GameObject objectToSpawn;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject SpawnObject(Transform parent)
    {
        if (objectToSpawn != null)
        {
            GameObject obj = Instantiate(objectToSpawn, Vector3.zero, Quaternion.identity);
            obj.transform.SetParent(parent, false);
            return obj;
        }
        return null;
    }
}
