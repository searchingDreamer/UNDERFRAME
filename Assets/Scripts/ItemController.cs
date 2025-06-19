using System;
using System.Collections;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    [SerializeField] int cost = 1000;
    [SerializeField] float fragility = 1f;
    [SerializeField] float collisionDetectionDelay = 1f;
    private bool detectCollisions = false;

    public event Action OnDestroy;

    // Новые события для звуков
    public event Action OnHit;
    public event Action OnBreak;

    void Start()
    {
        StartCoroutine(ActivateCollisions(collisionDetectionDelay));
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!detectCollisions) return;

        Vector3 impulse = collision.impulse;
        float impactForce = impulse.magnitude / Time.fixedDeltaTime;

        Debug.Log("Hit strength: " + impactForce);

        int damage = (int)(impactForce * fragility);
        if (damage > 0)
        {
            cost -= damage;
            OnHit?.Invoke();  // Сообщаем о ударе звуковому менеджеру
        }

        if (cost <= 0)
        {
            OnDestroy?.Invoke();
            OnBreak?.Invoke(); // Сообщаем о разрушении звуковому менеджеру
            Destroy(gameObject);
        }
    }

    IEnumerator ActivateCollisions(float delay)
    {
        yield return new WaitForSeconds(delay);
        detectCollisions = true;
    }

    public int GetCost()
    {
        return cost;
    }
}
