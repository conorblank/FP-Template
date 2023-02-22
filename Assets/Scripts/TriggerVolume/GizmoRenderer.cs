using UnityEngine;


[DisallowMultipleComponent]
public class GizmoRenderer : MonoBehaviour
{
    public bool IsTriggered
    {
        set => _gizmoColor = value ? _gizmoColorTriggered : gizmoColor;
    }
    
    
    private Component[] _colliderComponents;
    private TriggerVolume _triggerVolume;
    private Mesh _cylinder;
    private Color _gizmoColor;
    private Color _gizmoColorTriggered;
    
    
    //Settings
    [Header("Settings")]
    
    [SerializeField, Tooltip("Should the gizmos be visible in the editor at all?")]
    private bool drawGizmos = true;
    
    [SerializeField, Tooltip(
         "Should the gizmos be visible in the editor only when they are selected?")]
    private bool drawOnlyWhileSelected;
    
    [SerializeField, Tooltip("Draws a box gizmo based on transform without rotation. " +
                             "Only works if there are no collider components.")]
    private bool drawDefaultBox;
    
    
    //Color
    [Header("Color")]
    
    [SerializeField, Tooltip(
         "Color of the gizmos. Also effects the gizmos wireframe color and trigger color.")]
    private Color gizmoColor = new(0.2f, 1f, 0.2f, 0.25f);
    
    [SerializeField, Tooltip("Color difference for the gizmos when triggered.")]
    private float triggeredColorDifference = 0.3f;
    
    
    //Wireframe
    [Header("Wireframe")]
    
    [SerializeField, Tooltip("Whether to draw the wireframe of the gizmos or not")]
    private bool drawWireframe = true;
    
