using System.Collections.Generic;
using UnityEngine;

public class FootstepManager : MonoBehaviour
{
    [System.Serializable]
    public class FootstepData
    {
        public string surfaceTag;
        public List<AudioClip> clips;
    }

    public List<FootstepData> footstepSounds;
    public AudioSource audioSource;
    public float stepInterval = 0.5f;

    private float stepTimer = 0f;
    private Rigidbody rb;
    private bool wasMoving = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.loop = false;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.2f);
        bool isMoving = rb.linearVelocity.magnitude > 0.1f;

        if (isGrounded && isMoving)
        {
            // Si antes estaba quieto y ahora se mueve, reproducimos inmediatamente
            if (!wasMoving)
            {
                TryPlayFootstep();
                stepTimer = 0f;
            }
            else
            {
                stepTimer += Time.deltaTime;
                if (stepTimer >= stepInterval && !audioSource.isPlaying)
                {
                    TryPlayFootstep();
                    stepTimer = 0f;
                }
            }
        }

        if (!isMoving)
        {
            stepTimer = 0f; // Reiniciar si se detiene
        }

        wasMoving = isMoving;
    }

    void TryPlayFootstep()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f))
        {
            PlayFootstep(hit.collider.tag);
        }
    }

    void PlayFootstep(string surfaceTag)
    {
        var data = footstepSounds.Find(f => f.surfaceTag == surfaceTag);
        if (data != null && data.clips.Count > 0)
        {
            AudioClip clip = data.clips[Random.Range(0, data.clips.Count)];
            audioSource.PlayOneShot(clip);
        }
    }
}
