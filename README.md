# Platform2D with Unity
Platform game template made with custom physics behaviours in Unity2D.

[Watch the demo here](https://youtu.be/XULkGwHlF5I)


# Platform2D Engine for Unity

A 2D platforming engine built in Unity3D, using **raycasting** for collision detection and movement instead of Unity's default physics system. Based on an official Unity tutorial, but extended with **pixelated camera effects, shaders**, and a **ScriptableObject-based AI state machine**, similar to my 2.5D FPS engine.

## Features

### Raycast-Based Physics
- **Custom 2D motor:** Physics handled via `RaycastMotor2D`, allowing precise control over movement and collisions.
- **Slope & wall detection:** Handles slopes, collisions, and wall jumps accurately without relying on Rigidbody physics.
- **Smooth motion:** Supports acceleration, deceleration, and responsive jumping.

### Enemy AI
- **Simple FSM:** Enemies like `GroundEnemy` use a straightforward state machine without interface segregation.
- **Patrol, Chase, Attack behaviors:** Determined by raycasts and linecasts for ground, wall, and player detection.
- **Flexible extension:** While currently non-modular, the architecture allows experimentation and adaptation for more complex FSM systems.

### Component Highlights
```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles raycast-based physics.
/// </summary>
public class RaycastMotor2D
{
    public const float SKIN_WIDTH = 0.015f;
    public const float DISTANCE_BETWEEN_RAYS = 0.1f;
    protected BoxCollider2D collider;
    protected Bounds bounds;
    protected RaycastOrigin raycastOrigin;
    
    public int HorizontalRayCount { get; private set; }
    public int VerticalRayCount { get; private set; }
    public float HorizontalRaySpacing { get; private set; }
    public float VerticalRaySpacing { get; private set; }
    public Bounds GetBounds() => bounds;
    public Vector2 GetBottomLeft() => raycastOrigin.bottomLeft;
    public Vector2 GetBottomRight() => raycastOrigin.bottomRight;
    public Vector2 GetTopLeft() => raycastOrigin.topLeft;
    public Vector2 GetTopRight() => raycastOrigin.topRight;
    
    protected struct RaycastOrigin
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    
    public RaycastMotor2D(BoxCollider2D collider)
    {
        this.collider = collider;
        CalculateRaySpacing();
        UpdateRaycastOrigins();
    }

    public void UpdateRaycastOrigins()
    {
        bounds = collider.bounds;
        bounds.Expand(SKIN_WIDTH * -2);

        raycastOrigin.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigin.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigin.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigin.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    void CalculateRaySpacing()
    {
        bounds = collider.bounds;
        bounds.Expand(SKIN_WIDTH * -2);

        HorizontalRayCount = Mathf.RoundToInt(bounds.size.y / DISTANCE_BETWEEN_RAYS);
        VerticalRayCount = Mathf.RoundToInt(bounds.size.x / DISTANCE_BETWEEN_RAYS);

        HorizontalRaySpacing = bounds.size.y / (HorizontalRayCount - 1);
        VerticalRaySpacing = bounds.size.x / (VerticalRayCount - 1);
    }
}
```

That is either used for the player... 

```csharp
    // ... bla bla bla
    // Components
    private PlatformMotor2D playerMotor;
    private PlayerInput playerInput;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerMotor = GetComponent<PlatformMotor2D>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        SetGravity();
    }
    
    private void Update()
    {
        HandleMovement();
        HandleWallSliding();
        HandleJumping();

        playerMotor.Move(velocity * Time.deltaTime, playerInput.MoveInput.y);

        HandleCollisions();
        HandleAnimations();

        // consume one-shot inputs so they don’t get reused this frame
        playerInput.ConsumeJumpInputs();
        
    }

    private void SetGravity()
    {
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity * timeToJumpApex);
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    }

    bla bla bla...
``` 

Or for movable platforms! 
```csharp 
    // ... bla bla bla
    private List<PassengerState> passengers;
    private Dictionary<Transform, PlatformMotor2D> dictionaryPassengers;
    private HashSet<Transform> movedPassengers;
    private Vector3[] globalWayPointsPosition;
    private int fromWayPointIndex;
    private float percentBetweenWaypoints;
    private float nextMoveTime;

    
    public override void Start()
    {
        base.Start();
        SetWayPoints();
    }
    
    void Update()
    {
        UpdateRaycastOrigins();
        Vector3 velocity = CalculatePlatformMovement();
        CalculatePassengerMovement(velocity);
        DisplaceWithPassengers(velocity);
    }
    
    private void SetWayPoints()
    {
        dictionaryPassengers = new Dictionary<Transform, PlatformMotor2D>();
        globalWayPointsPosition = new Vector3[localWaypoints.Length];
        passengers = new List<PassengerState>();
        movedPassengers = new HashSet<Transform>();

        for (int i = 0; i < localWaypoints.Length; i++)
        {
            globalWayPointsPosition[i] = localWaypoints[i] + transform.position;
        }
    }
    
    private float Ease(float x)
    {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    private Vector3 CalculatePlatformMovement()
    {
        if (Time.time < nextMoveTime)
        {
            return Vector3.zero;
        }

        fromWayPointIndex = fromWayPointIndex % globalWayPointsPosition.Length;
        int toWayPointIndex = (fromWayPointIndex + 1) % globalWayPointsPosition.Length;

        float distanceBetweenWayPoints = Vector3.Distance(globalWayPointsPosition[fromWayPointIndex], globalWayPointsPosition[toWayPointIndex]);
        percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWayPoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);

        float easedPercentBetweenWayPoints = Ease(percentBetweenWaypoints);
        Vector3 newPos = Vector3.Lerp(globalWayPointsPosition[fromWayPointIndex], globalWayPointsPosition[toWayPointIndex], easedPercentBetweenWayPoints);

        if (percentBetweenWaypoints >= 1)
        {
            percentBetweenWaypoints = 0;
            fromWayPointIndex++;

            if (!isCyclic && fromWayPointIndex >= globalWayPointsPosition.Length - 1)
            {
                fromWayPointIndex = 0;
                System.Array.Reverse(globalWayPointsPosition);
            }

            nextMoveTime = Time.time + waitTime;
        }

        return newPos - transform.position;
    }
    bla bla bla...
```

### Future Improvements

- More parkour based features like high speed circular speeds.
- Add more pluggable AI states for our dumb AI like `Flee`, `Patrol`, `Search`, etc.
- Make enemies actually die
- Perhaps implement a flashier combat system? it's super basic now. 
- Implement a Single entry point architecture to inject all references and avoid a couple sins I got here and there. 

## Unity version
Updated to `2021.3.16f1`

## For more info
[Link to the official docs here!](https://youtu.be/wGI2e3Dzk_w?list=PLX2vGYjWbI0SUWwVPCERK88Qw8hpjEGd8)

