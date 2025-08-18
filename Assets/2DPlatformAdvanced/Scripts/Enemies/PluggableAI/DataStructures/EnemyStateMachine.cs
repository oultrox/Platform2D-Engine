using UnityEngine;

/// <summary>
/// State machine scriptable object component that drives enemy AI behavior.
/// It updates the current state, handles state transitions,
/// tracks state duration, and provides countdown helpers for state logic.
/// </summary>
public class EnemyStateMachine : MonoBehaviour 
{
    [SerializeField] private State currentState;
    [SerializeField] private State remainState;
    private bool aiActive;
    private float stateTimeElapsed;
    public Enemy Enemy { get; set; }
    
    
    private void Awake()
    {
        Enemy = this.GetComponent<Enemy>();
        aiActive = true;
        stateTimeElapsed = 0;
    }

    private void Update () 
    {
        if (aiActive)
        {
            currentState.UpdateState(this);
        } 
    }
    
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
        if (duration <= 0)
        {
            return false;
        }

        stateTimeElapsed += Time.deltaTime;
        bool isCountDownElapsed = stateTimeElapsed >= duration;
        return isCountDownElapsed;
    }
    
    #region Debug
    void OnDrawGizmos()
    {
        if (currentState != null)
        {
            Gizmos.color = currentState.sceneGizmoColor;
            Gizmos.DrawWireSphere(transform.position, 0.4f);
        }
    }
    #endregion
}
