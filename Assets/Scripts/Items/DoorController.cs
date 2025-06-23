using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] float closingDelay = 3f;
    private float openTime = 2f;
    private bool isOpen = false;

    Quaternion initialRotation;
    Quaternion targetRotation;

    private Coroutine openCoroutine;
    private Coroutine closingCoroutine;

    [SerializeField] bool reverse;

    private void Start()
    {
        initialRotation = transform.rotation;
    }

    // false - in, true - out
    public void Open(Vector3 playerPosition)
    {
        if(isOpen)
        {
            StopCoroutine(closingCoroutine);
            closingCoroutine = StartCoroutine(CloseTimer());
        }
        else
        {
            Vector3 toPlayer = (playerPosition - transform.position).normalized;
            Vector3 doorForward = transform.forward;

            float dot = Vector3.Dot(doorForward, toPlayer);
            float angle;
            if (reverse) angle = (dot < 0) ? -90f : 90f;
            else angle = (dot >= 0) ? -90f : 90f;

            targetRotation = Quaternion.AngleAxis(angle, Vector3.up) * initialRotation;

            if(openCoroutine != null) StopCoroutine(openCoroutine);
            openCoroutine = StartCoroutine(SmoothRotate());
            closingCoroutine = StartCoroutine(CloseTimer());

            isOpen = true;
        }
    }

    private void Close()
    {
        targetRotation = initialRotation;
        openCoroutine = StartCoroutine(SmoothRotate());
        isOpen = false;
    }

    private IEnumerator SmoothRotate()
    {
        float timer = Time.fixedDeltaTime;
        float t;
        Quaternion target = targetRotation;

        while(timer < openTime)
        {
            t = timer / openTime;

            gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, target, t);

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator CloseTimer()
    {
        yield return new WaitForSeconds(closingDelay);

        Close();
    }
}
