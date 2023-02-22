using UnityEngine;


[DisallowMultipleComponent]
public sealed class CursorLockMode : MonoBehaviour
{
    [Header("Settings")]
    
    [SerializeField, Tooltip("Lock mode for the cursor.")] 
    private UnityEngine.CursorLockMode cursorLockMode = UnityEngine.CursorLockMode.Locked;
    
    
    private void Awake()
    {
        Cursor.lockState = cursorLockMode;
        //Test!
    }
}