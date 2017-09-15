using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent  (typeof(PlayerMovementController))]
public class PlayerInput : MonoBehaviour {

    PlayerMovementController playerMovement;

	// Use this for initialization
	void Start () {
        playerMovement = GetComponent<PlayerMovementController>();
	}
	
	// Update is called once per frame
	void Update () {
        playerMovement.SetDirectionalInput(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetKeyDown(KeyCode.Space))
        {
            playerMovement.JumpInputDown();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            playerMovement.JumpInputUp();
        }
    }
}
