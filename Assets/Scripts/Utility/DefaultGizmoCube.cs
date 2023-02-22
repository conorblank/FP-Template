using UnityEngine;


/// <summary>
/// Class for drawing a gizmo cube, used for visualizing otherwise invisible items.
/// </summary>
public sealed class DefaultGizmoCube : MonoBehaviour
{
    [Header("Settings")]
    
    [SerializeField, Tooltip("Offset for the position of the gizmo cube.")]
    private Vector3 positionOffset;
    
    [SerializeField, Tooltip("Size of the gizmo cube.")]
    [Min(0)] private Vector3 size = Vector3.one;
    
    [SerializeField, Tooltip("Color of the gizmo cube, including an alpha.")]
    private Color color = Color.magenta;
    
    
    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawCube(transform.position + positionOffset, size);
    }
}