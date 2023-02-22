using UnityEngine;
using UnityEngine.InputSystem;


[DisallowMultipleComponent]
public sealed class InteractionRaycast : MonoBehaviour
{
    //References
    [SerializeField, Tooltip("Reference to the player input component.")]
    private PlayerInput playerInput;
    
    
    //Input Action References
    [Header("Input Action References")]
    
    [SerializeField, Tooltip("Input action references for fire")]
    private InputActionReference fireReference;
    
    
    //Awake - Assigning the correct methods to the player input actions
    private void Awake()
    {
        playerInput.actions[fireReference.action.name].performed += OnFireInput;
    }
    
    
    //On Destroy - Prevent memory leaks by un-assigning the correct methods from the player input
    private void OnDestroy()
    {
        if (!playerInput)
            return;
        
        playerInput.actions[fireReference.action.name].performed -= OnFireInput;
    }
    
    
    /// <summary>
    /// Method with input context for handling the player firing.
    /// </summary>
    private void OnFireInput(InputAction.CallbackContext context)
    {
        CastInteractionRay();
    }
    
    
    /// <summary>
    /// Casts a ray from the main camera towards mouse world position, ...
    /// </summary>
    private void CastInteractionRay()
    {
        if (Camera.main == null)
            return;
        
        //Cast the ray
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, float.MaxValue))
            return;
        
        //If we hit...
        Debug.Log(hit.transform.name);
    }
}