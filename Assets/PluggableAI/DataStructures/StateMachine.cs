using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Clase que será usada por las entidades que llamará los estados.
public class StateMachine : MonoBehaviour {

    public State currentState;
    public State remainState;

    private bool aiActive;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void TransitionToState(State nextState)
    {
        if (nextState != remainState)
        {
            currentState = nextState;
        }
    }
}
