using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Enemigo terrestre.
public class GroundEnemy : Enemy {
    
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float visionMaxDistance = 2;
    [SerializeField] private LayerMask visionLayerMask;
    private const float VERTICAL_RAYLENGTH = 0.1f;
    private const float HORIZONTAL_RAYLENGTH = .03f;

    private Vector3 currentRotation;
    private Vector2 lineCastWall;
    private Vector2 lineCastGround;

    private bool isFacingRight = true;
    private bool isGrounded = false;
    private bool isTouchingWall = false;
    private Transform playerTransform;

    //----Metodos API-----
    public override void Awake()
    {
        base.Awake();
        playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    private void FixedUpdate()
    {
        CheckGround();
        CheckWalls();
    }
    //----Metodos Custom-----

    private void Move()
    {
        enemyTransform.Translate(GetMoveDirection() * (movementSpeed * Time.deltaTime));
    }

    private void CheckGround()
    {
        if (isFacingRight)
        {
            lineCastGround.x = hitbox.bounds.max.x;
            lineCastGround.y = hitbox.bounds.min.y;
            isGrounded = Physics2D.Linecast(lineCastGround, lineCastGround + (Vector2.down * VERTICAL_RAYLENGTH), groundLayer);
        }
        else
        {
            lineCastGround.x = hitbox.bounds.min.x;
            lineCastGround.y = hitbox.bounds.min.y;
            isGrounded = Physics2D.Linecast(lineCastGround, lineCastGround + (Vector2.down * VERTICAL_RAYLENGTH), groundLayer);
        }
    }

    private void CheckWalls()
    {

        if (isFacingRight)
        {
            lineCastWall.x = hitbox.bounds.max.x;
            lineCastWall.y = hitbox.bounds.max.y;
            isTouchingWall = Physics2D.Linecast(lineCastWall, lineCastWall + ((Vector2)enemyTransform.right * HORIZONTAL_RAYLENGTH), groundLayer);
        }
        else
        {
            lineCastWall.x = hitbox.bounds.min.x;
            lineCastWall.y = hitbox.bounds.max.y;
            isTouchingWall = Physics2D.Linecast(lineCastWall, lineCastWall + ((Vector2)enemyTransform.right * HORIZONTAL_RAYLENGTH), groundLayer);
        }
    }

    public void ChangeDirection()
    {
        isFacingRight = !isFacingRight;

        currentRotation = enemyTransform.eulerAngles;
        currentRotation.y += 180;
        enemyTransform.eulerAngles = currentRotation;
    }

    public override void Patrol()
    {
        if (!isGrounded || isTouchingWall)
        {
            ChangeDirection();
        }

        Move();
        Debug.DrawLine(lineCastWall, lineCastWall + (Vector2)enemyTransform.right * HORIZONTAL_RAYLENGTH);
        Debug.DrawLine(lineCastGround, lineCastGround + (Vector2.down * VERTICAL_RAYLENGTH));
    }

    public override void Chase()
    {
        if (!isGrounded || isTouchingWall)
        {
            return;
        }
        if (playerTransform.position.x < enemyTransform.position.x)
        {
            isFacingRight = false;
            currentRotation = enemyTransform.eulerAngles;
            currentRotation.y = 180;
            enemyTransform.eulerAngles = currentRotation;
        }
        else
        {
            isFacingRight = true;
            currentRotation = enemyTransform.eulerAngles;
            currentRotation.y = 360;
            enemyTransform.eulerAngles = currentRotation;
        }
        Move();
    }

    public override bool Look()
    {
        RaycastHit2D hit;
        Vector2 startPositon;
        startPositon.x = hitbox.bounds.max.x;
        startPositon.y = hitbox.bounds.max.y * 0.98f;
        hit = Physics2D.Linecast(startPositon, startPositon + ((Vector2)enemyTransform.right * visionMaxDistance), visionLayerMask);

        //Editor Only
        Debug.DrawLine(startPositon, startPositon + ((Vector2)enemyTransform.right * visionMaxDistance), Color.blue);

        if (hit && hit.collider.CompareTag("Player"))
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }

    public override void Attack(){}
}
