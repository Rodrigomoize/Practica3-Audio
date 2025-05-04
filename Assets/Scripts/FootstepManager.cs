using System.Collections.Generic;
using UnityEngine;
using System.Collections;

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
    public float walkStepInterval = 0.5f;
    public float runStepInterval = 0.3f;
    public float fadeOutDuration = 0.2f;

    private float stepTimer = 0f;
    private Rigidbody rb;
    private bool wasGrounded = true;
    private bool wasMoving = false;
    private string currentSurfaceTag = "";
    private Coroutine fadeOutCoroutine;

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
        float speed = rb.linearVelocity.magnitude;
        bool isMoving = speed > 0.1f;
        bool isRunning = isMoving && Input.GetKey(KeyCode.LeftShift);
        float currentStepInterval = isRunning ? runStepInterval : walkStepInterval;

        if (!isGrounded || !isMoving)
        {
            if (audioSource.isPlaying && fadeOutCoroutine == null)
            {
                fadeOutCoroutine = StartCoroutine(FadeOutAudio());
            }
            stepTimer = 0f;
        }
        else
        {
            if (fadeOutCoroutine != null)
            {
                StopCoroutine(fadeOutCoroutine);
                fadeOutCoroutine = null;
                audioSource.volume = 1f;
            }

            UpdateCurrentSurface();

            if (!wasMoving || !wasGrounded)
            {
                TryPlayFootstep();
                stepTimer = 0f;
            }
            else
            {
                stepTimer += Time.deltaTime;
                if (stepTimer >= currentStepInterval)
                {
                    TryPlayFootstep();
                    stepTimer = 0f;
                }
            }
        }

        wasMoving = isMoving;
        wasGrounded = isGrounded;
    }

    void UpdateCurrentSurface()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f))
        {
            currentSurfaceTag = hit.collider.tag;
        }
    }

    void TryPlayFootstep()
    {
        if (!string.IsNullOrEmpty(currentSurfaceTag))
        {
            PlayFootstep(currentSurfaceTag);
        }
    }

    void PlayFootstep(string surfaceTag)
    {
        var data = footstepSounds.Find(f => f.surfaceTag == surfaceTag);
        if (data != null && data.clips.Count > 0)
        {
            AudioClip clip = data.clips[Random.Range(0, data.clips.Count)];
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.volume = 1f;
            audioSource.Play();
        }
    }

    IEnumerator FadeOutAudio()
    {
        float startVolume = audioSource.volume;
        float startTime = Time.time;

        while (Time.time < startTime + fadeOutDuration)
        {
            float elapsed = Time.time - startTime;
            float normalizedTime = elapsed / fadeOutDuration;
            audioSource.volume = Mathf.Lerp(startVolume, 0, normalizedTime);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = 1f;
        fadeOutCoroutine = null;
    }
}