using UnityEngine;

[CreateAssetMenu(menuName ="PluggableAI/Actions/Attack")]
public class AttackAction : Action
{
    public override void Act(EnemyStateMachine stateController)
    {
        stateController.Enemy.Attack();
    }
}
