using UnityEngine;

public class FootstepSound : MonoBehaviour
{
    public CharacterController controller;
    public AudioSource audioSource;
    public AudioClip footstepClip;
    public float stepRate = 0.5f;
    private float stepTimer;

    void Update()
    {
        if (controller.isGrounded && controller.velocity.magnitude > 0.1f)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                audioSource.PlayOneShot(footstepClip);
                stepTimer = stepRate;
            }
        }
        else stepTimer = 0f;
    }
}