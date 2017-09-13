using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/Idle")]
public class IdleAction : Action
{
    public override void Act(EnemyStateMachine stateController)
    {
        Debug.Log("Estoy en Idle!");
    }
}
