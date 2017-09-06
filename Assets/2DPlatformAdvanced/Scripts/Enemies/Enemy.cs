using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Superclase enemiga que es padre de los enemigos.
public abstract class Enemy : MonoBehaviour {

    [SerializeField] protected float movementSpeed;
    protected Animator animator;
    protected bool isFacingRight;

    public virtual void Start()
    {
        animator = this.GetComponent<Animator>();
    }

    public virtual void ChangeDirection()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }
}
