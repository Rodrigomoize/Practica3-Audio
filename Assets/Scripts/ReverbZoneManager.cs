using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class ReverbZoneManager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public AudioMixerSnapshot zoneSnapshot;
    public AudioMixerSnapshot defaultSnapshot;
    public float transitionTime = 1.0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (zoneSnapshot != null)
            {
                zoneSnapshot.TransitionTo(transitionTime);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (defaultSnapshot != null)
            {
                defaultSnapshot.TransitionTo(transitionTime);
            }
        }
    }
}