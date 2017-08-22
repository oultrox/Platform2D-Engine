using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : RetroPhysicsObject {

    [SerializeField] private float jumpTakeOffSpeed = 7f;
    [SerializeField] private float maxSpeed = 7f;
    private void Start()
    {
        
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
        } else if (Input.GetButtonUp("Jump")) //for cancelling the jump sustain
        {
            if (velocity.y > 0)
            {
                velocity.y = velocity.y * 0.5f;
            }
            
        }
        targetVelocity = move * maxSpeed;
    }
    
}
