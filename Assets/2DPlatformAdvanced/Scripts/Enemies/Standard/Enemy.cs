using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enemy skeleton to use for our polymorphism.
public abstract class Enemy : MonoBehaviour {

    [SerializeField] protected float movementSpeed = 5;
    [SerializeField] protected int enemyHP = 100;
    [SerializeField] protected int touchDamage = 10;
    [SerializeField] protected int idleDuration = 7;
    [SerializeField] private int chaseOutDuration = 4;
    protected Transform enemyTransform;
    protected Animator animator;
    protected BoxCollider2D hitbox;
   

    public virtual void Awake()
    {
        animator = this.GetComponent<Animator>();
        enemyTransform = this.GetComponent<Transform>();
        hitbox = this.GetComponent<BoxCollider2D>();
    }

    public Vector2 GetMoveDirection()
    {
        return Vector2.right;
    }

    public abstract void Attack();
    public abstract void Chase();
    public abstract void Patrol();
    public abstract bool Look();
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

    public int IdleDuration
    {
        get
        {
            return idleDuration;
        }

        set
        {
            idleDuration = value;
        }
    }

    public int ChaseOutDuration
    {
        get
        {
            return chaseOutDuration;
        }

        set
        {
            chaseOutDuration = value;
        }
    }
    #endregion
}
