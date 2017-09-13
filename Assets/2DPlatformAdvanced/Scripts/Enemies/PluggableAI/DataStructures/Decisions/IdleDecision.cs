﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/Idle")]
public class IdleDecision : Decision
{
    public override bool Decide(EnemyStateMachine stateController)
    {
        return stateController.CheckIfCountDownElapsed(stateController.Enemy.IdleDuration);
    }
}
