using UnityEngine;

[CreateAssetMenu(menuName ="PluggableAI/Actions/Chase")]
public class ChaseAction : Action 
{
	public override void Act(EnemyStateMachine stateController)
    {
        stateController.Enemy.Chase();
    }
}
