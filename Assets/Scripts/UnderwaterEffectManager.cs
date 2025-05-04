using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class SimplifiedUnderwaterManager : MonoBehaviour
{
    [Header("Referencias de Audio")]
    // Sonido que se reproducirá mientras el jugador esté bajo el agua
    public AudioClip underwaterAmbience;

    [Header("Configuración del Mixer")]
    public AudioMixer audioMixer;
    public AudioMixerSnapshot underwaterSnapshot;
    public AudioMixerSnapshot defaultSnapshot;
    public float transitionTime = 1.0f;

    [Header("Configuración de Efectos")]
    // Velocidad del fade in/out para el sonido ambiental submarino
    public float fadeSpeed = 1.5f;

    // Referencias privadas
    private AudioSource playerUnderwaterSource;
    private bool isUnderwater = false;
    private GameObject player;

    void Start()
    {
        // Buscar el objeto jugador (asumiendo que tiene tag "Player")
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // Crear un AudioSource en el jugador para el sonido submarino
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
            playerUnderwaterSource.spatialBlend = 0f; // Sonido 2D (sigue al jugador)
            playerUnderwaterSource.volume = 0f; // Empezar con volumen 0

            // Asignar al grupo adecuado en el mixer (opcional)
            if (audioMixer != null)
            {
                // Asumimos que hay un grupo llamado "Underwater" en el mixer
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

        // Cambiar al snapshot de bajo el agua
        if (underwaterSnapshot != null)
        {
            underwaterSnapshot.TransitionTo(transitionTime);
        }

        // Iniciar el sonido de ambiente submarino
        if (underwaterAmbience != null && playerUnderwaterSource != null)
        {
            playerUnderwaterSource.Play();
            StartCoroutine(FadeAudioSource(playerUnderwaterSource, 1f, fadeSpeed));
        }

        // Deshabilitar temporalmente el FootstepManager del jugador si existe
        FootstepManager footsteps = player.GetComponent<FootstepManager>();
        if (footsteps != null)
        {
            footsteps.enabled = false;
        }
    }

    private void ExitWater()
    {
        isUnderwater = false;

        // Volver al snapshot por defecto
        if (defaultSnapshot != null)
        {
            defaultSnapshot.TransitionTo(transitionTime);
        }

        // Fade out del sonido submarino
        if (playerUnderwaterSource != null && playerUnderwaterSource.isPlaying)
        {
            StartCoroutine(FadeAudioSource(playerUnderwaterSource, 0f, fadeSpeed));
        }

        // Volver a habilitar el FootstepManager
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

        // Si el volumen objetivo es 0, parar el audio
        if (targetVolume <= 0 && source.isPlaying)
        {
            source.Stop();
        }
    }
}