using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Clase que será usada por las entidades que llamará los estados.
public class EnemyStateMachine : MonoBehaviour {

    [SerializeField] private State currentState;
    [SerializeField] private State remainState;
    private Enemy enemy;
    private bool aiActive;

    //----Metodos API-----
    private void Awake()
    {
        enemy = this.GetComponent<Enemy>();
        aiActive = true;
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
