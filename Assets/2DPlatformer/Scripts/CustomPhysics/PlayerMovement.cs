using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : RetroPhysicsObject
{

    [SerializeField] private float jumpTakeOffSpeed = 7f;
    [SerializeField] private float maxSpeed = 7f;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    protected override void ComputeVelocity()
    {
        //Horizontal movement
        Vector2 move = Vector2.zero;
        move.x = Input.GetAxis("Horizontal");

        //Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = jumpTakeOffSpeed;
            groundNormal.y = 1;
            groundNormal.x = 0;
        }
        else if (Input.GetButtonUp("Jump"))
        {
            if (velocity.y > 0)
            {
                velocity.y = velocity.y * 0.5f;
            }
        }

        spriteRenderer.flipX = move.x < -0.01f;

        animator.SetBool("grounded", isGrounded);
        animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);
        targetVelocity = move * maxSpeed;
    }

}