using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Enemigo terrestre.
public class GroundEnemy : Enemy {

    private const float VERTICAL_RAYLENGTH = 0.1f;
    private const float HORIZONTAL_RAYLENGTH = .03f;

    [SerializeField] private LayerMask groundLayer;

    private Vector3 currentRotation;
    private Vector2 lineCastWall;
    private Vector2 lineCastGround;
    
    private bool isFacingRight = true;
    private bool isGrounded = false;
    private bool isTouchingWall = false;

    //----Metodos API-----
    public override void Awake()
    {
        base.Awake();
    }

    private void FixedUpdate()
    {
        CheckGround();
        CheckWalls();

        if (!isGrounded || isTouchingWall)
        {
            ChangeDirection();
        }

        Debug.DrawLine(lineCastWall, lineCastWall + (Vector2)enemyTransform.right * HORIZONTAL_RAYLENGTH);
        Debug.DrawLine(lineCastGround, lineCastGround + (Vector2.down * VERTICAL_RAYLENGTH));
    }

    //----Metodos Custom-----
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
            isTouchingWall = Physics2D.Linecast(lineCastWall, lineCastWall + (Vector2.down * HORIZONTAL_RAYLENGTH), groundLayer);
        }
        else
        {
            lineCastWall.x = hitbox.bounds.min.x;
            lineCastWall.y = hitbox.bounds.max.y;
            isTouchingWall = Physics2D.Linecast(lineCastWall, lineCastWall + (Vector2.down * HORIZONTAL_RAYLENGTH), groundLayer);
        }
    }

    public virtual void ChangeDirection()
    {
        isFacingRight = !isFacingRight;

        currentRotation = enemyTransform.eulerAngles;
        currentRotation.y += 180;
        enemyTransform.eulerAngles = currentRotation;
    }

}
