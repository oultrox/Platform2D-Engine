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
using UnityEngine;

/// <summary>
/// Physics motor based on raycasts. Used to extend either our player controllers or movable objects in the game.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class RaycastMotor2D : MonoBehaviour 
{
    /// <summary>
    /// Helps define the edges of the raycast origin based on the collider.
    /// </summary>
    protected struct RaycastOrigin
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
    
    public LayerMask collisionMask;
    protected const float SKIN_WIDTH = 0.015f;
    protected const float DISTANCE_BETWEEN_RAYS = 0.1f;
    protected int horizontalRayCount;
    protected int verticalRayCount;
    protected float horizontalRaySpacing;
    protected float verticalRaySpacing;
    protected BoxCollider2D colliderObj;
    protected RaycastOrigin raycastOrigin;
    private Bounds _bounds;

    public virtual void Start()
    {
        colliderObj = this.GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    /// <summary>
    /// Updates the positions of the raycast origins based on the collider size.
    /// </summary>
    protected void UpdateRaycastOrigins()
    {
        _bounds = colliderObj.bounds;
        _bounds.Expand(SKIN_WIDTH * -2); // Shrinks bounds slightly to inset raycasts and prevent bugs.

        raycastOrigin.bottomLeft.x = _bounds.min.x;
        raycastOrigin.bottomLeft.y = _bounds.min.y;

        raycastOrigin.bottomRight.x = _bounds.max.x;
        raycastOrigin.bottomRight.y = _bounds.min.y;

        raycastOrigin.topLeft.x = _bounds.min.x;
        raycastOrigin.topLeft.y = _bounds.max.y;

        raycastOrigin.topRight.x = _bounds.max.x;
        raycastOrigin.topRight.y = _bounds.max.y;
    }

    /// <summary>
    /// Calculates the spacing between raycast lines. Done only once.
    /// </summary>
    private void CalculateRaySpacing()
    {
        _bounds = colliderObj.bounds;
        _bounds.Expand(SKIN_WIDTH * -2); 

        // Get the width and height of the collider.
        float boundsWidth = _bounds.size.x;
        float boundsHeight = _bounds.size.y;

        // Set the number of rays to cast based on the spacing along the opposite side.
        horizontalRayCount = Mathf.RoundToInt(boundsHeight / DISTANCE_BETWEEN_RAYS);
        verticalRayCount = Mathf.RoundToInt(boundsWidth / DISTANCE_BETWEEN_RAYS);

        horizontalRaySpacing = _bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = _bounds.size.x / (verticalRayCount - 1);
    }
}
```

That is either used for the player... 

```csharp
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
    private List<PassengerState> passengers;
    private Dictionary<Transform, PlatformMotor2D> dictionaryPassengers;
    private HashSet<Transform> movedPassengers;
    private Vector3[] globalWayPointsPosition;
    private int fromWayPointIndex;
    private float percentBetweenWaypoints;
    private float nextMoveTime;

    public override void Start()
    {
        SetWayPoints();
    }
    
    void Update()
    {
        UpdateRaycastOrigins();
        Vector3 velocity = CalculatePlatformMovement();
        CalculatePassengerMovement(velocity);
        MovePassenger(true);
        transform.Translate(velocity);
        MovePassenger(false);
    }
    
    private void SetWayPoints()
    {
        base.Start();
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

