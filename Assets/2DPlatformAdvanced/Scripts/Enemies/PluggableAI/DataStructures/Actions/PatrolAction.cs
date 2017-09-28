using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/Patrol")]
public class PatrolAction : Action {

    public override void Act(EnemyStateMachine state)
    {
        state.Enemy.Patrol();
    }
}
