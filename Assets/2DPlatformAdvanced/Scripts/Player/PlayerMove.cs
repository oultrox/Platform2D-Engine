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
    private float v;
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
        v = Input.GetAxisRaw("Vertical");
        int wallDirX = (controller.collisionState.left) ? -1 : 1;

        //Transición suave de la velocidad del jugador.
        targetVelocityX = h * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisionState.below) ? accelerationTimeAirborne : accelerationTimeGrounded);

        if (controller.collisionState.isWallJumpable)
        {
            // WALL SLIDING  - comentada la última condición para hacer más fluido el wall jumping continuo.
            if (((controller.collisionState.left && velocity.x < 0) || (controller.collisionState.right && velocity.x > 0)) && !controller.collisionState.slidingDownMaxSlope/*&&!controller.collisionInfo.below && velocity.y < 0*/)
            {
                //Para evitar pegarse estando en el suelo. puede ser retirable en caso de bugs.
                if (!controller.collisionState.below)
                {
                    isStickedToWall = true;
                }
                
            }

        }

        if (isStickedToWall)
        {
            if (!controller.collisionState.left && !controller.collisionState.right)
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
            if (controller.collisionState.below)
            {
                if (controller.collisionState.slidingDownMaxSlope)
                {
                    if (h != -Mathf.Sign(controller.collisionState.slopeNormal.x)) //not jumping against slope
                    {
                        velocity.y = maxJumpVelocity * controller.collisionState.slopeNormal.y;
                        velocity.x = maxJumpVelocity * controller.collisionState.slopeNormal.x;
                    }
                }
                else
                {
                    velocity.y = maxJumpVelocity;
                }
                
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
        controller.Move(velocity * Time.fixedDeltaTime,v);

        //Deten el movimiento si está en el suelo o tocando algo arriba.
        if (controller.collisionState.above || controller.collisionState.below)
        {
            if (controller.collisionState.slidingDownMaxSlope)
            {
                velocity.y += controller.collisionState.slopeNormal.y * -gravity * Time.deltaTime;
            }
            else
            {
                velocity.y = 0;
            }
            
        }

        //Animations
        if (velocity.x != 0)
        {
            spriteRenderer.flipX = velocity.x < 0;
        }

        animator.SetBool("grounded", controller.collisionState.below);
        animator.SetFloat("velocityX", Mathf.Abs(targetVelocityX));
    }


}
