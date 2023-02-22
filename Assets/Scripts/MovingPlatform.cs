using System;
using UnityEngine;


[DisallowMultipleComponent]
public class MovingPlatform : MonoBehaviour
{
    private Vector3 _startPos;
    private Vector3 _amountToAdd;
    
    
    [Space(8), Header("Settings")]
    
    [SerializeField, Tooltip("Tooltip")]
    [Min(0)]
    private float speed = 1;
    
    [SerializeField, Tooltip("Tooltip")]
    private MovementDirection movementDirection = MovementDirection.Right;
    
    
    private enum MovementDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    [SerializeField, Tooltip("Distance to move in Unity Units")]
    [Min(0)]
    private float distance = 3;
    
    
    private void Awake()
    {
        _startPos = transform.position;
    }
    
    private void Update()
    {
        Vector3 targetPos;
        Transform thisTransform = transform;
        
        switch (movementDirection)
        {
            case MovementDirection.Up:
                _amountToAdd = Vector3.up;
                targetPos = new Vector3(_startPos.x, _startPos.y + distance);
                if (transform.position.y >= targetPos.y)
                {
                    transform.position = targetPos;
                    _startPos = thisTransform.position;
                    movementDirection = MovementDirection.Down;
                }
                break;
            case MovementDirection.Down:
                _amountToAdd = Vector3.down;
                targetPos = new Vector3(_startPos.x, _startPos.y - distance);
                if (transform.position.y <= targetPos.y)
                {
                    transform.position = targetPos;
                    _startPos = thisTransform.position;
                    movementDirection = MovementDirection.Up;
                }
                break;
            case MovementDirection.Left:
                _amountToAdd = Vector3.left;
                targetPos = new Vector3(_startPos.x - distance, _startPos.y);
                if (transform.position.x <= targetPos.x)
                {
                    transform.position = targetPos;
                    _startPos = thisTransform.position;
                    movementDirection = MovementDirection.Right;
                }
                break;
            case MovementDirection.Right:
                _amountToAdd = Vector3.right;
                targetPos = new Vector3(_startPos.x + distance, _startPos.y);
                if (transform.position.x >= targetPos.x)
                {
                    transform.position = targetPos;
                    _startPos = thisTransform.position;
                    movementDirection = MovementDirection.Left;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        transform.position += _amountToAdd * (Time.deltaTime * speed);
    }
}