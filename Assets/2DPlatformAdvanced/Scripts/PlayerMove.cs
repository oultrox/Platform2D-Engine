using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class PlayerMove : MonoBehaviour
{

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
    private float gravity; //Antes era -20, ahora es seteado en base a la altura del salto.

    private Vector3 velocity;
    private Controller2D controller;
    private float targetVelocityX;
    private float velocityXSmoothing;
    private float h;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isStickedToWall = false;

    //-------Metodos API-------
    // Use this for initialization
    void Start()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        animator = this.GetComponent<Animator>();
        controller = this.GetComponent<Controller2D>();
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity * timeToJumpApex);
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    }

    private void Update()
    {
        h = Input.GetAxisRaw("Horizontal");
        int wallDirX = (controller.collisionInfo.left) ? -1 : 1;

        //Transición suave de la velocidad del jugador.
        targetVelocityX = h * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisionInfo.below) ? accelerationTimeAirborne : accelerationTimeGrounded);

        // WALL SLIDING  - comentada la última condición para hacer más fluido el wall jumping continuo.
        if (((controller.collisionInfo.left && velocity.x < 0) || (controller.collisionInfo.right && velocity.x > 0)) && !controller.collisionInfo.below /*&& velocity.y < 0*/) 
        {
            isStickedToWall = true;
        }

        if (isStickedToWall)
        {
            if (!controller.collisionInfo.left && !controller.collisionInfo.right)
            {
                isStickedToWall = false;
            }
            if (velocity.y < -wallsSlideSpeedMax)
            {
                velocity.y = -wallsSlideSpeedMax;
            }

            if (timeWallUnstick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;
                if (h != wallDirX || h == 0)
                {
                    timeWallUnstick -= Time.deltaTime;
                    if (timeWallUnstick <= 0)
                    {
                        isStickedToWall = false;
                    }
                }
                else if (h == wallDirX)
                {
                    timeWallUnstick = wallStickTime;
                }

            }
            else
            {
                timeWallUnstick = wallStickTime;
            }
        }

        if (controller.collisionInfo.above || controller.collisionInfo.below)
        {
            velocity.y = 0;
        }

        //JUMP
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Wall jump
            if (isStickedToWall)
            {
                if (wallDirX == h)
                {
                    velocity.x = -wallDirX * wallJumpClimb.x;
                    velocity.y = wallJumpClimb.y;
                }
                else if (h == 0)
                {
                    velocity.x = -wallDirX * wallJumpOff.x;
                    velocity.y = wallJumpOff.y;
                }
                else
                {
                    velocity.x = -wallDirX * wallLeap.x;
                    velocity.y = wallLeap.y;
                }
                isStickedToWall = false;
            }
            //Normal Jump
            if (controller.collisionInfo.below)
            {
                velocity.y = maxJumpVelocity;
            }

        }

        //Al soltar el espacio, si la velocidad Y es mayor al a minima, setearla, esto es para el jump sustain.
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (velocity.y > minJumpVelocity)
            {
                velocity.y = minJumpVelocity;
            }
        }

        //Gravedad
        velocity.y += gravity * Time.deltaTime;
        //Movmiento
        controller.Move(velocity * Time.fixedDeltaTime);

        //Animations
        if (velocity.x != 0)
        {
            spriteRenderer.flipX = velocity.x < 0;
        }

        animator.SetBool("grounded", controller.collisionInfo.below);
        animator.SetFloat("velocityX", Mathf.Abs(targetVelocityX));
    }


}
