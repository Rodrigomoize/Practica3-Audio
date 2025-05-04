using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AmbienceZone
{
    public string zoneName;
    public AudioClip ambienceClip;
    public float fadeInTime = 2.0f;
    public float fadeOutTime = 3.0f;
    public bool loopAudio = true;
    [Range(0f, 1f)]  // Changed range to 0-1
    public float volume = 1.0f;  // Changed default to 1.0 (100%)
    
    // Referencia al AudioMixerGroup
    public AudioMixerGroup outputGroup;
    
    [Range(0, 1)]
    public float spatialBlend = 0; 
    
    [HideInInspector]
    public AudioSource audioSource;
}

public class ZoneBasedAmbienceSystem : MonoBehaviour
{
    [Header("Global Settings")]
    public float transitionTime = 2.0f;
    public Transform player;
    
    [Header("Debug")]
    public bool showDebugMessages = true;
    
    [Header("Ambiente Zones")]
    public List<AmbienceZone> ambienceZones = new List<AmbienceZone>();
    
    private AmbienceZone currentZone;
    private AmbienceZone previousZone;
    private Dictionary<string, AmbienceZone> zonesByName = new Dictionary<string, AmbienceZone>();
    
    void Awake()
    {
        // Encuentra automáticamente al jugador si no se ha asignado
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
            
        // Configura las fuentes de audio para cada zona
        SetupAudioSources();
        
        // Crea un diccionario para un acceso más rápido por nombre
        foreach (var zone in ambienceZones)
        {
            if (!string.IsNullOrEmpty(zone.zoneName))
            {
                zonesByName[zone.zoneName] = zone;
                DebugLog("Registrada zona: " + zone.zoneName);
            }
            else
            {
                Debug.LogWarning("Se encontró una zona sin nombre en ZoneBasedAmbienceSystem");
            }
        }
    }
    
    void SetupAudioSources()
    {
        foreach (var zone in ambienceZones)
        {
            if (zone.ambienceClip == null)
            {
                Debug.LogWarning("Zona '" + zone.zoneName + "' no tiene un clip de audio asignado.");
                continue;
            }
            
            // Crear un objeto hijo para cada fuente de audio
            GameObject audioObj = new GameObject("Ambience_" + zone.zoneName);
            audioObj.transform.parent = transform;
            
            // Configurar el AudioSource
            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.clip = zone.ambienceClip;
            source.loop = zone.loopAudio;
            source.volume = 1.0f;  // Empezamos con volumen 0
            
            // Aseguramos que la espacialización 3D se configura correctamente
            source.spatialBlend = zone.spatialBlend;
            // Forzamos la actualización de la configuración 3D
            if (zone.spatialBlend > 0)
            {
                DebugLog("Configurando sonido 3D para zona: " + zone.zoneName + " con spatialBlend: " + zone.spatialBlend);
            }
            
            source.playOnAwake = false; // No reproducimos hasta que se necesite
            
            // Asignar al grupo de salida si existe
            if (zone.outputGroup != null)
            {
                source.outputAudioMixerGroup = zone.outputGroup;
                DebugLog("Asignado AudioMixerGroup a zona: " + zone.zoneName);
            }
            
            // Si el sonido es 3D, configuramos propiedades adicionales de forma explícita
            if (zone.spatialBlend > 0)
            {
                // Configuración explícita para sonidos 3D
                source.spatialize = true;
                source.spatialBlend = zone.spatialBlend; // Aplicamos de nuevo para asegurar
                source.rolloffMode = AudioRolloffMode.Logarithmic;
                source.minDistance = 5f;
                source.maxDistance = 50f;
                source.spread = 0f; // Definimos una directividad (0 es muy direccional)
                
                // Log para verificar la configuración
                DebugLog("Configuración 3D aplicada para zona: " + zone.zoneName + 
                         " | SpatialBlend: " + source.spatialBlend + 
                         " | Spatialize: " + source.spatialize);
            }
            
            // Guardar la referencia al AudioSource
            zone.audioSource = source;
            
            // Iniciamos la reproducción pero con volumen 0
            if (zone.loopAudio)
            {
                source.Play();
                DebugLog("Iniciada reproducción de audio para zona: " + zone.zoneName + " (volumen inicial 0)");
            }
        }
    }
    
    void Start()
    {
        // Comprobar si el jugador está inicialmente dentro de alguna zona
        CheckInitialPlayerPosition();
    }
    
    // Método para verificar si el jugador ya está en alguna zona al inicio
    private void CheckInitialPlayerPosition()
    {
        // Esto es solo para depuración inicial
        DebugLog("Verificando posición inicial del jugador");
    }
    
    // Método para cambiar a una zona específica por nombre
    public void TransitionToZone(string zoneName)
    {
        DebugLog("Intentando transición a zona: " + zoneName);
        
        if (zonesByName.TryGetValue(zoneName, out AmbienceZone newZone))
        {
            TransitionToZone(newZone);
        }
        else
        {
            Debug.LogWarning("Zona '" + zoneName + "' no encontrada en el sistema de ambiente. Zonas disponibles: " + 
                string.Join(", ", zonesByName.Keys));
        }
    }
    
    // Método para cambiar a una zona específica por referencia
    public void TransitionToZone(AmbienceZone newZone)
    {
        if (newZone == null)
        {
            Debug.LogError("Intentando transicionar a una zona nula");
            return;
        }
        
        if (newZone == currentZone)
        {
            DebugLog("Ya estamos en la zona: " + newZone.zoneName);
            return;
        }
            
        DebugLog("Transición de " + (currentZone != null ? currentZone.zoneName : "ninguna") + 
                " a " + newZone.zoneName);
                
        previousZone = currentZone;
        currentZone = newZone;
            
        // Iniciar la reproducción del nuevo audio si es necesario
        if (newZone.audioSource != null)
        {
            if (!newZone.audioSource.isPlaying)
            {
                newZone.audioSource.Play();
                DebugLog("Iniciando reproducción de audio para zona: " + newZone.zoneName);
            }
            
            // Transición de volumen para el audio entrante
            StartCoroutine(FadeAudioSource(newZone.audioSource, newZone.audioSource.volume, newZone.volume, newZone.fadeInTime));
        }
        else
        {
            Debug.LogError("AudioSource es nulo para la zona: " + newZone.zoneName);
        }
        
        // Transición de volumen para el audio saliente
        if (previousZone != null && previousZone.audioSource != null)
        {
            StartCoroutine(FadeAudioSource(previousZone.audioSource, previousZone.audioSource.volume, 0, previousZone.fadeOutTime));
        }
    }
    
    // Corrutina para hacer una transición suave de volumen
    private IEnumerator FadeAudioSource(AudioSource source, float startVolume, float targetVolume, float duration)
    {
        DebugLog("Iniciando fade de " + startVolume + " a " + targetVolume + " durante " + duration + " segundos");
        
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        source.volume = targetVolume;
        DebugLog("Fade completado. Volumen final: " + source.volume);
        
        // Si el volumen objetivo es 0, podemos pausar la reproducción para ahorrar recursos
        if (targetVolume <= 0 && source.isPlaying)
        {
            source.Pause();
            DebugLog("Audio pausado porque el volumen es 0");
        }
    }
    
    private void DebugLog(string message)
    {
        if (showDebugMessages)
            Debug.Log("[Ambience] " + message);
    }
}