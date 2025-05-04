using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioLowPassFilter))]
public class AudioOcclusion : MonoBehaviour
{
    public float defaultCutoffFrequency = 22000f;

    public float occludedCutoffFrequency = 1500f;

    public Transform listener;

    public LayerMask occlusionLayers = -1;

    private AudioSource audioSource;
    private AudioLowPassFilter lowPassFilter;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        lowPassFilter = GetComponent<AudioLowPassFilter>();

        if (listener == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                listener = mainCamera.transform;
            }
        }
    }

    void Update()
    {
        if (listener == null) return;

        Vector3 direction = listener.position - transform.position;
        float distance = direction.magnitude;

        direction.Normalize();

        RaycastHit hit;
        bool isOccluded = Physics.Raycast(
            transform.position,
            direction,
            out hit,
            distance,
            occlusionLayers
        );

        if (isOccluded)
        {
            float occlusionFactor = hit.distance / distance;

            float targetCutoff = Mathf.Lerp(
                occludedCutoffFrequency,
                defaultCutoffFrequency,
                occlusionFactor
            );

            lowPassFilter.cutoffFrequency = Mathf.Lerp(
                lowPassFilter.cutoffFrequency,
                targetCutoff,
                Time.deltaTime * 5f
            );
        }
        else
        {
            lowPassFilter.cutoffFrequency = Mathf.Lerp(
                lowPassFilter.cutoffFrequency,
                defaultCutoffFrequency,
                Time.deltaTime * 5f
            );
        }
    }
}