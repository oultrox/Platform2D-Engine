using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent (typeof (Controller2D))]
public class PlayerMove : MonoBehaviour {

    public float moveSpeed = 6;
    Vector3 velocity;
    float gravity = -20;
    Controller2D controller;

	// Use this for initialization
	void Start () {
        controller = this.GetComponent<Controller2D>();
	}

    private void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");



        velocity.x = h * moveSpeed;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

}
