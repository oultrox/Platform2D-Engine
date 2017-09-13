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
    private bool isCountDownElapsed;
    private float stateTimeElapsed;

    //----Metodos API-----
    private void Awake()
    {
        enemy = this.GetComponent<Enemy>();
        aiActive = true;
        isCountDownElapsed = false;
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
        }
    }

    public bool CheckIfCountDownElapsed(int duration)
    {
        //Para evitar transitar a idle si la duracion es 0.
        if (duration > 0)
        {
            stateTimeElapsed += Time.deltaTime;
            isCountDownElapsed = stateTimeElapsed >= duration;

            //Si pasó el tiempo, reiniciar.
            if (isCountDownElapsed)
            {
                stateTimeElapsed = 0;
            }
            return isCountDownElapsed;
        }
        else
        {
            return false;
        }
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
