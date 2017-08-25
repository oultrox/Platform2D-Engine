using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Controller2D))]
public class PlayerMove : MonoBehaviour {

    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;

    public float timeToJumpApex = 0.4f;
    public float moveSpeed = 6;
    public float accelerationTimeAirborne = 0.2f;
    public float accelerationTimeGrounded = 0.1f;
    public float wallsSlideSpeedMax = 3;
    public float wallStickTime = 0.25f;
    private float timeWallUnstick;

    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;



    private float maxJumpVelocity;
    private float minJumpVelocity;
    private float gravity = -20;
    private Vector3 velocity;
    private Controller2D controller;
    private float targetVelocityX;
    private float velocityXSmoothing;
    private float h;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
	// Use this for initialization
	void Start () {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        animator = this.GetComponent<Animator>();
        controller = this.GetComponent<Controller2D>();
        gravity = - (2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity * timeToJumpApex);
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
	}

    private void Update()
    {
        h = Input.GetAxisRaw("Horizontal");
        int wallDirX = (controller.collisions.left) ? -1 : 1;
        
        targetVelocityX = h * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeAirborne : accelerationTimeGrounded);

        // WALL JUMP
        bool wallSliding = false;
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)
        {
            wallSliding = true;
            if (velocity.y < -wallsSlideSpeedMax)
            {
                velocity.y = -wallsSlideSpeedMax;
            }

            if (timeWallUnstick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;
                if (h != wallDirX && h !=0)
                {
                    timeWallUnstick -= Time.deltaTime;
                }
                else
                {
                    timeWallUnstick = wallStickTime;
                }
            }
            else
            {
                timeWallUnstick = wallStickTime;
            }
        }


        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (wallSliding)
            {
                if (wallDirX == h)
                {
                    velocity.x = -wallDirX * wallJumpClimb.x;
                    velocity.y = wallJumpClimb.y;
                }
                else if(h == 0)
                {
                    velocity.x = -wallDirX * wallJumpOff.x;
                    velocity.y = wallJumpOff.y;
                }
                else
                {
                    velocity.x = -wallDirX * wallLeap.x;
                    velocity.y = wallLeap.y;
                }
            }
            if(controller.collisions.below)
            {
                velocity.y = maxJumpVelocity;
            }
            
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (velocity.y > minJumpVelocity)
            {
                velocity.y = minJumpVelocity;
            }
        }

        
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.fixedDeltaTime);

        if (velocity.x !=0)
        {
            spriteRenderer.flipX = velocity.x < -0.01f;
        }

        animator.SetBool("grounded", controller.collisions.below);
        animator.SetFloat("velocityX", Mathf.Abs(targetVelocityX));
    }


}
