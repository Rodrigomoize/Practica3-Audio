using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class SimplifiedUnderwaterManager : MonoBehaviour
{
    [Header("Referencias de Audio")]
    public AudioClip underwaterAmbience;

    [Header("Configuración del Mixer")]
    public AudioMixer audioMixer;
    public AudioMixerSnapshot underwaterSnapshot;
    public AudioMixerSnapshot defaultSnapshot;
    public float transitionTime = 1.0f;

    [Header("Configuración de Efectos")]
    public float fadeSpeed = 1.5f;

    private AudioSource playerUnderwaterSource;
    private bool isUnderwater = false;
    private GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerUnderwaterSource = player.AddComponent<AudioSource>();
            SetupUnderwaterAudioSource();
        }
        else
        {
            Debug.LogError("No se encontró al jugador con tag 'Player'. Asegúrate de que el jugador tenga este tag.");
        }
    }

    private void SetupUnderwaterAudioSource()
    {
        if (playerUnderwaterSource != null)
        {
            playerUnderwaterSource.clip = underwaterAmbience;
            playerUnderwaterSource.loop = true;
            playerUnderwaterSource.playOnAwake = false;
            playerUnderwaterSource.spatialBlend = 0f;
            playerUnderwaterSource.volume = 0f;

            if (audioMixer != null)
            {
                AudioMixerGroup[] groups = audioMixer.FindMatchingGroups("Master/Ambiente/Underwater");
                if (groups.Length > 0)
                {
                    playerUnderwaterSource.outputAudioMixerGroup = groups[0];
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isUnderwater)
        {
            EnterWater();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isUnderwater)
        {
            ExitWater();
        }
    }

    private void EnterWater()
    {
        isUnderwater = true;

        if (underwaterSnapshot != null)
        {
            underwaterSnapshot.TransitionTo(transitionTime);
        }

        if (underwaterAmbience != null && playerUnderwaterSource != null)
        {
            playerUnderwaterSource.Play();
            StartCoroutine(FadeAudioSource(playerUnderwaterSource, 1f, fadeSpeed));
        }

        FootstepManager footsteps = player.GetComponent<FootstepManager>();
        if (footsteps != null)
        {
            footsteps.enabled = false;
        }
    }

    private void ExitWater()
    {
        isUnderwater = false;

        if (defaultSnapshot != null)
        {
            defaultSnapshot.TransitionTo(transitionTime);
        }

        if (playerUnderwaterSource != null && playerUnderwaterSource.isPlaying)
        {
            StartCoroutine(FadeAudioSource(playerUnderwaterSource, 0f, fadeSpeed));
        }

        FootstepManager footsteps = player.GetComponent<FootstepManager>();
        if (footsteps != null)
        {
            footsteps.enabled = true;
        }
    }

    private IEnumerator FadeAudioSource(AudioSource source, float targetVolume, float speed)
    {
        float startVolume = source.volume;
        float time = 0;

        while (time < 1)
        {
            time += Time.deltaTime * speed;
            source.volume = Mathf.Lerp(startVolume, targetVolume, time);
            yield return null;
        }

        if (targetVolume <= 0 && source.isPlaying)
        {
            source.Stop();
        }
    }
}