    [SerializeField, Tooltip("Opacity of the gizmos wireframe if displayed")]
    [Range(0, 1f)]
    private float wireframeOpacity = 0.25f;
    
    
    private void Reset()
    {
        GetBuiltinCylinder();
        UpdateGizmoColors();
        UpdateTriggerVolume();
    }
    
    
    private void OnValidate()
    {
        GetBuiltinCylinder();
        UpdateGizmoColors();
        UpdateTriggerVolume();
    }
    
    
    private bool ShowGizmos => drawGizmos && !drawOnlyWhileSelected;
    private void OnDrawGizmos()
    {
        if (!ShowGizmos)
            return;

        DrawGizmos();
    }
    
    
    private bool ShowGizmosSelected => drawGizmos && drawOnlyWhileSelected;
    private void OnDrawGizmosSelected()
    {
        if (!ShowGizmosSelected)
            return;
        
        DrawGizmos();
    }
    
    
    /// <summary>
    /// Gets the build-in Unity cylinder.
    /// </summary>
    private void GetBuiltinCylinder()
    {
        if (!_cylinder)
            _cylinder = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
    }
    
    
    /// <summary>
    /// Looks for a trigger volume component and passes it this as a reference.
    /// </summary>
    private void UpdateTriggerVolume()
    {
        if (!_triggerVolume)
            _triggerVolume = GetComponent(typeof(TriggerVolume)) as TriggerVolume;
        
        if (_triggerVolume)
            _triggerVolume.GizmoRenderer = this;
    }
    
    
    /// <summary>
    /// Calculates and sets the gizmo color and gizmo color triggered.
    /// </summary>
    private void UpdateGizmoColors()
    {
        if (gizmoColor == _gizmoColor)
            return;
        
        _gizmoColor = gizmoColor;
        
        float r = _gizmoColor.r > 0.5f ?
            _gizmoColor.r - triggeredColorDifference : _gizmoColor.r + triggeredColorDifference;
        
        float g = _gizmoColor.g > 0.5f ?
            _gizmoColor.g - triggeredColorDifference : _gizmoColor.g + triggeredColorDifference;
        
        float b = _gizmoColor.b > 0.5f ?
            _gizmoColor.b - triggeredColorDifference : _gizmoColor.b + triggeredColorDifference;

        _gizmoColorTriggered = new Color(r, g, b, _gizmoColor.a);
    }
    
    
    /// <summary>
    /// Calls a gizmo drawing method based on the collider type for each collider component.
    /// If there are no collider components draws a default box gizmo if enabled.
    /// </summary>
    private void DrawGizmos()
    {
        //Check for collider components to render and if there is non draws a default box gizmo
        if ((_colliderComponents = GetComponents(typeof(Collider))).Length == 0)
        {
            if (drawDefaultBox)
                DrawDefaultBox();
            
            return;
        }
        
        //Selecting the right draw method for each collider component
        foreach (Component component in _colliderComponents)
        {
            switch (component.GetType().ToString())
            {
                case "UnityEngine.BoxCollider":
                    DrawBox(component);
                    break;
                
                case "UnityEngine.CapsuleCollider":
                    DrawCapsule(component);
                    break;
                
                case "UnityEngine.MeshCollider":
                    DrawCustomMesh(component);
                    break;
                
                case "UnityEngine.SphereCollider":
                    DrawSphere(component);
                    break;
                
                case "UnityEngine.TerrainCollider":
                    Debug.LogWarning(
                        "Gizmo Renderer: " + nameof(TerrainCollider) + " not supported!");
                    break;
                
                case "UnityEngine.WheelCollider":
                    Debug.LogWarning(
                        "Gizmo Renderer: " + nameof(WheelCollider) + " not supported!");
                    break;
                
                default:
                    Debug.LogWarning("Gizmo Renderer: Component not supported!");
                    break;
            }
        }
    }
    
    
    /// <summary>
    /// Draws a box gizmo.
    /// </summary>
    private void DrawDefaultBox()
    {
        //Collider & matrix
        Transform thisTransform = transform;
        Gizmos.matrix = Matrix4x4.identity;
    
        //Drawing
        Gizmos.color = _gizmoColor;
        Gizmos.DrawCube(thisTransform.position, thisTransform.localScale);
    
        //Wireframe
        if (!drawWireframe)
            return;
        
        Gizmos.color =
            new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, wireframeOpacity);
        Gizmos.DrawWireCube(thisTransform.position, thisTransform.localScale);
    }
    
    
    /// <summary>
    /// Draws a box gizmo.
    /// </summary>
    private void DrawBox(Component component)
    {
        //Collider & matrix
        BoxCollider boxCollider = (BoxCollider)component;
        Gizmos.matrix = transform.localToWorldMatrix;

        //Drawing
        Gizmos.color = _gizmoColor;
        Gizmos.DrawCube(boxCollider.center, boxCollider.size);

            //Wireframe
        if (!drawWireframe)
            return;
        
        Gizmos.color =
            new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, wireframeOpacity);
        Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
    }
    
    
    /// <summary>
    /// Draws a capsule gizmo. (Draws a cylinder instead of a capsule because of scaling issues.)
    /// </summary>
    private void DrawCapsule(Component component)
    {
        //Collider & matrix
        CapsuleCollider capsuleCollider = (CapsuleCollider)component;
        Gizmos.matrix = Matrix4x4.identity;
        
        //Transform & rotation
        Transform thisTransform = transform;
        Quaternion rotation = thisTransform.rotation;

        switch (capsuleCollider.direction)
        {
            case 0:
                rotation *= Quaternion.Euler(90f, 90f, 0);
                break;
            case 1:
                break;
            case 2:
                rotation *= Quaternion.Euler(90f, 0, 0);
                break;
        }
        
        //Scale
        Vector3 lossyScale = thisTransform.lossyScale;
        float scaleY;
        float scaleXZ;
        
        switch (capsuleCollider.direction)
        {
            case 0:
                scaleXZ = Mathf.Max(Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z));
                scaleY = Mathf.Abs(lossyScale.x);
                break;
            case 1:
                scaleXZ = Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.z));
                scaleY = Mathf.Abs(lossyScale.y);
                break;
            case 2:
                scaleXZ = Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y));
                scaleY = Mathf.Abs(lossyScale.z);
                break;
            default:
                scaleY = scaleXZ = 0;
                break;
        }
        
        //Height & radius
        float height = capsuleCollider.height * scaleY;
        float radius = capsuleCollider.radius * scaleXZ;
        height = height <= radius * 2 ? radius : height / 2;

        //Drawing
        Gizmos.color = _gizmoColor;
        Gizmos.DrawMesh(_cylinder, capsuleCollider.bounds.center, rotation,
            new Vector3(radius, height, radius));
        
        //Wireframe
        if (!drawWireframe)
            return;
        
        Gizmos.color =
            new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, wireframeOpacity);
        Gizmos.DrawWireMesh(_cylinder, capsuleCollider.bounds.center, rotation,
            new Vector3(radius, height, radius));
    }
    
    
    /// <summary>
    /// Draws a custom mesh gizmo.
    /// </summary>
    private void DrawCustomMesh(Component component)
    {
        //Collider, mesh & matrix
        MeshCollider meshCollider = (MeshCollider)component;
        Mesh customMesh = meshCollider.sharedMesh;
        Gizmos.matrix = Matrix4x4.identity;

        //Transform, rotation & scale
        Transform thisTransform = transform;
        Quaternion rotation = thisTransform.rotation;
        Vector3 localScale = thisTransform.localScale;

        //Drawing
        Gizmos.color = _gizmoColor;
        Gizmos.DrawMesh(customMesh, meshCollider.transform.position,
            rotation, localScale);

            //Wireframe
        if (!drawWireframe)
            return;

        Gizmos.color =
            new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, wireframeOpacity);
        Gizmos.DrawWireMesh(customMesh, meshCollider.transform.position, rotation, localScale);
    }
    
    
    /// <summary>
    /// Draws a sphere gizmo.
    /// </summary>
    private void DrawSphere(Component component)
    {
        //Collider & matrix
        SphereCollider sphereCollider = (SphereCollider)component;
        Gizmos.matrix = Matrix4x4.identity;

        //Scale
        Vector3 lossyScale = transform.lossyScale;
        float scale = Mathf.Max(
            Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z));

        //Drawing
        Gizmos.color = _gizmoColor;
        Gizmos.DrawSphere(sphereCollider.bounds.center, sphereCollider.radius * scale);
        
        //Wireframe
        if (!drawWireframe)
            return;

        Gizmos.color =
            new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, wireframeOpacity);
        Gizmos.DrawWireSphere(sphereCollider.bounds.center, sphereCollider.radius * scale);
    }
}