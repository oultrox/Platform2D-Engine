using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Automove : RetroPhysicsObject {
    
    //Se inicializa la normal porque sin ella no podemos mover un objeto que no haya tocado algo antes.
    //Es necesario para darle a un objeto que hereda del motor de fisicas una orientación.
    private void Start()
    {
        groundNormal = new Vector2(0f, 1f); //assumes there's a flat ground somewhere beneath him;
    }

    private void Update()
    {
        targetVelocity = Vector2.left;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("Collided with player.");
        }
        
    }
}
