using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineComponent : MonoBehaviour
{
    [SerializeField] GameObject reference;
    [SerializeField] string componentName;

    public bool isCollected { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        isCollected = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject GetReferenceComponent()
    {
        return reference;
    }

    public string GetName()
    {
        return name;
    }

    public void MarkCollected()
    {
        isCollected = true;
    }
}
