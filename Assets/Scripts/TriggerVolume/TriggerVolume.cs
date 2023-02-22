/*
Player mutable : if player is on trigger, won't get triggered by other players (NOT PLAYER PER-SEE)
Player mute count : the amount of players before being muted
on teleport player out @big
private readonly List<Collider> _allColliders = new();
 */


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// Custom Unity event with a single collider as pass-through value.
/// </summary>
[Serializable] public class UnityEventCollider : UnityEvent<Collider> { }


[DisallowMultipleComponent]
public class TriggerVolume : MonoBehaviour
{
    public GizmoRenderer GizmoRenderer
    {
        set => _gizmoRenderer = _gizmoRenderer ? _gizmoRenderer : value;
    }
    
    private List<Collider> _otherColliders;
    
    private Component[] _colliderComponents;
    private GizmoRenderer _gizmoRenderer;
    private int _triggerAmount;
    private int _otherCount;
    
    private bool _hasEnterEvent;
    private bool _hasExitEvent;
    private bool _hasStayEvent;
    
    private bool _hasEnteredOnce;
    private bool _hasExitedOnce;
    
    private Coroutine _lastIntervalCoroutine;
    private bool _firstIntervalCoroutine = true;
    private bool _isInIntervalCoroutine;
    
    
    //Settings (for how the collider components will act)
    [Header("Settings")]
    
    [SerializeField, Tooltip("Tooltip")]
    private bool triggerOnce;

    [SerializeField, Tooltip("Tooltip")]
    [Min(0)] private float triggerOnceResetTime;

    [SerializeField, Tooltip("Tooltip")]
    private bool mergeTriggerDetection = true;
    
    
    //Component Settings
    [Header("Collider Component Settings")]
    
    [SerializeField, Tooltip("Tooltip")]
    private Override overrideConvex;
    
    [SerializeField, Tooltip("Tooltip")]
    private Override overrideIsTrigger;
    
    private enum Override
    {
        NoOverride,
        EnableAll,
        DisableAll
    }
    
    
    //Filter (for filtering what can trigger the collider components)
    [Space(8), Header("Filter")]
    
    [SerializeField, Tooltip("Tooltip")]
    private GameObject specificTriggerObject;

    [SerializeField, Tooltip("Tooltip")]
    private LayerMask layersToDetect = -1;

    [SerializeField, Tooltip("Tooltip")]
    private string specificTag;
    
    
    //On Trigger Enter
    [Header("On Trigger Enter")]
    
    [SerializeField, Tooltip("Tooltip")]
    [Min(0)] private float enterTriggerDelay;
    
    [Space(8)] public UnityEventCollider onTriggerEnter;
    
    
    //On Trigger Stay
    [Header("On Trigger Stay")]
    
    [SerializeField, Tooltip("Tooltip")]
    [Min(0)] private float stayTriggerDelay;
    
    [SerializeField, Tooltip("Tooltip")]
    [Min(0)] private float stayTriggerInterval;
    
    [Space(8)] public UnityEventCollider onTriggerStay;
    
    
    //On Trigger Exit
    [Header("On Trigger Exit")]
    
    [SerializeField, Tooltip("Tooltip")]
    [Min(0)] private float exitTriggerDelay;
    
    [Space(8)] public UnityEventCollider onTriggerExit;


