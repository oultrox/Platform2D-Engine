using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/Patrol")]
public class PatrolAction : Action {

    public override void Act(EnemyStateMachine state)
    {
        Patrol(state);
    }

    private void Patrol(EnemyStateMachine state)
    {
        Move(state);
    }

    private void Move(EnemyStateMachine state)
    {
        state.Enemy.EnemyTransform.Translate(state.Enemy.GetDirection() * (state.Enemy.MovementSpeed * Time.deltaTime));
    }

    
}
