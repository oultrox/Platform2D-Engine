using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Automove : RetroPhysicsObject {

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