    private void Awake()
    {
        //Checking for collider components
        _colliderComponents = GetComponents(typeof(Collider));
        if (GetComponents(typeof(Collider)).Length == 0)
            return;
        
        CheckForEventsBound();
        
        //Override Convex
        switch (overrideConvex)
        {
            case Override.NoOverride:
                break;
            case Override.EnableAll:
                OverrideConvex(true);
                break;
            case Override.DisableAll:
                OverrideConvex();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        //Override IsTrigger
        switch (overrideIsTrigger)
        {
            case Override.NoOverride:
                break;
            case Override.EnableAll:
                OverrideIsTrigger(true);
                break;
            case Override.DisableAll:
                OverrideIsTrigger();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    
    private void Reset()
    {
        LookForGizmoRenderer();
        CheckForEventsBound();
    }
    
    
    private void OnValidate()
    {
        CheckForEventsBound();
    }
    
    
    private void FixedUpdate()
    {
        _triggerAmount = _otherCount = 0;
    }
    
    
    private void OnTriggerEnter(Collider other)
    {
        if (Filter(other))
            return;
        
        _triggerAmount += 1;
        if (mergeTriggerDetection && _triggerAmount > 1)
            return;
        
        if (triggerOnce && _hasEnteredOnce)
            return;
        
        _hasEnteredOnce = true;
        
        if (_gizmoRenderer)
            _gizmoRenderer.IsTriggered = true;
        
        if (_hasEnterEvent == false) return;
        
        //Start coroutine for enter event
        StartCoroutine(EventDelay(enterTriggerDelay, onTriggerEnter, other));
    }
    
    
    private void OnTriggerExit(Collider other)
    {
        if (Filter(other))
            return;
        
        _triggerAmount -= 1;
        if (mergeTriggerDetection && _triggerAmount > 0)
            return;
        
        if (triggerOnce && _hasExitedOnce)
        {
            StopLastIntervalCoroutine();
            return;
        }
        
        _hasExitedOnce = true;
        
        if (triggerOnce && triggerOnceResetTime > 0)
        {
            Invoke(nameof(ResetTriggerOnce), triggerOnceResetTime);
            
            if (_gizmoRenderer)
                _gizmoRenderer.IsTriggered = false;
        }
        else if (_gizmoRenderer)
            _gizmoRenderer.IsTriggered = !(_triggerAmount == 0 || triggerOnce);
        
        StopLastIntervalCoroutine();
        
        if (!_hasExitEvent)
            return;
        
        //Start coroutine for exit event
        StartCoroutine(EventDelay(exitTriggerDelay, onTriggerExit, other));
    }
    
    
    private void OnTriggerStay(Collider other)
    {
        _otherCount++;
        
        if (!_hasStayEvent)
            return;
        
        if (_triggerAmount == 0)
            return;
        
        if (triggerOnce && _hasExitedOnce)
            return;
        
        if (_isInIntervalCoroutine)
            return;
        
        //Start coroutine for stay event
        _lastIntervalCoroutine = StartCoroutine(Interval(other));
    }
    
    
    public void ResetTriggerOnce()
    {
        if (!triggerOnce)
            return;
        
        _hasExitedOnce = false;
        
        if (_triggerAmount > 0)
        {
            _hasEnteredOnce = true;
            
            if (_gizmoRenderer)
                _gizmoRenderer.IsTriggered = true;

            //Start coroutine for enter event
            StartCoroutine(EventDelay(enterTriggerDelay, onTriggerEnter, null));
        }
        else
            _hasEnteredOnce = false;
    }
    
    
    /// <summary>
    /// Overrides the Convex bool on all mesh collider components.
    /// </summary>
    private void OverrideConvex(bool value = false)
    {
        foreach (Component component in _colliderComponents)
        {
            if (component.GetType() != typeof(MeshCollider))
                continue;

            MeshCollider colliderComponent = (MeshCollider)component;
            colliderComponent.convex = value;
        }
    }
    
    
    /// <summary>
    /// Overrides the isTrigger bool on all collider components.
    /// </summary>
    private void OverrideIsTrigger(bool value = false)
    {
        foreach (Component component in _colliderComponents)
            if (component.GetType() == typeof(MeshCollider))
            {
                MeshCollider colliderComponent = (MeshCollider)component;
                if (colliderComponent.convex == false)
                    continue;

                colliderComponent.isTrigger = value;
            }
            else
            {
                Collider colliderComponent = (Collider)component;
                colliderComponent.isTrigger = value;
            }
    }
    
    
    private void LookForGizmoRenderer()
    {
        if (!_gizmoRenderer)
            _gizmoRenderer = GetComponent(typeof(GizmoRenderer)) as GizmoRenderer;
    }
    
    
    private static IEnumerator EventDelay(float delay, UnityEventCollider unityEventCollider, Collider other)
    {
        yield return new WaitForSeconds(delay);
        unityEventCollider.Invoke(other);
    }
    
    
    private IEnumerator Interval(Collider other)
    {
        _isInIntervalCoroutine = true;

        yield return new WaitForSeconds(
            _firstIntervalCoroutine ? stayTriggerDelay : stayTriggerInterval);

        _isInIntervalCoroutine = false;
        _firstIntervalCoroutine = false;

        onTriggerStay.Invoke(other);
    }
    
    
    private void StopLastIntervalCoroutine()
    {
        if (_lastIntervalCoroutine == null)
            return;

        StopCoroutine(_lastIntervalCoroutine);
        _isInIntervalCoroutine = false;
        _firstIntervalCoroutine = true;
    }
    
    
    private bool Filter(Component other)
    {
        if (specificTriggerObject != null && other.gameObject != specificTriggerObject)
            return true;
        
        if (layersToDetect != (layersToDetect | (1 << other.gameObject.layer)))
            return true;
        
        if (specificTag == string.Empty)
            return false;
        
        // ReSharper disable once Unity.ExplicitTagComparison
        return other.tag != specificTag;
    }
    
    
    private void CheckForEventsBound()
    {
        if (onTriggerEnter != null)
            _hasEnterEvent = onTriggerEnter != null || onTriggerEnter.GetPersistentEventCount() > 0;
        
        if (onTriggerExit != null)
            _hasExitEvent = onTriggerExit != null || onTriggerExit.GetPersistentEventCount() > 0;
        
        if (onTriggerStay != null)
            _hasStayEvent = onTriggerStay != null || onTriggerStay.GetPersistentEventCount() > 0;
    }
}