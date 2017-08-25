using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Automove : MonoBehaviour {

    //Se inicializa la normal porque sin ella no podemos mover un objeto que no haya tocado algo antes.
    //Es necesario para darle a un objeto que hereda del motor de fisicas una orientación.

    private void FixedUpdate()
    {
        transform.Translate(Vector2.right * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("Collided with player.");
        }
    }
}
