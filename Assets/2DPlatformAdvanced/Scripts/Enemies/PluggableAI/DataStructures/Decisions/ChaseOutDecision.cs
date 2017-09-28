
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/ChaseOut")]
public class ChaseOutDecision : Decision {

    public override bool Decide(EnemyStateMachine stateController)
    {
        return stateController.CheckIfCountDownElapsed(stateController.Enemy.ChaseOutDuration);
    }
}
