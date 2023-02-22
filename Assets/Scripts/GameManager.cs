using UnityEngine;


[DisallowMultipleComponent]
public sealed class GameManager : MonoBehaviour
{
    
    
    //Settings
    [Header("Settings")]
    
    [SerializeField, Tooltip("Tooltip")]
    private bool myBool;
    
    
    public void DebugMessage(string message)
    {
        print(message);
    }
}