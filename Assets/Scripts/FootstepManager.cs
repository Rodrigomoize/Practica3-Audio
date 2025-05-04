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

    private float stepTimer;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (IsGrounded() && IsMoving())
        {
            stepTimer += Time.deltaTime;
            if (stepTimer > stepInterval)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
                {
                    PlayFootstep(hit.collider.tag);
                }
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.2f); // Ajusta si tu personaje es m�s alto o bajo
    }

    bool IsMoving()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        return rb != null && rb.linearVelocity.magnitude > 0.1f;
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
