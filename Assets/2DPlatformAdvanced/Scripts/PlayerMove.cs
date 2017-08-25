using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Controller2D))]
public class PlayerMove : MonoBehaviour {

    public float jumpHeight = 4;
    public float timeToJumpApex = .4f;
    public float moveSpeed = 6;
    public float accelerationTimeAirborne = 0.2f;
    public float accelerationTimeGrounded = 0.1f;

    private float jumpVelocity = 10;
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
        gravity = - (2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity * timeToJumpApex);
	}

    private void Update()
    {
        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }
        h = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && controller.collisions.below)
        {
            velocity.y = jumpVelocity;
        }

        targetVelocityX = h * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeAirborne : accelerationTimeGrounded);
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
