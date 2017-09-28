using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//State machine que será usada por los enemigos para llamar los estados.
public class EnemyStateMachine : MonoBehaviour {

    [SerializeField] private State currentState;
    [SerializeField] private State remainState;
    private Enemy enemy;
    private bool aiActive;
    private float stateTimeElapsed;

    //----Metodos API-----
    private void Awake()
    {
        enemy = this.GetComponent<Enemy>();
        aiActive = true;
        stateTimeElapsed = 0;
    }

    void Update () {

        if (aiActive)
        {
            currentState.UpdateState(this);
        }
	}

    //----Metodos Custom----
    public void TransitionToState(State nextState)
    {
        if (nextState != remainState)
        {
            currentState = nextState;
            stateTimeElapsed = 0;
        }
    }

    public bool CheckIfCountDownElapsed(int duration)
    {
        //Para evitar transitar si la duracion es 0.
        if (duration <= 0)
        {
            return false;
        }

        stateTimeElapsed += Time.deltaTime;
        bool isCountDownElapsed = stateTimeElapsed >= duration;
        return isCountDownElapsed;
    }


    //-----Metodos de editor-----
    void OnDrawGizmos()
    {
        if (currentState != null)
        {
            Gizmos.color = currentState.sceneGizmoColor;
            Gizmos.DrawWireSphere(transform.position, 0.4f);
        }
    }

    #region Properties
    public bool AiActive
    {
        get
        {
            return aiActive;
        }

        set
        {
            aiActive = value;
        }
    }

    public Enemy Enemy
    {
        get
        {
            return enemy;
        }

        set
        {
            enemy = value;
        }
    }
    #endregion

}
