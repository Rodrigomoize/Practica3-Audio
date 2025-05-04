using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AmbienceZoneTrigger : MonoBehaviour
{
    [Tooltip("Nombre de la zona de ambiente a activar")]
    public string zoneName;
    
    [Tooltip("Color del Gizmo para visualizar mejor la zona en el editor")]
    public Color gizmoColor = new Color(0.2f, 0.8f, 0.5f, 0.3f);
    
    private ZoneBasedAmbienceSystem ambienceSystem;
    
    void Start()
    {
        ambienceSystem = FindObjectOfType<ZoneBasedAmbienceSystem>();
        
        if (ambienceSystem == null)
            Debug.LogError("No se ha encontrado ZoneBasedAmbienceSystem en la escena.");
        
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("El collider de la zona de ambiente debería estar configurado como Trigger.");
            col.isTrigger = true;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && ambienceSystem != null)
        {
            ambienceSystem.TransitionToZone(zoneName);
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (col is BoxCollider)
            {
                BoxCollider boxCol = col as BoxCollider;
                Gizmos.DrawCube(boxCol.center, boxCol.size);
            }
            else if (col is SphereCollider)
            {
                SphereCollider sphereCol = col as SphereCollider;
                Gizmos.DrawSphere(sphereCol.center, sphereCol.radius);
            }
            else if (col is CapsuleCollider)
            {
                CapsuleCollider capsuleCol = col as CapsuleCollider;
                Gizmos.DrawSphere(capsuleCol.center, capsuleCol.radius);
            }
        }
        
#if UNITY_EDITOR
        // Muestra el nombre de la zona
        if (!string.IsNullOrEmpty(zoneName))
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = new Color(1, 1, 1, 0.8f);
            style.alignment = TextAnchor.MiddleCenter;
            Handles.Label(transform.position, "Zona: " + zoneName, style);
        }
#endif
    }
}