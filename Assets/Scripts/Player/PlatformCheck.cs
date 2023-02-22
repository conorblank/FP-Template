using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[DisallowMultipleComponent]
public class PlatformCheck : MonoBehaviour
{
    //Settings
    [Space(8), Header("Settings")]
    
    [SerializeField, Tooltip("Tooltip")]
    private LayerMask mask;
    
    [SerializeField, Tooltip("Tooltip")]
    private Vector3 position;
    
    [SerializeField, Tooltip("Tooltip")]
    private Vector3 size;
    
    [SerializeField, Tooltip("Tooltip")]
    private List<string> tagsToCheckFor;
    
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position + position, size);
    }
    
    
    private void Update()
    {
        CheckForPlatform();
    }
    
    
    //Check For Platform
    private void CheckForPlatform()
    {
        Transform thisTransform = transform;
        thisTransform.SetParent(null);
        
        // ReSharper disable once Unity.PreferNonAllocApi
        Collider[] colliders = Physics.OverlapBox(thisTransform.position + position,
            size / 2, thisTransform.rotation, mask, QueryTriggerInteraction.Ignore);
        
        foreach (Collider other in colliders)
        {
            if (tagsToCheckFor.Any(checkTag => other.CompareTag(checkTag)) == false)
                continue;
            
            thisTransform.SetParent(other.transform);
        }
    }
}