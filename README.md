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
// Example: Raycast-based 2D motor
[RequireComponent(typeof(BoxCollider2D))]
public class RaycastMotor2D : MonoBehaviour {
    protected struct RaycastOrigin { public Vector2 topLeft, topRight, bottomLeft, bottomRight; }
    public LayerMask collisionMask;
    protected BoxCollider2D colliderObj;
    protected RaycastOrigin raycastOrigin;
    
    public virtual void Start() => colliderObj = GetComponent<BoxCollider2D>();
    protected void UpdateRaycastOrigins() 
    {
       Bla bla bla...
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

That for movable platforms! 
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
        SetLWayPoints();
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
    
    private void SetLWayPoints()
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

