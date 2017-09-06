using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent  (typeof(PlayerMove))]
public class PlayerInput : MonoBehaviour {

    PlayerMove playerMovement;
	// Use this for initialization
	void Start () {
        playerMovement = GetComponent<PlayerMove>();
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
