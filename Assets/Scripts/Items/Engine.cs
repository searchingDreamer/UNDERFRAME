using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{
    static public Engine Instance { get; private set; }

    [SerializeField] List<GameObject> engineComponents;
    [SerializeField] float insertingTime = 2f;
    [SerializeField] float flashTime = 0.2f;
    [SerializeField] Color targetFlashColor = Color.gray;

    public event Action OnComponentInserting;
    
    private bool isInserting = false;

    private void Awake()
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
        foreach (GameObject component in engineComponents)
        {
            component.GetComponent<MeshRenderer>()?.material.EnableKeyword("_EMISSION");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (isInserting) return;
        if(!other.CompareTag("EngineComponent")) return;
        else
        {
            EngineComponent component = other.gameObject.GetComponent<EngineComponent>();
            Debug.Log("Component entered");
            for (int i = 0; i < engineComponents.Count; i++)
            {
                if (component.GetReferenceComponent() == engineComponents[i])
                {
                    OnComponentInserting?.Invoke();
                    StartCoroutine(InsertEngineComponent(engineComponents[i], component.gameObject));
                    isInserting = true;
                }
            }
        }
    }

    private IEnumerator InsertEngineComponent(GameObject engineComponent, GameObject insertingComponent)
    {
        insertingComponent.GetComponent<Rigidbody>().isKinematic = true;
        float timer = Time.fixedDeltaTime;
        float t;
        Vector3 targetPos = engineComponent.transform.position;
        Quaternion targetRot = engineComponent.transform.rotation;
        Vector3 startPos = insertingComponent.transform.position;
        Quaternion startRot = insertingComponent.transform.rotation;

        while (timer < insertingTime)
        {
            t = timer / insertingTime;

            insertingComponent.transform.position = Vector3.Lerp(startPos, targetPos, t);
            insertingComponent.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        insertingComponent.SetActive(false);
        insertingComponent.GetComponent<EngineComponent>().MarkCollected();
        engineComponent.SetActive(true);
        StartCoroutine(FlashComponent(engineComponent));
        isInserting = false;
    }

    private IEnumerator FlashComponent(GameObject engineComponent)
    {
        float timer = Time.fixedDeltaTime;
        float t;
        float halfPeriod = flashTime / 2;
        Color emissionColor = engineComponent.GetComponent<MeshRenderer>().material.GetColor("_EmissionColor");
        Color initialColor = engineComponent.GetComponent<MeshRenderer>().material.GetColor("_EmissionColor");

        for (int i = 0; i < 2; i++)
        {
            while (timer < halfPeriod)
            {
                t = timer / halfPeriod;
                if(i == 0) emissionColor = Color.Lerp(initialColor, targetFlashColor, t);
                else emissionColor = Color.Lerp(targetFlashColor, initialColor, t);

                engineComponent.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", emissionColor);

                timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            timer = Time.fixedDeltaTime;
        }

        engineComponent.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", initialColor);
    }
}
