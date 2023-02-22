using UnityEngine;
using UnityEngine.InputSystem;


[DisallowMultipleComponent]
public sealed class PlayerCamera : MonoBehaviour
{
    private Vector3 _cameraRotation;
    private Vector2 _lookInput;
    
    
    //References
    [SerializeField, Tooltip("Tooltip.")]
    private PlayerInput playerInput;
    
    
    //Input Action References
    [Header("Input Action References")]
    
    [SerializeField, Tooltip("Input action references for mouse look around.")]
    private InputActionReference lookReference;
    
    
    //Player Camera Look Around Settings
    [Header("Player Camera Look Around Settings")]
    
    [SerializeField, Tooltip("Multiplier for mouse input on the individual axis.")]
    [Min(0f)] private Vector2 mouseSensitivity = new(0.1f, 0.1f);
    
    [SerializeField, Tooltip("Value for clamping rotation along the x-axis.")]
    [Range(0, 180)] private int verticalRotationMin = 90;
    
    [SerializeField, Tooltip("Value for clamping rotation along the x-axis.")]
    [Range(0, 180)] private int verticalRotationMax = 90;
    
    
    private void Awake()
    {
        playerInput.actions[lookReference.action.name].performed += OnLookInput;
        playerInput.actions[lookReference.action.name].canceled += OnLookInput;
    }
    
    
    //Update - Handling rotation calculations and updates based on player input
    private void Update()
    {
        Vector2 lookAxis = _lookInput;
        
        lookAxis.x *= mouseSensitivity.x;
        lookAxis.y *= mouseSensitivity.y;
        
        _cameraRotation.x -= lookAxis.y;
        _cameraRotation.y += lookAxis.x;
        
        //Clamp the x rotation
        _cameraRotation.x = Mathf.Clamp(
            _cameraRotation.x, -verticalRotationMin, verticalRotationMax);
        
        //Apply rotation
        transform.rotation = Quaternion.Euler(
            _cameraRotation.x, _cameraRotation.y, _cameraRotation.z);
        
        //Rotate the y-axis of the player root so it aligns with the way the camera is facing
        transform.parent.Rotate(Vector3.up * lookAxis.x);
    }
    
    
    private void OnDestroy()
    {
        if (!playerInput)
            return;
        
        playerInput.actions[lookReference.action.name].performed -= OnLookInput;
        playerInput.actions[lookReference.action.name].canceled -= OnLookInput;
    }
    
    
    private void OnDrawGizmos()
    {
        //Camera direction
        Gizmos.color = Color.green;
        Transform thisTransform = transform;
        Vector3 position = thisTransform.position;
        Vector3 forward = thisTransform.forward;
        Gizmos.DrawRay(position, forward.normalized * 2);
        Gizmos.DrawSphere(position + forward.normalized * 2, 0.1f);
    }
    
    
    /// <summary>
    /// Method with input context as vector2 for handling camera look around movement.
    /// </summary>
    private void OnLookInput(InputAction.CallbackContext context)
    {
        _lookInput = context.ReadValue<Vector2>();
    }
}