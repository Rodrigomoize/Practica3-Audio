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

    private CharacterController controller;
    private float stepTimer;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (controller.isGrounded && controller.velocity.magnitude > 0.1f)
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
