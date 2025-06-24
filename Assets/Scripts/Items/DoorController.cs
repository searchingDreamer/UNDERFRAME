using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] float closingDelay = 6f;
    private float openTime = 1f;
    private bool isOpen = false;

    Quaternion initialRotation;
    Quaternion targetRotation;

    private Coroutine openCoroutine;
    private Coroutine closingCoroutine;

    [SerializeField] bool reverse;

    [SerializeField] AudioSource audioSource;
    [SerializeField] List<AudioClip> rotatingSounds;

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
            audioSource.clip = rotatingSounds[Random.Range(0, rotatingSounds.Count)];
            audioSource.Play();
            openCoroutine = StartCoroutine(SmoothRotate());
            closingCoroutine = StartCoroutine(CloseTimer());

            isOpen = true;
        }
    }

    private void Close()
    {
        targetRotation = initialRotation;
        audioSource.clip = rotatingSounds[Random.Range(0, rotatingSounds.Count)];
        audioSource.Play();
        openCoroutine = StartCoroutine(SmoothRotate());
        isOpen = false;
    }

    private IEnumerator SmoothRotate()
    {
        float timer = Time.fixedDeltaTime;
        float t;
        Quaternion initial = gameObject.transform.rotation;
        Quaternion target = targetRotation;

        while(timer < openTime)
        {
            t = timer / openTime;

            gameObject.transform.rotation = Quaternion.Slerp(initial, target, t);

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        audioSource.Stop();
    }

    private IEnumerator CloseTimer()
    {
        yield return new WaitForSeconds(closingDelay);

        Close();
    }
}
