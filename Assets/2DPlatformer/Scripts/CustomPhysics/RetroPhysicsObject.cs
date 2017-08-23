using System.Collections.Generic;
using UnityEngine;

//Custom physics hecho para juegos con un feeling retro.
public class RetroPhysicsObject : MonoBehaviour {

    //Variables editables.
    public float minGroundNormalY = 0.5f;
    public float gravityModifier = 1f;

    //Variables que se heredarán.
    protected Vector2 targetVelocity;
    protected Rigidbody2D rgBody2D;
    protected Vector2 velocity;
    protected Vector2 groundNormal;
    protected ContactFilter2D contactFilter;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> hitBufferCollidedList = new List<RaycastHit2D>(16);
    protected bool isGrounded;

    //Constantes
    protected const float minMoveDistance = 0.001f;
    protected const float shellRadius = 0.01f;
    
    
    //Cached variables 
    private Vector2 move;
    private Vector2 deltaPosition;
    private float distance;

    //---------API metodos---------

    //Inicialización consiguiendo el rigidbody.
    void OnEnable()
    {
        rgBody2D = this.GetComponent<Rigidbody2D>();     
    }
    
    //Inicializa el filtro de contactos para el raycast basandose en la matriz
    //de colision en Project settings -> Physics2D.
    void Start () {
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFilter.useLayerMask = true;
	}

    //Metodo que será llamado por la clase PlayerMovement via override.
    private void Update()
    {
        targetVelocity = Vector2.zero;
        ComputeVelocity();
    }
    protected virtual void ComputeVelocity() { }

    Vector2 moveAlongGround;
    //Otorga gravedad al vector de velocidad, deltaPosition guarda la posicion del ultimo frame, y luego aplica la gravedad a traves de la función Movement.
	void FixedUpdate ()
    {
        //Agregamos la nueva velocidad a la velocidad antigua con la gravedad, que es una aceleración sobre el tiempo (velocidad/time). 
        //al multiplicarlo por el tiempo conseguimos el resultado deseado.
        velocity += gravityModifier * Physics2D.gravity * Time.deltaTime;
        velocity.x = targetVelocity.x; //almacena la velocidad horizontal inicial

        //Siempre debe ir después del frame de gravedad parar lograr el comportamiento correcto.
        isGrounded = false;

        //Al mutliplicarlo (nuevamente) por el tiempo conseguimos el incremento de la posición en este frame ya que velocity nos dirá cuanto debemos movernos por cada unidad.
        deltaPosition = velocity * Time.deltaTime; 

        //Aplica primero el movimiento horizontal y luego el vertical en la misma iteración, tal como en los classics  y así es más comodo manejar empinaduras.
        moveAlongGround.x = groundNormal.y; //almacena la velocidad horizontal en movimiento de la normal del suelo.
        moveAlongGround.y = -groundNormal.x;
        move = moveAlongGround * deltaPosition.x;
        Movement(move, false);
        //Aplica movimiento vertical.
        move = Vector2.up * deltaPosition.y;
        Movement(move,true);
    }

    //---------Custom methods---------

    //Método de movimiento, preguntando si se está movimiendo el minimo de distancia para no preguntar por el casting
    //sin estar en movimiento.
    private void Movement(Vector2 move, bool isYMovement)
    {
        distance = move.magnitude;
        if (distance > minMoveDistance)
        {
            int count = rgBody2D.Cast(move, contactFilter, hitBuffer, distance + shellRadius); // if you want to cast some defined collider - use, for example, BoxCollider2D.Cast﻿
            hitBufferCollidedList.Clear();
            for (int i = 0; i < count; i++)
            {
                hitBufferCollidedList.Add(hitBuffer[i]);
            }
            //Chequea si con lo que está colionando es un hitbox de suelo donde puede pararse. 
            //Esto puede complicarse con suelos muy empinados como rampas.
            //La solución allí es o limitar lo empinado de la rampa o bien extender el controller.
            Vector2 currentNormal;
            for (int i = 0; i < hitBufferCollidedList.Count; i++)
            {
                currentNormal = hitBufferCollidedList[i].normal;
                if (currentNormal.y > minGroundNormalY)
                {
                    isGrounded = true;
                    if (isYMovement)
                    {
                        groundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                }
                //Preguntamos si la proyección es menor que 0 para poder así evitar
                //la situacionn en donde el player golpee con algo arriba, no queremos matar su velocidad de golpe
                //sino simplemente dejarlo suavemente reducir su velocidad.
                float projection = Vector2.Dot(velocity, currentNormal);
                if (projection < 0)
                {
                    velocity = velocity - projection * currentNormal;
                }

                float modifiedDistance = hitBufferCollidedList[i].distance - shellRadius;
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }
        }
        //Asigna el movimiento.
        rgBody2D.position = rgBody2D.position + move.normalized * distance;
    }
}
