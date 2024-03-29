﻿using UnityEngine;

[RequireComponent(typeof(PlatformMotor2D))]
public class PlayerMovementController : MonoBehaviour
{
    //Variables editables vía inspector
    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;

    public float timeToJumpApex = 0.4f;
    public float moveSpeed = 6;
    public float accelerationOnGround = 0.03f;
    public float accelerationOnAir = 0.1f;
    public float wallsSlideSpeedMax = 3;
    public float wallStickTime = 0.25f;

    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;

    private int wallDirX;
    private float timeWallUnstick;
    private float maxJumpVelocity;
    private float minJumpVelocity;
    private float gravity;

    private float targetVelocityX;
    private float velocityXSmoothing;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 velocity;
    private PlatformMotor2D playerMotor;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    //-------Metodos API-------
    //Initialization
    private void Awake()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        animator = this.GetComponent<Animator>();
        playerMotor = this.GetComponent<PlatformMotor2D>();
    }

    void Start()
    {
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity * timeToJumpApex);
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    }

    private void Update()
    {
        CalculateVelocity();
        HandleWallSliding();
        playerMotor.Move(velocity * Time.deltaTime, verticalInput);

        //Deten el movimiento si está en el suelo o tocando algo arriba.
        if (playerMotor.collisionInfo.above || playerMotor.collisionInfo.below)
        {
            if (playerMotor.collisionInfo.isSlidingDownMaxSlope)
            {
                velocity.y += playerMotor.collisionInfo.slopeNormal.y * -gravity * Time.deltaTime;
            }
            else
            {
                velocity.y = 0;
            }
        }

        //Animation stuff
        if (velocity.x != 0)
        {
            spriteRenderer.flipX = velocity.x < 0;
        }

        animator.SetBool("grounded", playerMotor.collisionInfo.below);
        animator.SetFloat("velocityX", Mathf.Abs(targetVelocityX));
    }

    //---------Metodos custom--------
    public void SetDirectionalInput(float inputX, float inputY)
    {
        horizontalInput = inputX;
        verticalInput = inputY;
    }

    private void CalculateVelocity()
    {
        //Transición suave de la velocidad del jugador.
        targetVelocityX = horizontalInput * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (playerMotor.collisionInfo.below) ? accelerationOnGround : accelerationOnAir);
        //Gravedad
        velocity.y += gravity * Time.deltaTime;
    }

    private void HandleWallSliding()
    {
        wallDirX = (playerMotor.collisionInfo.left) ? -1 : 1;

        if (playerMotor.collisionInfo.isAbleToWallJump)
        {
            // WALL SLIDING  - comentada la última condición para hacer más fluido el wall jumping continuo.
            if (((playerMotor.collisionInfo.left && velocity.x < 0) || (playerMotor.collisionInfo.right && velocity.x > 0)) 
            && !playerMotor.collisionInfo.isSlidingDownMaxSlope && !playerMotor.collisionInfo.below /*&&!controller.collisionInfo.below && velocity.y < 0*/)
            {
                 playerMotor.collisionInfo.isStickedToWall = true;
            }
        }

        if (!playerMotor.collisionInfo.isStickedToWall)
        {
            return;
        }

        //Deslizar suavemente si está pegado a la pared.
        if (!playerMotor.collisionInfo.left && !playerMotor.collisionInfo.right)
        {
            playerMotor.collisionInfo.isStickedToWall = false;
        }
        if (velocity.y < -wallsSlideSpeedMax)
        {
            velocity.y = -wallsSlideSpeedMax;
        }

        //Si su input es distinto a la direccion donde está pegado, soltar despues de determinado tiempo.
        if (timeWallUnstick > 0)
        {
            velocityXSmoothing = 0;
            velocity.x = 0;
            if (horizontalInput != wallDirX || horizontalInput == 0)
            {
                timeWallUnstick -= Time.deltaTime;
                if (timeWallUnstick <= 0)
                {
                    playerMotor.collisionInfo.isStickedToWall = false;
                }
            }
            else if (horizontalInput == wallDirX)
            {
                timeWallUnstick = wallStickTime;
            }
        }
        else
        {
            timeWallUnstick = wallStickTime;
        }
    }

    public void JumpInputDown()
    {
        //Wall jump
        if (playerMotor.collisionInfo.isStickedToWall)
        {
            if (wallDirX == horizontalInput)
            {
                velocity.x = -wallDirX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            else if (horizontalInput == 0)
            {
                velocity.x = -wallDirX * wallJumpOff.x;
                velocity.y = wallJumpOff.y;
            }
            else
            {
                velocity.x = -wallDirX * wallLeap.x;
                velocity.y = wallLeap.y;
            }
            playerMotor.collisionInfo.isStickedToWall = false;
        }

        //Normal Jump
        if (playerMotor.collisionInfo.below)
        {
            if (playerMotor.collisionInfo.isSlidingDownMaxSlope)
            {
                if (horizontalInput != -Mathf.Sign(playerMotor.collisionInfo.slopeNormal.x)) //not jumping against slope
                {
                    velocity.y = maxJumpVelocity * playerMotor.collisionInfo.slopeNormal.y;
                    velocity.x = maxJumpVelocity * playerMotor.collisionInfo.slopeNormal.x;
                }
            }
            else
            {
                velocity.y = maxJumpVelocity;
            }
        }
    }

    public void JumpInputUp()
    {
        //Al soltar el espacio, si la velocidad Y es mayor al a minima, setearla, esto es para el jump sustain.
        if (velocity.y > minJumpVelocity)
        {
            velocity.y = minJumpVelocity;
        }
    }

}
