using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public enum ItemType
{
    Light,
    Heavy
}

public class ValuableItem : MonoBehaviour
{
    [SerializeField] int cost = 1000;
    [SerializeField] float fragility = 1f;
    [SerializeField] float destroyDelay = 0.3f;
    [SerializeField] AudioSource audioSource;
    public AudioClip hitSound;
    public AudioClip destroySound;
    public AudioClip baseSound;
    public AudioClip extraSound;

    public event Action OnHit;
    public event Action OnDestroy;
    private Coroutine extraSoundCoroutine;

    private bool detectCollisions = false;

    private GameObject minimapMarker;

    [SerializeField] ItemType type;

    private void Start()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!detectCollisions) return;

        Vector3 impulse = collision.impulse;
        float impactForce = impulse.magnitude / Time.fixedDeltaTime;

        int damage = (int)(impactForce * fragility);
        if (damage > 0)
        {
            cost -= damage;
            if (cost <= 0)
            {
                cost = 0;
                OnDestroy?.Invoke();
                audioSource.Stop();
                SoundOnDestroy();
                StartCoroutine(DestroyAfterDelay());
            }

            OnHit?.Invoke();
            SoundOnHit();
        }
    }

    private void FixedUpdate()
    {
        if (minimapMarker != null)
        {
            minimapMarker.gameObject.transform.position = gameObject.transform.position + Vector3.up * 30;
        }
    }
    public int GetCost()
    {
        return cost;
    }

    public void ActivateCollision()
    {
        detectCollisions = true;
    }

    private void SoundOnHit()
    {
        if(!audioSource || !hitSound) return;
        float volume = UnityEngine.Random.Range(0.5f, 0.7f);
        audioSource.PlayOneShot(hitSound, volume);
    }

    private void SoundOnDestroy()
    {
        if (!audioSource || !destroySound) return;
        float volume = UnityEngine.Random.Range(0.8f, 1f);
        audioSource.PlayOneShot(destroySound, volume);
    }

    private IEnumerator DestroyAfterDelay()
    {
        detectCollisions = false;
        gameObject.layer = LayerMask.NameToLayer("Destroy");
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
        if(minimapMarker != null) Destroy(minimapMarker);
    }

    public void SetMinimapMarker(GameObject marker)
    {
        minimapMarker = marker;
        minimapMarker.gameObject.transform.position = gameObject.transform.position + Vector3.up * 30;
    }

    public ItemType GetItemType()
    {
        return type;
    }

    public void PlayExtraSound()
    {
        if(audioSource == null) return;
        if(extraSound == null) return;
        if (extraSoundCoroutine != null) return;
        audioSource.Stop();
        audioSource.PlayOneShot(extraSound);
        extraSoundCoroutine = StartCoroutine(PlayAfterDelay(extraSound.length));
    }

    private IEnumerator PlayAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (baseSound != null)
        {
            audioSource.Stop();
            audioSource.clip = baseSound;
            audioSource.Play();
        }
        extraSoundCoroutine = null;
    }

    private void OnEnable()
    {
        if (baseSound == null) return;
        audioSource.clip = baseSound;
        audioSource.Play();
    }
}
