using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/State")]
public class State : ScriptableObject
{

    public Action[] actions;
    public Transition[] transitions;
    public Color sceneGizmoColor = Color.grey;

    public void UpdateState(StateMachine controller)
    {
        DoAction(controller);
        CheckTransitions(controller);
    }

    private void DoAction(StateMachine stateController)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            actions[i].Act(stateController);
        }
    }

    private void CheckTransitions(StateMachine stateController)
    {
        for (int i = 0; i < transitions.Length; i++)
        {
            bool decisionSucceded = transitions[i].decision.Decide(stateController);

            if (decisionSucceded)
            {
                stateController.TransitionToState(transitions[i].trueState);
            }
            else
            {
                stateController.TransitionToState(transitions[i].falseState);
            }
        }
    }
}
