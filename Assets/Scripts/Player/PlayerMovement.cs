/*
coyote ground delay
input before jump (like target speed)
0 to walk momentum
velocity mod based on look at
velocity based jump
crouching
refactor!
walking down stairs? small obstacles? slopes?

effects:
fov
camera (step) shake
sound
jump squish
crouch squish
 */


using UnityEngine;
using UnityEngine.InputSystem;


[DisallowMultipleComponent]
public sealed class PlayerMovement : MonoBehaviour
{
    private Vector3 _positionPreviousFrame;
    private Vector3 _velocity;
    private Vector3 _fallingVelocity;
    private Vector3 _motion;
    private Vector2 _movementInput;
    private float _targetSpeed;
    private float _jumpCoyoteTimer;
    private bool _isBoostedInput;
    private bool _isJumpingInput;
    private bool _isGrounded;
    private bool _canJump;
    
    
    //References
    [SerializeField, Tooltip("Tooltip")]
    private PlayerInput playerInput;

    [SerializeField, Tooltip("Tooltip")]
    private CharacterController characterController;
    
    [SerializeField, Tooltip("Tooltip")]
    private GroundCheckSphere groundCheckSphere;
    
    
    //Input Action References
    [Header("Input Action References")]
    
    [SerializeField, Tooltip("Tooltip")]
    private InputActionReference moveReference;
    
    [SerializeField, Tooltip("Tooltip")]
    private InputActionReference boostReference;
    
    [SerializeField, Tooltip("Tooltip")]
    private InputActionReference jumpReference;
    
    
    //Movement
    [Header("Movement")]
    
    [SerializeField, Tooltip("Tooltip")]
    [Min(0)] private float walkingSpeed = 6f;
    
    [SerializeField, Tooltip("Tooltip")]
    [Min(0)] private float runningSpeed = 10f;
    
    [SerializeField, Tooltip("Tooltip")]
    [Min(0)] private float speedLerpMagnitude = 12f;
    
    [SerializeField, Tooltip("Tooltip")]
    [Min(0)] private float groundedFallingVelocity = 6f;
    
    
    //Jumping
    [Header("Jumping")]
    
    [SerializeField, Tooltip("Tooltip")]
    [Min(0)] private float jumpHeight = 2f;
    
    [SerializeField, Tooltip("Tooltip")]
    [Min(0)] private float jumpCoyoteTime = 0.05f;
    
    
    //In Air
    [Header("In Air")]
    
    [SerializeField, Tooltip("Tooltip")]
    private bool airControl;
    
    [SerializeField, Tooltip("Tooltip")]
    [Min(0)] private float airFriction = 1f;
    
    [SerializeField, Tooltip("Tooltip")]
    private float gravity = -19.62f;
    

    private void Awake()
    {
        playerInput.actions[moveReference.action.name].performed += OnMovementInput;
        playerInput.actions[moveReference.action.name].canceled += OnMovementInput;
        
        playerInput.actions[boostReference.action.name].performed += OnBoostInput;
        playerInput.actions[boostReference.action.name].canceled += OnBoostInput;
        
        playerInput.actions[jumpReference.action.name].performed += OnJumpInput;
        playerInput.actions[jumpReference.action.name].canceled += OnJumpInput;
    }
    
    
    private void Update()
    {
        //Ground check
        _isGrounded = groundCheckSphere.IsGrounded;
        
        
        //Player velocity
        Vector3 position = transform.position;
        _velocity = (position - _positionPreviousFrame) / Time.deltaTime;
        
        
        //Jumping coyote time
        if (_isGrounded)
        {
            _jumpCoyoteTimer = 0;
            _canJump = true;
        }
        else
        {
            if (_jumpCoyoteTimer > jumpCoyoteTime)
                _canJump = false;
            else
                _jumpCoyoteTimer += Time.deltaTime;
        }
        
        
        //Jumping
        if (_isJumpingInput && _canJump)
        {
            _canJump = false;
            _fallingVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        
        
        //Gravity
        if (_isGrounded && _fallingVelocity.y < 0)
            _fallingVelocity.y = -groundedFallingVelocity;
        
        _fallingVelocity.y += gravity * Time.deltaTime;
        
        characterController.Move(_fallingVelocity * Time.deltaTime);
        
        
        //Target speed
        if (_isGrounded)
            _targetSpeed = Mathf.Lerp(_targetSpeed, _isBoostedInput ?
                runningSpeed : walkingSpeed, speedLerpMagnitude * Time.deltaTime);
        else
            _targetSpeed = Mathf.Lerp(_targetSpeed, 0, airFriction * Time.deltaTime);
        
        
        //Move & air control
        if (_isGrounded || airControl)
        {
            Transform thisTransform = transform;

            if ((thisTransform.right * _movementInput.x + thisTransform.forward * _movementInput.y)
                .magnitude > 0.5f)
            {
                _motion = thisTransform.right * _movementInput.x +
                          thisTransform.forward * _movementInput.y;

            }
            else
            {
                _motion = Vector3.Lerp(_motion, Vector3.zero, 24f * Time.deltaTime);
            }
        }
        characterController.Move(_motion * (_targetSpeed * Time.deltaTime));
        
        
        //Frame memory
        _positionPreviousFrame = position;
    }
    
    
    private void OnDestroy()
    {
        if (!playerInput)
            return;

        playerInput.actions[moveReference.action.name].performed -= OnMovementInput;
        playerInput.actions[moveReference.action.name].canceled -= OnMovementInput;
        
        playerInput.actions[boostReference.action.name].performed -= OnBoostInput;
        playerInput.actions[boostReference.action.name].canceled -= OnBoostInput;
        
        playerInput.actions[jumpReference.action.name].performed -= OnJumpInput;
        playerInput.actions[jumpReference.action.name].canceled -= OnJumpInput;
    }
    
    
    private void OnDrawGizmos()
    {
        //Velocity
        Gizmos.color = Color.magenta;
        Vector3 position = transform.position;
        Gizmos.DrawRay(position, _velocity.normalized * 1.5f);
        Gizmos.DrawSphere(position + _velocity.normalized * 1.5f, 0.2f);
    }
    
    
    private void OnMovementInput(InputAction.CallbackContext context)
    {
        _movementInput = context.ReadValue<Vector2>();
    }
    
    
    private void OnBoostInput(InputAction.CallbackContext context)
    {
        _isBoostedInput = context.ReadValueAsButton();
    }
    
    
    private void OnJumpInput(InputAction.CallbackContext context)
    {
        _isJumpingInput = context.ReadValueAsButton();
    }
}