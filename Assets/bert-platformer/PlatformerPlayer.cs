using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;



[RequireComponent(typeof(BoxCollider2D))]
public class PlatformerPlayer : MonoBehaviour
{

    #region Helper Classes


    struct InputState
    {
        public float HorizontalInput;
        public float VerticalInput;

    }
    
    struct MotionState
    {
        public float HorizontalVelocity;
        public float VerticalVelocity;
    }

    struct RaycastOrigins
    {
        public Vector2 BottomLeft;
        public Vector2 BottomRight;
        public Vector2 TopLeft;
        public Vector2 TopRight;
    }

    #endregion
    // Start is called before the first frame update
    private InputState _playerInputState = new InputState();
    private MotionState _playerMotionState = new MotionState();
    private RaycastOrigins _raycastOrigins = new RaycastOrigins();

    
    
    public BoxCollider2D boxCollider;

    [Header("Player Raycast Configurations")] 
    public float skinWidth = 0.15f;
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;
    public float horizontalRaySpacing;
    public float verticalRaySpacing;


    [Header("Velocity Configurations")] 
    public float gravity = 15f;
    public float horizontalAcceleration;
    public float horizontalDeceleration;
    public float horizontalMaxRunningVelocity;
    
    
    void Start()
    {
        CalculateRaySpacing();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRaycastOrigins();
        ShowGizmos();
        SetInputState();
        ApplyGravity();
        VerticalCollisions(Time.deltaTime);
        SetHorizontalMovementState(Time.deltaTime);
        ApplyMovementState(Time.deltaTime);
    }

    
    
    void ShowGizmos() {

        for (int i = 0; i < verticalRayCount; i++) {
        }
    }

    void VerticalCollisions(float deltaTime)
    {
        float directionY = Mathf.Sign(_playerMotionState.VerticalVelocity);
        float rayLength = Mathf.Abs(_playerMotionState.VerticalVelocity) + skinWidth;
        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = directionY == -1 ? _raycastOrigins.BottomLeft : _raycastOrigins.TopLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + _playerMotionState.HorizontalVelocity * deltaTime);

            Debug.DrawRay(rayOrigin , rayLength * Vector2.up * directionY, Color.red);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength,
                1 << LayerMask.NameToLayer("Terrain"));
            if (hit)
            {
                _playerMotionState.VerticalVelocity = (hit.distance - skinWidth) * directionY ;
                rayLength = hit.distance;
            }
        }
    }

    void ApplyGravity()
    {
        _playerMotionState.VerticalVelocity -= gravity * Time.deltaTime;
    }

    
    void UpdateRaycastOrigins()
    {
        Bounds bounds = boxCollider.bounds;
        bounds.Expand(skinWidth * -2f);

        _raycastOrigins.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        _raycastOrigins.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
        _raycastOrigins.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
        _raycastOrigins.TopRight = new Vector2(bounds.max.x, bounds.max.y);
    }
    
    

    void CalculateRaySpacing()
    {
        Bounds bounds = boxCollider.bounds;
        bounds.Expand(skinWidth * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount,2,int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount,2,int.MaxValue);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    private void FixedUpdate()
    {
    }

    private void SetInputState()
    {
        _playerInputState.HorizontalInput = Input.GetAxisRaw("Horizontal");
        _playerInputState.VerticalInput = Input.GetAxisRaw("Vertical");
    }

    private void SetHorizontalMovementState(float deltaTime)
    {
        float currentVelocity = 0;
        if (Mathf.Abs(_playerInputState.HorizontalInput) > 0.1f)
        {
            var currentHorizontalAcceleration = _playerInputState.HorizontalInput * horizontalAcceleration;
            currentVelocity = _playerMotionState.HorizontalVelocity + currentHorizontalAcceleration * deltaTime;
        }
        else
        {
            currentVelocity = Mathf.MoveTowards(_playerMotionState.HorizontalVelocity, 0,
                horizontalDeceleration * deltaTime);
        }
        
        _playerMotionState.HorizontalVelocity =
            Mathf.Clamp(currentVelocity, -horizontalMaxRunningVelocity, horizontalMaxRunningVelocity);
    }

    private void ApplyMovementState(float deltaTime)
    {
        var position = transform.position;
        position.x += _playerMotionState.HorizontalVelocity * deltaTime;
        position.y += _playerMotionState.VerticalVelocity * deltaTime;
        transform.position = position;

    }

}
