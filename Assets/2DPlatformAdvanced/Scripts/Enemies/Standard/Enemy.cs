using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Comparte todo lo que tienen en comun los enemigos.
public class Enemy : MonoBehaviour {

    [SerializeField] protected float movementSpeed = 5;
    [SerializeField] protected int enemyHP = 100;
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] private int touchDamage = 10;
    protected Transform enemyTransform;
    protected Animator animator;
    protected bool isFacingRight  = true;
    protected bool isGrounded     = false;
    protected bool isTouchingWall = false;
    private float enemyWidth;
    private float enemyHeight;

    //Cached variables
    private Vector2 lineCastPos;
    private const float VERTICAL_RAYLENGTH = 0.5f;
    private const float HORIZONTAL_RAYLENGTH = .03f;
    //----Metodos API-----
    public virtual void Awake()
    {
        animator = this.GetComponent<Animator>();
        enemyTransform = this.GetComponent<Transform>();
        enemyWidth = this.GetComponent<SpriteRenderer>().bounds.extents.x;
        enemyHeight = this.GetComponent<SpriteRenderer>().bounds.extents.y;
    }

    
    private void FixedUpdate()
    {
        //Comprueba si el enemigo esta en el suelo.
        lineCastPos = (Vector2)enemyTransform.position + (Vector2)enemyTransform.right * enemyWidth + Vector2.up * enemyHeight;
        lineCastPos.y -= 0.2f;
        isGrounded = Physics2D.Linecast(lineCastPos, lineCastPos + (Vector2.down * VERTICAL_RAYLENGTH), groundLayer);

        //Comprueba si el enemigo esta tocando una pared.
        isTouchingWall = Physics2D.Linecast(lineCastPos, lineCastPos + (Vector2)enemyTransform.right * HORIZONTAL_RAYLENGTH, groundLayer);

        if (!isGrounded || isTouchingWall)
        {
            ChangeDirection();
        }

        Debug.DrawLine(lineCastPos, lineCastPos + (Vector2)enemyTransform.right * HORIZONTAL_RAYLENGTH);
        Debug.DrawLine(lineCastPos, lineCastPos + (Vector2.down * VERTICAL_RAYLENGTH));
    }

    //----Metodos Custom-----
    public virtual void ChangeDirection()
    {
        isFacingRight = !isFacingRight;

        Vector3 currentRotation = enemyTransform.eulerAngles;
        currentRotation.y += 180;
        enemyTransform.eulerAngles = currentRotation;
    }

    public Vector2 GetDirection()
    {
        return Vector2.right;
    }

    #region Properties
    public float MovementSpeed
    {
        get
        {
            return movementSpeed;
        }

        set
        {
            movementSpeed = value;
        }
    }

    public int EnemyHP
    {
        get
        {
            return enemyHP;
        }

        set
        {
            enemyHP = value;
        }
    }

    public Animator Animator
    {
        get
        {
            return animator;
        }

        set
        {
            animator = value;
        }
    }

    public bool IsFacingRight
    {
        get
        {
            return isFacingRight;
        }

        set
        {
            isFacingRight = value;
        }
    }

    public Transform EnemyTransform
    {
        get
        {
            return enemyTransform;
        }

        set
        {
            enemyTransform = value;
        }
    }

    public int TouchDamage
    {
        get
        {
            return touchDamage;
        }

        set
        {
            touchDamage = value;
        }
    }
    #endregion
}
