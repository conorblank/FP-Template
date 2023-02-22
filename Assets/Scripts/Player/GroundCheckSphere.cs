using UnityEngine;


[DisallowMultipleComponent]
public sealed class GroundCheckSphere : MonoBehaviour
{
    public bool IsGrounded { private set; get; }
    
    
    //Ground Detection Settings
    [Header("Ground Detection Settings")]
    
    [SerializeField, Tooltip("Tooltip.")]
    private Vector3 positionOffset = new(0, -0.7f, 0);
    
    [SerializeField, Tooltip("Tooltip.")]
    private float radius = 0.4f;
    
    [SerializeField, Tooltip("Tooltip.")]
    private LayerMask layerMask = -1;
    
    [SerializeField, Tooltip("Tooltip.")]
    private QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore;
    
    [SerializeField, Tooltip("Tooltip.")]
    private Color gizmoColor = Color.red;
    
    [SerializeField, Tooltip("Tooltip.")]
    private Color gizmoColorGrounded = Color.green;
    
    
    private void Update()
    {
        IsGrounded = Physics.CheckSphere(transform.position + positionOffset, radius,
            layerMask, queryTriggerInteraction);
    }
    
    
    private void OnDrawGizmos()
    {
        Gizmos.color = IsGrounded ? gizmoColorGrounded : gizmoColor;
        Gizmos.DrawSphere(transform.position + positionOffset, radius);
    }
}